using System;
using System.Collections.Generic;
using System.Text;

namespace WavFileReader
{
    /// <summary>
    /// Generic utilities needed by the project
    /// </summary>
    public class Utilities
    {
        public static string getString(byte[] src, int cntBytes, int offset)
        {
            char[] dest = new char[cntBytes];
            int i;
            for (i = offset; i < offset + cntBytes; i++)
            {
                dest[i - offset] = Convert.ToChar(src[i]);
            }
            
            String s = new string(dest);
            return s;
        }

        public static byte[] getBytes(byte[] src, int cntBytes, int offset)
        {
            byte[] dest = new byte[cntBytes];

            for (int i = offset; i < offset + cntBytes; i++)
            {
                dest[i - offset] = src[i];
            }

            return dest;
        }

        public static uint getIntegerValue(byte[] b)
        {
            uint u = 0;

            if (b.Length == 4)
            {
                u = (uint)(b[0] | b[1] << 8 |
                   b[2] << 16 | b[3] << 24);
            }
            if (b.Length == 2)
            {
                u = (uint)(b[0] | b[1] << 8);
            }

            return u;
        }

        public static byte[] getByteValue(uint i)
        {
            byte[] b = new byte[4];
            uint mask = 0xFF;
            uint temp;

            b[0] = Convert.ToByte(i & 0xFF);
            b[1] = Convert.ToByte((i & 0xFF00) >> 8);
            b[2] = Convert.ToByte((i & 0xFF0000) >> 16);
            b[3] = Convert.ToByte((i & 0xFF000000) >> 24);

            return b;
        }

        public static byte[] getByteValue(string s)
        {
            if (s == null || s.Length == null)
                return null;
            byte[] b = new byte[s.Length];

            for (int i = 0; i < s.Length; i++)
                b[i] = Convert.ToByte(s[i]);
            return b;
        }

        public static void displayBytes(byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                Console.Write((b[i]));
            }
            Console.WriteLine();
        }
    }
}
