using System;
using System.Collections.Concurrent;

namespace TelltaleTextureTool.Graphics;

public class PixelFormatInfo(
    PixelFormat pixelFormat = PixelFormat.R8G8B8A8,
    DataType dataType = DataType.Unorm,
    ColorSpace colorSpace = ColorSpace.Linear
)
{
    public PixelFormat PixelFormat { get; set; } = pixelFormat;
    public DataType DataType { get; set; } = dataType;
    public ColorSpace ColorSpace { get; set; } = colorSpace;

    public override bool Equals(object? obj)
    {
        return obj is PixelFormatInfo other
            && PixelFormat == other.PixelFormat
            && DataType == other.DataType
            && ColorSpace == other.ColorSpace;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PixelFormat, DataType, ColorSpace);
    }
}

public static class PixelFormats
{
    public static readonly PixelFormatInfo Unknown = new(PixelFormat.Unknown);
    public static readonly PixelFormatInfo R8_Unorm_Linear = new(PixelFormat.R8);
    public static readonly PixelFormatInfo R8G8B8A8_Unorm_Linear = new(PixelFormat.R8G8B8A8);
    public static readonly PixelFormatInfo R8G8B8A8_Unorm_Srgb = new(
        PixelFormat.R8G8B8A8,
        DataType.Unorm,
        ColorSpace.sRGB
    );
    public static readonly PixelFormatInfo B8G8R8A8_Unorm_Linear = new(PixelFormat.B8G8R8A8);
    public static readonly PixelFormatInfo B8G8R8A8_Unorm_Srgb = new(
        PixelFormat.B8G8R8A8,
        DataType.Unorm,
        ColorSpace.sRGB
    );
    public static readonly PixelFormatInfo B8G8R8X8_Unorm_Linear = new(PixelFormat.B8G8R8X8);
    public static readonly PixelFormatInfo B8G8R8X8_Unorm_Srgb = new(
        PixelFormat.B8G8R8X8,
        DataType.Unorm,
        ColorSpace.sRGB
    );
    public static readonly PixelFormatInfo B5G5R5A1_Unorm_Linear = new(PixelFormat.B5G5R5A1);
    public static readonly PixelFormatInfo B5G6R5_Unorm_Linear = new(PixelFormat.B5G6R5);
    public static readonly PixelFormatInfo A8_Unorm_Linear = new(PixelFormat.A8);
    public static readonly PixelFormatInfo R32G32B32A32_Float_Linear = new(
        PixelFormat.R32G32B32A32,
        DataType.Float
    );
    public static readonly PixelFormatInfo R32G32B32_Float_Linear = new(
        PixelFormat.R32G32B32,
        DataType.Float
    );
    public static readonly PixelFormatInfo R16G16B16A16_Float_Linear = new(
        PixelFormat.R16G16B16A16,
        DataType.Float
    );
    public static readonly PixelFormatInfo R16G16B16A16_Unorm_Linear = new(
        PixelFormat.R16G16B16A16,
        DataType.Unorm
    );
}

public enum PixelFormat
{
    Unknown,
    R1,
    A8,
    R8, // L8
    R8G8, // L8A8
    B4G4R4A4,
    A4B4G4R4,
    R8G8B8,
    B8G8R8,
    R8G8B8A8,
    B8G8R8A8,
    B8G8R8X8,
    B5G6R5,
    B5G5R5A1,
    B5G5R5X1,
    R16, // L16
    R16G16,
    L16A16,
    R16G16B16,
    R16G16B16A16,
    R32,
    R32G32,
    R32G32B32,
    R32G32B32A32,
    R10G10B10A2,
    B10G10R10A2,
    R11G11B10,
    R9G9B9E5,
    D16,
    D24S8,
    D24X8,
    D32,
    D32S8,
    BC1, // DXT1
    BC2, // DXT2 (premultiplied) / DXT3
    BC3, // DXT4 (premultiplied) / DXT5
    BC4, // ATI1
    BC5, // ATI2
    BC6H,
    BC7,

    CTX1,
    PVRTC1_2BPP_RGB,
    PVRTC1_4BPP_RGB,
    PVRTC1_2BPP_RGBA,
    PVRTC1_4BPP_RGBA,
    ATC_RGB,
    ATC_RGBA_EXPLICIT_ALPHA,
    ATC_RGBA_INTERPOLATED_ALPHA,
    ETC1,
    ETC2_RGB,
    ETC2_RGBA,
    ETC2_RGB_A1,
    ETC2_R11,
    ETC2_RG11,
    EAC_R11,
    EAC_RG11,
    ASTC_4x4,
}

public enum DataType
{
    Unknown,
    Snorm,
    Unorm,
    Sint,
    Uint,
    Float,
}

public enum ColorSpace
{
    sRGB,
    Linear,
}
