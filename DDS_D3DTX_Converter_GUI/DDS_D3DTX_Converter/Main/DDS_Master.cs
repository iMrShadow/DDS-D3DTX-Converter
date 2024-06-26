﻿using System;
using System.Collections.Generic;
using System.IO;
using D3DTX_Converter.Utilities;
using D3DTX_Converter.DirectX;
using D3DTX_Converter.TelltaleEnums;
using System.Linq;
using D3DTX_Converter.TelltaleTypes;
using Hexa.NET.DirectXTex;
using D3DTX_Converter.DirectX.Enums;
using Silk.NET.DXGI;

namespace D3DTX_Converter.Main
{
    /// <summary>
    /// Main class for generating a DDS header.
    /// </summary>
    public class DDS_Master
    {
        public DDS dds;
        public List<byte[]> textureData;

        /// <summary>
        /// Create a DDS file from a D3DTX.
        /// </summary>
        /// <param name="d3dtx">The D3DTX data that will be used.</param>
        public DDS_Master(D3DTX_Master d3dtx)
        {
            // NOTES: Remember that mip tables are reversed
            // So in that vein cubemap textures are likely in order but reversed
            // Some normal maps specifically with type 4 (eTxNormalMap) channels are all reversed (ABGR instead of RGBA)

            // Initialize the DDS with a default header
            dds = new DDS
            {
                header = DDS_HEADER.GetPresetHeader()
            };

            // If the D3DTX is a legacy D3DTX and we don't know from which game it is, we can just get the DDS header the file. This is only for extraction purposes
            if (!d3dtx.ddsImage.IsNull)
            {
                //dds = d3dtx.genericDDS;
                return;
            }

            if (d3dtx.IsMbin())
            {
                InitializeDDSHeaderForMBIN(d3dtx);
                return;
            }

            // If the D3DTX is a legacy D3DTX (before mVersions exist), we can just get the DDS header from the file
            if (d3dtx.IsLegacyD3DTX())
            {
                var pixelData = d3dtx.GetLegacyPixelData();
                dds.header = DDS_HEADER.GetHeaderFromBytes(pixelData[0], true);
                return;
            }

            T3SurfaceFormat surfaceFormat = d3dtx.GetCompressionType();
            T3SurfaceGamma surfaceGamma = d3dtx.GetSurfaceGamma();

            // Initialize DDS flags
            dds.header.dwFlags |= d3dtx.GetMipMapCount() > 1 ? DDSD.MIPMAPCOUNT : 0x0;
            dds.header.dwFlags |= d3dtx.IsTextureCompressed() ? DDSD.LINEARSIZE : DDSD.PITCH;
            dds.header.dwFlags |= d3dtx.IsVolumeTexture() ? DDSD.DEPTH : 0x0;

            DXGIFormat dxgiFormat = DDS_HELPER.GetDXGIFromTelltaleSurfaceFormat(surfaceFormat);
            if (d3dtx.GetPlatformType() == PlatformType.ePlatform_iPhone)
            {
                dxgiFormat = DDS_HELPER.GetDXGIFormatWithSwappedChannels(dxgiFormat);
            }

            // Initialize DDS width, height, depth, pitch, mip level count
            dds.header.dwWidth = (uint)d3dtx.GetWidth();
            dds.header.dwHeight = (uint)d3dtx.GetHeight();
            dds.header.dwPitchOrLinearSize = DDS_DirectXTexNet.ComputePitch(dxgiFormat, dds.header.dwWidth, dds.header.dwHeight);
            dds.header.dwDepth = (uint)d3dtx.GetDepth();

            // If the texture has more than 1 mip level, set the mip level count
            if (d3dtx.GetMipMapCount() > 1)
                dds.header.dwMipMapCount = (uint)d3dtx.GetMipMapCount();
            else dds.header.dwMipMapCount = 0;

            if (surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGB || surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGBA || surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGB1A)
            {
                dds.header.dwMipMapCount = (uint)d3dtx.GetMipMapCount();
                dds.header.dwPitchOrLinearSize = 0;
            }


            if (d3dtx.IsFormatIncompatibleWithDDS(surfaceFormat))
            {
                // dds.header.dwPitchOrLinearSize = 0;
                dds.header.dwMipMapCount = 1;
            }

            // Set the DDS pixel format info
            dds.header.ddspf = GetPixelFormatHeaderFromT3Surface(surfaceFormat);

            // We will enable DX10 header if the format is not a legacy format. Telltale won't use old formats in DirectX 11 and above.
            if (dds.header.ddspf.dwFourCC == ByteFunctions.Convert_String_To_UInt32("DX10") || d3dtx.IsArrayTexture())
            {
                dds.header.ddspf.dwFourCC = ByteFunctions.Convert_String_To_UInt32("DX10");
                dds.dxt10Header = DDS_HEADER_DXT10.GetPresetDXT10Header();

                dds.dxt10Header.dxgiFormat = dxgiFormat;

                // 1D textures don't exist in Telltale games
                dds.dxt10Header.resourceDimension = d3dtx.IsVolumeTexture() ? D3D10_RESOURCE_DIMENSION.TEXTURE3D : D3D10_RESOURCE_DIMENSION.TEXTURE2D;

                dds.dxt10Header.arraySize = (uint)d3dtx.GetArraySize();

                if (d3dtx.IsCubeTexture())
                {
                    dds.dxt10Header.miscFlag |= DDS_RESOURCE.MISC_TEXTURECUBE;
                }
            }

            // Mandatory flag
            dds.header.dwCaps |= DDSCAPS.TEXTURE;

            // If the texture has mipmaps, enable the mipmap flags
            if (d3dtx.GetMipMapCount() > 1)
            {
                dds.header.dwCaps |= DDSCAPS.COMPLEX;
                dds.header.dwCaps |= DDSCAPS.MIPMAP;
            }

            // If the texture is a cube texture, enable the cube texture flags
            if (d3dtx.IsCubeTexture())
            {
                dds.header.dwCaps |= DDSCAPS.COMPLEX;

                dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP;
                dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP_POSITIVEX;
                dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP_NEGATIVEX;
                dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP_POSITIVEY;
                dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP_NEGATIVEY;
                dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP_POSITIVEZ;
                dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP_NEGATIVEZ;
            }
            // If the texture is a volume texture, enable the volume texture flags
            else if (d3dtx.IsVolumeTexture())
            {
                dds.header.dwCaps |= DDSCAPS.COMPLEX;

                dds.header.dwCaps2 |= DDSCAPS2.VOLUME;
            }

            // Extract pixel data using streamheaders to make my life easier
            RegionStreamHeader[] streamHeaders = d3dtx.GetRegionStreamHeaders();

            textureData = [];

            // Get all pixel data from the D3DTX
            var d3dtxTextureData = d3dtx.GetPixelData();

            if (d3dtx.IsVolumeTexture())
            {
                if (d3dtx.IsFormatIncompatibleWithDDS(surfaceFormat) || d3dtx.IsPlatformIncompatibleWithDDS(d3dtx.GetPlatformType()))
                {
                    dds.header.dwMipMapCount = 1;
                    textureData.Add(d3dtx.GetPixelDataByFirstMipmapIndex(d3dtx.GetCompressionType(), (int)d3dtx.GetWidth(), (int)d3dtx.GetHeight(), d3dtx.GetPlatformType()));
                }
                else
                {
                    int divideBy = 1;
                    for (int i = 0; i < dds.header.dwMipMapCount; i++)
                    {
                        textureData.Add(d3dtx.GetPixelDataByMipmapIndex(i, d3dtx.GetCompressionType(), (int)d3dtx.GetWidth() / divideBy, (int)d3dtx.GetHeight() / divideBy, d3dtx.GetPlatformType()));
                        divideBy *= 2;
                    }
                }
            }
            else
            {
                if (d3dtx.IsFormatIncompatibleWithDDS(surfaceFormat) || d3dtx.IsPlatformIncompatibleWithDDS(d3dtx.GetPlatformType()))
                {
                    dds.header.dwMipMapCount = 1;
                    textureData.Add(d3dtx.GetPixelDataByFirstMipmapIndex(d3dtx.GetCompressionType(), (int)d3dtx.GetWidth(), (int)d3dtx.GetHeight(), d3dtx.GetPlatformType()));
                }
                else
                {
                    int totalFaces = (int)(d3dtx.IsCubeTexture() ? d3dtx.GetArraySize() * 6 : d3dtx.GetArraySize());

                    // Get each face of the 2D texture
                    for (int i = 0; i < totalFaces; i++)
                    {
                        textureData.Add(d3dtx.GetPixelDataByFaceIndex(i, d3dtx.GetCompressionType(), (int)d3dtx.GetWidth(), (int)d3dtx.GetHeight(), d3dtx.GetPlatformType()));
                    }
                }



                // byte[] d3dtxTextureDataArray = textureData.SelectMany(b => b).ToArray();

                for (int i = 0; i < streamHeaders.Length; i++)
                {
                    // else if (surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGB)
                    // {
                    //     AtcDecoder atcDecoder = new AtcDecoder();
                    //     if (i == 0)
                    //     {
                    //         data = UTEX.readATC(data, 0, new byte[(int)dds.header.dwWidth * (int)dds.header.dwHeight * 4], (int)dds.header.dwWidth, (int)dds.header.dwHeight);
                    //         // data = atcDecoder.DecompressAtcRgb4(data, (int)dds.header.dwWidth, (int)dds.header.dwHeight);
                    //     }
                    //     else
                    //     {
                    //         // data = atcDecoder.DecompressAtcRgb4(data, (int)dds.header.dwWidth / (i * 2), (int)dds.header.dwHeight / (i * 2));
                    //     }

                    // }
                    // else if (surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGBA || surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGB1A)
                    // {
                    //     AtcDecoder atcDecoder = new AtcDecoder();
                    //     if (i == 0)
                    //     {
                    //         data = UTEX.readATA(data, 0, new byte[(int)dds.header.dwWidth * (int)dds.header.dwHeight * 4], (int)dds.header.dwWidth, (int)dds.header.dwHeight);
                    //         // data = atcDecoder.DecompressAtcRgba8(data, (int)dds.header.dwWidth, (int)dds.header.dwHeight);
                    //     }
                    //     else
                    //     {
                    //         // data = atcDecoder.DecompressAtcRgba8(data, (int)dds.header.dwWidth / (i * 2), (int)dds.header.dwHeight / (i * 2));
                    //     }
                    // }
                }
            }

            dds.Print();
        }

        public void InitializeDDSHeaderForMBIN(D3DTX_Master d3dtx)
        {
            ScratchImage image = DirectXTex.CreateScratchImage();
            Console.WriteLine("D3dtx width: " + d3dtx.GetWidth());
            Console.WriteLine("D3dtx height: " + d3dtx.GetHeight());
            Console.WriteLine("D3dtx mip map count: " + d3dtx.GetMipMapCount());
            Console.WriteLine("D3dtx d3d format: " + d3dtx.GetD3DFORMAT());

            TexMetadata metadata = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = (int)DDS_HELPER.GetDXGIFormatFromD3DFormat(d3dtx.GetD3DFORMAT()),
                Height = d3dtx.GetWidth(),
                Width = d3dtx.GetHeight(),
                MipLevels = d3dtx.GetMipMapCount(),
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            image.Initialize(metadata, CPFlags.None);
            Console.WriteLine("DDS Metadata:");

            Console.WriteLine("Width: " + image.GetMetadata().Width);
            Console.WriteLine("Width: " + image.GetMetadata().Width);
            Console.WriteLine("Height: " + image.GetMetadata().Height);
            Console.WriteLine("Depth: " + image.GetMetadata().Depth);
            Console.WriteLine("Array Size: " + image.GetMetadata().ArraySize);
            Console.WriteLine("Mip Levels: " + image.GetMetadata().MipLevels);


            dds.header = DDS_HEADER.GetHeaderFromBytes(DDS_DirectXTexNet.GetDDSByteArray(image, DDSFlags.ForceDx9Legacy), true);

            Console.WriteLine("DDS Header:");
            dds.header.Print();

            image.Release();
        }

        /// <summary>
        /// Get the DDS Pixelformat data from a Telltale surface format.
        /// </summary>
        /// <param name="surface">The Telltale surface format.</param>
        /// <returns>DDS Pixelformat object with the correct surface format settings.</returns>
        private DDSPixelFormat GetPixelFormatHeaderFromT3Surface(T3SurfaceFormat surface)
        {
            return surface switch
            {
                // Uncompressed formats
                T3SurfaceFormat.eSurface_ARGB8 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),// 'ARGB8'
                T3SurfaceFormat.eSurface_ARGB16 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 64, 0x00, 0x00, 0x00, 0x00),// 'ARGB16'
                T3SurfaceFormat.eSurface_RGB565 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 16, 0x00, 0x00, 0x00, 0x00),// 'RGB565'
                T3SurfaceFormat.eSurface_ARGB1555 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 16, 0x00, 0x00, 0x00, 0x00),// 'ARGB1555'
                T3SurfaceFormat.eSurface_ARGB4 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 16, 0x00, 0x00, 0x00, 0x00),// 'ARGB4'  //Due to a long-standing issue in DDS readers and writers, it's better to use DX10 header for this format
                T3SurfaceFormat.eSurface_ARGB2101010 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),// 'ARGB2101010'   
                T3SurfaceFormat.eSurface_R16 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 16, 0x00, 0x00, 0x00, 0x00),// 'R16' TODO, COULD BE L16
                T3SurfaceFormat.eSurface_RG16 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.ALPHAPIXELS, DDPF.RGB), 0, 32, 0x00, 0x00, 0x00, 0x00),// 'RG16'
                T3SurfaceFormat.eSurface_RGBA16 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 64, 0x00, 0x00, 0x00, 0x00),// 'RGBA16'
                T3SurfaceFormat.eSurface_RG8 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 16, 0x00, 0x00, 0x00, 0x00),// 'RG8'
                T3SurfaceFormat.eSurface_RGBA8 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),// 'RGBA8'
                T3SurfaceFormat.eSurface_R32 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),// 'R32'
                T3SurfaceFormat.eSurface_RG32 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 64, 0x00, 0x00, 0x00, 0x00),// 'RG32' 
                T3SurfaceFormat.eSurface_RGBA32 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 128, 0x00, 0x00, 0x00, 0x00),// 'RGBA32'
                T3SurfaceFormat.eSurface_R8 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 8, 0x00, 0x00, 0x00, 0x00),// 'R8'
                T3SurfaceFormat.eSurface_RGBA8S => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),// 'RGBA8S'
                T3SurfaceFormat.eSurface_A8 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.ALPHA), 0, 8, 0x00, 0x00, 0x00, 0xff),// 'A8'
                T3SurfaceFormat.eSurface_L8 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.LUMINANCE), 0, 8, 0xff, 0x00, 0x00, 0x00),// 'L8'
                T3SurfaceFormat.eSurface_AL8 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.LUMINANCE, DDPF.ALPHAPIXELS), 0, 16, 0x00ff, 0x00, 0x00, 0xff00),// 'AL8'
                T3SurfaceFormat.eSurface_L16 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.LUMINANCE), 0, 16, 0xffff, 0x00, 0x00, 0x00),// 'L16'
                T3SurfaceFormat.eSurface_RG16S => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),// 'RG16S'
                T3SurfaceFormat.eSurface_RGBA16S => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 64, 0x00, 0x00, 0x00, 0x00),// 'RGBA16S'
                T3SurfaceFormat.eSurface_R16UI => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 16, 0x00, 0x00, 0x00, 0x00),// 'R16UI'
                T3SurfaceFormat.eSurface_RG16UI => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),// 'RG16UI'
                T3SurfaceFormat.eSurface_RGBA1010102F => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),// 'RGBA1010102F'
                T3SurfaceFormat.eSurface_RGB111110F => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),// 'RGB111110F'
                T3SurfaceFormat.eSurface_R16F => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 16, 0x00, 0x00, 0x00, 0x00),// 'R16F'
                T3SurfaceFormat.eSurface_RG16F => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),// 'RG16F'
                T3SurfaceFormat.eSurface_RGBA16F => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 64, 0x00, 0x00, 0x00, 0x00),// 'RGBA16F'
                T3SurfaceFormat.eSurface_R32F => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),// 'R32F'
                T3SurfaceFormat.eSurface_RG32F => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 64, 0x00, 0x00, 0x00, 0x00),// 'RG32F'
                T3SurfaceFormat.eSurface_RGBA32F => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 128, 0x00, 0x00, 0x00, 0x00),// 'RGBA32F'
                T3SurfaceFormat.eSurface_RGB9E5F => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),// 'RGB9E5F'  

                // Compressed formats
                T3SurfaceFormat.eSurface_BC1 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 0x00, 0x00, 0x00, 0x00, 0x00),// 'DXT1'
                T3SurfaceFormat.eSurface_BC2 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 0x00, 0x00, 0x00, 0x00, 0x00),// 'DXT3'
                T3SurfaceFormat.eSurface_BC3 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 0x00, 0x00, 0x00, 0x00, 0x00),// 'DXT5'
                T3SurfaceFormat.eSurface_BC4 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 0x00, 0x00, 0x00, 0x00, 0x00),// 'DXT5'
                T3SurfaceFormat.eSurface_BC5 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 0x00, 0x00, 0x00, 0x00, 0x00),// 'DXN'
                T3SurfaceFormat.eSurface_BC6 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 0x00, 0x00, 0x00, 0x00, 0x00),// 'BC6H'
                T3SurfaceFormat.eSurface_BC7 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 0x00, 0x00, 0x00, 0x00, 0x00),// 'BC7U'

                // Unneeded depth conversions. These are probably inaccurate headers if they ever existed
                T3SurfaceFormat.eSurface_DepthPCF16 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), 80, 16, 0x00, 0x00, 0x00, 0x00),// 'DepthPCF16'
                T3SurfaceFormat.eSurface_DepthPCF24 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), 77, 24, 0x00, 0x00, 0x00, 0x00),// 'DepthPCF24'
                T3SurfaceFormat.eSurface_Depth16 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), 80, 16, 0x00, 0x00, 0x00, 0x00),// 'Depth16'
                T3SurfaceFormat.eSurface_Depth24 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), 77, 24, 0x00, 0x00, 0x00, 0x00),// 'Depth24'
                T3SurfaceFormat.eSurface_DepthStencil32 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), 71, 32, 0x00, 0x00, 0x00, 0x00),// 'DepthStencil32'
                T3SurfaceFormat.eSurface_Depth32F => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 0x00, 0x00, 0x00, 0x00, 0x00),// 'Depth32F'
                T3SurfaceFormat.eSurface_Depth32F_Stencil8 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),// 'Depth32F_Stencil8'
                T3SurfaceFormat.eSurface_Depth24F_Stencil8 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), 83, 32, 0x00, 0x00, 0x00, 0x00),// 'Depth24F_Stencil8'

                // ATC
                T3SurfaceFormat.eSurface_ATC_RGB => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("ATC "), 0x00, 0x00, 0x00, 0x00, 0x00),// 'ATC_RGB'

                // ATCI
                T3SurfaceFormat.eSurface_ATC_RGBA => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("ATCI"), 0x00, 0x00, 0x00, 0x00, 0x00),// 'ATC_RGBA'

                // ATCA
                T3SurfaceFormat.eSurface_ATC_RGB1A => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("ATCA"), 0x00, 0x00, 0x00, 0x00, 0x00),// 'ATC_RGB1A'

                T3SurfaceFormat.eSurface_PVRTC2 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),
                T3SurfaceFormat.eSurface_PVRTC4 => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),
                T3SurfaceFormat.eSurface_PVRTC2a => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),
                T3SurfaceFormat.eSurface_PVRTC4a => DDSPixelFormat.Of(32, SetDDPFFlags(DDPF.FOURCC), ByteFunctions.Convert_String_To_UInt32("DX10"), 32, 0x00, 0x00, 0x00, 0x00),

                // Default to DXT1 Compression
                _ => DDSPixelFormat.Of(32, 0x04, ByteFunctions.Convert_String_To_UInt32("DXT1"), 0x00, 0x00, 0x00, 0x00, 0x00),// 'DXT1'
            };
        }


        /// <summary>
        /// Set the DDS Pixel Format flags with a bitwise-OR operation.
        /// </summary>
        /// <param name="flags">The byte flags.</param>
        /// <returns>The final byte flag.</returns>
        private static uint SetDDPFFlags(params DDPF[] flags)
        {
            uint result = 0;

            foreach (DDPF flag in flags)
            {
                result |= (uint)flag;
            }

            return result;
        }

        /// <summary>
        /// Writes a D3DTX into a DDS file on the disk.
        /// </summary>
        /// <param name="d3dtx"></param>
        /// <param name="destinationPath"></param>
        public void WriteD3DTXAsDDS(D3DTX_Master d3dtx, string destinationDirectory)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string d3dtxFilePath = d3dtx.filePath;
            string fileName = Path.GetFileNameWithoutExtension(d3dtxFilePath);

            byte[] pixelData = GetData(d3dtx);
            string newDDSPath = destinationDirectory + Path.DirectorySeparatorChar + fileName +
                                            Main_Shared.ddsExtension;

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Time to get data: {0}", elapsedMs);
            File.WriteAllBytes(newDDSPath, pixelData);
        }

        /// <summary>
        /// Get the correct DDS file from Telltale texture.????
        /// </summary>
        /// <param name="d3dtx"></param>
        /// <returns></returns>
        public byte[]? GetData(D3DTX_Master d3dtx)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            // If the image is an unknown d3dtx version
            if (!d3dtx.ddsImage.IsNull)
            {
                return d3dtx.ddsData;
            }

            if (d3dtx.GetD3DTXObject() == null)
            {
                throw new InvalidDataException("There is no pixel data to be written.");
            }

            // Turn the dds header into bytes
            byte[] dds_header = ByteFunctions.Combine(ByteFunctions.GetBytes(DDS.MAGIC_WORD), DDS_HELPER.GetObjectBytes(dds.header));

            if (d3dtx.IsLegacyD3DTX())
            {
                if (d3dtx.IsMbin())
                {
                    var legacyPixelData = d3dtx.GetLegacyPixelData();

                    return ByteFunctions.Combine(dds_header, legacyPixelData[legacyPixelData.Count - 1]);
                }
                else
                {
                    var legacyPixelData = d3dtx.GetLegacyPixelData();

                    return legacyPixelData[legacyPixelData.Count - 1];
                }
            }

            if (dds.header.ddspf.dwFourCC == ByteFunctions.Convert_String_To_UInt32(DDS.DX10_FOURCC))
            {
                dds_header = ByteFunctions.Combine(dds_header, DDS_HELPER.GetObjectBytes(dds.dxt10Header));
            }

            return ByteFunctions.Combine(dds_header, textureData.SelectMany(b => b).ToArray());

            // int arraySize = 1; arraySize = (int)dds.dxt10Header.arraySize;
            // if (d3dtx.IsLegacyD3DTX())
            // {
            //     var legacyByteArray = d3dtx.GetLegacyPixelData()[0].ToArray();
            //     finalData = ByteFunctions.Combine(finalData, legacyByteArray);

            //     return finalData;
            // }

            // // If the texture is Volumemap, just use the texture data we already have. We stable sorted the data earlier.
            // if (d3dtx.IsVolumeTexture())
            // {
            //     byte[] volumeTextureData = textureData.SelectMany(b => b).ToArray();

            //     finalData = ByteFunctions.Combine(finalData, volumeTextureData);
            //     return finalData;
            // }

            // // If the texture is anything else, but a Volumemap, use this sorting algorithm
            // // It supports Cubemap array textures as well (Cubemap array textures are like 2D array textures * 6)

            // // This is the initial offset from the texture data. Currently it only skips through 1 region header at a time.
            // int sizeOffset = d3dtx.IsCubeTexture() ? 6 : 1;

            // // Loop through the regions array size by the offset. 
            // // Example 1: A normal 2D texture with 10 mips only has 10 regions. The array size is 1 and the offset is 1. It will loop 1 time.
            // // Example 2: A 2D array texture with 3 textures and 4 mips each will have 12 (3*4) regions. The array size is 3 and the offset is 1. It will loop 3 times.
            // // Example 3: A Cubemap texture with 7 mips will have 42 (6*7) regions. The array size is 1 and the offset is 6. It will loop 6 times.
            // // Example 4: A Cubemap array texture with 3 cubemaps and 3 mips will have 54 (3*6*3) regions. The array size is 3 and the offset is 6. It will loop 18 times.
            // for (int i = 0; i < arraySize * sizeOffset; i++)
            // {
            //     // The first index of the face.
            //     int faceIndex = i;
            //     List<byte[]> faceData = [];

            //     //Make sure we loop at least once
            //     nuint mipCount = dds.header.dwMipMapCount > 1 ? dds.header.dwMipMapCount : 1;

            //     // Each loop here collects only 1 face of the texture. Let's use the examples from earlier.
            //     // Example 1: The mips are 10 - the texture is 1 (1 face). We iterate 10 times while incrementing the index by 1.
            //     // Example 2: The mips are 4 - the textures are 3 (3 faces). We iterate 4 times while incrementing the index by 3.
            //     // Example 3: The mips are 7 - the textures are 6 (6 faces). We iterate 7 times while incrementing the index by 6.
            //     // Example 4: The mips are 3 - the textures are 6*3 (18 faces). We iterate 3 times while incrementing the index by 18.
            //     for (nuint j = 0; j < mipCount; j++)
            //     {
            //         faceData.Add(textureData[faceIndex]);

            //         faceIndex += arraySize * sizeOffset;
            //     }

            //     byte[] faceDataArray = faceData.SelectMany(b => b).ToArray();

            //     finalData = ByteFunctions.Combine(finalData, faceDataArray);
            // }

            // watch.Stop();
            // var elapsedMs = watch.ElapsedMilliseconds;
            // Console.WriteLine("Time to get data: {0}", elapsedMs);
            // return finalData;
        }
    }
}
