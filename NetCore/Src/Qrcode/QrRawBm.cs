/*
    This file is part of the VeriFactu (R) project.
    Copyright (c) 2024-2025 Irene Solutions SL
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
    serving VeriFactu XML data on the fly in a web application, shipping VeriFactu
    with a closed source product.
    
    For more information, please contact Irene Solutions SL. at this
    address: info@irenesolutions.com
 */

using System;

namespace VeriFactu.Qrcode
{

    /// <summary>
    /// Representa un bitmap 8-bit RGB color.
    /// </summary>
    public class QrRawBm
    {

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Bytes de la imágen.
        /// </summary>
        private readonly byte[] ImageBytes;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="width">Ancho en pixels.</param>
        /// <param name="height">Alto en pixels.</param>
        public QrRawBm(int width, int height)
        {

            Width = width;
            Height = height;
            ImageBytes = new byte[width * height * 4];

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Alto en pixels
        /// </summary>
        public readonly int Width;
        
        /// <summary>
        /// Ancho en pixels.
        /// </summary>
        public readonly int Height;

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Establece el color de un pixel.
        /// </summary>
        /// <param name="x">Posición del pixel horizontal.</param>
        /// <param name="y">Posición del pixel vertical.</param>
        /// <param name="r">Valor del rojo.</param>
        /// <param name="g">Valor del verde.</param>
        /// <param name="b">Valor del azul.</param>
        public void SetPixel(int x, int y, byte r, byte g, byte b)
        {
            int offset = ((Height - y - 1) * Width + x) * 4;
            ImageBytes[offset + 0] = b;
            ImageBytes[offset + 1] = g;
            ImageBytes[offset + 2] = r;
        }

        /// <summary>
        /// Obtiene los bytes de la imágen.
        /// </summary>
        /// <returns>Bytes de la imágen.</returns>
        public byte[] GetBitmapBytes()
        {

            const int imageHeaderSize = 54;

            byte[] bmpBytes = new byte[imageHeaderSize + ImageBytes.Length];

            // A. BITMAP FILE HEADER

            // 1. BM
            bmpBytes[0] = (byte)'B';
            bmpBytes[1] = (byte)'M';
            // 2. The size of the BMP file in bytes
            Array.Copy(BitConverter.GetBytes(bmpBytes.Length), 0, bmpBytes, 2, 4);
            // 3. 6-8 Reserved
            // 4. 8-10 Reserved
            // 5. The offset, i.e. starting address, of the byte where the bitmap image data (pixel array) can be found.
            Array.Copy(BitConverter.GetBytes(imageHeaderSize), 0, bmpBytes, 10, 4);


            // B. BITMAPINFOHEADER

            // https://github.com/mdiago/VeriFactu/discussions/159
            var horizontalResolution = 3780;
            var verticalResolution = 3780;

            // 1. the size of this header, in bytes (40)
            bmpBytes[14] = 40;
            // 2. the bitmap width in pixels (signed integer)
            Array.Copy(BitConverter.GetBytes(Width), 0, bmpBytes, 18, 4);
            // 3. the bitmap height in pixels (signed integer)
            Array.Copy(BitConverter.GetBytes(Height), 0, bmpBytes, 22, 4);
            // 4. the number of color planes (must be 1)
            Array.Copy(BitConverter.GetBytes(1), 0, bmpBytes, 26, 2);
            // 5. the number of bits per pixel, which is the color depth of the image. Typical values are 1, 4, 8, 16, 24 and 32.
            Array.Copy(BitConverter.GetBytes(32), 0, bmpBytes, 28, 2);
            // 6.  the compression method being used. BI_RGB none = 0.
            Array.Copy(BitConverter.GetBytes(0), 0, bmpBytes, 30, 4);
            // 7. the image size. This is the size of the raw bitmap data; a dummy 0 can be given for BI_RGB bitmaps.
            Array.Copy(BitConverter.GetBytes(ImageBytes.Length), 0, bmpBytes, 34, 4);
            // 8. the image size. This is the size of the raw bitmap data; a dummy 0 can be given for BI_RGB bitmaps.
            Array.Copy(BitConverter.GetBytes(0), 0, bmpBytes, 34, 4);
            // 9. the horizontal resolution of the image. (pixel per metre, signed integer)
            Array.Copy(BitConverter.GetBytes(horizontalResolution), 0, bmpBytes, 38, 4);
            // 10. the vertical resolution of the image. (pixel per metre, signed integer)
            Array.Copy(BitConverter.GetBytes(verticalResolution), 0, bmpBytes, 42, 4);
            //11. the number of colors in the color palette, or 0 to default to 2n
            Array.Copy(BitConverter.GetBytes(0), 0, bmpBytes, 46, 4);
            // 12. the number of important colors used, or 0 when every color is important; generally ignored
            Array.Copy(BitConverter.GetBytes(0), 0, bmpBytes, 50, 4);

            // C. IMAGE BYTES

            Array.Copy(ImageBytes, 0, bmpBytes, imageHeaderSize, ImageBytes.Length);
            return bmpBytes;

        }

        #endregion

    }

}
