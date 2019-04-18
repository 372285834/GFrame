//using Fire;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using KEngine;

//public class VersionPlayer : Singleton<VersionPlayer>
//{
//    public AppVersion version
//    {
//        get;
//        set;
//    }
//    public VersionPlayer()
//    {
//        version = LoadVersionStr();
//    }

//    public AppVersion LoadVersionStr()
//    {
//        AssetFileLoader floader = AssetFileLoader.Load("Version/Version.txt", null, LoaderMode.Sync);
//        TextAsset ta = floader.ResultObject as TextAsset;
//        string s = ta.text;
//        floader.Release();
//        AppVersion v = new AppVersion(s.Trim());
//        return v;
//    }
//    public override void Dispose()
//    {

//    }
//}
