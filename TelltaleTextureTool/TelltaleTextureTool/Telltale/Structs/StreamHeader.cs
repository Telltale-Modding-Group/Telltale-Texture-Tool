using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TelltaleTextureTool.TelltaleTypes;

public struct StreamHeader
{
    public int mRegionCount;
    public int mAuxDataCount;
    public int mTotalDataSize;

    public StreamHeader(BinaryReader reader)
    {
        mRegionCount = reader.ReadInt32(); // mRegionCount [4 bytes]
        mAuxDataCount = reader.ReadInt32(); // mAuxDataCount [4 bytes]
        mTotalDataSize = reader.ReadInt32(); // mTotalDataSize [4 bytes]
    }

    public readonly void WriteBinaryData(BinaryWriter writer)
    {
        writer.Write(mRegionCount); // mRegionCount [4 bytes]
        writer.Write(mAuxDataCount); // mAuxDataCount [4 bytes]
        writer.Write(mTotalDataSize); // mTotalDataSize [4 bytes]
    }

    public readonly uint GetByteSize()
    {
        uint totalByteSize = 0;

        totalByteSize += (uint)Marshal.SizeOf(mRegionCount); // mRegionCount [4 bytes]
        totalByteSize += (uint)Marshal.SizeOf(mAuxDataCount); // mAuxDataCount [4 bytes]
        totalByteSize += (uint)Marshal.SizeOf(mTotalDataSize); // mTotalDataSize [4 bytes]

        return totalByteSize;
    }

    public override readonly string ToString()
    {
        StringBuilder streamHeader = new();

        streamHeader.AppendFormat(
            "[RegionStreamHeader] mRegionCount: {0}, mAuxDataCount: {1}, mTotalDataSize: {2}",
            mRegionCount,
            mAuxDataCount,
            mTotalDataSize
        );

        return streamHeader.ToString();
    }
}
