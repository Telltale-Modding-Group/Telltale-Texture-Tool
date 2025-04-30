using System;
using static DrSwizzler.Util;

namespace DrSwizzler.Swizzling
{
    internal class PS3Deswizzler
    {
        public static byte[] PS3Deswizzle(byte[] swizzledData, int width, int height, int sourceBytesPerPixelSet, int pixelBlockSize, int formatbpp)
        {
            //If it's not long enough, return as is
            if (sourceBytesPerPixelSet >= swizzledData.Length)
            {
                return swizzledData;
            }

            int calculatedBufferSize = (formatbpp * width * height) / 8;
            byte[] outBuffer = new byte[calculatedBufferSize > sourceBytesPerPixelSet ? calculatedBufferSize : sourceBytesPerPixelSet];
            byte[] tempBuffer = new byte[sourceBytesPerPixelSet];
            int sy = height / pixelBlockSize;
            int sx = width / pixelBlockSize;
            for (int t = 0; t < sx * sy; ++t)
            {
                int num5 = Morton(t, sx, sy);
                Array.Copy(swizzledData, t * sourceBytesPerPixelSet, tempBuffer, 0, sourceBytesPerPixelSet);
                int destinationIndex = sourceBytesPerPixelSet * num5;
                Array.Copy(tempBuffer, 0, outBuffer, destinationIndex, sourceBytesPerPixelSet);
            }
            return outBuffer;
        }
    }
}
