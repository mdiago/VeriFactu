using System.Collections.Generic;
using System.Xml.Serialization;

namespace VeriFactu.Xml.Factu
{

    /// <summary>
    /// Registro de envío de alta o anulación al sistema de facturación.
    /// </summary>
    public class RegFactuSistemaFacturacion
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Datos de contexto de un suministro.
        /// </summary>
        [XmlElement("Cabecera", Namespace = Namespaces.NamespaceSFLR)]
        public Cabecera Cabecera { get; set; }

        /// <summary>
        /// Datos correspondientes a los registro de facturacion de alta o anulación.
        /// </summary>
        [XmlElement("RegistroFactura", Namespace = Namespaces.NamespaceSFLR)]
        public List<RegistroFactura> RegistroFactura { get; set; }


        /// <summary>
        /// Datos de control.
        /// </summary>
        public DatosControl DatosControl { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"Envío: {Cabecera} ({RegistroFactura.Count})";
        }

        #endregion

    }

}
