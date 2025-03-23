using System;
using System.ComponentModel.DataAnnotations;

namespace TelltaleTextureTool.TelltaleEnums;

// These are used as masks.
// In Telltale the enum is represented using Hungarian notation (eSamplerState_filterName_Value).
[Flags]
public enum T3SamplerStateValue
{
    WrapU = 0xF, // 15
    WrapV = 0xF0, // 240
    Filtered = 0x100, // 256
    BorderColor = 0x1E00, // 7680
    GammaCorrect = 0x2000, // 8192
    MipBias = 0x3FC000, // 4177920
    Count = 0x6,
}
