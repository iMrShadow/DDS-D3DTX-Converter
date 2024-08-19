using TelltaleTextureTool.DirectX.Enums;
using TelltaleTextureTool.TelltaleEnums;
using Hexa.NET.DirectXTex;
using SkiaSharp;
using static Ktx.Ktx2;

namespace TelltaleTextureTool.DirectX;

// DDS Docs - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header
// DDS PIXEL FORMAT - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-pixelformat
// DDS DDS_HEADER_DXT10 - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header-dxt10
// DDS File Layout https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-file-layout-for-textures
// Texture Block Compression in D3D11 - https://docs.microsoft.com/en-us/windows/win32/direct3d11/texture-block-compression-in-direct3d-11
// DDS Programming Guide - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds-pguide
// D3DFORMAT - https://learn.microsoft.com/en-us/windows/win32/direct3d9/d3dformat
// Map Direct3D 9 Formats to Direct3D 10 - https://learn.microsoft.com/en-gb/windows/win32/direct3d10/d3d10-graphics-programming-guide-resources-legacy-formats?redirectedfrom=MSDN

/// <summary>
/// The class is used for decoding and encoding .dds headers. 
/// </summary>
public static partial class DDS_HELPER
{
    /// <summary>
    /// Get the Telltale surface format from a DXGI format.
    /// This is used for the conversion process from .dds to .d3dtx.
    /// </summary>
    /// <param name="dxgiFormat">The Direct3D10/DXGI format.</param>
    /// <returns>The corresponding T3SurfaceFormat enum from the DXGI format.</returns>
    private static T3SurfaceFormat GetTelltaleSurfaceFormat(DXGIFormat dxgiFormat)
    {
        return dxgiFormat switch
        {
            DXGIFormat.B8G8R8A8_UNORM_SRGB => T3SurfaceFormat.ARGB8,
            DXGIFormat.B8G8R8A8_UNORM => T3SurfaceFormat.ARGB8,
            DXGIFormat.R16G16B16A16_UNORM => T3SurfaceFormat.ARGB16,
            DXGIFormat.B5G6R5_UNORM => T3SurfaceFormat.RGB565,
            DXGIFormat.B5G5R5A1_UNORM => T3SurfaceFormat.ARGB1555,
            DXGIFormat.B4G4R4A4_UNORM => T3SurfaceFormat.ARGB4,
            DXGIFormat.A4B4G4R4_UNORM => T3SurfaceFormat.ARGB4,
            DXGIFormat.R10G10B10A2_UNORM => T3SurfaceFormat.ARGB2101010,
            DXGIFormat.R16G16_UNORM => T3SurfaceFormat.RG16,
            DXGIFormat.R8G8B8A8_UNORM_SRGB => T3SurfaceFormat.RGBA8,
            DXGIFormat.R8G8B8A8_UNORM => T3SurfaceFormat.RGBA8,
            DXGIFormat.R32_UINT => T3SurfaceFormat.R32,
            DXGIFormat.R32G32_UINT => T3SurfaceFormat.RG32,
            DXGIFormat.R32G32B32A32_FLOAT => T3SurfaceFormat.RGBA32F,
            DXGIFormat.R8G8B8A8_SNORM => T3SurfaceFormat.RGBA8S,
            DXGIFormat.A8_UNORM => T3SurfaceFormat.A8,
            DXGIFormat.R8_UNORM => T3SurfaceFormat.L8,
            DXGIFormat.R8G8_UNORM => T3SurfaceFormat.AL8,
            DXGIFormat.R16_UNORM => T3SurfaceFormat.L16,
            DXGIFormat.R16G16_SNORM => T3SurfaceFormat.RG16S,
            DXGIFormat.R16G16B16A16_SNORM => T3SurfaceFormat.RGBA16S,
            DXGIFormat.R16G16B16A16_UINT => T3SurfaceFormat.R16UI,
            DXGIFormat.R16_FLOAT => T3SurfaceFormat.R16F,
            DXGIFormat.R16G16B16A16_FLOAT => T3SurfaceFormat.RGBA16F,
            DXGIFormat.R32_FLOAT => T3SurfaceFormat.R32F,
            DXGIFormat.R32G32_FLOAT => T3SurfaceFormat.RG32F,
            DXGIFormat.R32G32B32A32_UINT => T3SurfaceFormat.RGBA32,
            DXGIFormat.R11G11B10_FLOAT => T3SurfaceFormat.RGB111110F,
            DXGIFormat.R9G9B9E5_SHAREDEXP => T3SurfaceFormat.RGB9E5F,
            DXGIFormat.D16_UNORM => T3SurfaceFormat.DepthPCF16,
            DXGIFormat.D24_UNORM_S8_UINT => T3SurfaceFormat.DepthPCF24,
            DXGIFormat.D32_FLOAT_S8X24_UINT => T3SurfaceFormat.DepthStencil32,
            DXGIFormat.D32_FLOAT => T3SurfaceFormat.Depth32F,
            DXGIFormat.BC1_UNORM => T3SurfaceFormat.BC1,
            DXGIFormat.BC2_UNORM => T3SurfaceFormat.BC2,
            DXGIFormat.BC3_UNORM => T3SurfaceFormat.BC3,
            DXGIFormat.BC4_UNORM => T3SurfaceFormat.BC4,
            DXGIFormat.BC5_UNORM => T3SurfaceFormat.BC5,
            DXGIFormat.BC6H_UF16 => T3SurfaceFormat.BC6,
            DXGIFormat.BC6H_SF16 => T3SurfaceFormat.BC6,
            DXGIFormat.BC7_UNORM => T3SurfaceFormat.BC7,
            DXGIFormat.BC1_UNORM_SRGB => T3SurfaceFormat.BC1,
            DXGIFormat.BC2_UNORM_SRGB => T3SurfaceFormat.BC2,
            DXGIFormat.BC3_UNORM_SRGB => T3SurfaceFormat.BC3,
            DXGIFormat.BC7_UNORM_SRGB => T3SurfaceFormat.BC7,
            _ => T3SurfaceFormat.Unknown,
        };
    }

    /// <summary>
    /// Get the Telltale surface format from a DXGI format and an already existing surface format.
    /// This is used for the conversion process from .dds to .d3dtx.
    /// This is used when equivalent formats are found. 
    /// Some Telltale games have different values for the same formats, but they do not ship with all of them.
    /// This can create issues if the Telltale surface format is not found in the game. 
    /// In any case, use Lucas's Telltale Inspector to change the value if any issues arise.
    /// </summary>
    /// <param name="dxgiFormat">The Direct3D10/DXGI format.</param>
    /// <param name="surfaceFormat">(Optional) The existing Telltale surface format. Default value is UNKNOWN.</param>
    /// <returns>The corresponding T3SurfaceFormat enum from the DXGI format and Telltale surface format.</returns>
    public static T3SurfaceFormat GetTelltaleSurfaceFormat(DXGIFormat dxgiFormat, T3SurfaceFormat surfaceFormat = T3SurfaceFormat.Unknown)
    {
        T3SurfaceFormat surfaceFormatFromDXGI = GetTelltaleSurfaceFormat(dxgiFormat);

        if (surfaceFormatFromDXGI == T3SurfaceFormat.Unknown)
        {
            return surfaceFormat;
        }

        if (surfaceFormatFromDXGI == T3SurfaceFormat.L16 && surfaceFormat == T3SurfaceFormat.R16)
        {
            return T3SurfaceFormat.L16;
        }
        else if (surfaceFormatFromDXGI == T3SurfaceFormat.AL8 && surfaceFormat == T3SurfaceFormat.RG8)
        {
            return T3SurfaceFormat.RG8;
        }
        else if (surfaceFormatFromDXGI == T3SurfaceFormat.L8 && surfaceFormat == T3SurfaceFormat.R8)
        {
            return T3SurfaceFormat.R8;
        }
        else if (surfaceFormatFromDXGI == T3SurfaceFormat.ARGB16 && surfaceFormat == T3SurfaceFormat.RGBA16)
        {
            return T3SurfaceFormat.RGBA16;
        }
        else if (surfaceFormatFromDXGI == T3SurfaceFormat.ARGB2101010 && surfaceFormat == T3SurfaceFormat.RGBA1010102F)
        {
            return T3SurfaceFormat.RGBA1010102F;
        }
        else if (surfaceFormatFromDXGI == T3SurfaceFormat.R32F && surfaceFormat == T3SurfaceFormat.R32)
        {
            return T3SurfaceFormat.R32;
        }
        else if (surfaceFormatFromDXGI == T3SurfaceFormat.RG32F && surfaceFormat == T3SurfaceFormat.RG32)
        {
            return T3SurfaceFormat.R32;
        }

        return surfaceFormatFromDXGI;
    }

    /// <summary>
    /// Get the DXGI format from a Telltale surface format. Gamma and platform type are optional.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="gamma"></param>
    /// <param name="platformType"></param>
    /// <returns>The corresponding DXGI_Format.</returns>
    public static DXGIFormat GetDXGIFormat(T3SurfaceFormat format, T3SurfaceGamma gamma = T3SurfaceGamma.eSurfaceGamma_Linear, T3PlatformType platformType = T3PlatformType.ePlatform_PC)
    {
        DXGIFormat dxgiFormat = format switch
        {
            // In order of T3SurfaceFormat enum
            //--------------------ARGB8--------------------
            T3SurfaceFormat.ARGB8 => gamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.B8G8R8A8_UNORM_SRGB : DXGIFormat.B8G8R8A8_UNORM,
            //--------------------ARGB16--------------------
            T3SurfaceFormat.ARGB16 => DXGIFormat.R16G16B16A16_UNORM,
            //--------------------RGB565--------------------
            T3SurfaceFormat.RGB565 => DXGIFormat.B5G6R5_UNORM,
            //--------------------ARGB1555--------------------
            T3SurfaceFormat.ARGB1555 => DXGIFormat.B5G5R5A1_UNORM,
            //--------------------ARGB4--------------------
            T3SurfaceFormat.ARGB4 => DXGIFormat.B4G4R4A4_UNORM,
            //--------------------ARGB2101010--------------------
            T3SurfaceFormat.ARGB2101010 => DXGIFormat.R10G10B10A2_UNORM,
            //--------------------R16--------------------
            T3SurfaceFormat.R16 => DXGIFormat.R16_UNORM,
            //--------------------RG16--------------------
            T3SurfaceFormat.RG16 => DXGIFormat.R16G16_UNORM,
            //--------------------RGBA16--------------------
            T3SurfaceFormat.RGBA16 => DXGIFormat.R16G16B16A16_UNORM,
            //--------------------RG8--------------------
            T3SurfaceFormat.RG8 => DXGIFormat.R8G8_UNORM,
            //--------------------RGBA8--------------------
            T3SurfaceFormat.RGBA8 => gamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.R8G8B8A8_UNORM_SRGB : DXGIFormat.R8G8B8A8_UNORM,
            //--------------------R32--------------------
            T3SurfaceFormat.R32 => DXGIFormat.R32_FLOAT,
            //--------------------RG32--------------------
            T3SurfaceFormat.RG32 => DXGIFormat.R32G32_FLOAT,
            //--------------------RGBA32--------------------
            T3SurfaceFormat.RGBA32 => DXGIFormat.R32G32B32A32_FLOAT,
            //--------------------R8--------------------
            T3SurfaceFormat.R8 => DXGIFormat.R8_UNORM,
            //--------------------RGBA8S--------------------
            T3SurfaceFormat.RGBA8S => DXGIFormat.R8G8B8A8_SNORM,
            //--------------------A8--------------------
            T3SurfaceFormat.A8 => DXGIFormat.A8_UNORM,
            //--------------------L8--------------------
            T3SurfaceFormat.L8 => DXGIFormat.R8_UNORM,
            //--------------------AL8--------------------
            T3SurfaceFormat.AL8 => DXGIFormat.R8G8_UNORM,
            //--------------------R16--------------------
            T3SurfaceFormat.L16 => DXGIFormat.R16_UNORM,
            //--------------------RG16S--------------------
            T3SurfaceFormat.RG16S => DXGIFormat.R16G16_SNORM,
            //--------------------RGBA16S--------------------
            T3SurfaceFormat.RGBA16S => DXGIFormat.R16G16B16A16_SNORM,
            //--------------------RGBA16UI--------------------
            T3SurfaceFormat.R16UI => DXGIFormat.R16G16B16A16_UINT,
            //--------------------RG16F--------------------
            T3SurfaceFormat.R16F => DXGIFormat.R16_FLOAT,
            //--------------------RGBA16F--------------------
            T3SurfaceFormat.RGBA16F => DXGIFormat.R16G16B16A16_FLOAT,
            //--------------------R32F--------------------
            T3SurfaceFormat.R32F => DXGIFormat.R32_FLOAT,
            //--------------------RG32F--------------------
            T3SurfaceFormat.RG32F => DXGIFormat.R32G32_FLOAT,
            //--------------------RGBA32F--------------------
            T3SurfaceFormat.RGBA32F => DXGIFormat.R32G32B32A32_FLOAT,
            //--------------------RGBA1010102F--------------------
            T3SurfaceFormat.RGBA1010102F => DXGIFormat.R10G10B10A2_UNORM,
            //--------------------RGB111110F--------------------
            T3SurfaceFormat.RGB111110F => DXGIFormat.R11G11B10_FLOAT,
            //--------------------RGB9E5F--------------------
            T3SurfaceFormat.RGB9E5F => DXGIFormat.R9G9B9E5_SHAREDEXP,
            //--------------------DepthPCF16--------------------
            T3SurfaceFormat.DepthPCF16 => DXGIFormat.D16_UNORM,
            //--------------------DepthPCF24--------------------
            T3SurfaceFormat.DepthPCF24 => DXGIFormat.D24_UNORM_S8_UINT,
            //--------------------Depth16--------------------
            T3SurfaceFormat.Depth16 => DXGIFormat.D16_UNORM,
            //--------------------Depth24--------------------
            T3SurfaceFormat.Depth24 => DXGIFormat.D24_UNORM_S8_UINT,
            //--------------------DepthStencil32--------------------
            T3SurfaceFormat.DepthStencil32 => DXGIFormat.D32_FLOAT_S8X24_UINT,
            //--------------------Depth32F--------------------
            T3SurfaceFormat.Depth32F => DXGIFormat.D32_FLOAT,
            //--------------------Depth32F_Stencil8--------------------
            T3SurfaceFormat.Depth32F_Stencil8 => DXGIFormat.D32_FLOAT_S8X24_UINT,
            //--------------------Depth24F_Stencil8--------------------
            T3SurfaceFormat.Depth24F_Stencil8 => DXGIFormat.D24_UNORM_S8_UINT,
            //--------------------DXT1 / BC1--------------------
            T3SurfaceFormat.BC1 => gamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC1_UNORM_SRGB : DXGIFormat.BC1_UNORM,
            //--------------------DXT2 and DXT3 / BC2--------------------
            T3SurfaceFormat.BC2 => gamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC2_UNORM_SRGB : DXGIFormat.BC2_UNORM,
            //--------------------DXT4 and DXT5 / BC3--------------------
            T3SurfaceFormat.BC3 => gamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC3_UNORM_SRGB : DXGIFormat.BC3_UNORM,
            //--------------------ATI1 / BC4--------------------
            T3SurfaceFormat.BC4 => DXGIFormat.BC4_UNORM,
            //--------------------ATI2 / BC5--------------------
            T3SurfaceFormat.BC5 => DXGIFormat.BC5_UNORM,
            //--------------------BC6H--------------------
            T3SurfaceFormat.BC6 => DXGIFormat.BC6H_UF16,
            //--------------------BC7--------------------
            T3SurfaceFormat.BC7 => gamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC7_UNORM_SRGB : DXGIFormat.BC7_UNORM,
            //--------------------ATC--------------------
            T3SurfaceFormat.ATC_RGB => gamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.R8G8B8A8_UNORM_SRGB : DXGIFormat.R8G8B8A8_UNORM,
            //--------------------ATCA--------------------
            T3SurfaceFormat.ATC_RGB1A => gamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.R8G8B8A8_UNORM_SRGB : DXGIFormat.R8G8B8A8_UNORM,
            //--------------------ATCI--------------------
            T3SurfaceFormat.ATC_RGBA => gamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.R8G8B8A8_UNORM_SRGB : DXGIFormat.R8G8B8A8_UNORM,
            //-------------------PVRTC2--------------------
            T3SurfaceFormat.PVRTC2 => gamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.R8G8B8A8_UNORM_SRGB : DXGIFormat.R8G8B8A8_UNORM,
            //-------------------PVRTC2a--------------------
            T3SurfaceFormat.PVRTC2a => gamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.R8G8B8A8_UNORM_SRGB : DXGIFormat.R8G8B8A8_UNORM,
            //-------------------PVRTC4--------------------
            T3SurfaceFormat.PVRTC4 => gamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.R8G8B8A8_UNORM_SRGB : DXGIFormat.R8G8B8A8_UNORM,
            //-------------------PVRTC4a--------------------
            T3SurfaceFormat.PVRTC4a => gamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.R8G8B8A8_UNORM_SRGB : DXGIFormat.R8G8B8A8_UNORM,
            //-------------------CTX1--------------------
            T3SurfaceFormat.CTX1 => DXGIFormat.BC1_UNORM,
            //--------------------UNKNOWN--------------------
            T3SurfaceFormat.Unknown => DXGIFormat.UNKNOWN,
            //--------------------Default Conversion--------------------
            _ => DXGIFormat.R8G8B8A8_UNORM, // Choose R8G8B8A8 if the format is not specified. (Raw data)
        };

        if (platformType == T3PlatformType.ePlatform_iPhone || platformType == T3PlatformType.ePlatform_Android)
        {
            dxgiFormat = GetDXGIFormatWithSwappedChannels(dxgiFormat);
        }

        return dxgiFormat;
    }

    public static DXGIFormat GetDXGIFormatWithSwappedChannels(DXGIFormat dxgiFormat)
    {
        return dxgiFormat switch
        {
            DXGIFormat.B8G8R8A8_UNORM => DXGIFormat.R8G8B8A8_UNORM,
            DXGIFormat.R8G8B8A8_UNORM => DXGIFormat.B8G8R8A8_UNORM,
            DXGIFormat.A4B4G4R4_UNORM => DXGIFormat.B4G4R4A4_UNORM,
            DXGIFormat.B4G4R4A4_UNORM => DXGIFormat.A4B4G4R4_UNORM,
            DXGIFormat.B8G8R8A8_UNORM_SRGB => DXGIFormat.R8G8B8A8_UNORM_SRGB,
            DXGIFormat.R8G8B8A8_UNORM_SRGB => DXGIFormat.B8G8R8A8_UNORM_SRGB,
            _ => dxgiFormat
        };
    }

    public static DXGIFormat GetEquivalentDXGIFormat(T3SurfaceFormat format, T3SurfaceGamma surfaceGamma)
    {
        return format switch
        {
            T3SurfaceFormat.ATC_RGB => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC1_UNORM_SRGB : DXGIFormat.BC1_UNORM,
            T3SurfaceFormat.ATC_RGBA => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC3_UNORM_SRGB : DXGIFormat.BC3_UNORM,
            T3SurfaceFormat.ATC_RGB1A => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC3_UNORM_SRGB : DXGIFormat.BC3_UNORM,
            T3SurfaceFormat.ETC1_RGB => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC1_UNORM_SRGB : DXGIFormat.BC1_UNORM,
            T3SurfaceFormat.ETC2_RGB => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC1_UNORM_SRGB : DXGIFormat.BC1_UNORM,
            T3SurfaceFormat.ETC2_RGBA => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC3_UNORM_SRGB : DXGIFormat.BC3_UNORM,
            T3SurfaceFormat.ETC2_RGB1A => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC1_UNORM_SRGB : DXGIFormat.BC1_UNORM,
            T3SurfaceFormat.ETC2_R => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC1_UNORM_SRGB : DXGIFormat.BC1_UNORM,
            T3SurfaceFormat.ETC2_RG => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC3_UNORM_SRGB : DXGIFormat.BC3_UNORM,
            T3SurfaceFormat.ATSC_RGBA_4x4 => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC3_UNORM_SRGB : DXGIFormat.BC3_UNORM,
            T3SurfaceFormat.PVRTC2 => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC1_UNORM_SRGB : DXGIFormat.BC1_UNORM,
            T3SurfaceFormat.PVRTC2a => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC1_UNORM_SRGB : DXGIFormat.BC1_UNORM,
            T3SurfaceFormat.PVRTC4 => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC1_UNORM_SRGB : DXGIFormat.BC1_UNORM,
            T3SurfaceFormat.PVRTC4a => surfaceGamma == T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.BC1_UNORM_SRGB : DXGIFormat.BC1_UNORM,
            T3SurfaceFormat.CTX1 => DXGIFormat.BC4_UNORM,

            _ => surfaceGamma ==  T3SurfaceGamma.eSurfaceGamma_sRGB ? DXGIFormat.R8G8B8A8_UNORM_SRGB : DXGIFormat.R8G8B8A8_UNORM
        };
    }

    /// <summary>
    /// Returns the corresponding Direct3D9 surface format from a Direct3D10/DXGI format.
    /// This is used for the conversion process from .d3dtx to .dds. (Legacy .d3dtx)
    /// </summary>
    /// <param name="dxgiFormat">The DXGI format.</param>
    /// <param name="metadata">The metadata of our .dds file. It is used in determining if the alpha is premultiplied.</param>
    /// <returns>The corresponding Direct3D9 format.</returns>
    public static D3DFormat GetD3DFORMAT(DXGIFormat dxgiFormat, TexMetadata metadata)
    {
        return dxgiFormat switch
        {
            DXGIFormat.B8G8R8A8_UNORM => D3DFormat.A8R8G8B8,
            DXGIFormat.B8G8R8X8_UNORM => D3DFormat.X8R8G8B8,
            DXGIFormat.B5G6R5_UNORM => D3DFormat.R5G6B5,
            DXGIFormat.B5G5R5A1_UNORM => D3DFormat.A1R5G5B5,
            DXGIFormat.B4G4R4A4_UNORM => D3DFormat.A4R4G4B4,
            DXGIFormat.A4B4G4R4_UNORM => D3DFormat.A4R4G4B4,
            DXGIFormat.A8_UNORM => D3DFormat.A8,
            DXGIFormat.R10G10B10A2_UNORM => D3DFormat.A2B10G10R10,
            DXGIFormat.R8G8B8A8_UNORM => D3DFormat.A8B8G8R8,
            DXGIFormat.R8G8B8A8_UNORM_SRGB => D3DFormat.A8B8G8R8,
            DXGIFormat.R16G16_UNORM => D3DFormat.G16R16,
            DXGIFormat.R16G16B16A16_UNORM => D3DFormat.A16B16G16R16,
            DXGIFormat.R8_UNORM => D3DFormat.L8,
            DXGIFormat.R8G8_UNORM => D3DFormat.A8L8,
            DXGIFormat.R8G8_SNORM => D3DFormat.V8U8,
            DXGIFormat.R8G8B8A8_SNORM => D3DFormat.Q8W8V8U8,
            DXGIFormat.R16G16_SNORM => D3DFormat.V16U16,
            DXGIFormat.G8R8_G8B8_UNORM => D3DFormat.R8G8_B8G8,
            DXGIFormat.YUY2 => D3DFormat.YUY2,
            DXGIFormat.R8G8_B8G8_UNORM => D3DFormat.G8R8_G8B8,
            DXGIFormat.BC1_UNORM => D3DFormat.DXT1,
            DXGIFormat.BC2_UNORM => metadata.IsPMAlpha() ? D3DFormat.DXT2 : D3DFormat.DXT3,
            DXGIFormat.BC3_UNORM => metadata.IsPMAlpha() ? D3DFormat.DXT4 : D3DFormat.DXT5,
            DXGIFormat.BC4_UNORM => D3DFormat.ATI1,
            DXGIFormat.BC4_SNORM => D3DFormat.BC4S,
            DXGIFormat.BC5_UNORM => D3DFormat.ATI2,
            DXGIFormat.BC5_SNORM => D3DFormat.BC5S,
            DXGIFormat.D16_UNORM => D3DFormat.D16,
            DXGIFormat.D32_FLOAT => D3DFormat.D32F_LOCKABLE,
            DXGIFormat.D24_UNORM_S8_UINT => D3DFormat.D24S8,
            DXGIFormat.R16_UNORM => D3DFormat.L16,
            DXGIFormat.R16G16B16A16_SNORM => D3DFormat.Q16W16V16U16,
            DXGIFormat.R16_FLOAT => D3DFormat.R16F,
            DXGIFormat.R16G16_FLOAT => D3DFormat.G16R16F,
            DXGIFormat.R16G16B16A16_FLOAT => D3DFormat.A16B16G16R16F,
            DXGIFormat.R32_FLOAT => D3DFormat.R32F,
            DXGIFormat.R32G32_FLOAT => D3DFormat.G32R32F,
            DXGIFormat.R32G32B32A32_FLOAT => D3DFormat.A32B32G32R32F,

            _ => D3DFormat.UNKNOWN
        };
    }

    /// <summary>
    /// Returns the corresponding Direct3D9 surface format from a Direct3D10/DXGI format.
    /// This is used for the conversion process from .d3dtx to .dds. (Legacy .d3dtx)
    /// </summary>
    /// <param name="dxgiFormat">The DXGI format.</param>
    /// <param name="metadata">The metadata of our .dds file. It is used in determining if the alpha is premultiplied.</param>
    /// <returns>The corresponding Direct3D9 format.</returns>
    public static DXGIFormat GetDXGIFormat(D3DFormat format)
    {
        return format switch
        {
            D3DFormat.A8R8G8B8 => DXGIFormat.B8G8R8A8_UNORM,
            D3DFormat.X8R8G8B8 => DXGIFormat.B8G8R8X8_UNORM,
            D3DFormat.X8L8V8U8 => DXGIFormat.B8G8R8A8_UNORM,
            D3DFormat.R5G6B5 => DXGIFormat.B5G6R5_UNORM,
            D3DFormat.X1R5G5B5 => DXGIFormat.B5G5R5A1_UNORM,
            D3DFormat.A1R5G5B5 => DXGIFormat.B5G5R5A1_UNORM,
            D3DFormat.A4R4G4B4 => DXGIFormat.B4G4R4A4_UNORM,
            D3DFormat.A8 => DXGIFormat.A8_UNORM,
            D3DFormat.A2B10G10R10 => DXGIFormat.R10G10B10A2_UNORM,
            D3DFormat.A8B8G8R8 => DXGIFormat.R8G8B8A8_UNORM,
            D3DFormat.X8B8G8R8 => DXGIFormat.R8G8B8A8_UNORM,
            D3DFormat.G16R16 => DXGIFormat.R16G16_UNORM,
            D3DFormat.A16B16G16R16 => DXGIFormat.R16G16B16A16_UNORM,
            D3DFormat.L8 => DXGIFormat.R8_UNORM,
            D3DFormat.A8L8 => DXGIFormat.R8G8_UNORM,
            D3DFormat.V8U8 => DXGIFormat.R8G8_SNORM,
            D3DFormat.Q8W8V8U8 => DXGIFormat.R8G8B8A8_SNORM,
            D3DFormat.V16U16 => DXGIFormat.R16G16_SNORM,
            D3DFormat.R8G8_B8G8 => DXGIFormat.G8R8_G8B8_UNORM,
            D3DFormat.YUY2 => DXGIFormat.YUY2,
            D3DFormat.G8R8_G8B8 => DXGIFormat.R8G8_B8G8_UNORM,
            D3DFormat.DXT1 => DXGIFormat.BC1_UNORM,
            D3DFormat.DXT2 => DXGIFormat.BC2_UNORM,
            D3DFormat.DXT3 => DXGIFormat.BC2_UNORM,
            D3DFormat.DXT4 => DXGIFormat.BC3_UNORM,
            D3DFormat.DXT5 => DXGIFormat.BC3_UNORM,
            D3DFormat.ATI1 => DXGIFormat.BC4_UNORM,
            D3DFormat.BC4S => DXGIFormat.BC4_SNORM,
            D3DFormat.ATI2 => DXGIFormat.BC5_UNORM,
            D3DFormat.BC5S => DXGIFormat.BC5_SNORM,
            D3DFormat.D16 => DXGIFormat.D16_UNORM,
            D3DFormat.D32F_LOCKABLE => DXGIFormat.D32_FLOAT,
            D3DFormat.D24S8 => DXGIFormat.D24_UNORM_S8_UINT,
            D3DFormat.L16 => DXGIFormat.R16_UNORM,
            D3DFormat.Q16W16V16U16 => DXGIFormat.R16G16B16A16_SNORM,
            D3DFormat.R16F => DXGIFormat.R16_FLOAT,
            D3DFormat.G16R16F => DXGIFormat.R16G16_FLOAT,
            D3DFormat.A16B16G16R16F => DXGIFormat.R16G16B16A16_FLOAT,
            D3DFormat.R32F => DXGIFormat.R32_FLOAT,
            D3DFormat.G32R32F => DXGIFormat.R32G32_FLOAT,
            D3DFormat.A32B32G32R32F => DXGIFormat.R32G32B32A32_FLOAT,

            _ => DXGIFormat.UNKNOWN
        };
    }

    public static T3TextureLayout GetDimensionFromDDS(TexMetadata metadata)
    {
        if (metadata.ArraySize > 1)
        {
            return metadata.IsCubemap() ? T3TextureLayout.TextureCubemapArray : T3TextureLayout.Texture2DArray;
        }
        else if (metadata.IsVolumemap())
        {
            return T3TextureLayout.Texture3D;
        }
        else
        {
            return metadata.IsCubemap() ? T3TextureLayout.TextureCubemap : T3TextureLayout.Texture2D;
        }
    }

    public static SKColorType GetSKColorType(DXGIFormat format)
    {
        return format switch
        {
            DXGIFormat.R32G32B32A32_TYPELESS => SKColorType.RgbaF32,
            DXGIFormat.R32G32B32A32_FLOAT => SKColorType.RgbaF32,
            DXGIFormat.R32G32B32A32_UINT => SKColorType.RgbaF32,
            DXGIFormat.R32G32B32A32_SINT => SKColorType.RgbaF32,
            //DXGIFormat.R32G32B32_TYPELESS => SKColorType.RgbaF32,
            //DXGIFormat.R32G32B32_FLOAT => SKColorType.RgbaF32,
            //DXGIFormat.R32G32B32_UINT => SKColorType.RgbaF32,
            //DXGIFormat.R32G32B32_SINT => SKColorType.RgbaF32,
            DXGIFormat.R16G16B16A16_TYPELESS => SKColorType.Rgba16161616,
            DXGIFormat.R16G16B16A16_FLOAT => SKColorType.Rgba16161616,
            DXGIFormat.R16G16B16A16_UNORM => SKColorType.Rgba16161616,
            DXGIFormat.R16G16B16A16_UINT => SKColorType.Rgba16161616,
            DXGIFormat.R16G16B16A16_SNORM => SKColorType.Rgba16161616,
            DXGIFormat.R16G16B16A16_SINT => SKColorType.Rgba16161616,
            DXGIFormat.R10G10B10A2_TYPELESS => SKColorType.Rgba1010102,
            DXGIFormat.R10G10B10A2_UNORM => SKColorType.Rgba1010102,
            DXGIFormat.R10G10B10A2_UINT => SKColorType.Rgba1010102,
            DXGIFormat.R8G8B8A8_TYPELESS => SKColorType.Rgba8888,
            DXGIFormat.R8G8B8A8_UNORM => SKColorType.Rgba8888,
            DXGIFormat.R8G8B8A8_UNORM_SRGB => SKColorType.Rgba8888,
            DXGIFormat.R8G8B8A8_UINT => SKColorType.Rgba8888,
            DXGIFormat.R8G8B8A8_SNORM => SKColorType.Rgba8888,
            DXGIFormat.R8G8B8A8_SINT => SKColorType.Rgba8888,
            DXGIFormat.R16G16_TYPELESS => SKColorType.Rg1616,
            DXGIFormat.R16G16_FLOAT => SKColorType.Rg1616,
            DXGIFormat.R16G16_UNORM => SKColorType.Rg1616,
            DXGIFormat.R16G16_UINT => SKColorType.Rg1616,
            DXGIFormat.R16G16_SNORM => SKColorType.Rg1616,
            DXGIFormat.R16G16_SINT => SKColorType.Rg1616,
            DXGIFormat.R8G8_TYPELESS => SKColorType.Rg88,
            DXGIFormat.R8G8_UNORM => SKColorType.Rg88,
            DXGIFormat.R8G8_UINT => SKColorType.Rg88,
            DXGIFormat.R8G8_SNORM => SKColorType.Rg88,
            DXGIFormat.R8G8_SINT => SKColorType.Rg88,
            DXGIFormat.R8_TYPELESS => SKColorType.Gray8,
            DXGIFormat.R8_UNORM => SKColorType.Gray8,
            DXGIFormat.R8_UINT => SKColorType.Gray8,
            DXGIFormat.R8_SNORM => SKColorType.Gray8,
            DXGIFormat.R8_SINT => SKColorType.Gray8,
            DXGIFormat.A8_UNORM => SKColorType.Alpha8,
            DXGIFormat.B5G6R5_UNORM => SKColorType.Rgba8888,
            DXGIFormat.B8G8R8A8_UNORM => SKColorType.Bgra8888,
            DXGIFormat.B8G8R8A8_TYPELESS => SKColorType.Bgra8888,
            DXGIFormat.B8G8R8A8_UNORM_SRGB => SKColorType.Bgra8888,
            DXGIFormat.B4G4R4A4_UNORM => SKColorType.Argb4444,
            DXGIFormat.A4B4G4R4_UNORM => SKColorType.Argb4444,
            _ => SKColorType.Unknown // Default or unknown format
        };
    }
    }
