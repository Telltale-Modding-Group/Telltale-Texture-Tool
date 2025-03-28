using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;

namespace TelltaleTextureTool.Graphics.PVR
{
    internal class ATC_Master
    {
        public static Image Decode(Image image)
        {
            var decoder = new BcDecoder();

            var rgbaPixels = decoder.DecodeRaw(
                image.Pixels,
                (int)image.Width,
                (int)image.Height,
                GetCompressionFormat(image.PixelFormatInfo)
            );

            var pixels = new byte[rgbaPixels.Length * 4];

            for (int i = 0, j = 0; i < rgbaPixels.Length; i++, j += 4)
            {
                pixels[j] = rgbaPixels[i].r;
                pixels[j + 1] = rgbaPixels[i].g;
                pixels[j + 2] = rgbaPixels[i].b;
                pixels[j + 3] = rgbaPixels[i].a;
            }

            var newPixelFormatInfo = PixelFormats.R8G8B8A8_Unorm_Linear;

            var (rowPitch, slicePitch) = PixelFormatUtility.ComputePitch(
                newPixelFormatInfo.PixelFormat,
                image.Width,
                image.Height
            );

            return new Image
            {
                Width = image.Width,
                Height = image.Height,
                RowPitch = rowPitch,
                SlicePitch = slicePitch,
                PixelFormatInfo = newPixelFormatInfo,
                Pixels = pixels,
            };
        }

        public static CompressionFormat GetCompressionFormat(PixelFormatInfo pixelFormatInfo)
        {
            // Need to add signed/unsigned
            return pixelFormatInfo.PixelFormat switch
            {
                PixelFormat.ATC_RGB => CompressionFormat.Atc,
                PixelFormat.ATC_RGBA_EXPLICIT_ALPHA => CompressionFormat.AtcExplicitAlpha,
                PixelFormat.ATC_RGBA_INTERPOLATED_ALPHA => CompressionFormat.AtcInterpolatedAlpha,
                PixelFormat.BC1 => CompressionFormat.Bc1,
                PixelFormat.BC2 => CompressionFormat.Bc2,
                PixelFormat.BC3 => CompressionFormat.Bc3,
                PixelFormat.BC4 => CompressionFormat.Bc4,
                PixelFormat.BC5 => CompressionFormat.Bc5,
                PixelFormat.BC6H => CompressionFormat.Bc6U,
                PixelFormat.BC7 => CompressionFormat.Bc7,
                _ => throw new InvalidDataException("Invalid pixel format"),
            };
        }
    }
}
