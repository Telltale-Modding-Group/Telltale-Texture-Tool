using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using TelltaleTextureTool.TelltaleEnums;

namespace TelltaleTextureTool.TelltaleTypes;

public struct T3SamplerStateBlock
{
    public uint mData;

    public T3SamplerStateBlock(BinaryReader reader)
    {
        mData = reader.ReadUInt32(); // mSamplerState [4 bytes]
    }

    public readonly uint GetByteSize()
    {
        uint totalByteSize = 0;

        totalByteSize += (uint)Marshal.SizeOf(mData); // mData [4 bytes]

        return totalByteSize;
    }

    public override readonly string ToString()
    {
        StringBuilder enumFlags = new();

        var allEnums = Enum.GetValues(typeof(T3SamplerStateValue));

        foreach (var enumMask in allEnums)
        {
            if ((mData & (uint)(T3SamplerStateValue)enumMask) != 0)
            {
                enumFlags.AppendFormat(
                    "{0}: {1} | ",
                    Enum.GetName((T3SamplerStateValue)enumMask),
                    mData & (uint)(T3SamplerStateValue)enumMask
                );
            }
        }

        if (enumFlags.Length > 0)
        {
            enumFlags.Remove(enumFlags.Length - 3, 3);
        }
        else
        {
            enumFlags.Append("None");
        }

        return string.Format("[T3SamplerStateBlock] mData: {0} ({1})", enumFlags.ToString(), mData);
    }
}
