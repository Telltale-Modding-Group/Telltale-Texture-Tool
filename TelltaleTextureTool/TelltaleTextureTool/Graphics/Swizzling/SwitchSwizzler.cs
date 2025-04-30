using System;
using static DrSwizzler.Util;

namespace DrSwizzler.Swizzling
{
    internal class SwitchSwizzler
    {
        public static byte[] SwitchSwizzle(byte[] deswizzledData, int width, int height, int sourceBytesPerPixelSet, int pixelBlockSize, int formatbpp, int minBufferSize = 0)
        {
            //If it's not long enough, return as is
            if (sourceBytesPerPixelSet >= deswizzledData.Length)
            {
                return ExpandArray(deswizzledData, minBufferSize);
            }
            deswizzledData = ExpandArray(deswizzledData, minBufferSize);

            int calculatedBufferSize = (formatbpp * width * height) / 8;
            if (minBufferSize > calculatedBufferSize)
            {
                calculatedBufferSize = minBufferSize;
            }
            byte[] outBuffer = new byte[calculatedBufferSize > sourceBytesPerPixelSet ? calculatedBufferSize : sourceBytesPerPixelSet];
            byte[] tempBuffer = new byte[sourceBytesPerPixelSet];
            int sy = height / pixelBlockSize;
            int sx = width / pixelBlockSize;
            int[,] numArray = new int[sx * 2, sy * 2];
            int num7 = sy / 8;
            if (num7 > 16)
                num7 = 16;
            int num8 = 0;
            int num9 = 1;
            if (sourceBytesPerPixelSet == 16)
                num9 = 1;
            if (sourceBytesPerPixelSet == 8)
                num9 = 2;
            if (sourceBytesPerPixelSet == 4)
                num9 = 4;

            int streamPos = 0;
            for (int index1 = 0; index1 < sy / 8 / num7; ++index1)
            {
                for (int index2 = 0; index2 < sx / 4 / num9; ++index2)
                {
                    for (int index3 = 0; index3 < num7; ++index3)
                    {
                        for (int index4 = 0; index4 < 32; ++index4)
                        {
                            for (int index5 = 0; index5 < num9; ++index5)
                            {
                                int num10 = swi[index4];
                                int num11 = num10 / 4;
                                int num12 = num10 % 4;

                                int index6 = (index1 * num7 + index3) * 8 + num11;
                                int index7 = (index2 * 4 + num12) * num9 + index5;
                                int sourceIndex = sourceBytesPerPixelSet * (index6 * sx + index7);

                                Array.Copy(deswizzledData, sourceIndex, tempBuffer, 0, sourceBytesPerPixelSet);
                                Array.Copy(tempBuffer, 0, outBuffer, streamPos, sourceBytesPerPixelSet);
                                streamPos += sourceBytesPerPixelSet;
                            }
                        }
                    }
                }
            }

            return outBuffer;
        }
    }
}
