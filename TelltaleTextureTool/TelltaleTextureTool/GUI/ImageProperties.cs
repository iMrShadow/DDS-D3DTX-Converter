using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.Main;
using TelltaleTextureTool.Utilities;
using SixLabors.ImageSharp.PixelFormats;
using static Ktx.Ktx2;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;

namespace TelltaleTextureTool;

public class ImageProperties : ObservableObject
{
    /// <summary>
    /// Image properties that are displayed on the panel.
    /// </summary>
    public string? Name { get; set; }
    public string? Extension { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? CompressionType { get; set; }
    public string? HasAlpha { get; set; }

    public string? BitsPerPixel { get; set; }
    public string? ChannelCount { get; set; }
    public string? MipMapCount { get; set; }

    public static ImageProperties GetImagePropertiesFromD3DTX(string filePath, D3DTXVersion d3DTXConversionType = D3DTXVersion.DEFAULT)
    {
        var master = new D3DTX_Master();
        master.ReadD3DTXFile(filePath, d3DTXConversionType);

        D3DTXMetadata metadata = master.GetMetadata();

        return new ImageProperties()
        {
            Name = metadata.TextureName,
            CompressionType = master.GetStringFormat(),
            Width = metadata.Width.ToString(),
            Height = metadata.Height.ToString(),
            HasAlpha = master.GetHasAlpha(),
            ChannelCount = master.GetChannelCount(),
            MipMapCount =metadata.MipLevels.ToString()
        };
    }

    /// <summary>
    /// Gets the properties of the selected .dds file
    /// </summary>
    /// <param name="ddsFilePath"></param>
    public static ImageProperties GetDdsProperties(string ddsFilePath)
    {
        return DDS_DirectXTexNet.GetDDSProperties(ddsFilePath);
    }

    /// <summary>
    /// Gets the properties of the selected .dds file
    /// </summary>
    /// <param name="ddsFilePath"></param>
    public static ImageProperties GetKtx2Properties(string ddsFilePath)
    {
        Texture texture = KTX2_Bindings.GetKTX2Texture(ddsFilePath);

        return new ImageProperties
        {
            Name = Path.GetFileNameWithoutExtension(ddsFilePath),
            Extension = ".ktx2",
            Height = texture.BaseHeight.ToString(),
            Width = texture.BaseWidth.ToString(),
            CompressionType = texture.VkFormat.ToString(),
            HasAlpha = KTX2_HELPER.HasAlpha(texture.VkFormat) ? "True" : "False",
            //ChannelCount = Helper.GetDataFormatDescriptor(texture.VkFormat).DescriptorBlockSize.ToString(),
            MipMapCount = texture.NumLevels.ToString()
        };
    }

    /// <summary>
    /// Get the needed image properties from the selected file, excluding .dds and .d3dtx.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static ImageProperties GetImagePropertiesFromOthers(string filePath)
    {
        var imageInfo = SixLabors.ImageSharp.Image.Identify(filePath);

        var image = SixLabors.ImageSharp.Image.Load<Rgba32>(filePath);

        bool hasAlpha = ImageUtilities.IsImageOpaque(image);

        string hasAlphaString = hasAlpha ? "True" : "False";

        return new ImageProperties()
        {
            Name = Path.GetFileNameWithoutExtension(filePath),
            Extension = Path.GetExtension(filePath),
            CompressionType = imageInfo.Metadata.DecodedImageFormat.Name,
            ChannelCount = (imageInfo.PixelType.BitsPerPixel / 8).ToString(),
            Height = imageInfo.Height.ToString(),
            Width = imageInfo.Width.ToString(),
            HasAlpha = hasAlphaString,
            MipMapCount = "N/A"
        };

        throw new Exception("Error during getting image properties. ");
    }

    /// <summary>
    /// Helper function. Creates new empty ImageProperties to display unsupported files.
    /// </summary>
    /// <returns></returns>
    public static ImageProperties GetImagePropertiesFromInvalid()
    {
        return new ImageProperties()
        {
            Name = "",
            CompressionType = "",
            ChannelCount = "",
            Height = "",
            Width = "",
            MipMapCount = "",
            HasAlpha = ""
        };
    }
}