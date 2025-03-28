using Hexa.NET.DirectXTex;
using TelltaleTextureTool.DirectX.Enums;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.TelltaleEnums;

namespace TelltaleTextureTool.DirectX;

// DDS Docs - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header
// DDS PIXEL FORMAT - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-pixelformat
// DDS DDS_HEADER_DXT10 - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header-dxt10
// DDS File Layout https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-file-layout-for-textures
// Texture Block Compression in D3D11 - https://docs.microsoft.com/en-us/windows/win32/direct3d11/texture-block-compression-in-direct3d-11
// DDS Programming Guide - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds-pguide
// D3DFormat - https://learn.microsoft.com/en-us/windows/win32/direct3d9/d3dformat
// Map Direct3D 9 Formats to Direct3D 10 - https://learn.microsoft.com/en-gb/windows/win32/direct3d10/d3d10-graphics-programming-guide-resources-legacy-formats?redirectedfrom=MSDN

/// <summary>
/// The class is used for decoding and encoding .dds headers.
/// </summary>
public static partial class DDSHelper
{
    /// <summary>
    /// Get the Telltale surface format from a DXGI format.
    /// This is used for the conversion process from .dds to .d3dtx.
    /// </summary>
    /// <param name="dxgiFormat">The Direct3D10/DXGI format.</param>
    /// <returns>The corresponding T3SurfaceFormat enum from the DXGI format.</returns>
    private static T3SurfaceFormat GetTelltaleSurfaceFormat(DXGIFormat dxgiFormat) =>
        dxgiFormat switch
        {
            DXGIFormat.B8G8R8A8_UNORM_SRGB => T3SurfaceFormat.ARGB8,
            DXGIFormat.B8G8R8A8_UNORM => T3SurfaceFormat.ARGB8,
            DXGIFormat.R16G16B16A16_UNORM => T3SurfaceFormat.ARGB16,
            DXGIFormat.B5G6R5_UNORM => T3SurfaceFormat.RGB565,
            DXGIFormat.B5G5R5A1_UNORM => T3SurfaceFormat.ARGB1555,
            DXGIFormat.B4G4R4A4_UNORM => T3SurfaceFormat.ARGB4,
            DXGIFormat.A4B4G4R4_UNORM => T3SurfaceFormat.ARGB4,
            DXGIFormat.R10G10B10A2_UNORM => T3SurfaceFormat.ARGB2101010,
            DXGIFormat.R16G16_UNORM => T3SurfaceFormat.RG16,
            DXGIFormat.R8G8B8A8_UNORM_SRGB => T3SurfaceFormat.RGBA8,
            DXGIFormat.R8G8B8A8_UNORM => T3SurfaceFormat.RGBA8,
            DXGIFormat.R32_UINT => T3SurfaceFormat.R32,
            DXGIFormat.R32G32_UINT => T3SurfaceFormat.RG32,
            DXGIFormat.R32G32B32A32_FLOAT => T3SurfaceFormat.RGBA32F,
            DXGIFormat.R8G8B8A8_SNORM => T3SurfaceFormat.RGBA8S,
            DXGIFormat.A8_UNORM => T3SurfaceFormat.A8,
            DXGIFormat.R8_UNORM => T3SurfaceFormat.L8,
            DXGIFormat.R8G8_UNORM => T3SurfaceFormat.AL8,
            DXGIFormat.R16_UNORM => T3SurfaceFormat.L16,
            DXGIFormat.R16G16_SNORM => T3SurfaceFormat.RG16S,
            DXGIFormat.R16G16B16A16_SNORM => T3SurfaceFormat.RGBA16S,
            DXGIFormat.R16G16B16A16_UINT => T3SurfaceFormat.R16UI,
            DXGIFormat.R16_FLOAT => T3SurfaceFormat.R16F,
            DXGIFormat.R16G16B16A16_FLOAT => T3SurfaceFormat.RGBA16F,
            DXGIFormat.R32_FLOAT => T3SurfaceFormat.R32F,
            DXGIFormat.R32G32_FLOAT => T3SurfaceFormat.RG32F,
            DXGIFormat.R32G32B32A32_UINT => T3SurfaceFormat.RGBA32,
            DXGIFormat.R11G11B10_FLOAT => T3SurfaceFormat.RGB111110F,
            DXGIFormat.R9G9B9E5_SHAREDEXP => T3SurfaceFormat.RGB9E5F,
            DXGIFormat.D16_UNORM => T3SurfaceFormat.DepthPCF16,
            DXGIFormat.D24_UNORM_S8_UINT => T3SurfaceFormat.DepthPCF24,
            DXGIFormat.D32_FLOAT_S8X24_UINT => T3SurfaceFormat.DepthStencil32,
            DXGIFormat.D32_FLOAT => T3SurfaceFormat.Depth32F,
            DXGIFormat.BC1_UNORM => T3SurfaceFormat.BC1,
            DXGIFormat.BC2_UNORM => T3SurfaceFormat.BC2,
            DXGIFormat.BC3_UNORM => T3SurfaceFormat.BC3,
            DXGIFormat.BC4_UNORM => T3SurfaceFormat.BC4,
            DXGIFormat.BC5_UNORM => T3SurfaceFormat.BC5,
            DXGIFormat.BC6H_UF16 => T3SurfaceFormat.BC6,
            DXGIFormat.BC6H_SF16 => T3SurfaceFormat.BC6,
            DXGIFormat.BC7_UNORM => T3SurfaceFormat.BC7,
            DXGIFormat.BC1_UNORM_SRGB => T3SurfaceFormat.BC1,
            DXGIFormat.BC2_UNORM_SRGB => T3SurfaceFormat.BC2,
            DXGIFormat.BC3_UNORM_SRGB => T3SurfaceFormat.BC3,
            DXGIFormat.BC7_UNORM_SRGB => T3SurfaceFormat.BC7,
            _ => T3SurfaceFormat.Unknown,
        };

    /// <summary>
    /// Get the Telltale surface format from a DXGI format and an already existing surface format.
    /// This is used for the conversion process from .dds to .d3dtx.
    /// This is used when equivalent formats are found.
    /// Some Telltale games have different values for the same formats, but they do not ship with all of them.
    /// This can create issues if the Telltale surface format is not found in the game.
    /// In any case, use Lucas's Telltale Inspector to change the value if any issues arise.
    /// </summary>
    /// <param name="dxgiFormat">The Direct3D10/DXGI format.</param>
    /// <param name="surfaceFormat">(Optional) The existing Telltale surface format. Default value is UNKNOWN.</param>
    /// <returns>The corresponding T3SurfaceFormat enum from the DXGI format and Telltale surface format.</returns>
    public static T3SurfaceFormat GetTelltaleSurfaceFormat(
        DXGIFormat dxgiFormat,
        T3SurfaceFormat surfaceFormat = T3SurfaceFormat.Unknown
    )
    {
        T3SurfaceFormat surfaceFormatFromDXGI = GetTelltaleSurfaceFormat(dxgiFormat);

        if (surfaceFormatFromDXGI == T3SurfaceFormat.Unknown)
        {
            return surfaceFormat;
        }

        if (surfaceFormatFromDXGI == T3SurfaceFormat.L16 && surfaceFormat == T3SurfaceFormat.R16)
        {
            return T3SurfaceFormat.L16;
        }
        else if (
            surfaceFormatFromDXGI == T3SurfaceFormat.AL8
            && surfaceFormat == T3SurfaceFormat.RG8
        )
        {
            return T3SurfaceFormat.RG8;
        }
        else if (surfaceFormatFromDXGI == T3SurfaceFormat.L8 && surfaceFormat == T3SurfaceFormat.R8)
        {
            return T3SurfaceFormat.R8;
        }
        else if (
            surfaceFormatFromDXGI == T3SurfaceFormat.ARGB16
            && surfaceFormat == T3SurfaceFormat.RGBA16
        )
        {
            return T3SurfaceFormat.RGBA16;
        }
        else if (
            surfaceFormatFromDXGI == T3SurfaceFormat.ARGB2101010
            && surfaceFormat == T3SurfaceFormat.RGBA1010102F
        )
        {
            return T3SurfaceFormat.RGBA1010102F;
        }
        else if (
            surfaceFormatFromDXGI == T3SurfaceFormat.R32F
            && surfaceFormat == T3SurfaceFormat.R32
        )
        {
            return T3SurfaceFormat.R32;
        }
        else if (
            surfaceFormatFromDXGI == T3SurfaceFormat.RG32F
            && surfaceFormat == T3SurfaceFormat.RG32
        )
        {
            return T3SurfaceFormat.R32;
        }

        return surfaceFormatFromDXGI;
    }

    /// <summary>
    /// Returns the corresponding Direct3D9 surface format from a Direct3D10/DXGI format.
    /// This is used for the conversion process from .d3dtx to .dds. (Legacy .d3dtx)
    /// </summary>
    /// <param name="dxgiFormat">The DXGI format.</param>
    /// <param name="metadata">The metadata of our .dds file. It is used in determining if the alpha is premultiplied.</param>
    /// <returns>The corresponding Direct3D9 format.</returns>
    public static LegacyFormat GetD3DFormat(
        DXGIFormat dxgiFormat,
        Hexa.NET.DirectXTex.TexMetadata metadata
    ) =>
        dxgiFormat switch
        {
            DXGIFormat.B8G8R8A8_UNORM => LegacyFormat.A8R8G8B8,
            DXGIFormat.B8G8R8X8_UNORM => LegacyFormat.X8R8G8B8,
            DXGIFormat.B5G6R5_UNORM => LegacyFormat.R5G6B5,
            DXGIFormat.B5G5R5A1_UNORM => LegacyFormat.A1R5G5B5,
            DXGIFormat.B4G4R4A4_UNORM => LegacyFormat.A4R4G4B4,
            DXGIFormat.A4B4G4R4_UNORM => LegacyFormat.A4R4G4B4,
            DXGIFormat.A8_UNORM => LegacyFormat.A8,
            DXGIFormat.R10G10B10A2_UNORM => LegacyFormat.A2B10G10R10,
            DXGIFormat.R8G8B8A8_UNORM => LegacyFormat.A8B8G8R8,
            DXGIFormat.R8G8B8A8_UNORM_SRGB => LegacyFormat.A8B8G8R8,
            DXGIFormat.R16G16_UNORM => LegacyFormat.G16R16,
            DXGIFormat.R16G16B16A16_UNORM => LegacyFormat.A16B16G16R16,
            DXGIFormat.R8_UNORM => LegacyFormat.L8,
            DXGIFormat.R8G8_UNORM => LegacyFormat.A8L8,
            DXGIFormat.R8G8_SNORM => LegacyFormat.V8U8,
            DXGIFormat.R8G8B8A8_SNORM => LegacyFormat.Q8W8V8U8,
            DXGIFormat.R16G16_SNORM => LegacyFormat.V16U16,
            DXGIFormat.G8R8_G8B8_UNORM => LegacyFormat.R8G8_B8G8,
            DXGIFormat.YUY2 => LegacyFormat.YUY2,
            DXGIFormat.R8G8_B8G8_UNORM => LegacyFormat.G8R8_G8B8,
            DXGIFormat.BC1_UNORM => LegacyFormat.DXT1,
            DXGIFormat.BC2_UNORM => metadata.IsPMAlpha() ? LegacyFormat.DXT2 : LegacyFormat.DXT3,
            DXGIFormat.BC3_UNORM => metadata.IsPMAlpha() ? LegacyFormat.DXT4 : LegacyFormat.DXT5,
            DXGIFormat.BC4_UNORM => LegacyFormat.ATI1,
            DXGIFormat.BC4_SNORM => LegacyFormat.BC4S,
            DXGIFormat.BC5_UNORM => LegacyFormat.ATI2,
            DXGIFormat.BC5_SNORM => LegacyFormat.BC5S,
            DXGIFormat.D16_UNORM => LegacyFormat.D16,
            DXGIFormat.D32_FLOAT => LegacyFormat.D32F_LOCKABLE,
            DXGIFormat.D24_UNORM_S8_UINT => LegacyFormat.D24S8,
            DXGIFormat.R16_UNORM => LegacyFormat.L16,
            DXGIFormat.R16G16B16A16_SNORM => LegacyFormat.Q16W16V16U16,
            DXGIFormat.R16_FLOAT => LegacyFormat.R16F,
            DXGIFormat.R16G16_FLOAT => LegacyFormat.G16R16F,
            DXGIFormat.R16G16B16A16_FLOAT => LegacyFormat.A16B16G16R16F,
            DXGIFormat.R32_FLOAT => LegacyFormat.R32F,
            DXGIFormat.R32G32_FLOAT => LegacyFormat.G32R32F,
            DXGIFormat.R32G32B32A32_FLOAT => LegacyFormat.A32B32G32R32F,

            _ => LegacyFormat.UNKNOWN,
        };

    /// <summary>
    /// Returns the corresponding Direct3D9 surface format from a Direct3D10/DXGI format.
    /// This is used for the conversion process from .d3dtx to .dds. (Legacy .d3dtx)
    /// </summary>
    /// <param name="dxgiFormat">The DXGI format.</param>
    /// <param name="metadata">The metadata of our .dds file. It is used in determining if the alpha is premultiplied.</param>
    /// <returns>The corresponding Direct3D9 format.</returns>
    public static DXGIFormat GetDXGIFormat(LegacyFormat format) =>
        format switch
        {
            LegacyFormat.A8R8G8B8 => DXGIFormat.B8G8R8A8_UNORM,
            LegacyFormat.X8R8G8B8 => DXGIFormat.B8G8R8X8_UNORM,
            LegacyFormat.X8L8V8U8 => DXGIFormat.B8G8R8A8_UNORM,
            LegacyFormat.R5G6B5 => DXGIFormat.B5G6R5_UNORM,
            LegacyFormat.X1R5G5B5 => DXGIFormat.B5G5R5A1_UNORM,
            LegacyFormat.A1R5G5B5 => DXGIFormat.B5G5R5A1_UNORM,
            LegacyFormat.A4R4G4B4 => DXGIFormat.B4G4R4A4_UNORM,
            LegacyFormat.A8 => DXGIFormat.A8_UNORM,
            LegacyFormat.A2B10G10R10 => DXGIFormat.R10G10B10A2_UNORM,
            LegacyFormat.A8B8G8R8 => DXGIFormat.R8G8B8A8_UNORM,
            LegacyFormat.X8B8G8R8 => DXGIFormat.R8G8B8A8_UNORM,
            LegacyFormat.G16R16 => DXGIFormat.R16G16_UNORM,
            LegacyFormat.A16B16G16R16 => DXGIFormat.R16G16B16A16_UNORM,
            LegacyFormat.L8 => DXGIFormat.R8_UNORM,
            LegacyFormat.A8L8 => DXGIFormat.R8G8_UNORM,
            LegacyFormat.V8U8 => DXGIFormat.R8G8_SNORM,
            LegacyFormat.Q8W8V8U8 => DXGIFormat.R8G8B8A8_SNORM,
            LegacyFormat.V16U16 => DXGIFormat.R16G16_SNORM,
            LegacyFormat.R8G8_B8G8 => DXGIFormat.G8R8_G8B8_UNORM,
            LegacyFormat.YUY2 => DXGIFormat.YUY2,
            LegacyFormat.G8R8_G8B8 => DXGIFormat.R8G8_B8G8_UNORM,
            LegacyFormat.DXT1 => DXGIFormat.BC1_UNORM,
            LegacyFormat.DXT2 => DXGIFormat.BC2_UNORM,
            LegacyFormat.DXT3 => DXGIFormat.BC2_UNORM,
            LegacyFormat.DXT4 => DXGIFormat.BC3_UNORM,
            LegacyFormat.DXT5 => DXGIFormat.BC3_UNORM,
            LegacyFormat.ATI1 => DXGIFormat.BC4_UNORM,
            LegacyFormat.BC4S => DXGIFormat.BC4_SNORM,
            LegacyFormat.ATI2 => DXGIFormat.BC5_UNORM,
            LegacyFormat.BC5S => DXGIFormat.BC5_SNORM,
            LegacyFormat.D16 => DXGIFormat.D16_UNORM,
            LegacyFormat.D32F_LOCKABLE => DXGIFormat.D32_FLOAT,
            LegacyFormat.D24S8 => DXGIFormat.D24_UNORM_S8_UINT,
            LegacyFormat.L16 => DXGIFormat.R16_UNORM,
            LegacyFormat.Q16W16V16U16 => DXGIFormat.R16G16B16A16_SNORM,
            LegacyFormat.R16F => DXGIFormat.R16_FLOAT,
            LegacyFormat.G16R16F => DXGIFormat.R16G16_FLOAT,
            LegacyFormat.A16B16G16R16F => DXGIFormat.R16G16B16A16_FLOAT,
            LegacyFormat.R32F => DXGIFormat.R32_FLOAT,
            LegacyFormat.G32R32F => DXGIFormat.R32G32_FLOAT,
            LegacyFormat.A32B32G32R32F => DXGIFormat.R32G32B32A32_FLOAT,

            // XENOS GPU Format (Old Xbox 360 GPU formats)
            LegacyFormat.XENOS_DXT1 => DXGIFormat.BC1_UNORM,
            LegacyFormat.XENOS_LIN_DXT1 => DXGIFormat.BC1_UNORM,
            LegacyFormat.XENOS_DXT3 => DXGIFormat.BC2_UNORM,
            LegacyFormat.XENOS_LIN_DXT3 => DXGIFormat.BC2_UNORM,
            LegacyFormat.XENOS_DXT5 => DXGIFormat.BC3_UNORM,
            LegacyFormat.XENOS_LIN_DXT5 => DXGIFormat.BC3_UNORM,
            LegacyFormat.XENOS_DXN => DXGIFormat.BC5_UNORM,
            LegacyFormat.XENOS_LIN_DXN => DXGIFormat.BC5_UNORM,
            LegacyFormat.XENOS_A8 => DXGIFormat.A8_UNORM,
            LegacyFormat.XENOS_LIN_A8 => DXGIFormat.A8_UNORM,
            LegacyFormat.XENOS_L8 => DXGIFormat.R8_UNORM,
            LegacyFormat.XENOS_LIN_L8 => DXGIFormat.R8_UNORM,
            LegacyFormat.XENOS_R5G6B5 => DXGIFormat.B5G6R5_UNORM,
            LegacyFormat.XENOS_LIN_R5G6B5 => DXGIFormat.B5G6R5_UNORM,
            LegacyFormat.XENOS_R6G5B5 => DXGIFormat.B5G6R5_UNORM, // Weird format with no DXGI equivalent. Channels are swapped, and the first channel is 6 bits instead of the middle channel..
            LegacyFormat.XENOS_LIN_R6G5B5 => DXGIFormat.B5G5R5A1_UNORM, // Weird format with no DXGI equivalent. Channels are swapped, and the first channel is 6 bits instead of the middle channel..
            LegacyFormat.XENOS_L6V5U5 => DXGIFormat.UNKNOWN, // Bumpmap format - will be converted to R8G8B8A8_UNORM.
            LegacyFormat.XENOS_LIN_L6V5U5 => DXGIFormat.B5G5R5A1_UNORM, // Bumpmap format - will be converted to R8G8B8A8_UNORM.
            LegacyFormat.XENOS_X1R5G5B5 => DXGIFormat.B5G5R5A1_UNORM,
            LegacyFormat.XENOS_LIN_X1R5G5B5 => DXGIFormat.B5G5R5A1_UNORM,
            LegacyFormat.XENOS_A1R5G5B5 => DXGIFormat.B5G5R5A1_UNORM,
            LegacyFormat.XENOS_LIN_A1R5G5B5 => DXGIFormat.B5G5R5A1_UNORM,
            LegacyFormat.XENOS_A4R4G4B4 => DXGIFormat.B4G4R4A4_UNORM,
            LegacyFormat.XENOS_LIN_A4R4G4B4 => DXGIFormat.B4G4R4A4_UNORM,
            LegacyFormat.XENOS_X4R4G4B4 => DXGIFormat.B4G4R4A4_UNORM,
            LegacyFormat.XENOS_LIN_X4R4G4B4 => DXGIFormat.B4G4R4A4_UNORM,
            LegacyFormat.XENOS_Q4W4V4U4 => DXGIFormat.A4B4G4R4_UNORM, // Inverted channels.
            LegacyFormat.XENOS_LIN_Q4W4V4U4 => DXGIFormat.R8G8B8A8_UNORM, // Inverted channels.
            LegacyFormat.XENOS_A8L8 => DXGIFormat.R8G8_UNORM,
            LegacyFormat.XENOS_LIN_A8L8 => DXGIFormat.R8G8_UNORM,
            LegacyFormat.XENOS_G8R8 => DXGIFormat.R8G8_UNORM,
            LegacyFormat.XENOS_LIN_G8R8 => DXGIFormat.R8G8_UNORM,
            LegacyFormat.XENOS_V8U8 => DXGIFormat.R8G8_SNORM,
            LegacyFormat.XENOS_LIN_V8U8 => DXGIFormat.R8G8_SNORM,
            LegacyFormat.XENOS_D16 => DXGIFormat.D16_UNORM,
            LegacyFormat.XENOS_LIN_D16 => DXGIFormat.D16_UNORM,
            LegacyFormat.XENOS_L16 => DXGIFormat.R16_UNORM,
            LegacyFormat.XENOS_LIN_L16 => DXGIFormat.R16_UNORM,
            LegacyFormat.XENOS_R16F => DXGIFormat.R16_FLOAT,
            LegacyFormat.XENOS_LIN_R16F => DXGIFormat.R16_FLOAT,
            LegacyFormat.XENOS_R16F_EXPAND => DXGIFormat.R16_FLOAT,
            LegacyFormat.XENOS_LIN_R16F_EXPAND => DXGIFormat.R16_FLOAT,
            LegacyFormat.XENOS_UYVY => DXGIFormat.YUY2,
            LegacyFormat.XENOS_LIN_UYVY => DXGIFormat.YUY2,
            LegacyFormat.XENOS_LE_UYVY => DXGIFormat.YUY2,
            LegacyFormat.XENOS_LE_LIN_UYVY => DXGIFormat.YUY2,
            LegacyFormat.XENOS_G8R8_G8B8 => DXGIFormat.R8G8_B8G8_UNORM, // Channels could be reversed.
            LegacyFormat.XENOS_LIN_G8R8_G8B8 => DXGIFormat.R8G8_B8G8_UNORM, // Channels could be reversed.
            LegacyFormat.XENOS_R8G8_B8G8 => DXGIFormat.G8R8_G8B8_UNORM, // Channels could be reversed.
            LegacyFormat.XENOS_LIN_R8G8_B8G8 => DXGIFormat.G8R8_G8B8_UNORM, // Channels could be reversed.
            LegacyFormat.XENOS_YUY2 => DXGIFormat.YUY2,
            LegacyFormat.XENOS_LIN_YUY2 => DXGIFormat.YUY2,
            LegacyFormat.XENOS_LE_YUY2 => DXGIFormat.YUY2,
            LegacyFormat.XENOS_LE_LIN_YUY2 => DXGIFormat.YUY2,
            LegacyFormat.XENOS_A8R8G8B8 => DXGIFormat.B8G8R8A8_UNORM,
            LegacyFormat.XENOS_LIN_A8R8G8B8 => DXGIFormat.B8G8R8A8_UNORM,
            LegacyFormat.XENOS_X8R8G8B8 => DXGIFormat.B8G8R8X8_UNORM,
            LegacyFormat.XENOS_LIN_X8R8G8B8 => DXGIFormat.B8G8R8X8_UNORM,
            LegacyFormat.XENOS_A8B8G8R8 => DXGIFormat.R8G8B8A8_UNORM,
            LegacyFormat.XENOS_LIN_A8B8G8R8 => DXGIFormat.R8G8B8A8_UNORM,
            LegacyFormat.XENOS_X8B8G8R8 => DXGIFormat.R8G8B8A8_UNORM,
            LegacyFormat.XENOS_LIN_X8B8G8R8 => DXGIFormat.R8G8B8A8_UNORM,
            LegacyFormat.XENOS_X8L8V8U8 => DXGIFormat.B8G8R8A8_UNORM, // Bumpmap format - will be converted to R8G8B8A8_UNORM.
            LegacyFormat.XENOS_LIN_X8L8V8U8 => DXGIFormat.B8G8R8A8_UNORM, // Bumpmap format - will be converted to R8G8B8A8_UNORM.
            LegacyFormat.XENOS_Q8W8V8U8 => DXGIFormat.R8G8B8A8_SNORM,
            LegacyFormat.XENOS_LIN_Q8W8V8U8 => DXGIFormat.R8G8B8A8_SNORM,
            LegacyFormat.XENOS_A2R10G10B10 => DXGIFormat.R10G10B10A2_UNORM, // Channels could be reversed.
            LegacyFormat.XENOS_LIN_A2R10G10B10 => DXGIFormat.R10G10B10A2_UNORM, // Channels could be reversed.
            LegacyFormat.XENOS_X2R10G10B10 => DXGIFormat.R10G10B10A2_UNORM, // Channels could be reversed.
            LegacyFormat.XENOS_LIN_X2R10G10B10 => DXGIFormat.R10G10B10A2_UNORM, // Channels could be reversed.
            LegacyFormat.XENOS_A2B10G10R10 => DXGIFormat.R10G10B10A2_UNORM, // Channels could be reversed.
            LegacyFormat.XENOS_LIN_A2B10G10R10 => DXGIFormat.R10G10B10A2_UNORM, // Channels could be reversed.
            LegacyFormat.XENOS_A2W10V10U10 => DXGIFormat.R10G10B10A2_UNORM, // Channels could be reversed.
            LegacyFormat.XENOS_LIN_A2W10V10U10 => DXGIFormat.R10G10B10A2_UNORM, // Channels could be reversed.
            LegacyFormat.XENOS_A16L16 => DXGIFormat.R16G16_UNORM,
            LegacyFormat.XENOS_LIN_A16L16 => DXGIFormat.R16G16_UNORM,
            LegacyFormat.XENOS_G16R16 => DXGIFormat.R16G16_UNORM,
            LegacyFormat.XENOS_LIN_G16R16 => DXGIFormat.R16G16_UNORM,
            LegacyFormat.XENOS_V16U16 => DXGIFormat.R16G16_UNORM,
            LegacyFormat.XENOS_LIN_V16U16 => DXGIFormat.R16G16_UNORM,
            // --- Weird formats, I hope they are not used in any game. ---
            LegacyFormat.XENOS_R10G11B11 => DXGIFormat.R11G11B10_FLOAT,
            LegacyFormat.XENOS_LIN_R10G11B11 => DXGIFormat.R11G11B10_FLOAT,
            LegacyFormat.XENOS_R11G11B10 => DXGIFormat.R11G11B10_FLOAT,
            LegacyFormat.XENOS_LIN_R11G11B10 => DXGIFormat.R11G11B10_FLOAT,
            LegacyFormat.XENOS_W10V11U11 => DXGIFormat.R11G11B10_FLOAT,
            LegacyFormat.XENOS_LIN_W10V11U11 => DXGIFormat.R11G11B10_FLOAT,
            LegacyFormat.XENOS_W11V11U10 => DXGIFormat.R11G11B10_FLOAT,
            LegacyFormat.XENOS_LIN_W11V11U10 => DXGIFormat.R11G11B10_FLOAT,
            // -----------------------------------------------------------
            LegacyFormat.XENOS_G16R16F => DXGIFormat.R16G16_FLOAT,
            LegacyFormat.XENOS_LIN_G16R16F => DXGIFormat.R16G16_FLOAT,
            LegacyFormat.XENOS_G16R16F_EXPAND => DXGIFormat.R16G16_FLOAT,
            LegacyFormat.XENOS_LIN_G16R16F_EXPAND => DXGIFormat.R16G16_FLOAT,
            LegacyFormat.XENOS_L32 => DXGIFormat.R32_FLOAT,
            LegacyFormat.XENOS_LIN_L32 => DXGIFormat.R32_FLOAT,
            LegacyFormat.XENOS_R32F => DXGIFormat.R32_FLOAT,
            LegacyFormat.XENOS_LIN_R32F => DXGIFormat.R32_FLOAT,
            LegacyFormat.XENOS_A16B16G16R16 => DXGIFormat.R16G16B16A16_UNORM,
            LegacyFormat.XENOS_LIN_A16B16G16R16 => DXGIFormat.R16G16B16A16_UNORM,
            LegacyFormat.XENOS_Q16W16V16U16 => DXGIFormat.R16G16B16A16_SNORM,
            LegacyFormat.XENOS_LIN_Q16W16V16U16 => DXGIFormat.R16G16B16A16_SNORM,
            LegacyFormat.XENOS_A16B16G16R16F => DXGIFormat.R16G16B16A16_FLOAT,
            LegacyFormat.XENOS_LIN_A16B16G16R16F => DXGIFormat.R16G16B16A16_FLOAT,
            LegacyFormat.XENOS_A16B16G16R16F_EXPAND => DXGIFormat.R16G16B16A16_FLOAT,
            LegacyFormat.XENOS_LIN_A16B16G16R16F_EXPAND => DXGIFormat.R16G16B16A16_FLOAT,
            LegacyFormat.XENOS_A32L32 => DXGIFormat.R32G32_FLOAT,
            LegacyFormat.XENOS_LIN_A32L32 => DXGIFormat.R32G32_FLOAT,
            LegacyFormat.XENOS_G32R32 => DXGIFormat.R32G32_FLOAT,
            LegacyFormat.XENOS_LIN_G32R32 => DXGIFormat.R32G32_FLOAT,
            LegacyFormat.XENOS_V32U32 => DXGIFormat.R32G32_FLOAT,
            LegacyFormat.XENOS_LIN_V32U32 => DXGIFormat.R32G32_FLOAT,
            LegacyFormat.XENOS_G32R32F => DXGIFormat.R32G32_FLOAT,
            LegacyFormat.XENOS_LIN_G32R32F => DXGIFormat.R32G32_FLOAT,
            LegacyFormat.XENOS_A32B32G32R32 => DXGIFormat.R32G32B32A32_FLOAT,
            LegacyFormat.XENOS_LIN_A32B32G32R32 => DXGIFormat.R32G32B32A32_FLOAT,
            LegacyFormat.XENOS_Q32W32V32U32 => DXGIFormat.R32G32B32A32_FLOAT,
            LegacyFormat.XENOS_LIN_Q32W32V32U32 => DXGIFormat.R32G32B32A32_FLOAT,
            LegacyFormat.XENOS_A32B32G32R32F => DXGIFormat.R32G32B32A32_FLOAT,
            LegacyFormat.XENOS_LIN_A32B32G32R32F => DXGIFormat.R32G32B32A32_FLOAT,
            LegacyFormat.XENOS_A2B10G10R10F_EDRAM => DXGIFormat.R10G10B10A2_UNORM,
            LegacyFormat.XENOS_G16R16_EDRAM => DXGIFormat.R16G16_UNORM,
            LegacyFormat.XENOS_A16B16G16R16_EDRAM => DXGIFormat.R16G16B16A16_UNORM,
            LegacyFormat.XENOS_LE_X8R8G8B8 => DXGIFormat.B8G8R8X8_UNORM,
            LegacyFormat.XENOS_LE_A8R8G8B8 => DXGIFormat.B8G8R8A8_UNORM,
            LegacyFormat.XENOS_LE_X2R10G10B10 => DXGIFormat.R10G10B10A2_UNORM,
            LegacyFormat.XENOS_LE_A2R10G10B10 => DXGIFormat.R10G10B10A2_UNORM,
            LegacyFormat.XENOS_INDEX16 => DXGIFormat.R16_UINT,
            LegacyFormat.XENOS_INDEX32 => DXGIFormat.R32_UINT,
            LegacyFormat.XENOS_VERTEXDATA => DXGIFormat.R32G32B32A32_FLOAT,
            LegacyFormat.XENOS_DXT3A => DXGIFormat.BC2_UNORM, // 4x4 pixels - pixelScalars[16] 4-bit scalar value
            LegacyFormat.XENOS_LIN_DXT3A => DXGIFormat.BC2_UNORM, // 4x4 pixels - pixelScalars[16] 4-bit scalar value
            LegacyFormat.XENOS_DXT3A_1111 => DXGIFormat.BC2_UNORM, // Same as DXT3A, but each bit of scalar data represents mask for respective channel
            LegacyFormat.XENOS_LIN_DXT3A_1111 => DXGIFormat.BC2_UNORM, // Same as DXT3A, but each bit of scalar data represents mask for respective channel
            LegacyFormat.XENOS_DXT5A => DXGIFormat.BC3_UNORM, // s0 8-bit scalar; s1 8-bit scalar; pixelScalarIndices[16] 3-bit indices
            LegacyFormat.XENOS_LIN_DXT5A => DXGIFormat.BC3_UNORM, // s0 8-bit scalar; s1 8-bit scalar; pixelScalarIndices[16] 3-bit indices
            LegacyFormat.XENOS_CTX1 => DXGIFormat.BC1_UNORM, // s0 8-bit scalar channel 0; s1 8-bit scalar channel 1; s2 8-bit scalar channel 2; s3 8-bit scalar channel 3; pixelScalarIndices[16] 2-bit indices, shared between both channels
            LegacyFormat.XENOS_LIN_CTX1 => DXGIFormat.BC1_UNORM, // s0 8-bit scalar channel 0; s1 8-bit scalar channel 1; s2 8-bit scalar channel 2; s3 8-bit scalar channel 3; pixelScalarIndices[16] 2-bit indices, shared between both channels
            LegacyFormat.XENOS_D24S8 => DXGIFormat.D24_UNORM_S8_UINT,
            LegacyFormat.XENOS_LIN_D24S8 => DXGIFormat.D24_UNORM_S8_UINT,
            LegacyFormat.XENOS_D24X8 => DXGIFormat.X24_TYPELESS_G8_UINT,
            LegacyFormat.XENOS_LIN_D24X8 => DXGIFormat.X24_TYPELESS_G8_UINT,
            LegacyFormat.XENOS_D24FS8 => DXGIFormat.D24_UNORM_S8_UINT,
            LegacyFormat.XENOS_LIN_D24FS8 => DXGIFormat.D24_UNORM_S8_UINT,
            LegacyFormat.XENOS_D32 => DXGIFormat.D32_FLOAT,
            LegacyFormat.XENOS_LIN_D32 => DXGIFormat.D32_FLOAT,

            _ => DXGIFormat.UNKNOWN,
        };

    public static T3TextureLayout GetDimensionFromDDS(Hexa.NET.DirectXTex.TexMetadata metadata)
    {
        if (metadata.ArraySize > 1)
        {
            return metadata.IsCubemap()
                ? T3TextureLayout.TextureCubemapArray
                : T3TextureLayout.Texture2DArray;
        }
        else if (metadata.IsVolumemap())
        {
            return T3TextureLayout.Texture3D;
        }
        else
        {
            return metadata.IsCubemap()
                ? T3TextureLayout.TextureCubemap
                : T3TextureLayout.Texture2D;
        }
    }
}
