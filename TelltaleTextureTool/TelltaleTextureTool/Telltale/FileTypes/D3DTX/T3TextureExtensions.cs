using System;
using System.Collections.Generic;
using System.Linq;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.Utilities;
using TelltaleToolKit.T3Types.Textures;
using TelltaleToolKit.T3Types.Textures.T3Types;
using Texture = TelltaleTextureTool.Graphics.Texture;

namespace TelltaleTextureTool.Telltale.FileTypes.D3DTX;

public static class T3TextureExtensions
{
    public static bool IsCubemap(this T3Texture texture)
        => texture.TextureLayout is T3TextureLayout.TextureCubemap or T3TextureLayout.TextureCubemapArray;

    public static bool IsVolumemap(this T3Texture texture)
        => texture.TextureLayout == T3TextureLayout.Texture3D;

    public static bool IsArrayTexture(this T3Texture texture)
        => texture.TextureLayout is T3TextureLayout.Texture2DArray or T3TextureLayout.TextureCubemapArray;

    public static bool IsLegacyD3DTX(this T3Texture texture)
        => texture.Version < 3;

    public static List<T3Texture.RegionStreamHeader> GetRegionDataSortedByMips(this T3Texture texture)
    {
        List<T3Texture.RegionStreamHeader> mappedData = texture.RegionHeaders
            .OrderBy(x => x.MipIndex)
            .ThenBy(x => x.FaceIndex)
            .ThenBy(x => x)
            .ToList();
        return mappedData;
    }

    public static byte[] ExtractSingleMipFromRegion(this T3Texture texture, T3Texture.RegionStreamHeader region,
        PixelFormat pixelFormat, uint width, uint height, uint depth)
    {
        (uint rowPitch, uint slicePitch) pitches = PixelFormatUtility.ComputePitch(pixelFormat, width, height);
        uint slicePitch = pitches.slicePitch;

        return region.RegionData.Skip((int)(slicePitch * depth)).Take((int)slicePitch).ToArray();
    }

    public static void RemoveMip(this T3Texture texture, T3Texture.RegionStreamHeader region, PixelFormat pixelFormat,
        uint width, uint height, uint depth)
    {
        if (region.MipCount <= 1)
        {
            return;
        }

        (uint rowPitch, uint slicePitch) = PixelFormatUtility.ComputePitch(
            pixelFormat,
            width,
            height
        );

        region.MipCount -= 1;
        region.Pitch = (int)rowPitch;
        region.SlicePitch = (int)slicePitch;
        region.DataSize = (int)(region.SlicePitch * depth);
        region.RegionData = region.RegionData.Skip((int)(slicePitch * depth)).ToArray();
    }

    public static byte[] GetSliceData(this T3Texture texture, T3Texture.RegionStreamHeader region, uint sliceIndex)
    {
        if (sliceIndex > (region.DataSize / region.SlicePitch))
        {
            throw new ArgumentException("Slice index out of bounds!");
        }

        int sliceSize = region.SlicePitch;
        var sliceOffset = (int)(sliceIndex * sliceSize);

        return region.RegionData.Skip(sliceOffset).Take(sliceSize).ToArray();
    }

    public static void ConvertD3Dtx(this T3Texture texture, Texture commonTexture)
    {
        //     TexMetadata metadata = commonTexture.Metadata;
        //     // texture.Width = 
        //     texture.Width = metadata.Width;
        //     texture.Height = metadata.Height;
        //     texture.SurfaceFormat = metadata.Format;
        //     texture.NumMipLevels = metadata.MipLevels;
        //     texture.Depth = metadata.Depth;
        //     texture.ArraySize = metadata.ArraySize;
        //     texture.SurfaceGamma = metadata.SurfaceGamma;
        //     
        //     if (texture.Version < 3)
        //     {
        //         texture.HasTextureData = true;
        //         
        //         return;
        //     }
        //     
        //     texture.RegionHeaders = new List<T3Texture.RegionStreamHeader>(commonTexture.Images.Length);
        //     
        //     for (int i = 0; i < commonTexture.Images.Length; i++)
        //     {
        //         var regionHeader = new T3Texture.RegionStreamHeader
        //         {
        //             DataSize = commonTexture.Images[i].Pixels.Length,
        //             Pitch = (int)commonTexture.Images[i].RowPitch,
        //             SlicePitch = (int)commonTexture.Images[i].SlicePitch,
        //             MipCount = 1,
        //         };
        //
        //         texture.RegionHeaders.Add(regionHeader);
        //     }
        //     
        //     if (metadata.IsCubemap())
        //     {
        //         mArraySize /= 6;
        //         mTextureLayout = mArraySize > 1 ? T3TextureLayout.TextureCubemapArray : T3TextureLayout.TextureCubemap;
        //
        //         int interval = mStreamHeader.mRegionCount / (int)mNumMipLevels;
        //         // Example a cube array textures with 5 mips will have 30 regions (6 faces * 5 mips)
        //         // If the array is 2 element there will be 60 regions (6 faces * 5 mips * 2 elements)
        //         // The mip index will be the region index % interval
        //         for (int i = 0; i < mStreamHeader.mRegionCount; i++)
        //         {
        //             mRegionHeaders[i].mFaceIndex = i % (6 * (int)mArraySize); // Unknown guess, it could be 6 or 6 * mArraySize
        //             mRegionHeaders[i].mMipIndex = (mStreamHeader.mRegionCount - i - 1) / interval;
        //         }
        //     }
        //     else if (metadata.IsVolumemap())
        //     {
        //         mTextureLayout = T3TextureLayout.Texture3D;
        //
        //         int currDepth = (int)metadata.Depth;
        //         int currentMipIndex = (int)(mNumMipLevels - 1);
        //         int copyOfDepth = currDepth;
        //
        //         int depthIndex = 0;
        //         int faceIndex = 0;
        //
        //         for (int i = 0; i < mStreamHeader.mRegionCount; i++)
        //         {
        //             mRegionHeaders[mStreamHeader.mRegionCount - currDepth + depthIndex++].mFaceIndex =
        //                 faceIndex++; // NOTE: This could be different for 3D textures. I don't have information at the moment.
        //             mRegionHeaders[i].mMipIndex = currentMipIndex;
        //
        //             if (depthIndex == copyOfDepth)
        //             {
        //                 currentMipIndex--;
        //
        //                 copyOfDepth = Math.Max(copyOfDepth / 2, 1);
        //                 currDepth += copyOfDepth;
        //
        //                 depthIndex = 0;
        //                 faceIndex = 0;
        //             }
        //         }
        //     }
        //     else
        //     {
        //         mTextureLayout =
        //             mArraySize > 1 ? T3TextureLayout.Texture2DArray : T3TextureLayout.Texture2D;
        //
        //         int interval = mStreamHeader.mRegionCount / (int)mNumMipLevels;
        //
        //         for (int i = 0; i < mStreamHeader.mRegionCount; i++)
        //         {
        //             mRegionHeaders[i].mFaceIndex = i % (int)mArraySize;
        //             mRegionHeaders[i].mMipIndex = (mStreamHeader.mRegionCount - i - 1) / interval;
        //         }
        //     }
    }
}