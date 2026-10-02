using FluentAssertions;
using System;
using System.Collections.Generic;
using VeriFactu.Business;
using VeriFactu.Config;
using VeriFactu.Xml.Factu.Alta;
using Xunit;

namespace VeriFactu.Tests
{
    public class DisableBlockchainDeleteChainingTest
    {

        private static Invoice GetInvoice(string invoiceID, DateTime now)
        {

            return new Invoice(invoiceID, now, "B72877814")
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

        }

        [Fact]
        public void EjEncadenamientoTrasFalloComunicaciones()
        {

            Settings.Current.DisableBlockchainDelete = true;

            var now = DateTime.Now;

            // Envío alta 01 correcta
            var invoiceOk01 = GetInvoice($"OK-{now:ddMMyyyymmss}-CH-001", now);
            var invoiceEntryOk01 = new InvoiceEntry(invoiceOk01);
            invoiceEntryOk01.Save();

            invoiceEntryOk01.Status.Should().Be("Correcto",
                because: $"la presentación en AEAT falló con código {invoiceEntryOk01.ErrorCode}: {invoiceEntryOk01.ErrorDescription}");

            // Envío alta con fallo de comunicaciones: el eslabón se conserva en la cadena
            var tmp = Settings.Current.VeriFactuEndPointPrefix;
            Settings.Current.VeriFactuEndPointPrefix = "https://192.0.2.1/";

            var invoiceKo = GetInvoice($"KO-{now:ddMMyyyymmss}-CH-LINK_INCLUDED", now);
            var invoiceEntryKo = new InvoiceEntry(invoiceKo);

            Action act = () => invoiceEntryKo.Save();

            act.Should()
               .Throw<Common.Exceptions.SendException>("se provocó un fallo intencional de red simulando desconexión");

            Settings.Current.VeriFactuEndPointPrefix = tmp;

            var registroKo = invoiceEntryKo.Registro as RegistroAlta;

            Blockchain.Blockchain.Get(invoiceKo.SellerID).Current.Huella.Should().Be(registroKo.Huella,
                because: "con DisableBlockchainDelete = true el registro no enviado sigue siendo el último eslabón");

            // Envío alta 02: debe encadenar con el registro no enviado, no con la alta 01
            var invoiceOk02 = GetInvoice($"OK-{now:ddMMyyyymmss}-CH-002", now);
            var invoiceEntryOk02 = new InvoiceEntry(invoiceOk02);
            invoiceEntryOk02.Save();

            invoiceEntryOk02.Status.Should().Be("Correcto",
                because: $"la presentación en AEAT falló con código {invoiceEntryOk02.ErrorCode}: {invoiceEntryOk02.ErrorDescription}");

            var registroAnterior = (invoiceEntryOk02.Registro as RegistroAlta).Encadenamiento.RegistroAnterior;

            registroAnterior.Huella.Should().Be(registroKo.Huella);
            registroAnterior.NumSerieFactura.Should().Be(invoiceKo.InvoiceID);

            // Reenvío del registro no enviado: se remite el registro original y la cadena no cambia
            var invoiceRetrySendKo = new InvoiceRetrySend(invoiceKo);
            invoiceRetrySendKo.Save();

            invoiceRetrySendKo.Status.Should().Be("Correcto",
                because: $"la presentación en AEAT falló con código {invoiceRetrySendKo.ErrorCode}: {invoiceRetrySendKo.ErrorDescription}");

            (invoiceRetrySendKo.Registro as RegistroAlta).Huella.Should().Be(registroKo.Huella,
                because: "el reenvío remite el registro original");

            var huellaOk02 = (invoiceEntryOk02.Registro as RegistroAlta).Huella;

            Blockchain.Blockchain.Get(invoiceKo.SellerID).Current.Huella.Should().Be(huellaOk02,
                because: "el reenvío de un registro ya incluido en la cadena no añade un eslabón nuevo");

            // Envío alta 03: encadena con la alta 02
            var invoiceOk03 = GetInvoice($"OK-{now:ddMMyyyymmss}-CH-003", now);
            var invoiceEntryOk03 = new InvoiceEntry(invoiceOk03);
            invoiceEntryOk03.Save();

            invoiceEntryOk03.Status.Should().Be("Correcto",
                because: $"la presentación en AEAT falló con código {invoiceEntryOk03.ErrorCode}: {invoiceEntryOk03.ErrorDescription}");

            (invoiceEntryOk03.Registro as RegistroAlta).Encadenamiento.RegistroAnterior.Huella.Should().Be(huellaOk02);

        }

    }
}
