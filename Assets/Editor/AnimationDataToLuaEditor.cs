using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AnimationDataToLuaEditor : EditorWindow
{
    public static string AnimationPath = Application.dataPath +"/BundleResources/Animation/";
    public static string LuaConfigPath = EditorUserSettings.GetConfigValue(ANIMATION_TO_LUA_PATH_KEY) + "luaconfig/animation/";
    static float processIndx = 1;
    static float allIndex = 1;
    static string bartitle;
    static string context;

    const string ANIMATION_TO_LUA_PATH_KEY = "ANIMATION_TO_LUA__PATH_KEY";

    static void SetAnimationToLuaPath()
    {
        string path = EditorUserSettings.GetConfigValue(ANIMATION_TO_LUA_PATH_KEY);
        path = EditorUtility.OpenFolderPanel(
                    "设置Lua表导出到文件目录",
                    path,
                    "");

        if (path != "")
        {
            if (!IsConTargetFile(path))
            {
                Debug.LogError("目录校验失败,请指定策划配置工程根目录!");
                return;
            }
            Debug.Log("目录校验成功！");
            path = path + "/";
            EditorUserSettings.SetConfigValue(ANIMATION_TO_LUA_PATH_KEY, path);
        }
    }

    static bool IsConTargetFile(string path)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        if (dir.Exists)
        {
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo f in files)
            {
                if (f.Name == "README.md")
                {
                    return true;
                }
            }
        }
        return false;
    }


    [PreferenceItem("Lua")]
    private static void SelfPreferenceItem()
    {
        string animationToLuaPath = EditorUserSettings.GetConfigValue(ANIMATION_TO_LUA_PATH_KEY);
        EditorGUILayout.LabelField("AnimationToLuaTable表导出到目录:");
        GUILayout.Label(animationToLuaPath);
        if (GUILayout.Button("浏览"))
        {
            SetAnimationToLuaPath();
        }
    }

    public static List<FileInfo> GetAnimationFileInfo()
    {
        DirectoryInfo rootDir = new DirectoryInfo(AnimationPath);
        List<string> filePahts = new List<string>();
        List<FileInfo> files = RecurGetFinInfos(rootDir);
        return files;
    }

    static List<FileInfo> RecurGetFinInfos(DirectoryInfo dirInfo)
    {
        List<FileInfo> files = new List<FileInfo>();
        FileInfo[] filesarr = dirInfo.GetFiles();
        List<FileInfo> infos = new List<FileInfo>(filesarr);
        infos.ForEach((info) => 
        {
            if(info.Extension == ".anim")
            {
                files.Add(info);
            }
        });
        DirectoryInfo[] childDirs = dirInfo.GetDirectories();
        foreach (DirectoryInfo d in childDirs)
        {
            files.AddRange(RecurGetFinInfos(d));
        }
        return files;
    }

    public static List<PathData> GetPathData()
    {
        List<PathData> pathsData = new List<PathData>();
        List<FileInfo> infos = GetAnimationFileInfo();
        foreach (FileInfo f in infos)
        {
            string path = f.DirectoryName.Replace("\\", "/");
            string luaConfigPath = path.Replace(AnimationPath, "");
            luaConfigPath = luaConfigPath.ToLower();
            luaConfigPath = LuaConfigPath + luaConfigPath;
            PathData data = pathsData.Find(x => x.luaConfigPath == luaConfigPath);
            if(data == null)
            {
                PathData nData = new PathData();
                nData.fileDir = path;
                nData.luaConfigPath = luaConfigPath;
                nData.animationFileInfos.Add(f);
                nData.ctr = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(nData.GetDefaultAnimatorPath);
                pathsData.Add(nData);
            }
            else
            {
                data.animationFileInfos.Add(f);
            }
        }
        return pathsData;
    }

    public static string GetLuaTableStringBuff(List<FileInfo> infos,UnityEditor.Animations.AnimatorController ctr)
    {
        Dictionary<object, object> dirFile = new Dictionary<object, object>();
        UnityEditor.Animations.ChildAnimatorState[] states = null;
        if (ctr != null)
             states = ctr.layers[0].stateMachine.states;
        foreach (FileInfo info in infos)
        {
            processIndx++;
            OnGUI();
            string fileName = info.Name.Replace(info.Extension, "");
            float speed = 1;
            if (states != null)
            {
                for (int i = 0; i < states.Length; i++)
                {
                    if (states[i].state.name == fileName)
                    {
                        speed = states[i].state.speed;
                        break;
                    }
                }
            }
            string tpath = info.FullName.Replace("\\", "/");
            string path = tpath.Replace(Application.dataPath, "Assets");
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            Dictionary<object, object> clipInfo = new Dictionary<object, object>();
            clipInfo.Add("length", clip.length);
            clipInfo.Add("isLooping", clip.isLooping);
            clipInfo.Add("speed", speed);
            dirFile.Add(fileName, clipInfo);
        }
        return TileEditor.HELua.Encode(dirFile);
    }


    [MenuItem("Tools/AnimationToLuaTable")]
    public static void AnimationToLuaTable()
    {
        LuaConfigPath = EditorUserSettings.GetConfigValue(ANIMATION_TO_LUA_PATH_KEY) + "luaconfig/animation/";
        string p = EditorUserSettings.GetConfigValue(ANIMATION_TO_LUA_PATH_KEY);
        if (p == null)
        {
            Debug.LogError("请设置AnimationToLua导出目录:Unity菜单->Edit->Preferences->Lua->AnimationToLuaTable表导出到目录");
            return;
        }
        EditorUtility.DisplayProgressBar(bartitle, context, processIndx / allIndex);
        bartitle = "AnimationToLuatable";
        context = "正在执行...";
        processIndx = 0;
        List<PathData> datas = GetPathData();
        datas.ForEach(((d) => allIndex += d.animationFileInfos.Count));
        DirectoryInfo baseDir = new DirectoryInfo(LuaConfigPath);
        if(baseDir.Exists)
        {
            baseDir.Delete(true);
        }
        foreach (PathData d in datas)
        {
            string luafileName = d.luaConfigPath + ".lua";
            DirectoryInfo dir = new DirectoryInfo(d.GetDir);
            if (!dir.Exists)
            {
                dir.Create();
            }
            FileInfo f = new FileInfo(luafileName);
            FileStream fs = f.Create();
            fs.SetLength(0);
            UTF8Encoding utf8 = new UTF8Encoding(false);
            StreamWriter ws = new StreamWriter(fs,utf8);
            string str = GetLuaTableStringBuff(d.animationFileInfos,d.ctr);
            ws.Write(str);
            ws.Close();
            fs.Close();
        }
        processIndx = allIndex = 1;
        OnGUI();
    }

    [MenuItem("Tools/GitPush AnimaitonLuaTab")]
    public static void PushLuaTableToRemote()
    {
        string msPath = EditorUserSettings.GetConfigValue(ANIMATION_TO_LUA_PATH_KEY);
        if(msPath == null)
        {
            Debug.LogError("请设置AnimationToLua导出目录:Unity菜单->Edit->Preferences->Lua->AnimationToLuaTable表导出到目录");
            return;
        }
#if UNITY_EDITOR_WIN
        string path = EditorUserSettings.GetConfigValue(ANIMATION_TO_LUA_PATH_KEY) + "shell/";
        string cmmandLin1 = "cd " + path + "\n";
        string commandLin2 = path + "AnimationtoLuaTabPull.sh";
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "powershell";
        psi.UseShellExecute = false;
        psi.RedirectStandardInput = true;
        psi.RedirectStandardError = true;
        psi.RedirectStandardOutput = true;
        psi.Arguments = cmmandLin1 + commandLin2;
        Process p = System.Diagnostics.Process.Start(psi);
        p.Close();
#else
         Debug.LogError("目前只支持windows版本");
#endif
    }

    [MenuItem("Tools/ExcelLuaToProject")]
    public static void ExcelLuaToProject()
    {
        string msPath = EditorUserSettings.GetConfigValue(ANIMATION_TO_LUA_PATH_KEY);
        if (msPath == null)
        {
            Debug.LogError("请设置AnimationToLua导出目录:Unity菜单->Edit->Preferences->Lua->AnimationToLuaTable表导出到目录");
            return;
        }

#if UNITY_EDITOR_WIN
        string path = EditorUserSettings.GetConfigValue(ANIMATION_TO_LUA_PATH_KEY);
        ProcessStartInfo psi = new ProcessStartInfo();
        string command = GetDiskSymbol(path) + " & cd " + path + " & ExcelToLua.bat & Exit";
        psi.FileName = "cmd";
        //psi.Arguments = command;
        psi.UseShellExecute = false;
        psi.RedirectStandardInput = true;
        psi.RedirectStandardError = true;
        psi.RedirectStandardOutput = true;
        psi.CreateNoWindow = true;
        Process p = System.Diagnostics.Process.Start(psi);
        p.StandardInput.WriteLine(command);
        p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        p.Close();
        string luaConfigPath = GetLuaProjectPath() + "/Product/Lua/config/";
        string com1 = "rm -force " + luaConfigPath + "*.lua";
        string com2 = "cp -force -recurse " + path + @"luaconfig/excel/* " + luaConfigPath;
        ProcessStartInfo psi2 = new ProcessStartInfo();
        psi2.FileName = "powershell";
        psi2.UseShellExecute = false;
        psi2.RedirectStandardInput = true;
        psi2.RedirectStandardError = true;
        psi2.RedirectStandardOutput = true;
        psi2.Arguments = com1;
        psi2.CreateNoWindow = true;
        Process p2 = System.Diagnostics.Process.Start(psi2);
        p2.Close();

        ProcessStartInfo psi3 = new ProcessStartInfo();
        psi3.FileName = "powershell";
        psi3.UseShellExecute = false;
        psi3.RedirectStandardInput = true;
        psi3.RedirectStandardError = true;
        psi3.RedirectStandardOutput = true;
        psi3.CreateNoWindow = true;
        psi3.Arguments = com2;
        Process p3 = System.Diagnostics.Process.Start(psi3);
        p3.Close();

#else
         Debug.LogError("目前只支持windows版本");
#endif
    }

    public class PathData
    {
        public string fileDir;
        public string luaConfigPath;
        public string GetLastFileName
        {
            get
            {
                int startIndex = luaConfigPath.LastIndexOf("/");
                string s = luaConfigPath.Substring(startIndex + 1, luaConfigPath.Length - startIndex - 1);
                return s;
            }
        }

        public string GetDir
        {
            get
            {
                int startIndex = luaConfigPath.LastIndexOf("/");
                string s = luaConfigPath.Substring(0, startIndex);
                return s;
            }
        }

        public string GetDefaultAnimatorPath
        {
            get
            {
                string p = fileDir + "/default.controller";
                return p.Replace(Application.dataPath, "Assets");
            }
        }

        public UnityEditor.Animations.AnimatorController ctr;
        public List<FileInfo> animationFileInfos = new List<FileInfo>(); 
    }

    static void OnGUI()
    {
        if(processIndx < allIndex)
        {
            EditorUtility.DisplayProgressBar(bartitle, context, processIndx/allIndex);
        }
        else
        {
            EditorUtility.ClearProgressBar();
        }
    }

    static string GetLuaProjectPath()
    {
        string p = Application.dataPath;
        int startIndex = p.LastIndexOf("/");
        string s = p.Substring(0, startIndex);
        return s;
    }

    static string GetDiskSymbol(string path)
    {
        int startIndex = path.IndexOf("/");
        string s = path.Substring(0, startIndex);
        return s;
    }
}
