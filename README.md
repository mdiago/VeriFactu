![image](https://github.com/mdiago/VeriFactu/assets/22330809/97e3b3d1-3e54-4834-bf71-911743cee8d7)

# VeriFactu - Facturación sistema VERI*FACTU de la AEAT

![workflow](https://github.com/mdiago/VeriFactu/actions/workflows/Verifactu.yml/badge.svg)

## :receipt: ¡Automatiza el envío de facturas con la AEAT de forma fácil y eficiente utilizando VeriFactu!
<br>

La finalidad de esta biblioteca es la generación, conservación y envío de registros; relacionados con la emisión de facturas a la AEAT mediante un sistema VERI*FACTU ( :nerd_face: [Declaración responsable del software](https://github.com/mdiago/VeriFactu/blob/main/NetFramework/Doc/Legal/Declaracion%20Responsable%20v1.0.28-beta.pdf)).

🚀 **Si esta librería te resulta útil, ayúdanos a seguir creciendo marcando ⭐ el repositorio en GitHub**. ¡Cada estrella nos motiva a seguir mejorando!

<br>

> ### La funcionalidad de Verifactu está disponible ( :wink: gratis) también en línea:
>
> :globe_with_meridians: [Acceso al API REST](https://facturae.irenesolutions.com/verifactu/go)
> 
> Con el API REST disponemos de una herramienta de trabajo sencilla sin la complicación de preocuparnos de la gestión de certificados digitales.
> ( :nerd_face: [Declaración responsable del API REST](https://github.com/mdiago/VeriFactu/blob/main/NetFramework/Doc/Legal/Declaracion%20Responsable%20API%20REST%20v1.0.17-beta.pdf)).

<br>
<br>

Esperamos que esta documentación sea de utilidad, y agradeceremos profundamente cualquier tipo de colaboración o sugerencia. 

En primer lugar se encuentran los ejemplos de la operativa básica más común. Después encontraremos causísticas más complejas... y si queremos profundizar más siempre podemos recurrir a la [wiki del proyecto](https://github.com/mdiago/VeriFactu/wiki).

📩 **Contacto**  
Para cualquier duda o consulta, puedes escribirnos a **info@irenesolutions.com**.

[Irene Solutions](http://www.irenesolutions.com)

## Quickstart

### Instalar el paquete con el administrador de paquetes NuGet

![image](https://github.com/user-attachments/assets/2f872bc8-51f9-49c3-a8dc-3551d56adc20)

### Instalar el paquete con dotnet CLI

`dotnet add package Verifactu`

<br>
<br>
 
> [!IMPORTANT]
> Antes de comenzar a probar los envíos a la AEAT hay que configurar correctamente el certificado con el que vamos a trabajar.
> Podemos cargar el certificado desde un archivo .pfx / .p12 guardado en el disco, o (en Windows) cargar un certificado del  almacén de certificados de windows. La configuración del sistema esta accesible mediante la propiedad estática 'Current' del objeto `Settings'. En la siguiente tabla se describen los valores de configuración relacionados con el   certificado a utilizar:

<br>
<br>

## Establecer en la configuración los valores para el uso del certificado


| Propiedad  | Descripción |
| ------------- | ------------- |
| CertificatePath  | Ruta al archivo del certificado a utilizar.   |
| CertificatePassword  | Password del certificado. Este valor sólo es necesario si tenemos establecido el valor para 'CertificatePath' y el certificado tiene clave de acceso. Sólo se utiliza en los certificados cargados desde el sistema de archivos.  |
| CertificateSerial  | Número de serie del certificado a utilizar. Mediante este número de serie se selecciona del almacén de certificados de windows el certificado con el que realizar las comunicaciones.  |
| CertificateThumbprint  | Hash o Huella digital del certificado a utilizar. Mediante esta huella digital se selecciona del almacén de certificados de windows el certificado con el que realizar las comunicaciones.    |

En el siguiente ejemplo estableceremos la configuración de nuestro certificado para cargarlo desde el sitema de archivos:

### C#
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

### VB
```VB

' Valores actuales de configuración de certificado
Debug.Print($"{Settings.Current.CertificatePath}")
Debug.Print($"{Settings.Current.CertificatePassword}")

' Establezco nuevos valores
Settings.Current.CertificatePath = "C:\CERTIFICADO.pfx"
Settings.Current.CertificatePassword = "pass certificado"

' Guardo los cambios
Settings.Save()

```


## Envío de facturas

Para empezar, veamos un ejemplo sencillo de registro de una factura; El registro implica el almacenamiento de la factura en el sistema y el envío del documento a la AEAT:

### C#
```C#
// Creamos una instacia de la clase factura
var invoice = new Invoice("GIT-EJ-0002", new DateTime(2024, 11, 15), "B72877814")
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

### VB
```VB

' Creamos una instacia de la clase factura
Dim invoice = New Invoice("GIT-EJ-0052", New DateTime(2024, 12, 3), "B72877814")
 With invoice
     .InvoiceType = TipoFactura.F1
     .SellerName = "WEFINZ GANDIA SL"
     .BuyerID = "B44531218"
     .BuyerName = "WEFINZ SOLUTIONS SL"
     .Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE"
     .TaxItems = New List(Of TaxItem) From {
         New TaxItem() With
         {
             .TaxRate = 4,
             .TaxBase = 10,
             .TaxAmount = 0.4
         },
         New TaxItem() With
         {
             .TaxRate = 21,
             .TaxBase = 100,
             .TaxAmount = 21
         }
     }
 End With

 ' Creamos la entrada de la factura
 Dim invoiceEntry As InvoiceEntry = New InvoiceEntry(invoice)

 ' Guardamos la factura
 invoiceEntry.Save()

 ' Consultamos el estado
 Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.Status}")

 If (invoiceEntry.Status = "Correcto") Then

     ' Consultamos el CSV
     Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.CSV}")

 Else

     ' Consultamos el error
     Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.ErrorCode}: {invoiceEntry.ErrorDescription}")

 End If

 ' Consultamos el resultado devuelto por la AEAT
 Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.Response}")

```
## Anulación de facturas

Seguimos con la anulación de una factura previamente remitida:

### C#
```C#

// Creamos una instacia de la clase factura con los datos del documento a anular
var invoice = new Invoice("GIT-EJ-0002", new DateTime(2024, 11, 15), "B72877814")
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
### VB
```VB

 ' Creamos una instacia de la clase factura
 Dim invoice = New Invoice("GIT-EJ-0052", New DateTime(2024, 12, 3), "B72877814")
 With invoice
     .SellerName = "WEFINZ GANDIA SL"
 End With

 ' Creamos la cancelación de la factura
 Dim invoiceCancellation As InvoiceCancellation = New InvoiceCancellation(invoice)

 ' Guardamos la cancelación factura
 invoiceCancellation.Save()

 ' Consultamos el resultado devuelto por la AEAT
 Debug.Print($"Respuesta de la AEAT:\n{invoiceCancellation.Response}")

```

# Ejemplos

## 1. Generación de la huella o hash de un registro de alta de factura

```C#
// Creamos una instacia de la clase factura
var invoice = new Invoice("GITHUB-EJ-003", new DateTime(2024, 11, 4), "B72877814")
{
    BuyerID = "B44531218",
    BuyerName = "WEFINZ SOLUTIONS SL",
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

// Obtenemos una instancia de la clase RegistroAlta a partir de
// la instancia del objeto de negocio Invoice
var registro = invoice.GetRegistroAlta();

// El registro no ha sido envíado, pero forzamos el valor de
// FechaHoraHusoGenRegistro para que coincida con el últomo envío a la AEAT
var fechaHoraHusoGenRegistro = new DateTime(2024, 11, 4, 12, 36, 39); //2024-01-01T19:20:30+01:00 en España peninsula
registro.FechaHoraHusoGenRegistro = XmlParser.GetXmlDateTimeIso8601(fechaHoraHusoGenRegistro);

// Establecemos el valor del encadenamiento anterior
registro.Encadenamiento = new Encadenamiento() 
{ 
    RegistroAnterior = new RegistroAnterior() 
    { 
        Huella = "8C8DCEFB120522E0C71BC19902F44D5334FF6C98E74F0E3AC1D1E5A30C2EA836" 
    } 
};

// Obtenemos el valor de la huella
var hash = registro.GetHashOutput(); // 4EECCE4DD48C0539665385D61D451BA921B7160CA6FEF46CD3C2E2BC5C778E14

```

## 2. Obtención de la «URL» de cotejo o remisión de información de la factura contenida en el código «QR»

En este ejemplo obtendremos la url para el servicio de validación de una factura envíada al entorno de pruebas de la AEAT.

```C#
// Creamos una instacia de la clase factura
var invoice = new Invoice("GITHUB-EJ-004", new DateTime(2024, 11, 4), "B72877814")
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

// Obtenemos una instancia de la clase RegistroAlta a partir de
// la instancia del objeto de negocio Invoice
var registro = invoice.GetRegistroAlta();

// Obtenemos la url
var urlValidacion = registro.GetUrlValidate(); //https://prewww2.aeat.es/wlpl/TIKE-CONT/ValidarQR?nif=B72877814&numserie=GITHUB-EJ-004&fecha=04-11-2024&importe=131.4

```

## 3. Obtención de Bitmap con el QR con la URL de cotejo o remisión de información de la factura

En este ejemplo obtendremos la imágen del QR de la url para el servicio de validación de una factura envíada previamente al entorno de pruebas de la AEAT:

![image](https://github.com/user-attachments/assets/bc0e7562-4f20-4acc-a17d-95434f7e8ab1)


```C#

// Creamos una instacia de la clase factura
var invoice = new Invoice("GITHUB-EJ-004", new DateTime(2024, 11, 4), "B72877814")
{
    SellerName = "WEFINZ GANDIA SL",
    BuyerID = "B44531218",
    BuyerName = "WEFINZ SOLUTIONS SL",
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

// Obtenemos una instancia de la clase RegistroAlta a partir de
// la instancia del objeto de negocio Invoice
var registro = invoice.GetRegistroAlta();

// Obtenemos la imágen del QR
var bmQr = registro.GetValidateQr();

File.WriteAllBytes(@"C:\Users\usuario\Downloads\ValidateQrSampe.bmp", bmQr);

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
var blockchain = Blockchain.Get(invoiceFirst.SellerID);

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
          
// En este ejemplo añadimos a la cola de envío de registros 1.005 registros de
// alta y 10 registros de cancelación (de los 10 primeros registros de alta)
// Si activamos el log podremos revisar en el mismo que el sistema realiza 2
// envíos. El primero al alcanzar los 1.000 resgistros, y el segundo al transcurrir
// el tiempo de espera establecido en la respuesta a la primera llamada.

// Activo el log
Settings.Current.LoggingEnabled = true;

var testId = "08";
int start = 0;

// Deshabilito la validación de NIF en linea con la AEAT
Settings.Current.SkipNifAeatValidation = true;
// Deshabilito la validación de NIFs intracomunitarios
Settings.Current.SkipViesVatNumberValidation = true;

// Añado 1.005 registros de alta
for (int i = 1; i < 1006; i++)
{

    // Creamos una instacia de la clase factura
    var invoice = new Invoice($"TEST{testId}" + $"{start + i}".PadLeft(8, '0'), new DateTime(2024, 10, 29), "B10795649")
    {
        InvoiceType = TipoFactura.F1,
        SellerName = "KIVU SOLUTIONS SL",
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

// Añado la cancelación de los primeros 10 registros
for (int i = 1; i < 11; i++)
{

    // Creamos una instacia de la clase factura
    var invoice = new Invoice($"TEST{testId}" + $"{start + i}".PadLeft(8, '0'), new DateTime(2024, 10, 29), "B10795649")
    {
        InvoiceType = TipoFactura.F1,
        SellerName = "KIVU SOLUTIONS SL",
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
    var invoiceCencellation = new InvoiceCancellation(invoice);

    // Añadimos el documentos a la cola de procesamiento:
    // En la cola se irán realizando los envíos cuando
    // los documentos en espera sean 1.000 o cuando el
    // tiempo de espera haya finalizado
    InvoiceQueue.ActiveInvoiceQueue.Add(invoiceCencellation);

}

```

Al ejecutar este código, si tenemos el log activado obtendremos la siguiente información en el mismo:

```

[000001] 2024-11-15 17:31:48: Ejecutando por cola (B10795649) tras tiempo espera en segundos: 60 desde 01/01/0001 0:00:00 hasta 01/01/0001 0:01:00
[000002] 2024-11-15 17:31:48: Actualizando datos de la cadena de bloques (B10795649) en 1000 elementos 15/11/2024 17:31:48
[000003] 2024-11-15 17:31:49: Finalizada actualización de datos de la cadena de bloques en 1000 elementos 15/11/2024 17:31:49
[000004] 2024-11-15 17:31:49: Enviando datos a la AEAT B10795649 de 1000 elementos 15/11/2024 17:31:49
[000005] 2024-11-15 17:31:52: Finalizado envío de datos B10795649 a la AEAT de 1000 elementos (quedan 15 registros) 15/11/2024 17:31:52
[000006] 2024-11-15 17:31:52: Establecido momento próxima ejecución B10795649 (LastProcessMoment: 15/11/2024 17:31:52 + CurrentWaitSecods: 60) = 15/11/2024 17:32:52
[000007] 2024-11-15 17:32:53: Ejecutando por cola (B10795649) tras tiempo espera en segundos: 60 desde 15/11/2024 17:31:52 hasta 15/11/2024 17:32:52
[000008] 2024-11-15 17:32:53: Actualizando datos de la cadena de bloques (B10795649) en 15 elementos 15/11/2024 17:32:53
[000009] 2024-11-15 17:32:53: Finalizada actualización de datos de la cadena de bloques en 15 elementos 15/11/2024 17:32:53
[000010] 2024-11-15 17:32:53: Enviando datos a la AEAT B10795649 de 15 elementos 15/11/2024 17:32:53
[000011] 2024-11-15 17:32:53: Finalizado envío de datos B10795649 a la AEAT de 15 elementos (quedan 0 registros) 15/11/2024 17:32:53
[000012] 2024-11-15 17:32:53: Establecido momento próxima ejecución B10795649 (LastProcessMoment: 15/11/2024 17:32:53 + CurrentWaitSecods: 60) = 15/11/2024 17:33:53


```

> [!IMPORTANT]
> Es importante señalar que el control de flujo se desarrolla en un hilo diferente al principal. Es importante asegurarnos,
> antes de finalizar la ejecución de nuestra aplicación en el hilo principal, de que finalizamos el proceso de control
> de flujo.

El control de flujo se finaliza de la siguiente manera:

```C#

// Cerramos la cola asegurandonos previamente de que no queda nada pendiente
if (InvoiceQueue.ActiveInvoiceQueue.Count == 0)
    InvoiceQueue.Exit();

```
