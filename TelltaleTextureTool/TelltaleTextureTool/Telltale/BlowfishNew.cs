using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelltaleTextureTool.Telltale
{
    using System;
    using System.IO;

    public class Blowfish
    {
        private static uint[][] sbox = new uint[4][];
        private static uint[] pArray = new uint[18];
        private const int pArrayLength = 18;

        static Blowfish()
        {
            // Initialize sbox and pArray with proper values.
            // These values need to be properly initialized with the actual Blowfish initial values.
            // For example:
            // sbox[0] = new uint[256] { ... };
            // sbox[1] = new uint[256] { ... };
            // sbox[2] = new uint[256] { ... };
            // sbox[3] = new uint[256] { ... };
            // pArray = new uint[18] { ... };
        }

        private static uint F(uint leftHalf)
        {
            byte box0 = (byte)(leftHalf >> 24);
            byte box1 = (byte)(leftHalf >> 16);
            byte box2 = (byte)(leftHalf >> 8);
            byte box3 = (byte)leftHalf;

            uint a = sbox[0][box0];
            uint b = sbox[1][box1];
            uint c = sbox[2][box2];
            uint d = sbox[3][box3];

            uint output = a + b;
            output ^= c;
            output += d;

            return output;
        }

        private static void EncryptBlock(ref ulong block)
        {
            uint rightHalf = (uint)(block & 0xFFFFFFFF);
            uint leftHalf = (uint)(block >> 32);
            uint temp;

            for (int i = 0; i < pArrayLength - 2; ++i)
            {
                leftHalf ^= pArray[i];
                rightHalf ^= F(leftHalf);
                temp = leftHalf;
                leftHalf = rightHalf;
                rightHalf = temp;
            }

            temp = leftHalf;
            leftHalf = rightHalf;
            rightHalf = temp;

            rightHalf ^= pArray[pArrayLength - 2];
            leftHalf ^= pArray[pArrayLength - 1];

            block = ((ulong)leftHalf << 32) | rightHalf;
        }

        private static void DecryptBlock(ref ulong block)
        {
            uint rightHalf = (uint)(block & 0xFFFFFFFF);
            uint leftHalf = (uint)(block >> 32);
            uint temp;

            for (int i = pArrayLength - 1; i > 1; --i)
            {
                leftHalf ^= pArray[i];
                rightHalf ^= F(leftHalf);
                temp = leftHalf;
                leftHalf = rightHalf;
                rightHalf = temp;
            }

            temp = leftHalf;
            leftHalf = rightHalf;
            rightHalf = temp;

            rightHalf ^= pArray[1];
            leftHalf ^= pArray[0];

            block = ((ulong)leftHalf << 32) | rightHalf;
        }

        public static void EncryptData(ulong[] data)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                EncryptBlock(ref data[i]);
            }
        }

        public static void DecryptData(ulong[] data)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                DecryptBlock(ref data[i]);
            }
        }

        public static void InitBlowfish(byte[] key)
        {
            int keyIndex = 0;
            for (int i = 0; i < pArrayLength; ++i)
            {
                for (int j = 0; j < sizeof(uint); ++j)
                {
                    if (keyIndex >= key.Length)
                    {
                        keyIndex = 0;
                    }
                    pArray[i] = (pArray[i] & (uint)(0xFFFFFFFF00FFFFFF >> (8 * j))) |
                                ((uint)((byte)(pArray[i] >> (24 - (8 * j))) ^ key[keyIndex++]) << (24 - (8 * j)));
                }
            }

            ulong block = 0x0000000000000000;
            for (int i = 0; i < pArrayLength; i += 2)
            {
                EncryptBlock(ref block);
                uint rightHalf = (uint)(block);
                uint leftHalf = (uint)(block >> 32);
                pArray[i] = leftHalf;
                pArray[i + 1] = rightHalf;
            }

            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 256; j += 2)
                {
                    EncryptBlock(ref block);
                    uint rightHalf = (uint)(block);
                    uint leftHalf = (uint)(block >> 32);
                    sbox[i][j] = leftHalf;
                    sbox[i][j + 1] = rightHalf;
                }
            }
        }

        public static void PrintData(ulong[] data)
        {
            foreach (var datum in data)
            {
                Console.WriteLine($"{datum:X}");
            }
        }

        public static void PrintText(ulong[] data)
        {
            foreach (var datum in data)
            {
                for (int j = 0; j < sizeof(ulong); ++j)
                {
                    Console.Write((char)(datum >> (8 * j)));
                }
            }
        }

        private static uint BSwap(uint num)
        {
            return (((num & 0xff000000) >> 24) |
                    ((num & 0x00ff0000) >> 8) |
                    ((num & 0x0000ff00) << 8) |
                    ((num & 0x000000ff) << 24));
        }

        public static void InitBlowfish7(byte[] key)
        {
            sbox[0][118] = BSwap(sbox[0][118]);
            int keyIndex = 0;
            for (int i = 0; i < pArrayLength; ++i)
            {
                for (int j = 0; j < sizeof(uint); ++j)
                {
                    if (keyIndex >= key.Length)
                    {
                        keyIndex = 0;
                    }
                    pArray[i] = (pArray[i] & (uint)(0xFFFFFFFF00FFFFFF >> (8 * j))) |
                                ((uint)((byte)(pArray[i] >> (24 - (8 * j))) ^ key[keyIndex++]) << (24 - (8 * j)));
                }
            }

            ulong block = 0x0000000000000000;
            for (int i = 0; i < pArrayLength; i += 2)
            {
                EncryptBlock(ref block);
                uint rightHalf = (uint)(block);
                uint leftHalf = (uint)(block >> 32);
                pArray[i] = leftHalf;
                pArray[i + 1] = rightHalf;
            }

            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 256; j += 2)
                {
                    EncryptBlock(ref block);
                    uint rightHalf = (uint)(block);
                    uint leftHalf = (uint)(block >> 32);
                    sbox[i][j] = leftHalf;
                    sbox[i][j + 1] = rightHalf;
                }
            }
        }

        public static void VersDB()
        {
            using (FileStream dataBase = new FileStream("../dataBase/WDC.VersDB", FileMode.Open, FileAccess.Read))
            using (FileStream text = new FileStream("./typeNames3.txt", FileMode.Create, FileAccess.Write))
            {
                dataBase.Seek(0x27950, SeekOrigin.Begin);
                int byteRead;
                while ((byteRead = dataBase.ReadByte()) != -1)
                {
                    byte b = (byte)byteRead;
                    if (b == 0)
                    {
                        text.WriteByte((byte)',');
                        b = (byte)'\n';
                    }
                    text.WriteByte(b);
                }
            }
        }

        public static void Main(string[] args)
        {
            // Example usage of the methods
            byte[] key = { /* some key data */ };
            InitBlowfish(key);

            ulong[] data = { /* some data */ };
            EncryptData(data);
            PrintData(data);

            DecryptData(data);
            PrintData(data);

            VersDB();


}

    }

}
