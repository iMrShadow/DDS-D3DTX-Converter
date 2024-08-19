using System;
using System.Collections.Generic;
using System.IO;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.TelltaleEnums;

namespace TelltaleTextureTool.Main
{
    /// <summary>
    /// Main class for generating a DDS header.
    /// </summary>
    public class TextureMaster
    {
        public DDS_Master? ddsMaster;
        public KTX2_Master? ktx2Master;
        public KTX_Master? ktxMaster;

        public byte[] textureData;

        public TextureMaster(D3DTX_Master d3dtx)
        {
            //  this.WriteD3DTXAsDDS
        }

        /// <summary>
        /// Returns a byte array List containing the pixel data from a DDS_DirectXTexNet_ImageSection array.
        /// </summary>
        /// <param name="sections">The sections of the DDS image.</param>
        /// <returns></returns>
        public static List<byte[]> GetPixelDataFromSections(ImageSection[] sections)
        {
            List<byte[]> textureData = [];

            foreach (ImageSection imageSection in sections)
            {
                textureData.Add(imageSection.Pixels);
            }

            return textureData;
        }

        public void WriteD3DTXAsTexture(D3DTX_Master d3dtx, string destinationDirectory)
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

        public static bool IsValidContainerForSurfaceFormat(T3SurfaceFormat format, T3SurfaceGamma gamma)
        {
            return false;
        }

        public static bool IsValidFormatForDDS(T3SurfaceFormat format)
        {
            return format switch
            {
                T3SurfaceFormat.PVRTC2 or
                T3SurfaceFormat.PVRTC4 or
                T3SurfaceFormat.PVRTC2a or
                T3SurfaceFormat.PVRTC4a or
                T3SurfaceFormat.ETC1_RGB or
                T3SurfaceFormat.ETC2_RGB or
                T3SurfaceFormat.ETC2_RGB1A or
                T3SurfaceFormat.ETC2_RGBA or
                T3SurfaceFormat.ETC2_R or
                T3SurfaceFormat.ETC2_RG or
                T3SurfaceFormat.ATSC_RGBA_4x4 or
                T3SurfaceFormat.FrontBuffer => false,
                _ => true,
            };
        }

        public static bool IsValidFormatForKTX(T3SurfaceFormat format)
        {
            switch (format)
            {
                case T3SurfaceFormat.R16:
                case T3SurfaceFormat.RG16:
                case T3SurfaceFormat.RGBA16:
                case T3SurfaceFormat.RG8:
                case T3SurfaceFormat.RGBA8:
                case T3SurfaceFormat.R8:
                case T3SurfaceFormat.RGBA8S:
                case T3SurfaceFormat.A8:
                case T3SurfaceFormat.L8:
                case T3SurfaceFormat.AL8:
                case T3SurfaceFormat.L16:
                case T3SurfaceFormat.RGBA1010102F:
                case T3SurfaceFormat.RGB111110F:
                case T3SurfaceFormat.RGB9E5F:
                case T3SurfaceFormat.DepthPCF16:
                case T3SurfaceFormat.DepthPCF24:
                case T3SurfaceFormat.Depth16:
                case T3SurfaceFormat.Depth24:
                case T3SurfaceFormat.DepthStencil32:
                case T3SurfaceFormat.Depth32F:
                case T3SurfaceFormat.Depth32F_Stencil8:
                case T3SurfaceFormat.Depth24F_Stencil8:

                case T3SurfaceFormat.CTX1:

                case T3SurfaceFormat.PVRTC2:
                case T3SurfaceFormat.PVRTC4:
                case T3SurfaceFormat.PVRTC2a:
                case T3SurfaceFormat.PVRTC4a:
                case T3SurfaceFormat.ATC_RGB:
                case T3SurfaceFormat.ATC_RGB1A:
                case T3SurfaceFormat.ATC_RGBA:
                case T3SurfaceFormat.ETC1_RGB:
                case T3SurfaceFormat.ETC2_RGB:
                case T3SurfaceFormat.ETC2_RGB1A:
                case T3SurfaceFormat.ETC2_RGBA:
                case T3SurfaceFormat.ETC2_R:
                case T3SurfaceFormat.ETC2_RG:
                case T3SurfaceFormat.ATSC_RGBA_4x4:
                case T3SurfaceFormat.FrontBuffer:
                    return false;
                default:
                    return false;
            }


            return false;
        }


        public static bool IsValidFormatForKTX2(T3SurfaceFormat format)
        {
            switch (format)
            {
                case T3SurfaceFormat.ARGB8:
                case T3SurfaceFormat.ARGB16:
                case T3SurfaceFormat.RGB565:
                case T3SurfaceFormat.ARGB1555:
                case T3SurfaceFormat.ARGB4:
                case T3SurfaceFormat.ARGB2101010:
                case T3SurfaceFormat.R16:
                case T3SurfaceFormat.RG16:
                case T3SurfaceFormat.RGBA16:
                case T3SurfaceFormat.RG8:
                case T3SurfaceFormat.RGBA8:
                case T3SurfaceFormat.R32:
                case T3SurfaceFormat.RG32:
                case T3SurfaceFormat.RGBA32:
                case T3SurfaceFormat.R8:
                case T3SurfaceFormat.RGBA8S:
                case T3SurfaceFormat.A8:
                case T3SurfaceFormat.L8:
                case T3SurfaceFormat.AL8:
                case T3SurfaceFormat.L16:
                case T3SurfaceFormat.RG16S:
                case T3SurfaceFormat.RGBA16S:
                case T3SurfaceFormat.R16UI:
                case T3SurfaceFormat.RG16UI:
                case T3SurfaceFormat.R16F:
                case T3SurfaceFormat.RG16F:
                case T3SurfaceFormat.RGBA16F:
                case T3SurfaceFormat.R32F:
                case T3SurfaceFormat.RG32F:
                case T3SurfaceFormat.RGBA32F:
                case T3SurfaceFormat.RGBA1010102F:
                case T3SurfaceFormat.RGB111110F:
                case T3SurfaceFormat.RGB9E5F:
                case T3SurfaceFormat.DepthPCF16:
                case T3SurfaceFormat.DepthPCF24:
                case T3SurfaceFormat.Depth16:
                case T3SurfaceFormat.Depth24:
                case T3SurfaceFormat.DepthStencil32:
                case T3SurfaceFormat.Depth32F:
                case T3SurfaceFormat.Depth32F_Stencil8:
                case T3SurfaceFormat.Depth24F_Stencil8:
                case T3SurfaceFormat.BC1:
                case T3SurfaceFormat.BC2:
                case T3SurfaceFormat.BC3:
                case T3SurfaceFormat.BC4:
                case T3SurfaceFormat.BC5:
                case T3SurfaceFormat.CTX1:
                case T3SurfaceFormat.BC6:
                case T3SurfaceFormat.BC7:
                case T3SurfaceFormat.PVRTC2:
                case T3SurfaceFormat.PVRTC4:
                case T3SurfaceFormat.PVRTC2a:
                case T3SurfaceFormat.PVRTC4a:
                case T3SurfaceFormat.ATC_RGB:
                case T3SurfaceFormat.ATC_RGB1A:
                case T3SurfaceFormat.ATC_RGBA:
                case T3SurfaceFormat.ETC1_RGB:
                case T3SurfaceFormat.ETC2_RGB:
                case T3SurfaceFormat.ETC2_RGB1A:
                case T3SurfaceFormat.ETC2_RGBA:
                case T3SurfaceFormat.ETC2_R:
                case T3SurfaceFormat.ETC2_RG:
                case T3SurfaceFormat.ATSC_RGBA_4x4:
                case T3SurfaceFormat.FrontBuffer:
                    return true;
                default:
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Get the correct DDS file from Telltale texture.????
        /// </summary>
        /// <param name="d3dtx"></param>
        /// <returns></returns>
        public byte[] GetData(D3DTX_Master d3dtx)
        {
            return [];

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
