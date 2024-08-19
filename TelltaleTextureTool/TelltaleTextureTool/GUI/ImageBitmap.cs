using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BitMiracle.LibTiff.Classic;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.Main;
using Pfim;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;

namespace TelltaleTextureTool.Utilities;

public static class ImageUtilities
{

    /// <summary>
    /// Checks if the loaded image has any transparent pixels.
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public static bool IsImageOpaque(Image<Rgba32> image)
    {
        bool hasAlpha = false;

        image.ProcessPixelRows(pixelAccessor =>
        {
            for (int y = 0; y < pixelAccessor.Height; y++)
            {
                Span<Rgba32> pixelRow = pixelAccessor.GetRowSpan(y);

                for (int x = 0; x < pixelRow.Length; x++)
                {
                    if (pixelRow[x].A != 255)
                    {
                        hasAlpha = true;
                        break;
                    }
                }

                if (hasAlpha)
                {
                    break;
                }
            }
        });

        return hasAlpha;
    }
}
