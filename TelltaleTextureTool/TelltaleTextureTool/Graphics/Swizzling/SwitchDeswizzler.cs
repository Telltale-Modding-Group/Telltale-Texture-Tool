using System;
using static DrSwizzler.Util;

namespace DrSwizzlerZZZ.Swizzling
{
    internal class SwitchDeswizzler
    {
        public static byte[] SwitchDeswizzle(
            byte[] swizzledData,
            int width,
            int height,
            int sourceBytesPerPixelSet,
            int pixelBlockSize,
            int formatbpp
        )
        {
            //If it's not long enough, return as is
            if (sourceBytesPerPixelSet >= swizzledData.Length)
            {
                return swizzledData;
            }

            int calculatedBufferSize = (formatbpp * width * height) / 8;
            byte[] outBuffer = new byte[
                calculatedBufferSize > sourceBytesPerPixelSet
                    ? calculatedBufferSize
                    : sourceBytesPerPixelSet
            ];
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

                                Array.Copy(
                                    swizzledData,
                                    streamPos,
                                    tempBuffer,
                                    0,
                                    sourceBytesPerPixelSet
                                );
                                streamPos += sourceBytesPerPixelSet;
                                int index6 = (index1 * num7 + index3) * 8 + num11;
                                int index7 = (index2 * 4 + num12) * num9 + index5;
                                int destinationIndex =
                                    sourceBytesPerPixelSet * (index6 * sx + index7);
                                Array.Copy(
                                    tempBuffer,
                                    0,
                                    outBuffer,
                                    destinationIndex,
                                    sourceBytesPerPixelSet
                                );
                                numArray[index7, index6] = num8;
                                ++num8;
                            }
                        }
                    }
                }
            }

            return outBuffer;
        }

        private static byte[] ConvertSwitch(
            byte[] inputImageData,
            int imgWidth,
            int imgHeight,
            int bytesPerBlock,
            int blockHeight,
            int widthPad,
            int heightPad,
            bool swizzleFlag
        )
        {
            int widthShow,
                heightShow,
                widthReal,
                heightReal;

            if (imgWidth % widthPad != 0 || imgHeight % heightPad != 0)
            {
                widthShow = imgWidth;
                heightShow = imgHeight;
                widthReal = ((imgWidth + widthPad - 1) / widthPad) * widthPad;
                heightReal = ((imgHeight + heightPad - 1) / heightPad) * heightPad;
                imgWidth = widthReal;
                imgHeight = heightReal;
            }
            else
            {
                widthShow = widthReal = imgWidth;
                heightShow = heightReal = imgHeight;
            }

            int imageWidthInGobs = imgWidth * bytesPerBlock / 64;
            byte[] convertedData = new byte[inputImageData.Length];

            for (int Y = 0; Y < imgHeight; Y++)
            {
                for (int X_pixel = 0; X_pixel < imgWidth; X_pixel++)
                {
                    int Z = Y * imgWidth + X_pixel;

                    // Calculate GOB address
                    int gobAddress = 0;
                    gobAddress += (Y / (8 * blockHeight)) * 512 * blockHeight * imageWidthInGobs;
                    gobAddress += (X_pixel * bytesPerBlock / 64) * 512 * blockHeight;
                    gobAddress += ((Y % (8 * blockHeight)) / 8) * 512;

                    int X_bytes = X_pixel * bytesPerBlock;
                    int address = gobAddress;
                    address += ((X_bytes % 64) / 32) * 256;
                    address += ((Y % 8) / 2) * 64;
                    address += ((X_bytes % 32) / 16) * 32;
                    address += (Y % 2) * 16;
                    address += X_bytes % 16;

                    // Copy data
                    if (!swizzleFlag)
                    {
                        // Unswizzle: Copy from inputImageData[address] to convertedData[Z * bytesPerBlock]
                        Buffer.BlockCopy(
                            inputImageData,
                            address,
                            convertedData,
                            Z * bytesPerBlock,
                            bytesPerBlock
                        );
                    }
                    else
                    {
                        // Swizzle: Copy from inputImageData[Z * bytesPerBlock] to convertedData[address]
                        Buffer.BlockCopy(
                            inputImageData,
                            Z * bytesPerBlock,
                            convertedData,
                            address,
                            bytesPerBlock
                        );
                    }
                }
            }

            // Crop if needed
            if (widthShow != widthReal || heightShow != heightReal)
            {
                byte[] crop = new byte[widthShow * heightShow * bytesPerBlock];
                for (int Y = 0; Y < heightShow; Y++)
                {
                    int offsetIn = Y * widthReal * bytesPerBlock;
                    int offsetOut = Y * widthShow * bytesPerBlock;

                    if (
                        offsetIn + widthShow * bytesPerBlock <= convertedData.Length
                        && offsetOut + widthShow * bytesPerBlock <= crop.Length
                    )
                    {
                        if (!swizzleFlag)
                        {
                            Buffer.BlockCopy(
                                convertedData,
                                offsetIn,
                                crop,
                                offsetOut,
                                widthShow * bytesPerBlock
                            );
                        }
                        else
                        {
                            Buffer.BlockCopy(
                                convertedData,
                                offsetOut,
                                crop,
                                offsetIn,
                                widthShow * bytesPerBlock
                            );
                        }
                    }
                }
                convertedData = crop;
            }

            return convertedData;
        }

        public static byte[] UnswizzleSwitch(
            byte[] inputImageData,
            int imgWidth,
            int imgHeight,
            int bytesPerBlock = 4,
            int blockHeight = 8,
            int widthPad = 8,
            int heightPad = 8
        )
        {
            return ConvertSwitch(
                inputImageData,
                imgWidth,
                imgHeight,
                bytesPerBlock,
                blockHeight,
                widthPad,
                heightPad,
                false
            );
        }

        public static byte[] SwizzleSwitch(
            byte[] inputImageData,
            int imgWidth,
            int imgHeight,
            int bytesPerBlock = 4,
            int blockHeight = 8,
            int widthPad = 8,
            int heightPad = 8
        )
        {
            return ConvertSwitch(
                inputImageData,
                imgWidth,
                imgHeight,
                bytesPerBlock,
                blockHeight,
                widthPad,
                heightPad,
                true
            );
        }
    }
}
