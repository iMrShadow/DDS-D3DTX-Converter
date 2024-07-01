namespace TelltaleTextureTool.TexconvOptions;

/// <summary>
/// Output directory.
/// </summary>
public class OutputDirectory
{
    public string directory;

    public string GetArgumentOutput()
    {
        if (directory.Contains(" "))
        {
            return string.Format("-o \"{0}\"", directory);
        }
        else
        {
            return string.Format("-o {0}", directory);
        }
    }
}
