using PVRTexLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelltaleTextureTool.TelltaleEnums;

namespace TelltaleTextureTool.Graphics.PVR
{
    internal class PVR_Helper
    {

        public static PVRTexLibPixelFormat GetPVRFormat(T3SurfaceFormat format)
        {
            return format switch
            {
                T3SurfaceFormat.ETC1_RGB => PVRTexLibPixelFormat.ETC1,
                T3SurfaceFormat.ETC2_RGB => PVRTexLibPixelFormat.ETC2_RGB,
                T3SurfaceFormat.ETC2_RGBA => PVRTexLibPixelFormat.ETC2_RGBA,
                T3SurfaceFormat.ETC2_RGB1A => PVRTexLibPixelFormat.ETC2_RGB_A1,
                T3SurfaceFormat.ETC2_R => PVRTexLibPixelFormat.EAC_R11,
                T3SurfaceFormat.ETC2_RG => PVRTexLibPixelFormat.EAC_RG11,
                T3SurfaceFormat.ATSC_RGBA_4x4 => PVRTexLibPixelFormat.ASTC_4x4,
                T3SurfaceFormat.PVRTC2 => PVRTexLibPixelFormat.PVRTCI_2bpp_RGB,
                T3SurfaceFormat.PVRTC4 => PVRTexLibPixelFormat.PVRTCI_4bpp_RGB,
                T3SurfaceFormat.PVRTC2a => PVRTexLibPixelFormat.PVRTCI_2bpp_RGBA,
                T3SurfaceFormat.PVRTC4a => PVRTexLibPixelFormat.PVRTCI_4bpp_RGBA,
                _ => 0
            };
        }

    }
}
