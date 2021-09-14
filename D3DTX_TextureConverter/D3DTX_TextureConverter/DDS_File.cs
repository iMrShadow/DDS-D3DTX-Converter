﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DirectXTexNet;
using D3DTX_TextureConverter.Utilities;
using D3DTX_TextureConverter.DirectX;

namespace D3DTX_TextureConverter
{
    /// <summary>
    /// Main class for generating a dds byte header
    /// </summary>
    public class DDS_File
    {
        //dds image file extension
        public static string ddsExtension = ".dds";

        //main ddsPrefix (with space) [4 bytes]
        public readonly string ddsPrefix = "DDS ";

        public string sourceFileName; //file name + extension
        public string sourceFile; //file path
        public byte[] sourceFileData; //file data
        public uint[,] mipImageResolutions; //calculated mip resolutions [Pixel Value, Width or Height (0 or 1)]
        public byte[] ddsTextureData;

        public DDS_HEADER header;

        /// <summary>
        /// Manually builds a byte array of a dds header
        /// </summary>
        /// <returns></returns>
        public byte[] Build_DDSHeader_ByteArray()
        {
            //allocate our header byte array
            byte[] ddsHeader = new byte[128];

            //return the result
            return ddsHeader;
        }

        public void Read_DDS_File(string sourceFile, string sourceFileName, bool readHeaderOnly)
        {
            /*
             * NOTE TO SELF
             * DDS --> D3DTX EXTRACTION, THE BYTES ARE NOT FULLY 1:1 WHEN THERE IS A CONVERSION (off by 8 bytes)
             * MABYE TRY TO CHANGE THE TEXTURE DATA BYTE SIZE IN THE D3DTX HEADER AND SEE IF THAT CHANGES ANYTHING?
            */

            //read the source texture file into a byte array
            sourceFileData = File.ReadAllBytes(sourceFile);
            this.sourceFileName = sourceFileName;
            this.sourceFile = sourceFile;

            ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.White); 
            Console.WriteLine("Total Source Texture Byte Size = {0}", sourceFileData.Length);

            //which byte offset we are on for the source texture (will be changed as we go through the file)
            uint bytePointerPosition = 4; //skip past the 'DDS '
            byte[] headerBytes = ByteFunctions.AllocateBytes(124, sourceFileData, bytePointerPosition);

            //this will automatically read all of the byte data in the header
            header = DDS_Functions.GetHeaderFromBytes(headerBytes);

            ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.White);
            Console.WriteLine("DDS Height = {0}", header.dwHeight);
            Console.WriteLine("DDS Width = {0}", header.dwWidth);
            Console.WriteLine("DDS Compression = {0}", header.ddspf.dwFourCC);

            //if we are not reading
            if(!readHeaderOnly)
            {
                //--------------------------EXTRACT DDS TEXTURE DATA--------------------------
                //calculate dds header length (we add 4 because we skipped the 4 bytes which contain the ddsPrefix, it isn't necessary to parse this data)
                uint ddsHeaderLength = 4 + header.dwSize;

                //calculate the length of just the dds texture data
                uint ddsTextureDataLength = (uint)sourceFileData.Length - ddsHeaderLength;

                //allocate a byte array of dds texture length
                ddsTextureData = new byte[ddsTextureDataLength];

                //copy the data from the source byte array past the header (so we are only getting texture data)
                Array.Copy(sourceFileData, ddsHeaderLength, ddsTextureData, 0, ddsTextureData.Length);

                //--------------------------CALCULATE MIP MAP RESOLUTIONS--------------------------
                //because I suck at math, we will generate our mip map resolutions using the same method we did in d3dtx to dds (can't figure out how to calculate them in reverse properly)
                mipImageResolutions = new uint[header.dwMipMapCount + 1, 2];

                //get our mip image dimensions (have to multiply by 2 as the mip calculations will be off by half)
                uint mipImageWidth = header.dwWidth * 2;
                uint mipImageHeight = header.dwHeight * 2;

                //add the resolutions in reverse
                for (uint i = header.dwMipMapCount; i > 0; i--)
                {
                    //divide the resolutions by 2
                    mipImageWidth /= 2;
                    mipImageHeight /= 2;

                    //assign the resolutions
                    mipImageResolutions[i, 0] = mipImageWidth;
                    mipImageResolutions[i, 1] = mipImageHeight;
                }
            }
        }
    }
}
