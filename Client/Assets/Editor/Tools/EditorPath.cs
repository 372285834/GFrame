using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEditor;
using highlight;
public static class EditorPath
{
    public static BuildAssetBundleOptions options
    {
        get
        {
            BuildAssetBundleOptions op = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle;
#if UNITY_ANDROID
            op = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle;
#elif (UNITY_IOS || UNITY_IPHONE)
            op = BuildAssetBundleOptions.ForceRebuildAssetBundle;
#elif UNITY_WEBPLAYER

#elif UNITY_STANDALONE_WIN

#elif UNITY_STANDALONE_OSX

#else

#endif
            return op;
        }
        //BuildAssetBundleOptions.ForceRebuildAssetBundle | 
    }

    public static string SDKDir
    {
        get { return Application.dataPath + "/../../Other/BuildPackage"; }/// 获取SDK目录
    }
    public static string MainScenePath = Application.dataPath + "/Game.unity";
    public static string ApkPath = Application.dataPath + "/../../Other/Publish/";
    public static string HotFixPath = Application.dataPath + "/../../../msgame_assetbundle/Publish/";
    public static string ServerLuaPath = Application.dataPath + "/../../Other/lua-server/work/";
    public static string StreamingAssetsPath = Application.dataPath + "/../Product/" + PlatformDir + "/";
    public static string androidSDKPath = Application.dataPath + "/Plugins/Android/";

    static string md5Dir = "Assets/BuildPrefabs/md5Dir/";
    static string md5TempDir = "Assets/Temp/md5Dir/";
    public static CompareMD5 UIMd5 = new CompareMD5(md5Dir, PlatformDir + "_UIMd5.txt");
    public static CompareMD5 ModelMd5 = new CompareMD5(md5Dir, PlatformDir + "_ModelMd5.txt");
    public static CompareMD5 EffectMd5 = new CompareMD5(md5TempDir, PlatformDir + "_EffectMd5.txt");
    public static CompareMD5 AtlasMd5 = new CompareMD5(md5Dir, PlatformDir + "_AtlasMd5.txt");
    public static CompareMD5 AudioMd5 = new CompareMD5(md5Dir, PlatformDir + "_AudioMd5.txt");
    public static CompareMD5 TextureMd5 = new CompareMD5(md5Dir, PlatformDir + "_TextureMd5.txt");

    public static string TempDir = "Assets/Temp/";
    public static string PrefabTempDir = "Assets/ArtRes/Temp/";
    public static string UITempDir = "Assets/Temp/UI";
    public static string AltlsTempDir = "Assets/Temp/Atlas";
    public static string ModelTempDir = "Assets/Temp/Model";
    public static string SceneTempDir = "Assets/Temp/Scene";
    public static string EffectsTempDir = "Assets/Temp/Effects";
    public static string LuaTempDir = "Assets/Temp/Lua";
    public static string MatTempDir = "Assets/Temp/Material";
    public static string MeshTempDir = "Assets/Temp/Mesh";
    public static string LuaDir = Application.dataPath + "/../Product/Lua";
    public static string pbTempDir = "Assets/Temp/pb/";

    public static string atlasDir = "Assets/ArtRes/Atlas/";
    public static string atlasPackDir = "Assets/ArtRes/Atlas/AtlasPack/";
    public static string DBDir = "Assets/ArtRes/DB/";
    public static string AudioDir = "Assets/ArtRes/Audio/";
    public static string MovieDir = "Assets/ArtRes/Movie/";

    public static string prefabDir = "Assets/BuildPrefabs";
    public static string uiPrefabsDir = "Assets/BuildPrefabs/UI/";
    public static string ModelDir = "Assets/BuildPrefabs/Model/";
    public static string SceneDir = "Assets/BuildPrefabs/Scene/";
    public static string AnimationDir = "Assets/BuildPrefabs/Animation/";
    public static string EffectsDir = "Assets/BuildPrefabs/Effects/";
    public static string TextureDir = "Assets/BuildPrefabs/Texture/";
    public static string MaterialDir = "Assets/DependResources/";//"Assets/BuildPrefabs/Material/";
    public static string stylesDir = "Assets/ArtRes/mapData/";

    public static string FontDir = "Assets/ArtRes/XPrefab/Fonts.prefab";
    public static string LoadingSceneDir = "Assets/ArtRes/XPrefab/LoadingScene.prefab";
    public static string ShaderDir = "Assets/ArtRes/XPrefab/shaderab.prefab";
    public static string MapDataStyleDir = "Assets/BundleEditing/Scene/shamo/MapDataStyle.asset";
    // public static string AudioPrefabDir = "Assets/BuildPrefabs/AudioController.prefab";
    public static string uiRootDir = "Assets/BuildPrefabs/UI/UIRoot.prefab";

    public static string MapsModelDir = "Assets/BuildPrefabs/Model/Maps/";

    public static string languageDir = Application.dataPath + "/ArtRes/txts/";
    public static string languageABDir = StreamingAssetsPath + "Language/";
    public static string GetTargetDir(string dirName)
    {
        string artResPath = Application.dataPath + "/../StreamingAssets/" + EditorPath.PlatformDir + "/" + dirName;
        if (!System.IO.Directory.Exists(artResPath))
            System.IO.Directory.CreateDirectory(artResPath);
        return artResPath;
    }
    public static string AssetsPath(this FileInfo file)
    {
        //string t= file.FullName.Substring(file.FullName.IndexOf("Assets")).Replace("\\", "/");
        string fullName = file.FullName.Replace("\\", "/");
        string t = fullName.Replace(Application.dataPath, "Assets");
        return t;
    }

    public static string GetBuildTarget(BuildTarget platform)
    {
        string str = "Windows";
        switch (platform)
        {
            case BuildTarget.Android:
                str = "Android";
                break;
            case BuildTarget.StandaloneWindows:
                str = "Windows";
                break;
            //case BuildTarget.WebPlayer:
            //    str = "Windows";
            //    break;
            case BuildTarget.iOS:
                str = "IOS";
                break;
            default:
                str = "Windows";
                break;
        }
        return str;
    }

    public static string PlatformDir
    {
        get
        {
            return Util.PlatformDir;
        }
    }
    public static VersionStyle GetVersionStyle()
    {
        return VersionStyle.Instance;
    }
    static MapDataStyle m_mapDataStyle = null;
    public static MapDataStyle mapDataStyle
    {
        get
        {
            if (m_mapDataStyle == null)
            {
                m_mapDataStyle = AssetDatabase.LoadAssetAtPath<MapDataStyle>(MapDataStyleDir);
            }
            return m_mapDataStyle;
        }
    }
    public static BuildTarget GetBuildTarget(int platform)
    {
        BuildTarget buildTarget = new BuildTarget();
        if (platform == 0)
        {
            buildTarget = BuildTarget.iOS;
        }
        else if (platform == 1)
        {
            buildTarget = BuildTarget.Android;
        }
        else if (platform == 2)
        {
            buildTarget = BuildTarget.StandaloneWindows;
        }
        //else if (platform == 3)
        //{
        //    buildTarget = BuildTarget.WebPlayer;
        //}
        else
        {
            buildTarget = BuildTarget.StandaloneOSXIntel;
        }
        return buildTarget;
    }
    public static string GetVersionTime()
    {
        string timeStr = System.DateTime.Now.ToString("MM-dd-HH-mm");
        timeStr = timeStr.Replace("-", "");
        return timeStr;
        //System.DateTime now = System.DateTime.Now;
        //string year = now.Year.ToString();
        //string month = now.Month.ToString();
        //string day = now.Day.ToString();
        //string hour = now.Hour.ToString();
        //string min = now.Minute.ToString();
        //string sec = now.Second.ToString();
        //return year + month + day + hour + min;
    }
    public delegate string AcCopyFile(string name);
    /// <summary>
    /// 路径以 / 结尾
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="delete"></param>
    public static void CopyFile(string from, string to, bool delete = true, string[] removeArr = null, AcCopyFile ac = null)
    {
        if (delete && Directory.Exists(to))
            Directory.Delete(to, true);
        if (Directory.Exists(from))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(from);
            FileInfo[] fls = dirInfo.GetFiles("*", SearchOption.AllDirectories);
            int index = 0;
            foreach (FileInfo file in fls)
            {
                index++;
                if (EditorPath.CheckFileExtensionInvalid(file))
                    continue;
                string url = file.FullName.Replace(dirInfo.FullName, "").Replace("\\","/");
                string outFile = "";
                if (ac != null)
                    outFile = ac(url);
                else
                    outFile = to + url;
                if (string.IsNullOrEmpty(outFile))
                    continue;
                string dir = Path.GetDirectoryName(outFile);
                EditorUtility.DisplayProgressBar("复制到" + outFile, "正在复制..." + file.Name + index + "/" + fls.Length, (float)index / fls.Length);
                if(removeArr != null)
                {
                    if (Array.Exists<string>(removeArr, item => item == url))
                        continue;
                }
                //判断目录是否存在;
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                File.Copy(file.FullName, outFile, true);
            }
            EditorUtility.ClearProgressBar();
        }
    }
    static void matchName(string name)
    {

    }
    public static void CopyFilesOrDir(string from, string to, bool delete = true)
    {
        FileInfo fil = new FileInfo(from);
        if (fil.Attributes == FileAttributes.Directory)
        {
            CopyFile(from, to, delete);
        }
        else
            File.Copy(from, to, true);
    }

    public static DirectoryInfo ClearDir(string url)
    {
        if (Directory.Exists(url))
            Directory.Delete(url,true);
        Directory.CreateDirectory(url);
        DirectoryInfo dirInfo = new DirectoryInfo(url);
        return dirInfo;
    }
    public static string GetAnimationClipUrl(AnimationClip clip)
    {
        string clipPath = AssetDatabase.GetAssetPath(clip).ToLower();
        string guid = AssetDatabase.AssetPathToGUID(clipPath);
        return guid;
        //clipPath = clipPath.Split('.')[0].Substring(7);
        //clipPath = clipPath.Replace('/', '-');
        //clipPath += "-" + clip.name;
        //return clipPath;
    }
    public static bool CheckFileExtensionInvalid(FileInfo file)
    {
        if (file.Extension == ".temp")
            return true;
        if (file.Extension == ".meta")
            return true;
        return false;
    }
    public static bool IsDeleteFileDir(FileInfo fi)
    {
        if (EditorPath.CheckFileExtensionInvalid(fi))
            return true;
        return IsDeleteFileDir(fi.Directory.Name);
    }
    public static bool IsDeleteFileDir(string path)
    {
        return path.EndsWith("Delete");
    }
    public static string GetAnimatorDirName(string path)
    {
        string url = Path.GetDirectoryName(path);
        string dir = url.Substring(url.LastIndexOf("/")+1);
        return dir.ToLower();
    }
}


public class MetaUserData
{
    [System.Serializable]
    public class Md5Data
    {
        public BuildTarget target;
        public string md5;
        public string abmd5;
        public Md5Data(BuildTarget _target, string _md5, string _abmd5)
        {
            target = _target;
            md5 = _md5;
            abmd5 = _abmd5;
        }
    }
    public string name;
    public string path;
    
    public List<Md5Data> md5List = new List<Md5Data>();

    public void Set(BuildTarget _target, string _md5, string _abmd5)
    {
        for (int i = 0; i < md5List.Count; i++)
        {
            if (md5List[i].target == _target)
            {
                md5List[i].md5 = _md5;
                md5List[i].abmd5 = _abmd5;
                return;
            }
        }
        Md5Data data = new Md5Data(_target,_md5, _abmd5);
        md5List.Add(data);
    }
    public bool Check(BuildTarget _target, string _md5, string _abmd5)
    {
        if (string.IsNullOrEmpty(_abmd5))
            return false;
        for (int i = 0; i < md5List.Count; i++)
        {
            if (md5List[i].target == _target)
            {
                return md5List[i].md5 == _md5 && md5List[i].abmd5 == _abmd5;
            }
        }
        return false;
    }
    public string ToString()
    {
        return JsonUtility.ToJson(this);
    }
    public static MetaUserData FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new MetaUserData();
        MetaUserData data = null;
        try
        {
            data = JsonUtility.FromJson<MetaUserData>(json);
        }
        catch(Exception e)
        {
            Debug.LogError("error:"+json);
        }
        return data == null ? new MetaUserData() : data;
    }
    public static MetaUserData CheckAssetsMeta(string path, string dirName, bool force)
    {
        BuildTarget target = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
        string fName = Path.GetFileNameWithoutExtension(path).ToLower();
        if (path.EndsWith(".controller"))
            fName = EditorPath.GetAnimatorDirName(path);
        string to = Application.dataPath + "/../StreamingAssets/" + EditorPath.GetBuildTarget(target) + "/" + dirName + "/" + fName;
        string md5 = CompareMD5.GetMD5HashFromFile(path);
        string abmd5 = CompareMD5.GetMD5HashFromFile(to);
        AssetImporter import = AssetImporter.GetAtPath(path);
        MetaUserData uData = FromJson(import.userData);
        if (uData.Check(target, md5, abmd5))
        {
            if (force)
                return uData;
            Debug.LogError("md5无变化：" + path);
            return null;
        }
        uData.name = fName;
        uData.path = path;
        return uData;
    }
    public static void SaveAssetsMeta(Dictionary<string, MetaUserData> userDataDic, string to, List<AssetBundleBuild> list)
    {
        if (userDataDic == null || userDataDic.Count == 0)
            return;
        BuildTarget target = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
        for (int i = 0; i < list.Count; i++)
        {
            AssetBundleBuild abb = list[i];
            EditorUtility.DisplayProgressBar("保存md5", abb.assetBundleName + ":" + i + " /" + list.Count, (float)i / (float)list.Count);
            if (abb.assetNames.Length != 1)
                continue;
            string fName = abb.assetBundleName.ToLower();
            MetaUserData uData = userDataDic[fName];
            string path = uData.path;
            string md5 = CompareMD5.GetMD5HashFromFile(path);
            string abmd5 = CompareMD5.GetMD5HashFromFile(to + fName);
            uData.Set(target, md5, abmd5);
            AssetImporter import = AssetImporter.GetAtPath(path);
            import.userData = uData.ToString();
            import.SaveAndReimport();
        }
        EditorUtility.ClearProgressBar();
    }
}
