/*
    This file is part of the VeriFactu (R) project.
    Copyright (c) 2023-2024 Irene Solutions SL
    Authors: Irene Solutions SL.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    IRENE SOLUTIONS SL. IRENE SOLUTIONS SL DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
        http://www.irenesolutions.com/terms-of-use.pdf
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the VeriFactu software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving sii XML data on the fly in a web application, shipping VeriFactu
    with a closed source product.
    
    For more information, please contact Irene Solutions SL. at this
    address: info@irenesolutions.com
 */

using System.Collections.Generic;
using System.IO;
using System.Xml;
using VeriFactu.Net;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu.Nif;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.NIF
{

    /// <summary>
    /// Valida si un NIF está identificado en la AEAT.
    /// </summary>
    public class NifValidation
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Url del web service de validación de NIF de la AEAT.
        /// </summary>
        static string Url = "https://www1.agenciatributaria.gob.es/wlpl/BURT-JDIT/ws/VNifV2SOAP";

        /// <summary>
        /// Action del web service de validación de NIF de la AEAT.
        /// </summary>
        static string Action = "https://www1.agenciatributaria.gob.es/wlpl/BURT-JDIT/ws/VNifV2SOAP?op=VNifV2";

        /// <summary>
        /// Nombre.
        /// </summary>
        string _Name;

        /// <summary>
        /// NIF
        /// </summary>
        string _Nif;

        /// <summary>
        /// Sobre SOAP con la petición
        /// </summary>
        Envelope _Envelope;

        /// <summary>
        /// Binario del xml de la petición.
        /// </summary>
        byte[] _Xml;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nif">NIF</param>
        /// <param name="name">Nombre</param>
        public NifValidation(string nif, string name)
        {

            _Nif = nif;
            _Name = name;
            _Envelope = GetEnvelope(nif, name);
            _Xml = new XmlParser().GetBytes(_Envelope, Namespaces.NifItems);

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Obtiene sobre SOAP de la petición.
        /// </summary>
        /// <param name="nif">NIF</param>
        /// <param name="name">Nombre</param>
        /// <returns>Sobre SOAP de la petición.</returns>
        private Envelope GetEnvelope(string nif, string name)
        {

            Envelope envelope = new Envelope();

            envelope.Body.Registro = new VNifVEnt()
            {
                Contribuyente = new List<Contribuyente>()
                {
                    new Contribuyente {Nif = nif, Nombre = name }
                }
            };

            envelope.Header = null;

            return envelope;

        }

        /// <summary>
        /// Obtiene sobre SOAP de la respuesta.
        /// </summary>
        /// <returns>Sobre SOAP de la respuesta.</returns>
        private Envelope GetResponse()
        {

            XmlDocument xmlDocument = new XmlDocument();

            using (var msXml = new MemoryStream(_Xml))
                xmlDocument.Load(msXml);


            var response = Wsd.Call(Url, Action, xmlDocument);

            return Envelope.FromXml(response);

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Ejecuta las validaciones del obejeto de negocio.
        /// </summary>
        public virtual List<string> GetErrors()
        {

            var result = new List<string>();

            var responseEnvelope = GetResponse();

            if (responseEnvelope.Body.Contribuyentes[0].Resultado != "IDENTIFICADO")
                result.Add($"Error en la validación del NIF {_Nif} de {_Name}. Si el NIF es" +
                    $" de una persona física es necesario que conste también el nombre" +
                    $" correcto para poderlo validar.");

            return result;

        }

        #endregion

    }

}
