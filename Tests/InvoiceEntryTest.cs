using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeriFactu.Business;
using VeriFactu.Xml.Factu.Alta;
using Xunit;
using FluentAssertions;

namespace VeriFactu.Tests
{
    public class InvoiceEntryTest
    {

        [Fact]
        public void EjInvoiceEntry()
        {


            // --- ARRANGE (Preparación de datos) ---
            var now = DateTime.Now;

            var invoice = new Invoice($"GIT-EJ25-{now:ddMMyyyymmss}", now, "B72877814")
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

            var invoiceEntry = new InvoiceEntry(invoice);

            // --- ACT (Ejecución de la acción a probar) ---
            invoiceEntry.Save();

            // --- ASSERT (Verificaciones con afirmaciones automáticas) ---
            invoiceEntry.Status.Should().Be("Correcto",
                because: $"la presentación en AEAT falló con código {invoiceEntry.ErrorCode}: {invoiceEntry.ErrorDescription}");

            invoiceEntry.CSV.Should().NotBeNullOrEmpty("debe generarse un CSV cuando el estado es Correcto");
            invoiceEntry.Response.Should().NotBeNullOrEmpty("la respuesta de la AEAT no debe estar vacía");            


        }

    }
}
