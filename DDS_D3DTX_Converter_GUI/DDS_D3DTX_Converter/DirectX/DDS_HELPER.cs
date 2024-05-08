﻿using D3DTX_Converter.Main;
using D3DTX_Converter.TelltaleEnums;
using D3DTX_Converter.TelltaleTypes;
using D3DTX_Converter.Utilities;
using DirectXTexNet;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;

namespace D3DTX_Converter.DirectX;

// DDS Docs - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header
// DDS PIXEL FORMAT - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-pixelformat
// DDS DDS_HEADER_DXT10 - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header-dxt10
// DDS File Layout https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-file-layout-for-textures
// Texture Block Compression in D3D11 - https://docs.microsoft.com/en-us/windows/win32/direct3d11/texture-block-compression-in-direct3d-11
// DDS Programming Guide - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds-pguide

//TODO Fix documentation
//TODO Other functions need documentation.
//TODO Delete functions which aren't used
//TODO Test formats

/// <summary>
/// The class is used for decoding and encoding .dds headers. 
/// </summary>
public static partial class DDS_HELPER
{
    public static uint GetDDSBlockSize(DDS_HEADER header, DDS_HEADER_DXT10 dx10_header)
    {
        uint compressionValue = header.ddspf.dwFourCC;

        if (compressionValue == ByteFunctions.Convert_String_To_UInt32("DX10"))
        {
            compressionValue = (uint)dx10_header.dxgiFormat;
        }

        return GetDXGICompressionBlockSize(compressionValue);
    }

    public static uint[,] CalculateMipResolutions(uint mipCount, uint width, uint height)
    {
        //because I suck at math, we will generate our mip map resolutions using the same method we did in d3dtx to dds (can't figure out how to calculate them in reverse properly)
        //first [] is the "resolution" index, and the second [] always has a length of 2, and contains the width and height
        uint[,] mipResolutions = new uint[mipCount, 2];

        //get our mip image dimensions (have to multiply by 2 as the mip calculations will be off by half)
        uint mipImageWidth = width * 2;
        uint mipImageHeight = height * 2;

        //add the resolutions in reverse ( largest mipmap - first index, smallest mipmap will be last index)
        for (int i = 0; i < mipCount; i++)
        {
            //divide the resolutions by 2
            mipImageWidth = Math.Max(1, mipImageWidth / 2);
            mipImageHeight = Math.Max(1, mipImageHeight / 2);

            //assign the resolutions
            mipResolutions[i, 0] = mipImageWidth;
            mipResolutions[i, 1] = mipImageHeight;
        }

        return mipResolutions;
    }

    public static uint GetVolumeFaceCount(uint depth, uint mipCount)
    {
        uint faceCount = 0;

        for (int i = 0; i < mipCount; i++)
        {
            faceCount += depth;
            depth = Math.Max(1, depth / 2);
        }

        return faceCount;
    }


    /// <summary>
    /// Calculates the mip resolutions for a volume texture.
    /// </summary>
    /// <param name="mipCount">The number of mip levels.</param>
    /// <param name="width">The width of the base level texture.</param>
    /// <param name="height">The height of the base level texture.</param>
    /// <param name="depth">The depth of the base level texture.</param>
    /// <returns>A 3D array containing the mip resolutions for each level.</returns>
    public static uint[,] CalculateVolumeMipResolutions(uint mipCount, uint width, uint height, uint depth)
    {
        uint count = GetVolumeFaceCount(depth, mipCount);

        uint[,] mipResolutions = new uint[count, 2];
        uint mipImageWidth = width;
        uint mipImageHeight = height;

        uint depthCopy = depth;

        for (int i = 0; i < mipCount; i++)
        {
            for (int j = 0; j < depthCopy; j++)
            {
                mipResolutions[i, 0] = mipImageWidth;
                mipResolutions[i, 1] = mipImageHeight;
            }

            mipImageWidth = Math.Max(1, mipImageWidth / 2);
            mipImageHeight = Math.Max(1, mipImageHeight / 2);
            depthCopy = Math.Max(1, depthCopy / 2);
        }

        return mipResolutions;
    }


    public static uint[] GetImageByteSizes(uint[,] mipResolutions, uint baseLinearSize, uint bitPixelSize, bool isCompressed)
    {
        uint[] byteSizes = new uint[mipResolutions.GetLength(0)];

        //Get the byte sizes for each mip map, first index - largest mip map, last index - smallest mip map
        for (int i = 0; i < byteSizes.Length; i++)
        {
            uint mipWidth = mipResolutions[i, 0];
            uint mipHeight = mipResolutions[i, 1];

            // It works for square textures
            byteSizes[i] = CalculateByteSize(mipWidth, mipHeight, bitPixelSize, isCompressed);
            Console.WriteLine("Mip " + i + " size: " + byteSizes[i]);

            // This is outdated code, used only for reference
            // if (mipWidth == mipHeight) //SQUARE SIZE
            // {
            //     computed linear size
            //     (mipWidth * mipWidth) / 2

            //     byteSizes[i] = Calculate_ByteSize_Square(mipWidth, mipHeight, baseLinearSize, (uint)i, (uint)byteSizes.Length, blockSize);
            //     byteSizes[i] = Calculate_ByteSize(mipWidth, mipHeight, blockSize);
            // }   
            // else //NON SQUARE
            // {
            //     byteSizes[i] = Calculate_ByteSize_NonSquare(mipWidth, mipHeight, blockSize);
            // }
            //
            // original calculation
            // byteSizes[i] = CalculateDDS_ByteSize((int)mipResolutions[i, 0], (int)mipResolutions[i, 1], isDXT1);
        }

        return byteSizes;
    }

    public static uint[] GetVolumeImageByteSizes(uint[,,] mipResolutions, uint bitPixelSize, bool isCompressed)
    {
        uint[] byteSizes = new uint[mipResolutions.GetLength(0)];

        //Get the byte sizes for each mip map, first index - largest mip map, last index - smallest mip map
        for (int i = 0; i < byteSizes.Length; i++)
        {
            uint mipWidth = mipResolutions[i, 0, 0];
            uint mipHeight = mipResolutions[i, 0, 1];

            // It works for square textures
            byteSizes[i] = CalculateByteSize(mipWidth, mipHeight, bitPixelSize, isCompressed);
            Console.WriteLine("Mip " + i + " size: " + byteSizes[i]);
        }

        return byteSizes;
    }

    /// <summary>
    /// The block-size is 8 bytes for DXT1, BC1, and BC4 formats, and 16 bytes for other block-compressed formats.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="blockSizeDouble"></param>
    /// <returns></returns>
    public static uint ComputePitchValue_BlockCompression(uint width, uint blockSize)
    {
        //max(1, ((width + 3) / 4)) * blocksize
        return (uint)(MathF.Max(1, (width + 3) / 4) * blockSize);
    }

    public static uint ComputePitchValue_Legacy(uint width)
    {
        //((width+1) >> 1) * 4
        return ((width + 1) >> 1) * 4;
    }

    public static uint ComputePitchValue_Other(uint width, uint bitsPerPixel)
    {
        //( width * bits-per-pixel + 7 ) / 8
        return (width * bitsPerPixel + 7) / 8;
    }

    public static bool CompressionBool(DDS_HEADER header) => header.ddspf.dwFourCC.Equals("DXT1");

    public static T3SurfaceFormatDesc GetSurfaceFormatDesc()
    {
        T3SurfaceFormatDesc result = new()
        {
            mBitsPerBlock = 0,
            mBitsPerPixel = 0,
            mBlockHeight = 1,
            mBlockWidth = 1,
            mMinBytesPerSurface = 1
        };

        uint test = 0;

        switch (test)
        {
            case 0xEu:
            case 0x10u:
            case 0x11u:
                result.mBitsPerPixel = 8;
                break;
            case 0xDu:
            case 0x25u:
                result.mBitsPerPixel = 128;
                break;
            case 2u:
            case 3u:
            case 4u:
            case 6u:
            case 9u:
            case 0x12u:
            case 0x13u:
            case 0x16u:
            case 0x20u:
            case 0x30u:
            case 0x32u:
                result.mBitsPerPixel = 16;
                break;
            case 1u:
            case 8u:
            case 0xCu:
            case 0x15u:
            case 0x22u:
            case 0x24u:
            case 0x36u:
                result.mBitsPerPixel = 64;
                break;
            case 0x40u:
            case 0x43u:
            case 0x45u:
            case 0x70u:
            case 0x71u:
            case 0x72u:
            case 0x74u:
                result.mBitsPerPixel = 4;
                result.mBlockWidth = 4;
                result.mBlockHeight = 4;
                result.mBitsPerBlock = 64;
                break;
            case 0x50u:
            case 0x52u:
                result.mBitsPerPixel = 2;
                result.mBlockHeight = 8;
                goto LABEL_8;
            case 0x51u:
            case 0x53u:
                result.mBitsPerPixel = 4;
                result.mBlockHeight = 4;
            LABEL_8:
                result.mBlockWidth = 4;
                result.mBitsPerBlock = 64;
                result.mMinBytesPerSurface = 32;
                break;
            case 0x60u:
                result.mBitsPerPixel = 4;
                result.mBlockWidth = 4;
                result.mBlockHeight = 4;
                result.mBitsPerBlock = 64;
                result.mMinBytesPerSurface = 8;
                break;
            case 0x61u:
            case 0x62u:
                result.mBitsPerPixel = 8;
                result.mBlockWidth = 4;
                result.mBlockHeight = 4;
                result.mBitsPerBlock = 128;
                result.mMinBytesPerSurface = 16;
                break;
            case 0x41u:
            case 0x42u:
            case 0x44u:
            case 0x46u:
            case 0x47u:
            case 0x73u:
            case 0x75u:
            case 0x80u:
                result.mBitsPerPixel = 8;
                result.mBlockWidth = 4;
                result.mBlockHeight = 4;
                result.mBitsPerBlock = 128;
                break;
            case 0u:
            case 5u:
            case 7u:
            case 0xAu:
            case 0xBu:
            case 0xFu:
            case 0x14u:
            case 0x17u:
            case 0x21u:
            case 0x23u:
            case 0x26u:
            case 0x31u:
            case 0x33u:
            case 0x34u:
            case 0x35u:
            case 0x37u:
            case 0x90u:
                result.mBitsPerPixel = 32;
                break;
            default:
                break;
        }

        /*
        v3 = result->mBitsPerBlock;
        v4 = 1;

        if (!v3)
        {
            v3 = v2->mBitsPerPixel;
            v2->mBitsPerBlock = v3;
            v2->mBlockWidth = 1;
            v2->mBlockHeight = 1;
        }

        if (!v2->mMinBytesPerSurface)
        {
            v5 = (v3 + 7) / 8;

            if (v5 > 1)
                v4 = v5;

            v2->mMinBytesPerSurface = v4;
        }
        */

        return result;
    }

    /// <summary>
    /// Calculates the byte size of a DDS texture
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="isDXT1"></param>
    /// <returns></returns>
    public static uint CalculateByteSize(uint width, uint height, uint bitPixelSize, bool isCompressed)
    {
        //formula (from microsoft docs)
        //max(1, ( (width + 3) / 4 ) ) x max(1, ( (height + 3) / 4 ) ) x 8(DXT1) or 16(DXT2-5)

        //formula (from here) - http://doc.51windows.net/directx9_sdk/graphics/reference/DDSFileReference/ddstextures.htm
        //max(1,width ?4)x max(1,height ?4)x 8 (DXT1) or 16 (DXT2-5)

        //do the micorosoft magic texture byte size calculation formula

        if (isCompressed)
        {
            return Math.Max(1, ((width + 3) / 4)) * Math.Max(1, ((height + 3) / 4)) * bitPixelSize;
        }
        else
        {
            return width * height * bitPixelSize;
        }

        //formula (from here) - http://doc.51windows.net/directx9_sdk/graphics/reference/DDSFileReference/ddstextures.htm
        //return Math.Max(1, width / 4) * Math.Max(1, height / 4) * bitPixelSize;
    }

    /// <summary>
    /// Converts a DDS_HEADER object into a byte array.
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    public static byte[] GetHeaderBytes(DDS_HEADER header)
    {
        int size = Marshal.SizeOf(header);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(header, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }

    /// <summary>
    /// Converts a DDS_HEADER_DXT10 object into a byte array.
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    public static byte[] GetDXT10HeaderBytes(DDS_HEADER_DXT10 header)
    {
        int size = Marshal.SizeOf(header);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(header, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }

    //TODO ADD GET BITS PER PIXEL TO REFACTOR
    public static uint GetPitchOrLinearSizeFromD3DTX(T3SurfaceFormat format, uint width)
    {
        if (D3DTX_Master.IsTextureCompressed(format))
        {
            return Math.Max(1, (width + 3) / 4 * D3DTX_Master.GetD3DTXBlockSize(format));
        }
        // check for legacy formats
        else return (width * D3DTX_Master.GetBitsPerPixel(format) + 7) / 8;
    }

    /// <summary>
    /// Returns the corresponding Telltale surface format from a .dds four-character code.
    /// </summary>
    /// <param name="fourCC"></param>
    /// <param name="dds"></param>
    /// <returns></returns>
    public static T3SurfaceFormat Get_T3Format_FromFourCC(uint fourCC, DDS_Master ddsMaster)
    {
        if (fourCC == ByteFunctions.Convert_String_To_UInt32("DXT1")) return T3SurfaceFormat.eSurface_DXT1;
        else if (fourCC == ByteFunctions.Convert_String_To_UInt32("DXT3")) return T3SurfaceFormat.eSurface_DXT3;
        else if (fourCC == ByteFunctions.Convert_String_To_UInt32("DXT5")) return T3SurfaceFormat.eSurface_DXT5;
        else if (fourCC == ByteFunctions.Convert_String_To_UInt32("ATI2")) return T3SurfaceFormat.eSurface_DXN;
        else if (fourCC == ByteFunctions.Convert_String_To_UInt32("ATI1")) return T3SurfaceFormat.eSurface_DXT5A;
        else if (fourCC == ByteFunctions.Convert_String_To_UInt32("BC4S")) return T3SurfaceFormat.eSurface_BC4;
        else if (fourCC == ByteFunctions.Convert_String_To_UInt32("BC4U")) return T3SurfaceFormat.eSurface_BC4;
        else if (fourCC == ByteFunctions.Convert_String_To_UInt32("BC5S")) return T3SurfaceFormat.eSurface_BC5;
        else if (fourCC == ByteFunctions.Convert_String_To_UInt32("BC5U")) return T3SurfaceFormat.eSurface_BC5;
        else if (fourCC == ByteFunctions.Convert_String_To_UInt32("DX10")) return Parse_T3Format_FromDX10(ddsMaster.dds.dxt10Header.dxgiFormat);
        else if (fourCC == 0) return Parse_T3Format_FromD3FORMAT(D3D9FormatConverter.GetD3D9Format(ddsMaster.dds.header.ddspf));
        else return T3SurfaceFormat.eSurface_DXT1;
    }

    /// <summary>
    /// If the four-character code is DX10, we have an additional parser for the format.
    /// </summary>
    /// <param name="dxgi_format"></param>
    /// <returns></returns>
    public static T3SurfaceFormat Parse_T3Format_FromDX10(DXGI_FORMAT dxgi_format)
    {
        //TODO Check if other formats are needed
        return (int)dxgi_format switch
        {
            (int)DXGI_FORMAT.B8G8R8A8_UNORM_SRGB => T3SurfaceFormat.eSurface_ARGB8,
            (int)DXGI_FORMAT.B8G8R8A8_UNORM => T3SurfaceFormat.eSurface_ARGB8,
            (int)DXGI_FORMAT.R16G16B16A16_SNORM => T3SurfaceFormat.eSurface_ARGB16,
            (int)DXGI_FORMAT.B5G6R5_UNORM => T3SurfaceFormat.eSurface_RGB565,
            (int)DXGI_FORMAT.B5G5R5A1_UNORM => T3SurfaceFormat.eSurface_ARGB1555,
            (int)DXGI_FORMAT.B4G4R4A4_UNORM => T3SurfaceFormat.eSurface_ARGB4,
            (int)DXGI_FORMAT.R10G10B10A2_UNORM => T3SurfaceFormat.eSurface_ARGB2101010,
            (int)DXGI_FORMAT.R16_UNORM => T3SurfaceFormat.eSurface_R16,
            (int)DXGI_FORMAT.R16G16_UNORM => T3SurfaceFormat.eSurface_RG16,
            (int)DXGI_FORMAT.R16G16B16A16_UNORM => T3SurfaceFormat.eSurface_RGBA16,
            (int)DXGI_FORMAT.R8G8_UNORM => T3SurfaceFormat.eSurface_RG8,
            (int)DXGI_FORMAT.R8G8B8A8_UNORM_SRGB => T3SurfaceFormat.eSurface_RGBA8,
            (int)DXGI_FORMAT.R8G8B8A8_UNORM => T3SurfaceFormat.eSurface_RGBA8,
            //TODO FIX R32 (could be int here)
            (int)DXGI_FORMAT.R32_UINT => T3SurfaceFormat.eSurface_R32,
            (int)DXGI_FORMAT.R32G32_UINT => T3SurfaceFormat.eSurface_RG32,
            (int)DXGI_FORMAT.R32G32B32A32_FLOAT => T3SurfaceFormat.eSurface_RGBA32F,
            (int)DXGI_FORMAT.R8_UNORM => T3SurfaceFormat.eSurface_R8,
            (int)DXGI_FORMAT.R8G8B8A8_SNORM => T3SurfaceFormat.eSurface_RGBA8S,
            (int)DXGI_FORMAT.A8_UNORM => T3SurfaceFormat.eSurface_A8,
            //(int)DXGI_FORMAT.R8_UNORM =>T3SurfaceFormat.eSurface_L8, //needs to check dds for luminance
            //(int)DXGI_FORMAT.R8G8_UNORM => T3SurfaceFormat.eSurface_AL8, //needs to check dds for luminance
            //(int) DXGI_FORMAT.R16_UNORM => T3SurfaceFormat.eSurface_L16, //needs to check dds for luminance
            (int)DXGI_FORMAT.R16G16_SNORM => T3SurfaceFormat.eSurface_RG16S,
            //(int)DXGI_FORMAT.R16G16B16A16_SNORM=>T3SurfaceFormat.eSurface_RGBA16S,
            (int)DXGI_FORMAT.R16G16B16A16_UINT => T3SurfaceFormat.eSurface_R16UI,
            (int)DXGI_FORMAT.R16_FLOAT => T3SurfaceFormat.eSurface_R16F,
            (int)DXGI_FORMAT.R16G16B16A16_FLOAT => T3SurfaceFormat.eSurface_RGBA16F,
            (int)DXGI_FORMAT.R32_FLOAT => T3SurfaceFormat.eSurface_R32F,
            (int)DXGI_FORMAT.R32G32_FLOAT => T3SurfaceFormat.eSurface_RG32F,
            //(int)DXGI_FORMAT.R32G32B32A32_FLOAT=>T3SurfaceFormat.eSurface_RGBA32,
            //TODO SAME HERE, IS IT INT?
            // (int)DXGI_FORMAT.R10G10B10A2_UNORM=>T3SurfaceFormat.eSurface_RGBA1010102F,
            (int)DXGI_FORMAT.R11G11B10_FLOAT => T3SurfaceFormat.eSurface_RGB111110F,
            (int)DXGI_FORMAT.R9G9B9E5_SHAREDEXP => T3SurfaceFormat.eSurface_RGB9E5F,
            (int)DXGI_FORMAT.D16_UNORM => T3SurfaceFormat.eSurface_DepthPCF16,
            (int)DXGI_FORMAT.D24_UNORM_S8_UINT => T3SurfaceFormat.eSurface_DepthPCF24, //maybe check in what type of map it's used?
                                                                                       //??
                                                                                       //(int)DXGI_FORMAT.D16_UNORM =>T3SurfaceFormat.eSurface_Depth16,
                                                                                       //(int)DXGI_FORMAT.D24_UNORM_S8_UINT => T3SurfaceFormat.eSurface_Depth24,
            (int)DXGI_FORMAT.D32_FLOAT_S8X24_UINT => T3SurfaceFormat.eSurface_DepthStencil32,
            (int)DXGI_FORMAT.D32_FLOAT => T3SurfaceFormat.eSurface_Depth32F,
            //(int)DXGI_FORMAT.D32_FLOAT_S8X24_UINT => T3SurfaceFormat.eSurface_Depth32F_Stencil8, 
            //(int)DXGI_FORMAT.D24_UNORM_S8_UINT =>T3SurfaceFormat.eSurface_Depth24F_Stencil8,  //maybe check in what type of map it's used?
            //TODO Check for game meta versions potentially?
            (int)DXGI_FORMAT.BC1_UNORM => T3SurfaceFormat.eSurface_BC1,
            (int)DXGI_FORMAT.BC2_UNORM => T3SurfaceFormat.eSurface_BC2,
            (int)DXGI_FORMAT.BC3_UNORM => T3SurfaceFormat.eSurface_BC3,
            (int)DXGI_FORMAT.BC4_UNORM => T3SurfaceFormat.eSurface_BC4,
            (int)DXGI_FORMAT.BC5_UNORM => T3SurfaceFormat.eSurface_BC5,
            (int)DXGI_FORMAT.BC6H_UF16 => T3SurfaceFormat.eSurface_BC6,
            (int)DXGI_FORMAT.BC7_UNORM => T3SurfaceFormat.eSurface_BC7, //check T3SurfaceGamma.eSurfaceGamma_sRGB
            (int)DXGI_FORMAT.BC1_UNORM_SRGB => T3SurfaceFormat.eSurface_BC1,
            (int)DXGI_FORMAT.BC2_UNORM_SRGB => T3SurfaceFormat.eSurface_BC2,
            (int)DXGI_FORMAT.BC3_UNORM_SRGB => T3SurfaceFormat.eSurface_BC3,
            (int)DXGI_FORMAT.BC7_UNORM_SRGB => T3SurfaceFormat.eSurface_BC7,
            _ => T3SurfaceFormat.eSurface_Unknown,
        };

    }

    public static T3SurfaceFormat Parse_T3Format_FromD3FORMAT(D3DFORMAT d3dformat_format)
    {
        //TODO Check if other formats are needed
        return (int)d3dformat_format switch
        {
            (int)D3DFORMAT.A8R8G8B8 => T3SurfaceFormat.eSurface_ARGB8,
            (int)D3DFORMAT.X8R8G8B8 => T3SurfaceFormat.eSurface_ARGB8,
            (int)D3DFORMAT.A16B16G16R16 => T3SurfaceFormat.eSurface_ARGB16,
            (int)D3DFORMAT.R5G6B5 => T3SurfaceFormat.eSurface_RGB565,
            (int)D3DFORMAT.A1R5G5B5 => T3SurfaceFormat.eSurface_ARGB1555,
            (int)D3DFORMAT.A4R4G4B4 => T3SurfaceFormat.eSurface_ARGB4,
            (int)D3DFORMAT.A2B10G10R10 => T3SurfaceFormat.eSurface_ARGB2101010,
            (int)D3DFORMAT.G16R16 => T3SurfaceFormat.eSurface_RG16,
            //  (int)D3DFORMAT.A16B16G16R16 => T3SurfaceFormat.eSurface_RGBA16, swap color channels?
            (int)D3DFORMAT.A8B8G8R8 => T3SurfaceFormat.eSurface_RGBA8,
            //  (int)D3DFORMAT.X8R8G8B8 => T3SurfaceFormat.eSurface_RGBA8,
            (int)D3DFORMAT.D32 => T3SurfaceFormat.eSurface_DepthStencil32,
            (int)D3DFORMAT.A32B32G32R32F => T3SurfaceFormat.eSurface_RGBA32F,
            //(int)D3DFORMAT.A8 => T3SurfaceFormat.eSurface_R8, check channels?
            // (int)D3DFORMAT.A8R8G8B8 => T3SurfaceFormat.eSurface_RGBA8S,
            (int)D3DFORMAT.A8 => T3SurfaceFormat.eSurface_A8,
            //(int)D3DFORMAT.D3DFMT_R8 =>T3SurfaceFormat.eSurface_L8,
            //(int)D3DFORMAT.D3DFMT_G8R8 => T3SurfaceFormat.eSurface_AL8,
            //(int) D3DFORMAT.D3DFMT_R16 => T3SurfaceFormat.eSurface_L16,
            //(int)D3DFORMAT.D3DFMT_A16B16G16R16=>T3SurfaceFormat.eSurface_RGBA16S,
            (int)D3DFORMAT.R16F => T3SurfaceFormat.eSurface_R16F,
            (int)D3DFORMAT.A16B16G16R16F => T3SurfaceFormat.eSurface_RGBA16F,
            (int)D3DFORMAT.R32F => T3SurfaceFormat.eSurface_R32F,
            (int)D3DFORMAT.G32R32F => T3SurfaceFormat.eSurface_RG32F,
            //(int)D3DFORMAT.D3DFMT_A32B32G32R32F=>T3SurfaceFormat.eSurface_RGBA32F,
            //TODO SAME HERE, IS IT INT?
            // (int)D3DFORMAT.D3DFMT_A2B10G10R10=>T3SurfaceFormat.eSurface_RGBA1010102F,
            // (int)D3DFORMAT.R11G11B10 => T3SurfaceFormat.eSurface_RGB111110F,
            (int)D3DFORMAT.D16 => T3SurfaceFormat.eSurface_Depth16, //check for maps
            (int)D3DFORMAT.D24S8 => T3SurfaceFormat.eSurface_Depth24, //check for maps
                                                                      //??
                                                                      //(int)D3DFORMAT.D3DFMT_D16 =>T3SurfaceFormat.eSurface_Depth16,
                                                                      //(int)D3DFORMAT.D3DFMT_D24S8 => T3SurfaceFormat.eSurface_Depth24,
            (int)D3DFORMAT.D32F_LOCKABLE => T3SurfaceFormat.eSurface_Depth32F,
            // (int)D3DFORMAT.D32FS8_TEXTURE => T3SurfaceFormat.eSurface_Depth32F_Stencil8,
            (int)D3DFORMAT.D24FS8 => T3SurfaceFormat.eSurface_Depth24F_Stencil8,
            //TODO ADD BC1, BC2, BC3, BC4, BC5, BC6H, BC7
            (int)D3DFORMAT.DXT1 => T3SurfaceFormat.eSurface_DXT1,
            (int)D3DFORMAT.DXT2 => T3SurfaceFormat.eSurface_DXT3,
            (int)D3DFORMAT.DXT3 => T3SurfaceFormat.eSurface_DXT3,
            (int)D3DFORMAT.DXT4 => T3SurfaceFormat.eSurface_DXT3,
            (int)D3DFORMAT.DXT5 => T3SurfaceFormat.eSurface_DXT5, //alpha?
            _ => T3SurfaceFormat.eSurface_Unknown,
        };

    }


    /// <summary>
    /// Returns the corresponding DXGI_Format from a Telltale surface format. This is used for the conversion process from .d3dtx to .dds.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="gamma"></param>
    /// <returns></returns>
    public static DXGI_FORMAT GetSurfaceFormatAsDXGI(T3SurfaceFormat format, T3SurfaceGamma gamma = T3SurfaceGamma.eSurfaceGamma_sRGB)
    {
        switch (format)
        {
            default:
                return DXGI_FORMAT.BC1_UNORM; // Choose DXT1 if the format is not specified

            // In order of T3SurfaceFormat enum
            //--------------------ARGB8--------------------
            case T3SurfaceFormat.eSurface_ARGB8:
                if (gamma == T3SurfaceGamma.eSurfaceGamma_sRGB)
                    return DXGI_FORMAT.B8G8R8A8_UNORM_SRGB;
                else
                    return DXGI_FORMAT.B8G8R8A8_UNORM;
            //--------------------ARGB16--------------------
            case T3SurfaceFormat.eSurface_ARGB16:
                return DXGI_FORMAT.R16G16B16A16_UNORM;

            //--------------------RGB565--------------------
            case T3SurfaceFormat.eSurface_RGB565:
                return DXGI_FORMAT.B5G6R5_UNORM;

            //--------------------ARGB1555--------------------
            case T3SurfaceFormat.eSurface_ARGB1555:
                return DXGI_FORMAT.B5G5R5A1_UNORM;

            //--------------------ARGB4--------------------
            case T3SurfaceFormat.eSurface_ARGB4:
                return DXGI_FORMAT.B4G4R4A4_UNORM;

            //--------------------ARGB2101010--------------------
            case T3SurfaceFormat.eSurface_ARGB2101010:
                return DXGI_FORMAT.R10G10B10A2_UNORM;

            //--------------------R16--------------------
            case T3SurfaceFormat.eSurface_R16:
                return DXGI_FORMAT.R16_UNORM;

            //--------------------RG16--------------------
            case T3SurfaceFormat.eSurface_RG16:
                return DXGI_FORMAT.R16G16_UNORM;

            //--------------------RGBA16--------------------
            case T3SurfaceFormat.eSurface_RGBA16:
                return DXGI_FORMAT.R16G16B16A16_UNORM;

            //--------------------RG8--------------------
            case T3SurfaceFormat.eSurface_RG8:
                return DXGI_FORMAT.R8G8_UNORM;

            //--------------------RGBA8--------------------
            case T3SurfaceFormat.eSurface_RGBA8:
                if (gamma == T3SurfaceGamma.eSurfaceGamma_sRGB)
                    return DXGI_FORMAT.R8G8B8A8_UNORM_SRGB;
                else
                    return DXGI_FORMAT.R8G8B8A8_UNORM;

            //--------------------R32--------------------
            case T3SurfaceFormat.eSurface_R32:
                return DXGI_FORMAT.R32_UINT; //from https://www.khronos.org/opengl/wiki/Image_Format

            //--------------------RG32--------------------
            case T3SurfaceFormat.eSurface_RG32:
                return DXGI_FORMAT.R32G32_UINT; // from https://www.khronos.org/opengl/wiki/Image_Format

            //--------------------RGBA32--------------------
            case T3SurfaceFormat.eSurface_RGBA32:
                return DXGI_FORMAT.R32G32B32A32_UINT;

            //--------------------R8--------------------
            case T3SurfaceFormat.eSurface_R8:
                return DXGI_FORMAT.R8_UNORM;

            //--------------------RGBA8S--------------------
            case T3SurfaceFormat.eSurface_RGBA8S:
                return DXGI_FORMAT.R8G8B8A8_SNORM;

            //--------------------A8--------------------
            case T3SurfaceFormat.eSurface_A8:
                return DXGI_FORMAT.A8_UNORM;

            //--------------------L8--------------------
            case T3SurfaceFormat.eSurface_L8:
                return DXGI_FORMAT.R8_UNORM;

            //--------------------AL8--------------------
            case T3SurfaceFormat.eSurface_AL8:
                return DXGI_FORMAT.R8G8_UNORM;

            //--------------------R16--------------------
            case T3SurfaceFormat.eSurface_L16:
                return DXGI_FORMAT.R16_UNORM;

            //--------------------RG16S--------------------
            case T3SurfaceFormat.eSurface_RG16S:
                return DXGI_FORMAT.R16G16_SNORM;

            //--------------------RGBA16S--------------------
            case T3SurfaceFormat.eSurface_RGBA16S:
                return DXGI_FORMAT.R16G16B16A16_SNORM;

            //--------------------RGBA16UI--------------------
            case T3SurfaceFormat.eSurface_R16UI:
                return DXGI_FORMAT.R16G16B16A16_UINT;

            //--------------------RG16F--------------------
            case T3SurfaceFormat.eSurface_R16F:
                return DXGI_FORMAT.R16_FLOAT;

            //--------------------RGBA16F--------------------
            case T3SurfaceFormat.eSurface_RGBA16F:
                return DXGI_FORMAT.R16G16B16A16_FLOAT;

            //--------------------R32F--------------------
            case T3SurfaceFormat.eSurface_R32F:
                return DXGI_FORMAT.R32_FLOAT;

            //--------------------RG32F--------------------
            case T3SurfaceFormat.eSurface_RG32F:
                return DXGI_FORMAT.R32G32_FLOAT;

            //--------------------RGBA32F--------------------
            case T3SurfaceFormat.eSurface_RGBA32F:
                return DXGI_FORMAT.R32G32B32A32_FLOAT;

            //--------------------RGBA1010102F--------------------
            case T3SurfaceFormat.eSurface_RGBA1010102F:
                return DXGI_FORMAT.R10G10B10A2_UNORM;

            //--------------------RGB111110F--------------------
            case T3SurfaceFormat.eSurface_RGB111110F:
                return DXGI_FORMAT.R11G11B10_FLOAT;

            //--------------------RGB9E5F--------------------
            case T3SurfaceFormat.eSurface_RGB9E5F:
                return DXGI_FORMAT.R9G9B9E5_SHAREDEXP;

            //--------------------DepthPCF16--------------------
            case T3SurfaceFormat.eSurface_DepthPCF16:
                return DXGI_FORMAT.D16_UNORM;

            //--------------------DepthPCF24--------------------
            case T3SurfaceFormat.eSurface_DepthPCF24:
                return DXGI_FORMAT.D24_UNORM_S8_UINT;

            //--------------------Depth16--------------------
            case T3SurfaceFormat.eSurface_Depth16:
                return DXGI_FORMAT.D16_UNORM;

            //--------------------Depth24--------------------
            case T3SurfaceFormat.eSurface_Depth24:
                return DXGI_FORMAT.D24_UNORM_S8_UINT;

            //--------------------DepthStencil32--------------------
            case T3SurfaceFormat.eSurface_DepthStencil32:
                return DXGI_FORMAT.D32_FLOAT_S8X24_UINT;

            //--------------------Depth32F--------------------
            case T3SurfaceFormat.eSurface_Depth32F:
                return DXGI_FORMAT.D32_FLOAT;

            //--------------------Depth32F_Stencil8--------------------
            case T3SurfaceFormat.eSurface_Depth32F_Stencil8:
                return DXGI_FORMAT.D32_FLOAT_S8X24_UINT;

            //--------------------Depth24F_Stencil8--------------------
            case T3SurfaceFormat.eSurface_Depth24F_Stencil8:
                return DXGI_FORMAT.D24_UNORM_S8_UINT;

            //--------------------DXT1 / BC1--------------------
            case T3SurfaceFormat.eSurface_BC1:
                if (gamma == T3SurfaceGamma.eSurfaceGamma_sRGB)
                    return DXGI_FORMAT.BC1_UNORM_SRGB;
                else
                    return DXGI_FORMAT.BC1_UNORM;
            case T3SurfaceFormat.eSurface_DXT1:
                if (gamma == T3SurfaceGamma.eSurfaceGamma_sRGB)
                    return DXGI_FORMAT.BC1_UNORM_SRGB;
                else
                    return DXGI_FORMAT.BC1_UNORM;

            //--------------------DXT2 and DXT3 / BC2--------------------
            case T3SurfaceFormat.eSurface_BC2:
                if (gamma == T3SurfaceGamma.eSurfaceGamma_sRGB)
                    return DXGI_FORMAT.BC2_UNORM_SRGB;
                else
                    return DXGI_FORMAT.BC2_UNORM;
            case T3SurfaceFormat.eSurface_DXT3:
                if (gamma == T3SurfaceGamma.eSurfaceGamma_sRGB)
                    return DXGI_FORMAT.BC2_UNORM_SRGB;
                else
                    return DXGI_FORMAT.BC2_UNORM;

            //--------------------DXT4 and DXT5 / BC3--------------------
            case T3SurfaceFormat.eSurface_BC3:
                if (gamma == T3SurfaceGamma.eSurfaceGamma_sRGB)
                    return DXGI_FORMAT.BC3_UNORM_SRGB;
                else
                    return DXGI_FORMAT.BC3_UNORM;
            case T3SurfaceFormat.eSurface_DXT5:
                if (gamma == T3SurfaceGamma.eSurfaceGamma_sRGB)
                    return DXGI_FORMAT.BC3_UNORM_SRGB;
                else
                    return DXGI_FORMAT.BC3_UNORM;

            //--------------------ATI1 / BC4--------------------
            case T3SurfaceFormat.eSurface_BC4:
                return DXGI_FORMAT.BC4_UNORM;
            case T3SurfaceFormat.eSurface_DXT5A:
                return DXGI_FORMAT.BC4_UNORM;

            //--------------------ATI2 / BC5--------------------
            case T3SurfaceFormat.eSurface_BC5:
                return DXGI_FORMAT.BC5_UNORM;
            case T3SurfaceFormat.eSurface_DXN:
                return DXGI_FORMAT.BC5_UNORM;

            //--------------------BC6H--------------------
            case T3SurfaceFormat.eSurface_BC6:
                return DXGI_FORMAT.BC6H_UF16;

            //--------------------BC7--------------------
            case T3SurfaceFormat.eSurface_BC7:
                if (gamma == T3SurfaceGamma.eSurfaceGamma_sRGB)
                    return DXGI_FORMAT.BC7_UNORM_SRGB;
                else
                    return DXGI_FORMAT.BC7_UNORM;

            //--------------------UNKNOWN--------------------
            case T3SurfaceFormat.eSurface_Unknown:
                return DXGI_FORMAT.UNKNOWN;
        }
    }
    public static uint GetDXGIBitsPerPixel(DXGI_FORMAT fmt)
    {
        switch (fmt)
        {
            case DXGI_FORMAT.R32G32B32A32_TYPELESS:
            case DXGI_FORMAT.R32G32B32A32_FLOAT:
            case DXGI_FORMAT.R32G32B32A32_UINT:
            case DXGI_FORMAT.R32G32B32A32_SINT:
                return 128;

            case DXGI_FORMAT.R32G32B32_TYPELESS:
            case DXGI_FORMAT.R32G32B32_FLOAT:
            case DXGI_FORMAT.R32G32B32_UINT:
            case DXGI_FORMAT.R32G32B32_SINT:
                return 96;

            case DXGI_FORMAT.R16G16B16A16_TYPELESS:
            case DXGI_FORMAT.R16G16B16A16_FLOAT:
            case DXGI_FORMAT.R16G16B16A16_UNORM:
            case DXGI_FORMAT.R16G16B16A16_UINT:
            case DXGI_FORMAT.R16G16B16A16_SNORM:
            case DXGI_FORMAT.R16G16B16A16_SINT:
            case DXGI_FORMAT.R32G32_TYPELESS:
            case DXGI_FORMAT.R32G32_FLOAT:
            case DXGI_FORMAT.R32G32_UINT:
            case DXGI_FORMAT.R32G32_SINT:
            case DXGI_FORMAT.R32G8X24_TYPELESS:
            case DXGI_FORMAT.D32_FLOAT_S8X24_UINT:
            case DXGI_FORMAT.R32_FLOAT_X8X24_TYPELESS:
            case DXGI_FORMAT.X32_TYPELESS_G8X24_UINT:
            case DXGI_FORMAT.Y416:
            case DXGI_FORMAT.Y210:
            case DXGI_FORMAT.Y216:
                return 64;

            case DXGI_FORMAT.R10G10B10A2_TYPELESS:
            case DXGI_FORMAT.R10G10B10A2_UNORM:
            case DXGI_FORMAT.R10G10B10A2_UINT:
            case DXGI_FORMAT.R11G11B10_FLOAT:
            case DXGI_FORMAT.R8G8B8A8_TYPELESS:
            case DXGI_FORMAT.R8G8B8A8_UNORM:
            case DXGI_FORMAT.R8G8B8A8_UNORM_SRGB:
            case DXGI_FORMAT.R8G8B8A8_UINT:
            case DXGI_FORMAT.R8G8B8A8_SNORM:
            case DXGI_FORMAT.R8G8B8A8_SINT:
            case DXGI_FORMAT.R16G16_TYPELESS:
            case DXGI_FORMAT.R16G16_FLOAT:
            case DXGI_FORMAT.R16G16_UNORM:
            case DXGI_FORMAT.R16G16_UINT:
            case DXGI_FORMAT.R16G16_SNORM:
            case DXGI_FORMAT.R16G16_SINT:
            case DXGI_FORMAT.R32_TYPELESS:
            case DXGI_FORMAT.D32_FLOAT:
            case DXGI_FORMAT.R32_FLOAT:
            case DXGI_FORMAT.R32_UINT:
            case DXGI_FORMAT.R32_SINT:
            case DXGI_FORMAT.R24G8_TYPELESS:
            case DXGI_FORMAT.D24_UNORM_S8_UINT:
            case DXGI_FORMAT.R24_UNORM_X8_TYPELESS:
            case DXGI_FORMAT.X24_TYPELESS_G8_UINT:
            case DXGI_FORMAT.R9G9B9E5_SHAREDEXP:
            case DXGI_FORMAT.R8G8_B8G8_UNORM:
            case DXGI_FORMAT.G8R8_G8B8_UNORM:
            case DXGI_FORMAT.B8G8R8A8_UNORM:
            case DXGI_FORMAT.B8G8R8X8_UNORM:
            case DXGI_FORMAT.R10G10B10_XR_BIAS_A2_UNORM:
            case DXGI_FORMAT.B8G8R8A8_TYPELESS:
            case DXGI_FORMAT.B8G8R8A8_UNORM_SRGB:
            case DXGI_FORMAT.B8G8R8X8_TYPELESS:
            case DXGI_FORMAT.B8G8R8X8_UNORM_SRGB:
            case DXGI_FORMAT.AYUV:
            case DXGI_FORMAT.Y410:
            case DXGI_FORMAT.YUY2:
                return 32;

            case DXGI_FORMAT.P010:
            case DXGI_FORMAT.P016:
            case DXGI_FORMAT.V408:
                return 24;

            case DXGI_FORMAT.R8G8_TYPELESS:
            case DXGI_FORMAT.R8G8_UNORM:
            case DXGI_FORMAT.R8G8_UINT:
            case DXGI_FORMAT.R8G8_SNORM:
            case DXGI_FORMAT.R8G8_SINT:
            case DXGI_FORMAT.R16_TYPELESS:
            case DXGI_FORMAT.R16_FLOAT:
            case DXGI_FORMAT.D16_UNORM:
            case DXGI_FORMAT.R16_UNORM:
            case DXGI_FORMAT.R16_UINT:
            case DXGI_FORMAT.R16_SNORM:
            case DXGI_FORMAT.R16_SINT:
            case DXGI_FORMAT.B5G6R5_UNORM:
            case DXGI_FORMAT.B5G5R5A1_UNORM:
            case DXGI_FORMAT.A8P8:
            case DXGI_FORMAT.B4G4R4A4_UNORM:
            case DXGI_FORMAT.P208:
            case DXGI_FORMAT.V208:
                return 16;

            case DXGI_FORMAT.NV12:
            case DXGI_FORMAT.OPAQUE_420:
            case DXGI_FORMAT.NV11:
                return 12;

            case DXGI_FORMAT.R8_TYPELESS:
            case DXGI_FORMAT.R8_UNORM:
            case DXGI_FORMAT.R8_UINT:
            case DXGI_FORMAT.R8_SNORM:
            case DXGI_FORMAT.R8_SINT:
            case DXGI_FORMAT.A8_UNORM:
            case DXGI_FORMAT.BC2_TYPELESS:
            case DXGI_FORMAT.BC2_UNORM:
            case DXGI_FORMAT.BC2_UNORM_SRGB:
            case DXGI_FORMAT.BC3_TYPELESS:
            case DXGI_FORMAT.BC3_UNORM:
            case DXGI_FORMAT.BC3_UNORM_SRGB:
            case DXGI_FORMAT.BC5_TYPELESS:
            case DXGI_FORMAT.BC5_UNORM:
            case DXGI_FORMAT.BC5_SNORM:
            case DXGI_FORMAT.BC6H_TYPELESS:
            case DXGI_FORMAT.BC6H_UF16:
            case DXGI_FORMAT.BC6H_SF16:
            case DXGI_FORMAT.BC7_TYPELESS:
            case DXGI_FORMAT.BC7_UNORM:
            case DXGI_FORMAT.BC7_UNORM_SRGB:
            case DXGI_FORMAT.AI44:
            case DXGI_FORMAT.IA44:
            case DXGI_FORMAT.P8:
                return 8;

            case DXGI_FORMAT.R1_UNORM:
                return 1;

            case DXGI_FORMAT.BC1_TYPELESS:
            case DXGI_FORMAT.BC1_UNORM:
            case DXGI_FORMAT.BC1_UNORM_SRGB:
            case DXGI_FORMAT.BC4_TYPELESS:
            case DXGI_FORMAT.BC4_UNORM:
            case DXGI_FORMAT.BC4_SNORM:
                return 4;

            default:
                return 0;
        }
    }

    public static uint GetD3D9FORMATBitsPerPifxel(D3DFORMAT fmt)
    {
        switch (fmt)
        {
            case D3DFORMAT.A32B32G32R32F:
                return 128;

            case D3DFORMAT.A16B16G16R16:
            case D3DFORMAT.Q16W16V16U16:
            case D3DFORMAT.A16B16G16R16F:
            case D3DFORMAT.G32R32F:
                return 64;

            case D3DFORMAT.A8R8G8B8:
            case D3DFORMAT.X8R8G8B8:
            case D3DFORMAT.A2B10G10R10:
            case D3DFORMAT.A8B8G8R8:
            case D3DFORMAT.X8B8G8R8:
            case D3DFORMAT.G16R16:
            case D3DFORMAT.A2R10G10B10:
            case D3DFORMAT.Q8W8V8U8:
            case D3DFORMAT.V16U16:
            case D3DFORMAT.X8L8V8U8:
            case D3DFORMAT.A2W10V10U10:
            case D3DFORMAT.D32:
            case D3DFORMAT.D24S8:
            case D3DFORMAT.D24X8:
            case D3DFORMAT.D24X4S4:
            case D3DFORMAT.D32F_LOCKABLE:
            case D3DFORMAT.D24FS8:
            case D3DFORMAT.INDEX32:
            case D3DFORMAT.G16R16F:
            case D3DFORMAT.R32F:
            case D3DFORMAT.D32_LOCKABLE:
                return 32;

            case D3DFORMAT.R8G8B8:
                return 24;

            case D3DFORMAT.A4R4G4B4:
            case D3DFORMAT.X4R4G4B4:
            case D3DFORMAT.R5G6B5:
            case D3DFORMAT.L16:
            case D3DFORMAT.A8L8:
            case D3DFORMAT.X1R5G5B5:
            case D3DFORMAT.A1R5G5B5:
            case D3DFORMAT.A8R3G3B2:
            case D3DFORMAT.V8U8:
            case D3DFORMAT.CxV8U8:
            case D3DFORMAT.L6V5U5:
            case D3DFORMAT.G8R8_G8B8:
            case D3DFORMAT.R8G8_B8G8:
            case D3DFORMAT.D16_LOCKABLE:
            case D3DFORMAT.D15S1:
            case D3DFORMAT.D16:
            case D3DFORMAT.INDEX16:
            case D3DFORMAT.R16F:
            case D3DFORMAT.YUY2:
            // From DX docs, reference/d3d/enums/d3dformat.asp
            // (note how it says that D3DFMT_R8G8_B8G8 is "A 16-bit packed RGB format analogous to UYVY (U0Y0, V0Y1, U2Y2, and so on)")
            case D3DFORMAT.UYVY:
                return 16;

            case D3DFORMAT.R3G3B2:
            case D3DFORMAT.A8:
            case D3DFORMAT.A8P8:
            case D3DFORMAT.P8:
            case D3DFORMAT.L8:
            case D3DFORMAT.A4L4:
            case D3DFORMAT.DXT2:
            case D3DFORMAT.DXT3:
            case D3DFORMAT.DXT4:
            case D3DFORMAT.DXT5:
            // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/directshow/htm/directxvideoaccelerationdxvavideosubtypes.asp
            case D3DFORMAT.AI44:
            case D3DFORMAT.IA44:
            case D3DFORMAT.S8_LOCKABLE:
                return 8;

            case D3DFORMAT.DXT1:
                return 4;

            case D3DFORMAT.YV12:
                return 12;

            case D3DFORMAT.A1:
                return 1;

            default:
                return 0;
        }
    }

    public static uint GetD3D9FORMATChannelCount(D3DFORMAT fmt)
    {
        switch (fmt)
        {
            case D3DFORMAT.A32B32G32R32F:
                return 128;

            case D3DFORMAT.A16B16G16R16:
            case D3DFORMAT.Q16W16V16U16:
            case D3DFORMAT.A16B16G16R16F:
            case D3DFORMAT.G32R32F:
                return 64;

            case D3DFORMAT.A8R8G8B8:
            case D3DFORMAT.X8R8G8B8:
            case D3DFORMAT.A2B10G10R10:
            case D3DFORMAT.A8B8G8R8:
            case D3DFORMAT.X8B8G8R8:
            case D3DFORMAT.G16R16:
            case D3DFORMAT.A2R10G10B10:
            case D3DFORMAT.Q8W8V8U8:
            case D3DFORMAT.V16U16:
            case D3DFORMAT.X8L8V8U8:
            case D3DFORMAT.A2W10V10U10:
            case D3DFORMAT.D32:
            case D3DFORMAT.D24S8:
            case D3DFORMAT.D24X8:
            case D3DFORMAT.D24X4S4:
            case D3DFORMAT.D32F_LOCKABLE:
            case D3DFORMAT.D24FS8:
            case D3DFORMAT.INDEX32:
            case D3DFORMAT.G16R16F:
            case D3DFORMAT.R32F:
            case D3DFORMAT.D32_LOCKABLE:
                return 32;

            case D3DFORMAT.R8G8B8:
                return 24;

            case D3DFORMAT.A4R4G4B4:
            case D3DFORMAT.X4R4G4B4:
            case D3DFORMAT.R5G6B5:
            case D3DFORMAT.L16:
            case D3DFORMAT.A8L8:
            case D3DFORMAT.X1R5G5B5:
            case D3DFORMAT.A1R5G5B5:
            case D3DFORMAT.A8R3G3B2:
            case D3DFORMAT.V8U8:
            case D3DFORMAT.CxV8U8:
            case D3DFORMAT.L6V5U5:
            case D3DFORMAT.G8R8_G8B8:
            case D3DFORMAT.R8G8_B8G8:
            case D3DFORMAT.D16_LOCKABLE:
            case D3DFORMAT.D15S1:
            case D3DFORMAT.D16:
            case D3DFORMAT.INDEX16:
            case D3DFORMAT.R16F:
            case D3DFORMAT.YUY2:
            // From DX docs, reference/d3d/enums/d3dformat.asp
            // (note how it says that D3DFMT_R8G8_B8G8 is "A 16-bit packed RGB format analogous to UYVY (U0Y0, V0Y1, U2Y2, and so on)")
            case D3DFORMAT.UYVY:
                return 16;

            case D3DFORMAT.R3G3B2:
            case D3DFORMAT.A8:
            case D3DFORMAT.A8P8:
            case D3DFORMAT.P8:
            case D3DFORMAT.L8:
            case D3DFORMAT.A4L4:
            case D3DFORMAT.DXT2:
            case D3DFORMAT.DXT3:
            case D3DFORMAT.DXT4:
            case D3DFORMAT.DXT5:
            // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/directshow/htm/directxvideoaccelerationdxvavideosubtypes.asp
            case D3DFORMAT.AI44:
            case D3DFORMAT.IA44:
            case D3DFORMAT.S8_LOCKABLE:
                return 8;

            case D3DFORMAT.DXT1:
                return 4;

            case D3DFORMAT.YV12:
                return 12;

            case D3DFORMAT.A1:
                return 1;

            default:
                return 0;
        }
    }

    public static bool IsTextureFormatCompressed(uint format)
    {
        switch (format)
        {
            // DXGI formats
            case (uint)DXGI_FORMAT.BC1_TYPELESS:
            case (uint)DXGI_FORMAT.BC1_UNORM:
            case (uint)DXGI_FORMAT.BC1_UNORM_SRGB:
            case (uint)DXGI_FORMAT.BC2_TYPELESS:
            case (uint)DXGI_FORMAT.BC2_UNORM:
            case (uint)DXGI_FORMAT.BC2_UNORM_SRGB:
            case (uint)DXGI_FORMAT.BC3_TYPELESS:
            case (uint)DXGI_FORMAT.BC3_UNORM:
            case (uint)DXGI_FORMAT.BC3_UNORM_SRGB:
            case (uint)DXGI_FORMAT.BC4_TYPELESS:
            case (uint)DXGI_FORMAT.BC4_UNORM:
            case (uint)DXGI_FORMAT.BC4_SNORM:
            case (uint)DXGI_FORMAT.BC5_TYPELESS:
            case (uint)DXGI_FORMAT.BC5_UNORM:
            case (uint)DXGI_FORMAT.BC5_SNORM:
            case (uint)DXGI_FORMAT.BC6H_TYPELESS:
            case (uint)DXGI_FORMAT.BC6H_UF16:
            case (uint)DXGI_FORMAT.BC6H_SF16:
            case (uint)DXGI_FORMAT.BC7_TYPELESS:
            case (uint)DXGI_FORMAT.BC7_UNORM:
            case (uint)DXGI_FORMAT.BC7_UNORM_SRGB:

            // D3D9 formats
            case (uint)D3DFORMAT.DXT1:
            case (uint)D3DFORMAT.DXT2:
            case (uint)D3DFORMAT.DXT3:
            case (uint)D3DFORMAT.DXT4:
            case (uint)D3DFORMAT.DXT5:

            // FourCC formats
            // Note: Other FourCC compressed formats do exist, but they are rare: https://github.com/microsoft/DirectXTex/blob/fa22a4ec53dcc67505e66eca0c788ad8feed6b34/DirectXTex/DirectXTexDDS.cpp#L60
            // TODO: Refactor code
            case 0x31495441: // "ATI1"
            case 0x32495441: // "ATI2"
            case 0x55344342: // "BC4U"
            case 0x53344342: // "BC4S"
            case 0x55354342: // "BC5U"
            case 0x53354342: // "BC5S"
                return true;
        }

        return false;
    }

    public static uint GetDXGICompressionBlockSize(uint format)
    {
        switch (format)
        {
            case (uint)D3DFORMAT.DXT1:
            case (uint)DXGI_FORMAT.BC1_TYPELESS:
            case (uint)DXGI_FORMAT.BC1_UNORM:
            case (uint)DXGI_FORMAT.BC1_UNORM_SRGB:
            case (uint)DXGI_FORMAT.BC4_TYPELESS:
            case (uint)DXGI_FORMAT.BC4_UNORM:
            case (uint)DXGI_FORMAT.BC4_SNORM:
            case 0x31495441: // "ATI1"
            case 0x55344342: // "BC4U"
            case 0x53344342: // "BC4S"
                return 8;
        }

        return 16;
    }
}
