![image](https://github.com/mdiago/VeriFactu/assets/22330809/97e3b3d1-3e54-4834-bf71-911743cee8d7)

# VeriFactu - Facturación sistema VERI*FACTU de la AEAT

![workflow](https://github.com/mdiago/VeriFactu/actions/workflows/Verifactu.yml/badge.svg)

:receipt: ¡Automatiza el envío de facturas con la AEAT de forma fácil y eficiente utilizando VeriFactu!

La finalidad de esta biblioteca es la generación, conservación y envío de registros; relacionados con la emisión de facturas a la AEAT mediante un sistema VERI*FACTU ( :nerd_face: [Declaración responsable del software](https://github.com/mdiago/VeriFactu/blob/main/Doc/Legal/Declaracion%20Responsable%20v1.0.6-beta.pdf)).

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

Para emprezar, veamos un ejemplo sencillo de registro de una factura; El registro implica el almacenamiento de la factura en el sistema y el envío del documento a la AEAT:

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

// Creamos la entrada de la factura
var invoiceEntry = new InvoiceEntry(invoice);

// Guardamos la factura
invoiceEntry.Save();

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

En este ejemplo calcularemos el hash de el registro de alta de verifactu que aparece en la documentación técnica.

![image](https://github.com/mdiago/VeriFactu/assets/22330809/cbe2b8c5-4536-49ec-89b5-cd10110fe2d6)


```C#
          
// Creamos una instacia de la clase factura
var invoice = new Invoice("12345678/G33", new DateTime(2024, 1, 1), "89890001K") 
{
    TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxRate = 10,
            TaxBase = 111.1m,
            TaxAmount = 12.35m
        }
    }
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
var invoice = new Invoice("12345678&G33", new DateTime(2024, 1, 1), "89890001K")
{
    TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxRate = 21,
            TaxBase = 199.25m,
            TaxAmount = 41.85m
        }
    }
};

// Obtenemos una instancia de la clase RegistroAlta a partir de
// la instancia del objeto de negocio Invoice
var registro = invoice.GetRegistroAlta();

// Obtenemos la url de validación
var urlValidacion = registro.GetUrlValidate(); // https://prewww2.aeat.es/wlpl/TIKE-CONT/ValidarQR?nif=89890001K&numserie=12345678%26G33&fecha=01-01-2024&importe=241.10


```

## 3. Obtención de Bitmap con el QR con la URL de cotejo o remisión de información de la factura

En este ejemplo obtendremos la imágen del QR de la url para el servicio de validación de una factura de las especificaciones técnicas de la AEAT, que hemos visto en el ejemplo anterior.

```C#
          
// Creamos una instacia de la clase factura
var invoice = new Invoice("12345678&G33", new DateTime(2024, 1, 1), "89890001K")
{
    TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxRate = 21,
            TaxBase = 199.25m,
            TaxAmount = 41.85m
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
El bitmap obtenido:

![image](https://github.com/user-attachments/assets/24448239-3319-4272-8c97-069fae6fcd65)

Url que consta en el QR:

https://prewww2.aeat.es/wlpl/TIKE-CONT/ValidarQR?nif=89890001K&numserie=12345678%26G33&fecha=01-01-2024&importe=241.10


## 4. Inicialización de la cadena de bloques para un vendedor

Es este ejemplo iniciaremos la cadena de bloques del vendedor que figura en los ejemplos de la AEAT con NIF '89890001K'. Iniciaremos
la cadena de bloques con el primer ejemplo y luego añadiremos a la cadena el segundo ejemplo:

![image](https://github.com/mdiago/VeriFactu/assets/22330809/3ac3e2dc-5279-4702-9bda-ac028d167689)

![image](https://github.com/mdiago/VeriFactu/assets/22330809/0ef57803-edba-4705-adcb-0c79e6275453)

```C#
          
// Creamos una instacia de la clase factura (primera factura)
var invoiceFirst = new Invoice("12345678/G33", new DateTime(2024, 1, 1), "89890001K")
{
    InvoiceType = TipoFactura.F1,
    TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 10,
            TaxBase = 111.1m,
            TaxAmount = 12.35m
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
var invoiceSecond = new Invoice("12345679/G34", new DateTime(2024, 1, 1), "89890001K")
{
    InvoiceType = TipoFactura.F1,
    TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxScheme = ClaveRegimen.RegimenGeneral,
            TaxType = CalificacionOperacion.S1,
            TaxRate = 10,
            TaxBase = 111.1m,
            TaxAmount = 12.35m
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





