using System;
using static DrSwizzler.Util;

namespace DrSwizzler.Swizzling
{
    internal class PS4Swizzler
    {
        public static byte[] PS4Swizzle(byte[] deswizzledData, int width, int height, int sourceBytesPerPixelSet, int pixelBlockSize, int formatbpp, int minBufferSize = 0x200)
        {
            //If it's not long enough, return as is
            if (sourceBytesPerPixelSet >= deswizzledData.Length)
            {
                return ExpandArray(deswizzledData, minBufferSize);
            }
            deswizzledData = ExpandArray(deswizzledData, minBufferSize);

            int calculatedBufferSize = (formatbpp * width * height) / 8;
            if(minBufferSize > calculatedBufferSize)
            {
                calculatedBufferSize = minBufferSize;
            }
            byte[] outBuffer = new byte[calculatedBufferSize > sourceBytesPerPixelSet ? calculatedBufferSize : sourceBytesPerPixelSet];
            byte[] tempBuffer = new byte[sourceBytesPerPixelSet];
            int sy = height / pixelBlockSize;
            int sx = width / pixelBlockSize;

            int streamPos = 0;
            for (int index1 = 0; index1 < (sy + 7) / 8; ++index1)
            {
                for (int index2 = 0; index2 < (sx + 7) / 8; ++index2)
                {
                    for (int t = 0; t < 64; ++t)
                    {
                        int num7 = Morton(t, 8, 8);
                        int num8 = num7 / 8;
                        int num9 = num7 % 8;

                        var byteLimit = (deswizzledData.Length - sourceBytesPerPixelSet);
                        if (streamPos > byteLimit)
                        {
                            return outBuffer;
                        }
                        if (index2 * 8 + num9 < sx && index1 * 8 + num8 < sy)
                        {
                            int sourceIndex = sourceBytesPerPixelSet * ((index1 * 8 + num8) * sx + index2 * 8 + num9);
                            if(sourceIndex < deswizzledData.Length)
                            {
                                Array.Copy(deswizzledData, sourceIndex, tempBuffer, 0, sourceBytesPerPixelSet);
                                Array.Copy(tempBuffer, 0, outBuffer, streamPos, sourceBytesPerPixelSet);
                            }
                        }
                        streamPos += sourceBytesPerPixelSet;
                    }
                }
            }
            return outBuffer;
        }
    }
}
