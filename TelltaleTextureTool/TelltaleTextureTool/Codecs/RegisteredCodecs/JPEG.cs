using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Hexa.NET.DirectXTex;
using HexaGen.Runtime;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.Graphics.Plugins;
using DirectXTexMetadata = Hexa.NET.DirectXTex.TexMetadata;
using DirectXTexScratchImage = Hexa.NET.DirectXTex.ScratchImage;

namespace TelltaleTextureTool.Codecs;

public class JpegCodec : IImageCodec
{
    public string Name => "JPEG Codec";
    public string FormatName => "Joint Photographic Experts Group";
    public string[] SupportedExtensions => [".jpeg", ".jpg"];

    static PixelFormatInfo[] SupportedPixelFormats =>
    [
        PixelFormats.R8_Unorm_Linear,
        PixelFormats.R8G8B8A8_Unorm_Linear,
        PixelFormats.B8G8R8A8_Unorm_Linear,
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
                    DirectXTex.GetWICCodec(WICCodecs.CodecJpeg),
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

    public Texture LoadFromMemory(Stream input, CodecOptions options)
    {
        DirectXTexScratchImage scratchImage = DirectXTex.CreateScratchImage();
        DirectXTexMetadata texMetadata = new();

        Texture texture;

        byte[] buffer;
        using (var ms = new MemoryStream())
        {
            input.CopyTo(ms);
            buffer = ms.ToArray();
        }

        unsafe
        {
            fixed (byte* pData = buffer)
            {
                var res = DirectXTex.LoadFromWICMemory(
                    pData,
                    (nuint)buffer.Length,
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
            return LoadFromMemory(File.OpenRead(filePath), options);
        }
        else
        {
            DirectXTexScratchImage scratchImage = DirectXTex.CreateScratchImage();
            DirectXTexMetadata texMetadata = new();

            HResult res = DirectXTex.LoadFromJPEGFile(filePath, ref texMetadata, ref scratchImage);

            if (res.IsFailure)
            {
                scratchImage.Release();
                res.Throw();
            }

            Texture texture = DirectXTexUtility.LoadFromScratchImage(scratchImage);

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

            DirectXTexScratchImage newImage = DirectXTexUtility.CreateScratchImageFromTexture(input);

            try
            {
                DirectXTex.SaveToJPEGFile(newImage.GetImage(0, 0, 0), filePath).ThrowIf();
            }
            finally
            {
                newImage.Release();
            }
        }
    }
}