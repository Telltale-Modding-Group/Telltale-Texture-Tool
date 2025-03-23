using System.IO;
using TelltaleTextureTool.Utilities;

namespace TelltaleTextureTool.TelltaleTypes;

public struct ToolProps
{
    public bool mbHasProps;

    public ToolProps(BinaryReader reader)
    {
        mbHasProps = ByteFunctions.ReadTelltaleBoolean(reader);
    }

    public static uint GetByteSize()
    {
        uint totalByteSize = 0;

        totalByteSize += 1; // mbHasProps [1 byte]

        return totalByteSize;
    }

    public override readonly string ToString() => string.Format("[ToolProps] mbHasProps: {0}", mbHasProps);
}
