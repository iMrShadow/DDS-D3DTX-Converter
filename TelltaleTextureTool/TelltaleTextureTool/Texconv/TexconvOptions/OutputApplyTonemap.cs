namespace TelltaleTextureTool.TexconvOptions;

/// <summary>
/// Applies tonemap operator (reinhard) based on maximum luminosity to ensure HDR image data is adjusted to an LDR range.
/// </summary>
public class OutputApplyTonemap
{
    public string GetArgumentOutput() => "-tonemap";
}
