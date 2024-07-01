using System;
using System.IO;
using D3DTX_Converter.TelltaleEnums;
using System.Runtime.InteropServices;

namespace D3DTX_Converter.TelltaleTypes;

public struct T3SamplerStateBlock
{
  public uint mData;

  public T3SamplerStateBlock(BinaryReader reader)
  {
    mData = reader.ReadUInt32(); //mSamplerState [4 bytes]
  }

  public readonly void WriteBinaryData(BinaryWriter writer)
  {
    writer.Write(mData); //mData [4 bytes] 
  }

  public readonly uint GetByteSize()
  {
    uint totalByteSize = 0;

    totalByteSize += (uint)Marshal.SizeOf(mData); //mData [4 bytes]

    return totalByteSize;
  }

  public override readonly string ToString()
  {
    string enumFlags = "";

    var allEnums = Enum.GetValues(typeof(T3SamplerStateValue));

    foreach (var enumMask in allEnums)
    {
      if ((mData & (uint)(T3SamplerStateValue)enumMask) != 0)
        enumFlags += Enum.GetName((T3SamplerStateValue)enumMask) + ": " + (mData & (uint)(T3SamplerStateValue)enumMask) + " | ";
    }

    enumFlags += "(" + (int)mData + ")";
    return string.Format("[T3SamplerStateBlock] mData: {0}", enumFlags);
  }
}

public struct T3SamplerStateBlock_SamplerStateEntry
{
  public int mShift;
  public int mMask;

  public override string ToString() => string.Format("[T3SamplerStateBlock::SamplerStateEntry] mShift: {0} mMask: {1}", mShift, mMask);
}

