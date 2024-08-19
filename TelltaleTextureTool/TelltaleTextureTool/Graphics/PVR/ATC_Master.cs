using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelltaleTextureTool.Main;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;

namespace TelltaleTextureTool.Graphics.PVR
{
    internal class ATC_Master
    {

        public static byte[] Decode(D3DTXMetadata d3dtxMetadata, byte[] data)
        {
            // Arrange
            var decoder = new BcDecoder();
            
            DdsFile file = DdsFile.Load(new MemoryStream(data));
            var images = decoder.DecodeAllMipMaps(file);

            List<byte> ddsData = new List<byte>();

            foreach (var image in images) {


                foreach (var pixel in image)
                {
                    ddsData.Add(pixel.r);
                    ddsData.Add(pixel.g);
                    ddsData.Add(pixel.b);
                    ddsData.Add(pixel.a);
                }
            }
            return ddsData.ToArray();
        }
    }
}
