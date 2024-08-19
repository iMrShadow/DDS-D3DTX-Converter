using System;
using System.IO;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.Main;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace TelltaleTextureTool;

public class ImageData
{
    public ImageProperties ImageProperties { get; set; } = new ImageProperties();
    public Texture DDSImage { get; set; } = new Texture();

    public uint MaxMip { get; set; }
    public uint MaxFace { get; set; }

    private string CurrentFilePath { get; set; } = string.Empty;
    private bool IsSamePath { get; set; }
    private TextureType CurrentTextureType { get; set; }

    private D3DTXVersion CurrentD3DTXVersion { get; set; }

    public void Initialize(string filePath, TextureType textureType, D3DTXVersion d3DTXVersion = D3DTXVersion.DEFAULT, uint mip = 0, uint face = 0)
    {
        IsSamePath = CurrentFilePath == filePath;

        if (!IsSamePath && CurrentFilePath != string.Empty)
        {
            if (CurrentTextureType != TextureType.Unknown)
            {
                DDSImage.Release();
            }
        }

        CurrentFilePath = filePath;
        CurrentTextureType = textureType;
        CurrentD3DTXVersion = d3DTXVersion;

        GetImageData(out ImageProperties imageProperties);

        ImageProperties = imageProperties;
    }

    /// <summary>
    /// Applies the effects to the image.
    /// </summary>
    /// <param name="options"></param>
    public void ApplyEffects(ImageAdvancedOptions options)
    {
        DDSImage.ChangePreviewImage(options);

        DDSImage.GetBounds(out uint maxMip, out uint maxFace);
        MaxMip = maxMip;
        MaxFace = maxFace;

        ImageProperties = DDS_DirectXTexNet.GetDDSProperties(CurrentFilePath, DDSImage.Metadata);
    }

    private void GetImageData(out ImageProperties imageProperties)
    {
        switch (CurrentTextureType)
        {
            case TextureType.DDS:
            case TextureType.BMP:
            case TextureType.PNG:
            case TextureType.TGA:
            case TextureType.JPEG:
            case TextureType.TIFF:
            case TextureType.HDR:
                GetImageDataFromCommon(out imageProperties);
                break;

            case TextureType.D3DTX:
                GetImageDataFromD3DTX(CurrentD3DTXVersion, out imageProperties);
                break;
            default:
                GetImageDataFromInvalid(out imageProperties);
                break;
        }
    }

    private void GetImageDataFromD3DTX(D3DTXVersion d3DTXVersion, out ImageProperties imageProperties)
    {
        var d3dtx = new D3DTX_Master();
        d3dtx.ReadD3DTXFile(CurrentFilePath, d3DTXVersion);

        // Initialize image properties
        D3DTXMetadata metadata = d3dtx.GetMetadata();

        imageProperties = new ImageProperties()
        {
            Name = metadata.TextureName,
            SurfaceFormat = d3dtx.GetSurfaceFormat(),
            Width = metadata.Width.ToString(),
            Height = metadata.Height.ToString(),
            HasAlpha = d3dtx.GetHasAlpha(),
            // ChannelCount = d3dtx.GetChannelCount(),
            MipMapCount = metadata.MipLevels.ToString(),
            TextureLayout = metadata.Dimension.ToString(),
            AlphaMode = metadata.AlphaMode.ToString(),
            ArraySize = metadata.ArraySize.ToString(),
        };

        if (!IsSamePath)
        {
            // Initialize image bitmap
            DDS_Master ddsFile = new(d3dtx);
            var array = ddsFile.GetData(d3dtx);

            DDSImage = new DirectX.Texture(array, TextureType.D3DTX);
        }
    }

    /// <summary>
    /// Gets the pre-defined advanced options for the image.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public ImageAdvancedOptions GetImageAdvancedOptions(ImageAdvancedOptions options)
    {
        if (CurrentTextureType != TextureType.D3DTX)
        {
            return options;
        }

        var d3dtx = new D3DTX_Master();
        d3dtx.ReadD3DTXFile(CurrentFilePath, CurrentD3DTXVersion);

        // Initialize image properties
        D3DTXMetadata metadata = d3dtx.GetMetadata();

        if (D3DTX_Master.IsPlatformIncompatibleWithDDS(metadata.Platform))
        {
            // options.EnableSwizzle = true;
            // options.IsDeswizzle = true;
            // options.PlatformType = metadata.Platform;
        }

        return options;
    }

    private void GetImageDataFromCommon(out ImageProperties imageProperties)
    {
        if (!IsSamePath)
        {
            DDSImage = new Texture(CurrentFilePath, CurrentTextureType);
        }

        imageProperties = DDS_DirectXTexNet.GetDDSProperties(CurrentFilePath, DDSImage.Metadata);
    }

    /// <summary>
    /// Converts the data from the scratch image to a bitmap.
    /// </summary>
    /// <param name="mip"></param>
    /// <param name="face"></param>
    /// <returns>The bitmap from the mip and face. </returns>
    public Bitmap GetBitmapFromScratchImage(uint mip = 0, uint face = 0)
    {
        if (TextureType.Unknown == CurrentTextureType)
        {
            return new Bitmap(MemoryStream.Null);
        }

        DDSImage.GetBounds(out uint maxMip, out uint maxFace);
        MaxMip = maxMip;
        MaxFace = maxFace;

        if (mip > maxMip)
        {
            mip = MaxMip;
        }
        if (face > maxFace)
        {
            face = MaxFace;
        }

        DDSImage.GetData(mip, face, out ulong width, out ulong height, out ulong pitch, out ulong length, out byte[] pixelData);

        // Converts the data into writeableBitmap. (TODO Insert a link to the code)
        var imageInfo = new SKImageInfo((int)width, (int)height, SKColorType.Rgba8888);
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

    private static void GetImageDataFromInvalid(out ImageProperties imageProperties)
    {
        imageProperties = new();
    }
}