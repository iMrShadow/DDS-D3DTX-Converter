﻿using LibWindPop.Utils.Graphics.Bitmap;
using LibWindPop.Utils.Graphics.Texture.IGraphicsAPITexture.OpenGLES;
using LibWindPop.Utils.Graphics.Texture.Shared;
using System;

namespace LibWindPop.Utils.Graphics.Texture.Coder
{
    public readonly unsafe struct RGB_ATC_UByte : ITextureCoder, IPitchableTextureCoder, IOpenGLES20CompressedTexture
    {
        public static int OpenGLES20InternalFormat => 0x8C92; // GL_ATC_RGB_AMD

        public readonly void Decode(ReadOnlySpan<byte> srcData, int width, int height, RefBitmap dstBitmap)
        {
            Decode(srcData, width, height, (width + 3) / 4 * 8, dstBitmap);
        }

        public readonly void Encode(RefBitmap srcBitmap, Span<byte> dstData, int width, int height)
        {
            Encode(srcBitmap, dstData, width, height, (width + 3) / 4 * 8);
        }

        public readonly void Decode(ReadOnlySpan<byte> srcData, int width, int height, int pitch, RefBitmap dstBitmap)
        {
            ThrowHelper.ThrowWhen(width < 0 || height < 0);
            int tempDataIndex;
            Span<YFColor> decodeBlockData = stackalloc YFColor[16];
            int srcDataIndex = 0;
            for (int yTile = 0; yTile < height; yTile += 4)
            {
                tempDataIndex = srcDataIndex;
                srcDataIndex += pitch;
                for (int xTile = 0; xTile < width; xTile += 4)
                {
                    ATCCoder.DecodeATCBlock(srcData.Slice(tempDataIndex, 8), decodeBlockData);
                    tempDataIndex += 8;
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

        public readonly void Encode(RefBitmap srcBitmap, Span<byte> dstData, int width, int height, int pitch)
        {
            ThrowHelper.ThrowWhen(width < 0 || height < 0);
            int tempDataIndex;
            Span<YFColor> encodeBlockData = stackalloc YFColor[16];
            int srcDataIndex = 0;
            for (int yTile = 0; yTile < height; yTile += 4)
            {
                tempDataIndex = srcDataIndex;
                srcDataIndex += pitch;
                for (int xTile = 0; xTile < width; xTile += 4)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        for (int x = 0; x < 4; x++)
                        {
                            encodeBlockData[(y << 2) | x] = ((yTile + y) >= height || (xTile + x) >= width) ? YFColor.Transparent : srcBitmap[xTile | x, yTile | y];
                        }
                    }
                    ATCCoder.EncodeATCBlock(dstData.Slice(tempDataIndex, 8), encodeBlockData);
                    tempDataIndex += 8;
                }
            }
        }
    }
}
