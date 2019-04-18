using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;

public enum eChannel
{
    YJ = 0,//易接
    UC_ALI_9YOU = 1,//9游
    XM = 2,//小米
    CYY = 3,//
    IAP = 4,//苹果
    I31YOU = 5,//31you
    Galaxy = 6,//银河
    AOAIOS = 7,
    IAPP = 101,     //爱贝
    Google = 102,//谷歌
    Onestore = 103,// 韩国Onestore

    SuperSDK = 998,//
    BanShu = 999,//版署
    AndroidTest = 1000,//安卓测试
}
public enum eLoginSDK
{
    None = 0,
    Google = 1,
    FaceBook,
    QQ,
    WeiXin,
    GameCenter,
    GooglePlay,
}
public enum ePublish
{
    Debug = 0,//程序开发测试包
    BETA = 1,//发布前的测试包，最后同步复制到正式版
    RELEASE = 2,
}

//////////////////////////////////////////////语言类型///////////////////////////////////
/*
auto	自动检测
zh	中文
en	英语
yue	粤语
wyw	文言文
jp	日语
kor	韩语
fra	法语
spa	西班牙语
th	泰语
ara	阿拉伯语
ru	俄语
pt	葡萄牙语
de	德语
it	意大利语
el	希腊语
nl	荷兰语
pl	波兰语
bul	保加利亚语
est	爱沙尼亚语
dan	丹麦语
fin	芬兰语
cs	捷克语
rom	罗马尼亚语
slo	斯洛文尼亚语
swe	瑞典语
hu	匈牙利语
cht	繁体中文
vie	越南语

*/
////////////////////////////////////////////////////////////
public enum eLanguage
{
    auto=0,
    zh=1,//中文简体
    en=2,//英语
    cht=3,//中文繁体
    kor = 4,//韩语
    jp=5,//日语
    th=6, // 泰语
    vie=7,// 越南    
    fra=8,//法语
    spa = 9,//西班牙语
    ru=10,//俄语
    de=11,//德语
    nl = 12,//荷兰语
    pt = 13,//葡萄牙语

    my = 14,//马来西亚语
    tr = 15,//土耳其语
    ua = 16,//乌克兰语
    ph = 17,//菲律宾语
    id = 18,//印尼语
    //ara,
    //it,
    //pl,     
    //yue,
    //cs,
    //fin,
    //el,
    //rom,
    //swe,
    //slo,
    //hu,
    //bul,
    //est,    
}
public enum ePlatform
{
    ANDROID = 0,
    IOS = 1,
}

public enum eGetType
{
    None = -1,
    file_14_10 = 0,
    rzcdz2 = 1,
}
public class HotFixData
{
    public int value;
    public string mPath;
    public HotFixData(string path,int v=0)
    {
        value = v;
        mPath = path;
        if (File.Exists(path))
        {
            try
            {
                string str = File.ReadAllText(path);// Util.GetFileText(persistentVersionPath);
                Int32.TryParse(str, out this.value);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
    public void Save(int v)
    {
        value = v;
        StreamWriter sw = new StreamWriter(mPath, false);
        sw.Write(v);
        sw.Flush();
        sw.Close();
        sw.Dispose();
    }
    public void Delete()
    {
        if (File.Exists(mPath))
            File.Delete(mPath);
    }
}
public class VersionData
{
    public static string minVersion
    {
        get
        {
            return VersionStyle.FrameVersion + ".0";
        }
    }

    public string mPath;
    public string Version;
    //public string Version { get { return config.Version; } }
    //public VersionConfig config = null;
    public string NextVersion = "";
    public string LastVersion = "";
    public int index;//下一个ResVersion值
    public VersionData(string path)
    {
        mPath = path;
        if (File.Exists(path))
        {
            try
            {
                Version = File.ReadAllText(path);
                //string str = File.ReadAllText(path);
                //config = JsonUtility.FromJson<VersionConfig>(str);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }
        if (string.IsNullOrEmpty(Version))
            Version = minVersion;
        SetVersion(Version);
        //if (config == null)
        //{
        //    config = new VersionConfig();
        //}
    }
    public void SetVersion(string v)
    {
        Version = v;
        string[] Values = Version.Split('.');//Array.ConvertAll(, new Converter<String, int>(delegate(string s) { int v = 0; Int32.TryParse(s, out v); return v; }));
        index = Int32.Parse(Values[Values.Length - 1]);//Values.Length == 0 ? 0 : Values[Values.Length - 1];
        if (index > 0)
            LastVersion = VersionStyle.FrameVersion + "." + (index - 1).ToString();
        else
            LastVersion = "";
        index++;
        NextVersion = VersionStyle.FrameVersion + "." + index.ToString();
    }
    public bool Check(string v)
    {
        string[] vs = v.Split('.');
        int idx = Int32.Parse(vs[vs.Length - 1]);
        return idx < index;
    }
    public void Save(string ver)
    {
        string str = ver;
        StreamWriter sw = new StreamWriter(mPath, false);

        //config.Version = ver;
        //config.Channel = ch;
        //config.Publish = pub;
        //config.Platform = pla;
        //config.Time = System.DateTime.Now.Ticks.ToString();
        //switch (pub)
        //{
        //    case ePublish.ALPHA:
        //        config.HotUpdateUrl = "http://o6ssva6nz.bkt.clouddn.com/";
        //        break;
        //    case ePublish.BETA:
        //        config.HotUpdateUrl = "http://o6ssel25n.bkt.clouddn.com/";
        //        break;
        //    case ePublish.RELEASE:
        //        config.HotUpdateUrl = "http://o6ss33fbj.bkt.clouddn.com/";
        //        break;
        //}
        //string logPath = mPath.Replace("version.txt", "Info.txt");
        //if (File.Exists(logPath))
        //    config.Log = File.ReadAllText(logPath);
        //str = JsonUtility.ToJson(config);

        sw.Write(str);
        sw.Close();
        this.SetVersion(ver);
    }
}
public class PatchResData
{
    public static char Split = ',';
    public string version;
    public FileInfo file;
    public string sourceName;
    public string curName;
   // public string dir;
    public string name;
    public int size;
    public string toName { get { return name.Replace(Split, '/'); } }
    public string resName { get { return VersionStyle.PatchTag + "_" + version + Split + name; } }
    public PatchResData()
    { }
    public PatchResData(string info)
    {
        info = info.Trim();
        int sIdx = info.IndexOf(Split);
        int eIdx = info.LastIndexOf(Split);
        if (sIdx < 0 || eIdx < 0)
        {
            Debug.LogError("infos.Length < 3: " + info);
            return;
        }
        version = info.Substring(0, sIdx);
        name = info.Substring(sIdx+1, eIdx - sIdx - 1);
        string sizeStr = info.Substring(eIdx + 1);
        int.TryParse(sizeStr, out size);
    }
    public override string ToString()
    {
        if (string.IsNullOrEmpty(version))
            return "";
        return version + "," + name + "," + size;
    }
    public static void Save(List<PatchResData> list, string path)
    {
        long size = 0;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            string info = list[i].ToString();
            if (!string.IsNullOrEmpty(info) && list[i].size > 0)
            {
                sb.AppendLine(info);
                size += list[i].size;
            }
        }
        sb.Append(size.ToString());
        highlight.FileUtils.WriteTxt(path, sb);
    }
    public static void SaveFileList(string publishPath, string v,int idx)
    {
        string curPatchDir = publishPath + v + "/";
        string patchName = VersionStyle.PatchTag + "_" + VersionStyle.FrameVersion + "." + 0 + "_" + v + ".txt";
        string patchPath = curPatchDir + patchName;
        long size = 0;
        List<PatchResData> list = CreatPatchListByUrl(patchPath, out size);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < idx; i++)
        {
            string patchListFileUrl = VersionStyle.PatchTag + "_" + VersionStyle.FrameVersion + "." + i + "_" + v + ".txt";
            if (File.Exists(curPatchDir + patchListFileUrl))
                sb.AppendLine(QQCloudMgr.HotFixUrl + patchListFileUrl);
        }
        for(int i=0;i<list.Count;i++)
        {
            sb.AppendLine(QQCloudMgr.HotFixUrl + list[i].resName);
        }
        string info = sb.ToString();
        highlight.FileUtils.WriteTxt(publishPath + v + ".txt", info);
        Debug.Log(info);
    }
    public static List<PatchResData> CreatPatchListByUrl(string url, out long size)
    {
        string txt = File.ReadAllText(url);
        List<PatchResData> list = PatchResData.CreatPatchList(txt, out size);
        return list;
    }
    
    public static List<PatchResData> CreatPatchList(string txt,out long size)
    {
        List<PatchResData> list = new List<PatchResData>();
        txt = txt.TrimEnd();
        string[] txts = txt.Split('\n');
        for (int i = 0; i < txts.Length-1; i++)
        {
            PatchResData data = new PatchResData(txts[i]);
            list.Add(data);
        }
        size = 0;
        if (txts.Length > 0)
            long.TryParse(txts[txts.Length - 1], out size);
        return list;
    }
}
public class VIndex
{
    public int big = 1;
    public int small = 0;
    public int patch = 0;
    public VIndex(int i, int j, int k)
    {
        big = i;
        small = j;
        patch = k;
    }
    public string Value
    {
        get { return big + "." + small + "." + patch; }
    }
    public VIndex Next
    {
        get
        {
            return new VIndex(big, small, patch + 1);
        }
    }
    public VIndex Last
    {
        get
        {
            return new VIndex(big, small, patch - 1);
        }
    }
    public VIndex Clone()
    {
        return new VIndex(big,small,patch);
    }
}

/*
public class VSData
{
    public int Major = 1;
    public int Minor = 0;
    public int Patch = 0;
    public int Build = 0;
    public void SetVersion(string vs)
    {
        string[] vss = vs.Split('.');
        if (vss.Length >= 3)
        {
            Int32.TryParse(vss[0], out Major);
            Int32.TryParse(vss[1], out Minor);
            Int32.TryParse(vss[2], out Patch);
        }
        if(vss.Length == 4)
        {
            Int32.TryParse(vss[4], out Build);
        }
    }
    public string ToString()
    {
        return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Patch, Build);
    }
    public string Version
    {
        get
        {
            return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
        }
    }
}*/
