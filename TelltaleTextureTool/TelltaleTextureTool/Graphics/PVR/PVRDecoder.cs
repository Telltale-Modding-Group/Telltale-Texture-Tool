using System;
using System.Runtime.InteropServices;
using PVRTexLib;

namespace TelltaleTextureTool.Graphics.PVR;

internal class PVR_Main
{
    public static Image DecodeTexture(Image image)
    {
        var format = GetPVRFormat(image.PixelFormatInfo);
        ulong RGBA8888 = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 8, 8, 8, 8);
        uint width = image.Width;
        uint height = image.Height;
        uint depth = 1;
        uint numMipMaps = 1;
        uint numArrayMembers = 1;
        uint numFaces = 1;
        using PVRTextureHeader textureHeader = new(
            (ulong)format,
            width,
            height,
            depth,
            numMipMaps,
            numArrayMembers,
            numFaces
        );
        ulong textureSize = textureHeader.GetTextureDataSize();

        if (textureSize == 0)
        {
            throw new Exception("Could not create PVR header!");
        }

        unsafe
        {
            fixed (byte* ptr = &image.Pixels[0])
            {
                using PVRTexture texture = new(textureHeader, ptr);

                var colorSpace =
                    image.PixelFormatInfo.ColorSpace == ColorSpace.sRGB
                        ? PVRTexLibColourSpace.sRGB
                        : PVRTexLibColourSpace.Linear;
                texture.Transcode(RGBA8888, PVRTexLibVariableType.UnsignedByteNorm, colorSpace);

                byte[] pixels = new byte[texture.GetTextureDataSize()];

                try
                {
                    Marshal.Copy(
                        new IntPtr(texture.GetTextureDataPointer()),
                        pixels,
                        0,
                        (int)texture.GetTextureDataSize()
                    );

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
                finally
                {
                    texture.Dispose();
                }
            }
        }
    }

    public static PVRTexLibPixelFormat GetPVRFormat(PixelFormatInfo pixelFormatInfo)
    {
        return pixelFormatInfo.PixelFormat switch
        {
            PixelFormat.ETC1 => PVRTexLibPixelFormat.ETC1,
            PixelFormat.ETC2_RGB => PVRTexLibPixelFormat.ETC2_RGB,
            PixelFormat.ETC2_RGBA => PVRTexLibPixelFormat.ETC2_RGBA,
            PixelFormat.ETC2_RGB_A1 => PVRTexLibPixelFormat.ETC2_RGB_A1,
            PixelFormat.ETC2_R11 => PVRTexLibPixelFormat.EAC_R11,
            PixelFormat.ETC2_RG11 => PVRTexLibPixelFormat.EAC_RG11,
            PixelFormat.ASTC_4x4 => PVRTexLibPixelFormat.ASTC_4x4,
            PixelFormat.PVRTC1_2BPP_RGB => PVRTexLibPixelFormat.PVRTCI_2bpp_RGB,
            PixelFormat.PVRTC1_4BPP_RGB => PVRTexLibPixelFormat.PVRTCI_4bpp_RGB,
            PixelFormat.PVRTC1_2BPP_RGBA => PVRTexLibPixelFormat.PVRTCI_2bpp_RGBA,
            PixelFormat.PVRTC1_4BPP_RGBA => PVRTexLibPixelFormat.PVRTCI_4bpp_RGBA,
            _ => 0,
        };
    }
}
