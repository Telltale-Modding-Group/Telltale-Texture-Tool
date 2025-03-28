using System;
using System.Linq;
using Hexa.NET.DirectXTex;
using HexaGen.Runtime;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.Graphics.Plugins;
using DirectXTexMetadata = Hexa.NET.DirectXTex.TexMetadata;
using DirectXTexScratchImage = Hexa.NET.DirectXTex.ScratchImage;

namespace TelltaleTextureTool.Codecs;

public class TgaCodec : IImageCodec
{
    public string Name => "TGA Codec";
    public string FormatName => "Truevision TGA";
    public string[] SupportedExtensions => [".tga"];

    public static PixelFormatInfo[] SupportedPixelFormats =>
        [
            PixelFormats.R8_Unorm_Linear,
            PixelFormats.A8_Unorm_Linear,
            PixelFormats.R8G8B8A8_Unorm_Linear,
            PixelFormats.B8G8R8A8_Unorm_Linear,
            PixelFormats.B8G8R8X8_Unorm_Linear,
            PixelFormats.R8G8B8A8_Unorm_Srgb,
            PixelFormats.B8G8R8A8_Unorm_Srgb,
            PixelFormats.B8G8R8X8_Unorm_Srgb,
            PixelFormats.B5G5R5A1_Unorm_Linear,
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
                .SaveToTGAMemory(newImage.GetImage(0, 0, 0), TGAFlags.None, ref blob, default)
                .ThrowIf();

            return DirectXTexUtility.GetBytesFromBlob(blob);
        }
        finally
        {
            blob.Release();
            newImage.Release();
        }
    }

    public Texture LoadFromMemory(byte[] input, CodecOptions options)
    {
        DirectXTexScratchImage scratchImage = DirectXTex.CreateScratchImage();
        DirectXTexMetadata texMetadata = new();

        Texture texture;

        unsafe
        {
            fixed (byte* pInput = input)
            {
                var res = DirectXTex.LoadFromTGAMemory(
                    pInput,
                    (nuint)input.Length,
                    TGAFlags.None,
                    ref texMetadata,
                    ref scratchImage
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
}
