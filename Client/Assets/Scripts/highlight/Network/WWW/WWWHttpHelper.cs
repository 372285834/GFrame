using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public class WWWHttpHelper : MonoBehaviour
{
    public delegate void WWWCallback(string data, bool isError);
    static List<WWWHttpData> mUpdateList = new List<WWWHttpData>();
	private static GameObject container;

	public static WWWHttpHelper Instance;
    void Awake()
    {
        Instance = this;
    }

	/// <summary>
	/// Tos the get.
	/// </summary>
	/// <param name="aUrl">A URL.</param>
	/// <param name="aCallback">A callback.</param>
    public static WWWHttpData ToGet(string aUrl, Action<WWWHttpData> aCallback, float timeOut=0f)
	{
        WWWHttpData data = new WWWHttpData("",aUrl);
        data.IsJson = false;
        data.aCallback = aCallback;
        AddData(aUrl, data);
        if (timeOut > 0f)
            data.TimeOut = timeOut;
        return data;
	}
    public static string logUrl = "";
    public static bool IsLog = false;
    public static void Log(string info)
    {
        if (!IsLog)
            return;
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict.Add("log", info);
    }
    //public static WWWHttpData ToPostLua(string url, XLua.LuaTable lua, XLua.LuaFunction aCallback,string def="")//LuaInterface.LuaTable
    //{
    //    string json = "";//MUtil.ToJsonLua(lua);
    //    WWWHttpData data = new WWWHttpData(json, url);
    //    data.luaCallback = aCallback;
    //    def = string.IsNullOrEmpty(def) ? "DefPay" : def;
    //    data.defUrl = VersionManager.Instance.mStyle.GetJsonInfo(def);
    //    AddData(url, data);
    //    return data;
    //}
    public static WWWHttpData ToPost(string url, string json, Action<WWWHttpData> aCallback)
    {
        WWWHttpData data = new WWWHttpData(json,url);
        data.IsJson = true;
        data.aCallback = aCallback;
        AddData(url, data);
        return data;
    }
    public static WWWHttpData ToPostByte(string url, string json, Action<WWWHttpData> aCallback)
    {
        WWWHttpData data = new WWWHttpData(json,url);
        data.IsJson = false;
        data.aCallback = aCallback;
        AddData(url, data);
        return data;
    }
    public static void AddData(string aUrl, WWWHttpData data)
    {
        mUpdateList.Add(data);
    }

	/// <summary>
	/// GET the specified aUrl and aCallback.
	/// </summary>
	/// <param name="aUrl">A URL.</param>
	/// <param name="aCallback">A callback.</param>
    IEnumerator GET(string aUrl, WWWCallback aCallback)
	{	
		WWW getData = new WWW(aUrl);
			
		yield return getData;
			
		if(getData.error!= null)
		{  				
			Debug.Log(getData.error);
            aCallback(getData.error, true);
		} 
		else
        {
            Debug.Log(getData.text);
            aCallback(getData.text, false);
		}         
	}
    public static void SendRequest(RequestInfo eInfo)
    {
        requestList.Add(eInfo);
    }
    public static List<RequestInfo> requestList = new List<RequestInfo>();

    public static List<MResTools.mFileInfo> fileInfoList = new List<MResTools.mFileInfo>();
    public static void SendDepressFile(MResTools.mFileInfo eInfo)
    {
        fileInfoList.Add(eInfo);
    }
    void Update()
    {
        if (mUpdateList.Count > 0)
        {
            for (int i = 0; i < mUpdateList.Count; i++)
            {
                WWWHttpData data = mUpdateList[i];
                data.Update();
                if (data.IsDispose)
                {
                    mUpdateList.RemoveAt(i);
                    i--;
                }
            }
        }
        if(requestList.Count > 0)
        {
            for (int i = 0; i < requestList.Count; i++)
            {
                RequestInfo info = requestList[i];
                info.Update(Time.deltaTime);
                if (info.isEnd)
                {
                    requestList.RemoveAt(i);
                    i--;
                }
            }
        }
        if (fileInfoList.Count > 0)
        {
            for (int i = 0; i < fileInfoList.Count; i++)
            {
                MResTools.mFileInfo info = fileInfoList[i];
                if (info.thread.isEnd)
                {
                    info.CallBack();
                    fileInfoList.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    /// <summary> 
    /// URLEncode 
    /// </summary> 
    /// <returns>encode value</returns> 
    /// <param name="value">要encode的值</param> 
    public static string UrlEncode(string value)
    {
        StringBuilder sb = new StringBuilder();
        byte[] byStr = System.Text.Encoding.UTF8.GetBytes(value);
        for (int i = 0; i < byStr.Length; i++)
        {
            sb.Append(@"%" + Convert.ToString(byStr[i], 16));
        }
        return (sb.ToString());
    }
}
