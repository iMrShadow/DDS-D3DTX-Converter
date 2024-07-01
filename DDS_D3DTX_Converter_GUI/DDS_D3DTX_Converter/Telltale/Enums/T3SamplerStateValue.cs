using System;

namespace D3DTX_Converter.TelltaleEnums;

// These are used as masks.
// Original enum structure was eSamplerState_[State]_Value
[Flags]
public enum T3SamplerStateValue
{
    WrapU_Value = 0xF, // 15 
    WrapV_Value = 0xF0, // 240
    Filtered_Value = 0x100, // 256
    BorderColor_Value = 0x1E00, // 7680
    GammaCorrect_Value = 0x2000, // 8192
    MipBias_Value = 0x3FC000, // 4177920
    Count = 0x6
}
