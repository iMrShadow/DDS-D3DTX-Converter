namespace TelltaleTextureTool.DirectX.Enums;

// DDS Docs - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header
// DDS PIXEL FORMAT - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-pixelformat
// DDS DDS_HEADER_DXT10 - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header-dxt10
// DDS File Layout https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-file-layout-for-textures
// Texture Block Compression in D3D11 - https://docs.microsoft.com/en-us/windows/win32/direct3d11/texture-block-compression-in-direct3d-11
// DDS Programming Guide - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds-pguide
// D3DFORMAT - https://learn.microsoft.com/en-us/windows/win32/direct3d9/d3dformat
// Map Direct3D 9 Formats to Direct3D 10 - https://learn.microsoft.com/en-gb/windows/win32/direct3d10/d3d10-graphics-programming-guide-resources-legacy-formats?redirectedfrom=MSDN

/// <summary>
/// Defines the various types of Direct3D9 surface formats. It also include some DDS FourCC codes.
/// </summary>
public enum D3DFormat
{
    UNKNOWN = 0,

    R8G8B8 = 20, // Supported
    A8R8G8B8 = 21, // Supported
    X8R8G8B8 = 22, // Supported
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

    L8 = 50, // Supported
    A8L8 = 51,
    A4L4 = 52,

    V8U8 = 60,
    L6V5U5 = 61,
    X8L8V8U8 = 62, // Used by Telltale, Not Supported
    Q8W8V8U8 = 63,
    V16U16 = 64,
    A2W10V10U10 = 67,

    UYVY = 0x59565955, // 'UYVY'
    R8G8_B8G8 = 0x47424752, // 'RGBG'
    YUY2 = 0x32595559, // 'YUY2'
    G8R8_G8B8 = 0x42475247, // 'GRGB'
    DXT1 = 0x31545844, // 'DXT1' // Supported
    DXT2 = 0x32545844, // 'DXT2' // Supported
    DXT3 = 0x33545844, // 'DXT3' // Supported
    DXT4 = 0x34545844, // 'DXT4' // Supported
    DXT5 = 0x35545844, // 'DXT5' // Supported
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

public enum DXGIFormat
{
    UNKNOWN = 0,
    R32G32B32A32_TYPELESS = 1,
    R32G32B32A32_FLOAT = 2,
    R32G32B32A32_UINT = 3,
    R32G32B32A32_SINT = 4,
    R32G32B32_TYPELESS = 5,
    R32G32B32_FLOAT = 6,
    R32G32B32_UINT = 7,
    R32G32B32_SINT = 8,
    R16G16B16A16_TYPELESS = 9,
    R16G16B16A16_FLOAT = 10,
    R16G16B16A16_UNORM = 11,
    R16G16B16A16_UINT = 12,
    R16G16B16A16_SNORM = 13,
    R16G16B16A16_SINT = 14,
    R32G32_TYPELESS = 15,
    R32G32_FLOAT = 16,
    R32G32_UINT = 17,
    R32G32_SINT = 18,
    R32G8X24_TYPELESS = 19,
    D32_FLOAT_S8X24_UINT = 20,
    R32_FLOAT_X8X24_TYPELESS = 21,
    X32_TYPELESS_G8X24_UINT = 22,
    R10G10B10A2_TYPELESS = 23,
    R10G10B10A2_UNORM = 24,
    R10G10B10A2_UINT = 25,
    R11G11B10_FLOAT = 26,
    R8G8B8A8_TYPELESS = 27,
    R8G8B8A8_UNORM = 28,
    R8G8B8A8_UNORM_SRGB = 29,
    R8G8B8A8_UINT = 30,
    R8G8B8A8_SNORM = 31,
    R8G8B8A8_SINT = 32,
    R16G16_TYPELESS = 33,
    R16G16_FLOAT = 34,
    R16G16_UNORM = 35,
    R16G16_UINT = 36,
    R16G16_SNORM = 37,
    R16G16_SINT = 38,
    R32_TYPELESS = 39,
    D32_FLOAT = 40,
    R32_FLOAT = 41,
    R32_UINT = 42,
    R32_SINT = 43,
    R24G8_TYPELESS = 44,
    D24_UNORM_S8_UINT = 45,
    R24_UNORM_X8_TYPELESS = 46,
    X24_TYPELESS_G8_UINT = 47,
    R8G8_TYPELESS = 48,
    R8G8_UNORM = 49,
    R8G8_UINT = 50,
    R8G8_SNORM = 51,
    R8G8_SINT = 52,
    R16_TYPELESS = 53,
    R16_FLOAT = 54,
    D16_UNORM = 55,
    R16_UNORM = 56,
    R16_UINT = 57,
    R16_SNORM = 58,
    R16_SINT = 59,
    R8_TYPELESS = 60,
    R8_UNORM = 61,
    R8_UINT = 62,
    R8_SNORM = 63,
    R8_SINT = 64,
    A8_UNORM = 65,
    R1_UNORM = 66,
    R9G9B9E5_SHAREDEXP = 67,
    R8G8_B8G8_UNORM = 68,
    G8R8_G8B8_UNORM = 69,
    BC1_TYPELESS = 70,
    BC1_UNORM = 71,
    BC1_UNORM_SRGB = 72,
    BC2_TYPELESS = 73,
    BC2_UNORM = 74,
    BC2_UNORM_SRGB = 75,
    BC3_TYPELESS = 76,
    BC3_UNORM = 77,
    BC3_UNORM_SRGB = 78,
    BC4_TYPELESS = 79,
    BC4_UNORM = 80,
    BC4_SNORM = 81,
    BC5_TYPELESS = 82,
    BC5_UNORM = 83,
    BC5_SNORM = 84,
    B5G6R5_UNORM = 85,
    B5G5R5A1_UNORM = 86,
    B8G8R8A8_UNORM = 87,
    B8G8R8X8_UNORM = 88,
    R10G10B10_XR_BIAS_A2_UNORM = 89,
    B8G8R8A8_TYPELESS = 90,
    B8G8R8A8_UNORM_SRGB = 91,
    B8G8R8X8_TYPELESS = 92,
    B8G8R8X8_UNORM_SRGB = 93,
    BC6H_TYPELESS = 94,
    BC6H_UF16 = 95,
    BC6H_SF16 = 96,
    BC7_TYPELESS = 97,
    BC7_UNORM = 98,
    BC7_UNORM_SRGB = 99,
    AYUV = 100,
    Y410 = 101,
    Y416 = 102,
    NV12 = 103,
    P010 = 104,
    P016 = 105,
    OPAQUE_420 = 106,
    YUY2 = 107,
    Y210 = 108,
    Y216 = 109,
    NV11 = 110,
    AI44 = 111,
    IA44 = 112,
    P8 = 113,
    A8P8 = 114,
    B4G4R4A4_UNORM = 115,

    P208 = 130,
    V208 = 131,
    V408 = 132,

    ASTC_4X4_TYPELESS = 133,
    ASTC_4X4_UNORM = 134,
    ASTC_4X4_UNORM_SRGB = 135,
    ASTC_5X4_TYPELESS = 137,
    ASTC_5X4_UNORM = 138,
    ASTC_5X4_UNORM_SRGB = 139,
    ASTC_5X5_TYPELESS = 141,
    ASTC_5X5_UNORM = 142,
    ASTC_5X5_UNORM_SRGB = 143,
    ASTC_6X5_TYPELESS = 145,
    ASTC_6X5_UNORM = 146,
    ASTC_6X5_UNORM_SRGB = 147,
    ASTC_6X6_TYPELESS = 149,
    ASTC_6X6_UNORM = 150,
    ASTC_6X6_UNORM_SRGB = 151,
    ASTC_8X5_TYPELESS = 153,
    ASTC_8X5_UNORM = 154,
    ASTC_8X5_UNORM_SRGB = 155,
    ASTC_8X6_TYPELESS = 157,
    ASTC_8X6_UNORM = 158,
    ASTC_8X6_UNORM_SRGB = 159,
    ASTC_8X8_TYPELESS = 161,
    ASTC_8X8_UNORM = 162,
    ASTC_8X8_UNORM_SRGB = 163,
    ASTC_10X5_TYPELESS = 165,
    ASTC_10X5_UNORM = 166,
    ASTC_10X5_UNORM_SRGB = 167,
    ASTC_10X6_TYPELESS = 169,
    ASTC_10X6_UNORM = 170,
    ASTC_10X6_UNORM_SRGB = 171,
    ASTC_10X8_TYPELESS = 173,
    ASTC_10X8_UNORM = 174,
    ASTC_10X8_UNORM_SRGB = 175,
    ASTC_10X10_TYPELESS = 177,
    ASTC_10X10_UNORM = 178,
    ASTC_10X10_UNORM_SRGB = 179,
    ASTC_12X10_TYPELESS = 181,
    ASTC_12X10_UNORM = 182,
    ASTC_12X10_UNORM_SRGB = 183,
    ASTC_12X12_TYPELESS = 185,
    ASTC_12X12_UNORM = 186,
    ASTC_12X12_UNORM_SRGB = 187,


    SAMPLER_FEEDBACK_MIN_MIP_OPAQUE = 189,
    SAMPLER_FEEDBACK_MIP_REGION_USED_OPAQUE = 190,

    A4B4G4R4_UNORM = 191,
    //FORCE_UINT = 0xffffffff
}
