using System;

namespace TelltaleTextureTool.Graphics.PVR
{
    internal class CTX1Decoder
    {
        public static byte[] DecodeCTX1(byte[] data, int width, int height)
        {
            byte[] DestData = new byte[width * height * 4];

            int dptr = 0;
            for (int i = 0; i < width * height; i += 16)
            {
                int c1 = data[dptr + 1] << 8 | data[dptr];
                int c2 = data[dptr + 3] << 8 | data[dptr + 2];

                RGBAColor[] colorTable = new RGBAColor[4];
                colorTable[0].R = data[dptr];
                colorTable[0].G = data[dptr + 1];
                colorTable[1].R = data[dptr + 2];
                colorTable[1].G = data[dptr + 3];

                if (c1 > c2)
                {
                    colorTable[2] = GradientColors(colorTable[0], colorTable[1]);
                    colorTable[3] = GradientColors(colorTable[1], colorTable[0]);
                }
                else
                {
                    colorTable[2] = GradientColorsHalf(colorTable[0], colorTable[1]);
                    colorTable[3] = colorTable[0];
                }

                int CData =
                    (data[dptr + 5])
                    | (data[dptr + 4] << 8)
                    | (data[dptr + 7] << 16)
                    | (data[dptr + 6] << 24);

                int ChunkNum = i / 16;
                int XPos = ChunkNum % (width / 4);
                int YPos = (ChunkNum - XPos) / (width / 4);

                int sizeH = (height < 4) ? height : 4;
                int sizeW = (width < 4) ? width : 4;

                for (int x = 0; x < sizeH; x++)
                for (int y = 0; y < sizeW; y++)
                {
                    RGBAColor CColor = colorTable[CData & 3];
                    CData >>= 2;

                    int tmp1 = ((YPos * 4 + x) * width + XPos * 4 + y) * 4;

                    float cx = (CColor.R / 255.0f * 2.0f) - 1.0f;
                    float cy = (CColor.G / 255.0f * 2.0f) - 1.0f;
                    float cz = (float)
                        Math.Sqrt(Math.Max(0.0f, Math.Min(1.0f, 1.0f - cx * cx - cy * cy)));

                    DestData[tmp1] = (byte)((cz + 1.0f) / 2.0f * 255.0f); // (255 - (Math.Abs(CColor.R - CColor.G)));
                    DestData[tmp1 + 1] = (byte)CColor.G;
                    DestData[tmp1 + 2] = (byte)CColor.R;
                    DestData[tmp1 + 3] = 255;
                }
                dptr += 8;
            }

            return DestData;
        }

        private struct RGBAColor
        {
            public int R,
                G,
                B,
                A;
        }

        private static RGBAColor GradientColors(RGBAColor Color1, RGBAColor Color2)
        {
            RGBAColor newColor;
            newColor.R = (byte)((Color1.R * 2 + Color2.R) / 3);
            newColor.G = (byte)((Color1.G * 2 + Color2.G) / 3);
            newColor.B = (byte)((Color1.B * 2 + Color2.B) / 3);
            newColor.A = 0xFF;
            return newColor;
        }

        private static RGBAColor GradientColorsHalf(RGBAColor Color1, RGBAColor Color2)
        {
            RGBAColor newColor;
            newColor.R = (byte)(Color1.R / 2 + Color2.R / 2);
            newColor.G = (byte)(Color1.G / 2 + Color2.G / 2);
            newColor.B = (byte)(Color1.B / 2 + Color2.B / 2);
            newColor.A = 0xFF;
            return newColor;
        }
    }
}
