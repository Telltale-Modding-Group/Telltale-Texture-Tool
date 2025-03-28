using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TelltaleTextureTool.Graphics;

namespace TelltaleTextureTool.Codecs;

public interface IImageCodec
{
    string Name { get; } // Codec name (e.g., "PNG Codec")
    string FormatName { get; } // Format name (e.g., "Portable Network Graphics")
    string[] SupportedExtensions { get; } // Supported file extensions
    static PixelFormatInfo[]? SupportedPixelFormats { get; } // Supported pixel formats

    // Core operations
    public Texture LoadFromMemory(byte[] input, CodecOptions options);

    public Texture LoadFromFile(string filePath, CodecOptions options)
    {
        var bytes = File.ReadAllBytes(filePath);
        return LoadFromMemory(bytes, options);
    }

    public byte[] SaveToMemory(Texture input, CodecOptions options);

    public void SaveToFile(string filePath, Texture input, CodecOptions options)
    {
        var bytes = SaveToMemory(input, options);
        File.WriteAllBytes(filePath, bytes);
    }

    bool IsSupportedPixelFormat(PixelFormatInfo format)
    {
        return SupportedPixelFormats.Contains(format);
    }
}

public class CodecManager
{
    private readonly Dictionary<string, IImageCodec> _codecs = new(
        StringComparer.OrdinalIgnoreCase
    );

    public CodecManager()
    {
        // Register built-in codecs
        RegisterCodec(new PngCodec());
        RegisterCodec(new DdsCodec());
        RegisterCodec(new TgaCodec());
        RegisterCodec(new JpegCodec());
        RegisterCodec(new TiffCodec());
        RegisterCodec(new D3dtxCodec());
        RegisterCodec(new BmpCodec());
        RegisterCodec(new HdrCodec());
    }

    public void RegisterCodec(IImageCodec codec)
    {
        foreach (var ext in codec.SupportedExtensions)
        {
            var normalizedExt = ext.StartsWith('.') ? ext : $".{ext}";
            _codecs[normalizedExt] = codec;
        }
    }

    public IEnumerable<string> GetAllSupportedExtensions()
    {
        return _codecs.Keys.Distinct().OrderBy(x => x);
    }

    public IImageCodec GetCodecForExtension(string extension)
    {
        var normalizedExt = extension.StartsWith('.') ? extension : $".{extension}";

        if (_codecs.TryGetValue(normalizedExt, out var codec))
            return codec;

        throw new NotSupportedException($"No codec registered for extension {normalizedExt}");
    }

    // Core operations through manager
    public Texture LoadFromFile(string filePath, CodecOptions options)
    {
        var extension = Path.GetExtension(filePath);
        return GetCodecForExtension(extension).LoadFromFile(filePath, options);
    }

    public Texture LoadFromMemory(string format, byte[] input, CodecOptions options)
    {
        return GetCodecForExtension(format).LoadFromMemory(input, options);
    }

    public void SaveToFile(string filePath, Texture input, CodecOptions options)
    {
        var extension = Path.GetExtension(filePath);
        GetCodecForExtension(extension).SaveToFile(filePath, input, options);
    }

    public byte[] SaveToMemory(string format, Texture input, CodecOptions options)
    {
        return GetCodecForExtension(format).SaveToMemory(input, options);
    }
}
