using System;
using System.IO;
using TelltaleTextureTool.Utilities;

namespace TelltaleTextureTool.TelltaleTypes;

public struct TelltalePixelData
{
    public uint length;
    public byte[] pixelData;

    public TelltalePixelData(BinaryReader reader, bool IsEncrypted = false)
    {
        length = reader.ReadUInt32();

        if (length > reader.BaseStream.Length - reader.BaseStream.Position)
        {
            throw new Exception("Pixel data length is larger than the file size.");
        }

        pixelData = reader.ReadBytes((int)length);

        if (pixelData.Length != length)
        {
            throw new Exception(
                "Pixel data length does not match the length specified in the header."
            );
        }
    }

    public TelltalePixelData(BinaryReader reader, int skip)
    {
        length = reader.ReadUInt32();
        reader.BaseStream.Position += skip;
        pixelData = reader.ReadBytes((int)length - skip);
    }

    public void WriteBinaryData(BinaryWriter writer)
    {
        writer.Write(length);
        writer.Write(pixelData);
    }

    public TelltalePixelData(byte[] ddsData, int skippedOriginalBytes = 0, int skippedDDSBytes = 0)
    {
        if (skippedOriginalBytes > ddsData.Length || skippedDDSBytes > ddsData.Length)
            throw new Exception("One of the parameters is larger than the data size.");

        byte[] copyBuffer = new byte[skippedOriginalBytes];

        Array.Copy(pixelData, 0, copyBuffer, 0, skippedOriginalBytes);

        byte[] copyDDSBuffer = new byte[ddsData.Length - skippedDDSBytes];

        Array.Copy(ddsData, skippedDDSBytes, copyDDSBuffer, 0, ddsData.Length - skippedDDSBytes);

        pixelData = ByteFunctions.Combine(copyBuffer, copyDDSBuffer);
        length = (uint)pixelData.Length;
    }

    public readonly uint GetByteSize()
    {
        uint totalByteSize = 0;

        totalByteSize += sizeof(uint); // length [4 bytes]
        totalByteSize += (uint)pixelData.Length; // pixelData [n bytes]

        return totalByteSize;
    }

    public override readonly string ToString() => string.Format("Pixel Data: {0} bytes", length);
}
