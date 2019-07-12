using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
    public class MFileUtils
    {
        public static void WriteTxt(string url, string[] strs, System.Text.Encoding enc = null)
        {
            MFileUtils.WriteTxt(url, string.Join("\n", strs), enc);
        }
        public static void WriteTxt(string url, System.Text.StringBuilder sb, System.Text.Encoding enc = null)
        {
            WriteTxt(url, sb.ToString(), enc);
        }
        public static void WriteTxt(string url, string str, System.Text.Encoding enc = null)
        {
            if(enc == null)
                enc = new System.Text.UTF8Encoding(false);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(url, false, enc);
            sw.Write(str);
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }
        public static void WriteTxt(string url, byte[] bts, System.Text.Encoding enc = null)
        {
            if (enc == null)
                enc = new System.Text.UTF8Encoding(false);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(url, false, enc);
            sw.Write(bts);
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }
        public static void FindFile(string dir, string pattern, List<string> list)
        {
            if (pattern.Length == 0)
                pattern = "*.*";

            DirectoryInfo info = new DirectoryInfo(dir);
            try
            {
                DirectoryInfo[] dirs = info.GetDirectories();
                foreach (DirectoryInfo d in dirs)
                    FindFile(d.ToString() + "\\", pattern, list);

                System.IO.FileInfo[] files = info.GetFiles(pattern);
                foreach (var f in files)
                    list.Add(f.ToString());
            }
            catch (System.Exception e)
            {

            }
        }
        public static void ReplaceFile(string url, string replace, string to)
        {
            if (!File.Exists(url))
                return;
            string info = File.ReadAllText(url);
            info = info.Replace(replace, to);
            MFileUtils.WriteTxt(url, info);
        }
        public static void CopyDir(string from,string to,string key = "*",string exceptExtension = "")
        {
            if (!Directory.Exists(from))
                return;
            if (Directory.Exists(to))
                Directory.Delete(to, true);
            Directory.CreateDirectory(to);
            DirectoryInfo toDir = new DirectoryInfo(to);
            DirectoryInfo dir = new DirectoryInfo(from);
            FileInfo[] fls = dir.GetFiles(key, SearchOption.AllDirectories);
            int num = 0;
            for (int i = 0; i < fls.Length; i++)
            {
                FileInfo file = fls[i];
                if(!string.IsNullOrEmpty(exceptExtension) && file.Extension == exceptExtension)
                {
                    continue;
                }
                string fromName = file.FullName.Replace("\\", "/");
                string toName = file.FullName.Replace(dir.FullName, toDir.FullName);
                if (!Directory.Exists(Path.GetDirectoryName(toName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(toName));
                File.Copy(fromName, toName);
                num++;
            }
            Debug.Log("Copy To:" + to + ",Num:" + num);
        }
        public static string GetFileNameOfUrl(string flag)
        {
            string aName = flag.Replace(@"\", "").Replace("/", "_").Replace(":", "").Replace("*", "").Replace("?", "").Replace("<", "").Replace(">", "").Replace("|", "");
            return aName;
        }
        /// <summary>
        /// 写入文件;
        /// </summary>
        /// <param name="bytes"></param>
        public static string WriteFileStream(byte[] bytes, string outFile)
        {
            //string outFile = Application.persistentDataPath + "/" + outPath;
            outFile = outFile.Replace('\\', '/');
            string dir = Path.GetDirectoryName(outFile);
            //Log.GameMgr.Print("写入文件：" + outPath);
            //判断目录是否存在;
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            if (File.Exists(outFile))
                File.Delete(outFile);
            FileStream newFileStream = new FileStream(outFile, FileMode.OpenOrCreate, FileAccess.Write);
            //BinaryWriter binaryWriter = new BinaryWriter(newFileStream);
            //binaryWriter.Write(bytes);
            int arraySize = bytes.Length;
            newFileStream.Write(bytes, 0, arraySize);
            newFileStream.Flush();
            newFileStream.Close();
            newFileStream.Dispose();
            return outFile;
        }
        public static bool MoveFile(string source,string to,bool deleteOld = true)
        {
            if (!File.Exists(source))
            {
                Debug.LogError("不存在文件：" + source);
                return false;
            }
            try
            {
                to = to.Replace('\\', '/');
                string dir = Path.GetDirectoryName(to);
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                if (File.Exists(to))
                    File.Delete(to);
                File.Move(source, to);
                if (deleteOld)
                    File.Delete(source);
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
                return false;
            }
            return true;
        }
        //public static void CompressLZMA(string srcFile, string desFile, bool isDeleteSrc = true)
        //{
        //    FileStream src = new FileStream(srcFile, FileMode.Open);
        //    FileStream des = new FileStream(desFile, FileMode.OpenOrCreate);

        //    SevenZip.Compression.LZMA.Encoder coder = new SevenZip.Compression.LZMA.Encoder();

        //    coder.WriteCoderProperties(des);
        //    des.Write(BitConverter.GetBytes(src.Length), 0, 8);

        //    coder.Code(src, des, src.Length, -1, null);

        //    des.Flush();
        //    des.Close();
        //    src.Close();

        //    File.Delete(srcFile);
        //}

        //public static void UnCompressLZMA(Stream src, Stream des)
        //{
        //    byte[] pros = new byte[5];
        //    src.Read(pros, 0, 5);

        //    byte[] header = new byte[8];
        //    src.Read(header, 0, 8);
        //    long count = BitConverter.ToInt64(header, 0);

        //    SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
        //    coder.SetDecoderProperties(pros);
        //    coder.Code(src, des, src.Length, count, null);

        //    des.Flush();
        //}



        public static readonly char[] FOLDER_SEPARATOR_CHARS = new char[] { '/', '\\' };

        public static string GameAssetPathToName(string path)
        {
            int num = path.LastIndexOf('/');
            if (num < 0)
            {
                return path;
            }
            return path.Substring(num + 1);
        }

        public static string GameToSourceAssetName(string folder, string name, string ext = "prefab")
        {
            return string.Format("{0}/{1}.{2}", folder, name, ext);
        }

        public static string GameToSourceAssetPath(string path, string ext = "prefab")
        {
            return string.Format("{0}.{1}", path, ext);
        }

        public static string GetAssetPath(string fileName)
        {
            return fileName;
        }

        public static bool GetLastFolderAndFileFromPath(string path, out string folderName, out string fileName)
        {
            folderName = null;
            fileName = null;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            int num = path.LastIndexOfAny(FOLDER_SEPARATOR_CHARS);
            if (num > 0)
            {
                int num2 = path.LastIndexOfAny(FOLDER_SEPARATOR_CHARS, num - 1);
                int startIndex = (num2 >= 0) ? (num2 + 1) : 0;
                int length = num - startIndex;
                folderName = path.Substring(startIndex, length);
            }
            if (num < 0)
            {
                fileName = path;
            }
            else if (num < (path.Length - 1))
            {
                fileName = path.Substring(num + 1);
            }
            return ((folderName != null) || (fileName != null));
        }

        //public static string GetOnDiskCapitalizationForDir(DirectoryInfo dirInfo)
        //{
        //    DirectoryInfo parent = dirInfo.Parent;
        //    if (parent == null)
        //    {
        //        return dirInfo.Name;
        //    }
        //    string name = parent.GetDirectories(dirInfo.Name)[0].Name;
        //    return System.IO.Path.Combine(GetOnDiskCapitalizationForDir(parent), name);
        //}

        //public static string GetOnDiskCapitalizationForDir(string dirPath)
        //{
        //    DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
        //    return GetOnDiskCapitalizationForDir(dirInfo);
        //}

        //public static string GetOnDiskCapitalizationForFile(FileInfo fileInfo)
        //{
        //    System.IO.DirectoryInfo directory = fileInfo.Directory;
        //    string name = directory.GetFiles(fileInfo.Name)[0].Name;
        //    return System.IO.Path.Combine(GetOnDiskCapitalizationForDir(directory), name);
        //}

        //public static string GetOnDiskCapitalizationForFile(string filePath)
        //{
        //    FileInfo fileInfo = new FileInfo(filePath);
        //    return GetOnDiskCapitalizationForFile(fileInfo);
        //}

        //public static IntPtr LoadPlugin(string fileName, bool handleError = true)
        //{
        //    PlatformDependentValue<string> value3 = new PlatformDependentValue<string>(PlatformCategory.OS)
        //    {
        //        PC = "Hearthstone_Data/Plugins/{0}",
        //        Mac = "Hearthstone.app/Contents/Plugins/{0}.bundle/Contents/MacOS/{0}",
        //        iOS = string.Empty,
        //        Android = string.Empty
        //    };
        //    PlatformDependentValue<string> value2 = value3;
        //    try
        //    {
        //        string filename = string.Format((string)value2, fileName);
        //        IntPtr ptr = DLLUtils.LoadLibrary(filename);
        //        if ((ptr == IntPtr.Zero) && handleError)
        //        {
        //            string str2 = string.Format("{0}/{1}", ApplicationMgr.Get().GetWorkingDir(), filename);
        //            object[] messageArgs = new object[] { str2 };
        //            Error.AddDevFatal("Failed to load plugin from '{0}'", messageArgs);
        //            object[] objArray2 = new object[] { fileName };
        //            Error.AddFatalLoc("GLOBAL_ERROR_ASSET_LOAD_FAILED", objArray2);
        //        }
        //        return ptr;
        //    }
        //    catch (Exception exception)
        //    {
        //        object[] objArray3 = new object[] { exception.Message, exception.StackTrace };
        //        Error.AddDevFatal("FileUtils.LoadPlugin() - Exception occurred. message={0} stackTrace={1}", objArray3);
        //        return IntPtr.Zero;
        //    }
        //}

        //public static string MakeLocalizedPathFromSourcePath(Locale locale, string path)
        //{
        //    string directoryName = System.IO.Path.GetDirectoryName(path);
        //    string fileName = System.IO.Path.GetFileName(path);
        //    int startIndex = directoryName.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
        //    if ((startIndex >= 0) && directoryName.Substring(startIndex + 1).Equals(Localization.DEFAULT_LOCALE_NAME))
        //    {
        //        directoryName = directoryName.Remove(startIndex);
        //    }
        //    return string.Format("{0}/{1}/{2}", directoryName, locale, fileName);
        //}

        public static string MakeMetaPathFromSourcePath(string path)
        {
            return string.Format("{0}.meta", path);
        }

        public static string MakeSourceAssetMetaPath(string path)
        {
            return MakeMetaPathFromSourcePath(MakeSourceAssetPath(path));
        }

        public static string MakeSourceAssetPath(DirectoryInfo folder)
        {
            return MakeSourceAssetPath(folder.FullName);
        }

        public static string MakeSourceAssetPath(FileInfo fileInfo)
        {
            return MakeSourceAssetPath(fileInfo.FullName);
        }

        public static string MakeSourceAssetPath(string path)
        {
            string str = path.Replace(@"\", "/");
            int index = str.IndexOf("/Assets", StringComparison.OrdinalIgnoreCase);
            return str.Remove(0, index + 1);
        }

        public static bool ParseConfigFile(string filePath, ConfigFileEntryParseCallback callback)
        {
            return ParseConfigFile(filePath, callback, null);
        }

        public static bool ParseConfigFile(string filePath, ConfigFileEntryParseCallback callback, object userData)
        {
            if (callback == null)
            {
                Debug.LogWarning("FileUtils.ParseConfigFile() - no callback given");
                return false;
            }
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogWarning(string.Format("FileUtils.ParseConfigFile() - file {0} does not exist", filePath));
                return false;
            }
            int num = 1;
            using (StreamReader reader = System.IO.File.OpenText(filePath))
            {
                string baseKey = string.Empty;
                while (reader.Peek() != -1)
                {
                    string str2 = reader.ReadLine().Trim();
                    if ((str2.Length >= 1) && (str2[0] != ';'))
                    {
                        if (str2[0] == '[')
                        {
                            if (str2[str2.Length - 1] != ']')
                            {
                                Debug.LogWarning(string.Format("FileUtils.ParseConfigFile() - bad key name \"{0}\" on line {1} in file {2}", str2, num, filePath));
                            }
                            else
                            {
                                baseKey = str2.Substring(1, str2.Length - 2);
                            }
                        }
                        else
                        {
                            if (!str2.Contains("="))
                            {
                                Debug.LogWarning(string.Format("FileUtils.ParseConfigFile() - bad value pair \"{0}\" on line {1} in file {2}", str2, num, filePath));
                                continue;
                            }
                            char[] separator = new char[] { '=' };
                            string[] strArray = str2.Split(separator);
                            string subKey = strArray[0].Trim();
                            string val = strArray[1].Trim();
                            int num2 = val.Length - 1;
                            if ((((val[0] == '"') || (val[0] == '“')) || (val[0] == '”')) && (((val[num2] == '"') || (val[num2] == '“')) || (val[num2] == '”')))
                            {
                                val = val.Substring(1, val.Length - 2);
                            }
                            callback(baseKey, subKey, val, userData);
                        }
                    }
                }
            }
            return true;
        }

        public static bool SetFileWritableFlag(string path, bool setWritable)
        {
            bool flag = true;
            if (!System.IO.File.Exists(path))
            {
                return false;
            }
            try
            {
                FileAttributes attributes = System.IO.File.GetAttributes(path);
                FileAttributes fileAttributes = !setWritable ? (attributes | FileAttributes.ReadOnly) : (attributes & ~FileAttributes.ReadOnly);
                if (setWritable && (Environment.OSVersion.Platform == PlatformID.MacOSX))
                {
                    fileAttributes |= FileAttributes.Normal;
                }
                if (fileAttributes == attributes)
                {
                    return true;
                }
                System.IO.File.SetAttributes(path, fileAttributes);
                if (System.IO.File.GetAttributes(path) != fileAttributes)
                {
                    return false;
                }
                flag = true;
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception)
            {
            }
            return flag;
        }

        public static bool SetFolderWritableFlag(string dirPath, bool writable)
        {
            foreach (string str in Directory.GetFiles(dirPath))
            {
                SetFileWritableFlag(str, writable);
            }
            foreach (string str2 in Directory.GetDirectories(dirPath))
            {
                SetFolderWritableFlag(str2, writable);
            }
            return true;
        }

        public static string SourceToGameAssetName(string path)
        {
            int num = path.LastIndexOf('/');
            if (num < 0)
            {
                return path;
            }
            int length = path.LastIndexOf('.');
            if (length < 0)
            {
                return path;
            }
            return path.Substring(num + 1, length);
        }

        public static string SourceToGameAssetPath(string path)
        {
            int length = path.LastIndexOf('.');
            if (length < 0)
            {
                return path;
            }
            return path.Substring(0, length);
        }

        public static string BasePersistentDataPath
        {
            get
            {
                string str = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace('\\', '/');
                return string.Format("{0}/Blizzard/Hearthstone", str);
            }
        }

        public static string InternalPersistentDataPath
        {
            get
            {
                return string.Format("{0}/Dev", BasePersistentDataPath);
            }
        }

        //public static string PersistentDataPath
        //{
        //    get
        //    {
        //        string path = null;
        //        if (ApplicationMgr.IsInternal())
        //        {
        //            path = InternalPersistentDataPath;
        //        }
        //        else
        //        {
        //            path = PublicPersistentDataPath;
        //        }
        //        if (!Directory.Exists(path))
        //        {
        //            try
        //            {
        //                Directory.CreateDirectory(path);
        //            }
        //            catch (Exception exception)
        //            {
        //                Debug.LogError(string.Format("FileUtils.PersistentDataPath - Error creating {0}. Exception={1}", path, exception.Message));
        //                Error.AddFatalLoc("GLOBAL_ERROR_ASSET_CREATE_PERSISTENT_DATA_PATH", new object[0]);
        //            }
        //        }
        //        return path;
        //    }
        //}

        public static long GetFileSize(string fileName)
        {
            if (File.Exists(fileName))
            {
                FileInfo info = new FileInfo(fileName);
                return info.Length;
            }
            return 0;
        }
        public static string PublicPersistentDataPath
        {
            get
            {
                return BasePersistentDataPath;
            }
        }

        public delegate void ConfigFileEntryParseCallback(string baseKey, string subKey, string val, object userData);

        public static void byteToData()
        {
            var bytes = new byte[] { 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 4, 0, 0, 0, 0, 0, 0, 0 };
            GCHandle gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var data = (Data)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(Data));
            gcHandle.Free();
            //unsafe
            //{
            //    fixed (byte* packet = &data[0])
            //    {
            //        return *(GenericPacket*)packet;
            //    }
            //}
        }

    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class Data
    {
        public int _int1;
        public int _int2;
        public short _short1;
        public long _long1;
    }

