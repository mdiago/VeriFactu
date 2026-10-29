using VeriFactu.Business.Validation.VIES;
using Xunit;
using System.Collections.Generic;
using VeriFactu.Business.Validation.Validators.Alta;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;
using VeriFactu.Xml;
using VeriFactu.Config;

namespace VeriFactu.Tests
{

    [CollectionDefinition(
    "Identifier validation",
    DisableParallelization = true)]
    public class IdentifierValidationCollection
    {
    }

    [Collection("Identifier validation")]
    public class InvoiceValidationIdentifierTests
    {

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("D")]
        [InlineData("DE")]
        public void InvalidVatIdentifierHasInvalidStructure(string value)
        {
            Assert.False(ViesVatNumber.ValidateStructure(value));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("D")]
        [InlineData("DE")]
        public void IncompleteVatIdentifierIsRejectedBeforeRemoteValidation(
            string value)
        {
            Assert.False(ViesVatNumber.Validate(value));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("D")]
        [InlineData("DE")]
        public void InvalidVatIdentifierReturnsAStructureError(string value)
        {
            var alta = new RegistroAlta
            {
                TipoFactura = TipoFactura.F1,
                IDFacturaAlta = new IDFactura
                {
                    IDEmisorFactura = "B12345678",
                    NumSerieFactura = "TEST-1",
                    FechaExpedicionFactura = "25-10-2026"
                }
            };

            var envelope = new Envelope
            {
                Body = new Body
                {
                    Registro = new RegFactuSistemaFacturacion
                    {
                        Cabecera = new Cabecera(),
                        RegistroFactura = new List<RegistroFactura>
                {
                    new RegistroFactura { Registro = alta }
                }
                    }
                }
            };

            var destinatario = new Interlocutor
            {
                NombreRazon = "Destinatario de prueba",
                IDOtro = new IDOtro
                {
                    IDType = IDType.NIF_IVA,
                    CodigoPais = CodigoPais.DE,
                    CodigoPaisSpecified = true,
                    ID = value
                }
            };

            var errors = new ValidatorRegistroAltaInterlocutor(
                envelope,
                alta,
                destinatario,
                "Destinatario",
                true).GetErrors();

            var error = Assert.Single(errors);

            Assert.Contains("El valor del campo ID es incorrecto", error);

        }

        [Fact]
        public void PassportDoesNotUseVatIdentifierValidation()
        {
            var alta = new RegistroAlta
            {
                TipoFactura = TipoFactura.F1,
                IDFacturaAlta = new IDFactura
                {
                    IDEmisorFactura = "B12345678",
                    NumSerieFactura = "TEST-1",
                    FechaExpedicionFactura = "25-10-2026"
                }
            };

            var envelope = new Envelope
            {
                Body = new Body
                {
                    Registro = new RegFactuSistemaFacturacion
                    {
                        Cabecera = new Cabecera(),
                        RegistroFactura = new List<RegistroFactura>
                {
                    new RegistroFactura { Registro = alta }
                }
                    }
                }
            };

            var destinatario = new Interlocutor
            {
                NombreRazon = "Destinatario de prueba",
                IDOtro = new IDOtro
                {
                    IDType = IDType.PASAPORTE,
                    CodigoPais = CodigoPais.DE,
                    CodigoPaisSpecified = true,
                    ID = "A"
                }
            };

            var errors = new ValidatorRegistroAltaInterlocutor(
                envelope,
                alta,
                destinatario,
                "Destinatario",
                true).GetErrors();

            Assert.Empty(errors);

        }

        [Theory]
        [InlineData(CodigoPais.DE, "DE123456789", false)]
        [InlineData(CodigoPais.FR, "DE123456789", true)]
        [InlineData(CodigoPais.GR, "EL123456789", false)]
        public void VatIdentifierCountryIsChecked(CodigoPais country, string identifier, bool expectCountryError)
        { 

            var alta = new RegistroAlta
            {
                TipoFactura = TipoFactura.F1,
                IDFacturaAlta = new IDFactura
                {
                    IDEmisorFactura = "B12345678",
                    NumSerieFactura = "TEST-1",
                    FechaExpedicionFactura = "25-10-2026"
                }
            };

            var envelope = new Envelope
            {
                Body = new Body
                {
                    Registro = new RegFactuSistemaFacturacion
                    {
                        Cabecera = new Cabecera(),
                        RegistroFactura = new List<RegistroFactura>
                {
                    new RegistroFactura { Registro = alta }
                }
                    }
                }
            };

            var destinatario = new Interlocutor
            {
                NombreRazon = "Destinatario de prueba",
                IDOtro = new IDOtro
                {
                    IDType = IDType.NIF_IVA,
                    CodigoPais = country,
                    CodigoPaisSpecified = true,
                    ID = identifier
                }
            };

            var previousSkip =
                Settings.Current.SkipViesVatNumberValidation;

            try
            {
                Settings.Current.SkipViesVatNumberValidation = true;

                var errors = new ValidatorRegistroAltaInterlocutor(
                    envelope,
                    alta,
                    destinatario,
                    "Destinatario",
                    true).GetErrors();

                if (expectCountryError)
                {
                    var error = Assert.Single(errors);
                    Assert.Contains("no coincide", error);
                }
                else
                {
                    Assert.Empty(errors);
                }
            }
            finally
            {
                Settings.Current.SkipViesVatNumberValidation =
                    previousSkip;
            }
        }

    }

}