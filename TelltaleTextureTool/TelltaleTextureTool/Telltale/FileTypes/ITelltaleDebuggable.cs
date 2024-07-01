namespace TelltaleTextureTool.FileTypes;

/// <summary>
/// Interface for serializing a Telltale object.
/// </summary>
public interface ITelltaleDebuggable
{
    /// <summary>
    /// Gets debug information about the Telltale object.
    /// </summary>
    /// <returns>A string containing debug information.</returns>
    /// Note: I am aware that strings can be built with StringBuilder for better performance.
    string GetDebugInfo();
}
