using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace SevenZip
{

    public class Utility
    {
        public static void findFile(string dir, string pattern, List<string> list)
        {
            if (pattern.Length == 0)
                pattern = "*.*";

            DirectoryInfo info = new DirectoryInfo(dir);
            try
            {
                DirectoryInfo[] dirs = info.GetDirectories();
                foreach (DirectoryInfo d in dirs)
                    findFile(d.ToString() + "\\", pattern, list);

                System.IO.FileInfo[] files = info.GetFiles(pattern);
                foreach (var f in files)
                    list.Add(f.ToString());
            }
            catch (System.Exception e)
            {

            }
        }

        public static void compressLZMA(string srcFile, string desFile, bool isDeleteSrc = true)
        {
            if (File.Exists(desFile))
                File.Delete(desFile);
            FileStream src = new FileStream(srcFile, FileMode.Open);
            FileStream des = new FileStream(desFile, FileMode.CreateNew);
            SevenZip.Compression.LZMA.Encoder coder = new SevenZip.Compression.LZMA.Encoder();

            coder.WriteCoderProperties(des);
            des.Write(BitConverter.GetBytes(src.Length), 0, 8);

            coder.Code(src, des, src.Length, -1, null);

            src.Close();
            des.Flush();
            des.Close();
            if (isDeleteSrc)
                File.Delete(srcFile);
        }
        public static byte[] compressLZMA(byte[] srcArr)
        {
            byte[] desArr = null;
            MemoryStream src = new MemoryStream(srcArr);
            MemoryStream des = new MemoryStream();
            SevenZip.Compression.LZMA.Encoder coder = new SevenZip.Compression.LZMA.Encoder();

            coder.WriteCoderProperties(des);
            des.Write(BitConverter.GetBytes(src.Length), 0, 8);

            coder.Code(src, des, src.Length, -1, null);
            src.Close();
            des.Flush();
            desArr = des.ToArray();
            des.Close();
            return desArr;
        }

        public static void compressLZMAString(string srcStr, string desFile)  
        {
            if (File.Exists(desFile))
                File.Delete(desFile);
            byte[] srcArr = System.Text.Encoding.UTF8.GetBytes(srcStr);
            FileStream des = new FileStream(desFile, FileMode.CreateNew);
            MemoryStream src = new MemoryStream(srcArr);
            SevenZip.Compression.LZMA.Encoder coder = new SevenZip.Compression.LZMA.Encoder();
            coder.WriteCoderProperties(des);
            des.Write(BitConverter.GetBytes(src.Length), 0, 8);

            coder.Code(src, des, src.Length, -1, null);
            src.Close();
            des.Flush();
            des.Close();

        }
        public static string uncompressLZMAToString(byte[] srcArr)
        {
            byte[] desArr = uncompressLZMA(srcArr);
            return System.Text.Encoding.UTF8.GetString(desArr);
        }

        public static void uncompressLZMA(Stream src, Stream des)
        {
            byte[] pros = new byte[5];
            src.Read(pros, 0, 5);

            byte[] header = new byte[8];
            src.Read(header, 0, 8);
            long count = BitConverter.ToInt64(header, 0);

            SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
            coder.SetDecoderProperties(pros);
            coder.Code(src, des, src.Length, count, null);

            des.Flush();
        }
        public static byte[] uncompressLZMA(byte[] srcArr)
        {
            if (srcArr == null)
                return null;
            //UnityEngine.Profiling.Profiler.BeginSample("uncompressLZMA-byte[]");
            byte[] desArr = null;
            MemoryStream src = new MemoryStream(srcArr);
            //src.Flush();
            MemoryStream des = new MemoryStream();
            uncompressLZMA(src, des);
            desArr = des.ToArray();
            src.Close();
            //src.Dispose();
            des.Close();
            //des.Dispose();
            //UnityEngine.Profiling.Profiler.EndSample();
            return desArr;
        }
        public static void uncompressLZMA(string srcFile, string desFile, bool isDeleteSrc = true)
        {
            FileStream src = new FileStream(srcFile, FileMode.Open);
            FileStream des = new FileStream(desFile, FileMode.OpenOrCreate);
            uncompressLZMA(src, des);
            src.Close();
            des.Close();
            if(isDeleteSrc)
                File.Delete(srcFile);
        }


        //public static byte[] Compress(byte[] binary)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    ICSharpCode.SharpZipLib.GZip.GZipOutputStream gzip = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(ms);
        //    gzip.Write(binary, 0, binary.Length);
        //    gzip.Close();
        //    byte[] press = ms.ToArray();
        //    return press;
        //}

        //public static byte[] DeCompress(byte[] press)
        //{
        //    ICSharpCode.SharpZipLib.GZip.GZipInputStream gzi = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(new MemoryStream(press));
        //    MemoryStream re = new MemoryStream();
        //    int count = 0;
        //    byte[] data = new byte[4096];
        //    while ((count = gzi.Read(data, 0, data.Length)) != 0)
        //    {
        //        re.Write(data, 0, count);
        //    }
        //    byte[] depress = re.ToArray();
        //    return depress;
        //}
    }

}