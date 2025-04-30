using System;
using static DrSwizzler.Util;

namespace DrSwizzler.Swizzling
{
    internal class PS5Swizzler
    {
        public static byte[] PS5Swizzle(byte[] deswizzledData, int width, int height, int sourceBytesPerPixelSet, int pixelBlockSize, int formatbpp, int minBufferSize = 0x100)
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
            int verticalPixelBlockCount = height / pixelBlockSize;
            int horizontalPixelBlockCount = width / pixelBlockSize;
            int num7 = 1;
            if (sourceBytesPerPixelSet == 16)
                num7 = 1;
            if (sourceBytesPerPixelSet == 8)
                num7 = 2;
            if (sourceBytesPerPixelSet == 4)
                num7 = 4;

            int streamPos = 0;
            if (pixelBlockSize == 1)
            {
                for (int index1 = 0; index1 < (verticalPixelBlockCount + (int)sbyte.MaxValue) / 128; ++index1)
                {
                    for (int index2 = 0; index2 < (horizontalPixelBlockCount + (int)sbyte.MaxValue) / 128; ++index2)
                    {
                        for (int t = 0; t < 512; ++t)
                        {
                            int num8 = Morton(t, 32, 16);
                            int num9 = num8 % 32;
                            int num10 = num8 / 32;
                            for (int index3 = 0; index3 < 32 && streamPos + 0x10 < outBuffer.Length; ++index3)
                            {
                                int currentHorizontalPixelBlock = index2 * 128 + num9 * 4 + index3 % 4;
                                int currentVerticalPixelBlock = index1 * 128 + (num10 * 8 + index3 / 4);
                                if (currentHorizontalPixelBlock < horizontalPixelBlockCount && currentVerticalPixelBlock < verticalPixelBlockCount)
                                {
                                    int sourceIndex = sourceBytesPerPixelSet * (currentVerticalPixelBlock * horizontalPixelBlockCount + currentHorizontalPixelBlock);
                                    Array.Copy(deswizzledData, sourceIndex, tempBuffer, 0, sourceBytesPerPixelSet);
                                    Array.Copy(tempBuffer, 0, outBuffer, streamPos, sourceBytesPerPixelSet);
                                }
                                streamPos += sourceBytesPerPixelSet;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int index1 = 0; index1 < (verticalPixelBlockCount + 63) / 64; ++index1)
                {
                    for (int index2 = 0; index2 < (horizontalPixelBlockCount + 63) / 64; ++index2)
                    {
                        for (int t = 0; t < 256 / num7; ++t)
                        {
                            int num8 = Morton(t, 16, 16 / num7);
                            int num9 = num8 / 16;
                            int num10 = num8 % 16;
                            for (int index3 = 0; index3 < 16; ++index3)
                            {
                                for (int index4 = 0; index4 < num7 && streamPos + 0x10 < outBuffer.Length; ++index4)
                                {
                                    int currentHorizontalPixelBlock = index2 * 64 + (num9 * 4 + index3 / 4) * num7 + index4;
                                    int currentVerticalPixelBlock = index1 * 64 + num10 * 4 + index3 % 4;
                                    if (currentHorizontalPixelBlock < horizontalPixelBlockCount && currentVerticalPixelBlock < verticalPixelBlockCount)
                                    {
                                        int sourceIndex = sourceBytesPerPixelSet * (currentVerticalPixelBlock * horizontalPixelBlockCount + currentHorizontalPixelBlock);
                                        Array.Copy(deswizzledData, sourceIndex, tempBuffer, 0, sourceBytesPerPixelSet);
                                        Array.Copy(tempBuffer, 0, outBuffer, streamPos, sourceBytesPerPixelSet);
                                    }
                                    streamPos += sourceBytesPerPixelSet;
                                }
                            }
                        }
                    }
                }
            }

            return outBuffer;
        }

    }
}
