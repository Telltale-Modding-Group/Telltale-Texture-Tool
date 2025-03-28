using System;
using System.Linq;
using Hexa.NET.DirectXTex;
using HexaGen.Runtime;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.Graphics.Plugins;
using DirectXTexMetadata = Hexa.NET.DirectXTex.TexMetadata;
using DirectXTexScratchImage = Hexa.NET.DirectXTex.ScratchImage;

namespace TelltaleTextureTool.Codecs;

public class HdrCodec : IImageCodec
{
    public string Name => "HDR Codec";
    public string FormatName => "High Dynamic Range";
    public string[] SupportedExtensions => [".hdr"];

    public static PixelFormatInfo[] SupportedPixelFormats =>
        [
            PixelFormats.R32G32B32A32_Float_Linear,
            PixelFormats.R32G32B32_Float_Linear,
            PixelFormats.R16G16B16A16_Float_Linear,
        ];

    public unsafe byte[] SaveToMemory(Texture input, CodecOptions options)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!SupportedPixelFormats.Contains(input.Metadata.PixelFormatInfo))
        {
            input.ConvertToRGBA32F();
        }

        DirectXTexScratchImage newImage = DirectXTexUtility.CreateScratchImageFromTexture(input);
        Blob blob = DirectXTex.CreateBlob();

        try
        {
            DirectXTex.SaveToHDRMemory(newImage.GetImage(0, 0, 0), ref blob).ThrowIf();

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
        ScratchImage scratchImage = DirectXTex.CreateScratchImage();
        DirectXTexMetadata texMetadata = new();

        Texture texture;

        unsafe
        {
            fixed (byte* pData = data)
            {
                var res = DirectXTex.LoadFromHDRMemory(
                    pData,
                    (nuint)data.Length,
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
