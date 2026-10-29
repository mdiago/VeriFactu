using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using VeriFactu.Business.Validation;
using VeriFactu.Business.Validation.Validators.Alta;
using VeriFactu.Config;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Soap;
using Xunit;

namespace VeriFactu.Tests
{
    [CollectionDefinition("Local date validation", DisableParallelization = true)]
    public class LocalDateValidationCollection { }

    [Collection("Local date validation")]
    public class InvoiceValidationDateTests : IDisposable
    {
        private readonly bool previousNif = Settings.Current.SkipNifAeatValidation;
        private readonly bool previousVies = Settings.Current.SkipViesVatNumberValidation;

        public InvoiceValidationDateTests()
        {
            Settings.Current.SkipNifAeatValidation = true;
            Settings.Current.SkipViesVatNumberValidation = true;
        }

        public void Dispose()
        {
            Settings.Current.SkipNifAeatValidation = previousNif;
            Settings.Current.SkipViesVatNumberValidation = previousVies;
        }

        private static RegistroAlta Alta(string number = "TEST-1") => new RegistroAlta
        {
            IDVersion = "1.0",
            IDFacturaAlta = new IDFactura
            {
                IDEmisorFactura = "B12345678", NumSerieFactura = number,
                FechaExpedicionFactura = "25-10-2026"
            },
            NombreRazonEmisor = "Test", TipoFactura = TipoFactura.F2,
            DescripcionOperacion = "Servicio", CuotaTotal = "21.00", ImporteTotal = "121.00",
            SistemaInformatico = new SistemaInformatico { NIF = "B12345678" },
            Desglose = new List<DetalleDesglose>
            {
                new DetalleDesglose
                {
                    Impuesto = Impuesto.IVA, ClaveRegimen = ClaveRegimen.RegimenGeneral,
                    ClaveRegimenSpecified = true,
                    CalificacionOperacion = CalificacionOperacion.S1,
                    CalificacionOperacionSpecified = true,
                    BaseImponibleOimporteNoSujeto = "100.00", TipoImpositivo = "21.00",
                    CuotaRepercutida = "21.00"
                }
            }
        };

        private static Envelope Sobre(params object[] records) => new Envelope
        {
            Body = new Body
            {
                Registro = new RegFactuSistemaFacturacion
                {
                    Cabecera = new Cabecera
                    {
                        ObligadoEmision = new Interlocutor { NIF = "B12345678", NombreRazon = "Test" }
                    },
                    RegistroFactura = records.Select(r => new RegistroFactura { Registro = r }).ToList()
                }
            }
        };

        private static Envelope RoundTrip(Envelope envelope)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                new XmlSerializer(typeof(Envelope)).Serialize(writer, envelope);
                return Envelope.FromXml(writer.ToString());
            }
        }

        [Theory]
        [InlineData("2026-10-25")]
        [InlineData("31-02-2026")]
        [InlineData("29-02-2025")]
        [InlineData("1")]
        [InlineData("25/10/2026")]
        [InlineData("25-10-2026extra")]
        [InlineData("")]
        [InlineData(null)]
        public void InvalidExpeditionReturnsAnErrorAndContinuesValidation(string value)
        {
            var first = Alta();
            first.IDFacturaAlta.FechaExpedicionFactura = value;
            first.DescripcionOperacion = null;
            var second = Alta("TEST-2");
            second.IDFacturaAlta.NumSerieFactura = null;

            var errors = new InvoiceValidation(RoundTrip(Sobre(first, second))).GetErrors();

            Assert.Single(errors.Where(e => e.Contains("IDFactura.FechaExpedicion tiene")));
            Assert.Contains(errors, e => e.Contains("DescripcionOperacion"));
            Assert.Contains(errors, e => e.Contains("[3.1.3-1.5]"));
            Assert.DoesNotContain(errors, e => e.Contains("[3.1.3-1.3]"));
        }

        [Theory]
        [InlineData("2026-10-25")]
        [InlineData("31-02-2026")]
        [InlineData("29-02-2025")]
        [InlineData("1")]
        [InlineData(" ")]
        public void InvalidOperationDoesNotFallBackToExpeditionDate(string value)
        {
            var alta = Alta();
            alta.FechaOperacion = value;
            // An invalid date must not trigger a historical-rate error via a fallback.
            alta.Desglose[0].TipoImpositivo = "5.00";
            var errors = new InvoiceValidation(Sobre(alta)).GetErrors();
            Assert.Single(errors.Where(e => e.Contains("FechaOperacion tiene")));
            Assert.DoesNotContain(errors, e => e.Contains("[3.1.3-15.1.0]"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("29-02-2024")]
        public void MissingOrValidOperationIsNotAFormatError(string value)
        {
            var alta = Alta();
            alta.FechaOperacion = value;
            var errors = new ValidatorRegistroAltaFechaOperacion(Sobre(alta), alta).GetErrors();
            Assert.DoesNotContain(errors, e => e.Contains("FechaOperacion tiene"));
        }

        [Fact]
        public void InvalidCancellationReturnsAnError()
        {
            var cancellation = new RegistroAnulacion
            {
                IDFacturaAnulada = new IDFactura { FechaExpedicionFacturaAnulada = "31-02-2026" },
                SistemaInformatico = new SistemaInformatico { NIF = "B12345678" }
            };
            var errors = new InvoiceValidation(RoundTrip(Sobre(cancellation))).GetErrors();
            Assert.Single(errors.Where(e => e.Contains("IDFactura.FechaExpedicion tiene")));
        }

        [Fact]
        public void GregorianDatesAreParsedIndependentlyOfCurrentCulture()
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("th-TH");
                var alta = Alta();
                var errors = new ValidatorRegistroAltaIDFactura(Sobre(alta), alta).GetErrors();
                Assert.DoesNotContain(errors, e => e.Contains("IDFactura.FechaExpedicion tiene"));
                Assert.DoesNotContain(errors, e => e.Contains("[3.1.3-1.3]"));
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }
    }
}
