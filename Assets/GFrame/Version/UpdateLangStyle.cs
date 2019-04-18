using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace highlight
{
    public class UpdateLangStyle
    {
        public string 版本维护 { get { return GetValue("1"); } }
        //public string 版本维护info { get { return GetValue("2"); } }

        public string 版本更新 { get { return GetValue("3"); } }
        public string 版本更新info { get { return GetValue("4"); } }
        //public string wifi检测 = "\n检测到您当前WLAN还未打开或连接，是否继续下载？";

        public string 版本更新2 { get { return GetValue("5"); } }
        public string 版本更新2info { get { return GetValue("6"); } }
        public string 下载速度 { get { return GetValue("7"); } }

        public string 解压 { get { return GetValue("8"); } }
        public string 解压info { get { return GetValue("9"); } }

        public string 版本号下载异常 { get { return GetValue("10"); } }
        public string 版本号下载异常info { get { return GetValue("2"); } }

        public string 版本异常 { get { return GetValue("12"); } }
        public string 版本异常info { get { return GetValue("13"); } }

        public string 下载失败 { get { return GetValue("14"); } }
        public string 下载失败info { get { return GetValue("15"); } }

        public string 解压完毕 { get { return GetValue("16"); } }
        //public string 解压完毕info { get { return GetValue("17"); } }

        public string 解压失败 { get { return GetValue("18"); } }
        public string 解压失败info { get { return GetValue("19"); } }

        //public string 解压完毕 = "解压完毕";
        //public string 解压完毕info = "";

        public string 初始化 { get { return GetValue("20"); } }
        //public string 初始化检测版本 { get { return GetValue("21"); } }
        public string 初始化预加载 { get { return GetValue("22"); } }
        public string 初始化UI管理器 { get { return GetValue("23"); } }
        public string 初始化Lua { get { return GetValue("24"); } }
        public string 初始化数据 { get { return GetValue("25"); } }
        public string 初始化字体 { get { return GetValue("26"); } }
        //public string 初始化音乐 { get { return GetValue("27"); } }

        public string 进入游戏 { get { return GetValue("28"); } }
        public string 标题 { get { return GetValue("29"); } }
        public string 初始化网络 { get { return GetValue("30"); } }

        //public string 版本号 { get { return GetValue("31"); } }

        public string 提交问题 { get { return GetValue("32"); } }
        public string 进入 { get { return GetValue("33"); } }
        public string 退出 { get { return GetValue("34"); } }
        public string 确定 { get { return GetValue("35"); } }
        public string wifi { get { return GetValue("36"); } }
        public string 没有网络 { get { return GetValue("37"); } }
        public string 累计补偿 { get { return GetValue("38"); } }


        static Dictionary<string, string[]> dic = new Dictionary<string, string[]>();
        public static string infoPath = Application.dataPath + "/Resources/updatetxt.txt";
        public void Init()
        {
            //string error = "";
            //string allTxt = Util.LoadText(infoPath, out error);
            //string[] txts = allTxt.Split('\n');// System.IO.File.ReadAllLines(infoPath);
            string allInfo = "";
            string localUrl = Application.persistentDataPath + "/" + Util.PlatformDir + "/Data/updatetxt";
            if (File.Exists(localUrl))
            {
                allInfo = File.ReadAllText(localUrl);
            }
            if(string.IsNullOrEmpty(allInfo))
            {
                object obj = Resources.Load("updatetxt");
                allInfo = obj.ToString();
            }
            string[] txts = allInfo.Split('\n');
            for(int i=2;i<txts.Length;i++)
            {
                string info = txts[i].Trim();
                if (string.IsNullOrEmpty(info))
                    continue;
                string[] vs = info.Split('\t');
                if(!string.IsNullOrEmpty(vs[0]))
                    dic[vs[0]] = vs;
            }
        }
        public static int LangKey = 1;
        public static string GetValue(string k)
        {
            string[] lines = null;
            string value = "";
            if (dic.TryGetValue(k, out lines))
            {
                if (lines.Length > LangKey)
                    value = lines[LangKey];
                if (string.IsNullOrEmpty(value) && lines.Length > 2)
                    value = lines[2];
            }
            else
                value = k;
            return value;
        }
    }
}