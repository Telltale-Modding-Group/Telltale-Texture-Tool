using System;
using System.IO;
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

    public Texture LoadFromMemory(Stream input, CodecOptions options)
    {
        ScratchImage scratchImage = DirectXTex.CreateScratchImage();
        DirectXTexMetadata texMetadata = new();

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
                HResult res = DirectXTex.LoadFromHDRMemory(
                    pData,
                    (nuint)buffer.Length,
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

        Texture texture = DirectXTexUtility.LoadFromScratchImage(scratchImage);

        scratchImage.Release();

        return texture;
    }
}
