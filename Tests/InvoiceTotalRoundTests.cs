using System;
using System.Collections.Generic;
using VeriFactu.Business;
using VeriFactu.Xml.Factu.Alta;
using Xunit;

namespace VeriFactu.Tests
{

    public class InvoiceTotalRoundTests
    {

        [Fact]
        public void MidpointTaxIsRoundedAwayFromZeroOnTheTotal()
        {
            var invoice = new Invoice("A-1", new DateTime(2024, 1, 15), "B12345678");
            invoice.SellerName = "Test";
            invoice.Text = "Servicio";
            invoice.InvoiceType = TipoFactura.F2;
            invoice.TaxItems = new List<TaxItem>
            {
                new TaxItem { TaxBase = 10.00m, TaxRate = 21.00m, TaxAmount = 1.005m }
            };

            var alta = invoice.GetRegistroAlta();

            Assert.Equal("1.01", alta.Desglose[0].CuotaRepercutida);
            Assert.Equal("1.01", alta.CuotaTotal);
            Assert.Equal("11.01", alta.ImporteTotal);
        }

        [Fact]
        public void ExactCentsStayOnTheTotal()
        {
            var invoice = new Invoice("A-2", new DateTime(2024, 1, 15), "B12345678");
            invoice.SellerName = "Test";
            invoice.Text = "Servicio";
            invoice.InvoiceType = TipoFactura.F2;
            invoice.TaxItems = new List<TaxItem>
            {
                new TaxItem { TaxBase = 10.00m, TaxRate = 21.00m, TaxAmount = 2.10m }
            };

            var alta = invoice.GetRegistroAlta();

            Assert.Equal("2.10", alta.CuotaTotal);
            Assert.Equal("12.10", alta.ImporteTotal);
        }

    }

}
