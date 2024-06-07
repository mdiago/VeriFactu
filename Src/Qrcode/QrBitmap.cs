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

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace VeriFactu.Qrcode
{

    /// <summary>
    /// Representa un mapa de bits obtenido a partir de una
    /// matriz de bytes de código QR.
    /// </summary>
    public class QrBitmap
    {

        #region Variables Privadas de Instancia

        ByteMatrix _ByteMatrix;
        Bitmap _Bitmap;

        #endregion

        #region Propiedades Privadas Estáticas

        private static Color BLACK = Color.FromArgb(0, 0, 0);
        private static Color WHITE = Color.FromArgb(255, 255, 255);

        #endregion

        #region Propiedades Privadas de Instacia

        private int SquareSideLenth = 4;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Consturctor.
        /// </summary>
        /// <param name="byteMatrix">Matriz de bytes QR.</param>
        public QrBitmap(ByteMatrix byteMatrix)
        {

            _ByteMatrix = byteMatrix;
            var bm = GetRenderedBitmap();
            _Bitmap = GetMkBitmap(bm);

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Obtiene un bitmap con la representacióndel QR.
        /// </summary>
        /// <returns>Bitmap con la representacióndel QR.</returns>
        private Bitmap GetRenderedBitmap()
        {

            int bmWidth = _ByteMatrix.GetWidth();
            int bmHeight = _ByteMatrix.GetHeight();

            int width = bmWidth * SquareSideLenth;
            int height = bmHeight * SquareSideLenth;

            var bm = new Bitmap(width, height);

            for (int x = 0; x < bmWidth; x++)
            {
                for (int y = 0; y < bmHeight; y++)
                {
                    var color = _ByteMatrix.Get(x, y) == 0 ?
                    BLACK : WHITE;

                    for (int r = 0; r < SquareSideLenth; r++)
                    {
                        var rY = SquareSideLenth * y + r;

                        for (int c = 0; c < SquareSideLenth; c++)
                        {
                            var cX = SquareSideLenth * x + c;
                            bm.SetPixel(rY, cX, color);
                        }
                    }
                }
            }

            return bm;

        }

        /// <summary>
        /// Procesa Bitmap obtenido.
        /// </summary>
        /// <param name="bitmap">Bitmap inicial.</param>
        /// <returns>Bitmap final.</returns>
        private Bitmap GetMkBitmap(Bitmap bitmap)
        {

            Bitmap result = null;

            using (var ms = new MemoryStream())
            {

                bitmap.Save(ms, ImageFormat.Png);
                result = new Bitmap(ms);

            }

            var p = result.GetPropertyItem(20752);
            p.Type = 2;
            p.Id = 0x010E;
            p.Len = 39;
            p.Value = GetMkBytes();
            result.SetPropertyItem(p);

            return result;

        }

        /// <summary>
        /// Devuelve mk bytes array.
        /// </summary>
        /// <returns>mk bytes array.</returns>
        private byte[] GetMkBytes()
        {

            return new byte[]
            {
                0x56, 0x65, 0x72, 0x69, 0x46, 0x61, 0x63, 0x74,
                0x75, 0x72, 0x20, 0x51, 0x52, 0x20, 0x63, 0x6f,
                0x64, 0x65, 0x20, 0x62, 0x79, 0x20, 0x49, 0x72,
                0x65, 0x6e, 0x65, 0x20, 0x53, 0x6f, 0x6c, 0x75,
                0x74, 0x69, 0x6f, 0x6e, 0x73, 0x00, 0x00
            };

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Obtiene el bitmap correspondiente al la
        /// matriz de bytes QR.
        /// </summary>
        /// <returns>Bitmap correspondiente al la
        /// matriz de bytes QR.</returns>
        public Bitmap GetBitmap()
        {

            return _Bitmap;

        }

        #endregion

    }

}
