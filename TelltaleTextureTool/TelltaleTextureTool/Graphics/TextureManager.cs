using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Runtime.InteropServices;
using Hexa.NET.DirectXTex;
using TelltaleTextureTool.DirectX.Enums;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;
using TelltaleTextureTool.TelltaleEnums;
using TelltaleTextureTool.Utilities;
using static TelltaleTextureTool.DirectX.TextureManager;
using Image = Hexa.NET.DirectXTex.Image;

namespace TelltaleTextureTool.DirectX;

using TexMetadata = Hexa.NET.DirectXTex.TexMetadata;

/// <summary>
/// Image section of texture file. Contains width, height, format, slice pitch, row pitch and the pixels.
/// </summary>
public class ImageSection
{
    public nuint Width { get; set; }
    public nuint Height { get; set; }
    public DXGIFormat Format { get; set; }
    public nuint SlicePitch { get; set; }
    public nuint RowPitch { get; set; }
    public byte[] Pixels { get; set; } = [];
};

/// <summary>
/// Effects that can be applied to an image.
/// </summary>
public enum ImageEffect
{
    [Display(Name = "Default Mode")]
    DEFAULT,

    [Display(Name = "Swizzle ABGR")]
    SWIZZLE_ABGR,

    [Display(Name = "Restore Z")]
    RESTORE_Z,

    [Display(Name = "Remove Z")]
    REMOVE_Z,
}

/// <summary>
/// A class that provides methods to interact with DirectXTex. Mainly used for loading and saving DDS files.
/// </summary>
public unsafe static partial class TextureManager
{
    /// <summary>
    /// Returns a byte array from a DirectXTexNet DDS image.
    /// </summary>
    /// <param name="image">The DirectXTexNet DDS image.</param>
    /// <param name="flags">(Optional) The mode in which the DirectXTexNet will load the .dds file. If not provided, it defaults to NONE.</param>
    /// <returns>The byte array containing the DDS data.</returns>
    public static byte[] GetDDSByteArray(ScratchImage image, DDSFlags flags = DDSFlags.None)
    {
        Blob blob = DirectXTex.CreateBlob();
        try
        {
            TexMetadata metadata = image.GetMetadata();
            DirectXTex.SaveToDDSMemory2(
                image.GetImages(),
                image.GetImageCount(),
                ref metadata,
                flags,
                ref blob
            );
            // Create a byte array to hold the data

            byte[] ddsArray = new byte[blob.GetBufferSize()];

            // Read the data from the Blob into the byte array
            Marshal.Copy((nint)blob.GetBufferPointer(), ddsArray, 0, ddsArray.Length);
            return ddsArray;
        }
        finally
        {
            blob.Release();
        }
    }

    public static D3DTXMetadata GetTextureInformation(TexMetadata metadata)
    {
        return new D3DTXMetadata
        {
            Width = (uint)metadata.Width,
            Height = (uint)metadata.Height,
            Depth = (uint)metadata.Depth,
            ArraySize = (uint)metadata.ArraySize,
            MipLevels = (uint)metadata.MipLevels,
            Format = DDSHelper.GetTelltaleSurfaceFormat((DXGIFormat)metadata.Format),
            SurfaceGamma = DirectXTex.IsSRGB(metadata.Format)
                ? T3SurfaceGamma.sRGB
                : T3SurfaceGamma.Linear,
            D3DFormat = DDSHelper.GetD3DFormat((DXGIFormat)metadata.Format, metadata),
            Dimension = DDSHelper.GetDimensionFromDDS(metadata),
        };
    }

    /// <summary>
    /// Returns a byte array List containing the pixel data from an ImageSection array.
    /// </summary>
    /// <param name="sections">The sections of the image.</param>
    /// <returns></returns>
    public static List<byte[]> GetPixelDataListFromSections(ImageSection[] sections)
    {
        List<byte[]> textureData = [];

        foreach (ImageSection imageSection in sections)
        {
            textureData.Add(imageSection.Pixels);
        }

        return textureData;
    }

    /// <summary>
    /// Returns a byte array List containing the pixel data from an ImageSection array.
    /// </summary>
    /// <param name="sections">The sections of the image.</param>
    /// <returns></returns>
    public static byte[] GetPixelDataArrayFromSections(ImageSection[] sections)
    {
        byte[] textureData = [];

        foreach (ImageSection imageSection in sections)
        {
            textureData = ByteFunctions.Combine(textureData, imageSection.Pixels);
        }

        return textureData;
    }

    /// <summary>
    /// Returns the image sections of the DDS image. Each mipmap and slice is a section on its own.
    /// </summary>
    /// <param name="ddsImage">The DirectXTexNet DDS image.</param>
    /// <param name="flags">(Optional) The mode in which the DirectXTexNet will load the .dds file. If not provided, it defaults to NONE.</param>
    /// <returns>The DDS sections</returns>
    public static ImageSection[] GetDDSImageSections(
        ScratchImage ddsImage,
        DDSFlags flags = DDSFlags.None
    )
    {
        List<ImageSection> sections = [];

        if (flags == DDSFlags.ForceDx9Legacy)
        {
            sections.Add(new() { Pixels = GetDDSHeaderBytes(ddsImage) });
        }

        Image[] images = GetImages(ddsImage);

        for (int i = 0; i < images.Length; i++)
        {
            byte[] pixels = new byte[images[i].SlicePitch];

            Marshal.Copy((nint)images[i].Pixels, pixels, 0, pixels.Length);

            sections.Add(
                new()
                {
                    Width = (nuint)images[i].Width,
                    Height = (nuint)images[i].Height,
                    Format = (DXGIFormat)images[i].Format,
                    SlicePitch = (nuint)images[i].SlicePitch,
                    RowPitch = (nuint)images[i].RowPitch,
                    Pixels = pixels,
                }
            );
        }

        for (int i = 0; i < sections.Count; i++)
        {
            Console.WriteLine(
                $"Image {i} - Width: {sections[i].Width}, Height: {sections[i].Height}, Format: {sections[i].Format}, SlicePitch: {sections[i].SlicePitch}, RowPitch: {sections[i].RowPitch}"
            );
            Console.WriteLine($"Image {i} - Pixels: {sections[i].Pixels.Length}");
        }

        return sections.ToArray();
    }

    public static byte[] GetDDSHeaderBytes(ScratchImage image, DDSFlags flags = DDSFlags.None)
    {
        Blob blob = DirectXTex.CreateBlob();
        TexMetadata metadata = image.GetMetadata();
        DirectXTex
            .SaveToDDSMemory2(
                image.GetImages(),
                image.GetImageCount(),
                ref metadata,
                flags,
                ref blob
            )
            .ThrowIf();

        blob.GetBufferPointer();

        // Extract the DDS header from the blob
        byte[] ddsHeaderCheckDX10 = new byte[148];
        Marshal.Copy(
            (nint)blob.GetBufferPointer(),
            ddsHeaderCheckDX10,
            0,
            ddsHeaderCheckDX10.Length
        );
        blob.Release();

        byte[] ddsHeader;
        int size;
        if (
            ddsHeaderCheckDX10[84] == 0x44
            && ddsHeaderCheckDX10[85] == 0x58
            && ddsHeaderCheckDX10[86] == 0x31
            && ddsHeaderCheckDX10[87] == 0x30
        )
        {
            size = 148;
        }
        else
        {
            size = 128;
        }

        ddsHeader = new byte[size];
        Array.Copy(ddsHeaderCheckDX10, 0, ddsHeader, 0, size);

        return ddsHeader;
    }

    private static Image[] GetImages(ScratchImage image)
    {
        Image* pointerImages = DirectXTex.GetImages(image);

        int imageCount = (int)image.GetImageCount();

        Image[] images = new Image[imageCount];

        for (int i = 0; i < imageCount; i++)
        {
            images[i] = pointerImages[i];
        }

        return images;
    }

    public static unsafe void ReverseChannels(
        Vector4* outPixels,
        Vector4* inPixels,
        nuint width,
        nuint y
    )
    {
        for (ulong j = 0; j < width; ++j)
        {
            Vector4 value = inPixels[j];

            outPixels[j].X = value.W;
            outPixels[j].Y = value.Z;
            outPixels[j].Z = value.Y;
            outPixels[j].W = value.X;
        }
    }

    public static unsafe void RemoveZ(Vector4* outPixels, Vector4* inPixels, nuint width, nuint y)
    {
        for (ulong j = 0; j < width; ++j)
        {
            Vector2 NormalXY = new(inPixels[j].X, inPixels[j].Y);

            outPixels[j] = new Vector4(inPixels[j].X, inPixels[j].Y, 0, 0);
        }
    }

    public static unsafe void RestoreZ(Vector4* outPixels, Vector4* inPixels, nuint width, nuint y)
    {
        for (ulong j = 0; j < width; ++j)
        {
            Vector2 NormalXY = new(inPixels[j].X, inPixels[j].Y);

            NormalXY = NormalXY * 2.0f - Vector2.One;
            float NormalZ = (float)
                Math.Sqrt(Math.Clamp(1.0f - Vector2.Dot(NormalXY, NormalXY), 0, 1));

            outPixels[j] = new Vector4(inPixels[j].X, inPixels[j].Y, NormalZ, 1.0f);
        }
    }

    public static unsafe void DefaultCopy(
        Vector4* outPixels,
        Vector4* inPixels,
        nuint width,
        nuint y
    )
    {
        for (ulong j = 0; j < width; ++j)
        {
            outPixels[j] = inPixels[j];
        }
    }
}

/// <summary>
/// Main Texture Class
/// </summary>
public unsafe partial class Texture
{
    public Hexa.NET.DirectXTex.TexMetadata Metadata { get; set; }
    public ImageAdvancedOptions CurrentOptions { get; set; } = new ImageAdvancedOptions();
    public TextureType TextureType { get; set; } = TextureType.Unknown;

    private ScratchImage Image { get; set; }
    private ScratchImage OriginalImage { get; set; }
    private string FilePath { get; set; } = string.Empty;

    public Texture() { }

    public Texture(string filePath, TextureType textureType, DDSFlags flags = DDSFlags.None)
    {
        Initialize(filePath, textureType, flags);
        FilePath = filePath;
        TextureType = textureType;
    }

    private void InitializeSingleScratchImage(
        byte[] ddsData,
        bool isCopy,
        DDSFlags flags = DDSFlags.None
    )
    {
        ScratchImage Image = DirectXTex.CreateScratchImage();

        Span<byte> src = new(ddsData);
        Blob blob = DirectXTex.CreateBlob();
        Hexa.NET.DirectXTex.TexMetadata meta = new();

        fixed (byte* srcPtr = src)
        {
            DirectXTex
                .LoadFromDDSMemory(srcPtr, (nuint)src.Length, flags, ref meta, ref Image)
                .ThrowIf();
        }

        if (isCopy)
        {
            this.Image = Image;
            Metadata = meta;
        }
        else
        {
            OriginalImage = Image;
        }

        blob.Release();
    }

    public void Compress(DXGIFormat format = DXGIFormat.UNKNOWN)
    {
        if (!DirectXTex.IsCompressed((int)format))
        {
            return;
        }

        ScratchImage transformedImage = DirectXTex.CreateScratchImage();

        if (
            !DirectXTex.IsCompressed(Image.GetMetadata().Format)
            && (int)format != Image.GetMetadata().Format
        )
        {
            TexMetadata originalMetadata = Image.GetMetadata();

            TexCompressFlags flags = TexCompressFlags.Default;

            if (DirectXTex.IsSRGB(Image.GetMetadata().Format))
            {
                flags |= TexCompressFlags.SrgbOut;
            }

            if (DirectXTex.IsSRGB((int)format))
            {
                flags |= TexCompressFlags.SrgbIn;
            }

            DirectXTex
                .Compress2(
                    Image.GetImages(),
                    Image.GetImageCount(),
                    ref originalMetadata,
                    (int)format,
                    flags,
                    0.5f,
                    ref transformedImage
                )
                .ThrowIf();

            Image.Release();
            Image = transformedImage;
        }
        else
        {
            transformedImage.Release();
        }
    }

    public void Decompress(DXGIFormat format = DXGIFormat.UNKNOWN)
    {
        if (DirectXTex.IsCompressed((int)format))
        {
            throw new Exception("Invalid format!");
        }

        ScratchImage transformedImage = DirectXTex.CreateScratchImage();

        if (
            DirectXTex.IsCompressed(Image.GetMetadata().Format)
            && (int)format != Image.GetMetadata().Format
        )
        {
            TexMetadata originalMetadata = Image.GetMetadata();

            DirectXTex
                .Decompress2(
                    Image.GetImages(),
                    Image.GetImageCount(),
                    ref originalMetadata,
                    (int)format,
                    ref transformedImage
                )
                .ThrowIf();

            Image.Release();
            Image = transformedImage;
        }
        else
        {
            transformedImage.Release();
        }
    }

    public void TransformImage(ImageEffect conversionMode = ImageEffect.DEFAULT)
    {
        Decompress();

        TransformImageFunc transformFunction = DefaultCopy;

        if (conversionMode == ImageEffect.RESTORE_Z)
        {
            transformFunction = RestoreZ;
        }
        else if (conversionMode == ImageEffect.REMOVE_Z)
        {
            transformFunction = RemoveZ;
        }
        else if (conversionMode == ImageEffect.SWIZZLE_ABGR)
        {
            transformFunction = ReverseChannels;
        }

        ScratchImage transformedImage = DirectXTex.CreateScratchImage();
        TexMetadata texMetadata = Image.GetMetadata();

        DirectXTex
            .TransformImage2(
                Image.GetImages(),
                Image.GetImageCount(),
                ref texMetadata,
                transformFunction,
                ref transformedImage
            )
            .ThrowIf();

        Image.Release();

        Image = transformedImage;
    }

    public void Initialize(string filePath, TextureType textureType, DDSFlags flags = DDSFlags.None)
    {
        TextureType = textureType;
    }

    private void ResetImageToOriginal()
    {
        if (TextureType != TextureType.D3DTX) { }
        else
        {
            var memory = GetDDSByteArray(OriginalImage);
            InitializeSingleScratchImage(memory, true);
        }
    }

    /// <summary>
    /// Changes the image itself based on the options provided.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="keepOriginal"></param>
    /// <param name="convertingOnly"></param>
    public void TransformTexture(
        ImageAdvancedOptions options,
        bool keepOriginal = false,
        bool convertingOnly = false
    )
    {
        ResetImageToOriginal();

        if (options.EnableSwizzle && options.IsDeswizzle)
        {
            //  Deswizzle(options.PlatformType);
        }

        Decompress();

        if (options.EnableNormalMap)
        {
            GenerateNormalMap();
        }

        if (options.EnableEditing)
        {
            //if (options.ImageEffect != ImageEffect.DEFAULT)
            //{
            //    TransformImage(options.ImageEffect);
            //}
        }

        if (options.EnableTelltaleNormalMap && options.IsTelltaleNormalMap)
        {
            TransformImage(ImageEffect.SWIZZLE_ABGR);
        }

        if (CurrentOptions.EnableNormalMap != options.EnableNormalMap)
        {
            if (!CurrentOptions.EnableNormalMap)
            {
                GenerateNormalMap();
            }
        }

        if (options.EnableMips)
        {
            if (options.AutoGenerateMips)
            {
                GenerateMipMaps(0);
            }
            else if (options.ManualGenerateMips && options.SetMips > 1)
            {
                GenerateMipMaps(Math.Min(options.SetMips, GetMaxMipLevels()));
            }
        }

        if (convertingOnly)
        {
            if (options.EnableAutomaticCompression)
            {
                if (options.EnableNormalMap && options.IsTelltaleXYNormalMap)
                {
                    Compress(DXGIFormat.BC5_UNORM);
                }
                else if (OriginalImage.IsAlphaAllOpaque())
                {
                    if (options.IsSRGB)
                    {
                        Compress(DXGIFormat.BC1_UNORM_SRGB);
                    }
                    else
                    {
                        Compress(DXGIFormat.BC1_UNORM);
                    }
                }
                else
                {
                    if (options.IsSRGB)
                    {
                        Compress(DXGIFormat.BC3_UNORM_SRGB);
                    }
                    else
                    {
                        Compress(DXGIFormat.BC3_UNORM);
                    }
                }
            }
        }
        else if (keepOriginal)
        {
            Compress((DXGIFormat)OriginalImage.GetMetadata().Format);
        }

        if (options.EnableSwizzle && options.IsSwizzle)
        {
            // Swizzle(options.PlatformType);
        }

        CurrentOptions = new(options);
    }

    public uint GetMaxMipLevels()
    {
        uint width = (uint)Metadata.Width;
        uint height = (uint)Metadata.Height;
        uint depth = (uint)Metadata.Depth;

        uint mipLevels = 1;

        while (width > 1 || height > 1 || depth > 1)
        {
            width = Math.Max(1, width / 2);
            height = Math.Max(1, height / 2);
            depth = Math.Max(1, depth / 2);

            mipLevels++;
        }

        return mipLevels;
    }

    public void GenerateNormalMap()
    {
        if (DirectXTex.IsCompressed(Image.GetMetadata().Format))
            Decompress();

        ScratchImage destImage = DirectXTex.CreateScratchImage();
        TexMetadata metadata = Image.GetMetadata();

        DirectXTex
            .ComputeNormalMap2(
                Image.GetImages(),
                Image.GetImageCount(),
                ref metadata,
                CNMAPFlags.Default,
                7,
                metadata.Format,
                ref destImage
            )
            .ThrowIf();

        Image.Release();
        Image = destImage;
    }

    public void GenerateMipMaps(uint mipLevels = 0)
    {
        Decompress();

        ScratchImage destImage = DirectXTex.CreateScratchImage();
        TexMetadata metadata = Image.GetMetadata();

        if (metadata.IsVolumemap())
        {
            DirectXTex
                .GenerateMipMaps3D2(
                    Image.GetImages(),
                    Image.GetImageCount(),
                    TexFilterFlags.Default,
                    mipLevels,
                    ref destImage
                )
                .ThrowIf();
        }
        else
        {
            DirectXTex
                .GenerateMipMaps2(
                    Image.GetImages(),
                    Image.GetImageCount(),
                    ref metadata,
                    TexFilterFlags.Default,
                    mipLevels,
                    ref destImage
                )
                .ThrowIf();
        }

        Console.WriteLine("Mipmaps generated" + destImage.GetImageCount());
        Image.Release();
        Image = destImage;
    }

    public void GetDDSInformation(
        out D3DTXMetadata metadata,
        out ImageSection[] sections,
        DDSFlags flags = DDSFlags.None
    )
    {
        metadata = GetTextureInformation(Image.GetMetadata());
        sections = GetDDSImageSections(Image, flags);
    }

    public void Release()
    {
        if (!Image.IsNull)
        {
            Image.Release();
        }
        if (!OriginalImage.IsNull)
        {
            OriginalImage.Release();
        }
    }
}
