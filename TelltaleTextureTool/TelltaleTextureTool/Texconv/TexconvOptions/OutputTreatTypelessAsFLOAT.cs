namespace TelltaleTextureTool.TexconvOptions;

/// <summary>
/// DDS files with TYPELESS formats are treated as FLOAT
/// </summary>
public class OutputTreatTypelessAsFLOAT
{
    public string GetArgumentOutput() => "-tf";
}
