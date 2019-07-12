using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
public class MDebuger : MonoBehaviour 
{
    public static MDebuger GetMDebuger(string name)
    {
        GameObject go = new GameObject(name);
        return go.AddComp<MDebuger>();
    }
    public static void ShowFps(bool b)
    {
        DrawFps.SetShow(b);
    }
#if UNITY_EDITOR
    public bool ShowDebug = true;
#else
    public bool ShowDebug = false;
#endif
    public static bool IsTestHttp = false;
    public void Log(string msg, params object[] args)
    {        
        if (!ShowDebug)
            return;
#if UNITY_EDITOR
             Debug.LogFormat(this.name + msg, args);
                //if (Application.isPlaying && GlobalGenerator.Intance.IsDebug)
                //Debug.LogFormat(msg, args);
#endif
    }
    public void logToFile(string sMsg) {
        //Debug.Log(sMsg);
        //打包报错临时注掉
        //string sProtoCsDir = Application.dataPath+"/luadofile.log";
        //FileStream fConfig = new FileStream(sProtoCsDir, FileMode.OpenOrCreate);
        //StreamWriter sw = new StreamWriter(fConfig, Encoding.ASCII);
        //sw.WriteLine(sMsg);
        
        //sw.Flush();
        //sw.Close();
    }
    public void LogWarning(string info, params object[] args)
    {
        if (!ShowDebug)
            return;
#if UNITY_EDITOR
        //if (Application.isPlaying && GlobalGenerator.Intance.IsDebug)
        Debug.LogWarningFormat(this.name + info, args);
#endif
    }
    public void LogError(string info, params object[] args)
    {
        //Debug.LogErrorFormat(info, args);        
        if (!ShowDebug)
            return;
        //if (Application.isPlaying && GlobalGenerator.Intance.IsDebug)
        Debug.LogErrorFormat(this.name + info, args);        
    }


    public void TestLogError(string str)
    {
        Debug.LogError(this.name + str);
    }
    public void TestLog(string str)
    {
        Debug.Log(this.name + str);
    }
    public class LuaDebugInfo
    {
        public string name;
        public int num=0;
        public int startNum = 0;
        public float time=0f;
        public float curTime=0f;
    }
    public static Dictionary<string, LuaDebugInfo> debugLuaDic = new Dictionary<string, LuaDebugInfo>();
    static float debugTime = 0f;
    public static void debugLuaBegin(string str)
    {
        LuaDebugInfo info = null;
        debugLuaDic.TryGetValue(str, out info);
        if(info == null)
        {
            info = new LuaDebugInfo();
            info.name = str;
            debugLuaDic[str] = info;
        }
        info.curTime = Time.realtimeSinceStartup;
        info.startNum++;
        //debugLuaDic[str] = 
        //debugTime = Time.realtimeSinceStartup;
        UnityEngine.Profiling.Profiler.BeginSample(str);
    }
    public static void debugLuaEnd(string str)
    {
        //  float add = Time.realtimeSinceStartup - debugTime;
        //  if (add < 0.01f)
        //      return;
        LuaDebugInfo info = null;
        debugLuaDic.TryGetValue(str, out info);
        if (info != null && info.curTime > 0f)
        {
            info.time += Time.realtimeSinceStartup - info.curTime;
            info.curTime = 0f;
            info.num++;
        }
        else
        {
            //Debug.Log(str);
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }
    ///// <summary>
    ///// 初始化日志
    ///// </summary>
    ///// <param name="info"></param>
    //public static void InitLog(string info)
    //{
    //    Debug.Log(info + Time.realtimeSinceStartup);
    //    if (MResourcesUpdate.Instance != null)
    //    {
    //        MResourcesUpdate.Instance.InitLog(info);
    //    }
    //}
}