using System;
using System.IO;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.Main;
using TelltaleTextureTool.Utilities;
using SixLabors.ImageSharp.PixelFormats;
using static Ktx.Ktx2;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;
using Avalonia.Media.Imaging;
using BitMiracle.LibTiff.Classic;
using Pfim;
using SkiaSharp;
using System.Runtime.InteropServices;
using Avalonia.Platform;
using Avalonia;

namespace TelltaleTextureTool;

public class ImageData
{
    public ImageProperties ImageProperties { get; set; }
    public Bitmap ImageBitmap { get; set; }
    public DDS DDSImage { get; set; }

    private uint Mip { get; set; }
    private uint Face { get; set; }

    public uint MaxMip { get; set; }
    public uint MaxFace { get; set; }

    private string CurrentFilePath { get; set; }
    private TextureType CurrentTextureType { get; set; }
    private D3DTXVersion CurrentD3DTXVersion { get; set; }


    public ImageData(string filePath, TextureType textureType, D3DTXVersion d3DTXVersion = D3DTXVersion.DEFAULT, uint mip = 0, uint face = 0)
    {
        Mip = mip;
        Face = face;
        CurrentFilePath = filePath;
        CurrentTextureType = textureType;
        CurrentD3DTXVersion = d3DTXVersion;

        GetImageData(out Bitmap imageBitmap, out ImageProperties imageProperties);

        ImageBitmap = imageBitmap;
        ImageProperties = imageProperties;
    }

    public void SetImageData(string filePath, TextureType textureType, D3DTXVersion d3DTXVersion = D3DTXVersion.DEFAULT, uint mip = 0, uint face = 0)
    {
        Mip = mip;
        Face = face;
        CurrentFilePath = filePath;
        CurrentTextureType = textureType;
        CurrentD3DTXVersion = d3DTXVersion;

        GetImageData(out Bitmap imageBitmap, out ImageProperties imageProperties);

        ImageBitmap = imageBitmap;
        ImageProperties = imageProperties;
    }

    private void GetImageData(out Bitmap bitmap, out ImageProperties imageProperties)
    {
        switch (CurrentTextureType)
        {
            case TextureType.BMP:
            case TextureType.PNG:
            case TextureType.TGA:
            case TextureType.JPEG:
                GetImageDataFromOthers(out bitmap, out imageProperties);
                break;
            case TextureType.TIFF:
                GetImageDataFromTIFF(out bitmap, out imageProperties);
                break;
            case TextureType.DDS:
                GetImageDataFromDDS(out bitmap, out imageProperties);
                break;

            case TextureType.D3DTX:
                GetImageDataFromD3DTX(CurrentD3DTXVersion, out bitmap, out imageProperties);
                break;
            case TextureType.KTX:
            case TextureType.KTX2:
                GetImageDataFromInvalid(out bitmap, out imageProperties);
                break;
            default:
                GetImageDataFromInvalid(out bitmap, out imageProperties);
                break;
        }
    }

    private void GetImageDataFromD3DTX(D3DTXVersion d3DTXVersion, out Bitmap bitmap, out ImageProperties imageProperties)
    {
        var d3dtx = new D3DTX_Master();
        d3dtx.ReadD3DTXFile(CurrentFilePath, d3DTXVersion);

        // Initialize image properties
        D3DTXMetadata metadata = d3dtx.GetMetadata();

        imageProperties = new ImageProperties()
        {
            Name = metadata.TextureName,
            SurfaceFormat = d3dtx.GetStringFormat(),
            Width = metadata.Width.ToString(),
            Height = metadata.Height.ToString(),
            HasAlpha = d3dtx.GetHasAlpha(),
            ChannelCount = d3dtx.GetChannelCount(),
            MipMapCount = metadata.MipLevels.ToString()
        };

        // Initialize image bitmap
        DDS_Master ddsFile = new(d3dtx);
        var array = ddsFile.GetData(d3dtx);

        DDSImage = new DDS(array);

        bitmap = ConvertDdsToBitmap(Mip, Face);
    }

    private void GetImageDataFromDDS(out Bitmap bitmap, out ImageProperties imageProperties)
    {
        // if (filePath != currentFilePath)
        // {

        //     currentFilePath = filePath;
        // }

        DDSImage = new DDS(CurrentFilePath);

        bitmap = ConvertDdsToBitmap(Mip, Face);

        imageProperties = DDS_DirectXTexNet.GetDDSProperties(CurrentFilePath, DDSImage.Metadata);
    }

    /// <summary>
    /// Converts .dds files to a bitmap. This is only used in the image preview.
    /// </summary>
    /// <param name="ddsImage">The dds image from DirectXTexNet.</param>
    /// <returns>The png bitmap from .dds.</returns>
    private Bitmap ConvertDdsToBitmap(uint mip, uint face)
    {
        DDSImage.GetData(mip, face, out ulong width, out ulong height, out ulong pitch, out ulong length, out byte[] pixelData);
        DDSImage.GetBounds(out uint maxMip, out uint maxFace);

        MaxMip = maxMip;
        MaxFace = maxFace;
        
        // Converts the data into writeableBitmap. (TODO Insert a link to the code)
        var imageInfo = new SKImageInfo((int)width, (int)height, SKColorType.Bgra8888);
        var handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
        var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(pixelData, 0);
        using var data = SKData.Create(ptr, (int)length, (_, _) => handle.Free());
        using var skImage = SKImage.FromPixels(imageInfo, data, (int)pitch);
        using var bitmap = SKBitmap.FromImage(skImage);

        // Create a memory stream to hold the PNG data
        var memoryStream = new MemoryStream();

        // Encode the bitmap to PNG and write it to the memory stream
        var wstream = new SKManagedWStream(memoryStream);

        var success = bitmap.Encode(wstream, SKEncodedImageFormat.Png, 95);
        Console.WriteLine(success ? "Image converted successfully" : "Image conversion failed");

        memoryStream.Position = 0;

        return new Bitmap(memoryStream);
    }

    private static Bitmap GetDDSBitmap(IImage image)
    {
        var pixels = image.DataLen;
        //get the data
        var newData = image.Data;
        var newDataLen = image.DataLen;
        var stride = image.Stride;

        // Get the color type
        SKColorType colorType;
        switch (image.Format)
        {
            case ImageFormat.Rgb8:
                colorType = SKColorType.Gray8;
                break;
            case ImageFormat.R5g5b5: // Pfim doesn't support L16 and L8A8 formats. Images with these formats will be incorrectly interpreted as R5G5B5.
                pixels /= 2;
                newDataLen = pixels * 2;
                newData = new byte[newDataLen];
                for (var i = 0; i < pixels; i++)
                {
                    ushort pixelData = BitConverter.ToUInt16(image.Data, i * 2);
                    byte r = (byte)((pixelData & 0x7C00) >> 10); // Red component
                    byte g = (byte)((pixelData & 0x03E0) >> 5); // Green component
                    byte b = (byte)(pixelData & 0x001F); // Blue component
                    ushort rgb565 = (ushort)((r << 11) | (g << 5) | b); // Combine components into RGB565 format
                    byte[] rgb565Bytes = BitConverter.GetBytes(rgb565);
                    newData[i * 2] = rgb565Bytes[0];
                    newData[i * 2 + 1] = rgb565Bytes[1];
                }
                stride = image.Width * 2;
                colorType = SKColorType.Rgb565;
                break;
            case ImageFormat.R5g5b5a1:
                pixels /= 2; // Each pixel is 2 bytes in R5G5B5A1 format
                newDataLen = pixels * 4; // Each pixel will be 4 bytes in RGBA8888 format
                newData = new byte[newDataLen];
                for (var i = 0; i < pixels; i++)
                {
                    ushort pixelData = BitConverter.ToUInt16(image.Data, i * 2);
                    byte r = (byte)((pixelData & 0x7C00) >> 10); // Red component
                    byte g = (byte)((pixelData & 0x03E0) >> 5); // Green component
                    byte b = (byte)(pixelData & 0x001F); // Blue component
                    newData[i * 4] = r;
                    newData[i * 4 + 1] = g;
                    newData[i * 4 + 2] = b;
                    newData[i * 4 + 3] = 255; // Alpha channel set to 255 (fully opaque)
                }
                stride = image.Width * 4;
                colorType = SKColorType.Rgba8888;
                break;
            case ImageFormat.R5g6b5:
                colorType = SKColorType.Rgb565;
                break;
            case ImageFormat.Rgba16:
                colorType = SKColorType.Argb4444;
                break;
            case ImageFormat.Rgb24:
                // Skia has no 24bit pixels, so we upscale to 32bit
                pixels = image.DataLen / 3;
                newDataLen = pixels * 4;
                newData = new byte[newDataLen];
                for (var i = 0; i < pixels; i++)
                {
                    newData[i * 4] = image.Data[i * 3];
                    newData[i * 4 + 1] = image.Data[i * 3 + 1];
                    newData[i * 4 + 2] = image.Data[i * 3 + 2];
                    newData[i * 4 + 3] = 255;
                }

                stride = image.Width * 4;
                colorType = SKColorType.Bgra8888;
                break;
            case ImageFormat.Rgba32:
                colorType = SKColorType.Bgra8888;
                break;
            default:
                throw new ArgumentException($"Skia unable to interpret pfim format: {image.Format}");
        }

        // Converts the data into writeableBitmap. (TODO Insert a link to the code)
        var imageInfo = new SKImageInfo(image.Width, image.Height, colorType);
        var handle = GCHandle.Alloc(newData, GCHandleType.Pinned);
        var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(newData, 0);
        using var data = SKData.Create(ptr, newDataLen, (_, _) => handle.Free());
        using var skImage = SKImage.FromPixels(imageInfo, data, stride);
        using var bitmap = SKBitmap.FromImage(skImage);

        // Create a memory stream to hold the PNG data
        var memoryStream = new MemoryStream();

        // Encode the bitmap to PNG and write it to the memory stream
        var wstream = new SKManagedWStream(memoryStream);

        var success = bitmap.Encode(wstream, SKEncodedImageFormat.Png, 95);
        Console.WriteLine(success ? "Image converted successfully" : "Image conversion failed");

        memoryStream.Position = 0;

        return new Bitmap(memoryStream);
    }

    private void GetImageDataFromOthers(out Bitmap bitmap, out ImageProperties imageProperties)
    {
        // Load the image using SkiaSharp
        using (var skBitmap = SKBitmap.Decode(CurrentFilePath))
        {
            // Create a memory stream to hold the image data
            using (var ms = new MemoryStream())
            {
                // Encode the SKBitmap to a PNG and write to the memory stream
                skBitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
                ms.Seek(0, SeekOrigin.Begin); // Rewind the stream to the beginning

                // Create an Avalonia Bitmap from the memory stream
                bitmap = new Bitmap(ms);
            }

            string hasAlphaString = skBitmap.Info.IsOpaque ? "False" : "True";

            imageProperties = new ImageProperties()
            {
                Name = Path.GetFileNameWithoutExtension(CurrentFilePath),
                Extension = Path.GetExtension(CurrentFilePath),
                SurfaceFormat = skBitmap.ColorType.ToString(),
                ChannelCount = (skBitmap.BytesPerPixel * 8).ToString(),
                Height = skBitmap.Height.ToString(),
                Width = skBitmap.Width.ToString(),
                HasAlpha = hasAlphaString,
                MipMapCount = "N/A" // SkiaSharp does not provide mipmap count directly
            };
        }
    }

    private void GetImageDataFromTIFF(out Bitmap bitmap, out ImageProperties imageProperties)
    {
        bitmap = ConvertTiffToBitmap();

        var imageInfo = SixLabors.ImageSharp.Image.Identify(CurrentFilePath);
        var image = SixLabors.ImageSharp.Image.Load<Rgba32>(CurrentFilePath);

        bool hasAlpha = ImageUtilities.IsImageOpaque(image);
        string hasAlphaString = hasAlpha ? "True" : "False";

        imageProperties = new ImageProperties()
        {
            Name = Path.GetFileNameWithoutExtension(CurrentFilePath),
            Extension = Path.GetExtension(CurrentFilePath),
            SurfaceFormat = imageInfo.Metadata.DecodedImageFormat.Name,
            ChannelCount = (imageInfo.PixelType.BitsPerPixel / 8).ToString(),
            Height = imageInfo.Height.ToString(),
            Width = imageInfo.Width.ToString(),
            HasAlpha = hasAlphaString,
            MipMapCount = "N/A"
        };
    }

    private void GetImageDataFromInvalid(out Bitmap? bitmap, out ImageProperties imageProperties)
    {
        bitmap = null;

        imageProperties = new ImageProperties()
        {
            Name = "",
            SurfaceFormat = "",
            ChannelCount = "",
            Height = "",
            Width = "",
            MipMapCount = "",
            HasAlpha = ""
        };
    }

    /// <summary>
    /// Converts .tiff to a bitmap. This is only used in the image preview. 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private Bitmap ConvertTiffToBitmap()
    {
        byte[] fileBytes = File.ReadAllBytes(CurrentFilePath);
        Stream tiffStream = new MemoryStream(fileBytes);
        // open a TIFF stored in the stream
        using var tifImg = Tiff.ClientOpen("in-memory", "r", tiffStream, new TiffStream());
        // read the dimensions
        var width = tifImg.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
        var height = tifImg.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

        //Experimentation, ignore this
        //var smth = tifImg.GetField(TiffTag.COMPRESSION)[0].ToInt();

        // create the bitmap
        var bitmap = new SKBitmap();
        var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul,
            SKColorSpace.CreateSrgb());

        // create the buffer that will hold the pixels
        var raster = new int[width * height];

        // get a pointer to the buffer, and give it to the bitmap
        var ptr = GCHandle.Alloc(raster, GCHandleType.Pinned);
        bitmap.InstallPixels(info, ptr.AddrOfPinnedObject(), info.RowBytes);


        // read the image into the memory buffer
        if (!tifImg.ReadRGBAImageOriented(width, height, raster,
                BitMiracle.LibTiff.Classic.Orientation.TOPLEFT))
        {
            // not a valid TIF image.
            return null;
        }

        // swap the red and blue because SkiaSharp may differ from the tiff
        if (SKImageInfo.PlatformColorType == SKColorType.Bgra8888)
        {
            SKSwizzle.SwapRedBlue(ptr.AddrOfPinnedObject(), raster.Length);
        }

        var writeableBitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96),
            PixelFormat.Bgra8888);

        using var lockedBitmap = writeableBitmap.Lock();
        // Copy the SKBitmap pixel data to the Avalonia WriteableBitmap
        Marshal.Copy(bitmap.Bytes, 0, lockedBitmap.Address, bitmap.Bytes.Length);

        return writeableBitmap;
    }

    /// <summary>
    /// Gets the properties of the selected .dds file
    /// </summary>
    /// <param name="ddsFilePath"></param>
    private static ImageProperties GetKtx2Properties(string ddsFilePath)
    {
        Texture texture = KTX2_Bindings.GetKTX2Texture(ddsFilePath);

        return new ImageProperties
        {
            Name = Path.GetFileNameWithoutExtension(ddsFilePath),
            Extension = ".ktx2",
            Height = texture.BaseHeight.ToString(),
            Width = texture.BaseWidth.ToString(),
            SurfaceFormat = texture.VkFormat.ToString(),
            HasAlpha = KTX2_HELPER.HasAlpha(texture.VkFormat) ? "True" : "False",
            //ChannelCount = Helper.GetDataFormatDescriptor(texture.VkFormat).DescriptorBlockSize.ToString(),
            MipMapCount = texture.NumLevels.ToString()
        };
    }

    public static ImageProperties GetImagePropertiesFromInvalid()
    {
        return new ImageProperties()
        {
            Name = "",
            SurfaceFormat = "",
            ChannelCount = "",
            Height = "",
            Width = "",
            MipMapCount = "",
            HasAlpha = ""
        };
    }
}