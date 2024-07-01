﻿namespace TelltaleTextureTool.TexconvOptions;

/// <summary>
/// When encoding WIC images, the use of this flag will encode multiframe images.
/// <para>By default it writes only the first frame like the hdr and tga writers.</para>
/// <para>Note that only some WIC containers support multiframe images (notably gif and tif).</para>
/// </summary>
public class OutputWICMultiFrameEncoding
{
    public string GetArgumentOutput() => "-wicmulti";
}
