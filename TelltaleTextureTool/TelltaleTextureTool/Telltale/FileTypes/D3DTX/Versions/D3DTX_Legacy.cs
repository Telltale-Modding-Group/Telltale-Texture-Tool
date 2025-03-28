using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.DirectX.Enums;
using TelltaleTextureTool.Main;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;
using TelltaleTextureTool.TelltaleEnums;
using TelltaleTextureTool.TelltaleTypes;
using TelltaleTextureTool.Utilities;

/*
 * NOTE:
 *
 * This version of D3DTX is COMPLETE.
 *
 * COMPLETE meaning that all of the data is known and getting identified.
 * Just like the versions before and after, this D3DTX version derives from version 9 and has been 'stripped' or adjusted to suit this version of D3DTX.
 * Also, Telltale uses Hungarian Notation for variable naming.
*/

/* - D3DTX Legacy Version  Games
- All games before The Walking Dead: Season One
*/

namespace TelltaleTextureTool.TelltaleD3DTX
{
    /// <summary>
    /// This is a custom class that matches what is serialized in a legacy D3DTX version supporting the listed titles. (COMPLETE)
    /// </summary>
    public class D3DTX_Legacy : ID3DTX
    {
        /// <summary>
        /// [4 bytes] The mSamplerState state block size in bytes. Note: the parsed value is always 8.
        /// </summary>
        public int mSamplerState_BlockSize { get; set; }

        /// <summary>
        /// [4 bytes] The sampler state, bitflag value that contains values from T3SamplerStateValue.
        /// </summary>
        public T3SamplerStateBlock mSamplerState { get; set; }

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
        /// [1 byte] Whether or not the d3dtx contains a Tool Properties. [PropertySet] (Always false)
        /// </summary>
        public ToolProps mToolProps { get; set; }

        /// <summary>
        /// [1 byte] Indicates whether or not the texture contains mips. (what? need further research)
        /// </summary>
        public TelltaleBoolean mbHasTextureData { get; set; }

        /// <summary>
        /// [1 byte] Indicates whether or not the texture contains mips.
        /// </summary>
        public TelltaleBoolean mbIsMipMapped { get; set; }

        /// <summary>
        /// [1 byte] Indicates if the texture is wrapped horizontally.
        /// </summary>
        public TelltaleBoolean mbIsWrapU { get; set; }

        /// <summary>
        /// [1 byte] Indicates if the texture is wrapped vertically.
        /// </summary>
        public TelltaleBoolean mbIsWrapV { get; set; }

        /// <summary>
        /// [1 byte] Indicates if the texture is filtered.
        /// </summary>
        public TelltaleBoolean mbIsFiltered { get; set; }

        /// <summary>
        /// [1 byte] Indicates if the texture contains embedded mips.
        /// </summary>
        public TelltaleBoolean mbEmbedMipMaps { get; set; }

        /// <summary>
        /// [4 bytes] Number of mips in the texture.
        /// </summary>
        public uint mNumMipLevels { get; set; }

        /// <summary>
        /// [4 bytes] The old T3SurfaceFormat. Makes use of FourCC but it can be an integer as well. Enums could not be found.
        /// </summary>
        public LegacyFormat mD3DFormat { get; set; }

        /// <summary>
        /// [4 bytes] The pixel width of the texture.
        /// </summary>
        public uint mWidth { get; set; }

        /// <summary>
        /// [4 bytes] The pixel height of the texture.
        /// </summary>
        public uint mHeight { get; set; }

        /// <summary>
        /// [4 bytes] Indicates the texture flags using bitwise OR operation. 0x1 is "Low quality", 0x2 is "Locked size" and 0x4 is "Generated mips".
        /// </summary>
        public uint mFlags { get; set; }

        /// <summary>
        /// [4 bytes] The pixel width of the texture when loaded on Wii platform.
        /// </summary>
        public uint mWiiForceWidth { get; set; }

        /// <summary>
        /// [4 bytes] The pixel height of the texture when loaded on Wii platform.
        /// </summary>
        public uint mWiiForceHeight { get; set; }

        /// <summary>
        /// [1 byte] Whether or not the texture is forced to compressed when on.
        /// </summary>
        public TelltaleBoolean mbWiiForceUncompressed { get; set; }

        /// <summary>
        /// [4 bytes] The type of the texture. No enums were found, need more analyzing. Could be texture layout too.
        /// </summary>
        public uint mType { get; set; } //mTextureDataFormats?

        /// <summary>
        /// [4 bytes] The texture data format. No enums were found, need more analyzing. Could be a flag.
        /// </summary>
        public uint mTextureDataFormats { get; set; }

        /// <summary>
        /// [4 bytes] The TPL texture data size, used for Wii textures.
        /// </summary>
        public uint mTplTextureDataSize { get; set; }

        /// <summary>
        /// [4 bytes] The TPL alpha data size, used for Wii textures.
        /// </summary>
        public uint mTplAlphaDataSize { get; set; }

        /// <summary>
        /// [4 bytes] The JPEG texture data size.
        /// </summary>
        public uint mJPEGTextureDataSize { get; set; }

        /// <summary>
        /// [4 bytes] Defines the brightness scale of the texture. (used for lightmaps)
        /// </summary>
        public float mHDRLightmapScale { get; set; }

        /// <summary>
        /// [4 bytes] An enum, defines what kind of alpha the texture will have.
        /// </summary>
        public T3TextureAlphaMode mAlphaMode { get; set; }

        /// <summary>
        /// [4 bytes] An enum, defines what kind of *exact* alpha the texture will have. (no idea why this exists, wtf Telltale)
        /// </summary>
        public T3TextureAlphaMode mExactAlphaMode { get; set; }

        /// <summary>
        /// [4 bytes] An enum, defines the color range of the texture.
        /// </summary>
        public T3TextureColor mColorMode { get; set; }

        /// <summary>
        /// [4 bytes] The Wii texture format.
        /// </summary>
        public WiiTextureFormat mWiiTextureFormat { get; set; }

        /// <summary>
        /// [1 byte] Whether or not the texture has alpha HDR?
        /// </summary>
        public TelltaleBoolean mbAlphaHDR { get; set; }

        /// <summary>
        /// [1 byte] Whether or not the texture is encrypted.
        /// </summary>
        public TelltaleBoolean mbEncrypted { get; set; }

        /// <summary>
        /// [1 byte] Whether or not the texture is used as a bumpmap.
        /// </summary>
        public TelltaleBoolean mbUsedAsBumpmap { get; set; }

        /// <summary>
        /// [1 byte] Whether or not the texture is used as a detail map.
        /// </summary>
        public TelltaleBoolean mbUsedAsDetailMap { get; set; }

        /// <summary>
        /// [4 bytes] The detail map brightness.
        /// </summary>
        public float mDetailMapBrightness { get; set; }

        /// <summary>
        /// [4 bytes] The normal map format.
        /// </summary>
        public int mNormalMapFmt { get; set; }

        /// <summary>
        /// [8 bytes] A vector, defines the UV offset values when the shader on a material samples the texture.
        /// </summary>
        public Vector2 mUVOffset { get; set; }

        /// <summary>
        /// [8 bytes] A vector, defines the UV scale values when the shader on a material samples the texture.
        /// </summary>
        public Vector2 mUVScale { get; set; }

        /// <summary>
        /// [4 bytes] An empty buffer for legacy console editions. It should be always zero.
        /// </summary>
        public int mEmptyBuffer { get; set; }

        /// <summary>
        /// A byte array of the pixel regions in a texture.
        /// </summary>
        public TelltalePixelData mPixelData { get; set; }

        /// <summary>
        /// The TPL texture data.
        /// </summary>
        public byte[] mTplData { get; set; } = [];

        /// <summary>
        /// The TPL alpha data.
        /// </summary>
        public byte[] mTplAlphaData { get; set; } = [];

        /// <summary>
        /// The JPEG texture data.
        /// </summary>
        public byte[] mJPEGTextureData { get; set; } = [];

        public D3DTX_Legacy() { }

        public void WriteToBinary(
            BinaryWriter writer,
            TelltaleToolGame game = TelltaleToolGame.DEFAULT,
            T3PlatformType platform = T3PlatformType.ePlatform_None,
            bool printDebug = false
        )
        {
            if (game is TelltaleToolGame.DEFAULT || game is TelltaleToolGame.UNKNOWN)
            {
                throw new Exception("The game is not supported.");
            }

            if (game is TelltaleToolGame.TEXAS_HOLD_EM_OG)
            {
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
            }

            if (game is TelltaleToolGame.THE_WALKING_DEAD)
            {
                writer.Write(mSamplerState_BlockSize);
                writer.Write(mSamplerState.mData);
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mToolProps.mbHasProps);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mFlags);
                writer.Write(mWiiForceWidth);
                writer.Write(mWiiForceHeight);
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean);
                writer.Write(mType);
                writer.Write(mTextureDataFormats);
                writer.Write(mTplTextureDataSize);
                writer.Write(mTplAlphaDataSize);
                writer.Write(mJPEGTextureDataSize);
                writer.Write(mHDRLightmapScale);
                writer.Write((int)mAlphaMode);
                writer.Write((int)mExactAlphaMode);
                writer.Write((int)mColorMode);
                writer.Write((int)mWiiTextureFormat);
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean);
                writer.Write(mDetailMapBrightness);
                writer.Write(mNormalMapFmt);
                mUVOffset.WriteBinaryData(writer);
                mUVScale.WriteBinaryData(writer);
            }

            if (
                game
                is TelltaleToolGame.PUZZLE_AGENT_2
                    or TelltaleToolGame.LAW_AND_ORDER_LEGACIES
                    or TelltaleToolGame.JURASSIC_PARK_THE_GAME
            )
            {
                writer.Write(mSamplerState_BlockSize);
                writer.Write(mSamplerState.mData);
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mToolProps.mbHasProps);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mFlags);
                writer.Write(mWiiForceWidth);
                writer.Write(mWiiForceHeight);
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean);
                writer.Write(mType);
                writer.Write(mTextureDataFormats);
                writer.Write(mTplTextureDataSize);
                writer.Write(mTplAlphaDataSize);
                writer.Write(mJPEGTextureDataSize);
                writer.Write((int)mAlphaMode);
                writer.Write((int)mExactAlphaMode);
                writer.Write((int)mColorMode);
                writer.Write((int)mWiiTextureFormat);
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean);
                writer.Write(mDetailMapBrightness);
                writer.Write(mNormalMapFmt);
                mUVOffset.WriteBinaryData(writer);
                mUVScale.WriteBinaryData(writer);
            }

            if (
                game
                is TelltaleToolGame.NELSON_TETHERS_PUZZLE_AGENT
                    or TelltaleToolGame.CSI_FATAL_CONSPIRACY
                    or TelltaleToolGame.HECTOR_BADGE_OF_CARNAGE
                    or TelltaleToolGame.BACK_TO_THE_FUTURE_THE_GAME
                    or TelltaleToolGame.POKER_NIGHT_AT_THE_INVENTORY
            )
            {
                writer.Write(mSamplerState_BlockSize);
                writer.Write(mSamplerState.mData);
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mToolProps.mbHasProps);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mWiiForceWidth);
                writer.Write(mWiiForceHeight);
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean);
                writer.Write(mType);
                writer.Write(mTextureDataFormats);
                writer.Write(mTplTextureDataSize);
                writer.Write(mTplAlphaDataSize);
                writer.Write(mJPEGTextureDataSize);
                writer.Write((int)mAlphaMode);
                writer.Write((int)mWiiTextureFormat);
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean);
                writer.Write(mDetailMapBrightness);
                writer.Write(mNormalMapFmt);
            }

            if (
                game
                is TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_104
                    or TelltaleToolGame.TALES_OF_MONKEY_ISLAND_V1
                    or TelltaleToolGame.CSI_DEADLY_INTENT
                    or TelltaleToolGame.SAM_AND_MAX_THE_DEVILS_PLAYHOUSE_301
                    or TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_101_2007
            )
            {
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mWiiForceWidth);
                writer.Write(mWiiForceHeight);
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean);
                writer.Write(mType);
                writer.Write(mTextureDataFormats);
                writer.Write(mTplTextureDataSize);
                writer.Write(mTplAlphaDataSize);
                writer.Write(mJPEGTextureDataSize);
                writer.Write((int)mAlphaMode);
                writer.Write((int)mWiiTextureFormat);
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean);
                writer.Write(mDetailMapBrightness);
                writer.Write(mNormalMapFmt);
            }

            if (game is TelltaleToolGame.TALES_OF_MONKEY_ISLAND_V2)
            {
                ByteFunctions.WriteString(writer, mName);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mWiiForceWidth);
                writer.Write(mWiiForceHeight);
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean);
                writer.Write(mType);
                writer.Write(mTextureDataFormats);
                writer.Write(mTplTextureDataSize);
                writer.Write(mTplAlphaDataSize);
                writer.Write((int)mAlphaMode);
                writer.Write((int)mWiiTextureFormat);
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean);
                writer.Write(mDetailMapBrightness);
                writer.Write(mNormalMapFmt);
            }

            if (
                game
                is TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_101
                    or TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_102
                    or TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_103
            )
            {
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mWiiForceWidth);
                writer.Write(mWiiForceHeight);
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean);
                writer.Write(mType);
                writer.Write(mTextureDataFormats);
                writer.Write(mTplTextureDataSize);
                writer.Write(mTplAlphaDataSize);
                writer.Write(mJPEGTextureDataSize);
                writer.Write((int)mAlphaMode);
                writer.Write((int)mWiiTextureFormat);
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean);
                writer.Write(mDetailMapBrightness);
                writer.Write(mNormalMapFmt);
            }

            if (game is TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_105)
            {
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mWiiForceWidth);
                writer.Write(mWiiForceHeight);
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean);
                writer.Write(mType);
                writer.Write(mTextureDataFormats);
                writer.Write(mTplTextureDataSize);
                writer.Write(mJPEGTextureDataSize);
                writer.Write((int)mAlphaMode);
                writer.Write((int)mWiiTextureFormat);
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean);
                writer.Write(mDetailMapBrightness);
            }

            if (
                game
                is TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_103
                    or TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_104
            )
            {
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mWiiForceWidth);
                writer.Write(mWiiForceHeight);
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean);
                writer.Write(mType);
                writer.Write(mTextureDataFormats);
                writer.Write(mTplTextureDataSize);
                writer.Write(mJPEGTextureDataSize);
                writer.Write((int)mAlphaMode);
                writer.Write((int)mWiiTextureFormat);
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbUsedAsBumpmap.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbUsedAsDetailMap.mbTelltaleBoolean);
                writer.Write(mDetailMapBrightness);
            }

            if (
                game
                is TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_101
                    or TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_102
            )
            {
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mWiiForceWidth);
                writer.Write(mWiiForceHeight);
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean);
                writer.Write(mType);
                writer.Write(mTextureDataFormats);
                writer.Write(mTplTextureDataSize);
                writer.Write((int)mAlphaMode);
                writer.Write((int)mWiiTextureFormat);
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbUsedAsBumpmap.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbUsedAsDetailMap.mbTelltaleBoolean);
                writer.Write(mDetailMapBrightness);
            }

            if (
                game
                is TelltaleToolGame.TEXAS_HOLD_EM_V1
                    or TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_201_NEW
            )
            {
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mWiiForceWidth);
                writer.Write(mWiiForceHeight);
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean);
                writer.Write(mType);
                writer.Write(mTextureDataFormats);
                writer.Write((int)mAlphaMode);
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbUsedAsBumpmap.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbUsedAsDetailMap.mbTelltaleBoolean);
                writer.Write(mDetailMapBrightness);
            }

            if (
                game
                is TelltaleToolGame.CSI_HARD_EVIDENCE
                    or TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_201_OG
            )
            {
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mWiiForceWidth);
                writer.Write(mWiiForceHeight);
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean);
                writer.Write(mType);
                writer.Write(mTextureDataFormats);
                writer.Write(mTplTextureDataSize);
                writer.Write((int)mAlphaMode);
                writer.Write((int)mWiiTextureFormat);
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbUsedAsBumpmap.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbUsedAsDetailMap.mbTelltaleBoolean);
                writer.Write(mDetailMapBrightness);
            }

            if (
                game
                is TelltaleToolGame.BONE_OUT_FROM_BONEVILLE
                    or TelltaleToolGame.BONE_THE_GREAT_COW_RACE
            )
            {
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mType);
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbUsedAsBumpmap.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbUsedAsDetailMap.mbTelltaleBoolean);
                writer.Write(mDetailMapBrightness);
            }

            if (game is TelltaleToolGame.CSI_3_DIMENSIONS)
            {
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mType);
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean);
            }

            if (game is TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_101_2006)
            {
                writer.Write(mName_BlockSize);
                ByteFunctions.WriteString(writer, mName);
                writer.Write(mImportName_BlockSize);
                ByteFunctions.WriteString(writer, mImportName);
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean);
                writer.Write(mNumMipLevels);
                writer.Write((uint)mD3DFormat);
                writer.Write(mWidth);
                writer.Write(mHeight);
                writer.Write(mWiiForceWidth);
                writer.Write(mWiiForceHeight);
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean);
                writer.Write(mType);
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbUsedAsBumpmap.mbTelltaleBoolean);
                ByteFunctions.WriteBoolean(writer, mbUsedAsDetailMap.mbTelltaleBoolean);
            }

            if (mTextureDataFormats is 128 or 256 or 512)
            {
                writer.Write(mEmptyBuffer);
            }

            mPixelData.WriteBinaryData(writer);

            if (mTplTextureDataSize > 0)
            {
                writer.Write(mTplTextureDataSize);
            }

            if (mTplAlphaDataSize > 0)
            {
                writer.Write(mTplAlphaDataSize);
            }

            if (mJPEGTextureDataSize > 0)
            {
                writer.Write(mJPEGTextureData);
            }
        }

        public void ReadFromBinary(
            BinaryReader reader,
            TelltaleToolGame game = TelltaleToolGame.DEFAULT,
            T3PlatformType platform = T3PlatformType.ePlatform_None,
            bool printDebug = false
        )
        {
            if (game == TelltaleToolGame.DEFAULT || game == TelltaleToolGame.UNKNOWN)
            {
                throw new Exception();
            }

            bool read = true;
            bool isValid = true;

            // while (read && isValid)
            // {
            isValid = true;
            // I know there is a lot of repetition and ifs, but the way Telltale have updated their textures is unreliable and I would prefer to have an easier time reading the data.

            if (game is TelltaleToolGame.TEXAS_HOLD_EM_OG)
            {
                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
            }

            if (game is TelltaleToolGame.THE_WALKING_DEAD)
            {
                mSamplerState_BlockSize = reader.ReadInt32();
                mSamplerState = new T3SamplerStateBlock(reader);

                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);
                mToolProps = new ToolProps(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
                mFlags = reader.ReadUInt32();
                mWiiForceWidth = reader.ReadUInt32();
                mWiiForceHeight = reader.ReadUInt32();
                mbWiiForceUncompressed = new TelltaleBoolean(reader);
                mType = reader.ReadUInt32(); //???
                mTextureDataFormats = reader.ReadUInt32();
                mTplTextureDataSize = reader.ReadUInt32();
                mTplAlphaDataSize = reader.ReadUInt32();
                mJPEGTextureDataSize = reader.ReadUInt32();
                mHDRLightmapScale = reader.ReadSingle();
                mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();
                mExactAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();
                mColorMode = (T3TextureColor)reader.ReadInt32();
                mWiiTextureFormat = (WiiTextureFormat)reader.ReadInt32();
                mbEncrypted = new TelltaleBoolean(reader);
                mDetailMapBrightness = reader.ReadSingle();
                mNormalMapFmt = reader.ReadInt32();
                mUVOffset = new Vector2(reader); //mUVOffset [8 bytes]
                mUVScale = new Vector2(reader); //mUVScale [8 bytes]
            }

            if (
                game
                is TelltaleToolGame.PUZZLE_AGENT_2
                    or TelltaleToolGame.LAW_AND_ORDER_LEGACIES
                    or TelltaleToolGame.JURASSIC_PARK_THE_GAME
            )
            {
                mSamplerState_BlockSize = reader.ReadInt32();
                mSamplerState = new T3SamplerStateBlock(reader);

                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);

                mToolProps = new ToolProps(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
                mFlags = reader.ReadUInt32();
                mWiiForceWidth = reader.ReadUInt32();
                mWiiForceHeight = reader.ReadUInt32();
                mbWiiForceUncompressed = new TelltaleBoolean(reader);
                mType = reader.ReadUInt32(); //???
                mTextureDataFormats = reader.ReadUInt32();
                mTplTextureDataSize = reader.ReadUInt32();
                mTplAlphaDataSize = reader.ReadUInt32();
                mJPEGTextureDataSize = reader.ReadUInt32();
                mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();
                mExactAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();
                mColorMode = (T3TextureColor)reader.ReadUInt32();
                mWiiTextureFormat = (WiiTextureFormat)reader.ReadInt32();
                mbAlphaHDR = new TelltaleBoolean(reader);
                mbEncrypted = new TelltaleBoolean(reader);
                mDetailMapBrightness = reader.ReadSingle();
                mNormalMapFmt = reader.ReadInt32();
                mUVOffset = new Vector2(reader); //mUVOffset [8 bytes]
                mUVScale = new Vector2(reader); //mUVScale [8 bytes]
            }

            if (
                game
                is TelltaleToolGame.NELSON_TETHERS_PUZZLE_AGENT
                    or TelltaleToolGame.CSI_FATAL_CONSPIRACY
                    or TelltaleToolGame.HECTOR_BADGE_OF_CARNAGE
                    or TelltaleToolGame.BACK_TO_THE_FUTURE_THE_GAME
                    or TelltaleToolGame.POKER_NIGHT_AT_THE_INVENTORY
            )
            {
                mSamplerState_BlockSize = reader.ReadInt32();
                mSamplerState = new T3SamplerStateBlock(reader); //mSamplerState [4 bytes]

                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);

                mToolProps = new ToolProps(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mbEmbedMipMaps = new TelltaleBoolean(reader);

                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
                mWiiForceWidth = reader.ReadUInt32();
                mWiiForceHeight = reader.ReadUInt32();
                mbWiiForceUncompressed = new TelltaleBoolean(reader);
                mType = reader.ReadUInt32(); //???
                mTextureDataFormats = reader.ReadUInt32();
                mTplTextureDataSize = reader.ReadUInt32();
                mTplAlphaDataSize = reader.ReadUInt32();
                mJPEGTextureDataSize = reader.ReadUInt32();
                mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();
                mWiiTextureFormat = (WiiTextureFormat)reader.ReadInt32();
                mbAlphaHDR = new TelltaleBoolean(reader);
                mbEncrypted = new TelltaleBoolean(reader);
                mDetailMapBrightness = reader.ReadSingle();
                mNormalMapFmt = reader.ReadInt32();
            }

            if (
                game
                is TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_104
                    or TelltaleToolGame.TALES_OF_MONKEY_ISLAND_V1
                    or TelltaleToolGame.CSI_DEADLY_INTENT
                    or TelltaleToolGame.SAM_AND_MAX_THE_DEVILS_PLAYHOUSE_301
                    or TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_101_2007
            )
            {
                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mbIsWrapU = new TelltaleBoolean(reader);
                mbIsWrapV = new TelltaleBoolean(reader);
                mbIsFiltered = new TelltaleBoolean(reader);
                mbEmbedMipMaps = new TelltaleBoolean(reader);

                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
                mWiiForceWidth = reader.ReadUInt32();
                mWiiForceHeight = reader.ReadUInt32();
                mbWiiForceUncompressed = new TelltaleBoolean(reader);
                mType = reader.ReadUInt32(); //???
                mTextureDataFormats = reader.ReadUInt32();
                mTplTextureDataSize = reader.ReadUInt32();
                mTplAlphaDataSize = reader.ReadUInt32();
                mJPEGTextureDataSize = reader.ReadUInt32();
                mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();
                mWiiTextureFormat = (WiiTextureFormat)reader.ReadInt32();
                mbAlphaHDR = new TelltaleBoolean(reader);
                mbEncrypted = new TelltaleBoolean(reader);
                mDetailMapBrightness = reader.ReadSingle();
                mNormalMapFmt = reader.ReadInt32();
            }

            if (game is TelltaleToolGame.TALES_OF_MONKEY_ISLAND_V2)
            {
                mName = ByteFunctions.ReadString(reader);
                mImportName = ByteFunctions.ReadString(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mbIsWrapU = new TelltaleBoolean(reader);
                mbIsWrapV = new TelltaleBoolean(reader);
                mbIsFiltered = new TelltaleBoolean(reader);
                mbEmbedMipMaps = new TelltaleBoolean(reader);

                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
                mWiiForceWidth = reader.ReadUInt32();
                mWiiForceHeight = reader.ReadUInt32();
                mbWiiForceUncompressed = new TelltaleBoolean(reader);
                mType = reader.ReadUInt32(); //???
                mTextureDataFormats = reader.ReadUInt32();
                mTplTextureDataSize = reader.ReadUInt32();
                mTplAlphaDataSize = reader.ReadUInt32();
                mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();
                mWiiTextureFormat = (WiiTextureFormat)reader.ReadInt32();
                mbAlphaHDR = new TelltaleBoolean(reader);
                mbEncrypted = new TelltaleBoolean(reader);
                mDetailMapBrightness = reader.ReadSingle();
                mNormalMapFmt = reader.ReadInt32();
            }

            if (
                game
                is TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_101
                    or TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_102
                    or TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_103
            )
            {
                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mbIsWrapU = new TelltaleBoolean(reader);
                mbIsWrapV = new TelltaleBoolean(reader);
                mbIsFiltered = new TelltaleBoolean(reader);
                mbEmbedMipMaps = new TelltaleBoolean(reader);

                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
                mWiiForceWidth = reader.ReadUInt32();
                mWiiForceHeight = reader.ReadUInt32();
                mbWiiForceUncompressed = new TelltaleBoolean(reader);
                mType = reader.ReadUInt32(); //???
                mTextureDataFormats = reader.ReadUInt32();
                mTplTextureDataSize = reader.ReadUInt32();
                mJPEGTextureDataSize = reader.ReadUInt32();
                mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();

                mWiiTextureFormat = (WiiTextureFormat)reader.ReadInt32();
                mbAlphaHDR = new TelltaleBoolean(reader);
                mbEncrypted = new TelltaleBoolean(reader);
                mDetailMapBrightness = reader.ReadSingle();
                mNormalMapFmt = reader.ReadInt32();
            }

            if (game is TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_105)
            {
                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mbIsWrapU = new TelltaleBoolean(reader);
                mbIsWrapV = new TelltaleBoolean(reader);
                mbIsFiltered = new TelltaleBoolean(reader);
                mbEmbedMipMaps = new TelltaleBoolean(reader);

                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
                mWiiForceWidth = reader.ReadUInt32();
                mWiiForceHeight = reader.ReadUInt32();
                mbWiiForceUncompressed = new TelltaleBoolean(reader);
                mType = reader.ReadUInt32(); //???
                mTextureDataFormats = reader.ReadUInt32();
                mTplTextureDataSize = reader.ReadUInt32();
                mJPEGTextureDataSize = reader.ReadUInt32();
                mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();

                mWiiTextureFormat = (WiiTextureFormat)reader.ReadInt32();
                mbAlphaHDR = new TelltaleBoolean(reader);
                mbEncrypted = new TelltaleBoolean(reader);
                mDetailMapBrightness = reader.ReadSingle();
            }

            if (
                game
                is TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_103
                    or TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_104
            )
            {
                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mbIsWrapU = new TelltaleBoolean(reader);
                mbIsWrapV = new TelltaleBoolean(reader);
                mbIsFiltered = new TelltaleBoolean(reader);
                mbEmbedMipMaps = new TelltaleBoolean(reader);
                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
                mWiiForceWidth = reader.ReadUInt32();
                mWiiForceHeight = reader.ReadUInt32();
                mbWiiForceUncompressed = new TelltaleBoolean(reader);
                mType = reader.ReadUInt32(); //???
                mTextureDataFormats = reader.ReadUInt32();
                mTplTextureDataSize = reader.ReadUInt32();
                mJPEGTextureDataSize = reader.ReadUInt32();
                mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();

                mWiiTextureFormat = (WiiTextureFormat)reader.ReadInt32();
                mbAlphaHDR = new TelltaleBoolean(reader);
                mbEncrypted = new TelltaleBoolean(reader);
                mbUsedAsBumpmap = new TelltaleBoolean(reader);
                mbUsedAsDetailMap = new TelltaleBoolean(reader);
                mDetailMapBrightness = reader.ReadSingle();
            }

            if (
                game
                is TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_102
                    or TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_101
            )
            {
                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mbIsWrapU = new TelltaleBoolean(reader);
                mbIsWrapV = new TelltaleBoolean(reader);
                mbIsFiltered = new TelltaleBoolean(reader);
                mbEmbedMipMaps = new TelltaleBoolean(reader);

                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
                mWiiForceWidth = reader.ReadUInt32();
                mWiiForceHeight = reader.ReadUInt32();
                mbWiiForceUncompressed = new TelltaleBoolean(reader);
                mType = reader.ReadUInt32();
                mTextureDataFormats = reader.ReadUInt32();
                mTplTextureDataSize = reader.ReadUInt32();
                mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();
                mWiiTextureFormat = (WiiTextureFormat)reader.ReadInt32();
                mbAlphaHDR = new TelltaleBoolean(reader);
                mbUsedAsBumpmap = new TelltaleBoolean(reader);
                mbUsedAsDetailMap = new TelltaleBoolean(reader);
                mbEncrypted = new TelltaleBoolean(reader);
                mDetailMapBrightness = reader.ReadSingle();
            }

            if (
                game
                is TelltaleToolGame.TEXAS_HOLD_EM_V1
                    or TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_201_NEW
            )
            {
                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mbIsWrapU = new TelltaleBoolean(reader);
                mbIsWrapV = new TelltaleBoolean(reader);
                mbIsFiltered = new TelltaleBoolean(reader);
                mbEmbedMipMaps = new TelltaleBoolean(reader);
                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
                mWiiForceWidth = reader.ReadUInt32();
                mWiiForceHeight = reader.ReadUInt32();
                mbWiiForceUncompressed = new TelltaleBoolean(reader);
                mType = reader.ReadUInt32();
                mTextureDataFormats = reader.ReadUInt32();
                mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();
                mWiiTextureFormat = (WiiTextureFormat)reader.ReadInt32();
                mbAlphaHDR = new TelltaleBoolean(reader);
                mbEncrypted = new TelltaleBoolean(reader);
                mbUsedAsBumpmap = new TelltaleBoolean(reader);
                mbUsedAsDetailMap = new TelltaleBoolean(reader);
                mDetailMapBrightness = reader.ReadSingle();
            }

            if (game is TelltaleToolGame.CSI_HARD_EVIDENCE)
            {
                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mbIsWrapU = new TelltaleBoolean(reader);
                mbIsWrapV = new TelltaleBoolean(reader);
                mbIsFiltered = new TelltaleBoolean(reader);
                mbEmbedMipMaps = new TelltaleBoolean(reader);
                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
                mType = reader.ReadUInt32();
                mTextureDataFormats = reader.ReadUInt32();
                mTplTextureDataSize = reader.ReadUInt32();
                mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();
                mbAlphaHDR = new TelltaleBoolean(reader);
                mbEncrypted = new TelltaleBoolean(reader);
                mbUsedAsBumpmap = new TelltaleBoolean(reader);
                mbUsedAsDetailMap = new TelltaleBoolean(reader);
                mDetailMapBrightness = reader.ReadSingle();
            }

            if (game is TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_201_OG)
            {
                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mbIsWrapU = new TelltaleBoolean(reader);
                mbIsWrapV = new TelltaleBoolean(reader);
                mbIsFiltered = new TelltaleBoolean(reader);
                mbEmbedMipMaps = new TelltaleBoolean(reader);
                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();

                mWiiForceWidth = reader.ReadUInt32();
                mWiiForceHeight = reader.ReadUInt32();
                mbWiiForceUncompressed = new TelltaleBoolean(reader);
                mTplTextureDataSize = reader.ReadUInt32();
                mTextureDataFormats = reader.ReadUInt32();

                mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();
                mbAlphaHDR = new TelltaleBoolean(reader);
                mbEncrypted = new TelltaleBoolean(reader);
                mbUsedAsBumpmap = new TelltaleBoolean(reader);
                mbUsedAsDetailMap = new TelltaleBoolean(reader);
                mDetailMapBrightness = reader.ReadSingle();
            }

            if (
                game
                is TelltaleToolGame.BONE_OUT_FROM_BONEVILLE
                    or TelltaleToolGame.BONE_THE_GREAT_COW_RACE
            )
            {
                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mbIsWrapU = new TelltaleBoolean(reader);
                mbIsWrapV = new TelltaleBoolean(reader);
                mbIsFiltered = new TelltaleBoolean(reader);
                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
                mType = reader.ReadUInt32();

                mbAlphaHDR = new TelltaleBoolean(reader);
                mbEncrypted = new TelltaleBoolean(reader);
                mbUsedAsBumpmap = new TelltaleBoolean(reader);
                mbUsedAsDetailMap = new TelltaleBoolean(reader);

                mDetailMapBrightness = reader.ReadSingle();
            }

            if (game is TelltaleToolGame.CSI_3_DIMENSIONS)
            {
                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mbIsWrapU = new TelltaleBoolean(reader);
                mbIsWrapV = new TelltaleBoolean(reader);
                mbIsFiltered = new TelltaleBoolean(reader);
                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();
                mType = reader.ReadUInt32();

                mbEncrypted = new TelltaleBoolean(reader);
            }

            if (game is TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_101_2006)
            {
                mName_BlockSize = reader.ReadInt32();
                mName = ByteFunctions.ReadString(reader);
                mImportName_BlockSize = reader.ReadInt32();
                mImportName = ByteFunctions.ReadString(reader);
                mbHasTextureData = new TelltaleBoolean(reader);
                mbIsMipMapped = new TelltaleBoolean(reader);
                mbIsWrapU = new TelltaleBoolean(reader);
                mbIsWrapV = new TelltaleBoolean(reader);
                mbIsFiltered = new TelltaleBoolean(reader);
                mbEmbedMipMaps = new TelltaleBoolean(reader);
                mNumMipLevels = reader.ReadUInt32();
                mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                mWidth = reader.ReadUInt32();
                mHeight = reader.ReadUInt32();

                mWiiForceHeight = reader.ReadUInt32();
                mWiiForceWidth = reader.ReadUInt32();
                mbWiiForceUncompressed = new TelltaleBoolean(reader);

                mType = reader.ReadUInt32();

                mbAlphaHDR = new TelltaleBoolean(reader);
                mbEncrypted = new TelltaleBoolean(reader);
                mbUsedAsBumpmap = new TelltaleBoolean(reader);
                mbUsedAsDetailMap = new TelltaleBoolean(reader);
            }

            if (platform is not T3PlatformType.ePlatform_None)
            {
                mEmptyBuffer = reader.ReadInt32(); //mEmptyBuffer [4 bytes]
                if (mEmptyBuffer != 0)
                {
                    throw new Exception("The empty buffer is not 0!");
                }
            }

            if (mTextureDataFormats is 128 or 256 or 512 or 258)
            {
                mEmptyBuffer = reader.ReadInt32(); //mEmptyBuffer [4 bytes]
            }

            // uint mTextureDataSize = reader.ReadUInt32();

            // if (mTextureDataSize == 0 && mbHasTextureData.mbTelltaleBoolean)
            // {
            //     //  continue;
            // }

            // reader.BaseStream.Position -= 4;

            // int magic = reader.ReadInt32();
            // if (magic == 8 || magic == mName.Length + 8)
            // {
            //     reader.BaseStream.Position -= 4;
            //     //  break;
            // }

            // reader.BaseStream.Position -= 4;

            /// DDS
            ///

            mPixelData = new TelltalePixelData(reader);

            if (mbEncrypted.mbTelltaleBoolean)
            {
                byte[] encryptedBytes = [.. mPixelData.pixelData.Take(2048)];

                BlowFish decHeader = new(TelltaleToolGameExtensions.GetBlowfishKey(game), 1);
                byte[] decryptedBytes = decHeader.Crypt_ECB(encryptedBytes, 1, true);

                if (!D3DTX_Master.HasDDSHeader(decryptedBytes))
                {
                    throw new Exception("The texture is encrypted but the decryption failed!");
                }

                encryptedBytes = decryptedBytes;

                Array.Copy(encryptedBytes, 0, mPixelData.pixelData, 0, encryptedBytes.Length);
            }

            /// DDS

            if (mTplTextureDataSize > 0)
            {
                mTplData = new byte[mTplTextureDataSize];

                for (int i = 0; i < mTplTextureDataSize; i++)
                {
                    mTplData[i] = reader.ReadByte();
                }
            }

            if (mTplAlphaDataSize > 0)
            {
                mTplAlphaData = new byte[mTplAlphaDataSize];

                for (int i = 0; i < mTplAlphaDataSize; i++)
                {
                    mTplAlphaData[i] = reader.ReadByte();
                }
            }

            if (mJPEGTextureDataSize > 0)
            {
                mJPEGTextureData = new byte[mJPEGTextureDataSize];

                for (int i = 0; i < mJPEGTextureDataSize; i++)
                {
                    mJPEGTextureData[i] = reader.ReadByte();
                }
            }

            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                PrintConsole();
                throw new Exception("We did not reach the end of the file!");
            }

            if (!mbHasTextureData.mbTelltaleBoolean)
            {
                return;
                // throw new PixelDataNotFoundException("The texture does not have any pixel data!");
            }

            if (printDebug)
                PrintConsole(game);
        }

        public void ModifyD3DTX(
            D3DTXMetadata metadata,
            ImageSection[] imageSections,
            bool printDebug = false
        )
        {
            mWidth = metadata.Width;
            mHeight = metadata.Height;
            mD3DFormat = metadata.D3DFormat;
            mNumMipLevels = metadata.MipLevels;
            mbHasTextureData = new TelltaleBoolean(true);
            mbIsMipMapped = new TelltaleBoolean(metadata.MipLevels > 1);
            mbEmbedMipMaps = new TelltaleBoolean(metadata.MipLevels > 1);

            var textureData = TextureManager.GetPixelDataArrayFromSections(imageSections);

            if (mTextureDataFormats >= 0x200)
            {
                // Attempt to write pixel data for PS3 and other console games
                mPixelData = new TelltalePixelData(textureData, 128, 128);
            }
            else
            {
                mPixelData = new TelltalePixelData()
                {
                    length = (uint)textureData.Length,
                    pixelData = textureData,
                };
            }

            PrintConsole();
        }

        public D3DTXMetadata GetD3DTXMetadata()
        {
            D3DTXMetadata metadata = new()
            {
                TextureName = mName,
                Width = mWidth,
                Height = mHeight,
                MipLevels = mNumMipLevels,
                Dimension = T3TextureLayout.Texture2D,
                AlphaMode = mAlphaMode,
                D3DFormat = mD3DFormat,
                SurfaceGamma = T3SurfaceGamma.Unknown,
            };

            return metadata;
        }

        public List<byte[]> GetPixelData()
        {
            return [mPixelData.pixelData];
        }

        public string GetDebugInfo(
            TelltaleToolGame game = TelltaleToolGame.DEFAULT,
            T3PlatformType platform = T3PlatformType.ePlatform_None
        )
        {
            if (game is TelltaleToolGame.DEFAULT)
            {
                return string.Empty;
            }

            StringBuilder d3dtxInfo = new();

            d3dtxInfo
                .AppendFormat("Game: {0}", Enum.GetName(typeof(TelltaleToolGame), game))
                .AppendLine();

            d3dtxInfo.AppendLine("||||||||||| D3DTX Legacy Version Header |||||||||||");

            d3dtxInfo.AppendFormat("SamplerState: {0}", mSamplerState).AppendLine();
            d3dtxInfo.AppendFormat("Name: {0}", mName).AppendLine();
            d3dtxInfo.AppendFormat("Import Name: {0}", mImportName).AppendLine();
            d3dtxInfo.AppendFormat("Has Texture Data: {0}", mbHasTextureData).AppendLine();
            d3dtxInfo.AppendFormat("Is Mip Mapped: {0}", mbIsMipMapped).AppendLine();
            d3dtxInfo.AppendFormat("Is Wrap U: {0}", mbIsWrapU).AppendLine();
            d3dtxInfo.AppendFormat("Is Wrap V: {0}", mbIsWrapV).AppendLine();
            d3dtxInfo.AppendFormat("Is Filtered: {0}", mbIsFiltered).AppendLine();
            d3dtxInfo.AppendFormat("Embed Mip Maps: {0}", mbEmbedMipMaps).AppendLine();
            d3dtxInfo.AppendFormat("Num Mip Levels: {0}", mNumMipLevels).AppendLine();
            d3dtxInfo
                .AppendFormat("Direct3D Format: {0} ({1})", mD3DFormat, (int)mD3DFormat)
                .AppendLine();
            d3dtxInfo.AppendFormat("Width: {0}", mWidth).AppendLine();
            d3dtxInfo.AppendFormat("Height: {0}", mHeight).AppendLine();
            d3dtxInfo.AppendFormat("Wii Force Width: {0}", mWiiForceWidth).AppendLine();
            d3dtxInfo.AppendFormat("Wii Force Height: {0}", mWiiForceHeight).AppendLine();
            d3dtxInfo
                .AppendFormat("Wii Force Uncompressed: {0}", mbWiiForceUncompressed)
                .AppendLine();
            d3dtxInfo.AppendFormat("Type: {0}", mType).AppendLine();
            d3dtxInfo.AppendFormat("Texture Data Formats: {0}", mTextureDataFormats).AppendLine();
            d3dtxInfo.AppendFormat("TPL Texture Data Size: {0}", mTplTextureDataSize).AppendLine();
            d3dtxInfo.AppendFormat("TPL Alpha Data Size: {0}", mTplAlphaDataSize).AppendLine();
            d3dtxInfo
                .AppendFormat("JPEG Texture Data Size: {0}", mJPEGTextureDataSize)
                .AppendLine();
            d3dtxInfo
                .AppendFormat("Alpha Mode: {0} ({1})", mAlphaMode, (int)mAlphaMode)
                .AppendLine();
            d3dtxInfo
                .AppendFormat(
                    "Wii Texture Format: {0} ({1})",
                    mWiiTextureFormat,
                    (int)mWiiTextureFormat
                )
                .AppendLine();
            d3dtxInfo.AppendFormat("Alpha HDR: {0}", mbAlphaHDR).AppendLine();
            d3dtxInfo.AppendFormat("Encrypted: {0}", mbEncrypted).AppendLine();
            d3dtxInfo.AppendFormat("Detail Map Brightness: {0}", mDetailMapBrightness).AppendLine();
            d3dtxInfo.AppendFormat("Normal Map Format: {0}", mNormalMapFmt).AppendLine();
            d3dtxInfo.AppendFormat("UV Offset: {0}", mUVOffset).AppendLine();
            d3dtxInfo.AppendFormat("UV Scale: {0}", mUVScale).AppendLine();
            d3dtxInfo.AppendFormat("Empty Buffer: {0}", mEmptyBuffer).AppendLine();

            d3dtxInfo.AppendLine("||||||||||| Pixel Data |||||||||||");
            d3dtxInfo.AppendFormat("Pixel Data Length: {0}", mPixelData.length).AppendLine();

            d3dtxInfo.AppendLine("||||||||||| TPL Texture Data |||||||||||");
            d3dtxInfo
                .AppendFormat("TPL Texture Data Length: {0}", mTplTextureDataSize)
                .AppendLine();

            d3dtxInfo.AppendLine("||||||||||| TPL Alpha Data |||||||||||");
            d3dtxInfo.AppendFormat("TPL Alpha Data Length: {0}", mTplAlphaDataSize).AppendLine();

            d3dtxInfo.AppendLine("||||||||||| JPEG Texture Data |||||||||||");
            d3dtxInfo
                .AppendFormat("JPEG Texture Data Length: {0}", mJPEGTextureDataSize)
                .AppendLine();

            return d3dtxInfo.ToString();
        }

        public uint GetHeaderByteSize()
        {
            return 0;
        }

        public void PrintConsole(
            TelltaleToolGame game = TelltaleToolGame.DEFAULT,
            T3PlatformType platform = T3PlatformType.ePlatform_None
        )
        {
            Console.WriteLine(GetDebugInfo(game, platform));
        }
    }
}
