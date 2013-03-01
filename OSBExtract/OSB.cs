using System;
using System.IO;

public static class OSB
{
    public static void Extract(string inFile, string outDir)
    {
        byte[] buffer = File.ReadAllBytes(inFile);

        Extract(buffer, inFile, outDir, 0, 0);
    }

    public static int Extract(byte[] buffer, string inFile, string outDir, int startOffset = 0, int index = 0)
    {
        // Make sure this is an OSB
        if (!(buffer[startOffset + 0x0] == 'S' && buffer[startOffset + 0x1] == 'O' && buffer[startOffset + 0x2] == 'S' && buffer[startOffset + 0x3] == 'B'))
        {
            return 0;
        }

        // Read the number of files in the OSB
        int numFiles = BitConverter.ToInt32(buffer, startOffset + 0xC);

        // Read the offset of the first SOSP chunk
        // Each SOSP chunk is an entry within the file table.
        int sospOffset = BitConverter.ToInt32(buffer, startOffset + 0x10);

        // Read each SOSP chunk
        for (int i = 0; i < numFiles; i++)
        {
            // File offset
            // Each file offset is 24 bits and is laid out as
            // 02 XX 00 01
            int offset = buffer[startOffset + sospOffset + 4] << 16 | BitConverter.ToUInt16(buffer, startOffset + sospOffset + 6);

            // ADPCM flag
            // This is what that XX in the file offset is used for
            // 00 = PCM
            // 01 = ADPCM
            bool isADPCM = (buffer[startOffset + sospOffset + 5] == 1);

            // File length
            // The number of samples in each sound is laid out as
            // 02 03 00 01
            // So, to get the file length:
            // If PCM: samples * 2
            // If ADPCM: samples / 2
            int length = BitConverter.ToUInt16(buffer, startOffset + sospOffset + 8) << 16 | BitConverter.ToUInt16(buffer, startOffset + sospOffset + 10);
            if (isADPCM)
                length >>= 1; // We can cheat and just do a bitwise operation here
            else
                length <<= 1; // Same for this

            // Create the output directory if it does not exist
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            string outFile = outDir + Path.DirectorySeparatorChar;
            
            // For any character OSBs, we are going to keep the name before "RENSA" so they match with
            // voice files for them not in the OSB
            int indexOfRensa = Path.GetFileNameWithoutExtension(inFile).IndexOf("RENSA");
            int indexOfRensa2 = Path.GetFileNameWithoutExtension(inFile).IndexOf("RENSA2");
            if (indexOfRensa2 > 0) // This is an other character OSB
                outFile += Path.GetFileNameWithoutExtension(inFile).Substring(0, indexOfRensa) + "2" + (index + 1).ToString("D2") + ".wav";
            else if (indexOfRensa > 0) // This is a character OSB
                outFile += Path.GetFileNameWithoutExtension(inFile).Substring(0, indexOfRensa) + (index + 1).ToString("D2") + ".wav";
            else
                outFile += index.ToString("D2") + ".wav"; // This one is for the MLT (& nothing will be extracting > 100 files anyway)

            if (isADPCM) // If this is an ADPCM encoded sound (aka AICA aka Yamaha 4-bit ADPCM), we need to convert it to 16-bit PCM
            {
                byte[] newBuffer = ADPCM.ToRaw(buffer, startOffset + offset, length);
                PCM.ToWav(newBuffer, 0, newBuffer.Length, outFile);
            }
            else // Just standard 16-bit PCM
            {
                PCM.ToWav(buffer, startOffset + offset, length, outFile);
            }

            sospOffset += 56;
            index++;
        }

        return numFiles;
    }
}