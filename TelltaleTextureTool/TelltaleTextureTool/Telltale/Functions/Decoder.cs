using System;

namespace TelltaleTextureTool.TelltaleFunctions
{
    public static class Decoder
    {

        public static byte[] MetaDecrypt(byte[] data, int skipLength, int meta)
        {
            ulong i, blocksSize = 0, blocksCrypt = 0, blocksClean = 0, blocks;

            void SetBlocks(ulong x, ulong y, ulong z)
            {
                blocksSize = x;
                blocksCrypt = y;
                blocksClean = z;
            }

            int pIndex = 0;

            if (skipLength < 4) return data;

            uint fileType = GetUInt32(data, ref pIndex);
            switch (fileType)
            {
                case 0x4D424553: SetBlocks(0x40, 0x40, 0x64); break;  // SEBM
                case 0x4D42494E: break;  // NIBM
                case 0xFB4A1764: SetBlocks(0x80, 0x20, 0x50); break;
                case 0xEB794091: SetBlocks(0x80, 0x20, 0x50); break;
                case 0x64AFDEFB: SetBlocks(0x80, 0x20, 0x50); break;
                case 0x64AFDEAA: SetBlocks(0x100, 0x8, 0x18); break;
                case 0x4D545245: break;  // ERTM
                default: break;  // is not a meta stream file
            }

            if (blocksSize > 0) // meta, just the same result
            {
                blocks = (ulong)(skipLength - 4) / blocksSize;
                for (i = 0; i < blocks; i++)
                {
                    if (pIndex >= skipLength) break;
                    if (i % blocksCrypt == 0)
                    {
                       // Blowfish(data, pIndex, (int)blocksSize, encrypt);
                    }
                    else if (i % blocksClean == 0 && i > 0)
                    {
                        // skip this block
                    }
                    else
                    {
                        Xor(data, pIndex, (int)blocksSize, 0xff);
                    }
                    pIndex += (int)blocksSize;
                }
            }

            return data;
        }

        private static uint GetUInt32(byte[] data, ref int index)
        {
            uint result = BitConverter.ToUInt32(data, index);
            index += 4;
            return result;
        }

        private static void Blowfish(byte[] data, int index, int length, bool encrypt)
        {
            // Implement Blowfish encryption/decryption here
        }

        private static void Xor(byte[] data, int index, int length, byte value)
        {
            for (int i = 0; i < length; i++)
            {
                data[index + i] ^= value;
            }
        }
    }
}
