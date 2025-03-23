using System.IO;
using System.Runtime.InteropServices;

namespace TelltaleTextureTool.TelltaleTypes;

/// <summary>
/// This is a class name struct used in a meta header.
/// This contains a CRC64'd string of a class name used in the file.
/// The CRC64 string of a classname is usually all lowercase, and uses
/// </summary>
public struct ClassNames
{
    public Symbol mTypeNameCRC;
    public uint mVersionCRC;

    public ClassNames(BinaryReader reader)
    {
        mTypeNameCRC = new Symbol(reader);
        mVersionCRC = reader.ReadUInt32();
    }

    public readonly void WriteBinaryData(BinaryWriter writer)
    {
        mTypeNameCRC.WriteBinaryData(writer);
        writer.Write(mVersionCRC);
    }

    public readonly uint GetByteSize()
    {
        uint totalByteSize = 0;

        totalByteSize += mTypeNameCRC.GetByteSize(); //mTypeNameCRC
        totalByteSize += (uint)Marshal.SizeOf(mVersionCRC); //mVersionCRC [4 bytes]

        return totalByteSize;
    }

    public override readonly string ToString() =>
        string.Format(
            "[MetaClass] mTypeNameCRC: {0}, mVersionCRC: {1}",
            mTypeNameCRC.ToString(),
            mVersionCRC
        );
}
