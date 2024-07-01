// using System;
// using System.Collections.Generic;
// using System.IO;
// using TelltaleTextureTool.TelltaleD3DTX;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;
// using Hexa.NET.DirectXTex;
// using TelltaleTextureTool.Telltale.Meta;

// namespace TelltaleTextureTool.Main
// {
//     /// <summary>
//     /// This is the master class object for a D3DTX file. Reads a file and automatically parses the data into the correct version.
//     /// </summary>
//     public class FONT_Master
//     {
//         public string? filePath { get; set; }

//         public string? fileName;

//         // Meta header versions (objects at the top of the file)
//         public MSV6? msv6;
//         public MSV5? msv5;
//         public MTRE? mtre;
//         public MBIN? mbin;

//         // D3DTX versions
//         // Legacy D3DTX versions
//         public D3DTX_LV1? d3dtxL1;
//         public D3DTX_LV2? d3dtxL2;
//         public D3DTX_LV3? d3dtxL3;
//         public D3DTX_LV4? d3dtxL4;
//         public D3DTX_LV5? d3dtxL5;

//         // Newer D3DTX versions. They are used from Poker Night 2 and later games.
//         public D3DTX_V3? d3dtx3;
//         public D3DTX_V4? d3dtx4;
//         public D3DTX_V5? d3dtx5;
//         public D3DTX_V6? d3dtx6;
//         public D3DTX_V7? d3dtx7;
//         public D3DTX_V8? d3dtx8;
//         public D3DTX_V9? d3dtx9;

//         // Generic DDS object if the D3DTX version is not found. This is used for legacy D3DTX versions only since they use DDS headers in the pixel data.
//         public ScratchImage ddsImage;

//         public byte[]? ddsData;

//         public D3DTXVersion d3dtxConversionType;

//         public struct D3DTX_JSON
//         {
//             public D3DTXVersion ConversionType;
//         }

//         /// <summary>
//         /// Reads in a D3DTX file from the disk.
//         /// </summary>
//         /// <param name="filePath"></param>
//         /// <param name="setD3DTXVersion"></param>
//         public void Read_FONT_File(string filePath, D3DTXVersion setD3DTXVersion = D3DTXVersion.DEFAULT)
//         {
//             this.filePath = filePath;
//             fileName = Path.GetFileNameWithoutExtension(filePath);

//             // Read meta version of the file
//             string metaVersion = ReadD3DTXFileMetaVersionOnly(filePath);

//             using BinaryReader reader = new(File.OpenRead(filePath));
//             // Read meta header
//             switch (metaVersion)
//             {
//                 case "6VSM":
//                   //  msv6 = new(reader);
//                     break;
//                 case "5VSM":
//                 case "4VSM":
//                   //  msv5 = new(reader);
//                     break;
//                 case "ERTM":
//                 case "MOCM":
//                   //  mtre = new(reader);
//                     break;
//                 case "NIBM":
//                 case "SEBM":
//                   //  mbin = new(reader);
//                     break;
//                 default:
//                     Console.WriteLine("ERROR! '{0}' meta stream version is not supported!", metaVersion);
//                     return;
//             }

//             // Attempt to read the d3dtx version of the file
//             int d3dtxVersion = ReadD3DTXFileD3DTXVersionOnly(filePath);
//             d3dtxConversionType = setD3DTXVersion;

//             switch (d3dtxVersion)
//             {
              
              
//                 default:
//                     Console.WriteLine("ERROR! '{0}' d3dtx version is not supported!", d3dtxVersion);
//                     break;
//             }
//         }

//         public object GetMetaObject()
//         {
//             if (msv6 != null) return msv6;
//             else if (msv5 != null) return msv5;
//             else if (mtre != null) return mtre;
//             else if (mbin != null) return mbin;
//             else return null;
//         }

//         public object GetD3DTXObject()
//         {
//             if (d3dtxL1 != null) return d3dtxL1;
//             else if (d3dtxL2 != null) return d3dtxL2;
//             else if (d3dtxL3 != null) return d3dtxL3;
//             else if (d3dtxL4 != null) return d3dtxL4;
//             else if (d3dtxL5 != null) return d3dtxL5;
//             else if (d3dtx3 != null) return d3dtx3;
//             else if (d3dtx4 != null) return d3dtx4;
//             else if (d3dtx5 != null) return d3dtx5;
//             else if (d3dtx6 != null) return d3dtx6;
//             else if (d3dtx7 != null) return d3dtx7;
//             else if (d3dtx8 != null) return d3dtx8;
//             else if (d3dtx9 != null) return d3dtx9;
//             else if (!ddsImage.IsNull) return ddsData;
//             else return null;
//         }

    

//         /// <summary>
//         /// Reads a json file and serializes it into the appropriate d3dtx version that was serialized in the json file.
//         /// </summary>
//         /// <param name="filePath"></param>
//         public void ReadD3DTXJSON(string filePath)
//         {
//             string jsonText = File.ReadAllText(filePath);

//             // parse the data into a json array
//             JArray jarray = JArray.Parse(jsonText);

//             // read the first object in the array to determine if the json file is a legacy json file or not
//             JObject firstObject = jarray[0] as JObject;

//             bool isLegacyJSON = true;

//             foreach (JProperty property in firstObject.Properties())
//             {
//                 if (property.Name.Equals("ConversionType")) isLegacyJSON = false;
//                 break;
//             }

//             int metaObjectIndex = isLegacyJSON ? 0 : 1;
//             int d3dtxObjectIndex = isLegacyJSON ? 1 : 2;

//             d3dtxConversionType = isLegacyJSON ? D3DTXVersion.DEFAULT : firstObject.ToObject<D3DTX_JSON>().ConversionType;

//             // I am creating the metaObject again instead of using the firstObject variable and i am aware of the performance hit.
//             JObject metaObject = jarray[metaObjectIndex] as JObject;
//             ConvertJSONObjectToMeta(metaObject);

//             // d3dtx object
//             JObject d3dtxObject = jarray[d3dtxObjectIndex] as JObject;

//             //deserialize the appropriate json object
//             if (d3dtxConversionType == D3DTXVersion.DEFAULT)
//             {
//                 ConvertJSONObjectToD3dtx(d3dtxObject);
//             }
//             else if (d3dtxConversionType == D3DTXVersion.LV1)
//             {
//                 d3dtxL1 = d3dtxObject.ToObject<D3DTX_LV1>();
//             }
//             else if (d3dtxConversionType == D3DTXVersion.LV2)
//             {
//                 d3dtxL2 = d3dtxObject.ToObject<D3DTX_LV2>();
//             }
//             else if (d3dtxConversionType == D3DTXVersion.LV3)
//             {
//                 d3dtxL3 = d3dtxObject.ToObject<D3DTX_LV3>();
//             }
//             else if (d3dtxConversionType == D3DTXVersion.LV4)
//             {
//                 d3dtxL4 = d3dtxObject.ToObject<D3DTX_LV4>();
//             }
//             else if (d3dtxConversionType == D3DTXVersion.LV5)
//             {
//                 d3dtxL5 = d3dtxObject.ToObject<D3DTX_LV5>();
//             }
//         }

//         public void ConvertJSONObjectToD3dtx(JObject d3dtxObject)
//         {
//             // d3dtx version value
//             int d3dtxVersion = 0;

//             // loop through each property to get the value of the variable 'mVersion' to determine what version of the d3dtx header to parse.
//             foreach (JProperty property in d3dtxObject.Properties())
//             {
//                 if (property.Name.Equals("mVersion")) d3dtxVersion = (int)property.Value;
//                 break;
//             }

//             switch (d3dtxVersion)
//             {
//                 case 3:
//                     d3dtx3 = d3dtxObject.ToObject<D3DTX_V3>();
//                     break;
//                 case 4:
//                     d3dtx4 = d3dtxObject.ToObject<D3DTX_V4>();
//                     break;
//                 case 5:
//                     d3dtx5 = d3dtxObject.ToObject<D3DTX_V5>();
//                     break;
//                 case 6:
//                     d3dtx6 = d3dtxObject.ToObject<D3DTX_V6>();
//                     break;
//                 case 7:
//                     d3dtx7 = d3dtxObject.ToObject<D3DTX_V7>();
//                     break;
//                 case 8:
//                     d3dtx8 = d3dtxObject.ToObject<D3DTX_V8>();
//                     break;
//                 case 9:
//                     d3dtx9 = d3dtxObject.ToObject<D3DTX_V9>();
//                     break;
//                 default:
//                     throw new DataMisalignedException("Invalid d3dtx version. Please convert this to the newer .json format.");
//             }
//         }

//         public void ConvertJSONObjectToMeta(JObject metaObject)
//         {
//             // parsed meta stream version from the json document
//             string metaStreamVersion = "";

//             // loop through each property to get the value of the variable 'mMetaStreamVersion' to determine what version of the meta header to parse.
//             foreach (JProperty property in metaObject.Properties())
//             {
//                 if (property.Name.Equals("mMetaStreamVersion")) metaStreamVersion = (string)property.Value;
//                 break;
//             }

//             // deserialize the appropriate json object
//             if (metaStreamVersion.Equals("6VSM")) msv6 = metaObject.ToObject<MSV6>();
//             else if (metaStreamVersion.Equals("5VSM")) msv5 = metaObject.ToObject<MSV5>();
//             else if (metaStreamVersion.Equals("ERTM")) mtre = metaObject.ToObject<MTRE>();
//             else if (metaStreamVersion.Equals("NIBM")) mbin = metaObject.ToObject<MBIN>();
//         }

//         public void WriteD3DTXJSON(string fileName, string destinationDirectory)
//         {
//             if (GetD3DTXObject() == null)
//             {
//                 return;
//             }

//             string newPath = destinationDirectory + Path.DirectorySeparatorChar + fileName + Main_Shared.jsonExtension;

//             //open a stream writer to create the text file and write to it
//             using StreamWriter file = File.CreateText(newPath);
//             //get our json serializer
//             JsonSerializer serializer = new();

//             D3DTX_JSON conversionTypeObject = new()
//             {
//                 ConversionType = d3dtxConversionType
//             };

//             List<object> jsonObjects = [conversionTypeObject, GetMetaObject(), GetD3DTXObject()];
//             //serialize the data and write it to the configuration file
//             serializer.Formatting = Formatting.Indented;
//             serializer.Serialize(file, jsonObjects);
//         }

//         public void SetMetaChunkSizes(uint mDefaultSectionChunkSize, uint mAsyncSectionChunkSize)
//         {
//             if (msv5 != null)
//             {
//                 msv5.mDefaultSectionChunkSize = mDefaultSectionChunkSize;
//                 msv5.mAsyncSectionChunkSize = mAsyncSectionChunkSize;
//             }
//             else if (msv6 != null)
//             {
//                 msv6.mDefaultSectionChunkSize = mDefaultSectionChunkSize;
//                 msv6.mAsyncSectionChunkSize = mAsyncSectionChunkSize;
//             }
//         }

//         /// <summary>
//         /// Reads a d3dtx file on the disk and returns the D3DTX version.
//         /// <para>NOTE: This only works with d3dtx meta version 5VSM and 6VSM</para>
//         /// </summary>
//         /// <param name="sourceFile"></param>
//         /// <returns></returns>
//         public static int ReadD3DTXFileD3DTXVersionOnly(string sourceFile)
//         {
//             string metaVersion = ReadD3DTXFileMetaVersionOnly(sourceFile);

//             using BinaryReader reader = new(File.OpenRead(sourceFile));

//             MSV6 meta6VSM;
//             MSV5 meta5VSM;
//             MTRE metaERTM = null;
//             MBIN metaMBIN = null;
//         //    if (metaVersion.Equals("6VSM")) meta6VSM = new(reader, false);
//          //   else if (metaVersion.Equals("5VSM")) meta5VSM = new(reader, false);
//          //   else if (metaVersion.Equals("4VSM")) meta5VSM = new(reader, false);
//           //  else if (metaVersion.Equals("ERTM")) metaERTM = new(reader, false);
//          //   else if (metaVersion.Equals("MOCM")) metaERTM = new(reader, false);
//          //   else if (metaVersion.Equals("NIBM")) metaMBIN = new(reader, false);
//          //   else if (metaVersion.Equals("SEBM")) metaMBIN = new(reader, false);

//             //read the first int (which is an mVersion d3dtx value)
//             if (metaERTM != null)
//             {
//                 return reader.ReadInt32() == 3 ? 3 : -1; //return -1 because d3dtx versions older than 3 don't have an mVersion variable (not that I know of atleast)
//             }
//             else if (metaMBIN != null) return -1;
//             else
//                 return reader.ReadInt32();
//         }
//         /// <summary>
//         /// Reads a d3dtx file on the disk and returns the meta version that is being used.
//         /// </summary>
//         /// <param name="sourceFile"></param>
//         /// <returns></returns>
//         public static string ReadD3DTXFileMetaVersionOnly(string sourceFile)
//         {
//             string metaStreamVersion = "";

//             using BinaryReader reader = new(File.OpenRead(sourceFile));

//             for (int i = 0; i < 4; i++) metaStreamVersion += reader.ReadChar();

//             return metaStreamVersion;
//         }

//     }
// }