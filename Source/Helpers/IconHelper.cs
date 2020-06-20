using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TrayToolkit.Helpers
{
    /// <summary>
    /// Inspired by: https://stackoverflow.com/questions/3213999/how-to-create-an-icon-file-that-contains-multiple-sizes-images-in-c-sharp
    /// </summary>
    public static class IconHelper
    {
        private const ushort HeaderReserved = 0;
        private const ushort HeaderIconType = 1;
        private const byte HeaderLength = 6;

        private const byte EntryReserved = 0;
        private const byte EntryLength = 16;

        private const byte PngColorsInPalette = 0;
        private const ushort PngColorPlanes = 1;


        /// <summary>
        /// Converts the image into an icon containg the default list of sizes
        /// </summary>

        public static Icon GetIcon(Bitmap img)
        {
            return GetIcon(img, 16, 24, 32, 48, 64);
        }


        /// <summary>
        /// Converts the image into an icon containg the given list of sizes
        /// </summary>
        public static Icon GetIcon(Bitmap img, params int[] sizes)
        {
            return new Icon(GetIconStream(img, sizes));
        }


        /// <summary>
        ///  Converts the image into an icon containg the given list of sizes
        /// </summary>
        public static Stream GetIconStream(Bitmap img, int[] sizes)
        {
            var iconStream = new MemoryStream();
            var writer = new BinaryWriter(iconStream);

            // write the header
            writer.Write(IconHelper.HeaderReserved);
            writer.Write(IconHelper.HeaderIconType);
            writer.Write((ushort)sizes.Length);

            // save the image buffers and offsets
            var buffers = new Dictionary<uint, byte[]>();

            // tracks the length of the buffers as the iterations occur
            // and adds that to the offset of the entries
            uint lengthSum = 0;
            uint baseOffset = (uint)(IconHelper.HeaderLength + IconHelper.EntryLength * sizes.Length);

            foreach (var size in sizes)
            {
                // creates a byte array from an image
                var data = getResizedBytes(img, size, size);
                var offset = baseOffset + lengthSum;

                // writes the image entry
                writer.Write((byte)size);
                writer.Write((byte)size);
                writer.Write(IconHelper.PngColorsInPalette);
                writer.Write(IconHelper.EntryReserved);
                writer.Write(IconHelper.PngColorPlanes);
                writer.Write((ushort)Image.GetPixelFormatSize(img.PixelFormat));
                writer.Write((uint)data.Length);
                writer.Write((uint)offset);

                lengthSum += (uint)data.Length;

                // adds the buffer to be written at the offset
                buffers.Add(offset, data);
            }

            // writes the buffers for each image
            foreach (var b in buffers)
            {
                writer.BaseStream.Seek(b.Key, SeekOrigin.Begin);
                writer.Write(b.Value);
            }

            iconStream.Seek(0, SeekOrigin.Begin);
            return iconStream;
        }


        /// <summary>
        /// Resizes the image and returns the byte array
        /// </summary>
        private static byte[] getResizedBytes(Bitmap srcBmp, int width, int height)
        {
            using (var b = new Bitmap(width, height))
            using (var g = Graphics.FromImage(b))
            using (var s = new MemoryStream())
            {
                g.SetHighQuality();
                g.DrawImage(srcBmp, 0, 0, width, height);
                b.Save(s, ImageFormat.Png);
                return s.ToArray();
            }
        }
    }
}
