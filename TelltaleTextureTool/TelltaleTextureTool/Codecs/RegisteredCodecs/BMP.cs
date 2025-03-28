using System;
using System.Linq;
using Hexa.NET.DirectXTex;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.Graphics.Plugins;
using DirectXTexMetadata = Hexa.NET.DirectXTex.TexMetadata;

namespace TelltaleTextureTool.Codecs;

public class BmpCodec : IImageCodec
{
    public string Name => "BMP Codec";
    public string FormatName => "Bitmap Image";
    public string[] SupportedExtensions => [".bmp"];
    public static PixelFormatInfo[] SupportedPixelFormats =>
        [
            PixelFormats.B5G5R5A1_Unorm_Linear,
            PixelFormats.R8G8B8A8_Unorm_Linear,
            PixelFormats.B8G8R8A8_Unorm_Linear,
            PixelFormats.B8G8R8X8_Unorm_Linear,
            PixelFormats.B5G6R5_Unorm_Linear,
        ];

    public unsafe byte[] SaveToMemory(Texture input, CodecOptions options)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!SupportedPixelFormats.Contains(input.Metadata.PixelFormatInfo))
        {
            input.ConvertToRGBA8();
        }

        ScratchImage newImage = DirectXTexUtility.CreateScratchImageFromTexture(input);
        Blob blob = DirectXTex.CreateBlob();

        try
        {
            DirectXTex
                .SaveToWICMemory(
                    newImage.GetImage(0, 0, 0),
                    WICFlags.None,
                    DirectXTex.GetWICCodec(WICCodecs.CodecBmp),
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

    public Texture LoadFromMemory(byte[] input, CodecOptions options)
    {
        ScratchImage scratchImage = DirectXTex.CreateScratchImage();
        DirectXTexMetadata texMetadata = new();

        Texture texture;

        unsafe
        {
            fixed (byte* pInput = input)
            {
                var res = DirectXTex.LoadFromWICMemory(
                    pInput,
                    (nuint)input.Length,
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
}
