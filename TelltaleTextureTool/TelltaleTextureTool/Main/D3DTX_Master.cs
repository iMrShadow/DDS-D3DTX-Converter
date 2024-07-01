using System;
using System.Collections.Generic;
using System.IO;
using TelltaleTextureTool.Utilities;
using TelltaleTextureTool.TelltaleEnums;
using TelltaleTextureTool.TelltaleD3DTX;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.TelltaleTypes;
using System.Linq;
using TelltaleTextureTool.DirectX.Enums;
using Pvrtc;
using Decoders;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;
using TelltaleTextureTool.Telltale.Meta;

namespace TelltaleTextureTool.Main
{
    /// <summary>
    /// This is the master class object for a D3DTX file. Reads a file and automatically parses the data into the correct version.
    /// </summary>
    public class D3DTX_Master
    {
        public string FilePath { get; set; } = string.Empty;

        public IMetaHeader metaHeaderObject;

        public MetaVersion metaVersion;

        public ID3DTX d3dtxObject;

        public D3DTXMetadata d3dtxMetadata;

        public D3DTXVersion d3dtxVersion;

        public struct D3DTX_JSON
        {
            public D3DTXVersion ConversionType;
        }

        /// <summary>
        /// Reads in a D3DTX file from the disk.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="setD3DTXVersion"></param>
        public void ReadD3DTXFile(string filePath, D3DTXVersion setD3DTXVersion = D3DTXVersion.DEFAULT)
        {
            FilePath = filePath;

            // Read meta version of the file
            string metaFourCC = ReadD3DTXFileMetaVersionOnly(filePath);

            using BinaryReader reader = new(File.OpenRead(filePath));

            // Read meta header
            switch (metaFourCC)
            {
                case "6VSM":
                    metaVersion = MetaVersion.MSV6;
                    break;
                case "5VSM":
                case "4VSM":
                    metaVersion = MetaVersion.MSV5;
                    break;
                case "ERTM":
                case "MOCM":
                    metaVersion = MetaVersion.MTRE;
                    break;
                case "NIBM":
                case "SEBM":
                    metaVersion = MetaVersion.MBIN;
                    break;
                default:
                    Console.WriteLine("ERROR! '{0}' meta stream version is not supported!", metaFourCC);
                    return;
            }

            metaHeaderObject = MetaHeaderFactory.CreateMetaHeader(metaVersion);
            metaHeaderObject.ReadFromBinary(reader);

            // Attempt to read the d3dtx version of the file
            int d3dtdMetaVersion = ReadD3DTXFileD3DTXVersionOnly(filePath);
            this.d3dtxVersion = setD3DTXVersion;

            switch (d3dtdMetaVersion)
            {
                case 1:
                case 2:
                case 3:
                    d3dtxVersion = D3DTXVersion.V3;
                    break;
                case 4:
                    d3dtxVersion = D3DTXVersion.V4;
                    break;
                case 5:
                    d3dtxVersion = D3DTXVersion.V5;
                    break;
                case 6:
                    d3dtxVersion = D3DTXVersion.V6;
                    break;
                case 7:
                    d3dtxVersion = D3DTXVersion.V7;
                    break;
                case 8:
                    d3dtxVersion = D3DTXVersion.V8;
                    break;
                case 9:
                    d3dtxVersion = D3DTXVersion.V9;
                    break;
                case -1:

                    if (setD3DTXVersion == D3DTXVersion.DEFAULT)
                    {
                        d3dtxVersion = TryToInitializeLegacyD3DTX(reader);
                        Console.WriteLine(d3dtxVersion);
                    }
                    else
                    {
                        d3dtxVersion = setD3DTXVersion;
                    }


                    // if (setD3DTXVersion == D3DTXConversionType.DEFAULT)
                    // {
                    //     try
                    //     {
                    //         if (mbin != null)
                    //         {
                    //             throw new IOException("No DDS Header are found in MBIN meta files. Please use a different conversion option.");
                    //         }

                    //         ddsData = ByteFunctions.GetBytesAfterBytePattern(DDS.MAGIC_WORD, reader.ReadBytes((int)reader.BaseStream.Length));
                    //         ddsImage = DDS_DirectXTexNet.GetDDSImage(ddsData);
                    //     }
                    //     catch (Exception e)
                    //     {
                    //         Console.WriteLine("No DDS Header found");
                    //     }
                    // }
                    // else
                    // {
                    //     Console.WriteLine("ERROR! '{0}' d3dtx version is not supported!", d3dtxVersion);
                    // }
                    // break;
                    break;
                default:
                    Console.WriteLine("ERROR! '{0}' d3dtx version is not supported!", d3dtxVersion);
                    break;
            }

            Console.WriteLine(d3dtxVersion);
            d3dtxObject = D3DTXFactory.CreateD3DTX(d3dtxVersion);
            d3dtxObject.ReadFromBinary(reader, true);
            d3dtxMetadata = d3dtxObject.GetD3DTXMetadata();
        }

        public static class D3DTXFactory
        {
            public static ID3DTX CreateD3DTX(D3DTXVersion version)
            {
                return version switch
                {
                    D3DTXVersion.V9 => new D3DTX_V9(),
                    D3DTXVersion.V8 => new D3DTX_V8(),
                    D3DTXVersion.V7 => new D3DTX_V7(),
                    D3DTXVersion.V6 => new D3DTX_V6(),
                    D3DTXVersion.V5 => new D3DTX_V5(),
                    D3DTXVersion.V4 => new D3DTX_V4(),
                    D3DTXVersion.V3 => new D3DTX_V3(),
                    D3DTXVersion.LV1 => new D3DTX_LV1(),
                    D3DTXVersion.LV2 => new D3DTX_LV2(),
                    D3DTXVersion.LV3 => new D3DTX_LV3(),
                    D3DTXVersion.LV4 => new D3DTX_LV4(),
                    D3DTXVersion.LV5 => new D3DTX_LV5(),
                    D3DTXVersion.LV6 => new D3DTX_LV6(),
                    D3DTXVersion.LV7 => new D3DTX_LV7(),
                    D3DTXVersion.LV8 => new D3DTX_LV8(),
                    D3DTXVersion.LV9 => new D3DTX_LV9(),
                    D3DTXVersion.LV10 => new D3DTX_LV10(),
                    D3DTXVersion.LV11 => new D3DTX_LV11(),
                    _ => throw new ArgumentException($"Unsupported D3DTX version: {version}")
                };
            }

            public static ID3DTX CreateD3DTX(D3DTXVersion version, JObject jsonObject)
            {
                return version switch
                {
                    D3DTXVersion.V9 => jsonObject.ToObject<D3DTX_V9>(),
                    D3DTXVersion.V8 => jsonObject.ToObject<D3DTX_V8>(),
                    D3DTXVersion.V7 => jsonObject.ToObject<D3DTX_V7>(),
                    D3DTXVersion.V6 => jsonObject.ToObject<D3DTX_V6>(),
                    D3DTXVersion.V5 => jsonObject.ToObject<D3DTX_V5>(),
                    D3DTXVersion.V4 => jsonObject.ToObject<D3DTX_V4>(),
                    D3DTXVersion.V3 => jsonObject.ToObject<D3DTX_V3>(),
                    D3DTXVersion.LV1 => jsonObject.ToObject<D3DTX_LV1>(),
                    D3DTXVersion.LV2 => jsonObject.ToObject<D3DTX_LV2>(),
                    D3DTXVersion.LV3 => jsonObject.ToObject<D3DTX_LV3>(),
                    D3DTXVersion.LV4 => jsonObject.ToObject<D3DTX_LV4>(),
                    D3DTXVersion.LV5 => jsonObject.ToObject<D3DTX_LV5>(),
                    D3DTXVersion.LV6 => jsonObject.ToObject<D3DTX_LV6>(),
                    D3DTXVersion.LV7 => jsonObject.ToObject<D3DTX_LV7>(),
                    D3DTXVersion.LV8 => jsonObject.ToObject<D3DTX_LV8>(),
                    D3DTXVersion.LV9 => jsonObject.ToObject<D3DTX_LV9>(),
                    D3DTXVersion.LV10 => jsonObject.ToObject<D3DTX_LV10>(),
                    D3DTXVersion.LV11 => jsonObject.ToObject<D3DTX_LV11>(),
                    _ => throw new ArgumentException($"Unsupported D3DTX version: {version}")
                };
            }
        }

        public static class MetaHeaderFactory
        {
            public static IMetaHeader CreateMetaHeader(MetaVersion version)
            {
                return version switch
                {
                    MetaVersion.MSV6 => new MSV6(),
                    MetaVersion.MSV5 => new MSV5(),
                    MetaVersion.MTRE => new MTRE(),
                    MetaVersion.MBIN => new MBIN(),
                    _ => throw new ArgumentException($"Unsupported Meta version: {version}")
                };
            }

            public static IMetaHeader CreateMetaHeader(MetaVersion version, JObject jsonObject)
            {
                return version switch
                {
                    MetaVersion.MSV6 => jsonObject.ToObject<MSV6>(),
                    MetaVersion.MSV5 => jsonObject.ToObject<MSV5>(),
                    MetaVersion.MTRE => jsonObject.ToObject<MTRE>(),
                    MetaVersion.MBIN => jsonObject.ToObject<MBIN>(),
                    _ => throw new ArgumentException($"Unsupported Meta version: {version}")
                };
            }
        }


        public object GetMetaObject()
        {
            return metaHeaderObject;
        }

        public D3DTXVersion TryToInitializeLegacyD3DTX(BinaryReader reader)
        {
            var startPos = reader.BaseStream.Position;

            D3DTXVersion[] legacyD3DTXVersion = [
                D3DTXVersion.LV1, D3DTXVersion.LV2,
                D3DTXVersion.LV3, D3DTXVersion.LV4,
                D3DTXVersion.LV5, D3DTXVersion.LV6,
                D3DTXVersion.LV7, D3DTXVersion.LV8,
                D3DTXVersion.LV9, D3DTXVersion.LV10,
                D3DTXVersion.LV11];

            foreach (var version in legacyD3DTXVersion)
            {
                try
                {
                    d3dtxObject = D3DTXFactory.CreateD3DTX(version);
                    d3dtxObject.ReadFromBinary(reader);
                    reader.BaseStream.Position = startPos;
                    return version;
                }
                catch (PixelDataNotFoundException e)
                {
                    throw new PixelDataNotFoundException("The texture does not have any pixel data!");
                }
                catch (Exception)
                {
                    reader.BaseStream.Position = startPos;
                }
            }

            d3dtxVersion = D3DTXVersion.Unknown;

            throw new Exception("This D3DTX version is not supported. Please report this issue to the author!");
        }

        public object GetD3DTXObject()
        {
            return d3dtxObject;
        }

        /// <summary>
        /// Writes a final .d3dtx file to disk
        /// </summary>
        /// <param name="destinationPath"></param>
        public void WriteFinalD3DTX(string destinationPath)
        {
            using BinaryWriter writer = new(File.OpenWrite(destinationPath));

            metaHeaderObject.WriteToBinary(writer);
            d3dtxObject.WriteToBinary(writer);
        }

        public string GetD3DTXDebugInfo()
        {
            string allInfo = "";

            if (metaVersion != MetaVersion.Unknown)
            {
                allInfo += metaHeaderObject.GetDebugInfo();
            }
            else allInfo += "Error! Meta data not found!" + Environment.NewLine;

            if (d3dtxObject != null) allInfo += d3dtxObject.GetDebugInfo();
            else allInfo += "Error! Data not found!";

            return allInfo;
        }

        /// <summary>
        /// Reads a json file and serializes it into the appropriate d3dtx version that was serialized in the json file.
        /// </summary>
        /// <param name="filePath"></param>
        public void ReadD3DTXJSON(string filePath)
        {
            string jsonText = File.ReadAllText(filePath);

            // parse the data into a json array
            JArray jarray = JArray.Parse(jsonText);

            // read the first object in the array to determine if the json file is a legacy json file or not
            JObject firstObject = jarray[0] as JObject;

            bool isLegacyJSON = true;

            foreach (JProperty property in firstObject.Properties())
            {
                if (property.Name.Equals("ConversionType")) isLegacyJSON = false;
                break;
            }

            int metaObjectIndex = isLegacyJSON ? 0 : 1;
            int d3dtxObjectIndex = isLegacyJSON ? 1 : 2;

            d3dtxVersion = isLegacyJSON ? D3DTXVersion.DEFAULT : firstObject.ToObject<D3DTX_JSON>().ConversionType;

            // I am creating the metaObject again instead of using the firstObject variable and i am aware of the performance hit.
            JObject? metaObject = jarray[metaObjectIndex] as JObject;
            ConvertJSONObjectToMeta(metaObject);

            // d3dtx object
            JObject? d3dtxObject1 = jarray[d3dtxObjectIndex] as JObject;

            //deserialize the appropriate json object
            if (d3dtxVersion == D3DTXVersion.DEFAULT)
            {
                ConvertJSONObjectToD3dtx(d3dtxObject1);
            }
            else
                d3dtxObject = D3DTXFactory.CreateD3DTX(d3dtxVersion, d3dtxObject1);

            d3dtxMetadata = d3dtxObject.GetD3DTXMetadata();
        }

        public void ConvertJSONObjectToD3dtx(JObject jObject)
        {
            // d3dtx version value
            int d3dtxVersion = 0;

            // loop through each property to get the value of the variable 'mVersion' to determine what version of the d3dtx header to parse.
            foreach (JProperty property in jObject.Properties())
            {
                if (property.Name.Equals("mVersion")) d3dtxVersion = (int)property.Value;
                break;
            }

            d3dtxObject = D3DTXFactory.CreateD3DTX((D3DTXVersion)d3dtxVersion, jObject);
        }

        public void ConvertJSONObjectToMeta(JObject metaObject)
        {
            // parsed meta stream version from the json document
            string metaStreamVersion = "";

            // loop through each property to get the value of the variable 'mMetaStreamVersion' to determine what version of the meta header to parse.
            foreach (JProperty property in metaObject.Properties())
            {
                if (property.Name.Equals("mMetaStreamVersion")) metaStreamVersion = (string)property.Value;
                break;
            }

            metaVersion = metaStreamVersion switch
            {
                "6VSM" => MetaVersion.MSV6,
                "5VSM" or "4VSM" => MetaVersion.MSV5,
                "ERTM" or "MOCM" => MetaVersion.MTRE,
                "NIBM" or "SEBM" => MetaVersion.MBIN,
                _ => throw new Exception("This meta version is not supported!"),
            };

            metaHeaderObject = MetaHeaderFactory.CreateMetaHeader(metaVersion, metaObject);
        }

        public void WriteD3DTXJSON(string fileName, string destinationDirectory)
        {
            if (GetD3DTXObject() == null)
            {
                return;
            }

            string newPath = destinationDirectory + Path.DirectorySeparatorChar + fileName + Main_Shared.jsonExtension;

            //open a stream writer to create the text file and write to it
            using StreamWriter file = File.CreateText(newPath);
            //get our json serializer
            JsonSerializer serializer = new();

            D3DTX_JSON conversionTypeObject = new()
            {
                ConversionType = d3dtxVersion
            };

            List<object> jsonObjects = [conversionTypeObject, GetMetaObject(), GetD3DTXObject()];
            //serialize the data and write it to the configuration file
            serializer.Formatting = Formatting.Indented;
            serializer.Serialize(file, jsonObjects);
        }

        public void ModifyD3DTX(D3DTXMetadata metadata, ImageSection[] sections)
        {
            if (IsLegacyD3DTX())
            {
                //Generate a DDS header
                // var dataArray = DDS_DirectXTexNet.GetDDSByteArray(image, DDSFlags.ForceDx9Legacy);
                // d3dtxObject.ModifyD3DTX(metadata, dataArray);
            }
            d3dtxMetadata = metadata;

            // var meta = DDS_DirectXTexNet.GetDDSMetaData(image);
            // if (d3dtxL1 != null)
            // {
            //     
            // }
            // else if (d3dtxL2 != null)
            // {
            //     var dataArray = DDS_DirectXTexNet.GetDDSByteArray(image, DDSFlags.ForceDx9Legacy);
            //     d3dtxL2.ModifyD3DTX(meta, dataArray);
            // }

            // If they are not legacy version, stable sort the image sections by size. (Smallest to Largest)
            // var sections = DDS_DirectXTexNet.GetDDSImageSections(sections);
            IEnumerable<ImageSection> newSections = sections;
            newSections = sections.OrderBy(section => section.Pixels.Length);

            d3dtxObject.ModifyD3DTX(metadata, newSections.ToArray()); //ISSUE HERE WITH DXT5 AND MIP MAPS WITH UPSCALED TEXTURES
            metaHeaderObject.SetMetaSectionChunkSizes(d3dtxObject.GetHeaderByteSize(), 0, ByteFunctions.GetByteArrayListElementsCount(d3dtxObject.GetPixelData()));
        }

        /// <summary>
        /// Reads a d3dtx file on the disk and returns the meta version that is being used.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        public static string ReadD3DTXFileMetaVersionOnly(string sourceFile)
        {
            string metaStreamVersion = "";

            using BinaryReader reader = new(File.OpenRead(sourceFile));

            for (int i = 0; i < 4; i++) metaStreamVersion += reader.ReadChar();

            return metaStreamVersion;
        }

        /// <summary>
        /// Reads a d3dtx file on the disk and returns the D3DTX version.
        /// <para>NOTE: This only works with d3dtx meta version 5VSM and 6VSM</para>
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        public int ReadD3DTXFileD3DTXVersionOnly(string sourceFile)
        {
            string metaFourCC = ReadD3DTXFileMetaVersionOnly(sourceFile);

            using BinaryReader reader = new(File.OpenRead(sourceFile));

            MetaVersion metaVersion = MetaVersion.Unknown;

            metaVersion = metaFourCC switch
            {
                "6VSM" => MetaVersion.MSV6,
                "5VSM" or "4VSM" => MetaVersion.MSV5,
                "ERTM" or "MOCM" => MetaVersion.MTRE,
                "NIBM" or "SEBM" => MetaVersion.MBIN,
                _ => throw new Exception("This meta version is not supported!"),
            };
            IMetaHeader metaHeaderObject = MetaHeaderFactory.CreateMetaHeader(metaVersion);
            metaHeaderObject.ReadFromBinary(reader);

            //read the first int (which is an mVersion d3dtx value)
            if (metaVersion == MetaVersion.MTRE)
                return reader.ReadInt32() == 3 ? 3 : -1; // Return -1 because D3DTX versions older than 3 don't have an mVersion variable.
            else if (metaVersion == MetaVersion.MBIN)
                return -1;
            else
                return reader.ReadInt32();
        }

        public bool IsLegacyD3DTX()
        {
            return (int)d3dtxVersion < 3;
        }

        public bool IsMbin()
        {
            return metaVersion == MetaVersion.MBIN;
        }

        public string GetStringFormat()
        {
            if (d3dtxMetadata.D3DFormat == D3DFormat.UNKNOWN)
                return Enum.GetName(d3dtxMetadata.Format).Remove(0, 9);
            else
            {
                return d3dtxMetadata.D3DFormat.ToString();
            }
        }

        public RegionStreamHeader[] GetRegionStreamHeaders()
        {
            return d3dtxMetadata.RegionHeaders;
        }

        public string GetHasAlpha()
        {
            return d3dtxMetadata.AlphaMode > 0 ? "True" : ((int)d3dtxMetadata.AlphaMode == -1 ? "Unknown" : "False");
        }

        /// <summary>
        /// Gets a string version of the channel count of .d3dtx surface format. 
        /// </summary>
        /// <returns></returns>
        public string GetChannelCount()
        {
            return DDS_DirectXTexNet.GetChannelCount(DDS_HELPER.GetDXGIFromTelltaleSurfaceFormat(d3dtxMetadata.Format)).ToString();
        }

        public int GetRegionCount()
        {
            return d3dtxMetadata.RegionHeaders.Length;
        }

        public D3DTXMetadata GetMetadata()
        {
            return d3dtxMetadata;
        }

        public bool HasMipMaps()
        {
            return d3dtxMetadata.MipLevels > 1;
        }

        public List<byte[]> GetLegacyPixelData()
        {
            return d3dtxObject.GetPixelData();
        }

        public List<byte[]> GetPixelData()
        {
            return d3dtxObject.GetPixelData();
        }

        public byte[] GetPixelDataByFaceIndex(int faceIndex, T3SurfaceFormat surfaceFormat, int width, int height, T3PlatformType platformType)
        {
            List<byte[]> newPixelData = [];

            RegionStreamHeader[] regionHeaders = GetRegionStreamHeaders();

            int divideBy = 1;

            for (int i = 0; i < regionHeaders.Length; i++)
            {
                if (regionHeaders[i].mFaceIndex == faceIndex)
                {
                    if (regionHeaders[i].mMipCount > 1)
                    {
                        newPixelData.Add(GetPixelData()[i]); continue;
                    }

                    GetPixelData()[i] = DecodePixelDataByPlatform(GetPixelData()[i], surfaceFormat, width / divideBy, height / divideBy, platformType);
                    GetPixelData()[i] = DecodePixelDataByFormat(GetPixelData()[i], surfaceFormat, width / divideBy, height / divideBy, platformType);

                    divideBy *= 2;

                    newPixelData.Add(GetPixelData()[i]);
                }
            }

            // Reverse the elements in the list to get the correct order.
            newPixelData.Reverse();

            return newPixelData.SelectMany(b => b).ToArray();
        }

        public byte[] GetPixelDataByMipmapIndex(int mipmapIndex, T3SurfaceFormat surfaceFormat, int width, int height, T3PlatformType platformType)
        {
            List<byte[]> newPixelData = [];

            RegionStreamHeader[] regionHeaders = GetRegionStreamHeaders();

            for (int i = 0; i < regionHeaders.Length; i++)
            {
                if (regionHeaders[i].mMipIndex == mipmapIndex)
                {
                    if (regionHeaders[i].mMipCount > 1)
                    {
                        newPixelData.Add(GetPixelData()[i]); continue;
                    }

                    GetPixelData()[i] = DecodePixelDataByPlatform(GetPixelData()[i], surfaceFormat, width, height, platformType);
                    GetPixelData()[i] = DecodePixelDataByFormat(GetPixelData()[i], surfaceFormat, width, height, platformType);

                    newPixelData.Add(GetPixelData()[i]);
                }
            }

            return newPixelData.SelectMany(b => b).ToArray();
        }

        public byte[] DecodePixelDataByPlatform(byte[] pixelData, T3SurfaceFormat surfaceFormat, int width, int height, T3PlatformType platformType)
        {
            if (platformType == T3PlatformType.ePlatform_PS4)
            {
                Console.WriteLine("Decoding PS4 texture data...");

                pixelData = PSTextureDecoder.DecodePS4(pixelData, DDS_HELPER.GetDXGIFromTelltaleSurfaceFormat(surfaceFormat), width, height);
            }

            else if (platformType == T3PlatformType.ePlatform_PS3 || platformType == T3PlatformType.ePlatform_WiiU)
            {
                Console.WriteLine("Decoding PS3 texture data...");

                pixelData = PSTextureDecoder.DecodePS3(pixelData, DDS_HELPER.GetDXGIFromTelltaleSurfaceFormat(surfaceFormat), width, height);
            }

            else if (platformType == T3PlatformType.ePlatform_Xbox || platformType == T3PlatformType.ePlatform_XBOne)
            {
                Console.WriteLine("Decoding Xbox texture data...");

                pixelData = XboxTextureDecoder.Decode(pixelData, width, height, surfaceFormat, true);
            }

            return pixelData;
        }

        public byte[] DecodePixelDataByFormat(byte[] pixelData, T3SurfaceFormat surfaceFormat, int width, int height, T3PlatformType platformType)
        {
            if (surfaceFormat == T3SurfaceFormat.eSurface_PVRTC4 || surfaceFormat == T3SurfaceFormat.eSurface_PVRTC4a)
            {
                PvrtcDecoder pvrtcDecoder = new PvrtcDecoder();
                pixelData = pvrtcDecoder.DecompressPVRTC(pixelData, width, height, false);
            }
            else if (surfaceFormat == T3SurfaceFormat.eSurface_PVRTC2 || surfaceFormat == T3SurfaceFormat.eSurface_PVRTC2a)
            {
                PvrtcDecoder pvrtcDecoder = new PvrtcDecoder();
                pixelData = pvrtcDecoder.DecompressPVRTC(pixelData, width, height, true);
            }
            else if (surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGB)
            {
                AtcDecoder atcDecoder = new AtcDecoder();
                pixelData = atcDecoder.DecompressAtcRgb4(pixelData, width, height);
            }
            else if (surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGBA || surfaceFormat == T3SurfaceFormat.eSurface_ATC_RGB1A)
            {
                AtcDecoder atcDecoder = new AtcDecoder();
                pixelData = atcDecoder.DecompressAtcRgba8(pixelData, width, height);
            }

            return pixelData;
        }

        public bool IsTextureCompressed()
        {
            return IsTextureCompressed(d3dtxMetadata.Format);
        }

        public byte[] GetPixelDataByFirstMipmapIndex(T3SurfaceFormat surfaceFormat, int width, int height, T3PlatformType platformType)
        {
            int index = 0;

            if (GetRegionCount() == 1)
            {
                index = GetRegionStreamHeaders()[0].mMipIndex;
            }

            return GetPixelDataByMipmapIndex(index, surfaceFormat, width, height, platformType);
        }

        public static bool IsTextureCompressed(T3SurfaceFormat format)
        {
            return format switch
            {
                T3SurfaceFormat.eSurface_BC1 => true,
                T3SurfaceFormat.eSurface_BC2 => true,
                T3SurfaceFormat.eSurface_BC3 => true,
                T3SurfaceFormat.eSurface_BC4 => true,
                T3SurfaceFormat.eSurface_BC5 => true,
                T3SurfaceFormat.eSurface_BC6 => true,
                T3SurfaceFormat.eSurface_BC7 => true,
                T3SurfaceFormat.eSurface_CTX1 => true,
                T3SurfaceFormat.eSurface_ATC_RGB => true,
                T3SurfaceFormat.eSurface_ATC_RGBA => true,
                T3SurfaceFormat.eSurface_ATC_RGB1A => true,
                T3SurfaceFormat.eSurface_ETC1_RGB => true,
                T3SurfaceFormat.eSurface_ETC2_RGB => true,
                T3SurfaceFormat.eSurface_ETC2_RGBA => true,
                T3SurfaceFormat.eSurface_ETC2_RGB1A => true,
                T3SurfaceFormat.eSurface_ETC2_R => true,
                T3SurfaceFormat.eSurface_ETC2_RG => true,
                T3SurfaceFormat.eSurface_ATSC_RGBA_4x4 => true,
                T3SurfaceFormat.eSurface_PVRTC2 => true,
                T3SurfaceFormat.eSurface_PVRTC4 => true,
                T3SurfaceFormat.eSurface_PVRTC2a => true,
                T3SurfaceFormat.eSurface_PVRTC4a => true,
                _ => false,
            };
        }

        public static bool IsFormatIncompatibleWithDDS(T3SurfaceFormat format)
        {
            return format switch
            {
                T3SurfaceFormat.eSurface_ATC_RGB => true,
                T3SurfaceFormat.eSurface_ATC_RGBA => true,
                T3SurfaceFormat.eSurface_ATC_RGB1A => true,
                T3SurfaceFormat.eSurface_ETC1_RGB => true,
                T3SurfaceFormat.eSurface_ETC2_RGB => true,
                T3SurfaceFormat.eSurface_ETC2_RGBA => true,
                T3SurfaceFormat.eSurface_ETC2_RGB1A => true,
                T3SurfaceFormat.eSurface_ETC2_R => true,
                T3SurfaceFormat.eSurface_ETC2_RG => true,
                T3SurfaceFormat.eSurface_ATSC_RGBA_4x4 => true,
                T3SurfaceFormat.eSurface_PVRTC2 => true,
                T3SurfaceFormat.eSurface_PVRTC4 => true,
                T3SurfaceFormat.eSurface_PVRTC2a => true,
                T3SurfaceFormat.eSurface_PVRTC4a => true,
                _ => false,
            };
        }

        public static bool IsPlatformIncompatibleWithDDS(T3PlatformType type)
        {
            return type switch
            {
                T3PlatformType.ePlatform_PS3 => true,
                T3PlatformType.ePlatform_PS4 => true,
                T3PlatformType.ePlatform_WiiU => true,
                T3PlatformType.ePlatform_Wii => true,
                T3PlatformType.ePlatform_Xbox => true,
                T3PlatformType.ePlatform_XBOne => true,
                T3PlatformType.ePlatform_iPhone => true,
                T3PlatformType.ePlatform_Android => true,
                _ => false,
            };
        }

        public string GetDDSHeaderInfo()
        {
            string info = "";
            foreach (var region in GetLegacyPixelData())
            {
                // if (ByteFunctions.Ge)
                // {
                //     info += DDS_DirectXTexNet.GetDDSDebugInfo(DirectXTex.GetMetadata(region));
                // }
            }

            return info;
        }
    }
}