using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using highlight;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public class GenVersion : EditorWindow
{
    public static string GenVersionHelpInfo = "一，注释A（1.2.3）,B（3）为上传七牛工具。\n" +
        "1.检测当前版本md5与[当前assetbundle资源]做对比，如果有新资源则继续，否则无更新。\n" +
        "2.获取所有版本md5，分别与[当前assetbundle资源]做对比，复制差异文件并压缩成7z包。\n" +
        "3.将所有生成的7z包和version.txt文件上传七牛。\n\n" +
        "二，使用步骤：\n" +
        "1.发行商对应不同的apk。\n" +
        "2.发布类型：测试时选择BETA选项，测试完毕后发布正式版选择RELEASE选项\n\n" +
        "三，路径注释：\n" +
        "1.[当前assetbundle资源]目录:Application.dataPath/../SteamingAssets/。\n" +
        "2.Publish目录:Application.dataPath + /../../Other/Publish/。\n" +
        "3.version.txt:" + "Publish/Android/version.txt。\n" +
        "4.md5目录:" + "Publish/Android/md5/。\n" +
        "5.上传资源目录:" + "Publish/Android/1.x.x/。\n";
    public static GenVersion window;

    [InitializeOnLoadMethod]
    static void Start()
    {
        EditorApplication.update += Update;
    }
    static void Update()
    {
        if (QQCloudMgr.uploadOk)
        {
            QQCloudMgr.curNum = 0;
            QQCloudMgr.AllNum = 0;
            QQCloudMgr.uploadOk = false;
            mData = new VersionData(publishPath + "version.txt");
            //window.Close();
            EditorUtility.ClearProgressBar();
            Debug.Log("上传完毕OK~~~~~");
            //EditorUtility.DisplayDialog("上传完毕", "上传完毕", "Ok");
        }
        else if (QQCloudMgr.curNum > 0 && QQCloudMgr.AllNum > 0)
        {
            EditorUtility.DisplayProgressBar("上传七牛", "正在上传..." + QQCloudMgr.curNum + "/" + QQCloudMgr.AllNum, (float)QQCloudMgr.curNum / QQCloudMgr.AllNum);
        }
    }
    [MenuItem("GameTools/CloseUpLoading")]
    static void CloseUpLoading()
    {
    //    QQCloudMgr.isUpLoading = false;
    }
    [MenuItem("GameTools/发新版本")]
        static void Execute()
        {
            //vData = new VersionData(EditorPath.PublishPath + "version.txt");
            if (window == null)
            {
                window = (GenVersion)GetWindow(typeof(GenVersion));
                inputVersion = VersionData.minVersion;
                window.Show();
            }
        }
        public static void Save()
        {
            mData = new VersionData(publishPath + "version.txt");
        }
        public static ePublish Publish
        {
            get { return EditorPath.GetVersionStyle().Publish; }
            set {
                EditorPath.GetVersionStyle().Publish = value;
                EditorUtility.SetDirty(EditorPath.GetVersionStyle());
            }
        }
        public static string publishPath { 
            get 
            {
                string pb = EditorPath.PlatformDir + "_" + Publish.ToString() + "_" + VersionStyle.FrameVersion;
                //if (Publish == ePublish.RELEASE)
                //    pb = ePublish.BETA.ToString();
                return EditorPath.HotFixPath + pb + "/";
            } 
        }
        static string inputVersion;
        void OnGUI()
        {
            if (GUI.Button(new Rect(120, 10, 100, 30), "刷新显示") || mData == null)
            {
                Save();
            }
            //EditorPrefs.SetString();
            GUILayout.BeginVertical();
            GUILayout.Label("当前游戏版本：" + vData.Version);
            GUILayout.Label("发布新版本：" + vData.NextVersion);
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            //eChannel ch = (eChannel)EditorGUILayout.EnumPopup("                     发行商:", Channel, GUILayout.MaxWidth(250));//, 
            //if (ch != Channel) { Channel = ch; Save(); }
            ePublish pub = (ePublish)EditorGUILayout.EnumPopup(Publish, GUILayout.MaxWidth(100));//, 
            if (pub != Publish) { Publish = pub; Save(); }
            GUILayout.EndVertical(); 

            
            GUILayout.Space(10f);
            
            if (GUI.Button(new Rect(100, 100, 400, 40), "A.发布新版本生成差量压缩包并上传七牛"))
            {
                //if (Publish == ePublish.RELEASE)
               // {
               //     uploadQiNiu();
               //     return;
                //}
                BuildHotFixByMd5();
            }
            if (GUI.Button(new Rect(100, 400, 200, 40), "B.重新上传老资源"))
            {
                vData.Save(vData.Version);
                uploadQiNiu(Publish);
            }
            if (GUI.Button(new Rect(100, 450, 200, 40), "C.生成上传文件列表"))
            {
                PatchResData.SaveFileList(publishPath, vData.Version, vData.index);
            }
            //if (GUI.Button(new Rect(100, 150, 400, 40), "C.发布自定义设置包"))
            //{
            //    BuildAPK.BuildAndroidPlayer(BuildAPK.GetVersionStyle());
            //    //window.Close();
            //    EditorUtility.DisplayDialog("发布完毕", "发布完毕", "Ok");
            //}
            //if (GUI.Button(new Rect(100, 200, 400, 40), "D.发布正式包--Beta版"))
            //{
            //    BuildAPK.SetVersionStyle(vData.Version, Channel, Publish);
            //    //BuildAPK.BuildAndroidPlayer();
            //    BuildAPK.BuildPlatformYj();
            //    //window.Close();
            //    EditorUtility.DisplayDialog("发布完毕", "发布完毕", "Ok");
            //}
            //if (GUI.Button(new Rect(100, 250, 400, 40), "E.发布正式包--Release版"))
            //{
            //    BuildAPK.SetVersionStyle(vData.Version, Channel, ePublish.RELEASE);
            //    //BuildAPK.BuildAndroidPlayer();
            //    BuildAPK.BuildPlatformYj();
            //    //window.Close();
            //    EditorUtility.DisplayDialog("发布完毕", "发布完毕", "Ok");
            //}

            if (GUI.Button(new Rect(510, 100, 80, 40), "打开上传目录"))
            {
                if (!Directory.Exists(publishPath))
                    Directory.CreateDirectory(publishPath);
                System.Diagnostics.Process.Start(publishPath);
            }
            //if (GUI.Button(new Rect(510, 150, 80, 40), "打开apk目录"))
            //{
            //    System.Diagnostics.Process.Start(Application.dataPath + "/../Publish/");
            //}
            if (GUI.Button(new Rect(300, 5, 100, 30), "发布当前版本"))
            {   
                if(!string.IsNullOrEmpty(vData.LastVersion))
                    vData.SetVersion(vData.LastVersion);
                BuildHotFixByMd5();
            }
            //inputVersion = GUI.TextField(new Rect(600, 10, 60, 20), inputVersion);
            if (GUI.Button(new Rect(500, 5, 150, 30), "清理版本回退到:" + VersionData.minVersion))
            {
                ClearHotFix(VersionData.minVersion);
            }
            //IsRelease = GUI.Toggle(new Rect(10, 250, 400, 20), IsRelease, "调试版");
            GUI.Label(new Rect(20, 150, 600, 200), GenVersionHelpInfo);
            //if (GUI.Button(new Rect(520, 350, 120, 40), "上传Beta到正式服"))
            //{
                
            //}
        }
        //static bool IsRelease = true;
    static VersionData mData = null;
    public static VersionData vData
    {
        get
        {
            if(mData == null)
                mData = new VersionData(publishPath + "version.txt");
            return mData;
        }
    }
    public static VersionData getVersionData(ePublish publish)
    {
        string pb = EditorPath.PlatformDir + "_" + publish.ToString() + "_" + VersionStyle.FrameVersion;
        string path = EditorPath.HotFixPath + pb + "/";
        VersionData v = new VersionData(path + "version.txt");
        return v;
    }
    public static void BackOneHotFix()
    {
        if (!string.IsNullOrEmpty(vData.LastVersion))
        {
            CompareMD5 old = new CompareMD5(publishPath + "md5/", vData.Version + "_files" + ".txt");
            if (File.Exists(old.md5Name))
            {
                File.Delete(old.md5Name);
            }
            vData.SetVersion(vData.LastVersion);
            Debug.Log("回退一个版本重新热更：" + vData.Version);
        }
        else
        {
            Debug.Log("没有可回退的版本：" + vData.Version);
        }
    }
    public static void ClearHotFix(string version)
    {
        if (Directory.Exists(publishPath))
            Directory.Delete(publishPath, true);
        if (!Directory.Exists(publishPath))
            Directory.CreateDirectory(publishPath);
        vData.SetVersion(version);
        BuildHotFixByMd5();
    }
    public static string splitABFlag { get{ return "Product" + Path.DirectorySeparatorChar + EditorPath.PlatformDir; } }  // Path.GetDirectoryName(EditorPath.StreamingAssetsPath);
    /// <summary>
    /// 分析差量文件
    /// </summary>
    public static void BuildHotFixByMd5()
    {
        if (!Directory.Exists(publishPath))
            Directory.CreateDirectory(publishPath);

        if (!Directory.Exists(publishPath + "md5/"))
            Directory.CreateDirectory(publishPath + "md5/");
        CompareMD5 old = new CompareMD5(publishPath + "md5/", vData.Version + "_files" + ".txt");
        bool isNoVersion = !File.Exists(old.md5Name);
        old.InitLocal();
        old.isSort = false;
        CompareMD5 curMd5 = new CompareMD5(publishPath + "md5/", vData.NextVersion + "_files" + ".txt");
        curMd5.InitLocal();
        curMd5.isSort = false;
        Debug.Log("热更新：" + vData.Version + "-" + vData.NextVersion + "   " + publishPath);
        if (!Directory.Exists(EditorPath.StreamingAssetsPath))
        {
            Directory.CreateDirectory(EditorPath.StreamingAssetsPath);
        }
        DirectoryInfo fromDir = new DirectoryInfo(EditorPath.StreamingAssetsPath);
        
        FileInfo[] fls = fromDir.GetFiles("*", SearchOption.AllDirectories);
        bool haveNew = false;
        int index = 0;
        List<FileInfo> diffList = new List<FileInfo>();
        foreach (FileInfo file in fls)
        {
            index++;
            if (file.Name == Util.PlatformDir || file.Name == ".DS_Store")
                continue;
            if (EditorPath.CheckFileExtensionInvalid(file))
                continue;
            if (file.Name.Contains(PatchResData.Split.ToString()))
            {
                Debug.LogError(PatchResData.Split + "禁止资源命名包含：" + file.Name);
                continue;
            }
            curMd5.Check(file, true, splitABFlag);
            if (!old.Check(file, true, splitABFlag))
            {
                diffList.Add(file);
                haveNew = true;
            }
            EditorUtility.DisplayProgressBar( "分析差量 " + fls.Length, index + " 正在分析差量..." + file.Name, (float)index / (float)fls.Length);
        }
        EditorUtility.ClearProgressBar();
        if (isNoVersion)
        {
            old.Clear();
            vData.Save(vData.Version);
            if(vData.index > 1)
                uploadQiNiu(Publish);
            Debug.Log("没有初始化版本，清理版本回退到:" + vData.Version + ",  num:" + diffList.Count);
        } 
        else if (haveNew)
        {//有可更新文件
            //if (backOneVersion && !string.IsNullOrEmpty(vData.LastVersion))
            //{
            //    vData.SetVersion(vData.LastVersion);
            //    old.SaveMD5(curMd5.getTempMD5List.ToArray());
            //}
            //else
                curMd5.Clear();
            string str = "";
            str = copyFile2(fls, diffList);
            //str = copyFile(to, fls);
            //gen7z(to);

            vData.Save(vData.NextVersion);
            uploadQiNiu(Publish);

            Debug.Log(UIUtil.GetRichText("完成版本号：" + vData.Version + "，生成数量：" + str, "00ff00"));
        }
        else
        {
            QQCloudMgr.uploadOk = true;
            Debug.Log("没有新资源-----------" + QQCloudMgr.uploadOk.ToString());
        }
        
    }
    List<string> uploadList = new List<string>();
    static string copyFile2(FileInfo[] fls, List<FileInfo> diffList)
    {
        string to = publishPath + vData.NextVersion + "/";
        if (Directory.Exists(to))
            Directory.Delete(to, true);
        DirectoryInfo dir = System.IO.Directory.CreateDirectory(to);
        to = dir.FullName.Replace("\\", "/");
        List<PatchResData> nextList = new List<PatchResData>();
        for (int i = 0; i < diffList.Count; i++)
        {
            FileInfo file = diffList[i];
            if(file.Name.Contains(PatchResData.Split.ToString()))
            {
                Debug.LogError(PatchResData.Split + "错误的文件名：" + file.Name);
                continue;
            }
            string url = GetFileCopyUrl(file.FullName);//file.FullName.Substring(file.FullName.LastIndexOf(EditorPath.PlatformDir) + EditorPath.PlatformDir.Length + 1).Replace("\\", "/");
            PatchResData data = new PatchResData();
            data.version = vData.NextVersion;
            data.name = url;
            data.size = (int)file.Length;
            string outFile = to + data.resName;
            try
            {
                File.Copy(file.FullName, outFile, true);
                nextList.Add(data);
            }
            catch(System.Exception e)
            {
                Debug.LogError(file.FullName + "\n" + outFile);
                throw new System.Exception("复制差异文件报错");
            }
            EditorUtility.DisplayProgressBar("复制差量:" + diffList.Count, "正在复制差量..." + i + " /" + (diffList.Count), (float)i / (float)(diffList.Count));
        }
        Dictionary<string, List<PatchResData>> listDic = new Dictionary<string, List<PatchResData>>(); 
        string nextPatchName = VersionStyle.PatchTag + "_" + vData.Version + "_" + vData.NextVersion;
        PatchResData.Save(nextList, to + nextPatchName + ".txt");
        listDic[nextPatchName] = nextList;
        int index = 0;
        string curPatchDir = publishPath + vData.Version + "/";
        if(vData.index > 1 && Directory.Exists(curPatchDir))
        {
            for(int i=0;i<vData.index;i++)
            {
                index++;
                string patchName = VersionStyle.PatchTag + "_" + VersionStyle.FrameVersion + "." + i + "_" + vData.Version;
                string patchPath = curPatchDir + patchName + ".txt";
                string toPatchName = VersionStyle.PatchTag + "_" + VersionStyle.FrameVersion + "." + i + "_" + vData.NextVersion;
                string toPatchPath = to + toPatchName + ".txt";
                EditorUtility.DisplayProgressBar("生成patchTxt:" + toPatchName, toPatchName + "..." + index + " /" + vData.index, (float)index / (float)vData.index);
                if(File.Exists(patchPath))
                {
                    string txt = File.ReadAllText(patchPath);
                    long lsize = 0;
                    List<PatchResData> list = PatchResData.CreatPatchList(txt, out lsize);
                    for (int j = 0; j < nextList.Count;j++ )
                    {
                        list.RemoveAll(x => x.name == nextList[j].name);
                        list.Add(nextList[j]);
                    }
                    listDic[toPatchName] = list;
                    PatchResData.Save(list, toPatchPath);
                }
            }
            
        }
        gen7z2(listDic, to + "package/");
        return diffList.Count.ToString();
    }
    public static void gen7z2(Dictionary<string, List<PatchResData>> listDic,string to)
    {
        DirectoryInfo dir = System.IO.Directory.CreateDirectory(publishPath);
        FileInfo[] fls = dir.GetFiles("*", SearchOption.AllDirectories);
        Dictionary<string, FileInfo> allFls = new Dictionary<string, FileInfo>();
        Dictionary<string, PatchResData> resDataDic = new Dictionary<string, PatchResData>();
        foreach (FileInfo file in fls)
        {
            if (file.Name == "version.txt")
                continue;
            if (file.Extension == ".7z")
                continue;
            allFls[file.Name] = file;
        }
        int curIndex = 0;
        foreach(var key in listDic.Keys)
        {
            List<PatchResData> list = listDic[key];
            string toDir = to + key;
            if (!Directory.Exists(toDir))
                Directory.CreateDirectory(toDir);
            for (int i = 0; i < list.Count;i++ )
            {
                string resName = list[i].resName;
                PatchResData data = null;
                if(!resDataDic.TryGetValue(resName,out data))
                {
                    data = list[i];
                    resDataDic[resName] = data;
                }
                if (data.file == null)
                    allFls.TryGetValue(resName, out data.file);
                if (data.file != null)
                {
                    if (string.IsNullOrEmpty(data.sourceName))
                        data.sourceName = data.file.FullName;
                    string sName = string.IsNullOrEmpty(data.curName) ? data.file.FullName : data.curName;
                    data.curName = toDir + "/" + resName;
                    File.Move(sName, data.curName);
                }
                else
                    Debug.LogError("不存在资源a：" + resName);
            }
            curIndex++;
            EditorUtility.DisplayProgressBar("生成压缩文件:" + toDir + " ,num:" + list.Count, toDir + curIndex + " /" + (listDic.Count), (float)curIndex / (float)(listDic.Count));
            MResTools.FileCompression(toDir, toDir + ".7z");

            FileInfo fi = new FileInfo(toDir + ".7z");
            string sizeInfo = fi.Length.ToString();
            StreamWriter sw = new StreamWriter(toDir + "_7z.txt", false);
            sw.Write(sizeInfo);
            sw.Close();
        }

        foreach (var key in resDataDic.Keys)
        {
            PatchResData data = resDataDic[key];
            if (!File.Exists(data.curName))
                Debug.LogError("不存在资源b:" + data.curName);
            else
                File.Move(data.curName, data.sourceName);
        }
        EditorUtility.ClearProgressBar();
        //list.Add(vData.mPath);
        //return list;
    }
    static string GetFileCopyUrl(string fullName)
    {
        string url = fullName.Substring(fullName.LastIndexOf(splitABFlag) + splitABFlag.Length + 1).Replace("\\", "/");
        return url.Replace('/', PatchResData.Split);
    }
    /// <summary>
    /// 上传七牛
    /// </summary>
    /// <param name="list"></param>
	public static void uploadQiNiu(ePublish ep,string path = "")
    {
        Publish = ep;
        if (QQCloudMgr.isUpLoading)
        {
            Debug.LogError("正在上传中");
            return;
        }
        path = string.IsNullOrEmpty(path) ? publishPath + vData.Version + "/package/" : path;
       // path = string.IsNullOrEmpty(path) ? publishPath + vData.Version + "/" : path;
        Debug.Log("资源目录：" + path);
        if (!Directory.Exists(path))
        {
            Debug.LogError("目录不存在：" + path);
            EditorUtility.ClearProgressBar();
            return;
        }
        MPrefs.SetString("_GenVersionToPath_", path);
        if (QQCloudMgr.IsAWS && !Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scripts/3rds/AWSS3/AWS.unity");
            UnityEditor.EditorApplication.isPlaying = true;
        }
        else
        {
            List<string> list = QQCloudMgr.uploadStartUp();
            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < list.Count; i++)
            //{
            //    sb.AppendLine(QQCloudMgr.HotFixUrl + list[i]);
            //}
            //FileUtils.WriteTxt(publishPath + vData.Version + ".txt", sb.ToString());
            //PatchResData.SaveFileList(publishPath, vData.Version, vData.index);
            EditorCoroutineRunner.StartEditorCoroutine(upLoadComplete());
        }
    }

    static IEnumerator upLoadComplete()
    {
        yield return null;
        while (QQCloudMgr.isUpLoading)
        {
            EditorUtility.DisplayProgressBar("上传七牛:" + QQCloudMgr.AllNum + "/" + QQCloudMgr.curNum, QQCloudMgr.curFl.Name, (float)QQCloudMgr.curNum / (float)QQCloudMgr.AllNum);
            yield return null;
        }
        //yield return null;
        try
        {
            EditorUtility.ClearProgressBar();
        }
		catch(System.Exception e)
        {

        }
        mData = new VersionData(publishPath + "version.txt");
    }
    /*
    /// <summary>
    /// 复制更新文件
    /// </summary>
    /// <param name="to"></param>
    /// <param name="fls"></param>
    /// <returns></returns>
    static string copyFile(string to, FileInfo[] fls)
    {
        if (Directory.Exists(to))
            Directory.Delete(to, true);
        int num = 0;
        md5Dic = GetAllMd5();
        int index = 0;
        foreach (var key in md5Dic.Keys)
        {
            CompareMD5 md5 = md5Dic[key];
            foreach (FileInfo file in fls)
            {
                index++;
                if (!md5.Check(file, false, EditorPath.PlatformDir))
                {
                    num++;
                    string url = file.FullName.Substring(file.FullName.LastIndexOf(EditorPath.PlatformDir) + EditorPath.PlatformDir.Length + 1).Replace("\\", "/");// + EditorPath.PlatformDir.Length
                    string outFile = to + "/" + VersionStyle.PatchTag + "_" + key + "_" + vData.NextVersion + "/" + url;
                    string dir = Path.GetDirectoryName(outFile);
                    //判断目录是否存在;
                    if (!System.IO.Directory.Exists(dir))
                        System.IO.Directory.CreateDirectory(dir);
                    File.Copy(file.FullName, outFile, true);
                    //CompressFileLZMA(file.FullName, serverDir.FullName + "/" + url);
                }
                EditorUtility.DisplayProgressBar("生成差量", "正在生成差量..." + key + "_" + vData.NextVersion + "..." + index + " /" + (fls.Length * md5Dic.Count), (float)index / (float)(fls.Length * md5Dic.Count));
            }
        }
        EditorUtility.ClearProgressBar();
        return num.ToString();
    }
    /// <summary>
    /// 生成7z压缩包
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static void gen7z(string path)
    {
        //List<string> list = new List<string>();
        if (Directory.Exists(path))
        {
            DirectoryInfo from = new DirectoryInfo(path);
            DirectoryInfo[] dirs = from.GetDirectories("*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < dirs.Length; i++)
            {
                string dirName = dirs[i].FullName;
                if (dirName.IndexOf(".DS")>-1)
                    continue;
                DirectoryInfo dir = new DirectoryInfo(dirName);// + EditorPath.PlatformDir.Length
                string to = from.FullName + dirs[i].Name + ".7z";
				MResTools.FileCompression(dir.FullName, to);
                dirs[i].Delete(true);
                FileInfo fi = new FileInfo(to);
                string sizeInfo = "size:" + fi.Length;
                StreamWriter sw = new StreamWriter(from.FullName + dirs[i].Name + ".txt", false);
                sw.Write(sizeInfo);
                sw.Close();
            }
        }
        //list.Add(vData.mPath);
        //return list;
    }
    */
    /*
    public static Dictionary<string, CompareMD5> GetAllMd5()
    {
        Dictionary<string, CompareMD5> dic = new Dictionary<string, CompareMD5>();
        string to = publishPath + "md5/";
        if (!Directory.Exists(to))
            Directory.CreateDirectory(to);

        DirectoryInfo dir = new DirectoryInfo(to);
        FileInfo[] fls = dir.GetFiles("*.txt", SearchOption.TopDirectoryOnly);
        foreach (FileInfo file in fls)
        {
            string key = file.Name.Split('_')[0];
            if (vData.Check(key))
            {
                CompareMD5 md5 = new CompareMD5(to, file.Name);
                md5.InitLocal();
                dic.Add(key, md5);
            }
        }
        return dic;
    }
     * */
}
