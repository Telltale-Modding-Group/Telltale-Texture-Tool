using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace TelltaleTextureTool.Graphics;

public enum ImageEffect
{
    [Display(Name = "None")]
    None,
    [Display(Name = "RGBA->BGRA")]
    SwizzleRB,
    [Display(Name = "RGBA->ABGR")]
    SwizzleRGBA,
    [Display(Name = "Restore Z")]
    RestoreZ,
    [Display(Name = "Remove Z")]
    RemoveZ
}

public static class PixelFunctions
{
    // For RGBA8, RGBA16, RGBA32
    public static void ReverseChannels<T>(T[] pixels)
        where T : INumber<T>
    {
        for (int i = 0; i < pixels.Length; i += 4)
        {
            T r = pixels[i];
            T g = pixels[i + 1];
            T b = pixels[i + 2];
            T a = pixels[i + 3];

            pixels[i] = a;
            pixels[i + 1] = b;
            pixels[i + 2] = g;
            pixels[i + 3] = r;
        }
    }

    // For RGBA8, RGBA16, RGBA32
    public static void ReverseRBChannels<T>(T[] pixels)
        where T : INumber<T>
    {
        for (int i = 0; i < pixels.Length; i += 4)
        {
            T r = pixels[i];
            T b = pixels[i + 2];

            pixels[i] = b;
            pixels[i + 2] = r;
        }
    }

    // For RGBA8, RGBA16, RGBA32
    public static void RemoveZ<T>(T[] pixels)
        where T : INumber<T>
    {
        for (int i = 0; i < pixels.Length; i += 4)
        {
            T r = pixels[i];
            T g = pixels[i + 1];

            pixels[i] = r;
            pixels[i + 1] = g;
            pixels[i + 2] = T.Zero;
            pixels[i + 3] = T.Zero;
        }
    }

    // For RGBA8, RGBA16, RGBA32
    public static void RestoreZ(Vector4[] pixels)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            float r = pixels[i].X;
            float g = pixels[i].Y;

            Vector2 NormalXY = new(r, g);

            NormalXY = NormalXY * 2.0f - Vector2.One;
            float NormalZ = (float)
                Math.Sqrt(Math.Clamp(1.0f - Vector2.Dot(NormalXY, NormalXY), 0, 1));

            pixels[i].Z = NormalZ;
            pixels[i].W = 1.0f;
        }
    }

    public static T[] GetTransformedPixels<T>(T[] pixels, ImageEffect effect)
    {
        return pixels;

    }
}
