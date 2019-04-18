/*资源工具类
 * 1.Txt生成，读取
 * 2.资源压缩，解压
 * 3.资源下载，断点续传
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif


public static class MResTools
{
    public delegate void IsOKDelegate(bool IsOK, string errorInfo);
    public class mFileInfo
    {
        public long size = 0;
        public int count = 0;
        public IsOKDelegate callBack;
        public MyThread thread;
        public void CallBack()
        {
            if(callBack != null)
            {
                callBack(thread.isOk,thread.error);
            }
        }
    }
    public static WWW downLoadWWW;//WWW下载可获取进度

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="mUrl">下载地址</param>
    /// <param name="OutPut">输出路径</param>
    /// <param name="mIsOK">完成回调</param>
    public static IEnumerator DownLoad(string mUrl, string OutPut, IsOKDelegate mIsOK)
    {
        downLoadWWW = new WWW(mUrl);
        yield return downLoadWWW;
        if (downLoadWWW.error != null)
        {
            string errorInfo = downLoadWWW.error;
            downLoadWWW.Dispose();
            downLoadWWW = null;
            if (mIsOK != null)
            {
                mIsOK(false, errorInfo);
            }
        }
        else
        {
            if (downLoadWWW.bytes.Length > 0)
            {
                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(OutPut)))
                        Directory.CreateDirectory(Path.GetDirectoryName(OutPut));
                    File.WriteAllBytes(OutPut, downLoadWWW.bytes);
                    downLoadWWW.Dispose();
                    downLoadWWW = null;
                    if (mIsOK != null)
                    {
                        mIsOK(true, "");
                    }
                }
                catch (Exception e)
                {
                    if (mIsOK != null)
                    {
                        mIsOK(false, e.ToString());
                    }
                }
            }
            else
            {
                downLoadWWW.Dispose();
                downLoadWWW = null;
                if (mIsOK != null)
                {
                    mIsOK(false, "Down Assets is null");
                }
            }
        }
    }

   // public static Thread DecompressionThrd;//解压线程
    public static bool IsDecompression;//是否在解压
    public static int[] DecompressionProgress = new int[1];//解压进度
    public static mFileInfo DecompressionInfo;
    /// <summary>
    /// 解压文件（7Z，Zip）
    /// </summary>
    /// <param name="mInPut">输入路径</param>
    /// <param name="mOutPut">输出路径</param>
    /// <param name="mIsOK">完成回调</param>
    public static mFileInfo FileDecompression(string mInPut, string mOutPut, IsOKDelegate mIsOK)
    {
        if (!Directory.Exists(mOutPut))
            Directory.CreateDirectory(mOutPut);
        if (mInPut.ToLower().EndsWith(".7z"))
        {
            /*
#if UNITY_IOS
            mFileInfo mInfo = new mFileInfo();
            mInfo.size = lzma.get7zInfo(mInPut);
            mInfo.count = lzma.trueTotalFiles;
            DecompressionInfo = mInfo;
            return;
#endif
             * */
            MyThread mt = new MyThread(mInPut, mOutPut);
            highlight.Loom.RunAsync(mt.doDecompressionThrd);
           // DecompressionThrd = new Thread(new ThreadStart(mt.doDecompressionThrd));
           // DecompressionThrd.Start();     
       
            mFileInfo mInfo = new mFileInfo();
         //   mInfo.size = lzma.get7zInfo(mInPut);
         //   mInfo.count = lzma.trueTotalFiles;
            mInfo.callBack = mIsOK;
            mInfo.thread = mt;
            DecompressionInfo = mInfo;
            WWWHttpHelper.SendDepressFile(mInfo);
            return mInfo;
        }
        else if (mInPut.ToLower().EndsWith(".zip"))
        {
            MyThread mt = new MyThread(mInPut, mOutPut);
            highlight.Loom.RunAsync(mt.doDecompressionThrd);
            //DecompressionThrd = new Thread(new ThreadStart(mt.doDecompressionThrd));
            //DecompressionThrd.Start(); 

            mFileInfo mInfo = new mFileInfo();
            mInfo.callBack = mIsOK;
            mInfo.thread = mt;
         //   mInfo.size = lzip.getFileInfo(mInPut, Application.persistentDataPath);
          //  mInfo.count = lzip.zipFiles;
            DecompressionInfo = mInfo;
            WWWHttpHelper.SendDepressFile(mInfo);
            return mInfo;
        }
        else
        {
            if (mIsOK != null)
            {
                mIsOK(false, "暂时不支持" + Path.GetExtension(mInPut) + "解压");
            }
        }
        return null;
    }

    /// <summary>
    /// 线程解压
    /// </summary>
    public class MyThread
    {
        string mInPut;
        string mOutPut;
        public bool isOk = false;
        public string error;
        public bool isEnd = false;
        public MyThread(string sInPut, string sOutPut)
        {
            mInPut = sInPut;
            mOutPut = sOutPut;
        }
        public void doDecompressionThrd()
        {
            
            int ErrorCode=-500;
            IsDecompression = true;
            if (mInPut.ToLower().EndsWith(".7z"))
            {
                ErrorCode = lzma.doDecompress7zip(mInPut, mOutPut, DecompressionProgress, true, true);
               // if (mIsOK != null)
               // {
                    string errorInfo = "";
                    switch (ErrorCode)
                    {
                        case 2:
                            errorInfo = "ErrorCode:" + ErrorCode + "\n" + "Could not find requested file in archive";
                            break;
                        case -1:
                            errorInfo = "ErrorCode:" + ErrorCode + "\n" + "Could not open input(7z) file";
                            break;
                        case -2:
                            errorInfo = "ErrorCode:" + ErrorCode + "\n" + "Decoder doesn't support this archive";
                            break;
                        case -3:
                            errorInfo = "ErrorCode:" + ErrorCode + "\n" + "Can not allocate memory";
                            break;
                        case -4:
                            errorInfo = "ErrorCode:" + ErrorCode + "\n" + "CRC error of 7z file";
                            break;
                        case -5:
                            errorInfo = "ErrorCode:" + ErrorCode + "\n" + "Unknown error";
                            break;
                        case -6:
                            errorInfo = "ErrorCode:" + ErrorCode + "\n" + "Can not open output file (usually when the path to write to, is invalid)";
                            break;
                        case -7:
                            errorInfo = "ErrorCode:" + ErrorCode + "\n" + "Can not write output file";
                            break;
                        case -8:
                            errorInfo = "ErrorCode:" + ErrorCode + "\n" + "Can not close output file";
                            break;
                    }
                    compressOk(ErrorCode == 1, errorInfo);
               // }
            }
            else if (mInPut.ToLower().EndsWith(".zip"))
            {
                ErrorCode = lzip.decompress_File(mInPut, mOutPut, DecompressionProgress);
                //if (mIsOK != null)
                //{
                    string errorInfo = "";
                    switch (ErrorCode)
                    {
                        case -1:
                            errorInfo = "ErrorCode:" + ErrorCode + "\n" + "could not initialize zip archive.";
                            break;
                        case -2:
                            errorInfo = "ErrorCode:" + ErrorCode + "\n" + "failed reading content info";
                            break;
                        case -3:
                            errorInfo = "ErrorCode:" + ErrorCode + "\n" + "failed extraction";
                            break;
                    }
                    compressOk(ErrorCode == 1, errorInfo);
                //}
            }
            IsDecompression = false;
            
        }
        void compressOk(bool IsOK, string errorInfo)
        {
            this.isOk = IsOK;
            this.error = errorInfo;
            isEnd = true;
        }
    }

    /// <summary>
    /// 压缩文件
    /// </summary>
    /// <param name="mInPut">输入路径</param>
    /// <param name="mOutPut">输出路径</param>
    /// <param name="mIsOK">完成回调</param>
	public static void FileCompression(string mInPut, string mOutPut)
    {
        if (mOutPut.ToLower().EndsWith(".7z"))
        {
			Compress7z(mInPut, mOutPut);
        }
        else if (mOutPut.ToLower().EndsWith(".zip"))
        {

        }  
    }

	static void Compress7z(string mInPut, string mOutPut)
    {
        try
        {
            if(Application.platform == RuntimePlatform.OSXEditor)
            {
				System.Diagnostics.Process pNew = new System.Diagnostics.Process();
				pNew.StartInfo.FileName = Application.dataPath + @"\..\..\Other\Tools\7-Zip-linux\p7zip_16.02\bin\7za"; ;
				pNew.StartInfo.Arguments = string.Format(" a -t7z {0} {1} -m0=LZMA2:d=26 ", mOutPut, mInPut);
             //   pNew.StartInfo.UserName = "yk";
            //    pNew.StartInfo.Password = new System.Security.SecureString() "123";

                pNew.StartInfo.CreateNoWindow = true;
                pNew.StartInfo.UseShellExecute = false;
                pNew.StartInfo.RedirectStandardError = true;
                pNew.StartInfo.RedirectStandardOutput = true;


                pNew.Start();
				//一定要等待完成后，才能删除。
				pNew.WaitForExit();
				pNew.Close();
				//压完后删除原有的
				//File.Delete(backupFileFullName);
            }
            else
            {
 
                System.Diagnostics.Process pNew = new System.Diagnostics.Process();
			    pNew.StartInfo.FileName = Application.dataPath + @"\..\..\Other\Tools\7-Zip\7z.exe"; ;
				 
                
                pNew.StartInfo.Arguments = string.Format(" a -t7z {0} {1} -m0=LZMA2:d=26 ", mOutPut, mInPut);
                pNew.Start();
                //一定要等待完成后，才能删除。
                pNew.WaitForExit();
                pNew.Close();
                //压完后删除原有的
                //File.Delete(backupFileFullName);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }

    }

    #region 递归读取路径下所有文件
    /// <summary>
    /// 获取指定路径下的所有文件名
    /// </summary>
    /// <param name="rootPath">路径</param>
    /// <param name="removeEndStr">需要去除的指定结尾符文件</param>
    /// <returns></returns>
    public static List<string> GetFileList(string rootPath, string[] removeEndStr,bool IsFan=false)
    {
        List<string> mNames = new List<string>();
        mFileNames.Clear();
        GetAllDirectories(rootPath, removeEndStr, IsFan);
        mNames = mFileNames;
        return mNames;
    }
    static List<string> mFileNames = new List<string>();
    private static void GetAllDirectories(string rootPath, string[] removeEndStr, bool IsFan = false)
    {
        string[] subPaths = System.IO.Directory.GetDirectories(rootPath);//得到所有子目录    
        foreach (string path in subPaths)
        {
            GetAllDirectories(path, removeEndStr, IsFan);//对每一个字目录做与根目录相同的操作：即找到子目录并将当前目录的文件名存入List    
        }
        string[] files = System.IO.Directory.GetFiles(rootPath);
        foreach (string file in files)
        {
            string a = file.Replace("\\", "/");
            if (IsFan)
            {
                if (removeEndStr != null && removeEndStr.Length > 0)
                {
                    List<string> mExtensions = new List<string>(removeEndStr);
                    string Extension = Path.GetExtension(file).ToLower();
                    if (mExtensions.Contains(Extension))
                    {
                        mFileNames.Add(a);//将当前目录中的所有文件全名存入文件List
                    }
                }
            }
            else
            {
                if (removeEndStr != null && removeEndStr.Length > 0)
                {
                    List<string> mExtensions = new List<string>(removeEndStr);
                    string Extension = Path.GetExtension(file).ToLower();
                    if (!mExtensions.Contains(Extension))
                    {
                        mFileNames.Add(a);//将当前目录中的所有文件全名存入文件List
                    }
                }
                else
                {
                    mFileNames.Add(a);//将当前目录中的所有文件全名存入文件List    
                }
            }
        }
    }
    #endregion

    #region 资源分类

    public static int GetResType(string mPath)
    {
        if (IsResA(mPath))
        {
            return 1;
        }
        else if (IsResB(mPath))
        {
            return 2;
        }
        else if (IsResC(mPath))
        {
            return 3;
        }
        else if (IsResD(mPath))
        {
            return 4;
        }
        else
        {
            Debug.LogError("资源类型错误：" + mPath);
        }
        return 0;
    }

    /// <summary>
    /// 是否是A级资源
    /// </summary>
    /// <param name="mPath">路径</param>
    /// <returns></returns>
    public static bool IsResA(string mPath)
    {
        mPath = mPath.ToLower();
        return (mPath.Contains(".jpg") || mPath.Contains(".png") || mPath.Contains(".tga") || mPath.Contains(".tif") || mPath.Contains(".exr") || mPath.Contains(".psd") || mPath.Contains(".txt") || mPath.Contains(".anim") || mPath.Contains(".mp3") || mPath.Contains(".wav") || mPath.Contains(".cs") || mPath.Contains(".js") || mPath.Contains(".shader"));
    }

    /// <summary>
    /// 是否是B级资源
    /// </summary>
    /// <param name="mPath">路径</param>
    /// <returns></returns>
    public static bool IsResB(string mPath)
    {
        mPath = mPath.ToLower();
        return (mPath.Contains(".mat") || mPath.Contains(".asset"));
    }

    /// <summary>
    /// 是否是C级资源
    /// </summary>
    /// <param name="mPath">路径</param>
    /// <returns></returns>
    public static bool IsResC(string mPath)
    {
        mPath = mPath.ToLower();
        return (mPath.Contains(".fbx") || mPath.Contains(".ttf") || mPath.Contains(".fnt"));
    }

    /// <summary>
    /// 是否是D级资源
    /// </summary>
    /// <param name="mPath">路径</param>
    /// <returns></returns>
    public static bool IsResD(string mPath)
    {
        mPath = mPath.ToLower();
        return (mPath.Contains(".prefab"));
    }
    #endregion

    #region 资源加密
    /// <summary>
    /// 字节加密
    /// </summary>
    /// <param name="content"></param>
    /// <param name="secretKey"></param>
    /// <returns></returns>
    public static byte[] byteEncrypt(byte[] content, string secretKey)
    {
        byte[] data = content;
        byte[] key = System.Text.Encoding.Default.GetBytes(secretKey);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= key[i % key.Length];
        }

        return data;
    }

    /// <summary>
    /// 字节解密
    /// </summary>
    /// <param name="data"></param>
    /// <param name="secretKey"></param>
    /// <returns></returns>
    public static byte[] byteDecrypt(byte[] data, string secretKey)
    {
        byte[] key = System.Text.Encoding.Default.GetBytes(secretKey);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= key[i % key.Length];
        }

        return data;
    }
    #endregion

}

