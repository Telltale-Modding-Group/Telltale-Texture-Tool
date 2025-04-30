using System;
using static DrSwizzler.Util;

namespace DrSwizzler.Swizzling
{
    internal class PS5Deswizzler
    {
        public static byte[] PS5Deswizzle(byte[] swizzledData, int width, int height, int sourceBytesPerPixelSet, int pixelBlockSize, int formatbpp)
        {
            //If it's not long enough, return as is
            if (sourceBytesPerPixelSet >= swizzledData.Length)
            {
                return swizzledData;
            }

            int calculatedBufferSize = (formatbpp * width * height) / 8;
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
                            for (int index3 = 0; index3 < 32 && streamPos + 0x10 < swizzledData.Length; ++index3)
                            {
                                Array.Copy(swizzledData, streamPos, tempBuffer, 0, sourceBytesPerPixelSet);
                                streamPos += sourceBytesPerPixelSet;
                                int currentHorizontalPixelBlock = index2 * 128 + num9 * 4 + index3 % 4;
                                int currentVerticalPixelBlock = index1 * 128 + (num10 * 8 + index3 / 4);
                                if (currentHorizontalPixelBlock < horizontalPixelBlockCount && currentVerticalPixelBlock < verticalPixelBlockCount)
                                {
                                    int destinationIndex = sourceBytesPerPixelSet * (currentVerticalPixelBlock * horizontalPixelBlockCount + currentHorizontalPixelBlock);
                                    Array.Copy(tempBuffer, 0, outBuffer, destinationIndex, sourceBytesPerPixelSet);
                                }
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
                                for (int index4 = 0; index4 < num7 && streamPos + 0x10 < swizzledData.Length; ++index4)
                                {
                                    Array.Copy(swizzledData, streamPos, tempBuffer, 0, sourceBytesPerPixelSet);
                                    streamPos += sourceBytesPerPixelSet;
                                    int currentHorizontalPixelBlock = index2 * 64 + (num9 * 4 + index3 / 4) * num7 + index4;
                                    int currentVerticalPixelBlock = index1 * 64 + num10 * 4 + index3 % 4;
                                    if (currentHorizontalPixelBlock < horizontalPixelBlockCount && currentVerticalPixelBlock < verticalPixelBlockCount)
                                    {
                                        int destinationIndex = sourceBytesPerPixelSet * (currentVerticalPixelBlock * horizontalPixelBlockCount + currentHorizontalPixelBlock);
                                        Array.Copy(tempBuffer, 0, outBuffer, destinationIndex, sourceBytesPerPixelSet);
                                    }
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
