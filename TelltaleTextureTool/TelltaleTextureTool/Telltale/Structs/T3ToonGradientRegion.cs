using System.IO;
using System.Runtime.InteropServices;

namespace TelltaleTextureTool.TelltaleTypes;

public struct T3ToonGradientRegion
{
    public Color mColor;
    public float mSize;

    public T3ToonGradientRegion(BinaryReader reader)
    {
        mColor = new Color(reader);
        mSize = reader.ReadUInt32();
    }

    public void WriteBinaryData(BinaryWriter writer)
    {
        mColor.WriteBinaryData(writer);
        writer.Write(mSize); //[4 bytes]
    }

    public readonly uint GetByteSize()
    {
        uint totalByteSize = 0;

        totalByteSize += mColor.GetByteSize(); // mColor [4 bytes]
        totalByteSize += (uint)Marshal.SizeOf(mSize); // mSize [4 bytes]

        return totalByteSize;
    }

    public override readonly string ToString() =>
        string.Format("[T3ToonGradientRegion] mColor: {0}, mSize: {1}", mColor.ToString(), mSize);
}
