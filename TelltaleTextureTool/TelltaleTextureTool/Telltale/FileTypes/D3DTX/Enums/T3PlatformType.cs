using System;
using System.ComponentModel.DataAnnotations;

namespace TelltaleTextureTool.TelltaleEnums;

public enum T3PlatformType
{
    [Display(Name = "Default Mode")]
    ePlatform_None = 0,

    [Display(Name = "Unknown Platform")]
    ePlatform_All = 1,

    [Display(Name = "PC")]
    ePlatform_PC = 2,

    [Display(Name = "Wii")]
    ePlatform_Wii = 3,

    [Display(Name = "Xbox 360")]
    ePlatform_Xbox = 4,

    [Display(Name = "PS3")]
    ePlatform_PS3 = 5,

    [Display(Name = "Mac")]
    ePlatform_Mac = 6,

    [Display(Name = "iPhone")]
    ePlatform_iPhone = 7,

    [Display(Name = "Android")]
    ePlatform_Android = 8,

    [Display(Name = "PS Vita")]
    ePlatform_Vita = 9,

    [Display(Name = "Linux")]
    ePlatform_Linux = 10,

    [Display(Name = "PS4")]
    ePlatform_PS4 = 11,

    [Display(Name = "Xbox One")]
    ePlatform_XBOne = 12,

    [Display(Name = "Wii U")]
    ePlatform_WiiU = 13,

    [Display(Name = "Windows 10")]
    ePlatform_Win10 = 14,

    [Display(Name = "Nintendo Switch")]
    ePlatform_NX = 15,
    ePlatform_Count = 16,
}
