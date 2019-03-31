using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
    /// <summary>
    /// IniFiles的类
    /// </summary>
public class IniParser
{
    public Dictionary<string, string> configData = new Dictionary<string,string>();
    string fullFileName;
    public IniParser(string path)
    {
        bool hasCfgFile = File.Exists(path);
        if (hasCfgFile == false)
        {
            StreamWriter writer = new StreamWriter(File.Create(path), UTF8Encoding.Default);
            writer.Close();
        }
        fullFileName = path;
        StreamReader reader = new StreamReader(path);
        string line;
        //int indx = 0;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith(";") || string.IsNullOrEmpty(line) || line.StartsWith("//"))
                continue;
            int idx = line.IndexOf('=');
            if (idx == -1)
                continue;
            string key = line.Substring(0, idx).Trim();
            string value = line.Substring(idx + 1).Trim();
            if (!string.IsNullOrEmpty(value))
            {
                if (configData.ContainsKey(key))
                {
                    Debug.LogError("重复的key:" + key);
                    return;
                }
                configData.Add(key, value);
            }
        }
        reader.Close();
    }
    public string get(string key)
    {
        if (configData.Count <= 0)
            return null;
        else if(configData.ContainsKey(key))
            return configData[key].ToString();
        else
            return null;
    }
    public void set(string key, string value)
    {
        if (configData.ContainsKey(key))
            configData[key] = value;
        else
            configData.Add(key, value);
    }
    public void save()
    {
        StreamWriter writer = new StreamWriter(fullFileName,false,Encoding.Default);
        IDictionaryEnumerator enu = configData.GetEnumerator();
        while (enu.MoveNext())
        {
            if (enu.Key.ToString().StartsWith(";"))
                writer.WriteLine(enu.Value);
            else
                writer.WriteLine(enu.Key + "=" + enu.Value);
        }
        writer.Close();
    }
}
