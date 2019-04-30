using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net;

namespace highlight
{
    public static class Util
    {

#if UNITY_EDITOR
       // [LuaInterface.NoToLua]
        public static void AddUpdate(UnityEditor.EditorApplication.CallbackFunction ac)
        {
            UnityEditor.EditorApplication.update -= ac;
            UnityEditor.EditorApplication.update += ac;
        }
     //   [LuaInterface.NoToLua]
        public static void RemoveUpdate(UnityEditor.EditorApplication.CallbackFunction ac)
        {
            UnityEditor.EditorApplication.update -= ac;

        }
#endif
        public static int Int(object o)
        {
            return Convert.ToInt32(o);
        }

        public static float Float(object o)
        {
            return (float)Math.Round(Convert.ToSingle(o), 2);
        }

        public static long Long(object o)
        {
            return Convert.ToInt64(o);
        }

        public static int Random(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public static float Random(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public static string Uid(string uid)
        {
            int position = uid.LastIndexOf('_');
            return uid.Remove(0, position + 1);
        }

        /// <summary>
        /// 格式化字符串
        /// </summary>
        /// <returns></returns>
        public static string f(string format, params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            return sb.AppendFormat(format, args).ToString();
        }

        /// <summary>
        /// 字符串连接
        /// </summary>
        public static string c(params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                sb.Append(args[i].ToString());
            }
            return sb.ToString();
        }

        public static int GetTime()
        {            
            TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            return (int)ts.TotalSeconds;
        }
        public static DateTime GetTimeEnd(float second)
        {
            return System.DateTime.Now + new System.TimeSpan((long)(second * 10000000));
        }
        /// <summary>
        /// 手机震动
        /// </summary>
        public static void Vibrate()
        {
            //int canVibrate = PlayerPrefs.GetInt(Const.AppPrefix + "Vibrate", 1);
            //if (canVibrate == 1) iPhoneUtils.Vibrate();
        }

        /// <summary>
        /// Base64编码
        /// </summary>
        public static string Encode(string message)
        {
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(message);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        public static string Decode(string message)
        {
            byte[] bytes = Convert.FromBase64String(message);
            return Encoding.GetEncoding("utf-8").GetString(bytes);
        }

        /// <summary>
        /// 判断数字
        /// </summary>
        public static bool IsNumeric(string str)
        {
            if (str == null || str.Length == 0)
                return false;

            for (int i = 0; i < str.Length; i++)
            {
                if (!Char.IsNumber(str[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// HashToMD5Hex
        /// </summary>
        public static string HashToMD5Hex(string sourceStr)
        {
            byte[] Bytes = Encoding.UTF8.GetBytes(sourceStr);
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] result = md5.ComputeHash(Bytes);
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < result.Length; i++)
                    builder.Append(result[i].ToString("x2"));
                return builder.ToString();
            }
        }

        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        public static string md5(string source)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            string destString = "";
            for (int i = 0; i < md5Data.Length; i++)
            {
                destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }
            destString = destString.PadLeft(32, '0');
            return destString;
        }

        /// <summary>
        /// 清理内存
        /// </summary>
        public static void ClearMemory()
        {
            Resources.UnloadUnusedAssets();
            //GC.Collect();
        }

        /// <summary>
        /// 是否为数字
        /// </summary>
        public static bool IsNumber(string strNumber)
        {
            Regex regex = new Regex("[^0-9]");
            return !regex.IsMatch(strNumber);
        }

        /// <summary>
        /// 取得App包里面的读取目录
        /// </summary>
        public static Uri AppContentDataUri
        {
            get
            {
                string dataPath = Application.dataPath;
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    var uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = "file";
                    uriBuilder.Path = Path.Combine(dataPath, "Raw");
                    return uriBuilder.Uri;
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    return new Uri("jar:file://" + dataPath + "!/assets");
                }
                else
                {
                    var uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = "file";
                    uriBuilder.Path = Path.Combine(dataPath, "StreamingAssets");
                    return uriBuilder.Uri;
                }
            }
        }
        

        /// <summary>
        /// 网络可用
        /// </summary>
        public static bool NetAvailable
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }

        /// <summary>
        /// 是否是无线
        /// </summary>
        public static bool IsWifi
        {
            get
            {
                return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
            }
        }

        public static string LuaPath(string name)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                return Application.dataPath + "/lua/" + name;
            }
            string str = Application.persistentDataPath + "/" + Util.PlatformDir + "/";
            if(Directory.Exists(str))
            {
                return str + name;
            }
            return Application.streamingAssetsPath + "/" + Util.PlatformDir + "/" + name;
            
        }
        /// <summary>
        /// 应用程序内容路径
        /// </summary>
        /// 
        //不同平台下StreamingAssets的路径是不同的，这里需要注意一下。  
        public static string AppContentPath
        {
            get
            {
                //return Application.streamingAssetsPath;
                string path = Application.dataPath;
                if(Application.platform == RuntimePlatform.Android)
                {
                    path = "jar:file://" + Application.dataPath + "!/assets";
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    path = "file://" + Application.dataPath + "/Raw";
                }
                //else if (Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer)
                //{
                //    path = "Web";
                //}
                else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXEditor)
                {
                    path = "file://" + dataPathParent;//
                }
                return path;
            }
        }
        public static string dataPathParent
        {
            get
            {
                return Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")) + "/StreamingAssets";
            }
        }
        public static string PersistentDataPath
        {
            get
            {
                string url = Application.persistentDataPath;
                if (Application.platform == RuntimePlatform.Android)
                {
                    url = "file://" + Application.persistentDataPath;
                }
				else if (Application.platform == RuntimePlatform.IPhonePlayer ||Application.platform == RuntimePlatform.OSXPlayer||Application.platform == RuntimePlatform.OSXEditor)
                {
                    url = "file://" + Application.persistentDataPath;
                }
                else if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    url = "file://" + Application.persistentDataPath;
                }
                return url;

            }
        }
        public static string PlatformDir
        {
            get
            {
#if UNITY_ANDROID
                return "Android";
#elif UNITY_IPHONE || UNITY_IOS
            return "iOS";
#elif UNITY_WEBPLAYER
            return "WebPlayer";
#elif UNITY_STANDALONE_WIN
            return "Windows";
#elif UNITY_STANDALONE_OSX
            return "iOS";
#else
            return iOS;
#endif
            }
        }
        /// <summary>
        /// 取得行文本
        /// </summary>
        public static string GetFileText(string path)
        {
            if (!File.Exists(path))
                return "";
            return File.ReadAllText(path);
        }

        public static string LoadText(string url,out string error)
        {
            WWW www = new WWW(url);
            while (!www.isDone) { }
            error = www.error;
            if (www.error != null)
            {
                Debug.LogError("LoadText:" + url + ", error:" + www.error);
                return "";
            }
            return www.text.Trim();
        }

        /// <summary>
        /// HttpWebRequest 通过get
        /// </summary>
        /// <param name="url">URI</param>
        /// <returns></returns>
        public static string GetDataGetHtml(string url)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url); 
                HttpWebResponse webRespon = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream webStream = webRespon.GetResponseStream();
                if (webStream == null)
                {
                    return "网络错误(Network error)：" + new ArgumentNullException("webStream");
                }
                StreamReader streamReader = new StreamReader(webStream, Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();

                webRespon.Close();
                streamReader.Close();

                return responseContent;
            }
            catch (Exception ex)
            {
                return "网络错误(Network error)：" + ex.Message;
            }
        }


        public static bool IsTestClient = false;
        public static bool IsWeb()
        {
            //return true;
            if (IsTestClient)
                return false;
            if (Application.platform == RuntimePlatform.WindowsEditor)
                return false;
            if (Application.platform == RuntimePlatform.WindowsPlayer)
                return true;
            return false;
        }
        public static bool IsEditor()
        {
            //return false;
            //if(Wuxia.Config.IsHotFix)
            //    return false;
            if (Application.platform == RuntimePlatform.WindowsEditor) return true;
            return false;
        }
        public static bool IsMiniClient()
        {
            //return true;
            if (IsTestClient)
                return false;
            if (Application.platform == RuntimePlatform.WindowsPlayer) return true;
            return false;
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        public static string MD5Encrypt(string strText)
        {
            if (null == strText) return "";
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string result = BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(strText)));
            result = result.Replace("-", "").ToLower();
            return result;
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        public static string parseTimeYMD(DateTime time)
        {
            return string.Format("{0}年{1}月{2}日", time.Year, time.Month, time.Day);
        }

        public static string parseTimeDHM(DateTime time)
        {
            return string.Format("{0}天{1}时{2}分", time.Day, time.Hour, time.Minute);
        }

        public static string timeSpan(DateTime time1, DateTime time2)
        {
            TimeSpan ts = time2.Subtract(time1);
            return string.Format("{0}天{1}时{2}分", ts.Days, ts.Hours, ts.Minutes);
        }
        public static string TimeToString(DateTime dt)
        {
            TimeSpan ts = System.DateTime.Now - dt;
            return ts.TimeSpanToString();
        }
        public static string parseTimeDHMBySecond(int second)
        {
            string timeStr = "";
            TimeSpan ts = new TimeSpan(0, 0, second);
            timeStr = string.Format("{0} : {1} : {2}", GetTwoInt((int)ts.TotalHours), GetTwoInt(ts.Minutes), GetTwoInt(ts.Seconds));
            return timeStr;
        }
        public static string TimeSpanToString(this TimeSpan ts)
        {
            if ((int)ts.TotalHours > 0)
                return string.Format("{0} : {1} : {2}", GetTwoInt((int)ts.TotalHours), GetTwoInt(ts.Minutes), GetTwoInt(ts.Seconds));
            return string.Format("    {0} : {1}", GetTwoInt(ts.Minutes), GetTwoInt(ts.Seconds));
        }

        public static bool TimeCompareMin(int second,int min)
        {
            int minSec = min*60;
            return second >= minSec ? true : false;

        }
        public static string GetTwoInt(int num)
        {
            string str = "";
            if (num < 10)
            {
                str = "0" + num.ToString();
            }
            else
                str = num.ToString();

            return str;
        }
        public static bool RegexName(string str)
        {
            bool flag = Regex.IsMatch(str, @"^[A-Za-z0-9\u4e00-\u9fa5]+$");
            return flag;
        }
        
    }
}