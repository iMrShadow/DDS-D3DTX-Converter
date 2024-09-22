using System;
using System.Collections.Generic;
using System.IO;
using TelltaleTextureTool.TelltaleEnums;
using TelltaleTextureTool.TelltaleTypes;
using TelltaleTextureTool.Utilities;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.DirectX.Enums;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;
using System.Linq;
using ColorTextBlock.Avalonia;

/*
 * NOTE:
 * 
 * This version of D3DTX is COMPLETE. 
 * 
 * COMPLETE meaning that all of the data is known and getting identified.
 * Just like the versions before and after, this D3DTX version derives from version 9 and has been 'stripped' or adjusted to suit this version of D3DTX.
 * Also, Telltale uses Hungarian Notation for variable naming.
*/

/* - D3DTX Legacy Version 1 games
 * The Walking Dead Season 1 (TESTED)
*/

namespace TelltaleTextureTool.TelltaleD3DTX
{
    /// <summary>
    /// This is a custom class that matches what is serialized in a legacy D3DTX version supporting the listed titles. (COMPLETE)
    /// </summary>
    public class LegacyD3DTX : ID3DTX
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
        /// [1 byte] Indicates whether or not the texture contains mips.
        /// </summary>
        public TelltaleBoolean mbIsWrapU { get; set; }

        /// <summary>
        /// [1 byte] Indicates whether or not the texture contains mips.
        /// </summary>
        public TelltaleBoolean mbIsWrapV { get; set; }

        /// <summary>
        /// [1 byte] Indicates whether or not the texture contains mips.
        /// </summary>
        public TelltaleBoolean mbIsFiltered { get; set; }

        /// <summary>
        /// [1 byte] No idea.
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
        /// [4 bytes] The texture data size (tpl?). 
        /// </summary>
        public uint mTplTextureDataSize { get; set; }

        /// <summary>
        /// [4 bytes] The alpha size of the texture? No idea why this exists.
        /// </summary>
        public uint mTplAlphaDataSize { get; set; }

        /// <summary>
        /// [4 bytes] The JPEG texture data size? (There were some screenshots of the game in the ttarch archives)
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
        /// [1 byte] Whether or not the texture encrypted.
        /// </summary>
        public TelltaleBoolean mbEncrypted { get; set; }

        /// <summary>
        /// [1 byte] Whether or not the texture has alpha HDR?
        /// </summary>
        public TelltaleBoolean mbUsedAsBumpmap { get; set; }

        /// <summary>
        /// [1 byte] Whether or not the texture has alpha HDR?
        /// </summary>
        public TelltaleBoolean mbUsedAsDetailMap { get; set; }

        /// <summary>
        /// [4 bytes] Map brightness for the Detail map type.
        /// </summary>
        public float mDetailMapBrightness { get; set; }

        /// <summary>
        /// [4 bytes] Normal map related stuff. 
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
        /// [4 bytes] Legacy console editions. It should be always zero.
        /// </summary>
        public int mEmptyBuffer { get; set; }

        /// <summary>
        /// A byte array of the pixel regions in a texture.
        /// </summary>
        public List<TelltalePixelData> mPixelData { get; set; } = [];

        /// <summary>
        /// A byte array of the pixel regions in a texture.
        /// </summary>
        public byte[] mTplData { get; set; } = [];

        /// <summary>
        /// A byte array of the pixel regions in a texture.
        /// </summary>
        public byte[] mTplAlphaData { get; set; } = [];

        /// <summary>
        /// A byte array of the pixel regions in a texture.
        /// </summary>
        public byte[] mJPEGTextureData { get; set; } = [];

        public LegacyD3DTX() { }

        public void WriteToBinary(BinaryWriter writer, TelltaleToolGame game = TelltaleToolGame.DEFAULT, T3PlatformType platform = T3PlatformType.ePlatform_None, bool printDebug = false)
        {
            if (game == TelltaleToolGame.DEFAULT || game == TelltaleToolGame.UNKNOWN)
            {
                throw new Exception();
            }

            if (game == TelltaleToolGame.THE_WALKING_DEAD)
            {
                writer.Write(mSamplerState_BlockSize); // mSamplerState_BlockSize [4 bytes]
                mSamplerState.WriteBinaryData(writer); // mSamplerState [4 bytes]
                writer.Write(mName_BlockSize); // mName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mName); // mName [mName_StringLength bytes]
                writer.Write(mImportName_BlockSize); // mImportName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mImportName); // mImportName [x bytes] (this is always 0)
                ByteFunctions.WriteBoolean(writer, mToolProps.mbHasProps); // mToolProps mbHasProps [1 byte]
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean); // mbHasTextureData [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean); // mbIsMipMapped [1 byte]
                writer.Write(mNumMipLevels); // mNumMipLevels [4 bytes]
                writer.Write((uint)mD3DFormat); // mD3DFormat [4 bytes]
                writer.Write(mWidth); // mWidth [4 bytes]
                writer.Write(mHeight); // mHeight [4 bytes]
                writer.Write(mFlags); // mFlags [4 bytes]
                writer.Write(mWiiForceWidth); // mWiiForceWidth [4 bytes]
                writer.Write(mWiiForceHeight); // mWiiForceHeight [4 bytes]
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean); // mbWiiForceUncompressed [1 byte]
                writer.Write(mType); // mType [4 bytes]
                writer.Write(mTextureDataFormats); // mTextureDataFormats [4 bytes]
                writer.Write(mTplTextureDataSize); // mTplTextureDataSize [4 bytes]
                writer.Write(mTplAlphaDataSize); // mTplAlphaDataSize [4 bytes]
                writer.Write(mJPEGTextureDataSize); // mJPEGTextureDataSize [4 bytes]
                writer.Write(mHDRLightmapScale); // mHDRLightmapScale [4 bytes]
                writer.Write((int)mAlphaMode); // mAlphaMode [4 bytes]
                writer.Write((int)mExactAlphaMode); // mExactAlphaMode [4 bytes]
                writer.Write((int)mColorMode); // mColorMode [4 bytes]
                writer.Write((int)mWiiTextureFormat); // mWiiTextureFormat [4 bytes]
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean); // mbEncrypted [1 byte]
                writer.Write(mDetailMapBrightness); // mDetailMapBrightness [4 bytes]
                writer.Write(mNormalMapFmt); // mNormalMapFmt [4 bytes]
                mUVOffset.WriteBinaryData(writer); // mUVOffset [8 bytes]
                mUVScale.WriteBinaryData(writer); // mUVScale [8 bytes]
            }

            if (game == TelltaleToolGame.PUZZLE_AGENT_2 ||
            game == TelltaleToolGame.LAW_AND_ORDER_LEGACIES ||
            game == TelltaleToolGame.JURASSIC_PARK_THE_GAME)
            {
                writer.Write(mSamplerState_BlockSize); // mSamplerState_BlockSize [4 bytes]
                mSamplerState.WriteBinaryData(writer); // mSamplerState [4 bytes]
                writer.Write(mName_BlockSize); // mName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mName); // mName [mName_StringLength bytes]
                writer.Write(mImportName_BlockSize); // mImportName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mImportName); // mImportName [x bytes] (this is always 0)
                ByteFunctions.WriteBoolean(writer, mToolProps.mbHasProps); // mToolProps mbHasProps [1 byte]
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean); // mbHasTextureData [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean); // mbIsMipMapped [1 byte]
                writer.Write(mNumMipLevels); // mNumMipLevels [4 bytes]
                writer.Write((uint)mD3DFormat); // mD3DFormat [4 bytes]
                writer.Write(mWidth); // mWidth [4 bytes]
                writer.Write(mHeight); // mHeight [4 bytes]
                writer.Write(mFlags); // mFlags [4 bytes]
                writer.Write(mWiiForceWidth); // mWiiForceWidth [4 bytes]
                writer.Write(mWiiForceHeight); // mWiiForceHeight [4 bytes]
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean); // mbWiiForceUncompressed [1 byte]
                writer.Write(mType); // mType [4 bytes]
                writer.Write(mTextureDataFormats); // mTextureDataFormats [4 bytes]
                writer.Write(mTplTextureDataSize); // mTplTextureDataSize [4 bytes]
                writer.Write(mTplAlphaDataSize); // mTplAlphaDataSize [4 bytes]
                writer.Write(mJPEGTextureDataSize); // mJPEGTextureDataSize [4 bytes]
                writer.Write((int)mAlphaMode); // mAlphaMode [4 bytes]
                writer.Write((int)mExactAlphaMode); // mExactAlphaMode [4 bytes]
                writer.Write((int)mColorMode); // mColorMode [4 bytes]
                writer.Write((int)mWiiTextureFormat); // mWiiTextureFormat [4 bytes]
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean); // mbAlphaHDR [1 byte]
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean); // mbEncrypted [1 byte]
                writer.Write(mDetailMapBrightness); // mDetailMapBrightness [4 bytes]
                writer.Write(mNormalMapFmt); // mNormalMapFmt [4 bytes]
                mUVOffset.WriteBinaryData(writer); // mUVOffset [8 bytes]
                mUVScale.WriteBinaryData(writer); // mUVScale [8 bytes]
            }

            if (game == TelltaleToolGame.NELSON_TETHERS_PUZZLE_AGENT ||
            game == TelltaleToolGame.CSI_FATAL_CONSPIRACY ||
            game == TelltaleToolGame.HECTOR_BADGE_OF_CARNAGE ||
            game == TelltaleToolGame.BACK_TO_THE_FUTURE_THE_GAME ||
            game == TelltaleToolGame.POKER_NIGHT_AT_THE_INVENTORY)
            {
                writer.Write(mSamplerState_BlockSize); // mSamplerState_BlockSize [4 bytes]
                mSamplerState.WriteBinaryData(writer); // mSamplerState [4 bytes]
                writer.Write(mName_BlockSize); // mName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mName); // mName [mName_StringLength bytes]
                writer.Write(mImportName_BlockSize); // mImportName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mImportName); // mImportName [x bytes] (this is always 0)
                ByteFunctions.WriteBoolean(writer, mToolProps.mbHasProps); // mToolProps mbHasProps [1 byte]
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean); // mbHasTextureData [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean); // mbIsMipMapped [1 byte]
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean); // mbEmbedMipMaps [1 byte]
                writer.Write(mNumMipLevels); // mNumMipLevels [4 bytes]
                writer.Write((uint)mD3DFormat); // mD3DFormat [4 bytes]
                writer.Write(mWidth); // mWidth [4 bytes]
                writer.Write(mHeight); // mHeight [4 bytes]
                writer.Write(mWiiForceWidth); // mWiiForceWidth [4 bytes]
                writer.Write(mWiiForceHeight); // mWiiForceHeight [4 bytes]
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean); // mbWiiForceUncompressed [1 byte]
                writer.Write(mType); // mType [4 bytes]
                writer.Write(mTextureDataFormats); // mTextureDataFormats [4 bytes]
                writer.Write(mTplTextureDataSize); // mTplTextureDataSize [4 bytes]
                writer.Write(mTplAlphaDataSize); // mTplAlphaDataSize [4 bytes]
                writer.Write(mJPEGTextureDataSize); // mJPEGTextureDataSize [4 bytes]
                writer.Write((int)mAlphaMode); // mAlphaMode [4 bytes]
                writer.Write((int)mWiiTextureFormat); // mWiiTextureFormat [4 bytes]
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean); // mbAlphaHDR [1 byte]
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean); // mbEncrypted [1 byte]
                writer.Write(mDetailMapBrightness); // mDetailMapBrightness [4 bytes]
                writer.Write(mNormalMapFmt); // mNormalMapFmt [4 bytes]
            }

            if (game == TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_104 ||
            game == TelltaleToolGame.TALES_OF_MONKEY_ISLAND ||
            game == TelltaleToolGame.CSI_DEADLY_INTENT ||
            game == TelltaleToolGame.SAM_AND_MAX_THE_DEVILS_PLAYHOUSE ||
            game == TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2007)
            {
                writer.Write(mName_BlockSize); // mName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mName); // mName [mName_StringLength bytes]
                writer.Write(mImportName_BlockSize); // mImportName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mImportName); // mImportName [x bytes] (this is always 0)
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean); // mbHasTextureData [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean); // mbIsMipMapped [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean); // mbIsWrapU [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean); // mbIsWrapV [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean); // mbIsFiltered [1 byte]
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean); // mbEmbedMipMaps [1 byte]
                writer.Write(mNumMipLevels); // mNumMipLevels [4 bytes]
                writer.Write((uint)mD3DFormat); // mD3DFormat [4 bytes]
                writer.Write(mWidth); // mWidth [4 bytes]
                writer.Write(mHeight); // mHeight [4 bytes]
                writer.Write(mWiiForceWidth); // mWiiForceWidth [4 bytes]
                writer.Write(mWiiForceHeight); // mWiiForceHeight [4 bytes]
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean); // mbWiiForceUncompressed [1 byte]
                writer.Write(mType); // mType [4 bytes]
                writer.Write(mTextureDataFormats); // mTextureDataFormats [4 bytes]
                writer.Write(mTplTextureDataSize); // mTplTextureDataSize [4 bytes]
                writer.Write(mTplAlphaDataSize); // mTplAlphaDataSize [4 bytes]
                writer.Write(mJPEGTextureDataSize); // mJPEGTextureDataSize [4 bytes]
                writer.Write((int)mAlphaMode); // mAlphaMode [4 bytes]
                writer.Write((int)mWiiTextureFormat); // mWiiTextureFormat [4 bytes]
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean); // mbAlphaHDR [1 byte]
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean); // mbEncrypted [1 byte]
                writer.Write(mDetailMapBrightness); // mDetailMapBrightness [4 bytes]
                writer.Write(mNormalMapFmt); // mNormalMapFmt [4 bytes]
            }

            if (game == TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_101 ||
            game == TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_102 ||
            game == TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_103)
            {
                writer.Write(mName_BlockSize); // mName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mName); // mName [mName_StringLength bytes]
                writer.Write(mImportName_BlockSize); // mImportName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mImportName); // mImportName [x bytes] (this is always 0)
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean); // mbHasTextureData [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean); // mbIsMipMapped [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean); // mbIsWrapU [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean); // mbIsWrapV [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean); // mbIsFiltered [1 byte]
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean); // mbEmbedMipMaps [1 byte]
                writer.Write(mNumMipLevels); // mNumMipLevels [4 bytes]
                writer.Write((uint)mD3DFormat); // mD3DFormat [4 bytes]
                writer.Write(mWidth); // mWidth [4 bytes]
                writer.Write(mHeight); // mHeight [4 bytes]
                writer.Write(mWiiForceWidth); // mWiiForceWidth [4 bytes]
                writer.Write(mWiiForceHeight); // mWiiForceHeight [4 bytes]
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean); // mbWiiForceUncompressed [1 byte]
                writer.Write(mType); // mType [4 bytes]
                writer.Write(mTextureDataFormats); // mTextureDataFormats [4 bytes]
                writer.Write(mTplTextureDataSize); // mTplTextureDataSize [4 bytes]
                writer.Write(mTplAlphaDataSize); // mTplAlphaDataSize [4 bytes]
                writer.Write(mJPEGTextureDataSize); // mJPEGTextureDataSize [4 bytes]
                writer.Write((int)mAlphaMode); // mAlphaMode [4 bytes]
                writer.Write((int)mWiiTextureFormat); // mWiiTextureFormat [4 bytes]
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean); // mbAlphaHDR [1 byte]
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean); // mbEncrypted [1 byte]
                writer.Write(mDetailMapBrightness); // mDetailMapBrightness [4 bytes]
                writer.Write(mNormalMapFmt); // mNormalMapFmt [4 bytes]
            }

            if (game == TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_105)
            {
                writer.Write(mName_BlockSize); // mName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mName); // mName [mName_StringLength bytes]
                writer.Write(mImportName_BlockSize); // mImportName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mImportName); // mImportName [x bytes] (this is always 0)
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean); // mbHasTextureData [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean); // mbIsMipMapped [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean); // mbIsWrapU [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean); // mbIsWrapV [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean); // mbIsFiltered [1 byte]
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean); // mbEmbedMipMaps [1 byte]
                writer.Write(mNumMipLevels); // mNumMipLevels [4 bytes]
                writer.Write((uint)mD3DFormat); // mD3DFormat [4 bytes]
                writer.Write(mWidth); // mWidth [4 bytes]
                writer.Write(mHeight); // mHeight [4 bytes]
                writer.Write(mWiiForceWidth); // mWiiForceWidth [4 bytes]
                writer.Write(mWiiForceHeight); // mWiiForceHeight [4 bytes]
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean); // mbWiiForceUncompressed [1 byte]
                writer.Write(mType); // mType [4 bytes]
                writer.Write(mTextureDataFormats); // mTextureDataFormats [4 bytes]
                writer.Write(mTplTextureDataSize); // mTplTextureDataSize [4 bytes]
                writer.Write(mTplAlphaDataSize); // mTplAlphaDataSize [4 bytes]
                writer.Write(mJPEGTextureDataSize); // mJPEGTextureDataSize [4 bytes]
                writer.Write((int)mAlphaMode); // mAlphaMode [4 bytes]
                writer.Write((int)mWiiTextureFormat); // mWiiTextureFormat [4 bytes]
                ByteFunctions.WriteBoolean(writer, mbAlphaHDR.mbTelltaleBoolean); // mbAlphaHDR [1 byte]
                ByteFunctions.WriteBoolean(writer, mbEncrypted.mbTelltaleBoolean); // mbEncrypted [1 byte]
                writer.Write(mDetailMapBrightness); // mDetailMapBrightness [4 bytes]
                writer.Write(mNormalMapFmt); // mNormalMapFmt [4 bytes]
            }

            if (game == TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_104)
            {
                writer.Write(mName_BlockSize); // mName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mName); // mName [mName_StringLength bytes]
                writer.Write(mImportName_BlockSize); // mImportName_BlockSize [4 bytes]
                ByteFunctions.WriteString(writer, mImportName); // mImportName [x bytes] (this is always 0)
                ByteFunctions.WriteBoolean(writer, mbHasTextureData.mbTelltaleBoolean); // mbHasTextureData [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsMipMapped.mbTelltaleBoolean); // mbIsMipMapped [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsWrapU.mbTelltaleBoolean); // mbIsWrapU [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsWrapV.mbTelltaleBoolean); // mbIsWrapV [1 byte]
                ByteFunctions.WriteBoolean(writer, mbIsFiltered.mbTelltaleBoolean); // mbIsFiltered [1 byte]
                ByteFunctions.WriteBoolean(writer, mbEmbedMipMaps.mbTelltaleBoolean); // mbEmbedMipMaps [1 byte]
                writer.Write(mNumMipLevels); // mNumMipLevels [4 bytes]
                writer.Write((uint)mD3DFormat); // mD3DFormat [4 bytes]
                writer.Write(mWidth); // mWidth [4 bytes]
                writer.Write(mHeight); // mHeight [4 bytes]
                writer.Write(mWiiForceWidth); // mWiiForceWidth [4 bytes]
                writer.Write(mWiiForceHeight); // mWiiForceHeight [4 bytes]
                ByteFunctions.WriteBoolean(writer, mbWiiForceUncompressed.mbTelltaleBoolean); // mbWiiForceUncompressed [1 byte]
                writer.Write(mType); // mType [4 bytes]
                writer.Write(mTextureDataFormats); // mTextureDataFormats [4 bytes]
                writer.Write(mTplTextureDataSize); // mTplTextureDataSize [4 bytes]
                writer.Write(mTplAlphaDataSize); // mTplAlphaDataSize [4 bytes]
                writer.Write(mJPEGTextureDataSize); // mJPEGTextureDataSize [4 bytes]
                writer.Write((int)mAlphaMode); // mAlphaMode [4 bytes]
            }


            if (platform != T3PlatformType.ePlatform_None)
            {
                writer.Write(mEmptyBuffer); //mEmptyBuffer [4 bytes]
            }

            for (int i = 0; i < mPixelData.Count; i++) //DDS file including header [mTextureDataSize bytes]
            {
                mPixelData[i].WriteBinaryData(writer);
            }
        }

        public void ReadFromBinary(BinaryReader reader, TelltaleToolGame game = TelltaleToolGame.DEFAULT, T3PlatformType platform = T3PlatformType.ePlatform_None, bool printDebug = false)
        {
            if (game == TelltaleToolGame.DEFAULT || game == TelltaleToolGame.UNKNOWN)
            {
                throw new Exception();
            }

            bool read = true;
            bool isValid = true;

            while (read && isValid)
            {
                isValid = true;
                // I know there is a lot of repetition and ifs, but the way Telltale have updated their textures is unreliable and I would prefer to have an easier time reading the data.

                if (game == TelltaleToolGame.TEXAS_HOLD_EM_OG)
                {
                    mName_BlockSize = reader.ReadInt32();
                    mName = ByteFunctions.ReadString(reader);
                    mImportName_BlockSize = reader.ReadInt32();
                    mImportName = ByteFunctions.ReadString(reader);
                    mbHasTextureData = new TelltaleBoolean(reader);
                    mNumMipLevels = reader.ReadUInt32();
                    //  mNumMipLevels = 1; // The first version indicates that there are mips, but the actual texture doesn't. It applies to all textures.
                    mD3DFormat = (LegacyFormat)reader.ReadUInt32();
                    mWidth = reader.ReadUInt32();
                    mHeight = reader.ReadUInt32();
                }



                if (game == TelltaleToolGame.THE_WALKING_DEAD)
                {
                    mSamplerState_BlockSize = reader.ReadInt32();
                    mSamplerState = new T3SamplerStateBlock() //mSamplerState [4 bytes]
                    {
                        mData = reader.ReadUInt32()
                    };

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

                if (game == TelltaleToolGame.PUZZLE_AGENT_2 ||
                game == TelltaleToolGame.LAW_AND_ORDER_LEGACIES ||
                game == TelltaleToolGame.JURASSIC_PARK_THE_GAME)
                {
                    mSamplerState_BlockSize = reader.ReadInt32();
                    mSamplerState = new T3SamplerStateBlock() //mSamplerState [4 bytes]
                    {
                        mData = reader.ReadUInt32()
                    };

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

                if (game == TelltaleToolGame.NELSON_TETHERS_PUZZLE_AGENT ||
                game == TelltaleToolGame.CSI_FATAL_CONSPIRACY ||
                game == TelltaleToolGame.HECTOR_BADGE_OF_CARNAGE ||
                game == TelltaleToolGame.BACK_TO_THE_FUTURE_THE_GAME ||
                game == TelltaleToolGame.POKER_NIGHT_AT_THE_INVENTORY)
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

                if (game == TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_104 ||
                game == TelltaleToolGame.TALES_OF_MONKEY_ISLAND ||
                game == TelltaleToolGame.CSI_DEADLY_INTENT ||
                game == TelltaleToolGame.SAM_AND_MAX_THE_DEVILS_PLAYHOUSE ||
                game == TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2007)
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

                if (game == TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_101 ||
                game == TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_102 ||
                game == TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_103)
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

                if (game == TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_105)
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

                if (game == TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_103 || game == TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_104)
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

                if (game == TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_102 || game == TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_101)
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
                    mTextureDataFormats = reader.ReadUInt32();
                    mTplTextureDataSize = reader.ReadUInt32();
                    mJPEGTextureDataSize = reader.ReadUInt32();
                    mAlphaMode = (T3TextureAlphaMode)reader.ReadInt32();

                    mWiiTextureFormat = (WiiTextureFormat)reader.ReadInt32();
                    mbAlphaHDR = new TelltaleBoolean(reader);
                    mbUsedAsBumpmap = new TelltaleBoolean(reader);
                    mbUsedAsDetailMap = new TelltaleBoolean(reader);
                    mbEncrypted = new TelltaleBoolean(reader);
                    mDetailMapBrightness = reader.ReadSingle();
                }

                if (game == TelltaleToolGame.TEXAS_HOLD_EM_V1 || game == TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_NEW)
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

                    mWiiTextureFormat = (WiiTextureFormat)reader.ReadInt32();
                    mbAlphaHDR = new TelltaleBoolean(reader);
                    mbEncrypted = new TelltaleBoolean(reader);
                    mbUsedAsBumpmap = new TelltaleBoolean(reader);
                    mbUsedAsDetailMap = new TelltaleBoolean(reader);
                    mDetailMapBrightness = reader.ReadSingle();
                }

                if (game == TelltaleToolGame.CSI_HARD_EVIDENCE || game == TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_OG)
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

                if (game == TelltaleToolGame.BONE_OUT_FROM_BONEVILLE || game == TelltaleToolGame.BONE_THE_GREAT_COW_RACE)
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

                if (game == TelltaleToolGame.CSI_3_DIMENSIONS)
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

                if (game == TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2006)
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

                if (platform != T3PlatformType.ePlatform_None)
                {
                    mEmptyBuffer = reader.ReadInt32(); //mEmptyBuffer [4 bytes]
                }

                PrintConsole(game);

                if (!mbHasTextureData.mbTelltaleBoolean)
                {
                    PrintConsole();
                    throw new PixelDataNotFoundException("The texture does not have any pixel data!");
                }

                uint mTextureDataSize = reader.ReadUInt32();

                if (mTextureDataSize == 0 && mbHasTextureData.mbTelltaleBoolean)
                {
                    continue;
                }

                reader.BaseStream.Position -= 4;

                mPixelData = [];

                int magic = reader.ReadInt32();
                if (magic == 8 || magic == mName.Length + 8)
                {
                    reader.BaseStream.Position -= 4;
                    break;
                }

                reader.BaseStream.Position -= 4;

                mNumMipLevels = mbEncrypted.mbTelltaleBoolean ? 1 : mNumMipLevels;
                int skip = mbEncrypted.mbTelltaleBoolean ? 128 : 0;

                mPixelData.Add(new TelltalePixelData(reader, skip));

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

                read = false;
            }

            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                PrintConsole();
                throw new Exception("We did not reach the end of the file!");
            }

            if (printDebug)
                PrintConsole(game);
        }

        public void ModifyD3DTX(D3DTXMetadata metadata, ImageSection[] imageSections, bool printDebug = false)
        {
            mWidth = metadata.Width;
            mHeight = metadata.Height;
            mD3DFormat = metadata.D3DFormat;
            mNumMipLevels = metadata.MipLevels;
            mbHasTextureData = new TelltaleBoolean(true);
            mbIsMipMapped = new TelltaleBoolean(metadata.MipLevels > 1);

            var textureData = DDS_DirectXTexNet.GetPixelDataArrayFromSections(imageSections);

            mPixelData.Clear();

            TelltalePixelData telltalePixelData = new TelltalePixelData()
            {
                length = (uint)textureData.Length,
                pixelData = textureData
            };

            mPixelData.Add(telltalePixelData);

            PrintConsole();
        }

        public D3DTXMetadata GetD3DTXMetadata()
        {
            D3DTXMetadata metadata = new D3DTXMetadata()
            {
                TextureName = mName,
                Width = mWidth,
                Height = mHeight,
                MipLevels = mNumMipLevels,
                Dimension = T3TextureLayout.Texture2D,
                AlphaMode = mAlphaMode,
                D3DFormat = mD3DFormat,
            };

            return metadata;
        }

        public List<byte[]> GetPixelData()
        {
            return mPixelData.Select(x => x.pixelData).ToList();
        }

        public string GetDebugInfo(TelltaleToolGame game = TelltaleToolGame.DEFAULT, T3PlatformType platform = T3PlatformType.ePlatform_None)
        {
            if (game == TelltaleToolGame.DEFAULT)
            {
                return string.Empty;
            }

            bool after_CSI_FATAL_CONSPIRACY = game >= TelltaleToolGame.CSI_FATAL_CONSPIRACY;
            bool before_THE_WALKING_DEAD_GAME = game <= TelltaleToolGame.THE_WALKING_DEAD;
            bool between_CSI_FATAL_CONSPIRACY_AND_TWD = after_CSI_FATAL_CONSPIRACY && before_THE_WALKING_DEAD_GAME;

            string d3dtxInfo = "";

            d3dtxInfo += "|||||||||||" + Enum.GetName(typeof(TelltaleToolGame), (int)game) + "|||||||||||" + Environment.NewLine;
            d3dtxInfo += "||||||||||| D3DTX Legacy Version Header |||||||||||" + Environment.NewLine;
            if (between_CSI_FATAL_CONSPIRACY_AND_TWD && game != TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_NEW)
            {
                d3dtxInfo += "mSamplerState_BlockSize = " + mSamplerState_BlockSize + Environment.NewLine;
                d3dtxInfo += "mSamplerState = " + mSamplerState.ToString() + Environment.NewLine;
            }
            d3dtxInfo += "mName_BlockSize = " + mName_BlockSize + Environment.NewLine;
            d3dtxInfo += "mName = " + mName + Environment.NewLine;
            d3dtxInfo += "mImportName_BlockSize = " + mImportName_BlockSize + Environment.NewLine;
            d3dtxInfo += "mImportName = " + mImportName + Environment.NewLine;
            if (between_CSI_FATAL_CONSPIRACY_AND_TWD && game != TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_NEW)
            {
                d3dtxInfo += "mToolProps = " + mToolProps + Environment.NewLine;
            }
            d3dtxInfo += "mbHasTextureData = " + mbHasTextureData + Environment.NewLine;
            d3dtxInfo += "mbIsMipMapped = " + mbIsMipMapped + Environment.NewLine;
            if (!(between_CSI_FATAL_CONSPIRACY_AND_TWD && game != TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_NEW))
            {
                d3dtxInfo += "mbIsWrapU = " + mbIsWrapU + Environment.NewLine;
                d3dtxInfo += "mbIsWrapV = " + mbIsWrapV + Environment.NewLine;
                d3dtxInfo += "mbIsFiltered = " + mbIsFiltered + Environment.NewLine;
            }
            if ((game <= TelltaleToolGame.HECTOR_BADGE_OF_CARNAGE && game >= TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_NEW)
            && game != TelltaleToolGame.TEXAS_HOLD_EM_V1)
            {
                d3dtxInfo += "mbEmbedMipMaps = " + mbEmbedMipMaps + Environment.NewLine;
            }
            d3dtxInfo += "mNumMipLevels = " + mNumMipLevels + Environment.NewLine;
            d3dtxInfo += "mD3DFormat = " + mD3DFormat + Environment.NewLine;
            d3dtxInfo += "mWidth = " + mWidth + Environment.NewLine;
            d3dtxInfo += "mHeight = " + mHeight + Environment.NewLine;
            if (game >= TelltaleToolGame.HECTOR_BADGE_OF_CARNAGE)
            {
                d3dtxInfo += "mFlags = " + mFlags + Environment.NewLine;
            }
            if (game <= TelltaleToolGame.CSI_HARD_EVIDENCE && game != TelltaleToolGame.TEXAS_HOLD_EM_V1)
            {
                d3dtxInfo += "mWiiForceWidth = " + mWiiForceWidth + Environment.NewLine;
                d3dtxInfo += "mWiiForceHeight = " + mWiiForceHeight + Environment.NewLine;
                d3dtxInfo += "mbWiiForceUncompressed = " + mbWiiForceUncompressed + Environment.NewLine;
            }
            if (!(game == TelltaleToolGame.TEXAS_HOLD_EM_V1 ||

                game == TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_NEW ||
                game == TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_101 ||
                game == TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_102))
            {
                d3dtxInfo += "mType = " + mType + Environment.NewLine;
            }
            if (!(game == TelltaleToolGame.BONE_OUT_FROM_BONEVILLE ||

                game == TelltaleToolGame.CSI_3_DIMENSIONS ||
                game == TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2006 ||
                game == TelltaleToolGame.BONE_THE_GREAT_COW_RACE ||
                game == TelltaleToolGame.THE_WALKING_DEAD))
            {
                d3dtxInfo += "mTextureDataFormats = " + mTextureDataFormats + Environment.NewLine;
                d3dtxInfo += "mTplTextureDataSize = " + mTplTextureDataSize + Environment.NewLine;
                if (game >= TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_101)
                {
                    d3dtxInfo += "mTplAlphaDataSize = " + mTplAlphaDataSize + Environment.NewLine;
                }
            }
            if (!(game <= TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_NEW))
            {
                d3dtxInfo += "mJPEGTextureDataSize = " + mJPEGTextureDataSize + Environment.NewLine;
            }
            if (game == TelltaleToolGame.THE_WALKING_DEAD)
            {
                d3dtxInfo += "mHDRLightmapScale = " + mHDRLightmapScale + Environment.NewLine;
            }
            if (!(game == TelltaleToolGame.BONE_OUT_FROM_BONEVILLE ||

                game == TelltaleToolGame.CSI_3_DIMENSIONS ||
                game == TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2006 ||
                game == TelltaleToolGame.BONE_THE_GREAT_COW_RACE))
            {
                d3dtxInfo += "mAlphaMode = " + Enum.GetName(typeof(T3TextureAlphaMode), (int)mAlphaMode) + " (" + mAlphaMode + ")" + Environment.NewLine;
            }
            if (game >= TelltaleToolGame.HECTOR_BADGE_OF_CARNAGE)
            {
                d3dtxInfo += "mExactAlphaMode = " + Enum.GetName(typeof(T3TextureAlphaMode), (int)mExactAlphaMode) + " (" + mExactAlphaMode + ")" + Environment.NewLine;
                d3dtxInfo += "mColorMode = " + Enum.GetName(typeof(T3TextureColor), (int)mColorMode) + " (" + mColorMode + ")" + Environment.NewLine;
            }
            if (!(game == TelltaleToolGame.BONE_OUT_FROM_BONEVILLE ||

                game == TelltaleToolGame.CSI_3_DIMENSIONS ||
                game == TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2006 ||
                game == TelltaleToolGame.BONE_THE_GREAT_COW_RACE))
            {
                d3dtxInfo += "mWiiTextureFormat = " + mWiiTextureFormat + Environment.NewLine;
            }
            if (!(game == TelltaleToolGame.THE_WALKING_DEAD ||
            game == TelltaleToolGame.CSI_3_DIMENSIONS ||
            game == TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2006)
            )
            {
                d3dtxInfo += "mbAlphaHDR = " + mbAlphaHDR + Environment.NewLine;
            }
            d3dtxInfo += "mbEncrypted = " + mbEncrypted + Environment.NewLine;
            if (game >= TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_104 &&

                !(game == TelltaleToolGame.CSI_3_DIMENSIONS) &&
                !(game == TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2006))
            {
                d3dtxInfo += "mbUsedAsBumpmap = " + mbUsedAsBumpmap + Environment.NewLine;
                d3dtxInfo += "mbUsedAsDetailMap = " + mbUsedAsDetailMap + Environment.NewLine;
            }
            if (!(game == TelltaleToolGame.CSI_3_DIMENSIONS ||
                    game == TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2006))
            {
                d3dtxInfo += "mDetailMapBrightness = " + mDetailMapBrightness + Environment.NewLine;
            }
            if (game >= TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_101)
            {
                d3dtxInfo += "mNormalMapFmt = " + mNormalMapFmt + Environment.NewLine;
            }
            if (game >= TelltaleToolGame.HECTOR_BADGE_OF_CARNAGE)
            {
                d3dtxInfo += "mUVOffset = " + mUVOffset + Environment.NewLine;
                d3dtxInfo += "mUVScale = " + mUVScale + Environment.NewLine;
            }

            for (int i = 0; i < mPixelData.Count; i++)
            {
                d3dtxInfo += "mPixelData[" + i + "] = " + mPixelData[i].ToString() + Environment.NewLine;
            }

            d3dtxInfo += "|||||||||||||||||||||||||||||||||||||||";

            return d3dtxInfo;
        }

        public uint GetHeaderByteSize()
        {
            return 0;
        }

        public void PrintConsole(TelltaleToolGame game = TelltaleToolGame.DEFAULT, T3PlatformType platform = T3PlatformType.ePlatform_None)
        {
            Console.WriteLine(GetDebugInfo(game, platform));
        }
    }
}