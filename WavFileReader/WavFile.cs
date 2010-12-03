using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WavFileReader
{
    /// <summary>
    /// Class encapsulating 8KHz 16bit Mono PCM WAVE file
    /// </summary>
    public class WavFile
    {
        BinaryReader br = null;                     // To read the wav file
        private RIFFChunk rf = null;                // RIFF Chunk of WAVE file
        private FMTChunk ft = null;                 // FMT Chunk of WAVE file
        private DataChunk dt = null;                // Data chunk
        private WavDataCharacteristics wd = null;   // Class that encapsulates properties of wav data
        private string wavFileName = null;          // Name of the WAV file

        private WavFile(BinaryReader br)
        {
            rf = new RIFFChunk(br);

            if (rf.validate() == false)
                throw new Exception("RIFF chunk invalid");

            ft = new FMTChunk(br);
           
            if (ft.validate() == false)
            {
             //   ft.printInfo();
                throw new Exception("FMT chunk invalid. WAVE file is not 8KHz 16bit Mono PCM WAVE file.");
            }
            
            dt = new DataChunk(br, ft);
            
            if (dt.validate() == false)
            {
                throw new Exception("Data Chunk invalid");
            }

            wd = dt.analyzePCMData();
        }

        /// <summary>
        /// Method to obtain a new instance of this class
        /// </summary>
        /// <param name="wavFileName"></param>
        /// <returns></returns>
        public static WavFile getInstance(string wavFileName)
        {
            FileStream fs = null;
            BinaryReader br = null;
            WavFile wf = null;

            try
            {
                fs = new FileStream(wavFileName, FileMode.Open);
                br = new BinaryReader(fs);
                wf = new WavFile(br);
                wf.wavFileName = wavFileName;
            }
            catch (IOException ie)
            {
                Console.WriteLine("Exception in loading wav file " + wavFileName + ". Message: " + ie.Message + " Stack Trace = " + ie.StackTrace);
                wf = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in processing wav file " + wavFileName + ". Message : " + e.Message + " Stack Trace = " + e.StackTrace);
                wf = null;
            }
           
            finally
            {
                if(fs != null)
                    fs.Close();
            }
            return wf;
        }

        /// <summary>
        /// Method to obtain WAV file characteristics
        /// </summary>
        public void display()
        {
            Console.WriteLine("WAVE File Characteristics");
            Console.WriteLine(ft.ToString());

            if (wd == null)
            {
                wd = dt.analyzePCMData();
            }
            wd.display();
        }

        public string getWavFileType()
        {
            return ft.ToString();
        }

        public WavDataCharacteristics analyzeData()
        {
            if (wd == null)
            {
                wd = dt.analyzePCMData();
            }
            return wd;
        }

        //public void trim(string trimmedFileName)
        //{
        //    trimmedFileName = "c:\\temp\\" + trimmedFileName;

        //    //Create a new stream for trimmeeFileName
        //    FileStream fs = new FileStream(trimmedFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        //    BinaryWriter bw = new BinaryWriter(fs);
            
        //    // Open the original wav file to read from
        //    FileStream fs2 = new FileStream(wavFileName, FileMode.Open);
        //    br = new BinaryReader(fs2);
                      
        //    bw.Write(br.ReadBytes(12));
        //    bw.Write(br.ReadBytes(24));
        //    bw.Write(br.ReadBytes(4));

        //    uint idxFirstFrame = dt.numFirstAudioFrame;
        //    uint idxLastFrame = dt.numLastAudioFrame;
        //    uint totalFrames = dt.totalFrames;

        //    Console.WriteLine(idxFirstFrame + " " + idxLastFrame + " " + totalFrames);
        //    uint FramesToKeep = idxLastFrame - idxFirstFrame + 1;
        //    uint framesToRemove = totalFrames - FramesToKeep;

        //    Console.WriteLine("Frames to keep = " + FramesToKeep);
        //    byte[] dataSize = Utilities.getByteValue(FramesToKeep * dt.bytesPerFrame);
        //    bw.Write(dataSize, 0, 4);
           
        //    // Discard leading non-audio bytes
        //    Console.WriteLine("Skipping over " + (idxFirstFrame - 1) * dt.bytesPerFrame + " bytes");
        //    for(uint i = 0; i < (idxFirstFrame - 1) * dt.bytesPerFrame; i++)
        //    {
        //       Byte temp = br.ReadByte();
        //    }

        //    for (int i = 0; i < FramesToKeep * dt.bytesPerFrame; i++)
        //    {
        //        bw.Write(br.ReadByte());
        //    }
        //    br.Close();
        //    bw.Close();
        //}
    }
}
