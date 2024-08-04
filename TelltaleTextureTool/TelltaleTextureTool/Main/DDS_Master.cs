using System;
using System.Collections.Generic;
using System.IO;
using TelltaleTextureTool.Utilities;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.TelltaleEnums;
using System.Linq;
using Hexa.NET.DirectXTex;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;

namespace TelltaleTextureTool.Main
{
    /// <summary>
    /// Main class for generating a DDS file from a D3DTX.
    /// </summary>
    public unsafe class DDS_Master
    {
        public byte[] header = [];
        public byte[] pixelData = [];

        /// <summary>
        /// Create a DDS file from a D3DTX.
        /// </summary>
        /// <param name="d3dtx">The D3DTX data that will be used.</param>
        public DDS_Master(D3DTX_Master d3dtx)
        {
            if (d3dtx.HasDDSHeader())
            {
                return;
            }

            InitializeDDSHeader(d3dtx);
            InitializeDDSPixelData(d3dtx);
        }

        private void InitializeDDSHeader(D3DTX_Master d3dtx)
        {
            ScratchImage image = DirectXTex.CreateScratchImage();
            D3DTXMetadata d3dtxMetadata = d3dtx.GetMetadata();

            Console.WriteLine("D3dtx width: " + d3dtxMetadata.Width);
            Console.WriteLine("D3dtx height: " + d3dtxMetadata.Height);
            Console.WriteLine("D3dtx mip map count: " + d3dtxMetadata.MipLevels);
            Console.WriteLine("D3dtx d3d format: " + d3dtxMetadata.D3DFormat);

            T3SurfaceFormat surfaceFormat = d3dtxMetadata.Format;
            T3SurfaceGamma surfaceGamma = d3dtxMetadata.SurfaceGamma;
            T3PlatformType platformType = d3dtxMetadata.Platform;

            TexMetadata metadata = new()
            {
                Width = d3dtxMetadata.Width,
                Height = d3dtxMetadata.Height,
                ArraySize = d3dtxMetadata.ArraySize,
                Depth = d3dtxMetadata.Depth,
                MipLevels = d3dtxMetadata.MipLevels,
                Format = d3dtx.IsLegacyD3DTX() ? (int)DDS_HELPER.GetDXGIFormat(d3dtxMetadata.D3DFormat) : (int)DDS_HELPER.GetDXGIFormat(surfaceFormat, surfaceGamma, platformType),
                Dimension = d3dtxMetadata.IsVolumemap() ? TexDimension.Texture3D : TexDimension.Texture2D,
            };

            if (D3DTX_Master.IsFormatIncompatibleWithDDS(surfaceFormat) || D3DTX_Master.IsPlatformIncompatibleWithDDS(platformType))
            {
                metadata.MipLevels = 1;
            }

            image.Initialize(ref metadata, CPFlags.None);

            header = DDS_DirectXTexNet.GetDDSHeaderBytes(image);

            Console.WriteLine("Header length: " + header.Length);

            image.Release();
        }

        private void InitializeDDSPixelData(D3DTX_Master d3dtx)
        {
            // NOTES: Telltale mip levels are reversed in Poker Night 2 and above. The first level are the smallest mip levels and the last level is the largest mip level.
            // The faces are NOT reverse.
            // This is likely corelating with the way that KTX and KTX2 files are written.
            // Some normal maps specifically with type 4 (eTxNormalMap) channels are all reversed (ABGR instead of RGBA) (Only applies for newer games)
            // Some surface formats are dependant on platforms. For example, iOS textures have their R and B channels swapped.
            // Some surface formats are not supported by DDS. In this case, the texture will be written as a raw texture.

            if (d3dtx.IsLegacyD3DTX())
            {
                pixelData = d3dtx.GetPixelData()[d3dtx.GetPixelData().Count - 1];
                return;
            }

            D3DTXMetadata metadata = d3dtx.GetMetadata();

            T3SurfaceFormat surfaceFormat = metadata.Format;

            List<byte[]> textureData = [];

            if (D3DTX_Master.IsFormatIncompatibleWithDDS(surfaceFormat) || D3DTX_Master.IsPlatformIncompatibleWithDDS(metadata.Platform))
            {
                textureData.Add(d3dtx.GetPixelDataByFirstMipmapIndex(metadata.Format, (int)metadata.Width, (int)metadata.Height, metadata.Platform));
            }
            else
            {
                if (metadata.IsVolumemap())
                {
                    int divideBy = 1;
                    for (int i = 0; i < metadata.MipLevels; i++)
                    {
                        textureData.Add(d3dtx.GetPixelDataByMipmapIndex(i, metadata.Format, (int)metadata.Width / divideBy, (int)metadata.Height / divideBy, metadata.Platform));
                        divideBy *= 2;
                    }
                }
                else
                {
                    int totalFaces = (int)(metadata.IsCubemap() ? metadata.ArraySize * 6 : metadata.ArraySize);

                    // Get each face of the 2D texture
                    for (int i = 0; i < totalFaces; i++)
                    {
                        textureData.Add(d3dtx.GetPixelDataByFaceIndex(i, metadata.Format, (int)metadata.Width, (int)metadata.Height, metadata.Platform));
                    }
                }
            }

            pixelData = textureData.SelectMany(b => b).ToArray();
        }

        /// <summary>
        /// Writes a D3DTX into a DDS file on the disk.
        /// </summary>
        /// <param name="d3dtx"></param>
        /// <param name="destinationPath"></param>
        public void WriteD3DTXAsDDS(D3DTX_Master d3dtx, string destinationDirectory)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string fileName = Path.GetFileNameWithoutExtension(d3dtx.FilePath);

            byte[] pixelData = GetData(d3dtx);
            string newDDSPath = destinationDirectory + Path.DirectorySeparatorChar + fileName +
                                            Main_Shared.ddsExtension;

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Time to get data: {0}", elapsedMs);
            File.WriteAllBytes(newDDSPath, pixelData);
        }

        /// <summary>
        /// Get the correct DDS data from a Telltale texture.
        /// </summary>
        /// <param name="d3dtx"></param>
        /// <returns></returns>
        public byte[] GetData(D3DTX_Master d3dtx)
        {
            Console.WriteLine("Getting data for: " + d3dtx.FilePath);

            // If the D3DTX exists, return the pixel data.
            if (d3dtx.d3dtxObject == null)
            {
                throw new InvalidDataException("There is no pixel data to be written.");
            }

            // If the D3DTX has a DDS header, return the whole pixel data.
            if (d3dtx.HasDDSHeader())
            {
                return d3dtx.GetPixelData()[d3dtx.GetPixelData().Count - 1];
            }

            // Return the created DDS file.
            return ByteFunctions.Combine(header, pixelData);
        }
    }
}
