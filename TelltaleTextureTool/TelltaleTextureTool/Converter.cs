using System;
using System.Collections.Generic;
using System.IO;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.Main;
using TelltaleTextureTool.Utilities;
using System.Threading;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using Hexa.NET.DirectXTex;
using TelltaleTextureTool.DirectX.Enums;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;
using TelltaleTextureTool.TelltaleEnums;

namespace TelltaleTextureTool;

public static class Converter
{
    public static string[] GetExtension(TextureType textureType)
    {
        return textureType switch
        {
            TextureType.D3DTX => [Main_Shared.d3dtxExtension],
            TextureType.DDS => [Main_Shared.ddsExtension],
            TextureType.KTX => [Main_Shared.ktxExtension],
            TextureType.KTX2 => [Main_Shared.ktx2Extension],
            TextureType.PNG => [Main_Shared.pngExtension],
            TextureType.JPEG => [Main_Shared.jpegExtension, Main_Shared.jpgExtension],
            TextureType.BMP => [Main_Shared.bmpExtension],
            TextureType.TIFF => [Main_Shared.tiffExtension, Main_Shared.tifExtension],
            TextureType.TGA => [Main_Shared.tgaExtension],
            TextureType.HDR => [Main_Shared.hdrExtension],
            _ => throw new InvalidEnumArgumentException("Invalid texture type."),
        };
    }

    /// <summary>
    /// Converts multiple texture files from one format to another.
    /// </summary>
    /// <param name="texPath">The path to the folder containing the texture files.</param>
    /// <param name="resultPath">The path to save the converted texture files.</param>
    /// <param name="options">The advanced options to apply to the texture files.</param>
    /// <param name="oldTextureType">The file type of the original texture files.</param>
    /// <param name="newTextureType">The file type to convert the texture files to.</param>
    /// <returns>True if the bulk conversion is successful; otherwise, false.</returns>
    /// <exception cref="FileNotFoundException">Thrown when no files with the specified file type were found in the directory.</exception>
    /// <exception cref="Exception">Thrown when an invalid file type is provided or when one or more conversions fail.</exception>
    public static bool ConvertBulk(string texPath, string resultPath, ImageAdvancedOptions options, TextureType oldTextureType, TextureType newTextureType = TextureType.Unknown)
    {
        // Gather the files from the texture folder path into an array
        List<string> textures = new(Directory.GetFiles(texPath));

        // Filter the array so we only get the files required to convert
        textures = IOManagement.FilterFiles(textures, GetExtension(oldTextureType));

        // If no image files were found, throw an exception
        if (textures.Count < 1)
        {
            throw new FileNotFoundException($"No files with file type {Enum.GetName(oldTextureType)} were found in the directory.");
        }

        // Create an array of threads
        Thread[] threads = new Thread[(int)Math.Ceiling(textures.Count / (double)30)];
        int failedConversions = 0;
        for (int i = 0; i < threads.Length; i++)
        {
            var texturesMiniBulkList = textures.Skip(30 * i).Take(30);

            // Create a new thread and pass the conversion method as a parameter
            threads[i] = new Thread(async () =>
            {
                foreach (var texture in texturesMiniBulkList)
                {
                    try
                    {
                        ConvertTexture(texture, resultPath, options, oldTextureType, newTextureType);
                    }
                    catch (Exception)
                    {
                        // If an exception is thrown, increment the failedConversions count
                        Interlocked.Increment(ref failedConversions);
                    }
                }
            });

            // Start the thread
            threads[i].Start();
        }

        // Wait for all threads to finish
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // If there are failed conversions, throw an exception
        if (failedConversions > 0)
        {
            throw new Exception($"{failedConversions} conversions failed. Please check the files and try again.");
        }

        // Return true to indicate successful bulk conversion
        return true;
    }

    public static void ConvertTexture(string sourcePath, string resultPath, ImageAdvancedOptions options, TextureType oldTextureType, TextureType newTextureType)
    {
        if (oldTextureType == newTextureType)
        {
            return;
        }

        if (oldTextureType == TextureType.D3DTX)
        {
            switch (newTextureType)
            {
                case TextureType.DDS:
                case TextureType.PNG:
                case TextureType.JPEG:
                case TextureType.BMP:
                case TextureType.TIFF:
                case TextureType.TGA:
                case TextureType.HDR:
                    ConvertTextureFromD3DtxToOthers(sourcePath, resultPath, newTextureType, options); break;
                default:
                    throw new Exception("Invalid file type.");
            }
        }
        else if (oldTextureType == TextureType.DDS)
        {
            switch (newTextureType)
            {
                case TextureType.D3DTX:
                    ConvertTextureFromOthersToD3Dtx(sourcePath, resultPath, oldTextureType, options);
                    break;
                case TextureType.PNG:
                case TextureType.JPEG:
                case TextureType.BMP:
                case TextureType.TIFF:
                case TextureType.TGA:
                case TextureType.HDR:
                    ConvertTextureFromDdsToOthers(sourcePath, resultPath, newTextureType);
                    break;
                default:
                    throw new Exception("Invalid file type.");
            }
        }
        else if (oldTextureType == TextureType.PNG
            || oldTextureType == TextureType.JPEG
            || oldTextureType == TextureType.BMP
            || oldTextureType == TextureType.TIFF || oldTextureType == TextureType.TGA || oldTextureType == TextureType.HDR)
        {
            switch (newTextureType)
            {
                case TextureType.DDS:
                    ConvertTextureFileFromOthersToDds(sourcePath, resultPath); break;
                case TextureType.D3DTX:
                    ConvertTextureFromOthersToD3Dtx(sourcePath, resultPath, oldTextureType, options); break;
                default:
                    throw new Exception("Invalid file type.");
            }
        }
        else
        {
            throw new Exception("Invalid file type.");
        }
    }

    /// <summary>
    /// The main function for reading and converting said .d3dtx into a .dds file
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="destinationDirectory"></param>
    public static void ConvertTextureFromD3DtxToOthers(string sourceFilePath, string destinationDirectory, TextureType newTextureType, ImageAdvancedOptions options)
    {
        // Null safety validation of inputs.
        if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationDirectory))
        {
            throw new ArgumentException("Arguments cannot be null in D3DtxToDds function.");
        }

        D3DTX_Master d3dtxFile = new();
        d3dtxFile.ReadD3DTXFile(sourceFilePath, options.GameID, options.IsLegacyConsole);

        DDS_Master ddsFile = new(d3dtxFile);

        var array = ddsFile.GetData(d3dtxFile);

        Texture texture = new(array, TextureType.D3DTX);

        texture.ChangePreviewImage(options, true);
        texture.SaveTexture(Path.Combine(destinationDirectory, Path.GetFileNameWithoutExtension(sourceFilePath)), newTextureType);
        texture.Release();

        // Write the d3dtx data into a file
        d3dtxFile.WriteD3DTXJSON(Path.GetFileNameWithoutExtension(d3dtxFile.FilePath), destinationDirectory);
    }

    /// <summary>
    /// The main function for reading and converting said .dds back into a .d3dtx file
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="destinationDirectory"></param>
    public static void ConvertTextureFromOthersToD3Dtx(string sourceFilePath, string destinationDirectory, TextureType oldTextureType, ImageAdvancedOptions options)
    {
        // Null safety validation of inputs.
        if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationDirectory))
        {
            throw new ArgumentException("Arguments cannot be null in DdsToD3Dtx function.");
        }

        // Deconstruct the source file path
        string? textureFileDirectory = Path.GetDirectoryName(sourceFilePath);
        string textureFileNameOnly = Path.GetFileNameWithoutExtension(sourceFilePath);

        // Create the names of the following files
        string textureFileNameWithD3Dtx = textureFileNameOnly + Main_Shared.d3dtxExtension;
        string textureFileNameWithJSON = textureFileNameOnly + Main_Shared.jsonExtension;

        // Create the path of these files. If things go well, these files (depending on the version) should exist in the same directory at the original .dds file.
        string textureFilePathJson =
            textureFileDirectory + Path.DirectorySeparatorChar + textureFileNameWithJSON;

        // Create the final path of the d3dtx
        string textureResultPathD3Dtx =
            destinationDirectory + Path.DirectorySeparatorChar + textureFileNameWithD3Dtx;

        // If a json file exists
        if (File.Exists(textureFilePathJson))
        {
            // Create a new d3dtx object
            D3DTX_Master d3dtxMaster = new();

            // Parse the .json file as a d3dtx
            d3dtxMaster.ReadD3DTXJSON(textureFilePathJson);

            // If the d3dtx is a legacy D3DTX, force the use of the DX9 legacy flag
            DDSFlags flags = d3dtxMaster.IsLegacyD3DTX() ? DDSFlags.ForceDx9Legacy : DDSFlags.None;

            Texture texture = new(sourceFilePath, oldTextureType, flags);

            texture.ChangePreviewImage(options, true);

            // Get the image
            texture.GetDDSInformation(out D3DTXMetadata metadata, out ImageSection[] sections, flags);

            metadata.Platform = options.PlatformType;

            // Modify the d3dtx file using our dds data
            d3dtxMaster.ModifyD3DTX(metadata, sections);

            texture.Release();

            // Write our final d3dtx file to disk
            d3dtxMaster.WriteFinalD3DTX(textureResultPathD3Dtx);
        }
        // if we didn't find a json file, we're screwed!
        else
        {
            throw new FileNotFoundException("Conversion failed.\nNo .json file was found for the file.");
        }
    }

    /// <summary>
    /// The main function for reading and converting said .bmp into a .dds file
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="destinationDirectory"></param>
    public static void ConvertTextureFileFromOthersToDds(string sourceFilePath, string destinationDirectory)
    {
        if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationDirectory))
        {
            throw new ArgumentException("Arguments cannot be null in OthersToDds function.");
        }

        // Deconstruct the source file path
        string textureFileDirectory = Path.GetDirectoryName(sourceFilePath);
        string textureFileNameOnly = Path.GetFileNameWithoutExtension(sourceFilePath);

        // Create the names of the following files
        string textureFileNameWithJSON = textureFileNameOnly + Main_Shared.jsonExtension;

        // Create the path of these files. If things go well, these files (depending on the version) should exist in the same directory at the original .dds file.
        string textureFilePath_JSON = Path.Combine(textureFileDirectory, textureFileNameWithJSON);

        //TODO Update to DirectXTexNet

        // If a json file exists (for newer 5VSM and 6VSM)
        if (File.Exists(textureFilePath_JSON))
        {
            // Create a new d3dtx object
            D3DTX_Master d3dtxFile = new();

            // Parse the .json file as a d3dtx
            d3dtxFile.ReadD3DTXJSON(textureFilePath_JSON);

            // MasterOptions options = new()
            // {
            //     outputDirectory = new() { directory = destinationDirectory },
            //     outputOverwrite = new(),
            //     outputFileType = new() { fileType = TelltaleTextureTool.TexconvEnums.TexconvEnumFileTypes.dds }
            // };

            // if (d3dtxFile.HasMipMaps() == false)
            //     options.outputMipMaps = new() { remove = true };

            // switch (d3dtxFile.d3dtxMetadata.TextureType)
            // {
            //     case T3TextureType.eTxSingleChannelSDFDetailMap:
            //         options.outputFormat = new() { format = DXGIFormat.BC3_UNORM };

            //         await TexconvApp.RunTexconvAsync(sourceFilePath, options);

            //         //   DirectXTex.LoadFromWICFile(sourceFilePath).SaveToDDSFile(Path.Combine(destinationDirectory, Path.GetFileNameWithoutExtension(sourceFilePath) + Main_Shared.ddsExtension), DirectXTex.DDSFlags.ForceDX10Ext);
            //         //   DirectXTex.SaveToWICFile(DirectXTex.LoadFromDDSFile(sourceFilePath), WICCodecs.WIC_CODEC_PNG, Path.Combine(destinationDirectory, Path.GetFileNameWithoutExtension(sourceFilePath) + Main_Shared.pngExtension));
            //         break;
            //     case T3TextureType.eTxBumpmap:
            //     case T3TextureType.eTxNormalMap:

            //         options.outputFormat = new() { format = DXGIFormat.BC3_UNORM };
            //         options.outputTreatTypelessAsUNORM = new();

            //         if (fixes_generic_to_dds)
            //             options.outputSwizzle = new() { mask = "abgr" };

            //         await TexconvApp.RunTexconvAsync(sourceFilePath, options);
            //         break;
            //     case T3TextureType.eTxNormalXYMap:

            //         options.outputFormat = new() { format = DXGIFormat.BC5_UNORM };
            //         //options.outputSRGB = new() { srgbMode = TexconvEnums.TexconvEnumSrgb.srgbo };
            //         options.outputTreatTypelessAsUNORM = new();

            //         if (fixes_generic_to_dds)
            //             options.outputSwizzle = new() { mask = "rg00" };

            //         await TexconvApp.RunTexconvAsync(sourceFilePath, options);
            //         break;
            //     default:
            //         if (ImageUtilities.IsImageOpaque(sourceFilePath))
            //             options.outputFormat = new() { format = DXGIFormat.BC1_UNORM };
            //         else
            //             options.outputFormat = new() { format = DXGIFormat.BC3_UNORM };

            //         await TexconvApp.RunTexconvAsync(sourceFilePath, options);
            //         break;
            // }
        }
        // If we didn't find a json file, we're screwed!
        else
        {
            throw new FileNotFoundException("Conversion failed.\nNo .json file was found for the file.");
        }

        string outputTextureFilePath = Path.Combine(destinationDirectory, Path.GetFileNameWithoutExtension(sourceFilePath) + Main_Shared.ddsExtension);

        // Check if the output file exists, if it doesn't then the conversion failed so notify the user
        if (File.Exists(outputTextureFilePath) == false)
        {
            throw new FileNotFoundException("Conversion failed. Output file was not created.");
        }
    }

    /// <summary>
    /// The main function for reading and converting said .dds into the  more accessible supported file formats.
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="destinationDirectory"></param>
    /// <param name="newFileType"></param>
    public static void ConvertTextureFromDdsToOthers(string sourceFilePath, string destinationDirectory,
        TextureType newTextureType)
    {
        // Null safety validation of inputs.
        if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationDirectory))
        {
            throw new ArgumentException("Arguments cannot be null in DdsToOthers function.");
        }

        // Deconstruct the source file path
        string? textureFileDirectory = Path.GetDirectoryName(sourceFilePath);
        string textureFileNameOnly = Path.GetFileNameWithoutExtension(sourceFilePath);

        // Create the names of the following files
        string textureFileNameWithJson = textureFileNameOnly + Main_Shared.jsonExtension;

        // Create the path of these files. If things go well, these files (depending on the version) should exist in the same directory at the original .dds file.
        string textureFilePathJson = textureFileDirectory + Path.DirectorySeparatorChar + textureFileNameWithJson;
        string outputTextureFilePath =
            destinationDirectory + Path.DirectorySeparatorChar + textureFileNameOnly + GetExtension(newTextureType)[0];

        // If a json file exists (for newer 5VSM and 6VSM)
        if (File.Exists(textureFilePathJson))
        {
            // Create a new d3dtx object
            D3DTX_Master d3dtxFile = new();

            // Parse the .json file as a d3dtx
            d3dtxFile.ReadD3DTXJSON(textureFilePathJson);

            // Get the d3dtx texture type
            T3TextureType d3dtxTextureType = d3dtxFile.d3dtxMetadata.TextureType;

            //ConvertOptions
            if (d3dtxTextureType == T3TextureType.eTxBumpmap || d3dtxTextureType == T3TextureType.eTxNormalMap)
            {
                //  DDS_DirectXTexNet.SaveDDSToWIC(sourceFilePath, destinationDirectory, newTextureType, ImageEffect.SWIZZLE_ABGR);
            }
            else if (d3dtxTextureType == T3TextureType.eTxNormalXYMap)
            {
                //   DDS_DirectXTexNet.SaveDDSToWIC(sourceFilePath, destinationDirectory, newTextureType, ImageEffect.RESTORE_Z);
            }
            else
            {
                //     DDS_DirectXTexNet.SaveDDSToWIC(sourceFilePath, destinationDirectory, newTextureType, ImageEffect.DEFAULT);
            }
        }
        // If we didn't find a JSON file, use default conversion.
        else
        {
            // DDS_DirectXTexNet.SaveDDSToWIC(sourceFilePath, destinationDirectory, newTextureType, ImageEffect.DEFAULT);
            throw new FileNotFoundException(
                "No .json file was found for the file.\nDefaulting to classic conversion.");
        }

        // Check if the output file exists, if it doesn't then the conversion failed so notify the user
        if (!File.Exists(outputTextureFilePath))
        {
            throw new FileNotFoundException("Conversion failed. Output file was not created.");
        }
    }
}
