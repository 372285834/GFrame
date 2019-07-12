using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
namespace highlight
{
    public class CompareMD5
    {
        public static string FLAGSTR = ",";
        public CompareMD5(string _dir, string _name, int _outageValue = 10)
        {
            this.dirPath = _dir;
            this.md5Name = _dir + _name;
            outageValue = _outageValue;
        }
        int outageValue = 5;
        //本地MD5;
        protected List<string> tempMD5 = new List<string>();
        public List<string> getTempMD5List { get { return tempMD5; } }
        //MD5资源表;
        private Dictionary<string, MD5Config> dicOldMD5Info = new Dictionary<string, MD5Config>();

        //md5数据目录路径;
        public string dirPath;
        public string md5Name;
        public bool isSort = true;
        public List<MD5Config> GetDifferentList(List<MD5Config> list)
        {
            List<MD5Config> needList = new List<MD5Config>();
            foreach (string key in dicOldMD5Info.Keys)
            {
                MD5Config data = list.Find(x => x.url == key);
                if (data != null)
                {
                    if (data.md5Num != dicOldMD5Info[key].md5Num)
                    {
                        needList.Add(data);
                    }
                }
            }
            return needList;
        }
        public List<MD5Config> GetDeleteList(List<MD5Config> list)
        {
            List<MD5Config> deleteList = new List<MD5Config>();
            foreach (string key in dicOldMD5Info.Keys)
            {
                MD5Config data = list.Find(x => x.url == key);
                if (data == null)
                {
                    deleteList.Add(dicOldMD5Info[key]);
                }
            }
            return deleteList;
        }

        /// <summary>
        /// false为不相同
        /// </summary>
        /// <param name="md5Str"></param>
        /// <returns></returns>
        public bool Check(string md5Num, string url, bool add = true)
        {
            bool b = false;
            if (dicOldMD5Info.ContainsKey(url))
            {
                MD5Config con = dicOldMD5Info[url];
                //Debug.Log("本地md5>>>" + con.md5Num + ", 新 md5:" + md5Num);
                if (con.md5Num == md5Num)
                {
                    b = true;
                    con.IsOld = false;
                }
            }
            if (!b && add)
            {

                this.AddMD5(md5Num, url);
            }
            //Debug.Log("对比md5>>>" + _md5 + ", url:" + _url + ", b" + b);
            return b;
        }
        //false为不相同
        public bool Check(FileInfo file, bool add = true, string tag = "Assets")
        {
            if (!mIsInit)
                return false;
            int idx = file.FullName.LastIndexOf(tag);
            if(idx < 0)
            {
                throw new Exception("idx == -1  url:" + file.FullName + "    tag:" + tag);
            }
            bool b = false;
            string url = file.FullName.Substring(idx + tag.Length + 1).Replace("\\", "/");
            string md5 = CompareMD5.GetMD5HashFromFile(file.FullName);
            if (dicOldMD5Info.ContainsKey(url))
            {
                MD5Config con = dicOldMD5Info[url];
                //Debug.Log("本地md5>>>" + con.md5Num + ", 新 md5:" + md5Num);
                if (con.md5Num == md5)
                {
                    b = true;
                    con.IsOld = false;
                }
            }
            if (!b&&add)
            {

                this.AddMD5(md5, url);
            }
            return b;
        }
        /// <summary>
        /// 存储MD5;
        /// </summary>
        public void AddMD5(string _md5, string _url, int currentNum=1)
        {
            MD5Config con = null;
            if (dicOldMD5Info.ContainsKey(_url))
            {
                con = dicOldMD5Info[_url];
                tempMD5.Remove(con.GetContent(FLAGSTR));
                con.md5Num = _md5;
            }
            else
            {
                con = new MD5Config();
                con.md5Num = _md5;
                con.url = _url;
                dicOldMD5Info.Add(con.url, con);
            }
            tempMD5.Add(con.GetContent(FLAGSTR));
            con.IsOld = false;
            if (currentNum % outageValue == 0 || this.Length == currentNum)
            {
                SaveMD5();
            }
        }
        public void SaveMD5(string[] saveStr = null)
        {
            //Debug.Log("totalNum:" + totalNum + ", currentNum" + currentNum + ", 保存md5:" + infoAll);
            //SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
            if (isSort)
                tempMD5.Sort();
            saveStr = saveStr == null ? tempMD5.ToArray() : saveStr;
            MFileUtils.WriteTxt(md5Name, saveStr);
          //  string save = string.Join("\r\n", saveStr);
            //Debug.Log("保存md5>>>" + save);
            //save = Util.Compress(save);
          //  StreamWriter sw = new StreamWriter(md5Name, false);
          //  sw.Write(save);
         //   sw.Close();
        }
        private bool mIsInit = false;
        /// <summary>
        /// 读取文本文件;
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<string> InitLocal(string content = "")
        {
            mIsInit = true;
            dicOldMD5Info.Clear();

            string path = this.md5Name;
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            //Debug.Log("本地md5地址：" + path);
            string line = "";
            if (!string.IsNullOrEmpty(content))
            {
                line = content;
            }
            else
            {
#if !UNITY_WEBPLAYER
                if (File.Exists(path))
                {
                    line = File.ReadAllText(path);
                }
#endif
            }
            if (line == null) 
                line = "";
            // 存储旧MD5列表
            string[] paths = line.Split('\n');
            List<System.String> copy = new List<System.String>();
            tempMD5.Clear();
            int totalNum = paths.Length;
            //Debug.Log("paths.Length:" + paths.Length + ", line:" + line);
            for (int i = 0; i < totalNum; i++)
            {
                string url = paths[i].TrimEnd();

                string[] info = url.Split(new string[]{FLAGSTR},StringSplitOptions.RemoveEmptyEntries);
                if (info.Length < 2) continue;
                MD5Config con = new MD5Config();
                con.md5Num = info[0];
                con.url = info[1].Trim();
                dicOldMD5Info[con.url] = con;
                copy.Add(url);
                tempMD5.Add(url);
            }
            Length = copy.Count;
            return copy;
            //Debug.Log("初始化本地md5>>>" + line);
        }
        public int Length = 0;
        public void Clear(bool delete = false)
        {
            if (delete)
            {
                foreach (MD5Config data in dicOldMD5Info.Values)
                {
                    if (data.IsOld)
                    {
                        if (File.Exists(dirPath + data.url))
                        {
                            File.Delete(dirPath + data.url);
                        }
                        tempMD5.Remove(data.GetContent(FLAGSTR));
                    }
                }
            }
            SaveMD5();
            dicOldMD5Info.Clear();
            tempMD5.Clear();
        }

        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                    return "";
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
    }

    public class MD5Config
    {
        public string md5Num { get; set; }
        public string url { get; set; }
        public bool IsOld = true;
        public string GetContent(string flag)
        {
            return this.md5Num + flag + this.url;
        }
    }
}
