using CommunityToolkit.Mvvm.ComponentModel;

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
    public string? TextureLayout { get; set; }
}