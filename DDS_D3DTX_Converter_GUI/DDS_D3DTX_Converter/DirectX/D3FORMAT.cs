namespace D3DTX_Converter.DirectX;

// D3DFORMAT - https://learn.microsoft.com/en-us/windows/win32/direct3d9/d3dformat
// Map Direct3D 9 Formats to Direct3D 10 - https://learn.microsoft.com/en-gb/windows/win32/direct3d10/d3d10-graphics-programming-guide-resources-legacy-formats?redirectedfrom=MSDN

/// <summary>
/// Defines the various types of Direct3D9 surface formats. It also include some DDS FourCC codes.
/// </summary>
public enum D3DFORMAT : uint
{
    UNKNOWN = 0,

    R8G8B8 = 20,
    A8R8G8B8 = 21,
    X8R8G8B8 = 22,
    R5G6B5 = 23,
    X1R5G5B5 = 24,
    A1R5G5B5 = 25,
    A4R4G4B4 = 26,
    R3G3B2 = 27,
    A8 = 28,
    A8R3G3B2 = 29,
    X4R4G4B4 = 30,
    A2B10G10R10 = 31,
    A8B8G8R8 = 32,
    X8B8G8R8 = 33,
    G16R16 = 34,
    A2R10G10B10 = 35,
    A16B16G16R16 = 36,

    A8P8 = 40,
    P8 = 41,

    L8 = 50,
    A8L8 = 51,
    A4L4 = 52,

    V8U8 = 60,
    L6V5U5 = 61,
    X8L8V8U8 = 62,
    Q8W8V8U8 = 63,
    V16U16 = 64,
    A2W10V10U10 = 67,

    UYVY = 0x59565955, // 'UYVY'
    R8G8_B8G8 = 0x47424752, // 'RGBG'
    YUY2 = 0x32595559, // 'YUY2'
    G8R8_G8B8 = 0x42475247, // 'GRGB'
    DXT1 = 0x31545844, // 'DXT1'
    DXT2 = 0x32545844, // 'DXT2'
    DXT3 = 0x33545844, // 'DXT3'
    DXT4 = 0x34545844, // 'DXT4'
    DXT5 = 0x35545844, // 'DXT5'
    ATI1 = 0x31495441, // 'ATI1'
    ATI2 = 0x32495441, // 'ATI2'
    BC4S = 0x42433453, // 'BC4S'
    BC5S = 0x42433553, // 'BC4S'

    D16_LOCKABLE = 70,
    D32 = 71,
    D15S1 = 73,
    D24S8 = 75,
    D24X8 = 77,
    D24X4S4 = 79,
    D16 = 80,

    D32F_LOCKABLE = 82,
    D24FS8 = 83,

    D32_LOCKABLE = 84,
    S8_LOCKABLE = 85,

    L16 = 81,

    VERTEXDATA = 100,
    INDEX16 = 101,
    INDEX32 = 102,

    Q16W16V16U16 = 110,

    MULTI2_ARGB8 = 0x3145544D, // 'MET1'

    R16F = 111,
    G16R16F = 112,
    A16B16G16R16F = 113,

    R32F = 114,
    G32R32F = 115,
    A32B32G32R32F = 116,

    CxV8U8 = 117,

    A1 = 118,
    A2B10G10R10_XR_BIAS = 119,
    BINARYBUFFER = 199,

    AI44 = 0x34344941, // 'AI44'
    IA44 = 0x34344149, // 'IA44'
    YV12 = 0x32315659, // 'YV12'

    FORCE_DWORD = 0x7fffffff
}
