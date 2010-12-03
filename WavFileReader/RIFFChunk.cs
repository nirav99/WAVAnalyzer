using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WavFileReader
{
    /// <summary>
    /// Class to read and validate the RIFF chunk of WAVE file
    /// </summary>
    public class RIFFChunk
    {
        private string chunkID;                 // MUST be "RIFF"
        private uint size;                      // Size of entire WAV file - 8 bytes
        private string format;                  // MUST be "WAVE"
        private const int riffBlockSize = 12;   // RIFF block is 12 bytes

        public RIFFChunk(BinaryReader br)
        {
            byte[] buffer = br.ReadBytes(riffBlockSize);
            byte[] temp;

            chunkID = Utilities.getString(buffer, 4, 0);    // First 4 bytes must contain "RIFF"
            temp = Utilities.getBytes(buffer, 4, 4);        // Size of entire file - 8 bytes 
            size = Utilities.getIntegerValue(temp);
            format = Utilities.getString(buffer, 4, 8);     // Must be "WAVE"
        }

        public byte[] getRIFFBlock(uint chunkSize)
        {
            byte[] riffHeader = new byte[12];
            int rHIDx = 0;

            chunkID = "RIFF";
            size = chunkSize;
            format = "WAVE";

            byte[] temp = Utilities.getByteValue(chunkID);

            for (int i = 0; i < 4; i++, rHIDx++)
            {
                riffHeader[i] = temp[i];
            }

            temp = Utilities.getByteValue(size);
            for (int i = 4; i < 8; i++, rHIDx++)
            {
                riffHeader[i] = temp[i];
            }
            temp = Utilities.getByteValue(format);

            for (int i = 8; i < 12; i++, rHIDx++)
            {
                riffHeader[i] = temp[i];
            }

            return riffHeader;
        }

        public uint getChunkSize()
        {
            return size;
        }

        public int RIFFChunkSize()
        {
            return riffBlockSize;
        }

        /// <summary>
        /// If the specified WAVE file is indeed a WAV file
        /// </summary>
        /// <returns></returns>
        public bool validate()
        {
            if (size > 0 && chunkID.Equals("RIFF", StringComparison.CurrentCultureIgnoreCase) && format.Equals("WAVE", StringComparison.CurrentCultureIgnoreCase))
                return true;
            else
                return false;
        }
    }
}
