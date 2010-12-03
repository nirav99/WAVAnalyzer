using System;
using System.Collections.Generic;
using System.Text;

namespace WavFileReader
{
    /// <summary>
    /// Class that encapsulates wav file data characteristics (Characteristics of WAVE data chunk)
    /// </summary>
    public class WavDataCharacteristics
    {
        private double snr;                 // Signal-Noise Ratio
        private uint totalFrames;           // Total frames in the audio
        private uint numSaturatedFrames;    // Number of frames having power level of 32767
        private uint numEmptyFrames;        // Number of empty frames
        private uint numSilenceFrames;      // Number of silence frames
        private uint numNonSilenceFrames;   // Number of non-silence frames
        private double maxPower;            // Max power in audio frames in decibels
        private double minPower;            // Min power in audio frames in decibels
        private double avgPower;            // Avg power of the audio file in decibels
        private long numFramesAvgPower;     // Number of frames for which average power was computed
        private short maxSample;            // Max sample value (max amplitude)
        private short minSample;            // Max sample value (min amplitude)
        private double dcOffset;            // DC offset - Need to know what this is (TODO)

        // Represents the longest sequence of consecutive empty frames once audio is detected
        private long numConsecutiveEmptyFrames;
        private double emptyAudioDuration;

        // Represents the longest sequence of consecutive silence frames once audio is detected
        private long numConsecutiveSilenceFrames;
        

        public WavDataCharacteristics()
        {
            snr = maxPower = minPower = dcOffset = 0;
            numSaturatedFrames = numEmptyFrames = numSilenceFrames = numNonSilenceFrames = 0;
            maxSample = minSample = 0;
            avgPower = 0;
            numFramesAvgPower = 0;
            numConsecutiveEmptyFrames = 0;
            numConsecutiveSilenceFrames = 0;
            emptyAudioDuration = 0;
        }

        public WavDataCharacteristics(uint _totalFrames, uint _satFrames, uint _emptyFrames, uint _silenceFrames, double _maxPower, double _minPower, short _maxSample, short _minSample, double _dcOffset)
        {
            snr = _maxPower - _minPower;
            totalFrames = _totalFrames;
            numSaturatedFrames = _satFrames;
            numEmptyFrames = _emptyFrames;
            numSilenceFrames = _silenceFrames;
            numNonSilenceFrames = totalFrames - numEmptyFrames - numSilenceFrames;
            maxPower = _maxPower;
            minPower = _minPower;
            maxSample = _maxSample;
            minSample = _minSample;
            dcOffset = _dcOffset;
            avgPower = 0;
            numFramesAvgPower = 0;
            numConsecutiveEmptyFrames = 0;
            numConsecutiveSilenceFrames = 0;
            emptyAudioDuration = 0;
        }

        public void display()
        {
            Console.WriteLine();
            Console.WriteLine("WAVE File Data Characteristics");
            Console.WriteLine();
            Console.WriteLine("Power Levels");
            Console.WriteLine("    Average Power: " + avgPower.ToString("F") + " dB calculated over " + numFramesAvgPower + " frames");
            Console.WriteLine("    SNR: {0}\n    Max Power: {1}\n    Min Power {2}", snr.ToString("F"), maxPower, minPower);
            Console.WriteLine();
            Console.WriteLine("Frame Characteristics");
            Console.WriteLine("    Empty Frames: {0}\n    Silence Frames: {1}\n    Non-Silence Frames: {2}\n    Saturated Frames: {3}", numEmptyFrames, numSilenceFrames, numNonSilenceFrames, numSaturatedFrames);
            Console.WriteLine("    Max Sample: {0}\n    Min Sample: {1}    \n    DC Offset: {2}", maxSample, minSample, dcOffset);

            if (numConsecutiveEmptyFrames > 0)
            {
                Console.WriteLine("    Num. consecutive empty frames: " + numConsecutiveEmptyFrames + ", Approximate Missing Audio Duration: " + emptyAudioDuration + " sec");
            }
            if (numConsecutiveSilenceFrames > 0)
            {
                Console.WriteLine("    Num. consecutive silence frames: " + numConsecutiveSilenceFrames);
            }
            Console.WriteLine();
        }

        #region Collection of properties to read values of state memebers
        public double MaxPower
        {
            get
            {
                return maxPower;
            }
        }

        public double MinPower
        {
            get
            {
                return minPower;
            }
        }

        public double SNR
        {
            get
            {
                return snr;
            }
        }

        public uint NumTotalFrames
        {
            get
            {
                return totalFrames;
            }
        }

        public uint NumEmptyFrames
        {
            get
            {
                return numEmptyFrames;
            }
        }

        public uint NumSilenceFrames
        {
            get
            {
                return numSilenceFrames;
            }
        }

        public uint NumNonSilenceFrames
        {
            get
            {
                return numNonSilenceFrames;
            }
        }

        public uint NumSaturatedFrames
        {
            get
            {
                return numSaturatedFrames;
            }
        }

        public short MaxSample
        {
            get
            {
                return maxSample;
            }
        }

        public short MinSample
        {
            get
            {
                return minSample;
            }
        }

        public double DCOffset
        {
            get
            {
                return dcOffset;
            }
        }

        public double AveragePower
        {
            get
            {
                return avgPower;
            }
            set
            {
                avgPower = value;
            }
        }

        public long NumFramesForAvgPower
        {
            get
            {
                return numFramesAvgPower;
            }
            set
            {
                numFramesAvgPower = value;
            }
        }
        /// <summary>
        /// Longest sequence of consecutive empty frames once audio was detected
        /// </summary>
        public long NumConsecutiveEmptyFrames
        {
            get
            {
                return numConsecutiveEmptyFrames;
            }
            set
            {
                numConsecutiveEmptyFrames = value;
            }
        }

        /// <summary>
        /// Longest sequence of consecutive silence frames once audio has been detected
        /// </summary>
        public long NumConsecutiveSilenceFrames
        {
            get
            {
                return numConsecutiveSilenceFrames;
            }
            set
            {
                numConsecutiveSilenceFrames = value;
            }
        }

        /// <summary>
        /// Approximate duration (in sec) for which highest sequence of consecutive empty frames was detected
        /// </summary>
        public double EmptyAudioDuration
        {
            get
            {
                return emptyAudioDuration;
            }
            set
            {
                emptyAudioDuration = value;
            }
        }

        #endregion
    }
}

