using System;
using System.Collections.Generic;
using System.Text;
using WavFileReader;

namespace Driver
{
    /// <summary>
    /// A simple driver program to demonstrate usage of WavFileReader
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.WriteLine("Usage:\nDriver.exe wavFileName");
                return;
            }

            WavFile wf = WavFile.getInstance(args[0]);

            if (wf != null)
            {
                WavDataCharacteristics wd = wf.analyzeData();
                Console.WriteLine(wf.getWavFileType());
                           
                wd.display();
            }
        }
    }
}
