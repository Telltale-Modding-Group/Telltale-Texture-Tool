using System;
using System.Collections.Generic;
using System.Linq;
using TelltaleTextureTool.Codecs;
using TelltaleTextureTool.Graphics.Plugins;

namespace TelltaleTextureTool.Graphics;

public partial class Texture
{
    public byte[] GetImageRGBA8Pixels(Image image)
    {
        var extraMetadata = Metadata.ExtraMetadata;
        Image newImage = PixelFormatDecoder.DecodeImageToRGBA8(image);
        // Get the normal map

        // if (IsTextureCompressed())
        // {
        //     // Decompress(image);
        // }

        return newImage.Pixels;
    }

    // 140 - 25
    public byte[] GetCubemapImage(uint cubeIndex, uint mip)
    {
        Image faceX = GetImage(mip, cubeIndex * 6, 0);
        Image faceNX = GetImage(mip, cubeIndex * 6 + 1, 0);
        Image faceY = GetImage(mip, cubeIndex * 6 + 2, 0);
        Image faceNY = GetImage(mip, cubeIndex * 6 + 3, 0);
        Image faceZ = GetImage(mip, cubeIndex * 6 + 4, 0);
        Image faceNZ = GetImage(mip, cubeIndex * 6 + 5, 0);

        var faceLayout = new (int cellX, int cellY, byte[] pixels)[]
        {
            (2, 1, GetImageRGBA8Pixels(faceX)), // +X (Right)
            (0, 1, GetImageRGBA8Pixels(faceNX)), // -X (Left)
            (1, 0, GetImageRGBA8Pixels(faceY)), // +Y (Top)
            (1, 2, GetImageRGBA8Pixels(faceNY)), // -Y (Bottom)
            (3, 1, GetImageRGBA8Pixels(faceZ)), // +Z (Front)
            (1, 1, GetImageRGBA8Pixels(faceNZ)), // -Z (Back)
        };

        int faceWidth = (int)faceX.Width;
        int faceHeight = (int)faceX.Height;
        int crossWidth = 4 * faceWidth;
        int crossHeight = 3 * faceHeight;
        byte[] crossPixels = new byte[crossWidth * crossHeight * 4];

        foreach (var (cellX, cellY, pixels) in faceLayout)
        {
            for (int row = 0; row < faceHeight; row++)
            {
                int srcOffset = row * faceWidth * 4;
                int dstOffset = ((cellY * faceHeight + row) * crossWidth + cellX * faceWidth) * 4;

                Buffer.BlockCopy(pixels, srcOffset, crossPixels, dstOffset, faceWidth * 4);
            }
        }

        return crossPixels;
    }

    public byte[] GetRGBA8Pixels(uint mip, uint level, uint slice)
    {
        var image = GetImage(mip, level, slice);
        var extraMetadata = Metadata.ExtraMetadata;
        Image newImage = PixelFormatDecoder.DecodeImageToRGBA8(image);
        // Get the normal map

        // if (IsTextureCompressed())
        // {
        //     // Decompress(image);
        // }

        return newImage.Pixels;
    }

    public void ConvertToRGBA8()
    {
        for (int i = 0; i < Images.Length; i++)
        {
            Images[i] = PixelFormatDecoder.DecodeImageToRGBA8(Images[i]);
        }

        Metadata.PixelFormatInfo = new PixelFormatInfo(
            PixelFormat.R8G8B8A8,
            DataType.Unorm,
            ColorSpace.Linear
        );
    }

    public void ConvertToRGBA32F()
    {
         for (int i = 0; i < Images.Length; i++)
        {
            Images[i] = PixelFormatDecoder.DecodeImageToRGBA32F(Images[i]);
        }

        Metadata.PixelFormatInfo = new PixelFormatInfo(
            PixelFormat.R32G32B32A32,
            DataType.Float,
            ColorSpace.Linear
        );
    }

    public void SwizzleTexture(Platform platform, bool swizzle)
    {
        if (platform == Platform.None)
        {
            return;
        }

        foreach (var image in Images)
        {
            try
            {
                if (!Metadata.IsVolumemap)
                {
                    SwizzleImage(image, platform, swizzle);
                }
                else
                {
                    Image tempImage = new Image
                    {
                        Width = image.Width,
                        Height = image.Height * Metadata.Depth,
                        PixelFormatInfo = image.PixelFormatInfo,
                        Pixels = [],
                        RowPitch = image.RowPitch,
                        SlicePitch = image.SlicePitch * Metadata.Depth,
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public static void SwizzleImage(Image image, Platform platform)
    {
        if (platform == Platform.None)
        {
            return;
        }

        SwizzleImage(image, platform, true);
    }

    public static void DeswizzleImage(Image image, Platform platform)
    {
        if (platform == Platform.None)
        {
            return;
        }

        SwizzleImage(image, platform, false);
    }

    private static void SwizzleImage(Image image, Platform platform, bool swizzle)
    {
        var normalizedPixelFormatInfo = new PixelFormatInfo(
            image.PixelFormatInfo.PixelFormat,
            DataType.Unorm,
            ColorSpace.Linear
        );

        var pixelFormat = (DrSwizzler.DDS.DXEnums.DXGIFormat)
            DirectXTexUtility.GetDXGIFormat(normalizedPixelFormatInfo);

        DrSwizzler.Util.GetsourceBytesPerPixelSetAndPixelSize(
            pixelFormat,
            out int sourceBytesPerPixelSet,
            out int pixelBlockSize,
            out int formatbpp
        );

        Console.WriteLine("Swizzling image..." + image.Pixels.Length);
        image.Pixels = platform switch
        {
            Platform.Switch => swizzle
                ? DrSwizzler.Swizzler.SwitchSwizzle(
                    image.Pixels,
                    (int)image.Width,
                    (int)image.Height,
                    pixelFormat
                )
                : DrSwizzler.Deswizzler.SwitchDeswizzle(
                    image.Pixels,
                    (int)image.Width,
                    (int)image.Height,
                    pixelFormat
                ),
            Platform.PS3 => swizzle
                ? DrSwizzler.Swizzler.PS3Swizzle(
                    image.Pixels,
                    (int)image.Width,
                    (int)image.Height,
                    pixelFormat
                )
                : DrSwizzler.Deswizzler.PS3Deswizzle(
                    image.Pixels,
                    (int)image.Width,
                    (int)image.Height,
                    pixelFormat
                ),
            Platform.PS4 => swizzle
                ? DrSwizzler.Swizzler.PS4Swizzle(
                    image.Pixels,
                    (int)image.Width,
                    (int)image.Height,
                    pixelFormat
                )
                : DrSwizzler.Deswizzler.PS4Deswizzle(
                    image.Pixels,
                    (int)image.Width,
                    (int)image.Height,
                    pixelFormat
                ),
            Platform.Xbox360 => swizzle
                ? DrSwizzler.Swizzler.Xbox360Swizzle(
                    image.Pixels,
                    (int)image.Width,
                    (int)image.Height,
                    pixelFormat
                )
                : DrSwizzler.Deswizzler.Xbox360Deswizzle(
                    image.Pixels,
                    (int)image.Width,
                    (int)image.Height,
                    pixelFormat
                ),
            Platform.PSVita => swizzle
                ? DrSwizzler.Swizzler.VitaSwizzle(
                    image.Pixels,
                    (int)image.Width,
                    (int)image.Height,
                    pixelFormat
                )
                : DrSwizzler.Deswizzler.VitaDeswizzle(
                    image.Pixels,
                    (int)image.Width,
                    (int)image.Height,
                    pixelFormat
                ),

            _ => image.Pixels,
        };

        Console.WriteLine("Swizzled image: " + image.Pixels.Length);
    }

    public void GenerateMips() { }

    public void SetNormalMap() { }

    public uint GetImageIndex(uint mip, uint level, uint slice)
    {
        if (mip >= Metadata.MipLevels)
        {
            throw new ArgumentOutOfRangeException(
                $"Mip index out of bounds: {mip} >= {Metadata.MipLevels}"
            );
        }

        if (Metadata.Dimension is TexDimension.Tex1D or TexDimension.Tex2D)
        {
            if (slice > 0)
            {
                throw new ArgumentOutOfRangeException($"Slice index out of bounds: {slice} >= 0");
            }

            if (level >= Metadata.ArraySize)
            {
                throw new ArgumentOutOfRangeException(
                    $"Array index out of bounds: {level} >= {Metadata.ArraySize}"
                );
            }

            var index = level * Metadata.MipLevels + mip;

            if (index < 0 || index >= Images.Length)
            {
                throw new ArgumentOutOfRangeException(
                    $"Image not found for mip: {mip}, level: {level}, slice: {slice}"
                );
            }

            return index;
        }
        else if (Metadata.Dimension == TexDimension.Tex3D)
        {
            uint index = 0;
            uint depth = Metadata.Depth;

            for (int i = 0; i < mip; i++)
            {
                index += depth;
                if (depth > 1)
                {
                    depth >>= 1; // Divide depth by 2
                }
            }

            if (slice >= depth)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slice),
                    $"Slice index out of bounds: {slice} >= {depth}"
                );
            }

            index += slice;

            if (index < 0 || index >= Images.Length)
            {
                throw new InvalidOperationException(
                    $"Image not found for mip: {mip}, level: {level}, slice: {slice}"
                );
            }

            return index;
        }

        throw new InvalidOperationException("Invalid texture dimension");
    }

    public Image GetImage(uint mip, uint level, uint slice)
    {
        return Images[CalculateImageIndex(mip, level, slice)];
    }

    public bool IsTextureCompressed()
    {
        return PixelFormatUtility.IsFormatCompressed(Metadata.PixelFormatInfo.PixelFormat);
    }

    public static void DecompressImage(Image image, PixelFormatInfo format)
    {
        image = DirectXTexUtility.DecompressImage(image, format);
    }

    public static void CompressImage(Image image, PixelFormatInfo format)
    {
        DirectXTexUtility.CompressImage(format, image);
    }

    // public void GenerateMipMaps(int maxMips = 0)
    // {
    //     DirectXTexUtility.GenerateMipMaps(this, maxMips);
    // }

    public uint GetMaxPossibleImages()
    {
        uint maxImages = 0;

        uint depthCount = Metadata.Depth;

        for (int i = 0; i < Metadata.MipLevels; i++)
        {
            maxImages += Math.Max(1, depthCount);

            depthCount >>= 1; // Divide depth by 2
        }

        maxImages *= Metadata.ArraySize;

        return maxImages;
    }

    public uint GetMaxSliceCountPerArray()
    {
        uint maxSlices = 0;

        uint depthCount = Metadata.Depth;

        for (int i = 0; i < Metadata.MipLevels; i++)
        {
            maxSlices += depthCount;

            depthCount = Math.Max(1, depthCount >> 1); // Divide depth by 2
        }

        return maxSlices;
    }

    public uint CalculateImageIndex(uint mipIndex, uint levelIndex, uint sliceIndex)
    {
        uint index = 0;

        uint maxSlices = GetMaxSliceCountPerArray();

        if (levelIndex >= Metadata.ArraySize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(levelIndex),
                $"Level index out of bounds: {levelIndex} >= {Metadata.ArraySize}"
            );
        }

        index += maxSlices * levelIndex;

        uint depth = Metadata.Depth;

        for (int i = 0; i < mipIndex; i++)
        {
            index += depth;
            depth = Math.Max(1, depth >> 1); // Divide depth by 2
        }

        if (sliceIndex >= depth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sliceIndex),
                $"Slice index out of bounds: {sliceIndex} >= {depth}"
            );
        }

        index += sliceIndex;

        if (index < 0 || index >= Images.Length)
        {
            throw new InvalidOperationException(
                $"Image not found for mip: {mipIndex}, level: {levelIndex}, slice: {sliceIndex}"
            );
        }

        return index;
    }

    public static Image GetNewRGBA8Image(uint width, uint height, byte[] pixels)
    {
        return new Image
        {
            Width = width,
            Height = height,
            Pixels = pixels,
            PixelFormatInfo = PixelFormats.R8G8B8A8_Unorm_Linear,
            RowPitch = width * 4,
            SlicePitch = width * height * 4,
        };
    }
}
