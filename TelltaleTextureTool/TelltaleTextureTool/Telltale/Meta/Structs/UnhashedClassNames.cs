using System.IO;
using System.Runtime.InteropServices;

namespace TelltaleTextureTool.TelltaleTypes;

/// <summary>
/// This is a class name struct used in a meta header.
/// This contains a CRC64'd string of a class name used in the file.
/// The CRC64 string of a classname is usually all lowercase, and uses
/// </summary>
public struct UnhashedClassNames
{
    public uint length;
    public string className;
    public uint mVersionCRC;

    public UnhashedClassNames(BinaryReader reader)
    {
        length = reader.ReadUInt32();
        className = new string(reader.ReadChars((int)length));
        mVersionCRC = reader.ReadUInt32();
    }

    public readonly void WriteBinaryData(BinaryWriter writer)
    {
        writer.Write(length);
        writer.Write(className.ToCharArray());
        writer.Write(mVersionCRC);
    }

    public readonly uint GetByteSize()
    {
        uint totalByteSize = 0;

        totalByteSize += (uint)Marshal.SizeOf(length); //length [4 bytes]
        totalByteSize += (uint)Marshal.SizeOf(className.ToCharArray()); //className
        totalByteSize += (uint)Marshal.SizeOf(mVersionCRC); //mVersionCRC [4 bytes]

        return totalByteSize;
    }

    public override readonly string ToString() =>
        string.Format(
            "[UnhashedMetaClass] ClassName: {0}, mVersionCRC: {1}",
            className,
            mVersionCRC
        );
}
