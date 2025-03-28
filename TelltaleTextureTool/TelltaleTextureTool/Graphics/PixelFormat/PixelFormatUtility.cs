using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelltaleTextureTool.Graphics;

public static class PixelFormatUtility
{
    public static uint GetBitsPerPixel(PixelFormat pixelFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.R32G32B32A32 => 128,
            PixelFormat.R32G32B32 => 96,

            PixelFormat.R32G32 or PixelFormat.R16G16B16A16 => 64,

            PixelFormat.R16G16B16 => 48,

            PixelFormat.R32
            or PixelFormat.R16G16
            or PixelFormat.R8G8B8A8
            or PixelFormat.B8G8R8A8
            or PixelFormat.B8G8R8X8
            or PixelFormat.R9G9B9E5
            or PixelFormat.R10G10B10A2
            or PixelFormat.R11G11B10 => 32,

            PixelFormat.R8G8B8 or PixelFormat.B8G8R8 => 24,

            PixelFormat.R16
            or PixelFormat.R8G8
            or PixelFormat.B5G5R5A1
            or PixelFormat.B5G5R5X1
            or PixelFormat.B5G6R5
            or PixelFormat.B4G4R4A4 => 16,

            PixelFormat.R8
            or PixelFormat.A8
            or PixelFormat.BC2
            or PixelFormat.BC3
            or PixelFormat.BC5
            or PixelFormat.BC6H
            or PixelFormat.BC7 => 8,

            PixelFormat.BC1 or PixelFormat.BC4 => 4,

            PixelFormat.R1 => 1,

            PixelFormat.Unknown => throw new System.NotImplementedException(),
            PixelFormat.A4B4G4R4 => 16,
            PixelFormat.L16A16 => throw new System.NotImplementedException(),
            PixelFormat.D16 => throw new System.NotImplementedException(),
            PixelFormat.CTX1 => throw new System.NotImplementedException(),
            PixelFormat.PVRTC1_2BPP_RGB => 2,
            PixelFormat.PVRTC1_4BPP_RGB => 4,
            PixelFormat.PVRTC1_2BPP_RGBA => 2,
            PixelFormat.PVRTC1_4BPP_RGBA => 4,
            PixelFormat.ATC_RGB => throw new System.NotImplementedException(),
            PixelFormat.ATC_RGBA_EXPLICIT_ALPHA => throw new System.NotImplementedException(),
            PixelFormat.ATC_RGBA_INTERPOLATED_ALPHA => throw new System.NotImplementedException(),
            PixelFormat.ETC1 => throw new System.NotImplementedException(),
            PixelFormat.ETC2_RGB => throw new System.NotImplementedException(),
            PixelFormat.ETC2_RGBA => throw new System.NotImplementedException(),
            PixelFormat.ETC2_RGB_A1 => throw new System.NotImplementedException(),
            PixelFormat.ETC2_R11 => throw new System.NotImplementedException(),
            PixelFormat.ETC2_RG11 => throw new System.NotImplementedException(),
            PixelFormat.EAC_R11 => throw new System.NotImplementedException(),
            PixelFormat.EAC_RG11 => throw new System.NotImplementedException(),
            PixelFormat.ASTC_4x4 => throw new System.NotImplementedException(),
            _ => throw new NotImplementedException(),
        };
    }

    public static uint GetBytesPerBlock(PixelFormat pixelFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.BC1
            or PixelFormat.BC4
            or PixelFormat.ETC1
            or PixelFormat.ETC2_RGB
            or PixelFormat.ETC2_R11
            or PixelFormat.EAC_R11
            or PixelFormat.ASTC_4x4 => 8,
            PixelFormat.BC2
            or PixelFormat.BC3
            or PixelFormat.BC5
            or PixelFormat.BC6H
            or PixelFormat.BC7
            or PixelFormat.ETC2_RGBA
            or PixelFormat.ETC2_RGB_A1
            or PixelFormat.ETC2_RG11
            or PixelFormat.EAC_RG11 => 16,
            PixelFormat.CTX1 => 8,
            PixelFormat.PVRTC1_2BPP_RGB or PixelFormat.PVRTC1_2BPP_RGBA => 8,
            PixelFormat.PVRTC1_4BPP_RGB or PixelFormat.PVRTC1_4BPP_RGBA => 16,
            PixelFormat.ATC_RGB => 8,
            PixelFormat.ATC_RGBA_EXPLICIT_ALPHA or PixelFormat.ATC_RGBA_INTERPOLATED_ALPHA => 16,
            _ => GetBitsPerPixel(pixelFormat) / 8,
        };
    }

    public static uint GetBlockWidth(PixelFormat pixelFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.BC1
            or PixelFormat.BC4
            or PixelFormat.ETC1
            or PixelFormat.ETC2_RGB
            or PixelFormat.ETC2_R11
            or PixelFormat.EAC_R11
            or PixelFormat.ASTC_4x4 => 4,
            PixelFormat.BC2
            or PixelFormat.BC3
            or PixelFormat.BC5
            or PixelFormat.BC6H
            or PixelFormat.BC7
            or PixelFormat.ETC2_RGBA
            or PixelFormat.ETC2_RGB_A1
            or PixelFormat.ETC2_RG11
            or PixelFormat.EAC_RG11 => 4,
            PixelFormat.CTX1 => 4,
            PixelFormat.PVRTC1_2BPP_RGB or PixelFormat.PVRTC1_2BPP_RGBA => 8,
            PixelFormat.PVRTC1_4BPP_RGB or PixelFormat.PVRTC1_4BPP_RGBA => 4,
            PixelFormat.ATC_RGB => 4,
            PixelFormat.ATC_RGBA_EXPLICIT_ALPHA or PixelFormat.ATC_RGBA_INTERPOLATED_ALPHA => 4,
            _ => 1,
        };
    }

    public static bool IsFormatCompressed(PixelFormat pixelFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.BC1
            or PixelFormat.BC2
            or PixelFormat.BC3
            or PixelFormat.BC4
            or PixelFormat.BC5
            or PixelFormat.BC6H
            or PixelFormat.BC7
            or PixelFormat.CTX1
            or PixelFormat.PVRTC1_2BPP_RGB
            or PixelFormat.PVRTC1_4BPP_RGB
            or PixelFormat.PVRTC1_2BPP_RGBA
            or PixelFormat.PVRTC1_4BPP_RGBA
            or PixelFormat.ATC_RGB
            or PixelFormat.ATC_RGBA_EXPLICIT_ALPHA
            or PixelFormat.ATC_RGBA_INTERPOLATED_ALPHA
            or PixelFormat.ETC1
            or PixelFormat.ETC2_RGB
            or PixelFormat.ETC2_RGBA
            or PixelFormat.ETC2_RGB_A1
            or PixelFormat.ETC2_R11
            or PixelFormat.ETC2_RG11
            or PixelFormat.EAC_R11
            or PixelFormat.EAC_RG11
            or PixelFormat.ASTC_4x4 => true,
            _ => false,
        };
    }

    public static (uint rowPitch, uint slicePitch) ComputePitch(
        PixelFormat pixelFormat,
        uint width,
        uint height
    )
    {
        long slice;
        long pitch;
        if (IsFormatCompressed(pixelFormat))
        {
            // TODO: I need to add PVRTC
            uint blockWidth = Math.Max(1, (width + 3) / 4);
            uint blockHeight = Math.Max(1, (height + 3) / 4);
            uint blockBytes = GetBytesPerBlock(pixelFormat);

            pitch = blockWidth * blockBytes;
            slice = pitch * blockHeight;
        }
        else
        {
            pitch = (width * GetBitsPerPixel(pixelFormat) + 7) / 8;
            slice = pitch * height;
        }

        return ((uint)pitch, (uint)slice);
    }

    public static bool IsSRGB(PixelFormatInfo pixelFormatInfo)
    {
        return pixelFormatInfo.ColorSpace == ColorSpace.sRGB;
    }
}
