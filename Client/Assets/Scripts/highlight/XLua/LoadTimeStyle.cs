using highlight.tl;
using KEngine;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LoadTimeStyle
{
    public static string timlineDir = "Timeline";
    public static string editor_timeline_dir = "Assets/BundleEditing/Timeline/";
    public static Dictionary<string, TimelineStyle> styleDic = new Dictionary<string, TimelineStyle>();
    public static TimelineStyle Load(string name)
    {
        if (styleDic.ContainsKey(name))
            return styleDic[name];
        string json = LoadJson(name);
        if(string.IsNullOrEmpty(json))
        {
            Debug.LogError("找不到资源:" + name);
            return null;
        }
        TimelineStyle ps = JsonConvert.DeserializeObject(json, typeof(TimelineStyle), getSetting()) as TimelineStyle;
        styleDic[name] = ps;
        return ps;
    }
    public static JsonSerializerSettings getSetting()
    {
        JsonSerializerSettings setting = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
        // setting.Formatting = Formatting.None;
        return setting;
    }
    public static string LoadJson(string name)
    {
        string json = "";
        if (Application.isEditor)
        {
            string url = editor_timeline_dir + name + ".tl";
            if (!File.Exists(url))
            {
                Debug.LogError("找不到资源:" + url);
                return "";
            }
            json = File.ReadAllText(url);
        }
        else
        {
            var relativePath = string.Format("{0}/{1}.tl", timlineDir, name);
            relativePath = KResourceModule.GetBuildPlatformName() + "/" + relativePath;
            bool bExist = KEngine.KResourceModule.ContainsResourceUrl(relativePath);
            if (!bExist)
            {
                Debug.LogError("找不到资源:" + relativePath);
                return "";

            }

            HotBytesLoader loader = null;
            try
            {
                loader = HotBytesLoader.Load(relativePath, LoaderMode.Sync);
                json = loader.Text;

            }
            finally
            {
                if (loader != null)
                    loader.Release();
            }
        }
        
        return json;
    }
}
