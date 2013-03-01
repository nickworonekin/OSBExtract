using System;
using System.IO;

public static class MLT
{
    public static void Extract(string inFile, string outDir)
    {
        byte[] buffer = File.ReadAllBytes(inFile);

        // Make sure this is a MLT
        if (!(buffer[0x0] == 'S' && buffer[0x1] == 'M' && buffer[0x2] == 'L' && buffer[0x3] == 'T'))
        {
            return;
        }

        // Offset of the first SOSB chunk in the header
        int sosbHeaderChunkOffset = 0x20;

        // Index for extraction
        int index = 0;

        // There are only 3 SOSB header chunks in the header, so we'll just hardcode that
        for (int i = 0; i < 3; i++)
        {
            // Get the offset of the OSB within the MLT
            int osbOffset = BitConverter.ToInt32(buffer, sosbHeaderChunkOffset + 0x10);

            // Now we can just pass it to the OSB extractor
            index += OSB.Extract(buffer, inFile, outDir, osbOffset, index);

            sosbHeaderChunkOffset += 32;
        }
    }
}