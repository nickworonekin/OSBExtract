using System;
using System.IO;

public static class PCM
{
    static byte[] WavHeader =
    {
        0x52, 0x49, 0x46, 0x46, // RIFF
        0x00, 0x00, 0x00, 0x00, // Length of wav - 8 (blank for now)
        0x57, 0x41, 0x56, 0x45, 0x66, 0x6D, 0x74, 0x20, // WAVEfmt(space)
        0x10, 0x00, 0x00, 0x00, // Size of WAVEfmt chunk
        0x01, 0x00, // Compression format (01 = PCM)
        0x01, 0x00, // Channel count (always 1 for this)
        0x44, 0xAC, 0x00, 0x00, // Sample rate (always 44100)
        0x88, 0x58, 0x01, 0x00, // Bytes per second
        0x02, 0x00, // Block align
        0x10, 0x00, // Significant bits per sample
        0x64, 0x61, 0x74, 0x61, // data
        0x00, 0x00, 0x00, 0x00, // Length of the data chunk (blank for now)
    };

    public static void ToWav(byte[] buffer, int offset, int length, string outFile)
    {
        using (FileStream outStream = File.OpenWrite(outFile))
        {
            // Set some things in the WAV header before we write it
            // Set some things in the RIFF header before we write it
            BitConverter.GetBytes(length + WavHeader.Length - 8).CopyTo(WavHeader, 0x4);
            BitConverter.GetBytes(length).CopyTo(WavHeader, 0x28);

            // Now write the WAV header
            outStream.Write(WavHeader, 0, WavHeader.Length);

            // Write out the data
            outStream.Write(buffer, offset, length);
        }
    }
}