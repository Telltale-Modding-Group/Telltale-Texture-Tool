﻿using System;
using System.Collections.Generic;
using System.IO;
using TelltaleTextureTool.TelltaleEnums;
using TelltaleTextureTool.TelltaleTypes;
using TelltaleTextureTool.Utilities;
using TelltaleTextureTool.DirectX;
using System.Runtime.InteropServices;
using TelltaleTextureTool.DirectX.Enums;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;
using System.Text;

/*
 * NOTE:
 * 
 * This version of D3DTX is COMPLETE.
 * 
 * COMPLETE meaning that all of the data is known and getting identified.
 * Just like the versions before and after, this D3DTX version derives from version 9 and has been 'stripped' or adjusted to suit this version of D3DTX.
 * Also, Telltale uses Hungarian Notation for variable naming.
*/

/* --- D3DTX Version 7 games ---
 * The Walking Dead: Michonne (TESTED)
 * Tales from the Borderlands (Re-Release?) (TESTED)
*/

namespace TelltaleTextureTool.TelltaleD3DTX;

/// <summary>
/// This is a custom class that matches what is serialized in a D3DTX version 7 class. (INCOMPLETE)
/// </summary>
public class D3DTX_V7 : ID3DTX
{
    /// <summary>
    /// [4 bytes] The header version of this class.
    /// </summary>
    public int mVersion { get; set; }

    /// <summary>
    /// [4 bytes] The mSamplerState state block size in bytes. Note: the parsed value is always 8.
    /// </summary>
    public int mSamplerState_BlockSize { get; set; }

    /// <summary>
    /// [4 bytes] The sampler state, bitflag value that contains values from T3SamplerStateValue.
    /// </summary>
    public T3SamplerStateBlock mSamplerState { get; set; }

    /// <summary>
    /// [4 bytes] The mPlatform block size in bytes. Note: the parsed value is always 8.
    /// </summary>
    public int mPlatform_BlockSize { get; set; }

    /// <summary>
    /// [4 bytes] The platform type enum value.
    /// </summary>
    public T3PlatformType mPlatform { get; set; }

    /// <summary>
    /// [4 bytes] The mName block size in bytes.
    /// </summary>
    public int mName_BlockSize { get; set; }

    /// <summary>
    /// [mName_StringLength bytes] The string mName.
    /// </summary>
    public string mName { get; set; } = string.Empty;

    /// <summary>
    /// [4 bytes] The mImportName block size in bytes.
    /// </summary>
    public int mImportName_BlockSize { get; set; }

    /// <summary>
    /// [mImportName_StringLength bytes] The mImportName string.
    /// </summary>
    public string mImportName { get; set; } = string.Empty;

    /// <summary>
    /// [4 bytes] The import scale of the texture file.
    /// </summary>
    public float mImportScale { get; set; }

    /// <summary>
    /// [1 byte] Whether or not the d3dtx contains a Tool Properties. [PropertySet] (Always false)
    /// </summary>
    public ToolProps mToolProps { get; set; }

    /// <summary>
    /// [4 bytes] The number of mip maps in the texture.
    /// </summary>
    public uint mNumMipLevels { get; set; }

    /// <summary>
    /// [4 bytes] The pixel width of the texture.
    /// </summary>
    public uint mWidth { get; set; }

    /// <summary>
    /// [4 bytes] The pixel height of the texture.
    /// </summary>
    public uint mHeight { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines the compression used for the texture.
    /// </summary>
    public T3SurfaceFormat mSurfaceFormat { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines the dimension of the texture.
    /// </summary>
    public T3TextureLayout mTextureLayout { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines the gamma of the texture.
    /// </summary>
    public T3SurfaceGamma mSurfaceGamma { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines the resource type of the texture.
    /// </summary>
    public T3ResourceUsage mResourceUsage { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines what kind of texture it is.
    /// </summary>
    public T3TextureType mType { get; set; }

    /// <summary>
    /// [4 bytes] The size of the mSwizzle block data.
    /// </summary>
    public int mSwizzleSize { get; set; }

    /// <summary>
    /// [4 bytes] mSwizzle compression parameters. (usually used for consoles or mobile).
    /// </summary>
    public RenderSwizzleParams mSwizzle { get; set; }

    /// <summary>
    /// [4 bytes] Defines how glossy the texture is.
    /// </summary>
    public float mSpecularGlossExponent { get; set; }

    /// <summary>
    /// [4 bytes] Defines the brightness scale of the texture. (used for lightmaps)
    /// </summary>
    public float mHDRLightmapScale { get; set; }

    /// <summary>
    /// [4 bytes] Defines the toon cutoff gradient of the texture.
    /// </summary>
    public float mToonGradientCutoff { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines what kind of alpha the texture will have.
    /// </summary>
    public T3TextureAlphaMode mAlphaMode { get; set; }

    /// <summary>
    /// [4 bytes] An enum, defines the color range of the texture.
    /// </summary>
    public T3TextureColor mColorMode { get; set; }

    /// <summary>
    /// [8 bytes] A vector, defines the UV offset values when the shader on a material samples the texture.
    /// </summary>
    public Vector2 mUVOffset { get; set; }

    /// <summary>
    /// [8 bytes] A vector, defines the UV scale values when the shader on a material samples the texture.
    /// </summary>
    public Vector2 mUVScale { get; set; }

    /// <summary>
    /// [4 bytes] The size in bytes of the mToonRegions block. ??
    /// </summary>
    public uint mToonRegions_ArrayCapacity { get; set; }

    /// <summary>
    /// [4 bytes] The amount of elements in the mToonRegions array.
    /// </summary>
    public int mToonRegions_ArrayLength { get; set; }

    /// <summary>
    /// [16 bytes for each element] An array containing a toon gradient region.
    /// </summary>
    public T3ToonGradientRegion[] mToonRegions { get; set; } = [];

    /// <summary>
    /// [12 bytes] A struct for StreamHeader
    /// </summary>
    public StreamHeader mStreamHeader { get; set; }

    /// <summary>
    /// [24 bytes for each element] An array containing each pixel region in the texture.
    /// </summary>
    public RegionStreamHeader[] mRegionHeaders { get; set; } = [];

    /// <summary>
    /// A byte array of the pixel regions in a texture. Starts from smallest mip map to largest mip map.
    /// </summary>
    public List<byte[]> mPixelData { get; set; } = [];

    /// <summary>
    /// D3DTX V7 Header (empty constructor, only used for json deserialization)
    /// </summary>
    public D3DTX_V7() { }

    public void WriteToBinary(BinaryWriter writer, TelltaleToolGame game = TelltaleToolGame.DEFAULT, T3PlatformType platform = T3PlatformType.ePlatform_None, bool printDebug = false)
    {
        writer.Write(mVersion); //mVersion [4 bytes]
        writer.Write(mSamplerState_BlockSize); //mSamplerState Block Size [4 bytes]
        writer.Write(mSamplerState.mData); //mSamplerState mData [4 bytes] 
        writer.Write(mPlatform_BlockSize); //mPlatform Block Size [4 bytes]
        writer.Write((int)mPlatform); //mPlatform [4 bytes]
        writer.Write(mName_BlockSize); //mName Block Size [4 bytes] //mName block size (size + string len)
        ByteFunctions.WriteString(writer, mName); //mName [x bytes]
        writer.Write(mImportName_BlockSize); //mImportName Block Size [4 bytes] //mImportName block size (size + string len)
        ByteFunctions.WriteString(writer, mImportName); //mImportName [x bytes] (this is always 0)
        writer.Write(mImportScale); //mImportScale [4 bytes]
        ByteFunctions.WriteBoolean(writer, mToolProps.mbHasProps); //mToolProps mbHasProps [1 byte]
        writer.Write(mNumMipLevels); //mNumMipLevels [4 bytes]
        writer.Write(mWidth); //mWidth [4 bytes]
        writer.Write(mHeight); //mHeight [4 bytes]
        writer.Write((int)mSurfaceFormat); //mSurfaceFormat [4 bytes]
        writer.Write((int)mTextureLayout); //mTextureLayout [4 bytes]
        writer.Write((int)mSurfaceGamma); //mSurfaceGamma [4 bytes]
        writer.Write((int)mResourceUsage); //mResourceUsage [4 bytes]
        writer.Write((int)mType); //mType [4 bytes]
        writer.Write(mSwizzleSize); //mSwizzleSize [4 bytes]
        writer.Write(mSwizzle.mSwizzle1); //mSwizzle A [1 byte]
        writer.Write(mSwizzle.mSwizzle1); //mSwizzle B [1 byte]
        writer.Write(mSwizzle.mSwizzle1); //mSwizzle C [1 byte]
        writer.Write(mSwizzle.mSwizzle1); //mSwizzle D [1 byte]
        writer.Write(mSpecularGlossExponent); //mSpecularGlossExponent [4 bytes]
        writer.Write(mHDRLightmapScale); //mHDRLightmapScale [4 bytes]
        writer.Write(mToonGradientCutoff); //mToonGradientCutoff [4 bytes]
        writer.Write((int)mAlphaMode); //mAlphaMode [4 bytes]
        writer.Write((int)mColorMode); //mColorMode [4 bytes]
        writer.Write(mUVOffset.x); //mUVOffset X [4 bytes]
        writer.Write(mUVOffset.y); //mUVOffset Y [4 bytes]
        writer.Write(mUVScale.x); //mUVScale X [4 bytes]
        writer.Write(mUVScale.y); //mUVScale Y [4 bytes]

        writer.Write(mToonRegions_ArrayCapacity); //mToonRegions DCArray Capacity [4 bytes]
        writer.Write(mToonRegions_ArrayLength); //mToonRegions DCArray Length [4 bytes]
        for (int i = 0; i < mToonRegions_ArrayLength; i++)
        {
            writer.Write(mToonRegions[i].mColor.r); //[4 bytes]
            writer.Write(mToonRegions[i].mColor.g); //[4 bytes]
            writer.Write(mToonRegions[i].mColor.b); //[4 bytes]
            writer.Write(mToonRegions[i].mColor.a); //[4 bytes]
            writer.Write(mToonRegions[i].mSize); //[4 bytes]
        }

        writer.Write(mStreamHeader.mRegionCount); //mRegionCount [4 bytes]
        writer.Write(mStreamHeader.mAuxDataCount); //mAuxDataCount [4 bytes]
        writer.Write(mStreamHeader.mTotalDataSize); //mTotalDataSize [4 bytes]

        for (int i = 0; i < mStreamHeader.mRegionCount; i++)
        {
            writer.Write(mRegionHeaders[i].mFaceIndex); //[4 bytes]
            writer.Write(mRegionHeaders[i].mMipIndex); //[4 bytes]
            writer.Write(mRegionHeaders[i].mMipCount); //[4 bytes]
            writer.Write(mRegionHeaders[i].mDataSize); //[4 bytes]
            writer.Write(mRegionHeaders[i].mPitch); //[4 bytes]
        }

        for (int i = 0; i < mPixelData.Count; i++)
        {
            writer.Write(mPixelData[i]);
        }
    }

    public void ReadFromBinary(BinaryReader reader, TelltaleToolGame game = TelltaleToolGame.DEFAULT, T3PlatformType platform = T3PlatformType.ePlatform_None, bool printDebug = false)
    {
        mVersion = reader.ReadInt32(); //mVersion [4 bytes]
        mSamplerState_BlockSize = reader.ReadInt32(); //mSamplerState Block Size [4 bytes]
        mSamplerState = new T3SamplerStateBlock() //mSamplerState [4 bytes]
        {
            mData = reader.ReadUInt32()
        };
        mPlatform_BlockSize = reader.ReadInt32(); //mPlatform Block Size [4 bytes]
        mPlatform = (T3PlatformType)reader.ReadInt32(); //mPlatform [4 bytes]
        mName_BlockSize = reader.ReadInt32(); //mName Block Size [4 bytes] //mName block size (size + string len)
        mName = ByteFunctions.ReadString(reader); //mName [x bytes]
        mImportName_BlockSize = reader.ReadInt32(); //mImportName Block Size [4 bytes] //mImportName block size (size + string len)
        mImportName = ByteFunctions.ReadString(reader); //mImportName [x bytes] (this is always 0)
        mImportScale = reader.ReadSingle(); //mImportScale [4 bytes]
        mToolProps = new ToolProps(reader); //mToolProps [1 byte]
        mNumMipLevels = reader.ReadUInt32(); //mNumMipLevels [4 bytes]
        mWidth = reader.ReadUInt32(); //mWidth [4 bytes]
        mHeight = reader.ReadUInt32(); //mHeight [4 bytes]
        mSurfaceFormat = (T3SurfaceFormat)reader.ReadInt32(); //mSurfaceFormat [4 bytes]
        mTextureLayout = (T3TextureLayout)reader.ReadInt32(); //mTextureLayout [4 bytes]
        mSurfaceGamma = (T3SurfaceGamma)reader.ReadInt32(); //mSurfaceGamma [4 bytes]
        mResourceUsage = (T3ResourceUsage)reader.ReadInt32(); //mResourceUsage [4 bytes]
        mType = (T3TextureType)reader.ReadInt32(); //mType [4 bytes]
        mSwizzleSize = reader.ReadInt32(); //mSwizzleSize [4 bytes]
        mSwizzle = new(reader); //mSwizzle [4 bytes]
        mSpecularGlossExponent = reader.ReadSingle(); //mSpecularGlossExponent [4 bytes]
        mHDRLightmapScale = reader.ReadSingle(); //mHDRLightmapScale [4 bytes]
        mToonGradientCutoff = reader.ReadSingle(); //mToonGradientCutoff [4 bytes]
        mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32(); //mAlphaMode [4 bytes]
        mColorMode = (T3TextureColor)reader.ReadInt32(); //mColorMode [4 bytes]

        mUVOffset = new Vector2() //mUVOffset [8 bytes]
        {
            x = reader.ReadSingle(), //[4 bytes]
            y = reader.ReadSingle() //[4 bytes]
        };
        mUVScale = new Vector2() //mUVScale [8 bytes]
        {
            x = reader.ReadSingle(), //[4 bytes]
            y = reader.ReadSingle() //[4 bytes]
        };

        //--------------------------mToonRegions--------------------------
        mToonRegions_ArrayCapacity = reader.ReadUInt32(); //mToonRegions DCArray Capacity [4 bytes]
        mToonRegions_ArrayLength = reader.ReadInt32(); //mToonRegions DCArray Length [4 bytes]
        mToonRegions = new T3ToonGradientRegion[mToonRegions_ArrayLength];

        for (int i = 0; i < mToonRegions_ArrayLength; i++)
        {
            mToonRegions[i] = new T3ToonGradientRegion()
            {
                mColor = new Color()
                {
                    r = reader.ReadSingle(), //[4 bytes]
                    g = reader.ReadSingle(), //[4 bytes]
                    b = reader.ReadSingle(), //[4 bytes]
                    a = reader.ReadSingle() //[4 bytes]
                },

                mSize = reader.ReadSingle() //[4 bytes]
            };
        }

        //--------------------------StreamHeader--------------------------
        mStreamHeader = new StreamHeader()
        {
            mRegionCount = reader.ReadInt32(), //[4 bytes]
            mAuxDataCount = reader.ReadInt32(), //[4 bytes]
            mTotalDataSize = reader.ReadInt32() //[4 bytes]
        };

        //--------------------------mRegionHeaders--------------------------
        mRegionHeaders = new RegionStreamHeader[mStreamHeader.mRegionCount];
        for (int i = 0; i < mStreamHeader.mRegionCount; i++)
        {
            mRegionHeaders[i] = new RegionStreamHeader()
            {
                mFaceIndex = reader.ReadInt32(), //[4 bytes]
                mMipIndex = reader.ReadInt32(), //[4 bytes] 
                mMipCount = reader.ReadInt32(), //[4 bytes]
                mDataSize = reader.ReadUInt32(), //[4 bytes]
                mPitch = reader.ReadInt32(), //[4 bytes]
            };
        }
        //-----------------------------------------D3DTX HEADER END-----------------------------------------
        //--------------------------STORING D3DTX IMAGE DATA--------------------------
        mPixelData = [];

        for (int i = 0; i < mStreamHeader.mRegionCount; i++)
        {
            int dataSize = (int)mRegionHeaders[i].mDataSize;
            byte[] imageData = reader.ReadBytes(dataSize);

            mPixelData.Add(imageData);
        }

        if (printDebug)
            PrintConsole();
    }

    public void ModifyD3DTX(D3DTXMetadata metadata, ImageSection[] imageSections, bool printDebug = false)
    {
        mWidth = metadata.Width;
        mHeight = metadata.Height;
        mSurfaceFormat = metadata.Format;
        mNumMipLevels = metadata.MipLevels;
        mSurfaceGamma = metadata.SurfaceGamma;

        mPixelData.Clear();
        mPixelData = TextureManager.GetPixelDataListFromSections(imageSections);

        mStreamHeader = new()
        {
            mRegionCount = imageSections.Length,
            mAuxDataCount = mStreamHeader.mAuxDataCount,
            mTotalDataSize = (int)ByteFunctions.GetByteArrayListElementsCount(mPixelData)
        };

        mRegionHeaders = new RegionStreamHeader[mStreamHeader.mRegionCount];

        for (int i = 0; i < mStreamHeader.mRegionCount; i++)
        {
            mRegionHeaders[i] = new()
            {
                mDataSize = (uint)mPixelData[i].Length,
                mMipCount = 1, // mMipCount is a strange variable, it is always 1 for every single texture
                mPitch = (int)imageSections[i].RowPitch,
            };
        }

        if (metadata.IsCubemap())
        {
            if (metadata.ArraySize > 6)
            {
                throw new Exception("Cubemap array textures are not supported on this version!");
            }
            mTextureLayout = T3TextureLayout.TextureCubemap;

            int interval = mStreamHeader.mRegionCount / (int)mNumMipLevels;
            // Example a cube array textures with 5 mips will have 30 regions (6 faces * 5 mips)
            // If the array is 2 element there will be 60 regions (6 faces * 5 mips * 2 elements)
            // The mip index will be the region index % interval
            for (int i = 0; i < mStreamHeader.mRegionCount; i++)
            {
                mRegionHeaders[i].mFaceIndex = i % 6;
                mRegionHeaders[i].mMipIndex = (mStreamHeader.mRegionCount - i - 1) / interval;
            }
        }
        else if (metadata.IsVolumemap())
        {
            throw new ArgumentException("Volumemap textures are not supported on this version!");
        }
        else
        {
            if (metadata.ArraySize > 1)
            {
                throw new ArgumentException("2D Array textures are not supported on this version!");
            }

            mTextureLayout = T3TextureLayout.Texture2D;

            for (int i = 0; i < mStreamHeader.mRegionCount; i++)
            {
                mRegionHeaders[i].mFaceIndex = 0;
                mRegionHeaders[i].mMipIndex = mStreamHeader.mRegionCount - i - 1;
            }
        }

        UpdateArrayCapacities();
        PrintConsole();
    }

    public D3DTXMetadata GetD3DTXMetadata()
    {
        D3DTXMetadata metadata = new()
        {
            TextureName = mName,
            Width = mWidth,
            Height = mHeight,
            Format = mSurfaceFormat,
            MipLevels = mNumMipLevels,
            SurfaceGamma = mSurfaceGamma,
            Dimension = mTextureLayout,
            AlphaMode = mAlphaMode,
            Platform = mPlatform,
            TextureType = mType,
            RegionHeaders = mRegionHeaders,
            D3DFormat = LegacyFormat.UNKNOWN,
        };

        return metadata;
    }

    public List<byte[]> GetPixelData()
    {
        return mPixelData;
    }

    public string GetDebugInfo(TelltaleToolGame game = TelltaleToolGame.DEFAULT, T3PlatformType platform = T3PlatformType.ePlatform_None)
    {
        StringBuilder d3dtxInfo = new();

        d3dtxInfo.AppendLine("||||||||||| D3DTX Version 7 Header |||||||||||");
        d3dtxInfo.AppendFormat("mVersion: {0}", mVersion).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mSamplerState_BlockSize: {0}", mSamplerState_BlockSize).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mSamplerState: {0}", mSamplerState).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mPlatform_BlockSize: {0}", mPlatform_BlockSize).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mPlatform: {0} ({1})", Enum.GetName(typeof(T3PlatformType), (int)mPlatform), mPlatform).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mName Block Size: {0}", mName_BlockSize).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mName: {0}", mName).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mImportName Block Size: {0}", mImportName_BlockSize).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mImportName: {0}", mImportName).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mImportScale: {0}", mImportScale).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mToolProps: {0}", mToolProps).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mNumMipLevels: {0}", mNumMipLevels).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mWidth: {0}", mWidth).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mHeight: {0}", mHeight).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mSurfaceFormat: {0} ({1})", Enum.GetName(typeof(T3SurfaceFormat), mSurfaceFormat), (int)mSurfaceFormat).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mTextureLayout: {0} ({1})", Enum.GetName(typeof(T3TextureLayout), mTextureLayout), (int)mTextureLayout).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mSurfaceGamma: {0} ({1})", Enum.GetName(typeof(T3SurfaceGamma), mSurfaceGamma), (int)mSurfaceGamma).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mResourceUsage: {0} ({1})", Enum.GetName(typeof(T3ResourceUsage), mResourceUsage), (int)mResourceUsage).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mType: {0} ({1})", Enum.GetName(typeof(T3TextureType), mType), (int)mType).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mSwizzleSize: {0}", mSwizzleSize).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mSwizzle: {0}", mSwizzle).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mSpecularGlossExponent: {0}", mSpecularGlossExponent).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mHDRLightmapScale: {0}", mHDRLightmapScale).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mToonGradientCutoff: {0}", mToonGradientCutoff).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mAlphaMode: {0} ({1})", Enum.GetName(typeof(T3TextureAlphaMode), mAlphaMode), (int)mAlphaMode).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mColorMode: {0} ({1})", Enum.GetName(typeof(T3TextureColor), mColorMode), (int)mColorMode).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mUVOffset: {0}", mUVOffset).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mUVScale: {0}", mUVScale).Append(Environment.NewLine);

        d3dtxInfo.AppendLine("----------- mToonRegions -----------");
        d3dtxInfo.AppendFormat("mToonRegions_ArrayCapacity: {0}", mToonRegions_ArrayCapacity).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mToonRegions_ArrayLength: {0}", mToonRegions_ArrayLength).Append(Environment.NewLine);
        for (int i = 0; i < mToonRegions_ArrayLength; i++)
        {
            d3dtxInfo.AppendFormat("mToonRegion {0}: {1}", i, mToonRegions[i]).Append(Environment.NewLine);
        }

        d3dtxInfo.AppendLine("----------- mStreamHeader -----------");
        d3dtxInfo.AppendFormat("mRegionCount: {0}", mStreamHeader.mRegionCount).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mAuxDataCount: {0}", mStreamHeader.mAuxDataCount).Append(Environment.NewLine);
        d3dtxInfo.AppendFormat("mTotalDataSize: {0}", mStreamHeader.mTotalDataSize).Append(Environment.NewLine);

        d3dtxInfo.AppendLine("----------- mRegionHeaders -----------");
        for (int i = 0; i < mStreamHeader.mRegionCount; i++)
        {
            d3dtxInfo.AppendFormat("[mRegionHeader {0}]", i).Append(Environment.NewLine);
            d3dtxInfo.AppendFormat("mFaceIndex: {0}", mRegionHeaders[i].mFaceIndex).Append(Environment.NewLine);
            d3dtxInfo.AppendFormat("mMipIndex: {0}", mRegionHeaders[i].mMipIndex).Append(Environment.NewLine);
            d3dtxInfo.AppendFormat("mMipCount: {0}", mRegionHeaders[i].mMipCount).Append(Environment.NewLine);
            d3dtxInfo.AppendFormat("mDataSize: {0}", mRegionHeaders[i].mDataSize).Append(Environment.NewLine);
            d3dtxInfo.AppendFormat("mPitch: {0}", mRegionHeaders[i].mPitch).Append(Environment.NewLine);
        }

        return d3dtxInfo.ToString();
    }

    public void UpdateArrayCapacities()
    {
        mToonRegions_ArrayCapacity = 8 + (uint)(20 * mToonRegions.Length);
        mToonRegions_ArrayLength = mToonRegions.Length;
    }

    public uint GetHeaderByteSize()
    {
        uint totalSize = 0;

        totalSize += (uint)Marshal.SizeOf(mVersion); //mVersion [4 bytes]
        totalSize += (uint)Marshal.SizeOf(mSamplerState_BlockSize); //mSamplerState Block Size [4 bytes]
        totalSize += mSamplerState.GetByteSize(); //mSamplerState mData [4 bytes] 
        totalSize += (uint)Marshal.SizeOf(mPlatform_BlockSize); //mPlatform Block Size [4 bytes]
        totalSize += (uint)Marshal.SizeOf((int)mPlatform); //mPlatform [4 bytes]
        totalSize += (uint)Marshal.SizeOf(mName_BlockSize); //mName Block Size [4 bytes] //mName block size (size + string len)
        totalSize += (uint)Marshal.SizeOf(mName.Length); //mName (strength length prefix) [4 bytes]
        totalSize += (uint)mName.Length;  //mName [x bytes]
        totalSize += (uint)Marshal.SizeOf(mImportName_BlockSize); //mImportName Block Size [4 bytes] //mImportName block size (size + string len)
        totalSize += (uint)Marshal.SizeOf(mImportName.Length); //mImportName (strength length prefix) [4 bytes] (this is always 0)
        totalSize += (uint)mImportName.Length; //mImportName [x bytes] (this is always 0)
        totalSize += (uint)Marshal.SizeOf(mImportScale); //mImportScale [4 bytes]
        totalSize += mToolProps.GetByteSize();
        totalSize += (uint)Marshal.SizeOf(mNumMipLevels); //mNumMipLevels [4 bytes]
        totalSize += (uint)Marshal.SizeOf(mWidth); //mWidth [4 bytes]
        totalSize += (uint)Marshal.SizeOf(mHeight); //mHeight [4 bytes]
        totalSize += (uint)Marshal.SizeOf((int)mSurfaceFormat); //mSurfaceFormat [4 bytes]
        totalSize += (uint)Marshal.SizeOf((int)mTextureLayout); //mTextureLayout [4 bytes]
        totalSize += (uint)Marshal.SizeOf((int)mSurfaceGamma); //mSurfaceGamma [4 bytes]
        totalSize += (uint)Marshal.SizeOf((int)mResourceUsage); //mResourceUsage [4 bytes]
        totalSize += (uint)Marshal.SizeOf((int)mType); //mType [4 bytes]
        totalSize += (uint)Marshal.SizeOf(mSwizzleSize); //mSwizzleSize [4 bytes]
        totalSize += mSwizzle.GetByteSize();
        totalSize += (uint)Marshal.SizeOf(mSpecularGlossExponent); //mSpecularGlossExponent [4 bytes]
        totalSize += (uint)Marshal.SizeOf(mHDRLightmapScale); //mHDRLightmapScale [4 bytes]
        totalSize += (uint)Marshal.SizeOf(mToonGradientCutoff); //mToonGradientCutoff [4 bytes]
        totalSize += (uint)Marshal.SizeOf((int)mAlphaMode); //mAlphaMode [4 bytes]
        totalSize += (uint)Marshal.SizeOf((int)mColorMode); //mColorMode [4 bytes]
        totalSize += mUVOffset.GetByteSize(); //[4 bytes]
        totalSize += mUVScale.GetByteSize(); //[4 bytes]

        totalSize += (uint)Marshal.SizeOf(mToonRegions_ArrayCapacity); //mToonRegions DCArray Capacity [4 bytes]
        totalSize += (uint)Marshal.SizeOf(mToonRegions_ArrayLength); //mToonRegions DCArray Length [4 bytes]
        for (int i = 0; i < mToonRegions_ArrayLength; i++)
        {
            totalSize += mToonRegions[i].GetByteSize();
        }

        totalSize += mStreamHeader.GetByteSize();

        for (int i = 0; i < mStreamHeader.mRegionCount; i++)
        {
            totalSize += 4; //[4 bytes]
            totalSize += 4; //[4 bytes]
            totalSize += 4; //[4 bytes]
            totalSize += 4; //[4 bytes]
            totalSize += 4; //[4 bytes]
            totalSize += 4; //[4 bytes]
        }

        return totalSize;
    }

    public void PrintConsole()
    {
        Console.WriteLine(GetDebugInfo());
    }
}
