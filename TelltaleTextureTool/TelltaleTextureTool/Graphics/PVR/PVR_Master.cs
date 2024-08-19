using ExCSS;
using PVRTexLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;

namespace TelltaleTextureTool.Graphics.PVR
{
    internal class PVR_Main
    {
        private PVRTexture texture;

        public PVR_Main() { }
        //public EncodeTexture(D3DTXMetadata texture, byte[] data)
        //{
        //    ulong RGBA8888 = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 8, 8, 8, 8);
        //    uint width = texture.Width;
        //    uint height = texture.Height;
        //    uint depth = texture.Depth;
        //    uint numMipMaps = texture.MipLevels;
        //    uint numArrayMembers = texture.ArraySize;
        //    uint numFaces = texture.IsCubemap() ? 6u : 1u;
        //    using PVRTextureHeader textureHeader = new PVRTextureHeader(RGBA8888, width, height, depth, numMipMaps, numArrayMembers, numFaces);
        //    ulong textureSize = textureHeader.GetTextureDataSize();

        //    // Create a buffer to temporarily hold the pixel data
        //    byte[] textureData = new byte[textureSize];

        //    /*
        //      Fill in texture data...
        //    */


        //    // Create a new texture from the header and pixel data.
        //    unsafe
        //    {
        //        fixed (byte* ptr = &textureData[0])
        //        {
        //            using PVRTexture texture = new PVRTexture(textureHeader, ptr);
        //            return texture.SaveToFile(outFilePath);
        //        }
        //    }
        //}

        public static byte[] DecodeTexture(D3DTXMetadata d3dtxMetadata, byte[] data)
        {
            var format = PVR_Helper.GetPVRFormat(d3dtxMetadata.Format);
            ulong RGBA8888 = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 8, 8, 8, 8);
            uint width = d3dtxMetadata.Width;
            uint height = d3dtxMetadata.Height;
            uint depth = d3dtxMetadata.Depth;
            uint numMipMaps = d3dtxMetadata.MipLevels;
            uint numArrayMembers = d3dtxMetadata.ArraySize;
            uint numFaces = d3dtxMetadata.IsCubemap() ? 6U : 1U;
            using PVRTextureHeader textureHeader = new((ulong)format, width, height, depth, numMipMaps, numArrayMembers, numFaces);
            ulong textureSize = textureHeader.GetTextureDataSize();

            if (textureSize == 0)
            {
                throw new Exception("Could not create PVR header!");
            }

            unsafe
            {
                fixed (byte* ptr = &data[0])
                {
                    using PVRTexture texture = new PVRTexture(textureHeader, ptr);

                    var colorSpace = d3dtxMetadata.SurfaceGamma == TelltaleEnums.T3SurfaceGamma.eSurfaceGamma_sRGB ? PVRTexLibColourSpace.sRGB : PVRTexLibColourSpace.Linear;
                    texture.Transcode(RGBA8888, PVRTexLibVariableType.UnsignedByteNorm, colorSpace);

                    void* ddsPtr = null;
                    try
                    {
                        ddsPtr = texture.SaveTextureToMemory(PVRTexLibFileContainerType.DDS, out ulong size);
                        
                        byte[] ddsData = new byte[size];

                        Marshal.Copy(new IntPtr(ddsPtr), ddsData, 0, (int)size);
                        
                        return ddsData;
                    }
                    finally
                    {
                        texture.Dispose();
                    }

                }
            }
        }


    }
}
