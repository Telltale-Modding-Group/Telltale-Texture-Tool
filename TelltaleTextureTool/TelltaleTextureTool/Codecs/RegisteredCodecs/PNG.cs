using System;
using System.IO;
using System.Linq;
using Hexa.NET.DirectXTex;
using HexaGen.Runtime;
using SkiaSharp;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.Graphics.Plugins;
using DirectXTexMetadata = Hexa.NET.DirectXTex.TexMetadata;
using DirectXTexScratchImage = Hexa.NET.DirectXTex.ScratchImage;

namespace TelltaleTextureTool.Codecs;

public class PngCodec : IImageCodec
{
    public string Name => "PNG Codec";

    public string FormatName => "Portable Network Graphics";
    public string[] SupportedExtensions => [".png"];

    public static PixelFormatInfo[] SupportedPixelFormats =>
        [
            PixelFormats.R8_Unorm_Linear,
            PixelFormats.R8G8B8A8_Unorm_Linear,
            PixelFormats.B8G8R8A8_Unorm_Linear,
            PixelFormats.B8G8R8X8_Unorm_Linear,
        ];

    public unsafe byte[] SaveToMemory(Texture input, CodecOptions options)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!SupportedPixelFormats.Contains(input.Metadata.PixelFormatInfo))
        {
            input.ConvertToRGBA8();
        }

        DirectXTexScratchImage newImage = DirectXTexUtility.CreateScratchImageFromTexture(input);
        Blob blob = DirectXTex.CreateBlob();

        try
        {
            DirectXTex
                .SaveToWICMemory(
                    newImage.GetImage(0, 0, 0),
                    WICFlags.None,
                    DirectXTex.GetWICCodec(WICCodecs.CodecPng),
                    ref blob,
                    null,
                    default
                )
                .ThrowIf();

            return DirectXTexUtility.GetBytesFromBlob(blob);
        }
        finally
        {
            blob.Release();
            newImage.Release();
        }
    }

    public Texture LoadFromMemory(byte[] data, CodecOptions options)
    {
        DirectXTexScratchImage scratchImage = DirectXTex.CreateScratchImage();
        DirectXTexMetadata texMetadata = new();

        Texture texture;

        unsafe
        {
            fixed (byte* pData = data)
            {
                var res = DirectXTex.LoadFromWICMemory(
                    pData,
                    (nuint)data.Length,
                    WICFlags.AllFrames,
                    ref texMetadata,
                    ref scratchImage,
                    default
                );

                if (res.IsFailure)
                {
                    scratchImage.Release();
                    res.Throw();
                }
            }
        }

        texture = DirectXTexUtility.LoadFromScratchImage(scratchImage);

        scratchImage.Release();

        return texture;
    }

    public Texture LoadFromFile(string filePath, CodecOptions options)
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            var bytes = File.ReadAllBytes(filePath);
            return LoadFromMemory(bytes, options);
        }
        else
        {
            DirectXTexScratchImage scratchImage = DirectXTex.CreateScratchImage();
            DirectXTexMetadata texMetadata = new();

            Texture texture;

            var res = DirectXTex.LoadFromPNGFile(filePath, ref texMetadata, ref scratchImage);

            if (res.IsFailure)
            {
                scratchImage.Release();
                res.Throw();
            }

            texture = DirectXTexUtility.LoadFromScratchImage(scratchImage);

            scratchImage.Release();

            return texture;
        }
    }

    public unsafe void SaveToFile(string filePath, Texture input, CodecOptions options)
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            var bytes = SaveToMemory(input, options);
            File.WriteAllBytes(filePath, bytes);
        }
        else
        {
            ArgumentNullException.ThrowIfNull(input);

            if (!SupportedPixelFormats.Contains(input.Metadata.PixelFormatInfo))
            {
                input.ConvertToRGBA8();
            }

            DirectXTexScratchImage newImage = DirectXTexUtility.CreateScratchImageFromTexture(
                input
            );

            try
            {
                DirectXTex.SaveToPNGFile(newImage.GetImage(0, 0, 0), filePath).ThrowIf();
            }
            finally
            {
                newImage.Release();
            }
        }
    }
}
