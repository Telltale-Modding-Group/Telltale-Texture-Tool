using System;
using System.Collections.Generic;
using System.IO;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;
using TelltaleToolKit;
using TelltaleToolKit.Serialization.Binary;
using TelltaleToolKit.T3Types.Textures;
using TelltaleToolKit.T3Types.Textures.T3Types;
using TelltaleToolKit.Utility;
using TelltaleToolKit.Utility.Blowfish;

namespace TelltaleTextureTool.Codecs;

public class D3dtxCodec : IImageCodec
{
    public string Name => "D3DTX Codec";
    public string FormatName => "Telltale Tool Texture";
    public string[] SupportedExtensions => [".d3dtx"];
    public PixelFormatInfo[] SupportedPixelFormats => [];

    private static readonly DdsCodec ddsCodec = new();

    public byte[] SaveToMemory(Texture texture, CodecOptions options)
    {
        throw new NotImplementedException();
    }

    public Texture LoadFromMemory(Stream stream, CodecOptions options)
    {
        var d3dtxTexture = TTK.Load<T3Texture>(stream, out MetaStreamConfiguration configuration);

        //  d3dtx.ReadD3DTXBytes(data, options.TelltaleToolGame, options.IsLegacyConsole);

        if (d3dtxTexture.DdsTextureData.Length > 0)
        {
            if (!d3dtxTexture.IsEncrypted)
                return ddsCodec.LoadFromMemory(
                    new MemoryStream(d3dtxTexture.DdsTextureData),
                    options
                );

            T3BlowfishKey[] keys = Enum.GetValues<T3BlowfishKey>();

            foreach (T3BlowfishKey key in keys)
            {
                var blowfish = new Blowfish(key.GetBlowfishKey(), 2);

                d3dtxTexture.Decrypt(blowfish);

                if (
                    d3dtxTexture.DdsTextureData[0] == 0x44
                    && d3dtxTexture.DdsTextureData[1] == 0x44
                    && d3dtxTexture.DdsTextureData[2] == 0x53
                    && d3dtxTexture.DdsTextureData[3] == 0x20
                )
                {
                    return ddsCodec.LoadFromMemory(
                        new MemoryStream(d3dtxTexture.DdsTextureData),
                        options
                    );
                }

                d3dtxTexture.Encrypt(blowfish);

                var blowfishModified = new Blowfish(key.GetBlowfishKey(), 7);

                d3dtxTexture.Decrypt(blowfishModified);

                if (
                    d3dtxTexture.DdsTextureData[0] == 0x44
                    && d3dtxTexture.DdsTextureData[1] == 0x44
                    && d3dtxTexture.DdsTextureData[2] == 0x53
                    && d3dtxTexture.DdsTextureData[3] == 0x20
                )
                {
                    return ddsCodec.LoadFromMemory(
                        new MemoryStream(d3dtxTexture.DdsTextureData),
                        options
                    );
                }

                d3dtxTexture.Encrypt(blowfishModified);
            }

            throw new Exception("Texture couldn't be decrypted!");

            // ddsTexture.Metadata.ExtraMetadata.DebugInformation = d3dtx.GetD3DTXDebugInfo();
        }
        else if (d3dtxTexture.TplTextureData.Length > 0)
        {
            throw new Exception("Legacy console or wrong decrypting key.");
        }

        Texture texture = new();

        T3SurfaceFormat surfaceFormat = d3dtxTexture.SurfaceFormat;
        T3SurfaceGamma surfaceGamma = d3dtxTexture.SurfaceGamma;
        PlatformType platformType = d3dtxTexture.PlatformType.Value;


        if (d3dtxTexture.ArraySize == 0)
        {
            d3dtxTexture.ArraySize = 1;
        }

        if (d3dtxTexture.Depth == 0)
        {
            d3dtxTexture.Depth = 1;
        }

        texture.Metadata = new TexMetadata
        {
            Width = d3dtxTexture.Width,
            Height = d3dtxTexture.Height,
            PixelFormatInfo =
                GetPixelFormatInfo(surfaceFormat, surfaceGamma,
                    platformType), // Change this to the correct pixel format
            MipLevels = d3dtxTexture.NumMipLevels,
            ArraySize = d3dtxTexture.IsCubemap()
                ? d3dtxTexture.ArraySize * 6
                : d3dtxTexture.ArraySize,
            Depth = d3dtxTexture.Depth,
            Dimension = d3dtxTexture.IsVolumemap() ? TexDimension.Tex3D : TexDimension.Tex2D,
            IsVolumemap = d3dtxTexture.IsVolumemap(),
            IsCubemap = d3dtxTexture.IsCubemap(),
            IsPremultipliedAlpha = false, // Sort of?
        };

        // TODO:
        // texture.Metadata.ExtraMetadata.DebugInformation = d3dtx.GetD3DTXDebugInfo();

        // NOTES: Telltale mip levels are reversed in Poker Night 2 and above, presumably extracted from KTX and KTX2 files.
        //
        // The faces are NOT reversed.
        // This is likely corelating with the way that KTX and KTX2 files are written.
        // Some normal maps specifically with type 4 (eTxNormalMap) channels are all reversed (ABGR instead of RGBA) (Only applies for newer games)
        // Some surface formats are dependant on platforms. For example, iOS textures have their R and B channels swapped.
        // Some surface formats are not supported by DDS. In this case, the texture will be written as a raw texture.

        if (
            d3dtxTexture.PlatformType.Value == PlatformType.NX
            && d3dtxTexture.IsVolumemap()
        )
        {
            texture.Metadata.Height *= texture.Metadata.Depth;
            texture.Metadata.Depth = 1;
            texture.Images = new Image[1];

            texture.Images[0] = new Image
            {
                Width = texture.Metadata.Width,
                Height = texture.Metadata.Height,
                RowPitch = 0,
                SlicePitch = 0,
                PixelFormatInfo = texture.Metadata.PixelFormatInfo,
                Pixels = d3dtxTexture.RegionHeaders[0].RegionData
            };

            Texture.DeswizzleImage(texture.Images[0], Platform.Switch);

            texture.Metadata.Width = (uint)Math.Sqrt(d3dtxTexture.Depth) * d3dtxTexture.Width;
            texture.Metadata.Height = (uint)Math.Sqrt(d3dtxTexture.Depth) * d3dtxTexture.Height;

            texture.Images[0].Width = texture.Metadata.Width;
            texture.Images[0].Height = texture.Metadata.Height;

            return texture;
        }

        // Get all possible images in a single texture. This takes into account the mip levels, array size and depth.
        texture.Images = new Image[texture.GetMaxPossibleImages()];

        // The following segment is about extracting the pixel data after reading the header.
        // My library's texture format is very extensible and can support any texture layout, pixel format and most swizzling methods.

        // The first step is to sort all regions by face indexes and mip indexes.
        List<T3Texture.RegionStreamHeader> orderRegions = d3dtxTexture.GetRegionDataSortedByMips();

        // Initialize base dimensions and region index.
        int regionIndex = 0;
        uint width = texture.Metadata.Width;
        uint height = texture.Metadata.Height;
        uint depth = texture.Metadata.Depth;

        // These values are used for extracting regions with multiple mip levels. More on that below.
        bool lockRegionIndex = false;
        int backupRegionIndex = 0;

        // The 3 loops iterate through ktx-format like structure - mip levels -> array size -> depth.
        for (uint i = 0; i < texture.Metadata.MipLevels; i++)
        {
            for (uint j = 0; j < texture.Metadata.ArraySize; j++)
            {
                // Ok, this is the fun part. Usually in almost all games, the mip count per region is 1 (basically the region has a single image).
                // But D3DTX supports multiple mip levels per region.
                // Those regions with mip count above 1 have a mip index of 256 (or -1 if it was a byte value).
                // Why is that the case? Long-story short, Telltale devs wanted their textures to be customizable as possible.
                // For console games they wanted to disable some specific mipmap levels.
                // Another reason is that their regions for console games are padded to the highest possible data size.
                // Example: 512x512 BC1 textures has a data size of 128x128x8 bytes. For a console texture, the other regions will have a data size of 128x128x8.
                // But why would Telltale even bother storing them then? Well, I can only speculate - for non-destructive editing reasons.
                // After all, D3DTX can be configured using the Telltale Tool engine. They need to store the data somewhere.
                // So how do I extract them? I remove one mip in that region, but the values for slicepitch or rowpitch are not accurate.
                if (regionIndex >= orderRegions.Count)
                {
                    regionIndex = backupRegionIndex;
                    lockRegionIndex = false;
                }

                for (uint k = 0; k < depth; k++)
                {
                    // The region headers' accuracy are not guaranteed. There are textures where they are outright wrong, or have a value of 0.
                    // I override that, since my library supports all possible formats.
                    var pitches = PixelFormatUtility.ComputePitch(
                        texture.Metadata.PixelFormatInfo.PixelFormat,
                        width,
                        height
                    );

                    orderRegions[regionIndex].Pitch = (int)pitches.rowPitch;
                    orderRegions[regionIndex].SlicePitch = (int)(
                        orderRegions[regionIndex].Pitch * height
                    );

                    // Right, now I take the region pixel data and put it inside my texture format.
                    // My texture format resembles the DDS pixel data structure.
                    // I could technically make it to KTX to avoid sorting, but in this case only references are moved.
                    // The calculate image index is a helper function that calculates the index of the image in the texture array.
                    // As for the pixels themselves, I extract either a single slice, or a single mip (if mipcount is > 1).
                    // So, why do I extract the slice like that? D3DTX can support multiple slices per region. This is only used for volume textures.
                    texture.Images[texture.CalculateImageIndex(i, j, k)] = new Image
                    {
                        Width = width,
                        Height = height,
                        RowPitch = (uint)orderRegions[regionIndex].Pitch,
                        SlicePitch = (uint)orderRegions[regionIndex].SlicePitch,
                        PixelFormatInfo = texture.Metadata.PixelFormatInfo,
                        Pixels =
                            orderRegions[regionIndex].MipCount <= 1
                                ? d3dtxTexture.GetSliceData(orderRegions[regionIndex], k)
                                : d3dtxTexture.ExtractSingleMipFromRegion(
                                    orderRegions[regionIndex],
                                    texture.Metadata.PixelFormatInfo.PixelFormat,
                                    width,
                                    height,
                                    k
                                ),
                    };
                }

                // Read Mip
                // Remove mip
                // Queue next mip
                d3dtxTexture.RemoveMip(
                    orderRegions[regionIndex],
                    texture.Metadata.PixelFormatInfo.PixelFormat,
                    width,
                    height,
                    depth
                );

                if (orderRegions[regionIndex].MipCount > 1 && !lockRegionIndex)
                {
                    backupRegionIndex = regionIndex;
                    lockRegionIndex = true;
                }

                regionIndex++;
            }

            width = Math.Max(1, width >> 1);
            height = Math.Max(1, height >> 1);
            depth = Math.Max(1, depth >> 1);
        }

        return texture;
    }

    /// <summary>
    /// Get the DXGI format from a Telltale surface format. Gamma and platform type are optional.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="gamma"></param>
    /// <param name="platformType"></param>
    /// <returns>The corresponding DXGI_Format.</returns>
    public static PixelFormatInfo GetPixelFormatInfo(
        T3SurfaceFormat format,
        T3SurfaceGamma gamma = T3SurfaceGamma.Linear,
        PlatformType platformType = PlatformType.PC
    )
    {
        PixelFormat pixelFormat = format switch
        {
            T3SurfaceFormat.ARGB8 => PixelFormat.R8G8B8A8, // channel?
            T3SurfaceFormat.ARGB16 => PixelFormat.R16G16B16A16,
            T3SurfaceFormat.RGB565 => PixelFormat.B5G6R5, // channel?
            T3SurfaceFormat.ARGB1555 => PixelFormat.B5G5R5A1,
            T3SurfaceFormat.ARGB4 => PixelFormat.B4G4R4A4,
            T3SurfaceFormat.ARGB2101010 => PixelFormat.R10G10B10A2,
            T3SurfaceFormat.R16 => PixelFormat.R16, // L16
            T3SurfaceFormat.RG16 => PixelFormat.R16G16, // DXGI IS FLOAT, D3DFORMAT is UNORM
            T3SurfaceFormat.RGBA16 => PixelFormat.R16G16B16A16, // UNORM
            T3SurfaceFormat.RG8 => PixelFormat.R8G8,
            T3SurfaceFormat.RGBA8 => PixelFormat.R8G8B8A8, // channel?
            T3SurfaceFormat.R32 => PixelFormat.R32, // D3DFormat 42???? UINT??
            T3SurfaceFormat.RG32 => PixelFormat.R32G32, // UINT Yoooo
            T3SurfaceFormat.RGBA32 => PixelFormat.R32G32B32A32, // UINT LOL
            T3SurfaceFormat.R8 => PixelFormat.R8,
            T3SurfaceFormat.RGBA8S => PixelFormat.R8G8B8A8,
            T3SurfaceFormat.A8 => PixelFormat.A8,
            T3SurfaceFormat.L8 => PixelFormat.R8,
            T3SurfaceFormat.AL8 => PixelFormat.R8G8,
            T3SurfaceFormat.L16 => PixelFormat.R16,
            T3SurfaceFormat.RG16S => PixelFormat.R16G16,
            T3SurfaceFormat.RGBA16S => PixelFormat.R16G16B16A16,
            T3SurfaceFormat.R16UI => PixelFormat.R16,
            T3SurfaceFormat.RG16UI => PixelFormat.R16G16,
            T3SurfaceFormat.R16F => PixelFormat.R16, // FLOAT
            T3SurfaceFormat.RG16F => PixelFormat.R16G16,
            T3SurfaceFormat.RGBA16F => PixelFormat.R16G16B16A16, // FLOAT
            T3SurfaceFormat.R32F => PixelFormat.R32, // FLOAT
            T3SurfaceFormat.RG32F => PixelFormat.R32G32, // FLOAT
            T3SurfaceFormat.RGBA32F => PixelFormat.R32G32B32A32, // FLOAT
            T3SurfaceFormat.RGBA1010102F => PixelFormat.R10G10B10A2,
            T3SurfaceFormat.RGB111110F => PixelFormat.R11G11B10,
            T3SurfaceFormat.RGB9E5F => PixelFormat.R9G9B9E5,
            T3SurfaceFormat.DepthPCF16 => PixelFormat.D16,
            T3SurfaceFormat.DepthPCF24 => PixelFormat.D24X8,
            T3SurfaceFormat.Depth16 => PixelFormat.D16,
            T3SurfaceFormat.Depth24 => PixelFormat.D24X8,
            T3SurfaceFormat.DepthStencil32 => PixelFormat.D24S8,
            T3SurfaceFormat.Depth32F => PixelFormat.D32, // UNknown
            T3SurfaceFormat.Depth32F_Stencil8 => PixelFormat.D32S8, // UNknown
            T3SurfaceFormat.Depth24F_Stencil8 => PixelFormat.D24S8, // UNknown
            T3SurfaceFormat.BC1 => PixelFormat.BC1,
            T3SurfaceFormat.BC2 => PixelFormat.BC2,
            T3SurfaceFormat.BC3 => PixelFormat.BC3,
            T3SurfaceFormat.BC4 => PixelFormat.BC4,
            T3SurfaceFormat.BC5 => PixelFormat.BC5,
            T3SurfaceFormat.BC6 => PixelFormat.BC6H,
            T3SurfaceFormat.BC7 => PixelFormat.BC7,
            T3SurfaceFormat.ATC_RGB => PixelFormat.ATC_RGB,
            T3SurfaceFormat.ATC_RGB1A => PixelFormat.ATC_RGBA_EXPLICIT_ALPHA,
            T3SurfaceFormat.ATC_RGBA => PixelFormat.ATC_RGBA_INTERPOLATED_ALPHA,

            T3SurfaceFormat.PVRTC2 => PixelFormat.PVRTC1_2BPP_RGB,
            T3SurfaceFormat.PVRTC2a => PixelFormat.PVRTC1_2BPP_RGB,
            T3SurfaceFormat.PVRTC4 => PixelFormat.PVRTC1_4BPP_RGB,
            T3SurfaceFormat.PVRTC4a => PixelFormat.PVRTC1_4BPP_RGB,
            T3SurfaceFormat.CTX1 => PixelFormat.CTX1,
            T3SurfaceFormat.FrontBuffer => PixelFormat.B8G8R8X8,
            T3SurfaceFormat.Unknown => PixelFormat.Unknown,

            T3SurfaceFormat.ETC1_RGB => PixelFormat.ETC1,
            T3SurfaceFormat.ETC2_RGB => PixelFormat.ETC2_RGB,
            T3SurfaceFormat.ETC2_RGB1A => PixelFormat.ETC2_RGB_A1,
            T3SurfaceFormat.ETC2_RGBA => PixelFormat.ETC2_RGBA,
            T3SurfaceFormat.ETC2_R => PixelFormat.ETC2_R11,
            T3SurfaceFormat.ETC2_RG => PixelFormat.ETC2_RG11,
            T3SurfaceFormat.ASTC_RGBA_4x4 => PixelFormat.ASTC_4x4,
            T3SurfaceFormat.Count => throw new System.NotImplementedException(),
            _ => throw new System.NotImplementedException(),

            //  _ => DXGIFormat.R8G8B8A8_UNORM, // Choose R8G8B8A8 if the format is not specified. (Raw data)
        };

        if (platformType is PlatformType.iPhone or PlatformType.Android)
        {
            pixelFormat = GetPixelFormatWithSwappedRBChannels(pixelFormat);
        }

        ColorSpace colorSpace = gamma == T3SurfaceGamma.sRGB ? ColorSpace.sRGB : ColorSpace.Linear;

        DataType dataType = format switch
        {
            T3SurfaceFormat.RGBA8S => DataType.Snorm,
            T3SurfaceFormat.RG16S => DataType.Snorm,
            T3SurfaceFormat.RGBA16S => DataType.Snorm,

            T3SurfaceFormat.R16UI => DataType.Uint,
            T3SurfaceFormat.RG16UI => DataType.Uint,

            T3SurfaceFormat.R32 => DataType.Uint,
            T3SurfaceFormat.RG32 => DataType.Uint,
            T3SurfaceFormat.RGBA32 => DataType.Uint,

            T3SurfaceFormat.R16F => DataType.Float,
            T3SurfaceFormat.RG16F => DataType.Float,
            T3SurfaceFormat.RGBA16F => DataType.Float,
            T3SurfaceFormat.R32F => DataType.Float,
            T3SurfaceFormat.RG32F => DataType.Float,
            T3SurfaceFormat.RGBA32F => DataType.Float,
            T3SurfaceFormat.RGBA1010102F => DataType.Float,
            T3SurfaceFormat.RGB111110F => DataType.Float,
            T3SurfaceFormat.RGB9E5F => DataType.Float,
            _ => DataType.Unorm,
        };

        return new PixelFormatInfo(pixelFormat, dataType, colorSpace);
    }

    public static PixelFormat GetPixelFormatWithSwappedRBChannels(PixelFormat pixelFormat) =>
        pixelFormat switch
        {
            PixelFormat.B8G8R8A8 => PixelFormat.R8G8B8A8,
            PixelFormat.R8G8B8A8 => PixelFormat.B8G8R8A8,
            PixelFormat.A4B4G4R4 => PixelFormat.B4G4R4A4,
            PixelFormat.B4G4R4A4 => PixelFormat.A4B4G4R4,
            _ => pixelFormat,
        };
}