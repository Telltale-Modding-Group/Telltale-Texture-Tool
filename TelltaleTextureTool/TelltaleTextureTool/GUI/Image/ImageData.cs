using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Texture = TelltaleTextureTool.DirectX.Texture;

namespace TelltaleTextureTool;

public class ImageData
{
    public Texture DDSImage { get; set; } = new Texture();
    private bool HasPixelData { get; set; }

    /// <summary>
    /// Applies the effects to the image.
    /// </summary>
    /// <param name="options"></param>
    public void ApplyEffects(ImageAdvancedOptions options)
    {
        try
        {
            DDSImage.TransformTexture(options, true, false);
        }
        catch (Exception)
        {
            HasPixelData = false;
            throw;
        }
    }

    /// <summary>
    /// Converts the data from the scratch image to a bitmap.
    /// </summary>
    /// <param name="mip"></param>
    /// <param name="face"></param>
    /// <returns>The bitmap from the mip and face. </returns>
    public static WriteableBitmap GetBitmap(uint width, uint height, byte[] pixels)
    {
        // Create a WriteableBitmap in RGBA8888 format
        var bitmap = new WriteableBitmap(
            new PixelSize((int)width, (int)height),
            new Vector(96, 96), // Set DPI as necessary
            PixelFormat.Rgba8888,
            AlphaFormat.Unpremul
        );

        // Lock the WriteableBitmap's back buffer to write pixel data
        using (var framebuffer = bitmap.Lock())
        {
            IntPtr framebufferPtr = framebuffer.Address;
            int framebufferRowBytes = framebuffer.RowBytes;

            // Copy pixelData to the WriteableBitmap's memory
            Marshal.Copy(pixels, 0, framebufferPtr, pixels.Length);
        }

        return bitmap;
    }
}
