using System;
using System.Collections.Generic;
using System.IO;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.ImageProcessing;
using TelltaleTextureTool.Main;
using TelltaleTextureTool.Texconv;
using TelltaleTextureTool.TexconvOptions;
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
            _ => throw new InvalidEnumArgumentException("Invalid texture type."),
        };
    }


    /// <param name="texPath"></param>
    /// <param name="resultPath"></param>
    /// <param name="fixesGenericToDds"></param>
    /// <summary>
    /// Converts multiple texture files from one format to another.
    /// </summary>
    /// <param name="texPath">The path to the folder containing the texture files.</param>
    /// <param name="resultPath">The path to save the converted texture files.</param>
    /// <param name="oldTextureType">The file type of the original texture files.</param>
    /// <param name="newTextureType">The desired file type for the converted texture files. If not specified, the default is an empty string.</param>
    /// <param name="conversionType">The version of the D3DTX file that the converter should use. If not specified, the default is D3DTXVersion.DEFAULT.</param>
    /// <returns>True if the bulk conversion is successful; otherwise, false.</returns>
    /// <exception cref="FileNotFoundException">Thrown when no files with the specified file type were found in the directory.</exception>
    /// <exception cref="Exception">Thrown when an invalid file type is provided or when one or more conversions fail.</exception>
    public static bool ConvertBulk(string texPath, string resultPath, TextureType oldTextureType, TextureType newTextureType = TextureType.Unknown, D3DTXVersion conversionType = D3DTXVersion.DEFAULT)
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
                        await ConvertTexture(texture, resultPath, oldTextureType, newTextureType, conversionType);
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

    public static async Task ConvertTexture(string sourcePath, string resultPath, TextureType oldTextureType, TextureType newTextureType, D3DTXVersion conversionType = D3DTXVersion.DEFAULT)
    {
        if (oldTextureType == TextureType.D3DTX)
        {
            switch (newTextureType)
            {
                case TextureType.DDS:
                    ConvertTextureFromD3DtxToDds(sourcePath, resultPath, conversionType);
                    break;
                case TextureType.PNG:
                case TextureType.KTX:
                    throw new NotImplementedException("Conversion to KTX and KTX2 is not supported.");
                case TextureType.KTX2:
                    ConvertTextureFromD3DtxToKtx2(sourcePath, resultPath, conversionType);
                    break;
                default:
                    throw new Exception("Invalid file type.");
            }
        }
        else if (oldTextureType == TextureType.DDS)
        {
            switch (newTextureType)
            {
                case TextureType.D3DTX:
                    ConvertTextureFromD3DtxToDds(sourcePath, resultPath, conversionType);
                    break;
                case TextureType.PNG:
                case TextureType.JPEG:
                case TextureType.BMP:
                case TextureType.TIFF:
                case TextureType.TGA:
                    await ConvertTextureFromDdsToOthersAsync(sourcePath, resultPath, newTextureType, true);
                    break;
                default:
                    throw new Exception("Invalid file type.");
            }
        }
        else if (oldTextureType == TextureType.KTX)
        {
            switch (newTextureType)
            {
                case TextureType.D3DTX:
                    ConvertTextureFromD3DtxToDds(sourcePath, resultPath, conversionType);
                    break;
                default:
                    throw new Exception("Invalid file type.");
            }
        }
        else if (oldTextureType == TextureType.KTX2)
        {
            switch (newTextureType)
            {
                case TextureType.D3DTX:
                    ConvertTextureFromD3DtxToDds(sourcePath, resultPath, conversionType);
                    break;
                default:
                    throw new Exception("Invalid file type.");
            }
        }
        else if (oldTextureType == TextureType.PNG
            || oldTextureType == TextureType.JPEG
            || oldTextureType == TextureType.BMP
            || oldTextureType == TextureType.TIFF || oldTextureType == TextureType.TGA)
        {
            switch (newTextureType)
            {
                case TextureType.DDS:
                    await ConvertTextureFileFromOthersToDdsAsync(sourcePath, resultPath, true); break;
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
    public static void ConvertTextureFromD3DtxToKtx2(string? sourceFilePath, string destinationDirectory, D3DTXVersion conversionType = D3DTXVersion.DEFAULT)
    {
        // Null safety validation of inputs.
        if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationDirectory))
        {
            throw new ArgumentException("Arguments cannot be null in D3DtxToDds function.");
        }

        D3DTX_Master d3dtxFile = new();
        d3dtxFile.ReadD3DTXFile(sourceFilePath, conversionType);

        KTX2_Master ktx2File = new();
        //   ktx2File.InitializeTextureHeader(d3dtxFile);

        // Write the dds file to disk
        // ktx2File.WriteD3DTXAsKTX2(d3dtxFile, destinationDirectory);

        // Write the d3dtx data into a file
        d3dtxFile.WriteD3DTXJSON(Path.GetFileNameWithoutExtension(d3dtxFile.FilePath), destinationDirectory);
    }


    /// <summary>
    /// The main function for reading and converting said .d3dtx into a .dds file
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="destinationDirectory"></param>
    public static void ConvertTextureFromD3DtxToDds(string? sourceFilePath, string destinationDirectory, D3DTXVersion conversionType = D3DTXVersion.DEFAULT)
    {
        // Null safety validation of inputs.
        if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationDirectory))
        {
            throw new ArgumentException("Arguments cannot be null in D3DtxToDds function.");
        }

        D3DTX_Master d3dtxFile = new();
        d3dtxFile.ReadD3DTXFile(sourceFilePath, conversionType);

        DDS_Master ddsFile = new(d3dtxFile);

        // Write the dds file to disk
        ddsFile.WriteD3DTXAsDDS(d3dtxFile, destinationDirectory);

        // Write the d3dtx data into a file
        d3dtxFile.WriteD3DTXJSON(Path.GetFileNameWithoutExtension(d3dtxFile.FilePath), destinationDirectory);
    }

    /// <summary>
    /// The main function for reading and converting said .dds back into a .d3dtx file
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="destinationDirectory"></param>
    public static void ConvertTextureFromDdsToD3Dtx(string? sourceFilePath, string destinationDirectory)
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

            // Get the image
            DDS_DirectXTexNet.GetDDSInformation(sourceFilePath, out D3DTXMetadata metadata, out ImageSection[] sections, flags);

            // Modify the d3dtx file using our dds data
            d3dtxMaster.ModifyD3DTX(metadata, sections); //ISSUE HERE WITH DXT5 AND MIP MAPS WITH UPSCALED TEXTURES // Edit: This may not be an issue anymore

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
    public static async Task ConvertTextureFileFromOthersToDdsAsync(string sourceFilePath, string destinationDirectory,
        bool fixes_generic_to_dds)
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

            MasterOptions options = new()
            {
                outputDirectory = new() { directory = destinationDirectory },
                outputOverwrite = new(),
                outputFileType = new() { fileType = TelltaleTextureTool.TexconvEnums.TexconvEnumFileTypes.dds }
            };

            if (d3dtxFile.HasMipMaps() == false)
                options.outputMipMaps = new() { remove = true };

            switch (d3dtxFile.d3dtxMetadata.TextureType)
            {
                case T3TextureType.eTxSingleChannelSDFDetailMap:
                    options.outputFormat = new() { format = DXGIFormat.BC3_UNORM };

                    await TexconvApp.RunTexconvAsync(sourceFilePath, options);

                    //   DirectXTex.LoadFromWICFile(sourceFilePath).SaveToDDSFile(Path.Combine(destinationDirectory, Path.GetFileNameWithoutExtension(sourceFilePath) + Main_Shared.ddsExtension), DirectXTex.DDSFlags.ForceDX10Ext);
                    //   DirectXTex.SaveToWICFile(DirectXTex.LoadFromDDSFile(sourceFilePath), WICCodecs.WIC_CODEC_PNG, Path.Combine(destinationDirectory, Path.GetFileNameWithoutExtension(sourceFilePath) + Main_Shared.pngExtension));
                    break;
                case T3TextureType.eTxBumpmap:
                case T3TextureType.eTxNormalMap:

                    options.outputFormat = new() { format = DXGIFormat.BC3_UNORM };
                    options.outputTreatTypelessAsUNORM = new();

                    if (fixes_generic_to_dds)
                        options.outputSwizzle = new() { mask = "abgr" };

                    await TexconvApp.RunTexconvAsync(sourceFilePath, options);
                    break;
                case T3TextureType.eTxNormalXYMap:

                    options.outputFormat = new() { format = DXGIFormat.BC5_UNORM };
                    //options.outputSRGB = new() { srgbMode = TexconvEnums.TexconvEnumSrgb.srgbo };
                    options.outputTreatTypelessAsUNORM = new();

                    if (fixes_generic_to_dds)
                        options.outputSwizzle = new() { mask = "rg00" };

                    await TexconvApp.RunTexconvAsync(sourceFilePath, options);
                    break;
                default:
                    if (ImageUtilities.IsImageOpaque(sourceFilePath))
                        options.outputFormat = new() { format = DXGIFormat.BC1_UNORM };
                    else
                        options.outputFormat = new() { format = DXGIFormat.BC3_UNORM };

                    await TexconvApp.RunTexconvAsync(sourceFilePath, options);
                    break;
            }
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
    /// <param name="fixesDdsToGeneric"></param>
    public static async Task ConvertTextureFromDdsToOthersAsync(string? sourceFilePath, string destinationDirectory,
        TextureType newTextureType, bool fixesDdsToGeneric)
    {
        // Null safety validation of inputs.
        if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationDirectory))
        {
            throw new ArgumentException("Arguments cannot be null in DdsToOthers function.");
        }

        // Deconstruct the source file path
        string? textureFileDirectory = Path.GetDirectoryName(sourceFilePath);
        string textureFileNameOnly = Path.GetFileNameWithoutExtension(sourceFilePath);

        TexconvEnums.TexconvEnumFileTypes newFileType =
            GetEnumFileType(newTextureType);

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

            //TODO Update to DirectXTexNet

            // Get the d3dtx texture type
            T3TextureType d3dtxTextureType = d3dtxFile.d3dtxMetadata.TextureType;

            MasterOptions options = new()
            {
                outputDirectory = new() { directory = destinationDirectory },
                outputOverwrite = new(),
                outputFileType = new() { fileType = newFileType }
            };

            //ConvertOptions

            if (d3dtxTextureType == T3TextureType.eTxBumpmap ||
                d3dtxTextureType == T3TextureType.eTxNormalMap)
            {


              //  await TexconvApp.RunTexconvAsync(sourceFilePath, options);
                DDS_DirectXTexNet.SaveNormalMapToWIC(sourceFilePath, destinationDirectory, newTextureType, DDS_DirectXTexNet.DDSConversionMode.SWIZZLE_ABGR);
            }
            else if (d3dtxTextureType == T3TextureType.eTxNormalXYMap)
            {
                // options.outputReconstructZ = new();

                DDS_DirectXTexNet.SaveNormalMapToWIC(sourceFilePath, destinationDirectory, newTextureType, DDS_DirectXTexNet.DDSConversionMode.RESTORE_Z);

                // if (fixesDdsToGeneric)
                //     NormalMapProcessing.FromDDS_NormalMapReconstructZ(sourceFilePath, outputTextureFilePath);
                // else
                //     NormalMapConvert.ConvertNormalMapToOthers(sourceFilePath, GetExtension(newTextureType)[0]);
            }
            else
            {
                await TexconvApp.RunTexconvAsync(sourceFilePath, options);
            }
        }
        // If we didn't find a json file, we're screwed!
        else
        {
            MasterOptions options = new()
            {
                outputDirectory = new() { directory = destinationDirectory },
                outputOverwrite = new(),
                outputFileType = new() { fileType = newFileType }
            };
            await TexconvApp.RunTexconvAsync(sourceFilePath, options);
            throw new FileNotFoundException(
                "No .json file was found for the file.\nDefaulting to classic conversion.");
        }

        // Check if the output file exists, if it doesn't then the conversion failed so notify the user
        if (!File.Exists(outputTextureFilePath))
        {
            throw new FileNotFoundException("Conversion failed. Output file was not created.");
        }
    }


    /// <summary>
    /// Helper function for the converter. Gets the Texconv enum from the provided extension.
    /// </summary>
    private static TexconvEnums.TexconvEnumFileTypes GetEnumFileType(TextureType extension)
    {
        if (extension == TextureType.Unknown)
        {
            throw new ArgumentNullException("File type cannot be null.");
        }

        return extension switch
        {
            TextureType.DDS => TexconvEnums.TexconvEnumFileTypes.dds,
            TextureType.BMP => TexconvEnums.TexconvEnumFileTypes.bmp,
            TextureType.PNG => TexconvEnums.TexconvEnumFileTypes.png,
            TextureType.JPEG => TexconvEnums.TexconvEnumFileTypes.jpeg,
            TextureType.TIFF => TexconvEnums.TexconvEnumFileTypes.tiff,
            TextureType.TGA => TexconvEnums.TexconvEnumFileTypes.tga,
            _ => throw new Exception("File type " + extension + " is not supported."),
        };
    }
}
