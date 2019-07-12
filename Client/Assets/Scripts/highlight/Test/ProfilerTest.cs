using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
//using XLua;

public static class ProfilerTest
{
    static private GameObject profilerGameObject;
    public static float sampleTime = 5f;
    public static float waitTime = 0f;
    public static bool supported { get { return Profiler.supported; } }
    public static bool isSample { get { return profilerGameObject != null && profilerGameObject.GetComponent<InternalBehaviour>() != null; } }
    static public void BeginRecord(float t = 5f)
    {
        sampleTime = t;
        if (profilerGameObject == null)
        {
            profilerGameObject = new GameObject("#Profiler#");
            UnityEngine.Object.DontDestroyOnLoad(profilerGameObject);
        }
        if (!isSample)
        {
            profilerGameObject.AddComponent<InternalBehaviour>();
        }
    }

    class InternalBehaviour : MonoBehaviour
    {
        private string m_DebugInfo = String.Empty;

        private void OnGUI()
        {
            GUILayout.Label(String.Format("<size=50>{0}</size>", m_DebugInfo));
        }

        IEnumerator Start()
        {
            if (waitTime > 0f)
            {
                m_DebugInfo = string.Format("<color=blue>{0}s后开始保存Profiler日志</color>", waitTime);
                yield return new WaitForSeconds(waitTime);
            }
            string file = Application.persistentDataPath + "/profiler_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".log";
            Profiler.logFile = file;
            Profiler.enabled = true;
            Profiler.enableBinaryLog = true;
            m_DebugInfo = string.Format("<color=red>{0}s后结束保存Profiler日志</color>", sampleTime);
            yield return new WaitForSeconds(sampleTime);

            Profiler.enableBinaryLog = false;
            m_DebugInfo = string.Format("保存完毕:{0}", file);
            yield return new WaitForSeconds(10);
            Destroy(this);
        }
    }



    public static bool testStarUpTime = false;
    static float startCpuTime = 0f;
    static int startCpuFrame = 0;
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Test/luaCpu/start", false, 1)]
#endif
    public static void debugProfilerLuaStart()
    {
        CallMethod("debugProfilerLua", true);
        startCpuTime = Time.realtimeSinceStartup;
        startCpuFrame = Time.frameCount;
    }
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Test/luaCpu/stop", false, 1)]
#endif
    public static void debugProfilerLuaStop()
    {
        CallMethod("debugProfilerLua", false);
        Dictionary<string, MDebuger.LuaDebugInfo> dic = MDebuger.debugLuaDic;
        if (dic.Count == 0)
            return;
        StringBuilder sb = new StringBuilder();
        float totalTime = 0;
        List<MDebuger.LuaDebugInfo> list = new List<MDebuger.LuaDebugInfo>();
        foreach (var k in dic.Keys)
        {
            totalTime += dic[k].time;
            list.Add(dic[k]);
        }
        list.Sort(delegate (MDebuger.LuaDebugInfo a, MDebuger.LuaDebugInfo b)
        {
            if (a.time == b.time)
                return 0;
            return a.time > b.time ? -1 : 1;
        });
        float cupTotalTime = Time.realtimeSinceStartup - startCpuTime;
        float fpsPer = (Time.frameCount - startCpuFrame) / cupTotalTime;
        string title = "类名,函数名：行数,次数,s次数,耗时,耗时ms/次,占总百分比," + cupTotalTime.ToString("#.##") + "s,FPS:" + fpsPer.ToString("#.##") + "," + totalTime.ToString("#.##") + "s";
        sb.AppendLine(title);
        int num = 0;
        foreach (var k in list)
        {
            num++;
            MDebuger.LuaDebugInfo dinfo = k;
            float perTime = (dinfo.time * 1000f / dinfo.num);
            if (dinfo.time <= 0)
                perTime = 0f;
            string key = num.ToString();
            int idx = dinfo.name.LastIndexOf(",");
            int idx2 = dinfo.name.IndexOf("\r");
            int idx3 = idx2 > 9 ? 9 : 0;
            if (idx2 > 0)
                key += dinfo.name.Substring(idx3, idx2 - idx3);
            if (idx > 0)
                key += dinfo.name.Substring(idx);
            if (string.IsNullOrEmpty(key))
                key = dinfo.name;
            key = key.Replace("\r", "");
            sb.AppendLine(key + "," + dinfo.num + "," + dinfo.startNum + "," + dinfo.time + "," + perTime + "," + (dinfo.time * 100 / cupTotalTime) + "%");
        }
        //sb.Insert(0, ",,,总帧数：" + Time.frameCount + " totalNum:" + totalNum + " per:" + ((float)totalNum / Time.frameCount).ToString("#.##") + "\n");
        MFileUtils.WriteTxt(Application.persistentDataPath + "/LuaProfiler_" + System.DateTime.Now.ToString("MM-dd-HH-mm-ss") + ".csv", sb);
        dic.Clear();
    }
    static object[] CallMethod(string str, params object[] args)
    {
       // if (!Application.isPlaying)
            return null;
        //return KSFramework.LuaModule.CallFunction(str, args);
    }
    public static void BeginSample(string str)
    {
        // UWAEngine.PushSample(str);
        UnityEngine.Profiling.Profiler.BeginSample(str);
    }
    public static void EndSample()
    {
        // UWAEngine.PopSample();
        UnityEngine.Profiling.Profiler.EndSample();
    }
}
