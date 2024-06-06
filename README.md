![image](https://github.com/mdiago/VeriFactu/assets/22330809/97e3b3d1-3e54-4834-bf71-911743cee8d7)

# Descripción

La finalidad de esta biblioteca es la generación, conservación y envío de registros relacionados con la emisión de facturas a la AEAT mediante un sistema VERI*FACTU.

# Ejemplos

## 1. Generación de la huella o hash de un registro de alta de factura

En este ejemplo calcularemos el hash de el registro de alta de verifactu que aparece en la documentación técnica.

![image](https://github.com/mdiago/VeriFactu/assets/22330809/cbe2b8c5-4536-49ec-89b5-cd10110fe2d6)


```C#
          
// Creamos una instacia de la clase factura
var invoice = new Invoice() 
{
    InvoiceType = TipoFactura.F1,
    InvoiceID = "12345678/G33",
    InvoiceDate = new DateTime(2024, 1, 1),
    SellerID = "89890001K",
    TotalTaxOutput = 12.35m,
    TotalAmount = 123.45m
};

// Obtenemos una instancia de la clase RegistroAlta a partir de
// la instancia del objeto de negocio Invoice
var registro = invoice.GetRegistroAlta();

// El registro no ha sido envíado, pero forzamos el valor de
// FechaHoraHusoGenRegistro para que coincida con el ejemplo de la AEAT
var fechaHoraHusoGenRegistro = new DateTime(2024, 1, 1, 19, 20, 30); //2024-01-01T19:20:30+01:00 en España peninsula
registro.FechaHoraHusoGenRegistro = XmlParser.GetXmlDateTimeIso8601(fechaHoraHusoGenRegistro);

// Obtenemos el valor de la huella
var hash = registro.GetHashOutput(); // 3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60

```


info@irenesolutions.com
