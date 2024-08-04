using System.Collections.Generic;
using System.IO;
using TelltaleTextureTool.TelltaleEnums;

namespace TelltaleTextureTool.Telltale.TTArch
{

    struct TelltaleFile
    {
        string name;
        ulong offset; // unused at the moment
        ulong size;
        ulong name_crc;
    }

    struct FileHeader
    {
        public ulong crcName; // CRC64 EMCA 182 format
        public ulong offset;  // How many bytes after the name table the file is located
        public uint size;
        public uint unkownBytes; // TODO: Figure what these are or just ask Lucas
        public ushort nameTableChunkIndex;
        public ushort nameTableOffset;

        public void ReadFromBinary(BinaryReader reader, bool printDebug = false)
        {
            crcName = reader.ReadUInt64();
            offset = reader.ReadUInt64();
            size = reader.ReadUInt32();
            unkownBytes = reader.ReadUInt32();
            nameTableChunkIndex = reader.ReadUInt16();
            nameTableOffset = reader.ReadUInt16();
        }

        public void WriteToBinary(BinaryWriter writer, bool printDebug = false)
        {
            writer.Write(crcName);
            writer.Write(offset);
            writer.Write(size);
            writer.Write(unkownBytes);
            writer.Write(nameTableChunkIndex);
            writer.Write(nameTableOffset);
        }
    };

    struct ArchiveHeader
    {
        public uint version;
        public uint nameSize; // The size of the nameTable
        public uint fileCount;

        public void WriteToBinary(BinaryWriter writer, bool printDebug = false)
        {
            writer.Write(version);
            writer.Write(nameSize);
            writer.Write(fileCount);
        }

        public void ReadFromBinary(BinaryReader reader, bool printDebug = false)
        {
            version = reader.ReadUInt32();
            nameSize = reader.ReadUInt32();
            fileCount = reader.ReadUInt32();
        }
    };

     public enum Compression
    {

        None,
        Zlib,
        Oodle
    }

    public class TTArch
    {
        ArchiveHeader header;
        List<FileHeader> entries;
       

        public FileStream ttarchStream { get; set; }

        public TTARCHVer vers;
        //        public Game version;
        public Compression ttarchCompression;
        public uint fileCount;
        public int blowFishVersion;
        public string key;
        public T3PlatformType platform;
        public ulong fileDataStartIndex;
        public ulong nameTableStartIndex;

        public List<string> fileNames;

        public TTArch(string filePath){

            
        }
    }
       

    struct CompressedHeader
    {
        uint version;
        uint chunkDecompressedSize;
        uint chunkCount;
    };

    struct CompressedArchive
    {
        CompressedHeader header;
        List<ulong> chunkOffset;
        MemoryStream chunkData;
    };

   
}
