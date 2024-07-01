using System;
using System.Collections.Generic;
using System.IO;
using TelltaleTextureTool.Utilities;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.TelltaleEnums;
using System.Linq;
using TelltaleTextureTool.TelltaleTypes;
using Hexa.NET.DirectXTex;
using TelltaleTextureTool.DirectX.Enums;

namespace TelltaleTextureTool.Main
{
    /// <summary>
    /// Main class for generating a DDS header.
    /// </summary>
    public class KTX_Master
    {
        public DDS dds;
        public List<byte[]> textureData;

        /// <summary>
        /// Create a DDS file from a D3DTX.
        /// </summary>
        /// <param name="d3dtx">The D3DTX data that will be used.</param>
        // public KTX_Master(D3DTX_Master d3dtx)
        // {
        //     // NOTES: Remember that mip tables are reversed
        //     // So in that vein cubemap textures are likely in order but reversed
        //     // Some normal maps specifically with type 4 (eTxNormalMap) channels are all reversed (ABGR instead of RGBA)

        //     // Initialize the DDS with a default header
        //     dds = new DDS
        //     {
        //         header = DDS_HEADER.GetPresetHeader()
        //     };

        //     // If the D3DTX is a legacy D3DTX and we don't know from which game it is, we can just get the DDS header the file. This is only for extraction purposes
        //     if (!d3dtx.ddsImage.IsNull)
        //     {
        //         //dds = d3dtx.genericDDS;
        //         return;
        //     }

        //     if (d3dtx.IsMbin())
        //     {
        //         InitializeDDSHeaderForMBIN(d3dtx);
        //         return;
        //     }

        //     // If the D3DTX is a legacy D3DTX (before mVersions exist), we can just get the DDS header from the file
        //     if (d3dtx.IsLegacyD3DTX())
        //     {
        //         var pixelData = d3dtx.GetLegacyPixelData();
        //         dds.header = DDS_HEADER.GetHeaderFromBytes(pixelData[0], true);
        //         return;
        //     }

        //     T3SurfaceFormat surfaceFormat = d3dtx.GetCompressionType();
        //     T3SurfaceGamma surfaceGamma = d3dtx.GetSurfaceGamma();

        //     // Initialize DDS flags
        //     dds.header.dwFlags |= d3dtx.GetMipMapCount() > 1 ? DDSD.MIPMAPCOUNT : 0x0;
        //     dds.header.dwFlags |= d3dtx.IsTextureCompressed() ? DDSD.LINEARSIZE : DDSD.PITCH;
        //     dds.header.dwFlags |= d3dtx.IsVolumeTexture() ? DDSD.DEPTH : 0x0;

        //     // Initialize DDS width, height, depth, pitch, mip level count
        //     dds.header.dwWidth = (uint)d3dtx.GetWidth();
        //     dds.header.dwHeight = (uint)d3dtx.GetHeight();
        //     dds.header.dwPitchOrLinearSize = DDS_DirectXTexNet.ComputePitch(DDS_HELPER.GetDXGIFromTelltaleSurfaceFormat(surfaceFormat), dds.header.dwWidth, dds.header.dwHeight);
        //     dds.header.dwDepth = (uint)d3dtx.GetDepth();

        //     // If the texture has more than 1 mip level, set the mip level count
        //     if (d3dtx.GetMipMapCount() > 1)
        //         dds.header.dwMipMapCount = (uint)d3dtx.GetMipMapCount();
        //     else dds.header.dwMipMapCount = 0;

        //     if (surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGB || surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGBA || surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGB1A)
        //     {
        //         dds.header.dwMipMapCount = (uint)d3dtx.GetMipMapCount();
        //         dds.header.dwPitchOrLinearSize = 0;
        //     }

            
        //     // We will enable DX10 header if the format is not a legacy format. Telltale won't use old formats in DirectX 11 and above.
        //     if (dds.header.ddspf.dwFourCC == ByteFunctions.ConvertStringToUInt32("DX10") || d3dtx.IsArrayTexture())
        //     {
        //         dds.header.ddspf.dwFourCC = ByteFunctions.ConvertStringToUInt32("DX10");
        //         dds.dxt10Header = DDS_HEADER_DXT10.GetPresetDXT10Header();

        //         dds.dxt10Header.dxgiFormat = DDS_HELPER.GetDXGIFromTelltaleSurfaceFormat(surfaceFormat, surfaceGamma);

        //         // 1D textures don't exist in Telltale games
        //         dds.dxt10Header.resourceDimension = d3dtx.IsVolumeTexture() ? D3D10_RESOURCE_DIMENSION.TEXTURE3D : D3D10_RESOURCE_DIMENSION.TEXTURE2D;

        //         dds.dxt10Header.arraySize = (uint)d3dtx.GetArraySize();

        //         if (d3dtx.IsCubeTexture())
        //         {
        //             dds.dxt10Header.miscFlag |= DDS_RESOURCE.MISC_TEXTURECUBE;
        //         }
        //     }

        //     Console.WriteLine(dds.dxt10Header.dxgiFormat);

        //     // Mandatory flag
        //     dds.header.dwCaps |= DDSCAPS.TEXTURE;

        //     // If the texture has mipmaps, enable the mipmap flags
        //     if (d3dtx.GetMipMapCount() > 1)
        //     {
        //         dds.header.dwCaps |= DDSCAPS.COMPLEX;
        //         dds.header.dwCaps |= DDSCAPS.MIPMAP;
        //     }

        //     // If the texture is a cube texture, enable the cube texture flags
        //     if (d3dtx.IsCubeTexture())
        //     {
        //         dds.header.dwCaps |= DDSCAPS.COMPLEX;

        //         dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP;
        //         dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP_POSITIVEX;
        //         dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP_NEGATIVEX;
        //         dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP_POSITIVEY;
        //         dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP_NEGATIVEY;
        //         dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP_POSITIVEZ;
        //         dds.header.dwCaps2 |= DDSCAPS2.CUBEMAP_NEGATIVEZ;
        //     }
        //     // If the texture is a volume texture, enable the volume texture flags
        //     else if (d3dtx.IsVolumeTexture())
        //     {
        //         dds.header.dwCaps |= DDSCAPS.COMPLEX;

        //         dds.header.dwCaps2 |= DDSCAPS2.VOLUME;
        //     }

        //     // Extract pixel data using streamheaders to make my life easier
        //     RegionStreamHeader[] streamHeaders = d3dtx.GetRegionStreamHeaders();

        //     textureData = [];

        //     // Get all pixel data from the D3DTX
        //     var d3dtxTextureData = d3dtx.GetPixelData();

        //     if (d3dtx.IsVolumeTexture())
        //     {
        //         int divideBy = 1;
        //         for (int i = 0; i < dds.header.dwMipMapCount; i++)
        //         {
        //             textureData.Add(d3dtx.GetPixelDataByMipmapIndex(i, d3dtx.GetCompressionType(), (int)d3dtx.GetWidth() / divideBy, (int)d3dtx.GetHeight() / divideBy, d3dtx.GetPlatformType()));
        //             divideBy *= 2;
        //         }
        //     }
        //     else
        //     {
        //         int totalFaces = (int)(d3dtx.IsCubeTexture() ? d3dtx.GetArraySize() * 6 : d3dtx.GetArraySize());

        //         // Get each face of the 2D texture
        //         for (int i = 0; i < totalFaces; i++)
        //         {
        //             textureData.Add(d3dtx.GetPixelDataByFaceIndex(i, d3dtx.GetCompressionType(), (int)d3dtx.GetWidth(), (int)d3dtx.GetHeight(), d3dtx.GetPlatformType()));
        //         }

        //         byte[] d3dtxTextureDataArray = textureData.SelectMany(b => b).ToArray();

        //         for (int i = 0; i < streamHeaders.Length; i++)
        //         {
        //             // else if (surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGB)
        //             // {
        //             //     AtcDecoder atcDecoder = new AtcDecoder();
        //             //     if (i == 0)
        //             //     {
        //             //         data = UTEX.readATC(data, 0, new byte[(int)dds.header.dwWidth * (int)dds.header.dwHeight * 4], (int)dds.header.dwWidth, (int)dds.header.dwHeight);
        //             //         // data = atcDecoder.DecompressAtcRgb4(data, (int)dds.header.dwWidth, (int)dds.header.dwHeight);
        //             //     }
        //             //     else
        //             //     {
        //             //         // data = atcDecoder.DecompressAtcRgb4(data, (int)dds.header.dwWidth / (i * 2), (int)dds.header.dwHeight / (i * 2));
        //             //     }

        //             // }
        //             // else if (surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGBA || surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGB1A)
        //             // {
        //             //     AtcDecoder atcDecoder = new AtcDecoder();
        //             //     if (i == 0)
        //             //     {
        //             //         data = UTEX.readATA(data, 0, new byte[(int)dds.header.dwWidth * (int)dds.header.dwHeight * 4], (int)dds.header.dwWidth, (int)dds.header.dwHeight);
        //             //         // data = atcDecoder.DecompressAtcRgba8(data, (int)dds.header.dwWidth, (int)dds.header.dwHeight);
        //             //     }
        //             //     else
        //             //     {
        //             //         // data = atcDecoder.DecompressAtcRgba8(data, (int)dds.header.dwWidth / (i * 2), (int)dds.header.dwHeight / (i * 2));
        //             //     }
        //             // }
        //         }
        //     }
        // }

        // public void InitializeDDSHeaderForMBIN(D3DTX_Master d3dtx)
        // {
        //     ScratchImage image = DirectXTex.CreateScratchImage();
        //     Console.WriteLine("D3dtx width: " + d3dtx.GetWidth());
        //     Console.WriteLine("D3dtx height: " + d3dtx.GetHeight());
        //     Console.WriteLine("D3dtx mip map count: " + d3dtx.GetMipMapCount());
        //     Console.WriteLine("D3dtx d3d format: " + d3dtx.GetD3DFORMAT());

        //     TexMetadata metadata = new()
        //     {
        //         ArraySize = 1,
        //         Depth = 1,
        //         Dimension = TexDimension.Texture2D,
        //         Format = (int)DDS_HELPER.GetDXGIFormatFromD3DFormat(d3dtx.GetD3DFORMAT()),
        //         Height = d3dtx.GetWidth(),
        //         Width = d3dtx.GetHeight(),
        //         MipLevels = d3dtx.GetMipMapCount(),
        //         MiscFlags = 0,
        //         MiscFlags2 = 0,
        //     };

        //     image.Initialize(metadata, CPFlags.None);
        //     Console.WriteLine("DDS Metadata:");

        //     Console.WriteLine("Width: " + image.GetMetadata().Width);
        //     Console.WriteLine("Width: " + image.GetMetadata().Width);
        //     Console.WriteLine("Height: " + image.GetMetadata().Height);
        //     Console.WriteLine("Depth: " + image.GetMetadata().Depth);
        //     Console.WriteLine("Array Size: " + image.GetMetadata().ArraySize);
        //     Console.WriteLine("Mip Levels: " + image.GetMetadata().MipLevels);


        //     dds.header = DDS_HEADER.GetHeaderFromBytes(DDS_DirectXTexNet.GetDDSByteArray(image, DDSFlags.ForceDx9Legacy), true);

        //     Console.WriteLine("DDS Header:");
        //     dds.header.Print();

        //     image.Release();
        // }

        /// <summary>
        /// Writes a D3DTX into a DDS file on the disk.
        /// </summary>
        /// <param name="d3dtx"></param>
        /// <param name="destinationPath"></param>
        public void WriteD3DTXAsDDS(D3DTX_Master d3dtx, string destinationDirectory)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string d3dtxFilePath = d3dtx.FilePath;
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
            return [];
        }
    }
}
