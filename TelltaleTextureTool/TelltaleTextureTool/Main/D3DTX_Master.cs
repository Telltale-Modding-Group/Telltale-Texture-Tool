using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;
using TelltaleTextureTool.Telltale.Meta;
using TelltaleTextureTool.TelltaleD3DTX;
using TelltaleTextureTool.TelltaleEnums;
using TelltaleTextureTool.TelltaleTypes;
using TelltaleTextureTool.Utilities;
using PixelFormat = TelltaleTextureTool.Graphics.PixelFormat;

namespace TelltaleTextureTool.Main
{
    /// <summary>
    /// This is the master class object for a D3DTX file. Reads a file and automatically parses the data into the correct version.
    /// </summary>
    public class D3DTX_Master
    {
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

        public enum MetaVersion
        {
            MSV6 = 3,
            MSV5 = 2,
            MTRE = 1,
            MBIN = 0,
            Unknown = -1,
        }

        public void ReadD3DTXBytes(
            byte[] bytes,
            TelltaleToolGame game = TelltaleToolGame.DEFAULT,
            bool isLegacyConsole = false
        )
        {
            MemoryStream memoryStream = new(bytes);
            BinaryReader reader = new(memoryStream);

            string metaFourCC = ReadD3DTXFileMetaVersionOnly(bytes);

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
                    Console.WriteLine(
                        "ERROR! '{0}' meta stream version is not supported!",
                        metaFourCC
                    );
                    return;
            }

            metaHeaderObject = MetaHeaderFactory.CreateMetaHeader(metaVersion);
            metaHeaderObject.ReadFromBinary(
                reader,
                TelltaleToolGame.DEFAULT,
                T3PlatformType.ePlatform_None
            );

            // Attempt to read the d3dtx version of the file
            int d3dtdMetaVersion = ReadD3DTXFileD3DTXVersionOnly(bytes);
            Game = game;

            switch (d3dtdMetaVersion)
            {
                case 1:
                case 2:
                case 3:
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = new D3DTX_V3();
                    break;
                case 4:
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = new D3DTX_V4();
                    break;
                case 5:
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = new D3DTX_V5();
                    break;
                case 6:
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = new D3DTX_V6();
                    break;
                case 7:
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = new D3DTX_V7();
                    break;
                case 8:
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = new D3DTX_V8();
                    break;
                case 9:
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = new D3DTX_V9();
                    break;
                case -1:

                    if (game == TelltaleToolGame.DEFAULT)
                    {
                        Game = TryToInitializeLegacyD3DTX(reader);
                    }

                    if (Game != TelltaleToolGame.UNKNOWN)
                    {
                        d3dtxObject = new D3DTX_Legacy();
                    }

                    break;
                default:
                    Console.WriteLine("ERROR! '{0}' d3dtx version is not supported!", Game);
                    break;
            }

            if (Game != TelltaleToolGame.UNKNOWN)
            {
                d3dtxObject.ReadFromBinary(reader, Game, Platform);

                d3dtxMetadata = d3dtxObject.GetD3DTXMetadata();
            }
        }

        /// <summary>
        /// Reads in a D3DTX file from the disk.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="setD3DTXVersion"></param>
        public void ReadD3DTXFile(
            string filePath,
            TelltaleToolGame game = TelltaleToolGame.DEFAULT,
            bool isLegacyConsole = false
        )
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            ReadD3DTXBytes(bytes);
            // Read meta version of the file
            //
        }

        public void CreateD3DTX(TelltaleToolGame game, JObject jsonObject)
        {
            if (game == TelltaleToolGame.UNKNOWN)
            {
                throw new Exception(
                    "This D3DTX version is not supported. Please report this issue to the author!"
                );
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
                    _ => throw new ArgumentException($"Unsupported Meta version: {version}"),
                };
            }

            public static IMetaHeader CreateMetaHeader(MetaVersion version, JObject jsonObject)
            {
                if (jsonObject is null)
                    throw new ArgumentNullException(
                        nameof(jsonObject),
                        "JSON object cannot be null"
                    );

                return version switch
                {
                    MetaVersion.MSV6 => jsonObject.ToObject<MSV6>(),
                    MetaVersion.MSV5 => jsonObject.ToObject<MSV5>(),
                    MetaVersion.MTRE => jsonObject.ToObject<MTRE>(),
                    MetaVersion.MBIN => jsonObject.ToObject<MBIN>(),
                    _ => throw new ArgumentException($"Unsupported Meta version: {version}"),
                };
            }
        }

        public TelltaleToolGame TryToInitializeLegacyD3DTX(BinaryReader reader)
        {
            var startPos = reader.BaseStream.Position;
            var allGames = Enum.GetValues(typeof(TelltaleToolGame))
                .Cast<TelltaleToolGame>()
                .ToArray();

            // Try to initialize with no specific platform
            var game = TryInitializeForPlatform(
                reader,
                startPos,
                allGames,
                T3PlatformType.ePlatform_None
            );
            if (game != TelltaleToolGame.UNKNOWN)
                return game;

            return TelltaleToolGame.UNKNOWN;
        }

        private TelltaleToolGame TryInitializeForPlatform(
            BinaryReader reader,
            long startPos,
            TelltaleToolGame[] allGames,
            T3PlatformType platform
        )
        {
            foreach (var game in allGames)
            {
                try
                {
                    var testObj = new D3DTX_Legacy();
                    testObj.ReadFromBinary(reader, game, platform);
                    reader.BaseStream.Position = startPos;
                    return game;
                }
                catch (PixelDataNotFoundException)
                {
                    throw new PixelDataNotFoundException(
                        "The texture does not have any pixel data!"
                    );
                }
                catch (Exception)
                {
                    reader.BaseStream.Position = startPos;
                }
            }

            return TelltaleToolGame.UNKNOWN;
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
            else
                debugInformation.AppendLine("Error! Meta data not found!");

            if (d3dtxObject != null)
                debugInformation.Append(d3dtxObject.GetDebugInfo(Game));
            else
                debugInformation.AppendLine("Error! Data not found!");

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

            Game = TelltaleToolGameExtensions.GetTelltaleToolGameFromString(
                firstObject.ToObject<D3DTX_JSON>().GameID
            );
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
                if (property.Name.Equals("mVersion"))
                    d3dtxVersion = (int)property.Value;
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
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = jObject.ToObject<D3DTX_V3>();
                    break;
                case 4:
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = jObject.ToObject<D3DTX_V4>();
                    break;
                case 5:
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = jObject.ToObject<D3DTX_V5>();
                    break;
                case 6:
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = jObject.ToObject<D3DTX_V6>();
                    break;
                case 7:
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = jObject.ToObject<D3DTX_V7>();
                    break;
                case 8:
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = jObject.ToObject<D3DTX_V8>();
                    break;
                case 9:
                    Game = TelltaleToolGame.DEFAULT;
                    d3dtxObject = jObject.ToObject<D3DTX_V9>();
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
                if (property.Name.Equals("mMetaStreamVersion"))
                    metaStreamVersion = (string)property.Value;
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
            byte[] jsonBytes = WriteD3DTXJSONToBytes();

            string newPath =
                destinationDirectory
                + Path.DirectorySeparatorChar
                + fileName
                + Main_Shared.jsonExtension;

            File.WriteAllBytes(newPath, jsonBytes);
        }

        public byte[] WriteD3DTXJSONToBytes()
        {
            if (d3dtxObject == null)
            {
                return [];
            }

            D3DTX_JSON conversionTypeObject = new()
            {
                GameID = TelltaleToolGameExtensions.GetGameName(Game),
                PlatformType = Platform,
            };

            List<object> jsonObjects = [conversionTypeObject, metaHeaderObject, d3dtxObject];

            // Serialize to JSON string first
            string jsonString = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented);

            // Convert the JSON string to a byte array (UTF-8 encoded)
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);

            return jsonBytes;
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
                metaHeaderObject.SetMetaSectionChunkSizes(
                    d3dtxObject.GetHeaderByteSize(),
                    0,
                    ByteFunctions.GetByteArrayListElementsCount(d3dtxObject.GetPixelData())
                );
            }
        }

        /// <summary>
        /// Reads a d3dtx file on the disk and returns the meta version that is being used.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        public static string ReadD3DTXFileMetaVersionOnly(byte[] bytes)
        {
            using MemoryStream memoryStream = new(bytes);
            using BinaryReader reader = new(memoryStream);

            string metaStreamVersion = "";

            for (int i = 0; i < 4; i++)
                metaStreamVersion += reader.ReadChar();

            return metaStreamVersion;
        }

        /// <summary>
        /// Reads a d3dtx file on the disk and returns the D3DTX version.
        /// <para>NOTE: This only works with d3dtx meta version 5VSM and 6VSM</para>
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        public static int ReadD3DTXFileD3DTXVersionOnly(byte[] bytes)
        {
            string metaFourCC = ReadD3DTXFileMetaVersionOnly(bytes);

            using MemoryStream memoryStream = new(bytes);
            using BinaryReader reader = new(memoryStream);

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

        public bool IsInitialized()
        {
            return Game != TelltaleToolGame.UNKNOWN;
        }

        public bool HasDDSHeader()
        {
            foreach (var region in GetPixelData())
            {
                byte[] header = region.Take(128).ToArray();

                if (
                    header[0] == 0x44
                    && header[1] == 0x44
                    && header[2] == 0x53
                    && header[3] == 0x20
                )
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasDDSHeader(byte[] array)
        {
            byte[] header = array.Take(4).ToArray();

            return header[0] == 0x44 && header[1] == 0x44 && header[2] == 0x53 && header[3] == 0x20;
        }

        public RegionStreamHeader[] GetRegionStreamHeaders()
        {
            return d3dtxMetadata.RegionHeaders;
        }

        public List<byte[]> GetPixelData()
        {
            return d3dtxObject.GetPixelData();
        }

        public class RegionData
        {
            public RegionStreamHeader Header { get; set; }
            public byte[] PixelData { get; set; }
        }

        public List<RegionData> GetMappedData()
        {
            RegionStreamHeader[] regionHeaders = GetRegionStreamHeaders();
            List<byte[]> pixelData = GetPixelData(); // Your List<byte[]> here

            return regionHeaders
                .Zip(
                    pixelData,
                    (header, data) => new RegionData { Header = header, PixelData = data }
                )
                .ToList();
        }

        public List<RegionData> GetRegionDataSortedByMips()
        {
            List<RegionData> mappedData = GetMappedData();
            mappedData = mappedData
                .Select((x, index) => new { Data = x, Index = index })
                .OrderBy(x => x.Data.Header.mMipIndex)
                .ThenBy(x => x.Data.Header.mFaceIndex)
                .ThenBy(x => x.Index)
                .Select(x => x.Data)
                .ToList();
            return mappedData;
        }

        public static byte[] GetSliceData(RegionData region, uint sliceIndex)
        {
            if (sliceIndex > (region.Header.mDataSize / region.Header.mSlicePitch))
            {
                throw new ArgumentException("Slice index out of bounds!");
            }

            int sliceSize = region.Header.mSlicePitch;
            int sliceOffset = (int)(sliceIndex * sliceSize);

            return region.PixelData.Skip(sliceOffset).Take(sliceSize).ToArray();
        }

        public static byte[] ExtractSingleMipFromRegion(
            RegionData region,
            PixelFormat pixelFormat,
            uint width,
            uint height,
            uint depth
        )
        {
            var pitches = PixelFormatUtility.ComputePitch(pixelFormat, width, height);
            var slicePitch = pitches.slicePitch;

            return region.PixelData.Skip((int)(slicePitch * depth)).Take((int)slicePitch).ToArray();
        }

        public static void RemoveMip(
            RegionData region,
            PixelFormat pixelFormat,
            uint width,
            uint height,
            uint depth
        )
        {
            if (region.Header.mMipCount <= 1)
            {
                return;
            }

            var (rowPitch, slicePitch) = PixelFormatUtility.ComputePitch(
                pixelFormat,
                width,
                height
            );

            region.Header.mMipCount -= 1;
            region.Header.mPitch = (int)rowPitch;
            region.Header.mSlicePitch = (int)slicePitch;
            region.Header.mDataSize = (uint)region.Header.mSlicePitch * depth;
            region.PixelData = region.PixelData.Skip((int)(slicePitch * depth)).ToArray();
        }

        public bool IsLegacyConsole()
        {
            return Platform != T3PlatformType.ePlatform_None;
        }
    }
}
