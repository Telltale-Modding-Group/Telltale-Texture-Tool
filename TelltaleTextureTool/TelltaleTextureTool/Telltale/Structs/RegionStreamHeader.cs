using System;
using System.Text;

namespace TelltaleTextureTool.TelltaleTypes;

public class RegionStreamHeader
{
    // mVersion >= 5
    public int mFaceIndex { get; set; }

    // mVersion >= 3
    public int mMipIndex { get; set; }

    // mVersion >= 4
    public int mMipCount { get; set; }

    // mVersion >= 3
    public uint mDataSize { get; set; }

    // mVersion >= 3
    public int mPitch { get; set; }

    // mVersion >= 9
    public int mSlicePitch { get; set; }
}
