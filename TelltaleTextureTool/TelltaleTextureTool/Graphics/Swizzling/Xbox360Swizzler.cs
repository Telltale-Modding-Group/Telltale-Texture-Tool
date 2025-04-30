using System;
using System.IO;
using static DrSwizzler.DDS.DXEnums;

namespace DrSwizzler.Swizzling
{
    /// <summary>
    /// Borrowed and edited from https://github.com/emoose/FtexTool/blob/6874f4fb9c2e9c18d1dd01f7a877ed2cb78cf75e/FtexTool/FtexDdsConverter.cs
    /// Thank you to emoose!
    /// </summary>
    internal class Xbox360Swizzler
    {
        public static byte[] Swizzle(byte[] data, int width, int height, int numMipMaps, DXGIFormat pixelFormat, int sourceBytesPerPixelSet, int pixelBlockSize, int formatbpp)
        {
            //If it's not long enough, return as is
            if (sourceBytesPerPixelSet >= data.Length)
            {
                return data;
            }

            Xbox360Deswizzler.GetXbox360Align(pixelFormat, out int X360AlignX, out int X360AlignY);
            int width1 = Align(width, X360AlignX);
            int height1 = Align(height, X360AlignY);

            int size = (width1 / pixelBlockSize) * (height1 / pixelBlockSize) * sourceBytesPerPixelSet;

            byte[] mipMapData = new byte[size];
            Array.Copy(data, 0, mipMapData, 0, Math.Min(data.Length, mipMapData.Length));
            return TileCompressedX360Texture(mipMapData, width1, width, height1, pixelBlockSize, pixelBlockSize, sourceBytesPerPixelSet);
        }

        public static byte[] TileCompressedX360Texture(byte[] data, int tiledWidth, int originalWidth, int height, int blockSizeX, int blockSizeY, int bytesPerBlock)
        {
            MemoryStream ms = new MemoryStream(data);
            MemoryStream output = new MemoryStream(data.Length);
            output.SetLength(data.Length);
            BinaryReader reader = new BinaryReader(ms);
            BinaryWriter writer = new BinaryWriter(output);

            int blockWidth = tiledWidth / blockSizeX;
            int originalBlockWidth = originalWidth / blockSizeX;
            int blockHeight = height / blockSizeY;
            int logBpp = appLog2(bytesPerBlock);

            for (int y = 0; y < blockHeight; y++)
            {
                for (int x = 0; x < originalBlockWidth; x++)
                {
                    int addr = GetTiledOffset(x, y, blockWidth, logBpp);

                    int sy = addr / blockWidth;
                    int sx = addr % blockWidth;

                    int srcAddr = (y * originalBlockWidth + x) * bytesPerBlock;
                    int dstAddr = (sy * blockWidth + sx) * bytesPerBlock;

                    reader.BaseStream.Position = srcAddr;
                    byte[] data1 = reader.ReadBytes(bytesPerBlock);
                    writer.BaseStream.Position = dstAddr;
                    writer.Write(data1);
                }
            }
            return output.ToArray();
        }

        static uint Align(uint ptr, uint alignment)
        {
            return ((ptr + alignment - 1) & ~(alignment - 1));
        }
        static int Align(int ptr, int alignment)
        {
            return ((ptr + alignment - 1) & ~(alignment - 1));
        }

        static int appLog2(int n)
        {
            int r;
            int n2 = n;
            for (r = -1; n2 != 0; n2 >>= 1, r++)
            { /*empty*/ }
            return r;
        }

        // Input:
        //		x/y		coordinate of block
        //		width	width of image in blocks
        //		logBpb	log2(bytesPerBlock)
        // Reference:
        //		XGAddress2DTiledOffset() from XDK
        static int GetTiledOffset(int x, int y, int width, int logBpb)
        {
            int alignedWidth = Align(width, 32);
            // top bits of coordinates
            int macro = ((x >> 5) + (y >> 5) * (alignedWidth >> 5)) << (logBpb + 7);
            // lower bits of coordinates (result is 6-bit value)
            int micro = ((x & 7) + ((y & 0xE) << 2)) << logBpb;
            // mix micro/macro + add few remaining x/y bits
            int offset = macro + ((micro & ~0xF) << 1) + (micro & 0xF) + ((y & 1) << 4);
            // mix bits again
            return (((offset & ~0x1FF) << 3) +                  // upper bits (offset bits [*-9])
                    ((y & 16) << 7) +                           // next 1 bit
                    ((offset & 0x1C0) << 2) +                   // next 3 bits (offset bits [8-6])
                    (((((y & 8) >> 2) + (x >> 3)) & 3) << 6) +  // next 2 bits
                    (offset & 0x3F)                             // lower 6 bits (offset bits [5-0])
                    ) >> logBpb;
        }
    }
}
