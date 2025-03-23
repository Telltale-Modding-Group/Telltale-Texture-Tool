using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TelltaleTextureTool.Utilities;

public static class ByteFunctions
{
    private static readonly int MAX_STRING_BUFFER_SIZE = 256;

    /// <summary>
    /// Get the number of all items in a list of byte arrays
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static uint GetByteArrayListElementsCount(List<byte[]> array) =>
        (uint)(array?.Sum(arrayElem => arrayElem?.Length ?? 0) ?? 0);

    /// <summary>
    /// Reads a string from the current stream. The string is prefixed with the length, encoded as an integer 32 bits at a time.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static string ReadString(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        if (length < 0)
            throw new InvalidDataException($"Negative string length: {length}");

        if (length > reader.BaseStream.Length - reader.BaseStream.Position)
            throw new EndOfStreamException(
                $"Requested {length} characters but only {reader.BaseStream.Length - reader.BaseStream.Position} remain"
            );

        if (length > MAX_STRING_BUFFER_SIZE)
        {
            throw new InvalidDataException($"String length {length} is too long.");
        }

        return ReadFixedString(reader, length);
    }

    public static string ReadFixedString(BinaryReader reader, int length)
    {
        if (length > reader.BaseStream.Length - reader.BaseStream.Position)
            throw new EndOfStreamException(
                $"Requested {length} characters but only {reader.BaseStream.Length - reader.BaseStream.Position} remain"
            );

        char[] buffer = reader.ReadChars(length);

        if (buffer.Length < length)
            throw new EndOfStreamException(
                $"Requested {length} characters but only got {buffer.Length}"
            );

        return new string(buffer);
    }

    public static bool ReadTelltaleBoolean(BinaryReader reader) =>
        reader.ReadChar() switch
        {
            '1' => true,
            '0' => false,
            _ => throw new Exception("Invalid Telltale Boolean data."),
        };

    /// <summary>
    /// Writes a length-prefixed string (32 bit integer).
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    public static void WriteString(BinaryWriter writer, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.Write(value.Length);

        WriteFixedString(writer, value);
    }

    /// <summary>
    /// Writes a string (length specified by the string value itself).
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    public static void WriteFixedString(BinaryWriter writer, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.Write(value.ToCharArray());
    }

    /// <summary>
    /// Writes a boolean.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    public static void WriteBoolean(BinaryWriter writer, bool value) =>
        writer.Write(value ? '1' : '0');

    /// <summary>
    /// Combines two byte arrays into one.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static byte[] Combine(byte[] first, byte[] second)
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        // Handle edge cases with empty arrays
        if (first.Length == 0)
            return (byte[])second.Clone();
        if (second.Length == 0)
            return (byte[])first.Clone();
        // Check for potential overflow
        checked
        {
            try
            {
                byte[] result = new byte[first.Length + second.Length];
                Buffer.BlockCopy(first, 0, result, 0, first.Length);
                Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
                return result;
            }
            catch (OverflowException)
            {
                throw new InvalidOperationException(
                    $"Combined array size exceeds maximum allowed length ({int.MaxValue} bytes)"
                );
            }
        }
    }
}
