using System.IO;
using System.Runtime.InteropServices;

namespace TelltaleTextureTool.TelltaleTypes;

public struct Symbol
{
    public ulong mCrc64;

    public Symbol(BinaryReader reader)
    {
        mCrc64 = reader.ReadUInt64();
    }

    public readonly void WriteBinaryData(BinaryWriter writer)
    {
        writer.Write(mCrc64);
    }

    public readonly uint GetByteSize()
    {
        uint totalByteSize = 0;

        totalByteSize += (uint)Marshal.SizeOf(mCrc64); //mCrc64 [8 bytes]

        return totalByteSize;
    }

    public override readonly string ToString() => string.Format("[Symbol] {0}", mCrc64);
}
