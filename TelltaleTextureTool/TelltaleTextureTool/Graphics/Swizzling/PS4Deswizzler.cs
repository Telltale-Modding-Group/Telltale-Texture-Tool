using System;
using static DrSwizzler.Util;

namespace DrSwizzler.Swizzling
{
    internal class PS4Deswizzler
    {
        public static byte[] PS4Deswizzle(byte[] swizzledData, int width, int height, int sourceBytesPerPixelSet, int pixelBlockSize, int formatbpp)
        {
            //If it's not long enough, return as is
            if(sourceBytesPerPixelSet >= swizzledData.Length)
            {
                return swizzledData;
            }

            int calculatedBufferSize = (formatbpp * width * height) / 8;
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

                        var byteLimit = (swizzledData.Length - sourceBytesPerPixelSet);
                        if (streamPos > byteLimit)
                        {
                            return outBuffer;
                        }
                        Array.Copy(swizzledData, streamPos, tempBuffer, 0, sourceBytesPerPixelSet);
                        streamPos += sourceBytesPerPixelSet;
                        if (index2 * 8 + num9 < sx && index1 * 8 + num8 < sy)
                        {
                            int destinationIndex = sourceBytesPerPixelSet * ((index1 * 8 + num8) * sx + index2 * 8 + num9);
                            Array.Copy((Array)tempBuffer, 0, (Array)outBuffer, destinationIndex, sourceBytesPerPixelSet);
                        }
                    }
                }
            }
            return outBuffer;
        }
    }
}
