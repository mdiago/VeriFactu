using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VeriFactu.Business;
using VeriFactu.Config;
using VeriFactu.Xml.Factu.Alta;
using Xunit;

namespace VeriFactu.Tests
{
    public class InvoiceRetrySendDisableBlockchainDeleteChainTest
    {

        [Fact]
        public void EjInvoiceRetrySend()
        {

            var huellasIn = new string[4];
            var huellaDel = new string[1];

            // Envío alta 01
            var now = DateTime.Now;

            var invoiceOk01 = new Invoice($"OK-{now:ddMMyyyymmss}-001", now, "B72877814")
            {
                InvoiceType = TipoFactura.F1,
                SellerName = "WEFINZ GANDIA SL",
                BuyerID = "B44531218",
                BuyerName = "WEFINZ SOLUTIONS SL",
                Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
                TaxItems = new List<TaxItem>
                {
                    new TaxItem { TaxRate = 4, TaxBase = 10, TaxAmount = 0.4m },
                    new TaxItem { TaxRate = 21, TaxBase = 100, TaxAmount = 21 }
                }
            };

            var invoiceEntryOk01 = new InvoiceEntry(invoiceOk01);

            invoiceEntryOk01.Save();

            huellasIn[0] = (invoiceEntryOk01.Registro as RegistroAlta).Huella;

            invoiceEntryOk01.Status.Should().Be("Correcto",
                because: $"la presentación en AEAT falló con código {invoiceEntryOk01.ErrorCode}: {invoiceEntryOk01.ErrorDescription}");

            invoiceEntryOk01.CSV.Should().NotBeNullOrEmpty("debe generarse un CSV cuando el estado es Correcto");
            invoiceEntryOk01.Response.Should().NotBeNullOrEmpty("la respuesta de la AEAT no debe estar vacía");

            // Forzar fallo en las comunicaciones

            string urlInvalida = "https://192.0.2.1/";

            var tmp = Settings.Current.VeriFactuEndPointPrefix;
            Settings.Current.VeriFactuEndPointPrefix = urlInvalida;
            Settings.Current.DisableBlockchainDelete = false;

            var invoiceErrDisableBlockchainDeleteFalse = new Invoice($"KO-{now:ddMMyyyymmss}-LINK_DELETED", now, "B72877814")
            {
                InvoiceType = TipoFactura.F1,
                SellerName = "WEFINZ GANDIA SL",
                BuyerID = "B44531218",
                BuyerName = "WEFINZ SOLUTIONS SL",
                Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
                TaxItems = new List<TaxItem>
                {
                    new TaxItem { TaxRate = 4, TaxBase = 10, TaxAmount = 0.4m },
                    new TaxItem { TaxRate = 21, TaxBase = 100, TaxAmount = 21 }
                }
            };

            var invoiceEntryErrDisableBlockchainDeleteFalse = new InvoiceEntry(invoiceErrDisableBlockchainDeleteFalse);

            Action act = () => invoiceEntryErrDisableBlockchainDeleteFalse.Save();

            act.Should()
               .Throw<Common.Exceptions.SendException>("se provocó un fallo intencional de red simulando desconexión")
               .WithMessage("*");

            // Registro no incluido en la cadena (DisableBlockchainDelete = false)
            huellaDel[0] = (invoiceEntryErrDisableBlockchainDeleteFalse.Registro as RegistroAlta).Huella;

            Settings.Current.DisableBlockchainDelete = true;

            var invoiceErrDisableBlockchainDeleteTrue = new Invoice($"KO-{now:ddMMyyyymmss}--LINK_INCLUDED", now, "B72877814")
            {
                InvoiceType = TipoFactura.F1,
                SellerName = "WEFINZ GANDIA SL",
                BuyerID = "B44531218",
                BuyerName = "WEFINZ SOLUTIONS SL",
                Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
                TaxItems = new List<TaxItem>
                {
                    new TaxItem { TaxRate = 4, TaxBase = 10, TaxAmount = 0.4m },
                    new TaxItem { TaxRate = 21, TaxBase = 100, TaxAmount = 21 }
                }
            };

            var invoiceEntryErrDisableBlockchainDeleteTrue = new InvoiceEntry(invoiceErrDisableBlockchainDeleteTrue);

            act = () => invoiceEntryErrDisableBlockchainDeleteTrue.Save();

            act.Should()
               .Throw<Common.Exceptions.SendException>("se provocó un fallo intencional de red simulando desconexión")
               .WithMessage("*");

            // Registro incluido en la cadena (DisableBlockchainDelete = true)
            huellasIn[1] = (invoiceEntryErrDisableBlockchainDeleteTrue.Registro as RegistroAlta).Huella;

            Settings.Current.VeriFactuEndPointPrefix = tmp;

            var invoiceOk02 = new Invoice($"OK-{now:ddMMyyyymmss}-002", now, "B72877814")
            {
                InvoiceType = TipoFactura.F1,
                SellerName = "WEFINZ GANDIA SL",
                BuyerID = "B44531218",
                BuyerName = "WEFINZ SOLUTIONS SL",
                Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
                TaxItems = new List<TaxItem>
                {
                    new TaxItem { TaxRate = 4, TaxBase = 10, TaxAmount = 0.4m },
                    new TaxItem { TaxRate = 21, TaxBase = 100, TaxAmount = 21 }
                }
            };

            var invoiceEntryOk02 = new InvoiceEntry(invoiceOk02);

            invoiceEntryOk02.Save();

            invoiceEntryOk02.Status.Should().Be("Correcto",
                because: $"la presentación en AEAT falló con código {invoiceEntryOk02.ErrorCode}: {invoiceEntryOk02.ErrorDescription}");

            invoiceEntryOk02.CSV.Should().NotBeNullOrEmpty("debe generarse un CSV cuando el estado es Correcto");
            invoiceEntryOk02.Response.Should().NotBeNullOrEmpty("la respuesta de la AEAT no debe estar vacía");

            huellasIn[2] = (invoiceEntryOk02.Registro as RegistroAlta).Huella;

            // Reenvío de documento no incluido en la cadena por borrado (se incluye ahora)

            var invoiceRetrySendErrDisableBlockchainDeleteFalse = new InvoiceRetrySend(invoiceErrDisableBlockchainDeleteFalse);

            invoiceRetrySendErrDisableBlockchainDeleteFalse.Save();

            invoiceRetrySendErrDisableBlockchainDeleteFalse.Status.Should().Be("Correcto",
                because: $"la presentación en AEAT falló con código {invoiceRetrySendErrDisableBlockchainDeleteFalse.ErrorCode}: {invoiceRetrySendErrDisableBlockchainDeleteFalse.ErrorDescription}");

            invoiceRetrySendErrDisableBlockchainDeleteFalse.CSV.Should().NotBeNullOrEmpty("debe generarse un CSV cuando el estado es Correcto");
            invoiceRetrySendErrDisableBlockchainDeleteFalse.Response.Should().NotBeNullOrEmpty("la respuesta de la AEAT no debe estar vacía");

            huellasIn[3] = (invoiceRetrySendErrDisableBlockchainDeleteFalse.Registro as RegistroAlta).Huella;

            // Reenvío de documento ya incluido en la cadena (no se incluye ahora)

            var invoiceRetrySendinvoiceErrDisableBlockchainDeleteTrue = new InvoiceRetrySend(invoiceErrDisableBlockchainDeleteTrue);

            invoiceRetrySendinvoiceErrDisableBlockchainDeleteTrue.Save();

            invoiceRetrySendinvoiceErrDisableBlockchainDeleteTrue.Status.Should().Be("Correcto",
                because: $"la presentación en AEAT falló con código {invoiceRetrySendinvoiceErrDisableBlockchainDeleteTrue.ErrorCode}: {invoiceRetrySendinvoiceErrDisableBlockchainDeleteTrue.ErrorDescription}");

            invoiceRetrySendinvoiceErrDisableBlockchainDeleteTrue.CSV.Should().NotBeNullOrEmpty("debe generarse un CSV cuando el estado es Correcto");
            invoiceRetrySendinvoiceErrDisableBlockchainDeleteTrue.Response.Should().NotBeNullOrEmpty("la respuesta de la AEAT no debe estar vacía");

            // El registro no enviado en su momento y ya incluido en la cadena, ahora sólo se envía

            Assert.Equal(huellasIn[1], (invoiceRetrySendinvoiceErrDisableBlockchainDeleteTrue.Registro as RegistroAlta).Huella);
            

            var csvFile = Path.Combine(
                    Blockchain.Blockchain.Get(invoiceOk01.SellerID).ChainPath,
                    $"{now:yyyyMM}.csv");

            var csvLines = File.ReadAllLines(csvFile);            
            
            Assert.Equal(huellasIn[3], csvLines[csvLines.Length - 1].Split(';')[2]); // Ultimo eslabón el registro borrado al fallar las comunicaciones (invoiceErrDisableBlockchainDeleteFalse) 
            Assert.Equal(huellasIn[2], csvLines[csvLines.Length - 2].Split(';')[2]); // Factura correcta 2 envíada tras el fallo provocado en las comunicaciones (invoiceOk02)
            Assert.Equal(huellasIn[1], csvLines[csvLines.Length - 3].Split(';')[2]); // Factura fallida por fallo comunicaciones incluida en la cadena
                                                                                     // Aquí no tenemos nada por que en este fallo DisableBlockchainDelete = false (invoiceErrDisableBlockchainDeleteFalse)
            Assert.Equal(huellasIn[0], csvLines[csvLines.Length - 4].Split(';')[2]); // Factura correcta 1 envíada antes del fallo provocado en las comunicaciones (invoiceOk01)


        }

    }
}
