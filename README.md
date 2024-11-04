![image](https://github.com/mdiago/VeriFactu/assets/22330809/97e3b3d1-3e54-4834-bf71-911743cee8d7)

# VeriFactu - Facturación sistema VERI*FACTU de la AEAT

![workflow](https://github.com/mdiago/VeriFactu/actions/workflows/Verifactu.yml/badge.svg)

:receipt: ¡Automatiza el envío de facturas con la AEAT de forma fácil y eficiente utilizando VeriFactu!

La finalidad de esta biblioteca es la generación, conservación y envío de registros; relacionados con la emisión de facturas a la AEAT mediante un sistema VERI*FACTU ( :nerd_face: [Declaración responsable del software](https://github.com/mdiago/VeriFactu/blob/main/Doc/Legal/Declaracion%20Responsable%20v1.0.8-beta.pdf)).

La funcionalidad de Verifactu está disponible también en línea:

:globe_with_meridians: [Acceso al API REST](https://facturae.irenesolutions.com/verifactu/go)

Con el API REST disponemos de una herramienta de trabajo sencilla sin la complicación de preocuparnos de la gestión de certificados digitales.

Esperamos que esta documentación sea de utilidad, y agradeceremos profundamente cualquier tipo de colaboración o sugerencia. 

En primer lugar se encuentran los ejemplos de la operativa básica más común. Después encontraremos causísticas más complejas...

Podéis dirigir cualquier duda o consulta a info@irenesolutions.com.

[Irene Solutions](http://www.irenesolutions.com)

## Establecer en la configuración los valores para el uso del certificado

> [!IMPORTANT]
> Antes de comenzar a probar los envíos a la AEAT hay que configurar correctamente el certificado con el que vamos a trabajar.
> Podemos cargar el certificado desde un archivo .pfx / .p12 guardado en el disco, o (en Windows) cargar un certificado del  almacén de certificados de windows. La configuración del sistema esta accesible mediante la propiedad estática 'Current' del objeto `Settings'. En la siguiente tabla se describen los valores de configuración relacionados con el   certificado a utilizar:





| Propiedad  | Descripción |
| ------------- | ------------- |
| CertificatePath  | Ruta al archivo del certificado a utilizar.   |
| CertificatePassword  | Password del certificado. Este valor sólo es necesario si tenemos establecido el valor para 'CertificatePath' y el certificado tiene clave de acceso. Sólo se utiliza en los certificados cargados desde el sistema de archivos.  |
| CertificateSerial  | Número de serie del certificado a utilizar. Mediante este número de serie se selecciona del almacén de certificados de windows el certificado con el que realizar las comunicaciones.  |
| CertificateThumbprint  | Hash o Huella digital del certificado a utilizar. Mediante esta huella digital se selecciona del almacén de certificados de windows el certificado con el que realizar las comunicaciones.    |

En el siguiente ejemplo estableceremos la configuración de nuestro certificado para cargarlo desde el sitema de archivos:

```C#

// Valores actuales de configuración de certificado
Debug.Print($"{Settings.Current.CertificatePath}");
Debug.Print($"{Settings.Current.CertificatePassword}");

// Establezco nuevos valores
Settings.Current.CertificatePath = @"C:\CERTIFICADO.pfx";
Settings.Current.CertificatePassword = "pass certificado";

// Guardo los cambios
Settings.Save();

```


## Envío de facturas

Para empezar, veamos un ejemplo sencillo de registro de una factura; El registro implica el almacenamiento de la factura en el sistema y el envío del documento a la AEAT:

```C#

// Creamos una instacia de la clase factura
var invoice = new Invoice("GITHUB-EJ-002", new DateTime(2024, 11, 4), "B72877814")
{
    InvoiceType = TipoFactura.F1,
    SellerName = "WEFINZ GANDIA SL",
    BuyerID = "B44531218",
    BuyerName = "WEFINZ SOLUTIONS SL",
    Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
    TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxRate = 4,
            TaxBase = 10,
            TaxAmount = 0.4m
        },
        new TaxItem()
        {
            TaxRate = 21,
            TaxBase = 100,
            TaxAmount = 21
        }
    }
};

// Creamos la entrada de la factura
var invoiceEntry = new InvoiceEntry(invoice);

// Guardamos la factura
invoiceEntry.Save();

// Consultamos el estado
Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.Status}");

if (invoiceEntry.Status == "Correcto")
{

    // Consultamos el CSV
    Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.CSV}");


}
else 
{

    // Consultamos el error
    Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.ErrorCode}: {invoiceEntry.ErrorDescription}");


}

// Consultamos el resultado devuelto por la AEAT
Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.Response}");

```
## Anulación de facturas

Seguimos con la anulación de una factura previamente remitida:

```C#
// Creamos una instacia de la clase factura con los datos del documento a anular
var invoice = new Invoice("TEST009", new DateTime(2024, 10, 14), "B72877814")
{
    SellerName = "WEFINZ GANDIA SL",
};

// Creamos la cancelación de la factura
var invoiceCancellation = new InvoiceCancellation(invoice);

// Guardamos la cancelación factura
invoiceCancellation.Save();

// Consultamos el resultado devuelto por la AEAT
Debug.Print($"Respuesta de la AEAT:\n{invoiceCancellation.Response}");

```

# Ejemplos

## 1. Generación de la huella o hash de un registro de alta de factura

```C#
          
// Creamos una instacia de la clase factura
var invoice = new Invoice("TEST009", new DateTime(2024, 10, 14), "B72877814")
{
    InvoiceType = TipoFactura.F1,
    SellerName = "WEFINZ GANDIA SL",
    BuyerID = "B44531218",
    BuyerName = "WEFINZ SOLUTIONS SL",
    Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
    TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 4,
            TaxBase = 10,
            TaxAmount = 0.4m
        },
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 21,
            TaxBase = 100,
            TaxAmount = 21
        }
    }
};

// Obtenemos una instancia de la clase RegistroAlta a partir de
// la instancia del objeto de negocio Invoice
var registro = invoice.GetRegistroAlta();

// El registro no ha sido envíado, pero forzamos el valor de
// FechaHoraHusoGenRegistro
var fechaHoraHusoGenRegistro = new DateTime(2024, 1, 1, 19, 20, 30); 
registro.FechaHoraHusoGenRegistro = XmlParser.GetXmlDateTimeIso8601(fechaHoraHusoGenRegistro);

// Obtenemos el valor de la huella
var hash = registro.GetHashOutput(); 

```

## 2. Obtención de la «URL» de cotejo o remisión de información de la factura contenida en el código «QR»

En este ejemplo obtendremos la url para el servicio de validación de una factura de las especificaciones técnicas de la AEAT.

```C#
          
// Creamos una instacia de la clase factura
var invoice = new Invoice("TEST009", new DateTime(2024, 10, 14), "B72877814")
{
    InvoiceType = TipoFactura.F1,
    SellerName = "WEFINZ GANDIA SL",
    BuyerID = "B44531218",
    BuyerName = "WEFINZ SOLUTIONS SL",
    Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
    TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 4,
            TaxBase = 10,
            TaxAmount = 0.4m
        },
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 21,
            TaxBase = 100,
            TaxAmount = 21
        }
    }
};

// Obtenemos una instancia de la clase RegistroAlta a partir de
// la instancia del objeto de negocio Invoice
var registro = invoice.GetRegistroAlta();

// Obtenemos la url de validación
var urlValidacion = registro.GetUrlValidate(); 

```

## 3. Obtención de Bitmap con el QR con la URL de cotejo o remisión de información de la factura

En este ejemplo obtendremos la imágen del QR de la url para el servicio de validación de una factura de las especificaciones técnicas de la AEAT, que hemos visto en el ejemplo anterior.

```C#
          
// Creamos una instacia de la clase factura
var invoice = new Invoice("TEST009", new DateTime(2024, 10, 14), "B72877814")
{
    InvoiceType = TipoFactura.F1,
    SellerName = "WEFINZ GANDIA SL",
    BuyerID = "B44531218",
    BuyerName = "WEFINZ SOLUTIONS SL",
    Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
    TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 4,
            TaxBase = 10,
            TaxAmount = 0.4m
        },
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 21,
            TaxBase = 100,
            TaxAmount = 21
        }
    }
};

// Obtenemos una instancia de la clase RegistroAlta a partir de
// la instancia del objeto de negocio Invoice
var registro = invoice.GetRegistroAlta();

// Obtenemos la imágen del QR
var bmQr = registro.GetValidateQr();

File.WriteAllBytes(@"C:\Users\usuario\Downloads\zz\ValidateQrSampe.bmp", bmQr);

```

## 4. Inicialización de la cadena de bloques para un vendedor

Es este ejemplo iniciaremos la cadena de bloques de un vendedor:

```C#
          
// Creamos una instacia de la clase factura (primera factura)
var invoiceFirst = new Invoice("TEST001", new DateTime(2024, 10, 14), "B72877814")
{
    InvoiceType = TipoFactura.F1,
    SellerName = "WEFINZ GANDIA SL",
    BuyerID = "B44531218",
    BuyerName = "WEFINZ SOLUTIONS SL",
    Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
    TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 4,
            TaxBase = 10,
            TaxAmount = 0.4m
        },
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 21,
            TaxBase = 100,
            TaxAmount = 21
        }
    }
};

// Obtenemos una instancia de la clase RegistroAlta a partir de
// la instancia del objeto de negocio Invoice
var registroFirst = invoiceFirst.GetRegistroAlta();

// Ahora obtenemos el controlador de la cadena de bloques del vendedor
var blockchain = Blockchain.GetInstance(invoiceFirst.SellerID);
            
// Añadimos el registro de alta
blockchain.Add(registroFirst);

// Creamos una instacia de la clase factura (segunda factura)
var invoiceSecond = new Invoice("TEST002", new DateTime(2024, 10, 14), "B72877814")
{
    InvoiceType = TipoFactura.F1,
    SellerName = "WEFINZ GANDIA SL",
    BuyerID = "B44531218",
    BuyerName = "WEFINZ SOLUTIONS SL",
    Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
    TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 4,
            TaxBase = 10,
            TaxAmount = 0.4m
        },
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 21,
            TaxBase = 100,
            TaxAmount = 21
        }
    }
};

// Obtenemos una instancia de la clase RegistroAlta a partir de
// la instancia del objeto de negocio Invoice
var registroSecond = invoiceSecond.GetRegistroAlta();

// Añadimos el registro de alta
blockchain.Add(registroSecond);

Debug.Print($"La huella de la primera factura es: {registroFirst.GetHashOutput()}");
Debug.Print($"La huella de la segunda factura es: {registroSecond.GetHashOutput()}");

```

## 5. Control de flujo

La AEAT establece en sus especificaciones, que la aplicación debe disponer de un control de flujo que respete los tiempos de espera entre envíos. Estos tiempos vienen en las respuestas que remite. 
El sistema informático debe esperar antes de realizar otro envío a que transcurra este tiempo o se alcancen los 1.000 registros pendientes de envío. En el siguiente ejemplo veremos como podemos gestionar este flujo con Verifactu:

```C#
          
// Deshabilito la validación de NIF en linea con la AEAT
Settings.Current.SkipNifAeatValidation = true;

for (int i = 1; i < 1006; i++) 
{

    // Creamos una instacia de la clase factura
    var invoice = new Invoice("" + $"{i}".PadLeft(5, '0'), new DateTime(2024, 10, 29), "B12959755")
    {
        InvoiceType = TipoFactura.F1,
        SellerName = "IRENE SOLUTIONS SL",
        BuyerID = "B44531218",
        BuyerName = "WEFINZ SOLUTIONS SL",
        Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
        TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 4,
            TaxBase = 10,
            TaxAmount = 0.4m
        },
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 21,
            TaxBase = 100,
            TaxAmount = 21
        }
    }
    };

    // Creamos el documento de alta
    Debug.Print($"Añadiendo factura {invoice} {DateTime.Now}");
    var invoiceEntry = new InvoiceEntry(invoice);

    // Añadimos el documentos a la cola de procesamiento:
    // En la cola se irán realizando los envíos cuando
    // los documentos en espera sean 1.000 o cuando el
    // tiempo de espera haya finalizado
    InvoiceQueue.ActiveInvoiceQueue.Add(invoiceEntry);

}

```



