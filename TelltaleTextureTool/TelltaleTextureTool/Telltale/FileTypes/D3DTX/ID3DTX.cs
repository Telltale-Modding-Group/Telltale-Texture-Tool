using System.Collections.Generic;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.DirectX.Enums;
using TelltaleTextureTool.TelltaleEnums;
using TelltaleTextureTool.TelltaleTypes;
using TelltaleTextureTool.FileTypes;

namespace TelltaleTextureTool.Telltale.FileTypes.D3DTX;

/// <summary>
/// Interface for a D3DTX object.
/// </summary>
public interface ID3DTX : ITelltaleSerializable, ITelltaleDebuggable
{
    /// <summary>
    /// Modifies the D3DTX object with the given ImageSections.
    /// </summary>
    /// <param name="metadata">The metadata of the D3DTX object.</param>    
    /// <param name="imageSections">The ImageSections to modify the D3DTX object with.</param>
    /// <param name="printDebug">Indicates whether to print debug information.</param>
    /// <returns></returns>
    void ModifyD3DTX(D3DTXMetadata metadata, ImageSection[] imageSections, bool printDebug = false);

    /// <summary>
    /// Extracts common data from a D3DTX file.
    /// </summary>
    /// <returns>The common data extracted from the D3DTX file.</returns>
    D3DTXMetadata GetD3DTXMetadata();

    /// <summary>
    /// Gets the pixel data from the D3DTX object.
    /// </summary>
    /// <returns>The texture's pixel data.</returns>
    List<byte[]> GetPixelData();

    /// <summary>
    /// Gets the size of the header in bytes.
    /// </summary>
    /// <returns></returns>
    uint GetHeaderByteSize();
}

// /// <summary>
// /// Represents the metadata of a D3DTX file. This data is later used in creating texture files.
// /// </summary>
 public partial class D3DTXMetadata
{
     public string TextureName { get; set; } = string.Empty;

     public uint Width { get; set; }

     public uint Height { get; set; } = 1;

     public uint Depth { get; set; } = 1;

     public uint ArraySize { get; set; } = 1;

     public uint MipLevels { get; set; } = 1;

     public T3SurfaceFormat Format { get; set; } = T3SurfaceFormat.Unknown;

     public T3SurfaceGamma SurfaceGamma { get; set; } = T3SurfaceGamma.Unknown;

     public T3TextureLayout Dimension { get; set; } = T3TextureLayout.Unknown;

     public T3TextureAlphaMode AlphaMode { get; set; } = T3TextureAlphaMode.Unknown;

     public T3PlatformType Platform { get; set; } = T3PlatformType.ePlatform_None;

     public T3TextureType TextureType { get; set; } = T3TextureType.eTxUnknown;

     public RegionStreamHeader[] RegionHeaders { get; set; } = []; // Only used for D3DTX v3-9

     public LegacyFormat D3DFormat { get; set; } = LegacyFormat.UNKNOWN;

     public bool IsCubemap() => Dimension == T3TextureLayout.TextureCubemap || Dimension == T3TextureLayout.TextureCubemapArray;

     public bool IsVolumemap() => Dimension == T3TextureLayout.Texture3D;

     public bool IsArrayTexture() => Dimension == T3TextureLayout.Texture2DArray || Dimension == T3TextureLayout.TextureCubemapArray;

     public bool IsLegacyD3DTX() => D3DFormat == LegacyFormat.UNKNOWN;
}