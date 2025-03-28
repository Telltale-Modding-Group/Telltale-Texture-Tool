using System;
using System.Runtime.InteropServices;
using Hexa.NET.DirectXTex;
using TelltaleTextureTool.Codecs;
using TelltaleTextureTool.DirectX.Enums;
using DirectXImage = Hexa.NET.DirectXTex.Image;
using DirectXTexDimension = Hexa.NET.DirectXTex.TexDimension;
using DirectXTexMetadata = Hexa.NET.DirectXTex.TexMetadata;

namespace TelltaleTextureTool.Graphics.Plugins;

public static class DirectXTexUtility
{
    public static Texture LoadFromScratchImage(ScratchImage scratchImage)
    {
        return new Texture
        {
            Images = InitImagesFromScratchImage(scratchImage),
            Metadata = InitTexMetadataFromScratchImage(scratchImage),
        };
    }

    public static unsafe Image[] InitImagesFromScratchImage(ScratchImage scratchImage)
    {
        DirectXImage* scratchImages = DirectXTex.GetImages(scratchImage);

        int imageCount = (int)scratchImage.GetImageCount();

        Image[] images = new Image[imageCount];

        for (int i = 0; i < imageCount; i++)
        {
            byte[] pixels = GetPixelsFromDirectXImage(scratchImages[i]);

            images[i] = new Image
            {
                Width = (uint)scratchImages[i].Width,
                Height = (uint)scratchImages[i].Height,
                RowPitch = (uint)scratchImages[i].RowPitch,
                SlicePitch = (uint)scratchImages[i].SlicePitch,
                PixelFormatInfo = GetPixelFormatInfo((DXGIFormat)scratchImages[i].Format),
                Pixels = pixels,
            };
        }

        return images;
    }

    public static TexMetadata InitTexMetadataFromScratchImage(ScratchImage scratchImage)
    {
        DirectXTexMetadata metadata = scratchImage.GetMetadata();

        return new()
        {
            Width = (uint)metadata.Width,
            Height = (uint)metadata.Height,
            Depth = (uint)metadata.Depth,
            ArraySize = (uint)metadata.ArraySize,
            MipLevels = (uint)metadata.MipLevels,
            PixelFormatInfo = GetPixelFormatInfo((DXGIFormat)metadata.Format),
            Dimension = GetTexDimensionFromDirectXTexDimension(metadata.Dimension),
            IsPremultipliedAlpha = metadata.IsPMAlpha(),
            IsCubemap = metadata.IsCubemap(),
            IsVolumemap = metadata.IsVolumemap(),
        };
    }

    public static unsafe ScratchImage CreateScratchImageFromTexture(Texture texture)
    {
        var metadata = texture.Metadata;

        var dxMetadata = new DirectXTexMetadata
        {
            Width = metadata.Width,
            Height = metadata.Height,
            Depth = metadata.Depth,
            ArraySize = metadata.ArraySize,
            MipLevels = metadata.MipLevels,
            Format = (int)GetDXGIFormat(metadata.PixelFormatInfo),
            Dimension = GetDirectXTexDimension(metadata.Dimension),
            MiscFlags = (uint)(metadata.IsPremultipliedAlpha ? 0x2 : 0x0),
            MiscFlags2 = (uint)(metadata.IsCubemap ? 0x4 : 0x0),
        };

        var scratchImage = DirectXTex.CreateScratchImage();

        scratchImage.Initialize(ref dxMetadata, CPFlags.None).ThrowIf();

        var dxImages = scratchImage.GetImages();

        try
        {
            for (int i = 0; i < texture.Images.Length; i++)
            {
                var image = texture.Images[i];
                var dxImage = dxImages[i];

                if (image.Width != image.Width || image.Height != image.Height)
                    throw new InvalidOperationException("Mip level dimensions mismatch");

                fixed (byte* pSrc = image.Pixels)
                {
                    Buffer.MemoryCopy(
                        pSrc,
                        dxImage.Pixels,
                        (long)dxImage.SlicePitch,
                        (long)dxImage.SlicePitch
                    );
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return scratchImage;
    }

    public static unsafe DirectXImage GetDirectXImage(Image image)
    {
        var dxgiFormat = GetDXGIFormat(image.PixelFormatInfo);

        var (rowPitch, slicePitch) = PixelFormatUtility.ComputePitch(
            image.PixelFormatInfo.PixelFormat,
            image.Width,
            image.Height
        );

        fixed (byte* pixels = image.Pixels)
        {
            return new DirectXImage(
                image.Width,
                image.Height,
                (int)dxgiFormat,
                rowPitch,
                slicePitch,
                pixels
            );
        }
    }

    public static unsafe Image DecompressImage(Image image, PixelFormatInfo pixelFormatInfo)
    {
        if (!PixelFormatUtility.IsFormatCompressed(image.PixelFormatInfo.PixelFormat))
        {
            throw new InvalidOperationException("Image is not compressed");
        }

        var dxImage = GetDirectXImage(image);
        var dxgiFormat = GetDXGIFormat(pixelFormatInfo);
        ScratchImage scratchImage = DirectXTex.CreateScratchImage();
        try
        {
            DirectXTex.Decompress(&dxImage, (int)dxgiFormat, &scratchImage).ThrowIf();

            var pixels = GetPixelsFromDirectXScratchImage(scratchImage);

            var newPixelFormatInfo = GetPixelFormatInfo(
                (DXGIFormat)scratchImage.GetMetadata().Format
            );

            return Image.GetImage(image.Width, image.Height, newPixelFormatInfo, pixels);
        }
        finally
        {
            scratchImage.Release();
        }
    }

    public static unsafe Image CompressImage(PixelFormatInfo pixelFormatInfo, Image image)
    {
        if (PixelFormatUtility.IsFormatCompressed(image.PixelFormatInfo.PixelFormat))
        {
            throw new InvalidOperationException("Image is already compressed");
        }

        var dxImage = GetDirectXImage(image);
        var dxgiFormat = GetDXGIFormat(pixelFormatInfo);
        var scratchImage = DirectXTex.CreateScratchImage();

        DirectXTex
            .Compress(&dxImage, (int)dxgiFormat, TexCompressFlags.Default, 0.5f, &scratchImage)
            .ThrowIf();

        var pixels = GetPixelsFromDirectXScratchImage(scratchImage);

        var newPixelFormatInfo = GetPixelFormatInfo((DXGIFormat)scratchImage.GetMetadata().Format);

        scratchImage.Release();

        return Image.GetImage(image.Width, image.Height, newPixelFormatInfo, pixels);
    }

    public static unsafe Image ConvertImage(PixelFormatInfo pixelFormatInfo, Image image)
    {
        if (image.PixelFormatInfo.Equals(pixelFormatInfo))
        {
            return image;
        }

        var dxImage = GetDirectXImage(image);
        var dxgiFormat = GetDXGIFormat(pixelFormatInfo);
        var scratchImage = DirectXTex.CreateScratchImage();

        TexFilterFlags filterFlags = TexFilterFlags.Default;
        // filterFlags |= TexFilterFlags.ForceWic;

        DirectXTex.Convert(&dxImage, (int)dxgiFormat, filterFlags, 0.5f, &scratchImage).ThrowIf();

        var pixels = GetPixelsFromDirectXScratchImage(scratchImage);

        var newPixelFormatInfo = GetPixelFormatInfo((DXGIFormat)scratchImage.GetMetadata().Format);

        scratchImage.Release();

        return Image.GetImage(image.Width, image.Height, newPixelFormatInfo, pixels);
    }

    public static unsafe byte[] GetPixelsFromDirectXScratchImage(ScratchImage scratchImage)
    {
        return GetPixelsFromDirectXImage(scratchImage.GetImage(0, 0, 0)[0]);
    }

    public static unsafe byte[] GetPixelsFromDirectXImage(DirectXImage image)
    {
        var pixels = new byte[image.SlicePitch];
        Marshal.Copy((nint)image.Pixels, pixels, 0, (int)image.SlicePitch);

        return pixels;
    }

    // static void GenerateMipMaps(Texture texture, int maxMips)
    // {
    //     if (PixelFormatUtility.IsCompressed(texture.Metadata.PixelFormatInfo.PixelFormat))
    //     {
    //         throw new InvalidOperationException("Cannot generate mipmaps for compressed image");
    //     }

    //     var dxImage = GetDirectXImage(image);
    //     var dxgiFormat = GetDXGIFormat(pixelFormatInfo);
    //     var scratchImage = DirectXTex.CreateScratchImage();

    //     DirectXTex.GenerateMipMaps(texture, maxMips);
    // }



    public static unsafe byte[] GetBytes(
        Texture texture,
        TextureType type,
        CodecOptions codecOptions
    )
    {
        ScratchImage ddsScratchImage = DirectXTex.CreateScratchImage();
        var texMetadata = texture.Metadata;

        try
        {
            DirectXTexMetadata metadata = new()
            {
                Width = texMetadata.Width,
                Height = texMetadata.Height,
                ArraySize = texMetadata.ArraySize,
                Depth = texMetadata.Depth,
                MipLevels = texMetadata.MipLevels,
                Format = (int)GetDXGIFormat(texMetadata.PixelFormatInfo),
                Dimension = GetDirectXTexDimension(texMetadata.Dimension),
                MiscFlags = (uint)(texMetadata.IsCubemap ? 0x4 : 0x0),
                MiscFlags2 = (uint)(texMetadata.IsPremultipliedAlpha ? 0x2 : 0x0),
            };

            ddsScratchImage.Initialize(ref metadata, CPFlags.None).ThrowIf();

            DirectXImage* dxImages = DirectXTex.GetImages(ddsScratchImage);

            for (nuint i = 0; i < ddsScratchImage.GetImageCount(); i++)
            {
                Image image = texture.Images[i];
                DirectXImage dxImage = GetDirectXImage(image);

                dxImages[i] = dxImage;
            }

            DDSFlags flags = DDSFlags.None;

            if (codecOptions.ForceDx9Legacy)
            {
                flags |= DDSFlags.LegacyDword;
            }

            byte[] ddsData = GetDDSBlobBytes(ddsScratchImage, type, flags);

            return ddsData;
        }
        finally
        {
            ddsScratchImage.Release();
        }
    }

    public static byte[] GetDDSHeaderBytes(ScratchImage image, DDSFlags flags = DDSFlags.None)
    {
        var bytes = GetDDSBlobBytes(image, TextureType.DDS, flags);

        var headerSize =
            bytes[84] == 'D' && bytes[85] == 'X' && bytes[86] == '1' && bytes[87] == '0'
                ? 148
                : 128;

        byte[] ddsHeader = new byte[headerSize];

        Array.Copy(bytes, 0, ddsHeader, 0, headerSize);

        return ddsHeader;
    }

    public static unsafe byte[] GetDDSBlobBytes(
        ScratchImage image,
        TextureType type,
        DDSFlags flags = DDSFlags.None
    )
    {
        Blob blob = DirectXTex.CreateBlob();
        try
        {
            DirectXTexMetadata metadata = image.GetMetadata();
            DirectXTex
                .SaveToDDSMemory2(
                    image.GetImages(),
                    image.GetImageCount(),
                    ref metadata,
                    flags,
                    ref blob
                )
                .ThrowIf();

            return GetBytesFromBlob(blob);
        }
        finally
        {
            blob.Release();
        }
    }

    public static unsafe byte[] GetBytesFromBlob(Blob blob)
    {
        byte[] blobBytes = new byte[blob.GetBufferSize()];
        Marshal.Copy((nint)blob.GetBufferPointer(), blobBytes, 0, blobBytes.Length);

        return blobBytes;
    }

    public static unsafe void SaveDependingOnTextureType(
        ScratchImage image,
        TextureType type,
        CodecOptions options,
        ref Blob blob
    )
    {
        DirectXTexMetadata metadata = image.GetMetadata();

        DDSFlags flags = DDSFlags.None;

        if (options.ForceDx9Legacy)
        {
            flags |= DDSFlags.LegacyDword;
        }

        switch (type)
        {
            case TextureType.DDS:
                DirectXTex
                    .SaveToDDSMemory2(
                        image.GetImages(),
                        image.GetImageCount(),
                        ref metadata,
                        flags,
                        ref blob
                    )
                    .ThrowIf();
                break;
            case TextureType.TGA:
                DirectXTex
                    .SaveToTGAMemory(image.GetImages(), TGAFlags.None, ref blob, ref metadata)
                    .ThrowIf();
                break;
            case TextureType.PNG:
                throw new NotImplementedException();
            case TextureType.JPEG:
                throw new NotImplementedException();
            default:
                throw new NotImplementedException();
        }
    }

    public static PixelFormatInfo GetPixelFormatInfo(DXGIFormat dxgiFormat)
    {
        var pixelFormat = dxgiFormat switch
        {
            DXGIFormat.R32G32B32A32_TYPELESS
            or DXGIFormat.R32G32B32A32_FLOAT
            or DXGIFormat.R32G32B32A32_UINT
            or DXGIFormat.R32G32B32A32_SINT => PixelFormat.R32G32B32A32,

            DXGIFormat.R32G32B32_TYPELESS
            or DXGIFormat.R32G32B32_FLOAT
            or DXGIFormat.R32G32B32_UINT
            or DXGIFormat.R32G32B32_SINT => PixelFormat.R32G32B32,

            DXGIFormat.R16G16B16A16_TYPELESS
            or DXGIFormat.R16G16B16A16_FLOAT
            or DXGIFormat.R16G16B16A16_UNORM
            or DXGIFormat.R16G16B16A16_UINT
            or DXGIFormat.R16G16B16A16_SNORM
            or DXGIFormat.R16G16B16A16_SINT => PixelFormat.R16G16B16A16,

            DXGIFormat.R32G32_TYPELESS
            or DXGIFormat.R32G32_FLOAT
            or DXGIFormat.R32G32_UINT
            or DXGIFormat.R32G32_SINT => PixelFormat.R32G32,

            DXGIFormat.R10G10B10A2_TYPELESS
            or DXGIFormat.R10G10B10A2_UNORM
            or DXGIFormat.R10G10B10A2_UINT => PixelFormat.R10G10B10A2,

            DXGIFormat.R11G11B10_FLOAT => PixelFormat.R11G11B10,
            DXGIFormat.R9G9B9E5_SHAREDEXP => PixelFormat.R9G9B9E5,

            DXGIFormat.R8G8B8A8_TYPELESS
            or DXGIFormat.R8G8B8A8_UNORM
            or DXGIFormat.R8G8B8A8_UNORM_SRGB
            or DXGIFormat.R8G8B8A8_UINT
            or DXGIFormat.R8G8B8A8_SNORM
            or DXGIFormat.R8G8B8A8_SINT => PixelFormat.R8G8B8A8,

            DXGIFormat.B8G8R8A8_TYPELESS
            or DXGIFormat.B8G8R8A8_UNORM
            or DXGIFormat.B8G8R8A8_UNORM_SRGB => PixelFormat.B8G8R8A8,

            DXGIFormat.B8G8R8X8_TYPELESS
            or DXGIFormat.B8G8R8X8_UNORM
            or DXGIFormat.B8G8R8X8_UNORM_SRGB => PixelFormat.B8G8R8X8,

            DXGIFormat.R16G16_TYPELESS
            or DXGIFormat.R16G16_FLOAT
            or DXGIFormat.R16G16_UNORM
            or DXGIFormat.R16G16_UINT
            or DXGIFormat.R16G16_SNORM
            or DXGIFormat.R16G16_SINT => PixelFormat.R16G16,

            DXGIFormat.R32_TYPELESS
            or DXGIFormat.R32_FLOAT
            or DXGIFormat.R32_UINT
            or DXGIFormat.R32_SINT => PixelFormat.R32,

            DXGIFormat.R8G8_TYPELESS
            or DXGIFormat.R8G8_UNORM
            or DXGIFormat.R8G8_UINT
            or DXGIFormat.R8G8_SNORM
            or DXGIFormat.R8G8_SINT => PixelFormat.R8G8,

            DXGIFormat.R16_TYPELESS
            or DXGIFormat.R16_FLOAT
            or DXGIFormat.R16_UNORM
            or DXGIFormat.R16_UINT
            or DXGIFormat.R16_SNORM
            or DXGIFormat.R16_SINT => PixelFormat.R16,

            DXGIFormat.R8_TYPELESS
            or DXGIFormat.R8_UNORM
            or DXGIFormat.R8_UINT
            or DXGIFormat.R8_SNORM
            or DXGIFormat.R8_SINT => PixelFormat.R8,

            DXGIFormat.A8_UNORM => PixelFormat.A8,

            DXGIFormat.R1_UNORM => PixelFormat.R1,

            DXGIFormat.BC1_TYPELESS or DXGIFormat.BC1_UNORM or DXGIFormat.BC1_UNORM_SRGB =>
                PixelFormat.BC1,

            DXGIFormat.BC2_TYPELESS or DXGIFormat.BC2_UNORM or DXGIFormat.BC2_UNORM_SRGB =>
                PixelFormat.BC2,

            DXGIFormat.BC3_TYPELESS or DXGIFormat.BC3_UNORM or DXGIFormat.BC3_UNORM_SRGB =>
                PixelFormat.BC3,

            DXGIFormat.BC4_TYPELESS or DXGIFormat.BC4_UNORM or DXGIFormat.BC4_SNORM =>
                PixelFormat.BC4,

            DXGIFormat.BC5_TYPELESS or DXGIFormat.BC5_UNORM or DXGIFormat.BC5_SNORM =>
                PixelFormat.BC5,

            DXGIFormat.BC6H_TYPELESS or DXGIFormat.BC6H_UF16 or DXGIFormat.BC6H_SF16 =>
                PixelFormat.BC6H,

            DXGIFormat.BC7_TYPELESS or DXGIFormat.BC7_UNORM or DXGIFormat.BC7_UNORM_SRGB =>
                PixelFormat.BC7,

            DXGIFormat.B5G5R5A1_UNORM => PixelFormat.B5G5R5A1,

            DXGIFormat.B5G6R5_UNORM => PixelFormat.B5G6R5,

            DXGIFormat.B4G4R4A4_UNORM => PixelFormat.B4G4R4A4,

            DXGIFormat.A4B4G4R4_UNORM => PixelFormat.A4B4G4R4,

            _ => PixelFormat.Unknown,
        };

        var dataType = DirectXTex.FormatDataType((int)dxgiFormat) switch
        {
            FormatType.Unorm => DataType.Unorm,
            FormatType.Snorm => DataType.Snorm,
            FormatType.Uint => DataType.Uint,
            FormatType.Sint => DataType.Sint,
            FormatType.Float => DataType.Float,
            _ => DataType.Unknown,
        };

        var colorSpace = DirectXTex.IsSRGB((int)dxgiFormat) ? ColorSpace.sRGB : ColorSpace.Linear;

        return new PixelFormatInfo(pixelFormat, dataType, colorSpace);
    }

    public static DXGIFormat GetDXGIFormat(PixelFormatInfo pixelFormatInfo)
    {
        DXGIFormat dxgiFormat = pixelFormatInfo.PixelFormat switch
        {
            PixelFormat.R32G32B32A32 => DXGIFormat.R32G32B32A32_TYPELESS,
            PixelFormat.R32G32B32 => DXGIFormat.R32G32B32_TYPELESS,
            PixelFormat.R16G16B16A16 => DXGIFormat.R16G16B16A16_TYPELESS,
            PixelFormat.R32G32 => DXGIFormat.R32G32_TYPELESS,
            PixelFormat.R10G10B10A2 => DXGIFormat.R10G10B10A2_TYPELESS,
            PixelFormat.R11G11B10 => DXGIFormat.R11G11B10_FLOAT,
            PixelFormat.R9G9B9E5 => DXGIFormat.R9G9B9E5_SHAREDEXP,
            PixelFormat.R8G8B8A8 => DXGIFormat.R8G8B8A8_TYPELESS,
            PixelFormat.B8G8R8A8 => DXGIFormat.B8G8R8A8_TYPELESS,
            PixelFormat.B8G8R8X8 => DXGIFormat.B8G8R8X8_TYPELESS,
            PixelFormat.R16G16 => DXGIFormat.R16G16_TYPELESS,
            PixelFormat.R32 => DXGIFormat.R32_TYPELESS,
            PixelFormat.R8G8 => DXGIFormat.R8G8_TYPELESS,
            PixelFormat.R16 => DXGIFormat.R16_TYPELESS,
            PixelFormat.R8 => DXGIFormat.R8_TYPELESS,
            PixelFormat.A8 => DXGIFormat.A8_UNORM,
            PixelFormat.R1 => DXGIFormat.R1_UNORM,
            PixelFormat.BC1 => DXGIFormat.BC1_TYPELESS,
            PixelFormat.BC2 => DXGIFormat.BC2_TYPELESS,
            PixelFormat.BC3 => DXGIFormat.BC3_TYPELESS,
            PixelFormat.BC4 => DXGIFormat.BC4_TYPELESS,
            PixelFormat.BC5 => DXGIFormat.BC5_TYPELESS,
            PixelFormat.BC6H => DXGIFormat.BC6H_TYPELESS,
            PixelFormat.BC7 => DXGIFormat.BC7_TYPELESS,
            PixelFormat.B5G5R5A1 => DXGIFormat.B5G5R5A1_UNORM,
            PixelFormat.B5G6R5 => DXGIFormat.B5G6R5_UNORM,
            PixelFormat.B4G4R4A4 => DXGIFormat.B4G4R4A4_UNORM,
            PixelFormat.A4B4G4R4 => DXGIFormat.A4B4G4R4_UNORM,

            _ => DXGIFormat.UNKNOWN,
        };

        return ConvertTypeless(pixelFormatInfo, dxgiFormat);
    }

    public static DXGIFormat ConvertTypeless(PixelFormatInfo pixelFormatInfo, DXGIFormat dxgiFormat)
    {
        dxgiFormat = pixelFormatInfo.DataType switch
        {
            DataType.Snorm => MakeTypelessSNORM(dxgiFormat),
            DataType.Unorm => (DXGIFormat)DirectXTex.MakeTypelessUNORM((int)dxgiFormat),
            DataType.Sint => MakeTypelessSINT(dxgiFormat),
            DataType.Uint => MakeTypelessSUINT(dxgiFormat),
            DataType.Float => (DXGIFormat)DirectXTex.MakeTypelessFLOAT((int)dxgiFormat),
            _ => dxgiFormat,
        };

        dxgiFormat = pixelFormatInfo.ColorSpace switch
        {
            ColorSpace.sRGB => (DXGIFormat)DirectXTex.MakeSRGB((int)dxgiFormat),
            _ => dxgiFormat,
        };

        return dxgiFormat;
    }

    public static DXGIFormat MakeTypelessSNORM(DXGIFormat dxgiFormat)
    {
        return dxgiFormat switch
        {
            DXGIFormat.R16G16B16A16_TYPELESS => DXGIFormat.R16G16B16A16_SNORM,
            DXGIFormat.R16G16_TYPELESS => DXGIFormat.R16G16_SNORM,
            DXGIFormat.R8G8_TYPELESS => DXGIFormat.R8G8_SNORM,
            DXGIFormat.R16_TYPELESS => DXGIFormat.R16_SNORM,
            DXGIFormat.R8_TYPELESS => DXGIFormat.R8_SNORM,
            DXGIFormat.BC4_TYPELESS => DXGIFormat.BC4_SNORM,
            DXGIFormat.BC5_TYPELESS => DXGIFormat.BC5_SNORM,
            _ => dxgiFormat,
        };
    }

    public static DXGIFormat MakeTypelessSINT(DXGIFormat dxgiFormat)
    {
        return dxgiFormat switch
        {
            DXGIFormat.R32G32B32A32_TYPELESS => DXGIFormat.R32G32B32A32_SINT,
            DXGIFormat.R32G32B32_TYPELESS => DXGIFormat.R32G32B32_SINT,
            DXGIFormat.R16G16B16A16_TYPELESS => DXGIFormat.R16G16B16A16_SINT,
            DXGIFormat.R32G32_TYPELESS => DXGIFormat.R32G32_SINT,
            DXGIFormat.R16G16_TYPELESS => DXGIFormat.R16G16_SINT,
            DXGIFormat.R32_TYPELESS => DXGIFormat.R32_SINT,
            DXGIFormat.R8G8_TYPELESS => DXGIFormat.R8G8_SINT,
            DXGIFormat.R16_TYPELESS => DXGIFormat.R16_SINT,
            DXGIFormat.R8_TYPELESS => DXGIFormat.R8_SINT,
            DXGIFormat.R8G8B8A8_TYPELESS => DXGIFormat.R8G8B8A8_SINT,
            _ => dxgiFormat,
        };
    }

    public static DXGIFormat MakeTypelessSUINT(DXGIFormat dxgiFormat)
    {
        return dxgiFormat switch
        {
            DXGIFormat.R32G32B32A32_TYPELESS => DXGIFormat.R32G32B32A32_UINT,
            DXGIFormat.R32G32B32_TYPELESS => DXGIFormat.R32G32B32_UINT,
            DXGIFormat.R16G16B16A16_TYPELESS => DXGIFormat.R16G16B16A16_UINT,
            DXGIFormat.R32G32_TYPELESS => DXGIFormat.R32G32_UINT,
            DXGIFormat.R16G16_TYPELESS => DXGIFormat.R16G16_UINT,
            DXGIFormat.R32_TYPELESS => DXGIFormat.R32_UINT,
            DXGIFormat.R8G8_TYPELESS => DXGIFormat.R8G8_UINT,
            DXGIFormat.R16_TYPELESS => DXGIFormat.R16_UINT,
            DXGIFormat.R8_TYPELESS => DXGIFormat.R8_UINT,
            DXGIFormat.R8G8B8A8_TYPELESS => DXGIFormat.R8G8B8A8_UINT,
            DXGIFormat.R10G10B10A2_TYPELESS => DXGIFormat.R10G10B10A2_UINT,
            _ => dxgiFormat,
        };
    }

    public static TexDimension GetTexDimensionFromDirectXTexDimension(DirectXTexDimension dimension)
    {
        return dimension switch
        {
            DirectXTexDimension.Texture1D => TexDimension.Tex1D,
            DirectXTexDimension.Texture2D => TexDimension.Tex2D,
            DirectXTexDimension.Texture3D => TexDimension.Tex3D,
            _ => TexDimension.Tex2D,
        };
    }

    public static DirectXTexDimension GetDirectXTexDimension(TexDimension dimension)
    {
        return dimension switch
        {
            TexDimension.Tex1D => DirectXTexDimension.Texture1D,
            TexDimension.Tex2D => DirectXTexDimension.Texture2D,
            TexDimension.Tex3D => DirectXTexDimension.Texture3D,
            _ => DirectXTexDimension.Texture2D,
        };
    }
}
