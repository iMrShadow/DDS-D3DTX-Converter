﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TelltaleTextureTool.Utilities;

public static class ByteFunctions
{
    /// <summary>
    /// Get the number of all items in a list of byte arrays
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static uint GetByteArrayListElementsCount(List<byte[]> array)
    {
        uint result = 0;

        foreach (var t in array)
        {
            result += (uint)t.Length;
        }

        return result;
    }

    /// <summary>
    /// Reads a string from the current stream. The string is prefixed with the length, encoded as an integer 32 bits at a time.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static string ReadString(BinaryReader reader)
    {
        int stringLength = reader.ReadInt32();

        string value = "";

        for (int i = 0; i < stringLength; i++)
        {
            value += reader.ReadChar();
        }

        return value;
    }

    public static string ReadFixedString(BinaryReader reader, int length)
    {
        string value = "";

        for (int i = 0; i < length; i++)
        {
            value += reader.ReadChar();
        }

        return value;
    }

    public static bool ReadTelltaleBoolean(BinaryReader reader)
    {
        char parsedChar = reader.ReadChar();

        return parsedChar switch
        {
            '1' => true,
            '0' => false,
            _ => throw new Exception("Invalid Telltale Boolean data."),
        };
    }

    /// <summary>
    /// Writes a length-prefixed string (32 bit integer).
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    public static void WriteString(BinaryWriter writer, string value)
    {
        writer.Write(value.Length);

        foreach (var t in value)
        {
            writer.Write(t);
        }
    }

    /// <summary>
    /// Writes a string (length specified by the string value itself).
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    public static void WriteFixedString(BinaryWriter writer, string value)
    {
        foreach (var t in value)
        {
            writer.Write(t);
        }
    }

    /// <summary>
    /// Writes a boolean.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    public static void WriteBoolean(BinaryWriter writer, bool value) => writer.Write(value ? '1' : '0');

    /// <summary>
    /// Combines two byte arrays into one.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static byte[] Combine(byte[] first, byte[] second)
    {
        //allocate a byte array with both total lengths combined to accommodate both
        byte[] bytes = new byte[first.Length + second.Length];

        //copy the data from the first array into the new array
        Buffer.BlockCopy(first, 0, bytes, 0, first.Length);

        //copy the data from the second array into the new array (offset by the total length of the first array)
        Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);

        //return the final byte array
        return bytes;
    }
}
