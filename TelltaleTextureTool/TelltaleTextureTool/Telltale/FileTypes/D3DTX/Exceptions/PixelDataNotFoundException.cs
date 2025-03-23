using System;

public class PixelDataNotFoundException : Exception
{
    public PixelDataNotFoundException() { }

    public PixelDataNotFoundException(string message)
        : base(message) { }

    public PixelDataNotFoundException(string message, Exception inner)
        : base(message, inner) { }
}
