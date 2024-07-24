﻿using LibWindPop.Utils.Graphics.Bitmap;
using LibWindPop.Utils.Graphics.Texture.Shared;
using System;

namespace LibWindPop.Utils.Graphics.Texture.Coder
{
    public readonly unsafe struct RGBA_ETC2_EAC_UByte : ITextureCoder
    {
        public readonly void Decode(ReadOnlySpan<byte> srcData, int width, int height, RefBitmap dstBitmap)
        {
            // if (PVRTexLibHelper.Decode(srcData, PVRTexLib.PVRTexLibPixelFormat.ETC2_RGBA, dstBitmap))
            // {
            //     return;
            // }
            ThrowHelper.ThrowWhen(width < 0 || height < 0);
            int tempDataIndex = 0;
            Span<YFColor> decodeBlockData = stackalloc YFColor[16];
            for (int yTile = 0; yTile < height; yTile += 4)
            {
                for (int xTile = 0; xTile < width; xTile += 4)
                {
                    ETCCoder.DecodeR8G8B8A8ETC2EACBlock(srcData.Slice(tempDataIndex, 16), decodeBlockData);
                    tempDataIndex += 16;
                    for (int y = 0; y < 4; y++)
                    {
                        if ((yTile + y) >= height)
                        {
                            break;
                        }
                        for (int x = 0; x < 4; x++)
                        {
                            if ((xTile + x) >= width)
                            {
                                break;
                            }
                            dstBitmap[xTile | x, yTile | y] = decodeBlockData[(y << 2) | x];
                        }
                    }
                }
            }
        }

        public readonly void Encode(RefBitmap srcBitmap, Span<byte> dstData, int width, int height)
        {
            // if (PVRTexLibHelper.Encode(srcBitmap, PVRTexLib.PVRTexLibPixelFormat.ETC2_RGBA, dstData))
            // {
            //     return;
            // }
            ThrowHelper.ThrowWhen(width < 0 || height < 0);
            int tempDataIndex = 0;
            Span<YFColor> encodeBlockData = stackalloc YFColor[16];
            for (int yTile = 0; yTile < height; yTile += 4)
            {
                for (int xTile = 0; xTile < width; xTile += 4)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        for (int x = 0; x < 4; x++)
                        {
                            encodeBlockData[(y << 2) | x] = ((yTile + y) >= height || (xTile + x) >= width) ? YFColor.Transparent : srcBitmap[xTile | x, yTile | y];
                        }
                    }
                    ETCCoder.EncodeR8G8B8A8ETC2EACBlock(dstData.Slice(tempDataIndex, 16), encodeBlockData);
                    tempDataIndex += 16;
                }
            }
        }
    }
}
