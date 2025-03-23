using System.IO;
using TelltaleTextureTool.Utilities;

namespace TelltaleTextureTool.TelltaleTypes;

public struct TelltaleBoolean
{
    public bool mbTelltaleBoolean;

    public TelltaleBoolean(BinaryReader reader)
    {
        mbTelltaleBoolean = ByteFunctions.ReadTelltaleBoolean(reader);
    }

    public TelltaleBoolean(bool value)
    {
        mbTelltaleBoolean = value;
    }

    public static uint GetByteSize()
    {
        uint totalByteSize = 0;

        totalByteSize += 1; // mbHasProps [1 byte]

        return totalByteSize;
    }

    public override readonly string ToString() => string.Format("{0}", mbTelltaleBoolean);
}
