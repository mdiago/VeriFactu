![image](https://github.com/mdiago/VeriFactu/assets/22330809/97e3b3d1-3e54-4834-bf71-911743cee8d7)

# Descripción

La finalidad de esta biblioteca es la generación, conservación y envío de registros relacionados con la emisión de facturas a la AEAT mediante un sistema VERI*FACTU.

En primer lugar se encuentran los ejemplos de la operativa básica más común. Después encontraremos causísticas más complejas...

Esperamos que esta documentación sea de utilidad, y agradeceremos profundamente cualquier tipo de colaboración o sugerencia. 

Podéis dirigir cualquier duda o consulta a info@irenesolutions.com.

[Irene Solutions](http://www.irenesolutions.com)

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

## 2. Obtención de la «URL» de cotejo o remisión de información de la factura contenida en el código «QR»

En este ejemplo obtendremos la url para el servicio de validación de una factura de las especificaciones técnicas de la AEAT.

![image](https://github.com/mdiago/VeriFactu/assets/22330809/2f11a627-b446-46a2-a36c-8e77127eb839)

> [!NOTE]  
> En la documentación técnica de la AEAT el último carácter debería se '1' pero por error consta '4'.

```C#
          
// Creamos una instacia de la clase factura
var invoice = new Invoice()
{
    InvoiceType = TipoFactura.F1,
    InvoiceID = "12345678&G33",
    InvoiceDate = new DateTime(2024, 1, 1),
    SellerID = "89890001K",
    TotalAmount = 241.1m
};

// Obtenemos una instancia de la clase RegistroAlta a partir de
// la instancia del objeto de negocio Invoice
var registro = invoice.GetRegistroAlta();

// Obtenemos la url de validación
var urlValidacion = registro.GetUrlValidate(); // https://prewww2.aeat.es/wlpl/TIKE-CONT/ValidarQR?nif=89890001K&numserie=12345678%26G33&fecha=01-01-2024&importe=241.1


```

## 3. Obtención de Bitmap con el QR con la URL de cotejo o remisión de información de la factura

En este ejemplo obtendremos la imágen del QR de la url para el servicio de validación de una factura de las especificaciones técnicas de la AEAT, que hemos visto en el ejemplo anterior.

```C#
          
// Creamos una instacia de la clase factura
var invoice = new Invoice()
{
    InvoiceType = TipoFactura.F1,
    InvoiceID = "12345678&G33",
    InvoiceDate = new DateTime(2024, 1, 1),
    SellerID = "89890001K",
    TotalAmount = 241.1m
};

// Obtenemos una instancia de la clase RegistroAlta a partir de
// la instancia del objeto de negocio Invoice
var registro = invoice.GetRegistroAlta();

// Obtenemos la imágen del QR
var bmQr = registro.GetValidateQr();

File.WriteAllBytes(@"C:\Users\usuario\Downloads\zz\ValidateQrSampe.bmp", bmQr);

```
El bitmap obtenido:

![image](https://github.com/mdiago/VeriFactu/assets/22330809/d91e9202-78f0-4c33-9c1e-578a5c5dd3e1)

Url que consta en el QR:

https://prewww2.aeat.es/wlpl/TIKE-CONT/ValidarQR?nif=89890001K&numserie=12345678%26G33&fecha=01-01-2024&importe=241.1


## 4. Inicialización de la cadena de bloques para un vendedor

Es este ejemplo iniciaremos la cadena de bloques del vendedor que figura en los ejemplos de la AEAT con NIF '89890001K'. Iniciaremos
la cadena de bloques con el primer ejemplo y luego añadiremos a la cadena el segundo ejemplo:

![image](https://github.com/mdiago/VeriFactu/assets/22330809/3ac3e2dc-5279-4702-9bda-ac028d167689)

![image](https://github.com/mdiago/VeriFactu/assets/22330809/0ef57803-edba-4705-adcb-0c79e6275453)

```C#
          
// Creamos una instacia de la clase factura (primera factura)
var invoiceFirst = new Invoice()
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
var registroFirst = invoiceFirst.GetRegistroAlta();

// El registro no ha sido envíado, pero forzamos el valor de
// FechaHoraHusoGenRegistro para que coincida con el ejemplo de la AEAT
var fechaHoraHusoGenRegistro = new DateTime(2024, 1, 1, 19, 20, 30); //2024-01-01T19:20:30+01:00 en España peninsula
registroFirst.FechaHoraHusoGenRegistro = XmlParser.GetXmlDateTimeIso8601(fechaHoraHusoGenRegistro);

// Ahora obtenemos el controlador de la cadena de bloques del vendedor
var blockchain = Blockchain.GetInstance(invoiceFirst.SellerID);
            
// Añadimos el registro de alta
blockchain.Add(registroFirst);

// Creamos una instacia de la clase factura (segunda factura)
var invoiceSecond = new Invoice()
{
    InvoiceType = TipoFactura.F1,
    InvoiceID = "12345679/G34",
    InvoiceDate = new DateTime(2024, 1, 1),
    SellerID = "89890001K",
    TotalTaxOutput = 12.35m,
    TotalAmount = 123.45m
};

// Obtenemos una instancia de la clase RegistroAlta a partir de
// la instancia del objeto de negocio Invoice
var registroSecond = invoiceSecond.GetRegistroAlta();

// El registro no ha sido envíado, pero forzamos el valor de
// FechaHoraHusoGenRegistro para que coincida con el ejemplo de la AEAT
fechaHoraHusoGenRegistro = new DateTime(2024, 1, 1, 19, 20, 35); //2024-01-01T19:20:35+01:00 en España peninsula
registroSecond.FechaHoraHusoGenRegistro = XmlParser.GetXmlDateTimeIso8601(fechaHoraHusoGenRegistro);

// Añadimos el registro de alta
blockchain.Add(registroSecond);

Debug.Print($"La huella de la primera factura es: {registroFirst.GetHashOutput()}");
Debug.Print($"La huella de la segunda factura es: {registroSecond.GetHashOutput()}");


```





