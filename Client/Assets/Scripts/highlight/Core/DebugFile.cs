using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DebugFile
{
    public static DebugFile UI = new DebugFile("debugUI.txt", KeyCode.LeftControl,true);
    public static DebugFile socket = new DebugFile("debugSocket.txt",KeyCode.None,true);
    bool active = false;
    KeyCode code;
    public string url;
    public bool isWrite = false;
    public DebugFile(string _url,KeyCode k = KeyCode.None,bool _isWrite = false)
    {
        url = Application.persistentDataPath + "/" + _url;
        active = File.Exists(url);
        code = k;
        isWrite = _isWrite;
    }
    public static System.IO.StreamWriter debugFile;

    public void Log(string str)
    {
        if (!IsActive)
            return;
        Debug.Log(str);
        if(isWrite)
        {
            debugFile.Write(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff") + ":" + str + "\n");
            debugFile.Flush();

        }
        
    }
    public void LogError(string str)
    {
        if (!IsActive)
            return;
        Debug.LogError(str);
        if (isWrite)
        {
            debugFile.Write("[Error]" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff") + ":" + str + "\n");
            debugFile.Flush();
        }
    }
    public void Log(string key, object obj)
    {
        if (!IsActive)
            return;
        string str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        Log(key + " " + str);
    }
    public void LogError(string key, object obj)
    {
        if (!IsActive)
            return;
        string str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        LogError(key + " " + str);
    }
    
    public bool IsActive
    {
        get
        {
            if (!active)
                return false;
            if (code != KeyCode.None && !Input.GetKey(code))
            {
                return false;
            }
            if (debugFile == null)
                debugFile = new System.IO.StreamWriter(url, false, new System.Text.UTF8Encoding(false));
            return true;
        }
    }

}
