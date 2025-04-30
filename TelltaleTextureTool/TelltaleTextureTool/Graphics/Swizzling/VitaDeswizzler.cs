using System;

namespace DrSwizzler.Swizzling
{
    internal class VitaDeswizzler
    {
        /// <summary>
        /// Massive credit to Agrajag for Vita Deswizzling
        /// </summary>
        public static byte[] VitaDeswizzle(byte[] swizzledData, int width, int height, int sourceBytesPerPixelSet, int formatbpp)
        {
            //If it's not long enough, return as is
            if (sourceBytesPerPixelSet >= swizzledData.Length)
            {
                return swizzledData;
            }

            int calculatedBufferSize = (formatbpp * width * height) / 8;
            byte[] unswizzledData = new byte[calculatedBufferSize > sourceBytesPerPixelSet ? calculatedBufferSize : sourceBytesPerPixelSet];

            int maxU = (int)(Math.Log(width, 2));
            int maxV = (int)(Math.Log(height, 2));

            for (int j = 0; (j < width * height) && (j * sourceBytesPerPixelSet < swizzledData.Length); j++)
            {
                int u = 0, v = 0;
                int origCoord = j;
                for (int k = 0; k < maxU || k < maxV; k++)
                {
                    if (k < maxV)   //Transpose!
                    {
                        v |= (origCoord & 1) << k;
                        origCoord >>= 1;
                    }
                    if (k < maxU)   //Transpose!
                    {
                        u |= (origCoord & 1) << k;
                        origCoord >>= 1;
                    }
                }
                if (u < width && v < height)
                {
                    Array.Copy(swizzledData, j * sourceBytesPerPixelSet, unswizzledData, (v * width + u) * sourceBytesPerPixelSet, sourceBytesPerPixelSet);
                }

            }
            return unswizzledData;
        }
    }
}
