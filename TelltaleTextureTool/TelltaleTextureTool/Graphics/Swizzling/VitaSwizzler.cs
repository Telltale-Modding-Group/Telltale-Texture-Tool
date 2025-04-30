using System;

namespace DrSwizzler.Swizzling
{
    internal class VitaSwizzler
    {
        public static byte[] VitaSwizzle(byte[] deswizzledData, int width, int height, int sourceBytesPerPixelSet, int formatbpp, int minBufferSize = 0)
        {
            //If it's not long enough, return as is
            if (sourceBytesPerPixelSet >= deswizzledData.Length)
            {
                return Util.ExpandArray(deswizzledData, minBufferSize);
            }
            deswizzledData = Util.ExpandArray(deswizzledData, minBufferSize); 

            int calculatedBufferSize = (formatbpp * width * height) / 8;
            if (minBufferSize > calculatedBufferSize)
            {
                calculatedBufferSize = minBufferSize;
            }
            byte[] swizzledData = new byte[calculatedBufferSize > sourceBytesPerPixelSet ? calculatedBufferSize : sourceBytesPerPixelSet];

            int maxU = (int)(Math.Log(width, 2));
            int maxV = (int)(Math.Log(height, 2));

            for (int j = 0; (j < width * height) && (j * sourceBytesPerPixelSet < deswizzledData.Length); j++)
            {
                int u = 0, v = 0;
                int origCoord = j;
                for (int k = 0; k < maxU || k < maxV; k++)
                {
                    if (k < maxV)
                    {
                        v |= (origCoord & 1) << k;
                        origCoord >>= 1;
                    }
                    if (k < maxU)
                    {
                        u |= (origCoord & 1) << k;
                        origCoord >>= 1;
                    }
                }
                if (u < width && v < height)
                {
                    Array.Copy(deswizzledData, (v * width + u) * sourceBytesPerPixelSet, swizzledData, j * sourceBytesPerPixelSet, sourceBytesPerPixelSet);
                }
            }
            return swizzledData;
        }
    }
}
