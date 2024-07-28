
using TelltaleTextureTool.DirectX.Enums;
using TelltaleTextureTool.Utilities;
using Hexa.NET.DirectXTex;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using TelltaleTextureTool.Telltale.FileTypes.D3DTX;
using System.Numerics;
using HexaGen.Runtime;

namespace TelltaleTextureTool.DirectX;

/// <summary>
/// Image section of texture file. Contains width, height, format, slice pitch, row pitch and the pixels.
/// </summary>
public struct ImageSection
{
    public nuint Width;
    public nuint Height;
    public DXGIFormat Format;
    public nuint SlicePitch;
    public nuint RowPitch;
    public byte[] Pixels;
};

/// <summary>
/// A class that provides methods to interact with DirectXTexNet. Mainly used for loading and saving DDS files.
/// </summary>
public unsafe static partial class DDS_DirectXTexNet
{

    /// <summary>
    /// Get the DDS image from DirectXTexNet.
    /// </summary>
    /// <param name="ddsFilePath">The file path of the .dds file.</param>
    /// <param name="flags">(Optional) The mode in which the DirectXTexNet will load the .dds file. If not provided, it defaults to NONE.</param>
    /// <returns>ScratchImage instance of the DDS file.</returns>
    public static void GetDDSImage(string ddsFilePath, out ScratchImage image, out TexMetadata metadata, DDSFlags flags = DDSFlags.None)
    {
        image = GetDDSImage(ByteFunctions.LoadTexture(ddsFilePath), flags);
        metadata = image.GetMetadata();
    }

    public static string GetDDSDebugInfo(string ddsFilePath)
    {
        ScratchImage image = GetDDSImage(ByteFunctions.LoadTexture(ddsFilePath));

        string debugInfo = GetDDSDebugInfo(image.GetMetadata());

        image.Release();

        return debugInfo;
    }

    public static ImageProperties GetDDSProperties(string ddsFilePath, TexMetadata ddsMetadata, DDSFlags flags = DDSFlags.None)
    {
        DXGIFormat dxgiFormat = (DXGIFormat)ddsMetadata.Format;

        uint channelCount = (uint)Math.Ceiling((double)DirectXTex.BitsPerPixel((uint)dxgiFormat) / Math.Max(1, DirectXTex.BitsPerColor((uint)dxgiFormat)));

        string hasAlpha = DirectXTex.HasAlpha((uint)dxgiFormat) ? "True" : "False";

        ImageProperties properties = new()
        {
            Name = Path.GetFileNameWithoutExtension(ddsFilePath),
            Extension = ".dds",
            Height = ddsMetadata.Height.ToString(),
            Width = ddsMetadata.Width.ToString(),
            SurfaceFormat = dxgiFormat.ToString(),
            HasAlpha = hasAlpha,
            ChannelCount = channelCount.ToString(),
            MipMapCount = ddsMetadata.MipLevels.ToString()
        };

        return properties;
    }

    /// <summary>
    /// Compute the pitch given the Direct3D10/DXGI format, the width and the height.
    /// </summary>
    /// <param name="dxgiFormat">The Direct3D10/DXGI format.</param>
    /// <param name="width">The width of the DDS image.</param>
    /// <param name="height">(Optional) The height of the DDS image. If not provided, it defaults to 0.</param>
    /// <returns>The pitch.</returns>
    public static uint ComputePitch(DXGIFormat dxgiFormat, ulong width, ulong height = 1)
    {
        nuint rowPitch;
        nuint slicePitch;

        DirectXTex.ComputePitch((uint)dxgiFormat, width, height, (ulong*)&rowPitch, (ulong*)&slicePitch, CPFlags.None);
        return (uint)rowPitch;
    }

    /// <summary>
    /// Returns the channel count of a Direct3D10/DXGI format. It is used in the image properties display.
    /// </summary>
    /// <param name="dxgiFormat">The Direct3D10/DXGI format</param>
    /// <returns>The channel count.</returns>
    public static uint GetChannelCount(DXGIFormat dxgiFormat) => (uint)Math.Ceiling((double)DirectXTex.BitsPerPixel((uint)dxgiFormat) / Math.Max(1, DirectXTex.BitsPerColor((uint)dxgiFormat)));

    /// <summary>
    /// Returns a DirectXTexNet DDS image from a byte array.
    /// </summary>
    /// <param name="array">The byte array containing the DDS data.</param>
    /// <param name="flags">(Optional) The mode in which the DirectXTexNet will load the .dds file. If not provided, it defaults to NONE.</param>
    /// <returns>The DirectXTexNet DDS image.</returns>
    unsafe public static ScratchImage GetDDSImage(byte[] array, DDSFlags flags = DDSFlags.None)
    {
        GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        try
        {
            ScratchImage image = DirectXTex.CreateScratchImage();
            TexMetadata metadata;

            // Obtain a pointer to the data
            IntPtr ptr = handle.AddrOfPinnedObject();
            DirectXTex.LoadFromDDSMemory((void*)ptr, (nuint)array.Length, flags, &metadata, image);
            return image;
        }
        finally
        {
            // Release the handle to allow the garbage collector to reclaim the memory
            handle.Free();
        }
    }

    /// <summary>
    /// Returns a byte array from a DirectXTexNet DDS image.
    /// </summary>
    /// <param name="image">The DirectXTexNet DDS image.</param>
    /// <param name="flags">(Optional) The mode in which the DirectXTexNet will load the .dds file. If not provided, it defaults to NONE.</param>
    /// <returns>The byte array containing the DDS data.</returns>
    public static byte[] GetDDSByteArray(ref ScratchImage image, DDSFlags flags = DDSFlags.None)
    {
        Blob blob = DirectXTex.CreateBlob();
        try
        {
            Console.WriteLine("Image count: " + image.GetImageCount());
            Console.WriteLine("Image format: " + image.GetMetadata().Format);
            DirectXTex.SaveToDDSMemory2(image.GetImages(), image.GetImageCount(), image.GetMetadata(), flags, blob);
            // Create a byte array to hold the data
            Console.WriteLine(blob.GetBufferSize());
            Console.WriteLine((nint)blob.GetBufferPointer());

            byte[] ddsArray = new byte[blob.GetBufferSize()];

            Console.WriteLine("BLOB BUFFER SIZE: " + blob.GetBufferSize());

            // Read the data from the Blob into the byte array
            Marshal.Copy((nint)blob.GetBufferPointer(), ddsArray, 0, ddsArray.Length);
            return ddsArray;
        }
        finally
        {
            blob.Release();
        }
    }

    public static void GetDDSInformation(string ddsFilePath, out D3DTXMetadata metadata, out ImageSection[] sections, DDSFlags flags = DDSFlags.None)
    {
        ScratchImage image = GetDDSImage(ByteFunctions.LoadTexture(ddsFilePath), flags);

        metadata = GetDDSInformation(image.GetMetadata());
        sections = GetDDSImageSections(image, flags);

        image.Release();
    }

    public static D3DTXMetadata GetDDSInformation(TexMetadata metadata)
    {
        return new D3DTXMetadata
        {
            Width = (uint)metadata.Width,
            Height = (uint)metadata.Height,
            Depth = (uint)metadata.Depth,
            ArraySize = (uint)metadata.ArraySize,
            MipLevels = (uint)metadata.MipLevels,
            Format = DDS_HELPER.GetTelltaleSurfaceFormat((DXGIFormat)metadata.Format),
            SurfaceGamma = DirectXTex.IsSRGB((uint)metadata.Format) ? TelltaleEnums.T3SurfaceGamma.eSurfaceGamma_sRGB : TelltaleEnums.T3SurfaceGamma.eSurfaceGamma_Linear,
            D3DFormat = DDS_HELPER.GetD3DFORMAT((DXGIFormat)metadata.Format, metadata),
            Dimension = DDS_HELPER.GetDimensionFromDDS(metadata),
        };
    }

    /// <summary>
    /// Returns a byte array List containing the pixel data from a DDS_DirectXTexNet_ImageSection array.
    /// </summary>
    /// <param name="sections">The sections of the DDS image.</param>
    /// <returns></returns>
    public static List<byte[]> GetPixelDataListFromSections(ImageSection[] sections)
    {
        List<byte[]> textureData = [];

        foreach (ImageSection imageSection in sections)
        {
            textureData.Add(imageSection.Pixels);
        }

        return textureData;
    }

    /// <summary>
    /// Returns a byte array List containing the pixel data from a DDS_DirectXTexNet_ImageSection array.
    /// </summary>
    /// <param name="sections">The sections of the DDS image.</param>
    /// <returns></returns>
    public static byte[] GetPixelDataArrayFromSections(ImageSection[] sections)
    {
        byte[] textureData = [];

        foreach (ImageSection imageSection in sections)
        {
            textureData = ByteFunctions.Combine(textureData, imageSection.Pixels);
        }

        return textureData;
    }


    public static UnmanagedMemoryStream GetUnmanagedMemoryStreamFromMemory(byte[] array)
    {
        ScratchImage image = DirectXTex.CreateScratchImage();
        Blob blob = DirectXTex.CreateBlob();
        Console.WriteLine(array.Length);
        Span<byte> src = array;

        TexMetadata metadata;
        fixed (byte* srcPtr = src)
        {
            int i = DirectXTex.LoadFromDDSMemory(srcPtr, (nuint)src.Length, DDSFlags.None, &metadata, image);
            Console.WriteLine(GetDDSDebugInfo(metadata));
        }

        if (image.GetImageCount() == 0)
        {
            throw new Exception("Invalid DDS file!");
        }

        var ddsMainImage = image.GetImage(0, 0, 0);
        bool isCompressed = DirectXTex.IsCompressed(ddsMainImage.Format);

        // If the image is compressed, decompress it to RGBA32. Otherwise, convert it to RGBA32.
        if (isCompressed)
        {
            ScratchImage image1 = DirectXTex.CreateScratchImage();
            DirectXTex.Decompress2(image.GetImages(), image.GetImageCount(), image.GetMetadata(), (int)DXGIFormat.R8G8B8A8_UNORM, image1);
            DirectXTex.SaveToDDSMemory(ddsMainImage, DDSFlags.None, blob);

            image1.Release();
        }
        else
        {
            //  (TODO Insert a link to the code)
            ScratchImage image1 = DirectXTex.CreateScratchImage();
            if (ddsMainImage.Format != (int)DXGIFormat.R8G8B8A8_UNORM)
            {
                DirectXTex.Convert(ddsMainImage, (int)DXGIFormat.R8G8B8A8_UNORM, TexFilterFlags.Default, 0.5f, image1);
                DirectXTex.SaveToDDSMemory(image1.GetImage(0, 0, 0), DDSFlags.None, blob);
            }
            else DirectXTex.SaveToDDSMemory(ddsMainImage, DDSFlags.None, blob);

            image1.Release();
        }

        Span<byte> dest = new(blob.GetBufferPointer(), (int)blob.GetBufferSize());

        // Convert Span<byte> to byte[]
        byte[] byteArray = dest.ToArray();

        blob.Release();
        image.Release();

        // Allocate unmanaged memory for the byte array
        IntPtr ptr = Marshal.AllocHGlobal(byteArray.Length);

        // Copy the byte array to unmanaged memory
        Marshal.Copy(byteArray, 0, ptr, byteArray.Length);

        return new UnmanagedMemoryStream((byte*)ptr.ToPointer(), byteArray.Length);
    }

    /// <summary>
    /// Returns the image sections of the DDS image. Each mipmap and slice is a section on its own. 
    /// </summary>
    /// <param name="ddsImage">The DirectXTexNet DDS image.</param>
    /// <param name="flags">(Optional) The mode in which the DirectXTexNet will load the .dds file. If not provided, it defaults to NONE.</param>
    /// <returns>The DDS sections</returns>
    public static ImageSection[] GetDDSImageSections(ScratchImage ddsImage, DDSFlags flags = DDSFlags.None)
    {
        List<ImageSection> sections = [];

        if (flags == DDSFlags.ForceDx9Legacy)
        {
            sections.Add(new()
            {
                Pixels = GetDDSHeaderBytes(ddsImage)
            });
        }

        Image[] images = GetImages(ddsImage);

        for (int i = 0; i < images.Length; i++)
        {
            byte[] pixels = new byte[images[i].SlicePitch];

            Marshal.Copy((nint)images[i].Pixels, pixels, 0, pixels.Length);

            sections.Add(new()
            {
                Width = (nuint)images[i].Width,
                Height = (nuint)images[i].Height,
                Format = (DXGIFormat)images[i].Format,
                SlicePitch = (nuint)images[i].SlicePitch,
                RowPitch = (nuint)images[i].RowPitch,
                Pixels = pixels
            });
        }

        for (int i = 0; i < sections.Count; i++)
        {
            Console.WriteLine($"Image {i} - Width: {sections[i].Width}, Height: {sections[i].Height}, Format: {sections[i].Format}, SlicePitch: {sections[i].SlicePitch}, RowPitch: {sections[i].RowPitch}");
            Console.WriteLine($"Image {i} - Pixels: {sections[i].Pixels.Length}");
        }

        return sections.ToArray();
    }

    public static byte[] GetDDSHeaderBytes(ScratchImage image, DDSFlags flags = DDSFlags.None)
    {
        Blob blob = DirectXTex.CreateBlob();
        DirectXTex.SaveToDDSMemory2(image.GetImages(), image.GetImageCount(), image.GetMetadata(), flags, blob);

        blob.GetBufferPointer();

        // Extract the DDS header from the blob
        byte[] ddsHeaderCheckDX10 = new byte[148];
        Marshal.Copy((nint)blob.GetBufferPointer(), ddsHeaderCheckDX10, 0, ddsHeaderCheckDX10.Length);
        blob.Release();

        byte[] ddsHeader;
        int size;
        if (ddsHeaderCheckDX10[84] == 0x44 && ddsHeaderCheckDX10[85] == 0x58 && ddsHeaderCheckDX10[86] == 0x31 && ddsHeaderCheckDX10[87] == 0x30)
        {
            size = 148;
        }
        else
        {
            size = 128;
        }

        ddsHeader = new byte[size];
        Array.Copy(ddsHeaderCheckDX10, 0, ddsHeader, 0, size);

        return ddsHeader;
    }

    private static Image[] GetImages(ScratchImage image)
    {
        Image* pointerImages = DirectXTex.GetImages(image);

        int imageCount = (int)image.GetImageCount();

        Image[] images = new Image[imageCount];

        for (int i = 0; i < imageCount; i++)
        {
            images[i] = pointerImages[i];
        }

        return images;
    }

    public static byte[] AsBytes(Blob blob)
    {
        byte[] bytes = new byte[blob.GetBufferSize()];
        Marshal.Copy((nint)blob.GetBufferPointer(), bytes, 0, bytes.Length);
        return bytes;
    }

    /// <summary>
    /// Returns a boolean if a Direct3D10/DXGI format is sRGB.
    /// </summary>
    /// <param name="dxgiFormat">The Direct3D10/DXGI format.</param>
    /// <returns>True, if it is SRGB. Otherwise - false.</returns>
    public static bool IsSRGB(DXGIFormat dxgiFormat)
    {
        return DirectXTex.IsSRGB((uint)dxgiFormat);
    }

    /// <summary>
    /// Returns information about the DDS image.
    /// </summary>
    /// <param name="metadata">The metadata of the DDS image.</param>
    /// <returns>The string containing the DDS metadata information</returns>
    public static string GetDDSDebugInfo(TexMetadata metadata)
    {
        string information = "";

        information += "||||||||||| DDS Debug Information |||||||||||" + Environment.NewLine;
        information += $"Width: {metadata.Width}" + Environment.NewLine;
        information += $"Height: {metadata.Height}" + Environment.NewLine;
        information += $"Depth: {metadata.Depth}" + Environment.NewLine;
        information += $"Format: {Enum.GetName((DXGIFormat)metadata.Format)} " + "(" + metadata.Format + ")" + Environment.NewLine;
        information += $"Mips: {metadata.MipLevels}" + Environment.NewLine;
        information += $"Dimension: {metadata.Dimension}" + Environment.NewLine;
        information += $"Array Elements: {metadata.ArraySize}" + Environment.NewLine;
        information += $"Volumemap: {metadata.IsVolumemap()} " + Environment.NewLine;
        information += $"Cubemap: {metadata.IsCubemap()} " + Environment.NewLine;
        information += $"Alpha mode: {metadata.GetAlphaMode()} " + Environment.NewLine;
        information += $"Premultiplied alpha: {metadata.IsPMAlpha()} " + Environment.NewLine;
        information += $"Misc Flags: {metadata.MiscFlags}" + Environment.NewLine;
        information += $"Misc Flags2: {metadata.MiscFlags2}" + Environment.NewLine;

        return information;
    }
    public enum DDSConversionMode
    {
        DEFAULT = 0,
        RESTORE_Z = 1,
        SWIZZLE_MASK_RG00 = 2,
        SWIZZLE_ABGR = 3,
    }

    unsafe public static void SaveDDSToWIC(string ddsFilePath, string destinationDirectory, TextureType textureType, DDSConversionMode conversionMode = DDSConversionMode.DEFAULT)
    {
        GetDDSImage(ddsFilePath, out ScratchImage image, out TexMetadata metadata);

        var fileName = Path.GetFileNameWithoutExtension(ddsFilePath) + Converter.GetExtension(textureType)[0];
        var path = Path.Combine(destinationDirectory, fileName);

        ScratchImage rawImage = DirectXTex.CreateScratchImage();
        HResult result = 0;

        if (DirectXTex.IsCompressed(metadata.Format))
        {
            result = DirectXTex.Decompress2(image.GetImages(), image.GetImageCount(), image.GetMetadata(), (int)DXGIFormat.R8G8B8A8_UNORM, rawImage);
        }
        else
        {
            Rect rect = new() { X = 0, Y = 0, W = metadata.Width, H = metadata.Height };
            DirectXTex.Initialize(rawImage, image.GetMetadata(), CPFlags.None);
            result = DirectXTex.CopyRectangle(image.GetImage(0, 0, 0), rect, rawImage.GetImage(0, 0, 0), TexFilterFlags.Default, 0, 0);
        }

        result.ThrowIf();

        image.Release();

        TransformImageFunc transformFunction = DefaultCopy;

        if (conversionMode == DDSConversionMode.RESTORE_Z)
        {
            transformFunction = RestoreZ;
        }
        else if (conversionMode == DDSConversionMode.SWIZZLE_MASK_RG00)
        {
            transformFunction = ApplyMaskRG00;
        }
        else if (conversionMode == DDSConversionMode.SWIZZLE_ABGR)
        {
            transformFunction = ReverseChannels;
        }

        ScratchImage finalImage = DirectXTex.CreateScratchImage();

        if (transformFunction != null)
        {
            result = DirectXTex.TransformImage(rawImage.GetImage(0, 0, 0), transformFunction, finalImage);
        }
        else
        {
            result = DirectXTex.TransformImage(rawImage.GetImage(0, 0, 0), transformFunction, finalImage);
        }

        result.ThrowIf();

        rawImage.Release();

        Blob blob = DirectXTex.CreateBlob();

        if (textureType == TextureType.TGA)
        {
            result = DirectXTex.SaveToTGAMemory2(finalImage.GetImage(0, 0, 0), blob, &metadata);
        }
        else
        {
            result = DirectXTex.SaveToWICMemory(finalImage.GetImages()[0], WICFlags.None, DirectXTex.GetWICCodec(GetWicDecoder(textureType)), blob, null, default);
        }

        var bytes = AsBytes(blob);

        blob.Release();
        finalImage.Release();
        result.ThrowIf();

        File.WriteAllBytes(path, bytes);
    }

    unsafe public static void GetImageData(byte[] ddsData, out MemoryStream ddsMemoryStream, out TexMetadata texMetadata)
    {
        ScratchImage image = GetDDSImage(ddsData);
        texMetadata = image.GetMetadata();

        ScratchImage rawImage = DirectXTex.CreateScratchImage();

        HResult result = 0;

        if (DirectXTex.IsValid(texMetadata.Format))
        {
            if (DirectXTex.IsCompressed(texMetadata.Format))
            {
                result = DirectXTex.Decompress2(image.GetImages(), image.GetImageCount(), image.GetMetadata(), (int)DXGIFormat.R8G8B8A8_UNORM, rawImage);
            }
            else
            {
                Rect rect = new() { X = 0, Y = 0, W = texMetadata.Width, H = texMetadata.Height };
                DirectXTex.Initialize(rawImage, image.GetMetadata(), CPFlags.None);
                result = DirectXTex.CopyRectangle(image.GetImage(0, 0, 0), rect, rawImage.GetImage(0, 0, 0), TexFilterFlags.Default, 0, 0);
            }
        }

        image.Release();
        result.ThrowIf();

        Blob blob = DirectXTex.CreateBlob();

        result = DirectXTex.SaveToWICMemory(rawImage.GetImages()[0], WICFlags.No16Bpp, DirectXTex.GetWICCodec(WICCodecs.CodecPng), blob, null, default);

        Span<byte> dest = new(blob.GetBufferPointer(), (int)blob.GetBufferSize());

        // Convert Span<byte> to byte[]
        byte[] byteArray = dest.ToArray();

        ddsMemoryStream = new MemoryStream(byteArray);
    }

    unsafe public static void SaveWICToNormalMap(string ddsFilePath, string destinationDirectory, TextureType textureType, DDSConversionMode conversionMode = DDSConversionMode.DEFAULT)
    {
        GetDDSImage(ddsFilePath, out ScratchImage image, out TexMetadata metadata);

        var fileName = Path.GetFileNameWithoutExtension(ddsFilePath) + Converter.GetExtension(textureType)[0];
        var path = Path.Combine(destinationDirectory, fileName);

        ScratchImage rawImage = DirectXTex.CreateScratchImage();
        HResult result = 0;

        if (DirectXTex.IsCompressed(metadata.Format))
        {
            result = DirectXTex.Decompress2(image.GetImages(), image.GetImageCount(), image.GetMetadata(), (int)DXGIFormat.R8G8B8A8_UNORM, rawImage);
        }
        else
        {
            Rect rect = new() { X = 0, Y = 0, W = metadata.Width, H = metadata.Height };

            result = DirectXTex.CopyRectangle(image.GetImage(0, 0, 0), rect, rawImage.GetImage(0, 0, 0), TexFilterFlags.Default, 0, 0);
        }

        result.ThrowIf();

        image.Release();

        TransformImageFunc transformFunction = null;

        if (conversionMode == DDSConversionMode.RESTORE_Z)
        {
            transformFunction = RestoreZ;
        }
        else if (conversionMode == DDSConversionMode.SWIZZLE_MASK_RG00)
        {
            transformFunction = ApplyMaskRG00;
        }
        else if (conversionMode == DDSConversionMode.SWIZZLE_ABGR)
        {
            transformFunction = ReverseChannels;
        }

        ScratchImage finalImage = DirectXTex.CreateScratchImage();

        if (transformFunction != null)
        {
            result = DirectXTex.TransformImage(rawImage.GetImage(0, 0, 0), transformFunction, finalImage);
        }

        result.ThrowIf();

        rawImage.Release();

        Blob blob = DirectXTex.CreateBlob();

        if (textureType == TextureType.TGA)
        {
            result = DirectXTex.SaveToTGAMemory2(finalImage.GetImage(0, 0, 0), blob, &metadata);
        }
        else
        {
            result = DirectXTex.SaveToWICMemory(finalImage.GetImages()[0], WICFlags.IgnoreSrgb, DirectXTex.GetWICCodec(GetWicDecoder(textureType)), blob, null, default);
        }

        var bytes = AsBytes(blob);

        blob.Release();
        finalImage.Release();
        result.ThrowIf();

        File.WriteAllBytes(path, bytes);
    }

    public static WICCodecs GetWicDecoder(TextureType file)
    {
        return file switch
        {
            TextureType.PNG => WICCodecs.CodecPng,
            TextureType.JPEG => WICCodecs.CodecJpeg,
            TextureType.BMP => WICCodecs.CodecBmp,
            TextureType.TIFF => WICCodecs.CodecTiff,
            _ => WICCodecs.CodecPng,
        };
    }

    unsafe public static void ApplyMaskRG00(Vector4* outPixels, Vector4* inPixels, ulong width, ulong y)
    {
        for (ulong j = 0; j < width; ++j)
        {
            outPixels[j] = new Vector4(inPixels[j].X, inPixels[j].Y, 0.0f, 0.0f);
        }
    }

    unsafe public static void ReverseChannels(Vector4* outPixels, Vector4* inPixels, ulong width, ulong y)
    {
        for (ulong j = 0; j < width; ++j)
        {
            Vector4 value = inPixels[j];

            outPixels[j].X = value.W;
            outPixels[j].Y = value.Z;
            outPixels[j].Z = value.Y;
            outPixels[j].W = value.X;
        }
    }

    unsafe public static void RestoreZ(Vector4* outPixels, Vector4* inPixels, ulong width, ulong y)
    {
        for (ulong j = 0; j < width; ++j)
        {
            Vector2 NormalXY = new(inPixels[j].X, inPixels[j].Y);

            NormalXY = NormalXY * 2.0f - Vector2.One;
            float NormalZ = (float)Math.Sqrt(Math.Clamp(1.0f - Vector2.Dot(NormalXY, NormalXY), 0, 1));

            outPixels[j] = new Vector4(inPixels[j].X, inPixels[j].Y, NormalZ, 1.0f);
        }
    }

    unsafe public static void DefaultCopy(Vector4* outPixels, Vector4* inPixels, ulong width, ulong y)
    {
        for (ulong j = 0; j < width; ++j)
        {
            outPixels[j] = inPixels[j];
        }
    }
}


public unsafe partial class DDS
{
    public ScratchImage Image { get; set; }
    public TexMetadata Metadata { get; set; }

    public DXGIFormat PreviewFormat { get; set; }
    public ulong PreviewWidth { get; set; }
    public ulong PreviewHeight { get; set; }
    private uint PreviewMip { get; set; } = 0;

    public DDS(string filePath, DDSFlags flags = DDSFlags.None)
    {
        Initialize(ByteFunctions.LoadTexture(filePath), flags);
    }

    public DDS(byte[] data, DDSFlags flags = DDSFlags.None)
    {
        Initialize(data, flags);
    }

    private void Initialize(byte[] ddsData, DDSFlags flags = DDSFlags.None)
    {
        Image = DirectXTex.CreateScratchImage();
        Span<byte> src = ddsData;
        Blob blob = DirectXTex.CreateBlob();
        TexMetadata meta = new TexMetadata();

        fixed (byte* srcPtr = src)
        {
            DirectXTex.LoadFromDDSMemory(srcPtr, (nuint)src.Length, flags, &meta, Image);
        }

        Console.WriteLine(DDS_DirectXTexNet.GetDDSDebugInfo(meta));
        Metadata = meta;

        blob.Release();

    }

    public void LoadDDS(string filePath, DDSFlags flags = DDSFlags.None)
    {
        DDS_DirectXTexNet.GetDDSImage(filePath, out ScratchImage Image, out TexMetadata Metadata, flags);
    }

    public string GetDDSDebugInfo()
    {
        return DDS_DirectXTexNet.GetDDSDebugInfo(Metadata);
    }

    public void ConvertToRGBA()
    {
        uint format = (uint)(DirectXTex.IsSRGB(Metadata.Format) ? DXGIFormat.R8G8B8A8_UNORM_SRGB : DXGIFormat.R8G8B8A8_UNORM);

        ScratchImage destImage = DirectXTex.CreateScratchImage();

        if (DirectXTex.IsCompressed(Metadata.Format))
        {
            DirectXTex.Decompress2(Image.GetImages(), Image.GetImageCount(), Image.GetMetadata(), format, destImage);
        }
        else
        {
            DirectXTex.Convert2(Image.GetImages(), Image.GetImageCount(), Image.GetMetadata(), format, TexFilterFlags.Default, 0.5f, destImage);
        }

        Image.Release();
        Image = destImage;
    }

    public byte[] GetSectionPixelData(uint mip, uint face)
    {
        if (Metadata.Format != (uint)DXGIFormat.R8G8B8A8_UNORM_SRGB
            || Metadata.Format != (uint)DXGIFormat.R8G8B8A8_UNORM)
        {
            ConvertToRGBA();
        }

        uint slice = 0;

        // Swap slice and face if it's a 3D texture, because they don't have faces.
        if (Metadata.Dimension == TexDimension.Texture3D)
        {
            slice = face;
            face = 0;
        }

        var image = DirectXTex.GetImage(Image, (ulong)mip, (ulong)face, (ulong)slice);

        PreviewFormat = (DXGIFormat)image.Format;
        PreviewWidth = image.Width;
        PreviewHeight = image.Height;
        PreviewMip = mip;

        byte[] pixels = new byte[image.SlicePitch];

        Marshal.Copy((nint)image.Pixels, pixels, 0, pixels.Length);

        return pixels;
    }

    public void GetData(uint mip, uint face, out ulong width, out ulong height, out ulong pitch, out ulong length, out byte[] pixels)
    {
        pixels = GetSectionPixelData(mip, face);
        width = PreviewWidth;
        height = PreviewHeight;
        pitch = DDS_DirectXTexNet.ComputePitch(PreviewFormat, PreviewWidth, PreviewHeight);
        length = width * height * (DirectXTex.BitsPerPixel((uint)PreviewFormat) / 8);
    }

    public void GetBounds(out uint mip, out uint face)
    {
        mip = (uint)Metadata.MipLevels - 1;

        if (Metadata.IsVolumemap())
        {
            uint mipDiff = (uint)(Metadata.MipLevels - PreviewMip);
            uint depth = (uint)Metadata.Depth;

            while (mipDiff > 0)
            {
                depth = Math.Max(1, depth / 2);
                mipDiff--;
            }

            face = depth - 1;
        }
        else
        {
            face = (uint)Metadata.ArraySize - 1;
        }
    }

    ~DDS()
    {
        Image.Release();
    }
}