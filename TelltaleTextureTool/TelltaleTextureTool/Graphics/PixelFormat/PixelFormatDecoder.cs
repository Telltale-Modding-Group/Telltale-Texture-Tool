using System;
using TelltaleTextureTool.Graphics.Plugins;
using TelltaleTextureTool.Graphics.PVR;

namespace TelltaleTextureTool.Graphics;

public class PixelFormatDecoder
{
    public static Image DecodeImageToRGBA8(Image image)
    {
        Image newImage = image.PixelFormatInfo.PixelFormat switch
        {
            PixelFormat.R8G8B8 => ConvertRgbToRgba(image),

            PixelFormat.B8G8R8 => ConvertBgrToRgba(image),

            PixelFormat.R1
            or PixelFormat.A8
            or PixelFormat.R8
            or PixelFormat.R16
            or PixelFormat.R8G8
            or PixelFormat.R16G16
            or PixelFormat.R9G9B9E5
            or PixelFormat.B4G4R4A4
            or PixelFormat.A4B4G4R4
            or PixelFormat.B5G6R5
            or PixelFormat.R11G11B10
            or PixelFormat.R10G10B10A2
            or PixelFormat.B5G5R5A1
            or PixelFormat.B5G5R5X1
            or PixelFormat.R8G8B8A8
            or PixelFormat.B8G8R8X8
            or PixelFormat.B8G8R8A8
            or PixelFormat.R32
            or PixelFormat.R32G32
            or PixelFormat.L16A16
            or PixelFormat.R16G16B16
            or PixelFormat.R16G16B16A16
            or PixelFormat.R32G32B32
            or PixelFormat.R32G32B32A32 => DirectXTexUtility.ConvertImage(
                PixelFormats.R8G8B8A8_Unorm_Linear,
                image
            ),

            PixelFormat.BC1
            or PixelFormat.BC2
            or PixelFormat.BC3
            or PixelFormat.BC4
            or PixelFormat.BC5
            or PixelFormat.BC6H
            or PixelFormat.BC7 => DirectXTexUtility.DecompressImage(image, PixelFormats.Unknown),

            PixelFormat.ATC_RGB // BCn
            or PixelFormat.ATC_RGBA_EXPLICIT_ALPHA
            or PixelFormat.ATC_RGBA_INTERPOLATED_ALPHA => ATC_Master.Decode(image), //  BCn

            PixelFormat.PVRTC1_2BPP_RGB
            or PixelFormat.PVRTC1_4BPP_RGB
            or PixelFormat.PVRTC1_2BPP_RGBA
            or PixelFormat.PVRTC1_4BPP_RGBA
            or PixelFormat.ETC1
            or PixelFormat.ETC2_RGB
            or PixelFormat.ETC2_RGBA
            or PixelFormat.ETC2_RGB_A1
            or PixelFormat.ETC2_R11
            or PixelFormat.ETC2_RG11
            or PixelFormat.EAC_R11
            or PixelFormat.EAC_RG11
            or PixelFormat.ASTC_4x4 => PVR_Main.DecodeTexture(image), // PVRTC

            PixelFormat.B10G10R10A2 => throw new NotImplementedException(), // REQUIRES SWIZZLING
            PixelFormat.D16 => throw new NotImplementedException(), // Depth (ignore)
            PixelFormat.D24S8 => throw new NotImplementedException(), // Depth (ignore)
            PixelFormat.D24X8 => throw new NotImplementedException(), // Depth (ignore)
            PixelFormat.D32 => throw new NotImplementedException(), // Depth (ignore)
            PixelFormat.D32S8 => throw new NotImplementedException(), // Depth (ignore)
            PixelFormat.CTX1 => throw new NotImplementedException(), // Custom Decoder

            _ => throw new NotSupportedException("Unsupported pixel format"),
        };

        if (!image.PixelFormatInfo.Equals(PixelFormats.R8G8B8A8_Unorm_Linear))
        {
            return DecodeImageToRGBA8(newImage);
        }

        return newImage;
    }

    public static Image DecodeImageToRGBA32F(Image image)
    {
        Image newImage = image.PixelFormatInfo.PixelFormat switch
        {
            PixelFormat.R1
            or PixelFormat.A8
            or PixelFormat.R8
            or PixelFormat.R16
            or PixelFormat.R8G8
            or PixelFormat.R16G16
            or PixelFormat.R9G9B9E5
            or PixelFormat.B4G4R4A4
            or PixelFormat.A4B4G4R4
            or PixelFormat.B5G6R5
            or PixelFormat.R11G11B10
            or PixelFormat.R10G10B10A2
            or PixelFormat.B5G5R5A1
            or PixelFormat.B5G5R5X1
            or PixelFormat.R8G8B8A8
            or PixelFormat.B8G8R8X8
            or PixelFormat.B8G8R8A8
            or PixelFormat.R32
            or PixelFormat.R32G32
            or PixelFormat.L16A16
            or PixelFormat.R16G16B16
            or PixelFormat.R16G16B16A16
            or PixelFormat.R32G32B32
            or PixelFormat.R32G32B32A32 => DirectXTexUtility.ConvertImage(
                PixelFormats.R32G32B32A32_Float_Linear,
                image
            ),

            PixelFormat.BC1
            or PixelFormat.BC2
            or PixelFormat.BC3
            or PixelFormat.BC4
            or PixelFormat.BC5
            or PixelFormat.BC6H
            or PixelFormat.BC7 => DirectXTexUtility.DecompressImage(
                image,
                PixelFormats.R32G32B32A32_Float_Linear
            ),

            PixelFormat.R8G8B8
            or PixelFormat.B8G8R8
            or PixelFormat.ATC_RGB
            or PixelFormat.ATC_RGBA_EXPLICIT_ALPHA
            or PixelFormat.ATC_RGBA_INTERPOLATED_ALPHA
            or PixelFormat.PVRTC1_2BPP_RGB
            or PixelFormat.PVRTC1_4BPP_RGB
            or PixelFormat.PVRTC1_2BPP_RGBA
            or PixelFormat.PVRTC1_4BPP_RGBA
            or PixelFormat.ETC1
            or PixelFormat.ETC2_RGB
            or PixelFormat.ETC2_RGBA
            or PixelFormat.ETC2_RGB_A1
            or PixelFormat.ETC2_R11
            or PixelFormat.ETC2_RG11
            or PixelFormat.EAC_R11
            or PixelFormat.EAC_RG11
            or PixelFormat.ASTC_4x4 => DirectXTexUtility.ConvertImage(
                PixelFormats.R32G32B32A32_Float_Linear,
                DecodeImageToRGBA8(image)
            ),

            PixelFormat.B10G10R10A2 => throw new NotImplementedException(), // REQUIRES SWIZZLING
            PixelFormat.D16 => throw new NotImplementedException(), // Depth (ignore)
            PixelFormat.D24S8 => throw new NotImplementedException(), // Depth (ignore)
            PixelFormat.D24X8 => throw new NotImplementedException(), // Depth (ignore)
            PixelFormat.D32 => throw new NotImplementedException(), // Depth (ignore)
            PixelFormat.D32S8 => throw new NotImplementedException(), // Depth (ignore)
            PixelFormat.CTX1 => throw new NotImplementedException(), // Custom Decoder

            _ => throw new NotSupportedException("Unsupported pixel format"),
        };

        // TODO: Implement this

        return newImage;
    }

    private static Image ConvertRgbToRgba(Image image)
    {
        var pixels = image.Pixels;

        var output = new byte[pixels.Length / 3 * 4];
        for (int i = 0, j = 0; i < pixels.Length; i += 3, j += 4)
        {
            output[j] = pixels[i];
            output[j + 1] = pixels[i + 1];
            output[j + 2] = pixels[i + 2];
            output[j + 3] = 255; // Set alpha channel to 255
        }

        return Image.GetRGBA8Image(image.Width, image.Height, output);
    }

    private static Image ConvertBgrToRgba(Image image)
    {
        var pixels = image.Pixels;

        var output = new byte[pixels.Length / 3 * 4];
        for (int i = 0, j = 0; i < pixels.Length; i += 3, j += 4)
        {
            output[j] = pixels[i + 2]; // Red
            output[j + 1] = pixels[i + 1]; // Green
            output[j + 2] = pixels[i]; // Blue
            output[j + 3] = 255; // Set alpha channel to 255
        }

        return Image.GetRGBA8Image(image.Width, image.Height, output);
    }
}
