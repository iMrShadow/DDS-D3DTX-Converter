using System;
using System.Collections.Generic;
using System.IO;
using TelltaleTextureTool.DirectX;
using System.Linq;
using static Ktx.Ktx2;

namespace TelltaleTextureTool.Main
{
    /// <summary>
    /// Main class for managing D3DTX files and converting them to KTX2.
    /// </summary>
    public class KTX2_Master
    {
        TextureCreateInfo ktx2Texture;

        // /// <summary>
        // /// Create a KTX2 file from a D3DTX.
        // /// </summary>
        // /// <param name="d3dtx">The D3DTX data that will be used.</param>
        // unsafe public Texture* InitializeTextureHeader(D3DTX_Master d3dtx)
        // {
        //     if (d3dtx.IsLegacyD3DTX())
        //     {
        //         throw new NotImplementedException("KTX2 does not support legacy versions.");
        //     }

        //     // Initialize the KTX2 Header
        //     ErrorCode error = Create(new TextureCreateInfo()
        //     {
        //         BaseHeight = (uint)d3dtx.GetHeight(),
        //         BaseWidth = (uint)d3dtx.GetWidth(),
        //         BaseDepth = (uint)d3dtx.GetDepth(),
        //         VkFormat = KTX2_HELPER.GetVkFormatFromTelltaleSurfaceFormat(d3dtx.GetCompressionType(), d3dtx.GetSurfaceGamma(), d3dtx.GetAlpha()),
        //         NumLevels = (uint)d3dtx.GetMipMapCount(),
        //         NumLayers = (uint)d3dtx.GetArraySize(),
        //         NumFaces = (uint)(d3dtx.IsCubeTexture() ? 6 : 1),
        //         IsArray = (byte)(d3dtx.IsArrayTexture() ? 1 : 0),
        //         NumDimensions = (uint)(d3dtx.IsVolumeTexture() ? 3 : 2)
        //     },

        //     TextureCreateStorage.AllocStorage, out Texture* test);


        //     List<byte[]> textureData = [];

        //     // Get all pixel data from the D3DTX
        //     var d3dtxTextureData = d3dtx.GetPixelData();

        //     int divideBy = 1;

        //     // Get the pixel data by mipmap levels and unswizzle depending on the Platform
        //     for (int i = (int)(d3dtx.GetMipMapCount() - 1); i >= 0; i--)
        //     {
        //         textureData.Add(d3dtx.GetPixelDataByMipmapIndex(i, d3dtx.GetCompressionType(), (int)d3dtx.GetWidth() / divideBy, (int)d3dtx.GetHeight() / divideBy, d3dtx.GetPlatformType()));
        //         divideBy *= 2;
        //     }

        //     byte[] d3dtxTextureDataArray = textureData.SelectMany(b => b).ToArray();

        //     // // Attempt to put pixel data into the Texture
        //     return test;
        // }

        // /// <summary>
        // /// Writes a D3DTX into a KTX2 file on the disk.
        // /// </summary>
        // /// <param name="d3dtx"></param>
        // /// <param name="destinationDirectory"></param>
        // unsafe public void WriteD3DTXAsKTX2(D3DTX_Master d3dtx, string destinationDirectory)
        // {
        //     var watch = System.Diagnostics.Stopwatch.StartNew();
        //     string d3dtxFilePath = d3dtx.filePath;
        //     string fileName = Path.GetFileNameWithoutExtension(d3dtxFilePath);

        //     string newKTX2Path = destinationDirectory + Path.DirectorySeparatorChar + fileName +
        //                                                Main_Shared.ktx2Extension;

        //   //  Ktx2.WriteToNamedFile(texture, newKTX2Path);
        //     watch.Stop();
        //     var elapsedMs = watch.ElapsedMilliseconds;
        //     Console.WriteLine("Time to get data: {0}", elapsedMs);
        // }
    }
}
