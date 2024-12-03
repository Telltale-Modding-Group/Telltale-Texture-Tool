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
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;
using TelltaleTextureTool.Telltale.Meta;
using DrSwizzler;
using System.Text;

namespace TelltaleTextureTool.Main
{
    /// <summary>
    /// This is the master class object for a D3DTX file. Reads a file and automatically parses the data into the correct version.
    /// </summary>
    public class D3DTX_Master
    {
        public string FilePath { get; set; } = string.Empty;

        public IMetaHeader? metaHeaderObject;

        public MetaVersion metaVersion;

        public ID3DTX? d3dtxObject;

        public D3DTXMetadata? d3dtxMetadata;

        public TelltaleToolGame Game { get; set; } = TelltaleToolGame.DEFAULT;
        public T3PlatformType Platform { get; set; } = T3PlatformType.ePlatform_None;

        public struct D3DTX_JSON
        {
            public string GameID;
            public T3PlatformType PlatformType;
            public int ConversionType;
        }

        /// <summary>
        /// Reads in a D3DTX file from the disk.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="setD3DTXVersion"></param>
        public void ReadD3DTXFile(string filePath, TelltaleToolGame game = TelltaleToolGame.DEFAULT, bool isLegacyConsole = false)
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
            metaHeaderObject.ReadFromBinary(reader, TelltaleToolGame.DEFAULT, T3PlatformType.ePlatform_None, true);

            // Attempt to read the d3dtx version of the file
            int d3dtdMetaVersion = ReadD3DTXFileD3DTXVersionOnly(filePath);
            Game = game;

            switch (d3dtdMetaVersion)
            {
                case 1:
                case 2:
                case 3:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = new D3DTX_V3();
                    break;
                case 4:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = new D3DTX_V4();
                    break;
                case 5:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = new D3DTX_V5();
                    break;
                case 6:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = new D3DTX_V6();
                    break;
                case 7:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = new D3DTX_V7();
                    break;
                case 8:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = new D3DTX_V7();
                    break;
                case 9:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = new D3DTX_V9();
                    break;
                case -1:

                    if (game == TelltaleToolGame.DEFAULT)
                    {
                        Game = TryToInitializeLegacyD3DTX(reader);

                        if (Game == TelltaleToolGame.UNKNOWN)
                        {
                            throw new Exception("This D3DTX version is not supported. Please report this issue to the author!");
                        }
                    }
                    else
                    {
                        Game = game;
                        Platform = isLegacyConsole ? T3PlatformType.ePlatform_PS3 : T3PlatformType.ePlatform_None;
                    }

                    d3dtxObject = new D3DTX_Legacy();

                    break;
                default:
                    Console.WriteLine("ERROR! '{0}' d3dtx version is not supported!", Game);
                    break;
            }

            d3dtxObject.ReadFromBinary(reader, Game, Platform, true);

            d3dtxMetadata = d3dtxObject.GetD3DTXMetadata();
        }

        public void CreateD3DTX(TelltaleToolGame game, JObject jsonObject)
        {
            if (game == TelltaleToolGame.UNKNOWN)
            {
                throw new Exception("This D3DTX version is not supported. Please report this issue to the author!");
            }

            if (game >= TelltaleToolGame.POKER_NIGHT_2 || game == TelltaleToolGame.DEFAULT)
            {
                ConvertJSONObjectToD3dtx(jsonObject);
                return;
            }

            d3dtxObject = jsonObject.ToObject<D3DTX_Legacy>();
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

        public TelltaleToolGame TryToInitializeLegacyD3DTX(BinaryReader reader)
        {
            var startPos = reader.BaseStream.Position;

            TelltaleToolGame[] allGames = Enum.GetValues(typeof(TelltaleToolGame)).Cast<TelltaleToolGame>().ToArray();

            foreach (var game in allGames)
            {
                try
                {
                    d3dtxObject = new D3DTX_Legacy();
                    d3dtxObject.ReadFromBinary(reader, game, T3PlatformType.ePlatform_None, true);
                    reader.BaseStream.Position = startPos;
                    return game;
                }
                catch (PixelDataNotFoundException)
                {
                    throw new PixelDataNotFoundException("The texture does not have any pixel data!");
                }
                catch (Exception)
                {
                    reader.BaseStream.Position = startPos;
                }
            }

            foreach (var game in allGames)
            {
                try
                {
                    d3dtxObject = new D3DTX_Legacy();
                    d3dtxObject.ReadFromBinary(reader, game, T3PlatformType.ePlatform_PS3, true);
                    reader.BaseStream.Position = startPos;
                    Platform = T3PlatformType.ePlatform_PS3;
                    return game;
                }
                catch (PixelDataNotFoundException)
                {
                    throw new PixelDataNotFoundException("The texture does not have any pixel data!");
                }
                catch (Exception)
                {
                    reader.BaseStream.Position = startPos;
                }
            }

            Game = TelltaleToolGame.UNKNOWN;

            throw new Exception("This D3DTX version is not supported. Please report this issue to the author!");
        }

        /// <summary>
        /// Writes a final .d3dtx file to disk
        /// </summary>
        /// <param name="destinationPath"></param>
        public void WriteFinalD3DTX(string destinationPath)
        {
            using BinaryWriter writer = new(File.Create(destinationPath));

            metaHeaderObject.WriteToBinary(writer, Game, Platform, true);
            d3dtxObject.WriteToBinary(writer, Game, Platform, true);
        }

        public string GetD3DTXDebugInfo()
        {
            StringBuilder debugInformation = new();

            if (metaVersion != MetaVersion.Unknown)
            {
                debugInformation.Append(metaHeaderObject.GetDebugInfo());
            }
            else debugInformation.AppendLine("Error! Meta data not found!");

            if (d3dtxObject != null) debugInformation.Append(d3dtxObject.GetDebugInfo(Game));
            else debugInformation.AppendLine("Error! Data not found!");

            return debugInformation.ToString();
        }

        /// <summary>
        /// Reads a json file and serializes it into the appropriate d3dtx version that was serialized in the json file.
        /// </summary>
        /// <param name="filePath"></param>
        public void ReadD3DTXJSON(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            string jsonText = File.ReadAllText(filePath);

            // parse the data into a json array
            JArray jarray = JArray.Parse(jsonText);

            // read the first object in the array to determine if the json file is a legacy json file or not
            JObject firstObject = jarray[0] as JObject;

            int metaObjectIndex = 1;
            int d3dtxObjectIndex = 2;

            Game = TelltaleToolGameExtensions.GetTelltaleToolGameFromString(firstObject.ToObject<D3DTX_JSON>().GameID);
            Platform = firstObject.ToObject<D3DTX_JSON>().PlatformType;

            // I am creating the metaObject again instead of using the firstObject variable and i am aware of the performance hit.
            JObject? metaObject = jarray[metaObjectIndex] as JObject;
            ConvertJSONObjectToMeta(metaObject);

            // d3dtx object
            JObject? jsond3dtxObject = jarray[d3dtxObjectIndex] as JObject;

            //deserialize the appropriate json object
            CreateD3DTX(Game, jsond3dtxObject);

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

            ConvertToNewD3DTX(jObject, d3dtxVersion);
        }

        public void ConvertToNewD3DTX(JObject jObject, int d3dtxVersion)
        {
            switch (d3dtxVersion)
            {
                case 1:
                case 2:
                case 3:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = jObject.ToObject<D3DTX_V3>();
                    break;
                case 4:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = jObject.ToObject<D3DTX_V4>();
                    break;
                case 5:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = jObject.ToObject<D3DTX_V5>();
                    break;
                case 6:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = jObject.ToObject<D3DTX_V6>();
                    break;
                case 7:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = jObject.ToObject<D3DTX_V7>();
                    break;
                case 8:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = jObject.ToObject<D3DTX_V8>();
                    break;
                case 9:
                    Game = TelltaleToolGame.DEFAULT; d3dtxObject = jObject.ToObject<D3DTX_V9>();
                    break;
            }
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
                "6VSM" or "MSV6" => MetaVersion.MSV6,
                "5VSM" or "4VSM" or "MSV5" or "MSV4" => MetaVersion.MSV5,
                "ERTM" or "MTRE" => MetaVersion.MTRE,
                "NIBM" or "MBIN" => MetaVersion.MBIN,
                _ => throw new Exception("This meta version is not supported!"),
            };

            metaHeaderObject = MetaHeaderFactory.CreateMetaHeader(metaVersion, metaObject);
        }

        public void WriteD3DTXJSON(string fileName, string destinationDirectory)
        {
            if (d3dtxObject == null)
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
                GameID = TelltaleToolGameExtensions.GetGameName(Game),
                PlatformType = Platform
            };

            List<object> jsonObjects = [conversionTypeObject, metaHeaderObject, d3dtxObject];
            //serialize the data and write it to the configuration file
            serializer.Formatting = Formatting.Indented;
            serializer.Serialize(file, jsonObjects);
        }

        public void ModifyD3DTX(D3DTXMetadata metadata, ImageSection[] sections)
        {
            d3dtxMetadata = metadata;

            if (IsLegacyD3DTX())
            {
                // if (!HasDDSHeader())
                // {
                //sections = sections.Skip(1).ToArray();
                //}

                d3dtxObject.ModifyD3DTX(metadata, sections.ToArray());
            }
            else
            {
                // If they are not legacy version, stable sort the image sections by size. (Smallest to Largest)

                IEnumerable<ImageSection> newSections = sections;
                newSections = sections.OrderBy(section => section.Pixels.Length);

                d3dtxObject.ModifyD3DTX(metadata, newSections.ToArray());
                metaHeaderObject.SetMetaSectionChunkSizes(d3dtxObject.GetHeaderByteSize(), 0, ByteFunctions.GetByteArrayListElementsCount(d3dtxObject.GetPixelData()));
            }
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
        public static int ReadD3DTXFileD3DTXVersionOnly(string sourceFile)
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
            return Game != TelltaleToolGame.DEFAULT;
        }

        public bool HasDDSHeader()
        {
            foreach (var region in GetPixelData())
            {
                byte[] header = region.Take(128).ToArray();

                if (header[0] == 0x44 && header[1] == 0x44 && header[2] == 0x53 && header[3] == 0x20)
                {
                    return true;
                }
            }

            return false;
        }

        public string GetSurfaceFormat()
        {
            if (d3dtxMetadata.D3DFormat == LegacyFormat.UNKNOWN)
                return d3dtxMetadata.Format.ToString();
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

        public List<byte[]> GetPixelData()
        {
            return d3dtxObject.GetPixelData();
        }

        public byte[] GetReversedMipPixelData()
        {
            List<byte[]> finalArray = [];

            RegionStreamHeader[] regionHeaders = GetRegionStreamHeaders();

            for (int i = 0; i < d3dtxMetadata.MipLevels; i++)
            {
                for (int j = 0; j < regionHeaders.Length; j++)
                {
                    if (regionHeaders[j].mMipIndex == i)
                    {
                        finalArray.Add(GetPixelData()[j]);
                    }
                }
            }

            return finalArray.SelectMany(b => b).ToArray();
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

                    //  GetPixelData()[i] = DecodePixelDataByPlatform(GetPixelData()[i], surfaceFormat, width / divideBy, height / divideBy, platformType);

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

                    // GetPixelData()[i] = DecodePixelDataByPlatform(GetPixelData()[i], surfaceFormat, width, height, platformType);

                    newPixelData.Add(GetPixelData()[i]);
                }
            }

            return newPixelData.SelectMany(b => b).ToArray();
        }

        public static byte[] DecodePixelDataByPlatform(byte[] pixelData, T3SurfaceFormat surfaceFormat, int width, int height, T3PlatformType platformType)
        {
            DrSwizzler.DDS.DXEnums.DXGIFormat format = (DrSwizzler.DDS.DXEnums.DXGIFormat)DDSHelper.GetDXGIFormat(surfaceFormat);

            return platformType switch
            {
                T3PlatformType.ePlatform_PS3 => Deswizzler.PS3Deswizzle(pixelData, width, height, format),
                T3PlatformType.ePlatform_PS4 => Deswizzler.PS4Deswizzle(pixelData, width, height, format),
                T3PlatformType.ePlatform_NX => Deswizzler.SwitchDeswizzle(pixelData, width, height, format),
                T3PlatformType.ePlatform_Vita => Deswizzler.VitaDeswizzle(pixelData, width, height, format),
                T3PlatformType.ePlatform_Xbox or T3PlatformType.ePlatform_XBOne => Deswizzler.Xbox360Deswizzle(pixelData, width, height, format),
                _ => pixelData
            };
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
                T3SurfaceFormat.BC1 => true,
                T3SurfaceFormat.BC2 => true,
                T3SurfaceFormat.BC3 => true,
                T3SurfaceFormat.BC4 => true,
                T3SurfaceFormat.BC5 => true,
                T3SurfaceFormat.BC6 => true,
                T3SurfaceFormat.BC7 => true,
                T3SurfaceFormat.CTX1 => true,
                T3SurfaceFormat.ATC_RGB => true,
                T3SurfaceFormat.ATC_RGBA => true,
                T3SurfaceFormat.ATC_RGB1A => true,
                T3SurfaceFormat.ETC1_RGB => true,
                T3SurfaceFormat.ETC2_RGB => true,
                T3SurfaceFormat.ETC2_RGBA => true,
                T3SurfaceFormat.ETC2_RGB1A => true,
                T3SurfaceFormat.ETC2_R => true,
                T3SurfaceFormat.ETC2_RG => true,
                T3SurfaceFormat.ASTC_RGBA_4x4 => true,
                T3SurfaceFormat.PVRTC2 => true,
                T3SurfaceFormat.PVRTC4 => true,
                T3SurfaceFormat.PVRTC2a => true,
                T3SurfaceFormat.PVRTC4a => true,
                _ => false,
            };
        }

        public static bool IsFormatIncompatibleWithDDS(T3SurfaceFormat format)
        {
            return format switch
            {
                T3SurfaceFormat.ATC_RGB => true,
                T3SurfaceFormat.ATC_RGBA => true,
                T3SurfaceFormat.ATC_RGB1A => true,
                T3SurfaceFormat.ETC1_RGB => true,
                T3SurfaceFormat.ETC2_RGB => true,
                T3SurfaceFormat.ETC2_RGBA => true,
                T3SurfaceFormat.ETC2_RGB1A => true,
                T3SurfaceFormat.ETC2_R => true,
                T3SurfaceFormat.ETC2_RG => true,
                T3SurfaceFormat.ASTC_RGBA_4x4 => true,
                T3SurfaceFormat.PVRTC2 => true,
                T3SurfaceFormat.PVRTC4 => true,
                T3SurfaceFormat.PVRTC2a => true,
                T3SurfaceFormat.PVRTC4a => true,
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
                T3PlatformType.ePlatform_NX => true,
                T3PlatformType.ePlatform_Vita => true,
                _ => false
            };
        }

        public bool IsLegacyConsole()
        {
            return Platform != T3PlatformType.ePlatform_None;
        }
    }
}