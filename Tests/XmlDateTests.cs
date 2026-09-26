using System;
using System.Globalization;
using VeriFactu.Xml;
using Xunit;

namespace VeriFactu.Tests
{

    public class XmlDateTests
    {

        [Fact]
        public void DatesStayGregorianWhenTheCultureUsesAnotherCalendar()
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("th-TH");
                var day = new DateTime(2024, 1, 15, 19, 20, 30);

                Assert.Equal("15-01-2024", XmlParser.GetXmlDate(day));
                Assert.StartsWith("2024-01-15T19:20:30", XmlParser.GetXmlDateTimeIso8601(day));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [Fact]
        public void SpanishCultureKeepsTheExpeditionDate()
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("es-ES");
                Assert.Equal("15-01-2024", XmlParser.GetXmlDate(new DateTime(2024, 1, 15)));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

    }

}
