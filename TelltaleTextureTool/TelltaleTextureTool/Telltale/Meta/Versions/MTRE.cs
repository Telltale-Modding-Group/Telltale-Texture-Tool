using System;
using System.IO;
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
/// Meta Type REference (MTRE, or ERTM), a meta header often used in telltale files
/// </summary>
public class MTRE : IMetaHeader
{
    /// <summary>
    /// [4 bytes] The version of the meta stream version.
    /// </summary>
    public string mMetaStreamVersion { get; set; } = "ERTM";

    /// <summary>
    /// [4 bytes] Amount of class name elements (CRC64 Class Names) used in the file.
    /// </summary>
    public uint mClassNamesLength { get; set; }

    /// <summary>
    /// [12 bytes for each element] An array of class names (CRC64 Class Names) that are used in the file.
    /// </summary>
    public ClassNames[] mClassNames { get; set; } = [];

    /// <summary>
    /// [4 + string length + 4 bytes for each element] An array of class names (Unhashed Class Names) that are used in the file. Used in older games.
    /// </summary>
    public UnhashedClassNames[] mUnhashedClassNames { get; set; } = [];

    /// <summary>
    /// Meta Header (empty constructor, only used for json deserialization)
    /// </summary>
    public MTRE() { }

    public void WriteToBinary(BinaryWriter writer, TelltaleToolGame game = TelltaleToolGame.DEFAULT, T3PlatformType platform = T3PlatformType.ePlatform_None, bool printDebug = false)
    {
        ByteFunctions.WriteFixedString(writer, mMetaStreamVersion); // Meta Stream Keyword [4 bytes]
        writer.Write(mClassNamesLength); // mClassNamesLength [4 bytes]

        //--------------------------mClassNames--------------------------
        for (int i = 0; i < mClassNames.Length; i++)
        {
            mClassNames[i].WriteBinaryData(writer);
        }

        for (int i = 0; i < mUnhashedClassNames.Length; i++)
        {
            mUnhashedClassNames[i].WriteBinaryData(writer);
        }
    }

    public void ReadFromBinary(BinaryReader reader, TelltaleToolGame game = TelltaleToolGame.DEFAULT, T3PlatformType platform = T3PlatformType.ePlatform_None, bool printDebug = false)
    {
        mMetaStreamVersion = ByteFunctions.ReadFixedString(reader, 4); // Meta Stream Keyword [4 bytes]
        mClassNamesLength = reader.ReadUInt32(); // mClassNamesLength [4 bytes]

        uint checkValue = reader.ReadUInt32();

        reader.BaseStream.Position -= 4;

        // Interesting way to check if a string is hashed. Usually hashes are big numbers, while lengths are less than a couple of dozens.
        if (checkValue < 0 || checkValue > 128)
        {
            mClassNames = new ClassNames[mClassNamesLength];

            for (int i = 0; i < mClassNames.Length; i++)
            {
                mClassNames[i] = new ClassNames(reader);
            }
        }
        else
        {
            reader.BaseStream.Position -= 4;

            mUnhashedClassNames = new UnhashedClassNames[mClassNamesLength];

            for (int i = 0; i < mUnhashedClassNames.Length; i++)
            {
                mUnhashedClassNames[i] = new UnhashedClassNames(reader);
            }
        }

        if (printDebug)
            PrintConsole();
    }

    public void SetMetaSectionChunkSizes(uint defaultSectionChunkSize, uint debugSectionChunkSize, uint asyncSectionChunkSize)
    {
        return;
    }

    public string GetDebugInfo(TelltaleToolGame game = TelltaleToolGame.DEFAULT, T3PlatformType platform = T3PlatformType.ePlatform_None)
    {
        string metaInfo = "||||||||||| Meta Header |||||||||||" + Environment.NewLine;

        metaInfo += "Meta Stream Keyword = " + mMetaStreamVersion + Environment.NewLine;
        metaInfo += "Meta mClassNamesLength = " + mClassNamesLength + Environment.NewLine;

        for (int i = 0; i < mClassNames.Length; i++)
        {
            metaInfo += "mTypeNameCRC " + i + " = " + mClassNames[i].mTypeNameCRC + Environment.NewLine;
            metaInfo += "mVersionCRC " + i + " = " + mClassNames[i].mVersionCRC + Environment.NewLine;
        }

        for (int i = 0; i < mUnhashedClassNames.Length; i++)
        {
            metaInfo += "mClassName " + i + " = " + mUnhashedClassNames[i].className + Environment.NewLine;
            metaInfo += "mVersionCRC " + i + " = " + mUnhashedClassNames[i].mVersionCRC + Environment.NewLine;
        }

        return metaInfo;
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
