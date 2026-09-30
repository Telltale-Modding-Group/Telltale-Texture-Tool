using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hexa.NET.DirectXTex;
using TelltaleTextureTool.Codecs;
using TelltaleTextureTool.DirectX;
// using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.Main;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;
using TelltaleToolKit;
using TelltaleToolKit.Serialization.Binary;
using TelltaleToolKit.T3Types.Textures;
using Texture = TelltaleTextureTool.Graphics.Texture;
// using Texture = TelltaleTextureTool.DirectX.Texture;
using TextureType = TelltaleTextureTool.Graphics.TextureType;

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
    /// Converts multiple texture files from one format to another using parallel processing.
    /// </summary>
    /// <param name="texPath">The path to the folder containing the texture files.</param>
    /// <param name="resultPath">The path to save the converted texture files.</param>
    /// <param name="options">The advanced options to apply to the texture files.</param>
    /// <param name="oldTextureType">The file type of the original texture files.</param>
    /// <param name="newTextureType">The file type to convert the texture files to.</param>
    /// <returns>True if all conversions succeeded; otherwise, throws AggregateException.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when source or destination directory is invalid.</exception>
    /// <exception cref="FileNotFoundException">Thrown when no matching source files are found.</exception>
    /// <exception cref="ArgumentException">Thrown when invalid texture types are provided.</exception>
    /// <exception cref="AggregateException">Thrown when one or more conversions fail.</exception>
    public static bool ConvertBulk(
        string texPath,
        string resultPath,
        ConverterOptions imageOptions,
        TextureType oldTextureType,
        TextureType newTextureType
    )
    {
        // Validate inputs
        if (newTextureType == TextureType.Unknown)
            throw new ArgumentException(
                "Target texture type must be specified",
                nameof(newTextureType)
            );

        if (!Directory.Exists(texPath))
            throw new DirectoryNotFoundException($"Source directory not found: {texPath}");

        Directory.CreateDirectory(resultPath);

        // Get and filter files
        var supportedExtensions = GetExtension(oldTextureType);
        var textures = Directory
            .EnumerateFiles(texPath)
            .Where(f =>
                supportedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)
            )
            .ToList();

        if (textures.Count == 0)
            throw new FileNotFoundException(
                $"No {oldTextureType} files found in {texPath}",
                texPath
            );

        // Parallel processing with error handling
        var exceptions = new ConcurrentQueue<Exception>();
        var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

        Parallel.ForEach(
            textures,
            options,
            texture =>
            {
                try
                {
                    ConvertTexture(
                        texture,
                        resultPath,
                        imageOptions,
                        oldTextureType,
                        newTextureType
                    );
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(
                        new Exception(
                            $"Failed to convert {Path.GetFileName(texture)}: {ex.Message}",
                            ex
                        )
                    );
                }
            }
        );

        // Throw an exception if any conversions failed
        if (!exceptions.IsEmpty)
            throw new AggregateException(
                $"{exceptions.Count} conversion(s) failed. See inner exceptions for details.",
                exceptions
            );

        return true;
    }

    public static void ConvertTexture(
        string sourcePath,
        string resultPath,
        ConverterOptions options,
        TextureType oldTextureType,
        TextureType newTextureType
    )
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
                    ConvertD3DtxTextureToCommonTexture(
                        sourcePath,
                        resultPath,
                        newTextureType,
                        options
                    );
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
                    ConvertCommonTextureToD3DtxTexture(
                        sourcePath,
                        resultPath,
                        oldTextureType,
                        options
                    );
                    break;
                default:
                    throw new Exception("Invalid file type.");
            }
        }
        else if (oldTextureType is TextureType.PNG or TextureType.JPEG or TextureType.BMP or TextureType.TIFF
                 or TextureType.TGA or TextureType.HDR
                )
        {
            switch (newTextureType)
            {
                case TextureType.D3DTX:
                    ConvertCommonTextureToD3DtxTexture(
                        sourcePath,
                        resultPath,
                        oldTextureType,
                        options
                    );
                    break;
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
    public static void ConvertD3DtxTextureToCommonTexture(string sourceFilePath, string destinationDirectory,
        TextureType newTextureType, ConverterOptions options)
    {
        // Null safety validation of inputs.
        if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationDirectory))
        {
            throw new ArgumentException("Arguments cannot be null in D3DtxToDds function.");
        }

        D3DTX_Master d3dtxFile = new();
        d3dtxFile.ReadD3DTXFile(sourceFilePath, options.GameID);

        // DDS_Master ddsFile = new(d3dtxFile);

        //  var array = ddsFile.GetData(d3dtxFile);

        d3dtxFile.WriteD3DTXJSON(
            Path.GetFileNameWithoutExtension(sourceFilePath),
            destinationDirectory
        );

        //  Texture texture = new(array, TextureType.D3DTX);

        //    texture.TransformTexture(options, true, false);
        //   texture.SaveTexture(
        //       Path.Combine(destinationDirectory, Path.GetFileNameWithoutExtension(sourceFilePath)),
        //       newTextureType
        //   );
        //   texture.Release();
    }

    /// <summary>
    /// The main function for reading and converting said .dds back into a .d3dtx file
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="destinationDirectory"></param>
    /// <param name="oldTextureType"></param>
    /// <param name="options"></param>
    public static void ConvertCommonTextureToD3DtxTexture(string sourceFilePath, string destinationDirectory,
        TextureType oldTextureType, ConverterOptions options)
    {
        // Deconstruct the source file path
        string textureFileDirectory = Path.GetDirectoryName(sourceFilePath);
        string textureFileNameOnly = Path.GetFileNameWithoutExtension(sourceFilePath);

        // Create the path of these files. If things go well, these files (depending on the version) should exist in the same directory at the original .dds file.
        string textureFilePathJson = Path.Combine(textureFileDirectory, textureFileNameOnly + ".json");

        // Create a new d3dtx object
        (T3Texture tex, MetaStreamConfiguration configuration) d3dtx =
            T3TextureJsonConverter.FromJson(File.ReadAllText(textureFilePathJson));

        // If the d3dtx is a legacy D3DTX, force the use of the DX9 legacy flag
        //  DDSFlags flags = d3dtx.tex.IsLegacyD3DTX() ? DDSFlags.ForceDx9Legacy : DDSFlags.None;

        CodecManager codecManager = new();

        Texture commonTexture = codecManager.LoadFromFile(sourceFilePath, new CodecOptions());

        byte[] ddsTexture = codecManager.SaveToMemory(".dds", commonTexture,
            new CodecOptions() { ForceDx9Legacy = d3dtx.tex.IsLegacyD3DTX() });

        //  Texture texture = new(sourceFilePath, oldTextureType, flags);

        options.IsTelltaleNormalMap = d3dtx.tex.TypeEnum switch
        {
            // Set the options for the converter
            TelltaleToolKit.T3Types.Textures.TextureType.Bumpmap
                or TelltaleToolKit.T3Types.Textures.TextureType.NormalMap
                or TelltaleToolKit.T3Types.Textures.TextureType.NormalXyMap => true,
            _ => options.IsTelltaleNormalMap
        };

        if (d3dtx.tex.SurfaceGamma is TelltaleToolKit.T3Types.Textures.T3Types.T3SurfaceGamma.sRGB)
        {
            options.IsSRGB = true;
        }

        // texture.TransformTexture(options, true, true);
        // texture.GetDDSInformation(out D3DTXMetadata metadata, out ImageSection[] sections, flags);
        // texture.Release();
        //
        // // if (options.EnableSwizzle)
        // // {
        // //     // metadata.Platform = options.PlatformType;
        // // }
        //
        // // Modify the d3dtx file using our dds data
        // // TODO:
        // d3dtx.tex.ConvertD3Dtx(commonTexture.Metadata, sections);

        TTK.Save(d3dtx.tex, Path.Combine(destinationDirectory, textureFileNameOnly + ".d3dtx"), d3dtx.configuration);
    }
}