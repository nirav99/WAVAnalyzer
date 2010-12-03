using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WavFileReader
{
    /// <summary>
    /// Data Chunk of the WAVE file
    /// </summary>
    public class DataChunk
    {
        private string chunkID;                 // ID of data chunk "DATA"
        private uint chunkSize;                 // Size of data chunk
        private short[] wavData;                // Actual WAVE file data

        FMTChunk ftChunk;                       // Reference to FMT chunk

        #region WAVE data analysis related parameters
        private uint nFrameLengthMS = 20;       // Frame length in milli-sec
        private double dblNormalizeScaleFactor = .85;
        private double dblNoiseFloor = 40000.0;
        private int nLeadingSilenceLengthMS = 500;
        private int nTrailingSilenceLengthMS = 500;
        private short sMaxSample = -32768;
        private short sMinSample = 32767;
        private double dblDCOffset = 0.0;
        private uint nNumSaturatedFrames = 0;
        private double dblNormalizeFactor = 1.0;
        private double dblNormalizeContributionFactor = 0.75;
        #endregion

        private uint firstAudioFrameIdx = 0;    // First non-empty non-silence frame index
        private uint lastAudioFrameIdx = 0;     // Last non-empty non-silence frame index
       
        private uint cntTotalFrames = 0;        // Total number of frames in the wav file
        private uint numBytesPerFrame = 0;      // Number of bytes per frame
        private double avgPower = 0;            // Average power of each frame
        private long numFramesAvg = 0;          // Number of frames for which average was calculated

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="br"></param>
        /// <param name="_wavFile"></param>
        public DataChunk(BinaryReader br, FMTChunk _ftChunk)
        {
            byte[] buffer = br.ReadBytes(8);
            byte[] temp;

            ftChunk = _ftChunk;

            chunkID = Utilities.getString(buffer, 4, 0);
            temp = Utilities.getBytes(buffer, 4, 4);
            chunkSize = Utilities.getIntegerValue(temp);

            wavData = new short[chunkSize/2];
            for (uint i = 0; i < chunkSize / 2; i++)
            {
                wavData[i] = br.ReadInt16();
            }
        }

        /// <summary>
        /// Returns the data size in bytes
        /// </summary>
        /// <returns></returns>
        public uint dataChunkSizeInBytes
        {
            get
            {
                return chunkSize;
            }
        }

        /// <summary>
        /// Returns the total number of frames
        /// </summary>
        public uint totalFrames
        {
            get
            {
                return cntTotalFrames;
            }
        }

        /// <summary>
        /// Index of first non-empty, non-silence frame
        /// </summary>
        public uint numFirstAudioFrame
        {
            get
            {
                return firstAudioFrameIdx;
            }
        }

        /// <summary>
        /// Index of last non-empty, non-silence frame
        /// </summary>
        public uint numLastAudioFrame
        {
            get
            {
                return lastAudioFrameIdx;
            }
        }

        /// <summary>
        /// Number of bytes per frame
        /// </summary>
        public uint bytesPerFrame
        {
            get
            {
                return numBytesPerFrame;
            }
        }

        /// <summary>
        /// Returns the frame duration in sec
        /// </summary>
        public double frameDuration
        {
            get
            {
                return nFrameLengthMS / 1000.0;
            }
        }

        /// <summary>
        /// If the data chunk is valid
        /// </summary>
        /// <returns></returns>
        public bool validate()
        {
            return (chunkID.Equals("data", StringComparison.CurrentCultureIgnoreCase) && (wavData.Length * 2 == chunkSize));
        }

        public void printInfo()
        {
            Console.WriteLine("DATA Chunk ID = " + chunkID);
            Console.WriteLine("DATA Chunk Size = " + chunkSize);
        }

        /// <summary>
        /// Method to analyze the PCM data and provide information about max power, min power etc
        /// </summary>
        public WavDataCharacteristics analyzePCMData()
        {
            bool firstFrameFound = false;       // If first audio frame's index is already determined
            bool isFrameSilence = false;
            bool isFrameEmpty = false;

            long tempNumConsecutiveEmptyFrames = 0; // Temp variable for finding largest series of consecutive empty frames
            long numConsecutiveEmptyFrames = 0;

            long tempNumConsecutiveSilenceFrames = 0; // Temp variable for finding largest series of consecutive silence frames
            long numConsecutiveSilenceFrames = 0;

            uint nSamplesPerFrame;
            uint nNumFrames;
            uint nFrameCount;
            short currentSample;                // Current WAVE data sample

            uint nSamplesPerSec = ftChunk.SampleRate;
            uint nRawSampleCount = chunkSize / (ftChunk.BitsPerSample / 8);

            // compute number of samples per frame and number of frames
            nSamplesPerFrame = nFrameLengthMS * nSamplesPerSec / 1000;
            nNumFrames = (nRawSampleCount - 1) / nSamplesPerFrame;

            //Console.WriteLine("Num Samples Per Sec = " + nSamplesPerSec);
            //Console.WriteLine("Bits per Sample = " + ftChunk.BitsPerSample);
            //Console.WriteLine("Num Samples Per Frame = " + nSamplesPerFrame);
            //Console.WriteLine("Num Frames = " + nNumFrames);
            //Console.WriteLine("Their Product = " + nSamplesPerFrame * nNumFrames);

            // Now we compute the number of bytes per frame
            numBytesPerFrame = ftChunk.BitsPerSample * nSamplesPerFrame / 8;
            //Console.WriteLine("Number of Bytes per Frame = " + numBytesPerFrame);

            // Now we will analyze the raw audio checking for saturation, determining the minsample 
            // and maxsample, and compute the DC offset.
            int nNormalizeSampleCount = (int)(dblNormalizeContributionFactor * nRawSampleCount);
            for (int i = 0; i < nNormalizeSampleCount; i++)
            {
                currentSample = wavData[i];

                if ((currentSample == 32767) || (currentSample == -32768))
                {
                    nNumSaturatedFrames++;
                }
                if (currentSample > sMaxSample)
                {
                    sMaxSample = currentSample;
                }
                if (currentSample < sMinSample)
                {
                    sMinSample = currentSample;
                }
                dblDCOffset += currentSample;
            }
            
            if (nNormalizeSampleCount > 0)
            {
                dblDCOffset = dblDCOffset / nNormalizeSampleCount;
            }

            uint dataIdx;  // Index into WAVE data
            double dblMaxPower = -1000.0;
            double dblMinPower = 1000.0;
            uint nNumEmptyFrames = 0;
            uint nNumSilenceFrames = 0;
            double dblPower;
            short sSample = 0;

            for (uint nFrameIndex = 0; nFrameIndex < nNumFrames; nFrameIndex++)
            {
                dataIdx = nFrameIndex * nSamplesPerFrame;
                dblPower = 0.0;

                isFrameEmpty = false;
                isFrameSilence = false;

                for (int nSampleIndex = 0; nSampleIndex < nSamplesPerFrame; nSampleIndex++)
                {
                    //try
                    //{
                    //    sSample = Convert.ToInt16(wavData[dataIdx + nSampleIndex + 1] - wavData[dataIdx + nSampleIndex]);
                    //}
                    //catch (Exception e)
                    //{
                    //    Console.WriteLine(e.Message + " " + e.StackTrace);
                    //    Console.WriteLine(wavData[dataIdx + nSampleIndex + 1]);
                    //    Console.WriteLine(wavData[dataIdx + nSampleIndex]);
                    //}

                    //dblPower += sSample * sSample;


                    // If the transition between frames is very much abrupt, the difference could be more than 32767, the limit
                    // of Int16. Thus, we don't compute actual difference, instead we just use 32767.
                    if (wavData[dataIdx + nSampleIndex + 1] - wavData[dataIdx + nSampleIndex] <= 32767 && (wavData[dataIdx + nSampleIndex + 1] - wavData[dataIdx + nSampleIndex] >= -32768))
                    {
                        dblPower += Math.Pow(wavData[dataIdx + nSampleIndex + 1] - wavData[dataIdx + nSampleIndex], 2.0);
                    }
                    else
                    {
                        dblPower += Math.Pow(32767, 2.0);
                    }
                }

                if (dblPower == 0.0)
                {
                    ++nNumEmptyFrames;
                    isFrameEmpty = true;
                }
                else
                {
                    if ((dblPower / nSamplesPerFrame) <= dblNoiseFloor)
                    {
                        ++nNumSilenceFrames;
                        isFrameSilence = true;
                    }
                    dblPower = 10.0 * Math.Log10(dblPower / nSamplesPerFrame);
                    if (dblPower > dblMaxPower)
                    {
                        dblMaxPower = dblPower;
                    }
                    if (dblPower < dblMinPower)
                    {
                        dblMinPower = dblPower;
                    }
                }

                if (isFrameSilence == false && isFrameEmpty == false)
                {
                    avgPower += dblPower;
                    numFramesAvg++;

                    // Keep modifying the index of the last audio frame until we detect it at the end of the audio
                    lastAudioFrameIdx = nFrameIndex;

                    // If this was the first audio-frame, we want to keep track of it
                    if (firstFrameFound == false)
                    {
                        firstAudioFrameIdx = nFrameIndex;
                        firstFrameFound = true;
                    }
                }

                #region Code to detect the longest sequence of continuous empty frames
                // If an empty frame has been found after the audio frame, increment numConsecutiveEmptyFrames
                if (isFrameEmpty == true && firstFrameFound == true)
                {
                    tempNumConsecutiveEmptyFrames++;
                }
                else
                if (isFrameEmpty == false)
                {
                    if (tempNumConsecutiveEmptyFrames > numConsecutiveEmptyFrames)
                    {
                        numConsecutiveEmptyFrames = tempNumConsecutiveEmptyFrames;
                    }
                    tempNumConsecutiveEmptyFrames = 0;
                }
                #endregion

                #region Code to detect the longest sequence of continous silence frames
                if (isFrameSilence == true && firstFrameFound == true)
                {
                    tempNumConsecutiveSilenceFrames++;
                }
                else
                if(isFrameSilence == false)
                {
                    if (tempNumConsecutiveSilenceFrames > numConsecutiveSilenceFrames)
                    {
                        numConsecutiveSilenceFrames = tempNumConsecutiveSilenceFrames;
                    }
                    tempNumConsecutiveSilenceFrames = 0;
                }
                #endregion
            }

            if (tempNumConsecutiveSilenceFrames > numConsecutiveSilenceFrames)
                numConsecutiveSilenceFrames = tempNumConsecutiveSilenceFrames;

            if (tempNumConsecutiveEmptyFrames > numConsecutiveEmptyFrames)
                numConsecutiveEmptyFrames = tempNumConsecutiveEmptyFrames;

           cntTotalFrames = nNumFrames;
           
           // Calculate the average power for the audio file
           avgPower = avgPower / numFramesAvg * 1.0;
 
           WavDataCharacteristics wd = new WavDataCharacteristics(nNumFrames, nNumSaturatedFrames, nNumEmptyFrames, nNumSilenceFrames, dblMaxPower, dblMinPower, sMaxSample, sMinSample, dblDCOffset);
           wd.AveragePower = avgPower;
           wd.NumFramesForAvgPower = numFramesAvg;
           wd.NumConsecutiveEmptyFrames = numConsecutiveEmptyFrames;
           wd.NumConsecutiveSilenceFrames = numConsecutiveSilenceFrames;

           wd.EmptyAudioDuration = numConsecutiveEmptyFrames * frameDuration;
 
           //Console.WriteLine("Average Power = {0} over {1} frames ",avgPower, numFramesAvg);
           //Console.WriteLine("First Audio frame index = " + firstAudioFrameIdx);
           //Console.WriteLine("Last Audio frame index = " + lastAudioFrameIdx);
           //Console.WriteLine("Largest Number of consecutive empty frames inside Audio = " + numConsecutiveEmptyFrames);
           //Console.WriteLine("Duration for empty audio = " + numConsecutiveEmptyFrames * nFrameLengthMS / 1000.0 + " sec");
           return wd;
        }
    }
}
