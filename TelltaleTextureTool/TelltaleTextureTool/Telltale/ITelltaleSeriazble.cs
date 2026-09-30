using System.IO;
using TelltaleTextureTool.TelltaleEnums;

namespace TelltaleTextureTool.FileTypes;

/// <summary>
/// Interface for serializing a Telltale object.
/// </summary>
public interface ITelltaleSerializable
{
    /// <summary>
    /// Writes the Telltale object to a binary stream.
    /// </summary>
    /// <param name="writer">The BinaryWriter to write to.</param>
    /// <param name="printDebug">Indicates whether to print debug information.</param>
    void WriteToBinary(BinaryWriter writer, TelltaleToolGame game = TelltaleToolGame.NONE, T3PlatformType platform = T3PlatformType.ePlatform_None, bool printDebug = false);

    /// <summary>
    /// Reads the Telltale object from a binary stream.
    /// </summary>
    /// <param name="reader">The BinaryReader to read from.</param>
    /// <param name="printDebug">Indicates whether to print debug information.</param>
    void ReadFromBinary(BinaryReader reader, TelltaleToolGame game = TelltaleToolGame.NONE, T3PlatformType platform = T3PlatformType.ePlatform_None, bool printDebug = false);
}