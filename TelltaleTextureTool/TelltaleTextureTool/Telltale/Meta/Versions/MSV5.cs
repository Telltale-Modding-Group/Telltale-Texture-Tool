using System;
using System.IO;
using System.Text;
using TelltaleTextureTool.TelltaleEnums;
using TelltaleTextureTool.TelltaleTypes;
using TelltaleTextureTool.Utilities;

/*
 * This is a meta stream header.
 * These objects are often at the top of every telltale file.
 * They also contain info regarding the byte size of certain data chunks, along with the classes that are used (which are CRC64'd sadly).
 * Also, Telltale uses Hungarian Notation for variable naming.
*/

namespace TelltaleTextureTool.Telltale.Meta;

/// <summary>
/// Meta Stream Version 5 (MSV5 or 5VSM), a meta header often used in telltale files
/// </summary>
public class MSV5 : IMetaHeader
{
    /// <summary>
    /// [4 bytes] The version of the meta stream version.
    /// </summary>
    public string mMetaStreamVersion { get; set; } = "5VSM";

    /// <summary>
    /// [4 bytes] The size of the data 'header' after the meta header.
    /// </summary>
    public uint mDefaultSectionChunkSize { get; set; }

    /// <summary>
    /// [4 bytes] The size of the debug data chunk after the meta header, which is always 0.
    /// </summary>
    public uint mDebugSectionChunkSize { get; set; }

    /// <summary>
    /// [4 bytes] The size of the asynchronous data chunk (not the meta header, or the data chunk header, but the data itself).
    /// </summary>
    public uint mAsyncSectionChunkSize { get; set; }

    /// <summary>
    /// [4 bytes] Amount of class name elements (CRC64 Class Names) used in the file.
    /// </summary>
    public uint mClassNamesLength { get; set; }

    /// <summary>
    /// [12 bytes for each element] An array of class names (CRC64 Class Names) that are used in the file.
    /// </summary>
    public ClassNames[] mClassNames { get; set; } = [];

    /// <summary>
    /// Meta Stream Header version 5 (empty constructor, only used for json deserialization)
    /// </summary>
    public MSV5() { }

    public void WriteToBinary(BinaryWriter writer, TelltaleToolGame game = TelltaleToolGame.DEFAULT, T3PlatformType platform = T3PlatformType.ePlatform_None, bool printDebug = false)
    {
        ByteFunctions.WriteFixedString(writer, mMetaStreamVersion); // Meta Stream Keyword [4 bytes]
        writer.Write(mDefaultSectionChunkSize); // Default Section Chunk Size [4 bytes] default section chunk size
        writer.Write(mDebugSectionChunkSize); // Debug Section Chunk Size [4 bytes] debug section chunk size (always zero)
        writer.Write(mAsyncSectionChunkSize); // Async Section Chunk Size [4 bytes] async section chunk size (size of the bytes after the file header)
        writer.Write(mClassNamesLength); // mClassNamesLength [4 bytes]

        //--------------------------mClassNames--------------------------
        for (int i = 0; i < mClassNames.Length; i++)
        {
            mClassNames[i].WriteBinaryData(writer);
        }
    }

    public void ReadFromBinary(BinaryReader reader, TelltaleToolGame game = TelltaleToolGame.DEFAULT, T3PlatformType platform = T3PlatformType.ePlatform_None, bool printDebug = false)
    {
        mMetaStreamVersion = ByteFunctions.ReadFixedString(reader, 4); // Meta Stream Keyword [4 bytes]
        mDefaultSectionChunkSize = reader.ReadUInt32(); // Default Section Chunk Size [4 bytes] //default section chunk size
        mDebugSectionChunkSize = reader.ReadUInt32(); // Debug Section Chunk Size [4 bytes] //debug section chunk size (always zero)
        mAsyncSectionChunkSize = reader.ReadUInt32(); // Async Section Chunk Size [4 bytes] //async section chunk size (size of the bytes after the file header)
        mClassNamesLength = reader.ReadUInt32(); // mClassNamesLength [4 bytes]

        //--------------------------mClassNames--------------------------
        mClassNames = new ClassNames[mClassNamesLength];

        for (int i = 0; i < mClassNames.Length; i++)
        {
            mClassNames[i] = new ClassNames(reader);
        }

        if (printDebug)
            PrintConsole();
    }

    public void SetMetaSectionChunkSizes(uint defaultSectionChunkSize, uint debugSectionChunkSize, uint asyncSectionChunkSize)
    {
        mDefaultSectionChunkSize = defaultSectionChunkSize;
        mDebugSectionChunkSize = debugSectionChunkSize;
        mAsyncSectionChunkSize = asyncSectionChunkSize;
    }

    public string GetDebugInfo(TelltaleToolGame game = TelltaleToolGame.DEFAULT, T3PlatformType platform = T3PlatformType.ePlatform_None)
    {
        StringBuilder metaInfo = new();

        metaInfo.AppendLine("||||||||||| Meta Header |||||||||||");

        metaInfo.AppendFormat("Meta Stream Keyword: {0}", mMetaStreamVersion).Append(Environment.NewLine);
        metaInfo.AppendFormat("Meta Default Section Chunk Size: {0}", mDefaultSectionChunkSize).Append(Environment.NewLine);
        metaInfo.AppendFormat("Meta Debug Section Chunk Size: {0}", mDebugSectionChunkSize).Append(Environment.NewLine);
        metaInfo.AppendFormat("Meta Async Section Chunk Size: {0}", mAsyncSectionChunkSize).Append(Environment.NewLine);
        metaInfo.AppendFormat("Meta Class Names Length: {0}", mClassNamesLength).Append(Environment.NewLine);

        for (int i = 0; i < mClassNames.Length; i++)
        {
            metaInfo.AppendFormat("Meta Class Name CRC: {0}", mClassNames[i].mTypeNameCRC.mCrc64).Append(Environment.NewLine); ;
            metaInfo.AppendFormat("Meta Class Version CRC: {0}", mClassNames[i].mVersionCRC).Append(Environment.NewLine); ;
        }

        return metaInfo.ToString();
    }

    public uint GetHeaderByteSize()
    {
        return 0;
    }

    public void PrintConsole()
    {
        Console.WriteLine(GetDebugInfo());
    }
}
