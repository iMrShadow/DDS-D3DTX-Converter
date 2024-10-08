﻿using System;
using System.Collections.Generic;
using System.IO;
using D3DTX_Converter.Utilities;
using D3DTX_Converter.Main;
using D3DTX_Converter.Texconv;
using D3DTX_Converter.TexconvOptions;
using D3DTX_Converter.ImageProcessing;

namespace D3DTX_Converter.ProgramDebug
{
    public static class Program_JPEG_TO_DDS
    {
        public static void Execute(bool fixes_generic_to_dds)
        {
            //intro message
            ConsoleFunctions.SetConsoleColor(ConsoleColor.Blue, ConsoleColor.White);
            Console.WriteLine("JPEG to DDS Texture Converter");

            //-----------------GET TEXTURE FOLDER PATH-----------------
            ConsoleFunctions.SetConsoleColor(ConsoleColor.DarkGray, ConsoleColor.White);
            Console.WriteLine("Enter the folder path with the JPEG textures.");

            //texture folder path (containing the path to the textures to be converted)
            string textureFolderPath = Program_Shared.GetFolderPathFromUser();

            //-----------------GET RESULT FOLDER PATH-----------------
            ConsoleFunctions.SetConsoleColor(ConsoleColor.DarkGray, ConsoleColor.White);
            Console.WriteLine("Enter the resulting path where converted DDS textures will be stored.");

            //result folder path (will contain the converted textures)
            string resultFolderPath = Program_Shared.GetFolderPathFromUser();

            //-----------------START CONVERSION-----------------
            //notify the user we are starting
            ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Green);
            Console.WriteLine("Conversion Starting...");

            //we got our paths, so lets begin
            ConvertBulk(textureFolderPath, resultFolderPath, fixes_generic_to_dds);

            //once the process is finished, it will come back here and we will notify the user that we are done
            ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Green);
            Console.WriteLine("Conversion Finished.");
            Console.ResetColor();
        }

        /// <summary>
        /// Begins the conversion process. Gathers the files found in the texture folder path, filters them, and converts each one.
        /// </summary>
        /// <param name="texPath"></param>
        /// <param name="resultPath"></param>
        public static void ConvertBulk(string texPath, string resultPath, bool fixes_generic_to_dds)
        {
            ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Yellow);
            Console.WriteLine("Collecting Files..."); //notify the user we are collecting files

            //gather the files from the texture folder path into an array
            List<string> textures = new List<string>(Directory.GetFiles(texPath));

            ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Yellow);
            Console.WriteLine("Filtering Textures..."); //notify the user we are filtering the array

            //filter the array so we only get .jpeg files
            textures = IOManagement.FilterFiles(textures, new string[2] { Main_Shared.jpgExtension, Main_Shared.jpegExtension });

            //if no jpeg files were found, abort the program from going on any further (we don't have any files to convert!)
            if (textures.Count < 1)
            {
                ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Red);
                Console.WriteLine("No .jpeg files were found, aborting."); //notify the user we found x amount of jpeg files in the array
                Console.ResetColor();
                return;
            }

            ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Green);
            Console.WriteLine("Found {0} Textures.", textures.Count.ToString()); //notify the user we found x amount of jpeg files in the array
            Console.WriteLine("Starting...");//notify the user we are starting

            //run a loop through each of the found textures and convert each one
            for (int i = 0; i < textures.Count; i++)
            {
                //build the path for the resulting file
                string textureFileName = Path.GetFileName(textures[i]); //get the file name of the file + extension

                ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.White);
                Console.WriteLine("||||||||||||||||||||||||||||||||");
                ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Blue);
                Console.WriteLine("Converting '{0}'...", textureFileName); //notify the user are converting 'x' file.
                Console.ResetColor();

                //runs the main method for converting the texture
                ConvertTextureFile(textures[i], resultPath, fixes_generic_to_dds);

                ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Green);
                Console.WriteLine("Finished converting '{0}'...", textureFileName); //notify the user we finished converting 'x' file.
                ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.White);
            }
        }

        /// <summary>
        /// The main function for reading and converting said .jpeg into a .dds file
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destinationFile"></param>
        public static void ConvertTextureFile(string sourceFilePath, string destinationDirectory, bool fixes_generic_to_dds)
        {
            //deconstruct the source file path
            string textureFileDirectory = Path.GetDirectoryName(sourceFilePath);
            string textureFileNameOnly = Path.GetFileNameWithoutExtension(sourceFilePath);

            //create the names of the following files
            string textureFileNameWithJSON = textureFileNameOnly + Main_Shared.jsonExtension;

            //create the path of these files. If things go well, these files (depending on the version) should exist in the same directory at the original .dds file.
            string textureFilePath_JSON = textureFileDirectory + @"\" + textureFileNameWithJSON;

            //if a json file exists (for newer 5VSM and 6VSM)
            if (File.Exists(textureFilePath_JSON))
            {
                //create a new d3dtx object
                D3DTX_Master d3dtx_file = new();

                //parse the .json file as a d3dtx
                d3dtx_file.Read_D3DTX_JSON(textureFilePath_JSON);

                MasterOptions options = new();
                options.outputDirectory = new() { directory = destinationDirectory };
                options.outputOverwrite = new();
                options.outputFileType = new() { fileType = TexconvEnums.TexconvEnumFileTypes.dds };

                if (d3dtx_file.HasMipMaps() == false)
                    options.outputMipMaps = new() { remove = true };

                switch (d3dtx_file.GetTextureType())
                {
                    case TelltaleEnums.T3TextureType.eTxSingleChannelSDFDetailMap:
                        options.outputFormat = new() { format = DirectXTexNet.DXGI_FORMAT.BC3_UNORM };

                        TexconvApp.RunTexconv(sourceFilePath, options);
                        break;
                    case TelltaleEnums.T3TextureType.eTxBumpmap:
                    case TelltaleEnums.T3TextureType.eTxNormalMap:

                        options.outputFormat = new() { format = DirectXTexNet.DXGI_FORMAT.BC3_UNORM };
                        options.outputTreatTypelessAsUNORM = new();

                        if (fixes_generic_to_dds)
                            options.outputSwizzle = new() { mask = "abgr" };

                        TexconvApp.RunTexconv(sourceFilePath, options);

                        break;
                    case TelltaleEnums.T3TextureType.eTxNormalXYMap:

                        options.outputFormat = new() { format = DirectXTexNet.DXGI_FORMAT.BC5_UNORM };
                        //options.outputSRGB = new() { srgbMode = TexconvEnums.TexconvEnumSrgb.srgbo };
                        options.outputTreatTypelessAsUNORM = new();

                        if (fixes_generic_to_dds)
                            options.outputSwizzle = new() { mask = "rg00" };

                        TexconvApp.RunTexconv(sourceFilePath, options);

                        break;
                    default:
                        if (ImageUtilities.IsImageOpaque(sourceFilePath))
                            options.outputFormat = new() { format = DirectXTexNet.DXGI_FORMAT.BC1_UNORM };
                        else
                            options.outputFormat = new() { format = DirectXTexNet.DXGI_FORMAT.BC3_UNORM };

                        TexconvApp.RunTexconv(sourceFilePath, options);
                        break;
                }
            }
            //if we didn't find a json file, we're screwed!
            else
            {
                ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Red);
                Console.WriteLine("NO '.json' WAS FOUND FOR THE FILE WE ARE TRYING TO CONVERT!!!!");
                Console.WriteLine("{0}", textureFileNameOnly);
                Console.WriteLine("Skipping conversion on this file.", textureFileNameOnly);
                ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.White);

                return;
            }

            string outputTextureFilePath = destinationDirectory + @"\" + Path.GetFileNameWithoutExtension(sourceFilePath) + Main_Shared.ddsExtension;

            //check if the output file exists, if it doesn't then the conversion failed so notify the user
            if (File.Exists(outputTextureFilePath) == false)
            {
                ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Red);
                Console.WriteLine("Failed to convert JPEG image into a DDS!!!");
                Console.WriteLine("{0}", Path.GetFileNameWithoutExtension(sourceFilePath));
            }
        }
    }
}
