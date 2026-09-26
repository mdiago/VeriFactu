/*
    This file is part of the VeriFactu (R) project.
    Copyright (c) 2024-2026 Irene Solutions SL
    Author: Irene Solutions SL.

    NO VERI*FACTU implementation developed with the valuable contribution of:
    Javier Florit González
    GAMADI CONSULTING BALEARS SL (B57336786)
    javier.florit@gamadic.com

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
    serving VeriFactu XML data on the fly in a web application, shipping VeriFactu 
    with a closed source product. 
     
    For more information, please contact Irene Solutions SL. at this 
    address: info@irenesolutions.com 
 */

using System.IO;
using VeriFactu.Config;
using VeriFactu.Xml.Factu.Evento;

/// <summary>
/// Datos para el almacenamiento de un evento SIF.
/// </summary>
internal class EventData
{

    #region Propiedades Privadas de Instacia

    /// <summary>
    /// Identificador del emisor.
    /// </summary>
    internal string SellerID { get; private set; }

    /// <summary>
    /// Evento fuente.
    /// </summary>
    internal Evento Evento { get; private set; }

    /// <summary>
    /// Ruta de la carpeta donde se almacenan los eventos
    /// del emisor.
    /// </summary>
    internal string EventPath => GetEventPath();

    /// <summary>
    /// Ruta de la carpeta donde se almacenan los eventos
    /// correspondientes al año del evento.
    /// </summary>
    internal string EventYearPath => GetEventYearPath();

    /// <summary>
    /// Ruta del archivo XML del evento.
    /// </summary>
    internal string EventFilePath =>
        Path.Combine(
            EventYearPath,
            $"{Evento.ExternKey}.xml");

    #endregion

    #region Constructores de Instancia

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="sellerID"> Identificador del emisor.</param>
    /// <param name="evento"> Evento fuente.</param>
    internal EventData(string sellerID, Evento evento)
    {

        SellerID = sellerID;
        Evento = evento;

    }

    #endregion

    #region Métodos Privados de Instancia

    /// <summary>
    /// Devuelve el path de un directorio.
    /// Si no existe lo crea.
    /// </summary>
    /// <param name="dir">Ruta al directorio.</param>
    /// <returns>Ruta al directorio.</returns>
    private string GetDirPath(string dir)
    {

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        return dir;

    }

    /// <summary>
    /// Devuelve la ruta de almacenamiento de eventos
    /// para un emisor en concreto.
    /// </summary>
    /// <returns>Ruta de almacenamiento de eventos.</returns>
    private string GetEventPath()
    {

        return GetDirPath(
            Path.Combine(
                Settings.Current.EventPath,
                SellerID));

    }

    /// <summary>
    /// Devuelve la ruta de almacenamiento de eventos
    /// para el año del evento.
    /// </summary>
    /// <returns>Ruta de almacenamiento de eventos
    /// para el año correspondiente.</returns>
    private string GetEventYearPath()
    {

        return GetDirPath(
            Path.Combine(
                EventPath,
                Evento.FechaHoraHusoGenEvento.Substring(0, 4)));

    }

    #endregion

}