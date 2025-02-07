using System.Collections.Generic;

namespace VeriFactu.DataStore
{

    /// <summary>
    /// Representa un documento de factura generado en el sistema. Incluye todos los registros
    /// de alta y anulación envíados, así como todas las respuestas de la AEAT relacionadas con
    /// una factura determinada.
    /// </summary>
    public class Document
    {

        /// <summary>
        /// Información de vendedores y periodos.
        /// </summary>
        static Dictionary<string, List<PeriodOutbox>> _Sellers = Seller.GetSellers();

        /// <summary>
        /// Constructor.
        /// </summary>
        public Document(string sellerID, string periodID) 
        {

            PeriodID = periodID;
            PeriodOutbox = GetPeriodOutbox(sellerID, periodID);

            if (PeriodOutbox == null)
                return; 

            Seller = PeriodOutbox.Seller;


            //var outDocs = pOut.GetDocuments();


            //var pIn = new PeriodInbox(seller, periodID, 1000);
            //var inDocs = pIn.GetDocuments();



        }

        internal PeriodOutbox PeriodOutbox { get; private set; }

        /// <summary>
        /// Periodo del documento.
        /// </summary>
        public string PeriodID { get; private set; }

        /// <summary>
        /// Vendedor del documento.
        /// </summary>
        public Seller Seller { get; private set; }

        private PeriodOutbox GetPeriodOutbox(string sellerID, string periodID) 
        {

            PeriodOutbox periodOutbox = null;

            var periodOutboxes = _Sellers[sellerID];

            foreach (var pOutbox in periodOutboxes) 
                if (pOutbox.PeriodID == periodID) 
                    periodOutbox = pOutbox;

            return periodOutbox;
                
        }


    }
}
