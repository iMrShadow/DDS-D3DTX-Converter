﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TelltaleTextureTool.Utilities;

public static class ByteFunctions
{
    /// <summary>
    /// 
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

        switch (parsedChar)
        {
            case '1':
                return true;
            case '0':
                return false;
            default:
                throw new Exception("Invalid Telltale Boolean data.");
        }
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

    public static byte[] GetBytes(string stringValue)
    {
        //create byte array of the length of the string
        byte[] stringBytes = new byte[stringValue.Length];

        //for the length of the string, get each byte value
        for (int i = 0; i < stringBytes.Length; i++)
        {
            stringBytes[i] = Convert.ToByte(stringValue[i]);
        }

        //return it
        return stringBytes;
    }

    public static uint ConvertStringToUInt32(string sValue) => BitConverter.ToUInt32(GetBytes(sValue), 0);

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

    /// <summary>
    /// Checks if the pointer position is at the DCArray capacity, if's not then it moves the pointer past where it should be after reading the DCArray.
    /// </summary>
    /// <param name="pointerPositionBeforeCapacity"></param>
    /// <param name="arrayCapacity"></param>
    /// <param name="bytePointerPosition"></param>
    public static void DCArrayCheckAdjustment(uint pointerPositionBeforeCapacity, uint arrayCapacity, ref uint bytePointerPosition)
    {
        uint estimatedOffPoint = pointerPositionBeforeCapacity + arrayCapacity;
        Console.WriteLine("(DCArray Check) Estimated to be at = {0}", estimatedOffPoint);

        if (bytePointerPosition != estimatedOffPoint)
        {
            Console.WriteLine("(DCArray Check) Left off at = {0}", bytePointerPosition);
            Console.WriteLine("(DCArray Check) Skipping by using the estimated position...", bytePointerPosition);
            bytePointerPosition = estimatedOffPoint;
        }
        else
        {
            Console.WriteLine("(DCArray Check) Left off at = {0}", bytePointerPosition);
        }

    }

    /// <summary>
    /// Checks if we have reached the end of the file.
    /// </summary>
    /// <param name="bytePointerPosition"></param>
    /// <param name="fileSize"></param>
    public static void ReachedEndOfFile(uint bytePointerPosition, uint fileSize)
    {
        if (bytePointerPosition != fileSize)
        {
            Console.WriteLine("(End Of File Check) Didn't reach the end of the file!");
            Console.WriteLine("(End Of File Check) Left off at = {0}", bytePointerPosition);
            Console.WriteLine("(End Of File Check) File Size = {0}", fileSize);
        }
        else
        {
            Console.WriteLine("(End Of File Check) Reached end of file!");
            Console.WriteLine("(End Of File Check) Left off at = {0}", bytePointerPosition);
            Console.WriteLine("(End Of File Check) File Size = {0}", fileSize);
        }
    }

    public static byte[] LoadTexture(string path) => File.ReadAllBytes(path);

    /// <summary>
    /// Checks if we have reached a specific offset in the file.
    /// </summary>
    /// <param name="bytePointerPosition"></param>
    /// <param name="offsetPoint"></param>
    public static void ReachedOffset(uint bytePointerPosition, uint offsetPoint)
    {
        if (bytePointerPosition != offsetPoint)
        {
            Console.WriteLine("(Offset Check) Didn't reach the offset!");
            Console.WriteLine("(Offset Check) Left off at = {0}", bytePointerPosition);
            Console.WriteLine("(Offset Check) Offset = {0}", offsetPoint);
        }
        else
        {
            Console.WriteLine("(Offset Check) Reached the offset!");
            Console.WriteLine("(Offset Check) Left off at = {0}", bytePointerPosition);
            Console.WriteLine("(Offset Check) Offset = {0}", offsetPoint);
        }

    }

    public static byte[] GetBytesAfterBytePattern(string searchString, byte[] fileBytes)
    {
        byte[] searchBytes = Encoding.ASCII.GetBytes(searchString);

        int position = SearchBytePattern(searchBytes, fileBytes);

        if (position != -1)
        {
            byte[] resultBytes = new byte[fileBytes.Length - position];
            Array.Copy(fileBytes, position, resultBytes, 0, resultBytes.Length);
            return resultBytes;
        }

        return [];
    }

    public static int SearchBytePattern(byte[] pattern, byte[] bytes)
    {
        int patternLen = pattern.Length;
        int totalLen = bytes.Length;
        byte firstMatchByte = pattern[0];

        for (int i = 0; i < totalLen; i++)
        {
            if (firstMatchByte == bytes[i] && totalLen - i >= patternLen)
            {
                byte[] match = new byte[patternLen];
                Array.Copy(bytes, i, match, 0, patternLen);
                if (AreArraysEqual(match, pattern))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public static bool AreArraysEqual(byte[] a1, byte[] a2)
    {
        if (a1.Length != a2.Length)
            return false;

        for (int i = 0; i < a1.Length; i++)
        {
            if (a1[i] != a2[i])
                return false;
        }

        return true;
    }
}