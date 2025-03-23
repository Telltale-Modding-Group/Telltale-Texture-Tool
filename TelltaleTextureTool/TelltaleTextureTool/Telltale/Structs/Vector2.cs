using System.IO;
using System.Runtime.InteropServices;

namespace TelltaleTextureTool.TelltaleTypes;

public struct Vector2
{
    public float x;
    public float y;

    public Vector2(BinaryReader reader)
    {
        x = reader.ReadSingle();
        y = reader.ReadSingle();
    }

    public readonly void WriteBinaryData(BinaryWriter writer)
    {
        writer.Write(x);
        writer.Write(y);
    }

    public readonly uint GetByteSize()
    {
        uint totalByteSize = 0;

        totalByteSize += (uint)Marshal.SizeOf(x); // x [4 bytes]
        totalByteSize += (uint)Marshal.SizeOf(y); // y [4 bytes]

        return totalByteSize;
    }

    public override readonly string ToString() => string.Format("[Vector2] x: {0} y: {1}", x, y);
}
