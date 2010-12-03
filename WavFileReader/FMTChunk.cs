using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WavFileReader
{

    /// <summary>
    /// Class representing FMT part of the WAVE header
    /// </summary>
    public class FMTChunk
    {
        private string subChunkID;      // Must be "FMT "
        private uint subChunkSize;
        private uint audioFormat;
        private uint numChannels;
        private uint sampleRate;
        private uint byteRate;
        private uint blockAlignment;
        private uint bitsPerSample;
        private byte[] fmtBytes;     // Buffer containing FMT data 
        private const int fmtBlockSize = 24;   // FMT block has 24 bytes

        public FMTChunk(BinaryReader br)
        {
            byte[] buffer = br.ReadBytes(fmtBlockSize);
            byte[] temp;
            fmtBytes = buffer;

            subChunkID = Utilities.getString(buffer, 4, 0);

            temp = Utilities.getBytes(buffer, 4, 4);
            subChunkSize = Utilities.getIntegerValue(temp);

            temp = Utilities.getBytes(buffer, 2, 8);
            audioFormat = Utilities.getIntegerValue(temp);

            temp = Utilities.getBytes(buffer, 2, 10);
            numChannels = Utilities.getIntegerValue(temp);

            temp = Utilities.getBytes(buffer, 4, 12);
            sampleRate = Utilities.getIntegerValue(temp);

            temp = Utilities.getBytes(buffer, 4, 16);
            byteRate = Utilities.getIntegerValue(temp);

            temp = Utilities.getBytes(buffer, 2, 20);
            blockAlignment = Utilities.getIntegerValue(temp);

            temp = Utilities.getBytes(buffer, 2, 22);
            bitsPerSample = Utilities.getIntegerValue(temp);

            // Read excess bytes and discard them
            if (subChunkSize > 16)
                br.ReadBytes((int)subChunkSize - 16);
        }

        public byte[] getFMTBlock()
        {
            return fmtBytes;
        }

        public int FMTChunkSize
        {
            get
            {
                return fmtBlockSize;
            }
        }

        /// <summary>
        /// If the specified WAVE file is indeed an 8KHz 16Bit Mono PCM file
        /// </summary>
        /// <returns></returns>
        public bool validate()
        {
            if (subChunkID.Equals("FMT ", StringComparison.CurrentCultureIgnoreCase) && (audioFormat == 1) && (numChannels == 1) && (sampleRate == 8000))
                return true;
            else
                return false;
        }

        public uint SampleRate
        {
            get
            {
                return sampleRate;
            }
        }

        public uint BitsPerSample
        {
            get
            {
                return bitsPerSample;
            }
        }

        public void printInfo()
        {
            Console.WriteLine("FMT Header");
            Console.WriteLine("Chunk ID = " + subChunkID);
            Console.WriteLine("Chunk Size = " + subChunkSize);
            Console.WriteLine("Audio Format = " + audioFormat);
            Console.WriteLine("Num Channels = " + numChannels);
            Console.WriteLine("Sample Rate = " + sampleRate);
            Console.WriteLine("Byte Rate = " + byteRate);
            Console.WriteLine("Block Alignment = " + blockAlignment);
            Console.WriteLine("Bits per Sample = " + bitsPerSample);
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            if (audioFormat == 1)
            {
                result.Append("Audio Format: PCM WAVE File."); // with {0} channels. Sample Rate: {1} and Bits/Sample: {2}", numChannels, sampleRate, bitsPerSample);
            }
            else
            {
                result.Append("Audio Format: " + audioFormat);
            }
            result.Append(" Numer of Channels: " + numChannels + "  Sample Rate: " + sampleRate + "  Bits/Sample: " + bitsPerSample);
            return result.ToString();
        }
    }
}
