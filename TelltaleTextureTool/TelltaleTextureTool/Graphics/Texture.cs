using System;
using TelltaleTextureTool.Codecs;
using TelltaleTextureTool.TelltaleEnums;

namespace TelltaleTextureTool.Graphics;

public partial class Texture
{
    public TexMetadata Metadata { get; set; } = new();
    public Image[] Images { get; set; } = [];
}

public class TexMetadata
{
    public uint Width { get; set; }
    public uint Height { get; set; }
    public uint Depth { get; set; }
    public uint ArraySize { get; set; }
    public uint MipLevels { get; set; }
    public PixelFormatInfo PixelFormatInfo { get; set; } = new();
    public TexDimension Dimension { get; set; }
    public bool IsPremultipliedAlpha { get; set; }
    public bool IsCubemap { get; set; }
    public bool IsVolumemap { get; set; }
    public ExtraMetadata ExtraMetadata { get; set; } = new();
}

public enum TexDimension
{
    Tex1D,
    Tex2D,
    Tex3D,
}

public class Image : ICloneable
{
    public uint Width { get; set; }
    public uint Height { get; set; }
    public required PixelFormatInfo PixelFormatInfo { get; set; }
    public uint RowPitch { get; set; }
    public uint SlicePitch { get; set; }
    public required byte[] Pixels { get; set; }

    public object Clone()
    {
        var clonedImage = new Image
        {
            Width = Width,
            Height = Height,
            PixelFormatInfo = new PixelFormatInfo(
                PixelFormatInfo.PixelFormat,
                PixelFormatInfo.DataType,
                PixelFormatInfo.ColorSpace
            ),
            RowPitch = RowPitch,
            SlicePitch = SlicePitch,
            Pixels = new byte[Pixels.Length],
        };
        Array.Copy(this.Pixels, Pixels, this.Pixels.Length);

        return clonedImage;
    }

    public Image() { }

    public static Image GetImage(
        uint width,
        uint height,
        PixelFormatInfo pixelFormat,
        byte[] pixels
    )
    {
        var (rowPitch, slicePitch) = PixelFormatUtility.ComputePitch(
            pixelFormat.PixelFormat,
            width,
            height
        );

        return new Image
        {
            Width = width,
            Height = height,
            PixelFormatInfo = pixelFormat,
            Pixels = pixels,
            RowPitch = rowPitch,
            SlicePitch = slicePitch,
        };
    }

    public static Image GetRGBA8Image(uint width, uint height, byte[] pixels)
    {
        return GetImage(width, height, PixelFormats.R8G8B8A8_Unorm_Linear, pixels);
    }

    public static Image GetRGBA32FImage(uint width, uint height, byte[] pixels)
    {
        return GetImage(width, height, PixelFormats.R32G32B32A32_Float_Linear, pixels);
    }
}

public partial class ExtraMetadata
{
    public TelltaleToolGame Game { get; set; }
    public bool IsLegacyConsole { get; set; }
    public Platform UnswizzleMode { get; set; }
    public Platform SwizzleMode { get; set; }
    public string DebugInformation { get; set; } = string.Empty;
}

public partial class ExtraMetadata { }

public enum TextureType
{
    Unknown,
    D3DTX,
    DDS,
    KTX,
    KTX2,
    PNG,
    JPEG,
    BMP,
    TIFF,
    TGA,
    HDR,
}
