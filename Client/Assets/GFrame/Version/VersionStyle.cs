using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using IniParser.Model;
using IniParser.Parser;

public class VersionStyle : ScriptableObject
{
    public static int GameId = 1;
    public static int CSVersion = 0;
    public static ePlatform Platform
    {
        get
        {
#if UNITY_ANDROID
            return ePlatform.ANDROID;
#elif UNITY_IPHONE
                return ePlatform.IOS;
#elif UNITY_WEBPLAYER
            return ePlatform.ANDROID;
#elif UNITY_STANDALONE_WIN
            return ePlatform.ANDROID;
#elif UNITY_STANDALONE_OSX
            return ePlatform.ANDROID;
#else
            return ePlatform.IOS;
#endif
        }
    }
    private static VersionStyle _Instance = null;
    public static VersionStyle Instance
    {
        get
        {
#if UNITY_EDITOR
            string url = @"Assets/Resources/VersionStyle.asset";
            if (!File.Exists(url))
            {
                VersionStyle newAsset = ScriptableObject.CreateInstance<VersionStyle>();
                UnityEditor.AssetDatabase.CreateAsset(newAsset, url);
                UnityEditor.AssetDatabase.ImportAsset(url);
            }
            _Instance = UnityEditor.AssetDatabase.LoadAssetAtPath(url, typeof(VersionStyle)) as VersionStyle;

#endif
            if (_Instance == null)
                _Instance = Resources.Load("VersionStyle") as VersionStyle;
            return _Instance;
        }
    }
    public static string PatchTag { get { return (Platform + "_" + VersionStyle.GameId).ToLower(); } }
    public static string ResTag
    {
        get
        {
            string hotV = FrameVersion;
            return Platform.ToString().ToLower() + "_" + VersionStyle.GameId + "_" + hotV;
        }
    }
    public static string[] GetArr = new string[]{
                                                    "http://10.225.137.37:8088/shouyoufile/",
                                                    "http://rencunpatch.rzcdz2.com/msgame/",
                                                    };
    public static string FrameVersion
    {
        get
        {
            if (Instance.Publish == ePublish.Debug)
                return "0.1";
            else if (Instance.Publish == ePublish.BETA)
                return "0.2";
            else
                return "1.0";
        }
    }

    public static string hotFixVersion = "";
    public string defGetUrl { get { return MPrefs.GetString("DefGet", ""); } set { MPrefs.SetString("DefGet", value); } }
    public enum eServerState
    {
        Normal = 0,  // 服务器正常开启
        NewApp = 1,// 换包
        Close = 2, // 关闭维护
        Select = 3, //非强制换包
    }
    public bool IL2CPP = false;
    public bool IsEncryLua = false;
    public bool BuglyIsOpen = true;
    public bool isDevelopment = false;
    //public bool isHelpShift = true;
    //public bool showDebug = false;
    public bool isCheat = true;
    public int logLv = 2;
    public bool isTestLuaAB = false;
    public bool renderTest = false;
    public string Version { get { return FrameVersion + "." + this.ResVersion; } }
    public int ResVersion = 0;
    [Tooltip("是否为检测版本号并使用热更新功能")]
    public bool IsUpdateVersion = false;

    [NonSerialized]
    public eChannel Channel = eChannel.YJ;
    [Tooltip("发布类型，用于获取get服指定发布类型数据")]
    public ePublish Publish = ePublish.Debug;
    public eLanguage _Language = eLanguage.auto;
    private string m_sysLanguage = "";

    public string bundleIdentifier = "com.yk.msgame";
    public string bundleDisplayName = "five";
    public string _bundleName = "five";
    public string bundleName
    {
        set
        {
            _bundleName = value;
        }
        get
        {
            return _bundleName;
        }
    }
    public string appid;
    public string appkey;
    public string privatekey;
    public string bundleTimestamp = "1234567890";
    public int build = 1;
    [Tooltip("get服地址")]
    public eGetType GetUrl = eGetType.rzcdz2;
    public eGetType GetUrl2 = eGetType.None;
    public string sdk
    {
        get
        {
            if (this.eLoginSDK != eLoginSDK.None)
                return this.eLoginSDK.ToString();
            else
                return this.channelId;
        }
    }
    [NonSerialized]
    public eLoginSDK eLoginSDK = eLoginSDK.None;
    [NonSerialized]
    public bool IsSDKLogin = true;
    [NonSerialized]
    public int NetResVersion = 0;
    public string CDNVersions { get { return GetJsonInfo("cdn", "0"); } }
    public bool Is7ZPatch { get { return GetJsonInfo("Is7ZPatch","1") == "1"; } }
    [NonSerialized]
    public int Size = 0;
    [NonSerialized]
    public string Log;
    [NonSerialized]
    public string StartTime = ""; // 时间
    [NonSerialized]
    public string EndTime = ""; // 时间
                                //长连
    [NonSerialized]
    public string SocketIp = "192.168.0.55";
    [NonSerialized]
    public string SocketPort = "8888";
    //短连
    [NonSerialized]
    public string HttpIp = "http://192.168.0.55:90/";
    /// <summary>
    /// 服务器当前状态
    /// </summary>
    [NonSerialized]
    public eServerState State = eServerState.Normal; ///
    [NonSerialized]
    public bool IsGetCheat = false;
    [NonSerialized]
    public string ip = "";
    [NonSerialized]
    public string country = "";
    public Action<string, bool> mCallBack = null;
    //Dictionary<string, string> jsonDic = new Dictionary<string, string>();
    private IniData iniData;
    private string m_getUrl = "";
    public string getUrl
    {
        get
        {
            if (!string.IsNullOrEmpty(m_getUrl))
                return m_getUrl;
            eGetType t = GetUrl;
            if (curGetCall % 2 == 0)
            {
                if (GetUrl2 != eGetType.None)
                {
                    t = GetUrl2;
                }
                else if (!string.IsNullOrEmpty(defGetUrl))
                {
                    return defGetUrl;
                }

            }
            if (t < 0)
                t = 0;
            return GetArr[(int)t];
        }
        set { m_getUrl = value; }
    }
    public bool IsSDK
    {
        get
        {
            bool b = IsSDKLogin && this.IsUpdateVersion;
            //Debug.Log("IsSDKLogin"+ IsSDKLogin+ "this.IsUpdateVersion"+this.IsUpdateVersion +"QGameSDK.Instance.isSupportedLogin()" + QGameSDK.Instance.isSupportedLogin());
            return b;
        }
    }
    public bool IsNormal()
    {
        return this.State == eServerState.Normal || this.IsGetCheat || this.State == eServerState.Select;
    }
    public string getAppId()
    {
        string id = this.GetJsonInfo("appid");
        if (string.IsNullOrEmpty(id))
            id = this.appid;
        return id;
    }
    public string getAppKey()
    {
        string key = this.GetJsonInfo("appkey");
        if (string.IsNullOrEmpty(key))
            key = this.appkey;
        return key;
    }
    public string getPrivateKey()
    {
        string key = this.GetJsonInfo("privatekey");
        if (string.IsNullOrEmpty(key))
            key = this.privatekey;
        return key;
    }
    public bool IsShowUpdateConfirm { get; set; }
    public string channelId { get { return this.Channel.ToString(); } }
    private string TIMESTAMP = "_bundleTimestamp";
    [NonSerialized]
    public bool IsFirstStart = false;
    [NonSerialized]
    public int SourceResVersion = 0;
    public string Init(string path)
    {
        curGetCall = 1;
        SourceResVersion = this.ResVersion;
        //PluginMsgHandler.Init();
        if (this.IsUpdateVersion)
        {
            string localTimestamp = PlayerPrefs.GetString(TIMESTAMP);
            if (localTimestamp != bundleTimestamp)
            {
                IsFirstStart = true;
            }
            else
            {
                //读取本地版本号
                if (File.Exists(path))
                {
                    string str = File.ReadAllText(path);// Util.GetFileText(persistentVersionPath);
                    Int32.TryParse(str, out this.ResVersion);
                    Debug.Log("version:" + str + ", " + path);
                }
            }
        }
#if UNITY_EDITOR

#else
            //LuaInterface.Debugger.useLog = false;
            InitPlugins();
#endif
        return "";
    }
    public void SaveBundleTimestamp()
    {
        PlayerPrefs.SetString(TIMESTAMP, bundleTimestamp);
    }
    public void InitPlugins()
    {
        //初始化bugly
        if (BuglyIsOpen)
        {
         //   BuglyInit.Init();
        }
        //MtaSDK.InitByStyle(this);
        //#if UNITY_IOS

    }
    /// <summary>
    /// 回调参数 fun(string msg,bool isError);
    /// </summary>
    /// <param name="fun"></param>
    public void LuaInitGet(int num, Action<string, bool> callback)
    {
        curGetCall = 1;
        //getMaxCallNum = 5;
        InitGet(callback);
    }
    void initGet()
    {
        MResUpdate.ShowWait(true);
        InitGet(mCallBack);
    }
    public void InitGet(Action<string, bool> callback)
    {
        mCallBack = callback;
        //SystemInfo.deviceUniqueIdentifier
        //SystemInfo.deviceType
        //string ver = Version.Remove(Version.LastIndexOf('.'));
        string dev = SystemInfoUtil.deviceModel;
        if (string.IsNullOrEmpty(dev))
        {
            dev = "SystemInfo.deviceName==null";
        }
        string mac = SystemInfoUtil.GetMac() + "_" + this.ResVersion + "_" + this.build;
        this.initToPost(GameId.ToString(), this.bundleName, FrameVersion, Platform.ToString().ToLower(), SystemInfoUtil.GetDevice(), SystemInfoUtil.deviceUniqueIdentifier, mac, Publish.ToString());
    }
    private string ipInfo = "";
    /// <summary>
    /// Init the specified aGameId, aChannel, aVersion, aPlatform, aDevice, aIdfa, aMac and aCallback.
    /// </summary>
    /// <param name="aGameId">游戏id</param>
    /// <param name="abundleName">渠道id</param>
    /// <param name="aVersion">当前版本</param>
    /// <param name="aPlatform">平台 IOS or Android</param>
    /// <param name="aDevice">A device.</param>
    /// <param name="aIdfa">idfa.</param>
    /// <param name="aMac">mac.</param>
    /// <param name="aCallback">A callback.</param>
    /// 例："1", "默认", "1.0.0", "ANDROID", "device", "idfa", "mac", OnGameSDKInitCallback
    void initToPost(string aGameId, string abundleName, string aVersion, string aPlatform, string aDevice, string aIdfa, string aMac, string aBuild)
    {
        WWWHttpHelper.logUrl = this.getUrl;
        //请求参数里的json格式要求：["1","默认","1.0.0","ANDROID","device","idfa","mac"]
        //List<string> mList = new List<string>();
        //mList.Add(abundleName);
        //mList.Add(aPlatform);
        //mList.Add(aBuild);
        //mList.Add(aVersion);
        //string mjson = Newtonsoft.Json.JsonConvert.SerializeObject(mList);
        string tempUrl = MPrefs.GetString("_changeGetUrl", "");
        if (string.IsNullOrEmpty(tempUrl))
        {
            tempUrl = getUrl;
        }
        string rName = (abundleName + "_" + aPlatform + "_" + aVersion).ToLower();
        string sizeInfo = tempUrl + rName + ".txt" + "?v=" + UnityEngine.Random.Range(0f, 1f);
        WWWHttpData wData = WWWHttpHelper.ToGet(sizeInfo, CallBack);
       // WWWHttpData wData = WWWHttpHelper.ToPost(tempUrl, mjson, CallBack);
       // wData.IsJson = false;
        wData.TimeOut = 3f;
        wData.isToGet = true;
        //wData.defUrl = defGetUrl;
        wData.delayTime = (curGetCall-1);
        if (wData.delayTime > 3f)
            wData.delayTime = 3f;
#if UNITY_EDITOR
        wData.delayTime = 0f;
#endif
    }
    int getMaxCallNum = 3;
    int curGetCall = 1;
    private DateTime serverTime;
    private long serverTimeTick = 0;
    public DateTime CurDateTime
    {
        get
        {
            if (serverTimeTick != 0)
            {
                return System.DateTime.Now.AddTicks(serverTimeTick);// serverTime.AddSeconds(UnityEngine.Time.realtimeSinceStartup - serverTimeTick);
            }
            return System.DateTime.Now;
        }
    }
    private void CallBack(WWWHttpData data)
    {
       // excCallBakc("",false);
        //return;
        if (data.isError)
        {
            if (data.url == defGetUrl)
                defGetUrl = "";
            //MPrefs.SetString("getUrl", "");
            curGetCall++;
            if (curGetCall > getMaxCallNum || !MUtil.NetAvailable)
            {
                curGetCall = 1;
                MResUpdate.ShowWait(false);
                if (MResUpdate.Instance != null)
                {
                    string err = VersionManager.Instance.LangStyle.没有网络;
                    if (MUtil.NetAvailable)
                        err = data.Message;
                    MResUpdate.ShowPopup(VersionManager.Instance.LangStyle.没有网络, err, initGet, VersionManager.QuitGame);
                }
               // else if (GameManager.uluaMgr != null)
               //     GameManager.LuaCallMethod("UserConfig.GetError");
                return;
            }
            initGet();
            return;
        }
        //MPrefs.SetString("getUrl", getUrl);
        bool b = false;
        string msg = data.Message;
        try
        {
            var parser = new IniDataParser();
            this.iniData = parser.Parse(msg);

            string st = GetJsonInfo("State");
            State = (eServerState)Enum.Parse(typeof(eServerState), st, true);
            string sTime = GetJsonInfo("ServerTime");
            if (!string.IsNullOrEmpty(sTime) && DateTime.TryParse(sTime, out serverTime))
            {
                serverTimeTick = serverTime.Ticks - System.DateTime.Now.Ticks;//UnityEngine.Time.realtimeSinceStartup;
            }
            QQCloudMgr.Init(this);
            Log = GetJsonInfo("Log");
            Log = Log.Replace("|", "\n");
            Log = Log.Replace("\\n", "\n");
            string defGet = GetJsonInfo("DefGet");
            if (!string.IsNullOrEmpty(defGet))
                defGetUrl = defGet;
            StartTime = GetJsonInfo("StartTime");
            EndTime = GetJsonInfo("EndTime");
            Log = Log.Replace("[StartTime]", StartTime);
            Log = Log.Replace("[EndTime]", EndTime);
            string money = GetJsonInfo("Money");
            Log = Log.Replace("[Money]", money);
            if (!string.IsNullOrEmpty(money))
                Log = Log.Replace("[Money]", money);


            if (!Int32.TryParse(GetJsonInfo("PatchVersion"), out this.NetResVersion))
                NetResVersion = this.ResVersion;
            //NetResVersion = 1;
            hotFixVersion = GetJsonInfo("hotFixVersion");
            SocketIp = GetJsonInfo("SocketIp");
            SocketPort = GetJsonInfo("SocketPort");
            HttpIp = GetJsonInfo("HttpIp");
            string CheatCode = "";
            if (TryGetJsonInfo("CheatCode", out CheatCode))
            {
                MResUpdate.cheatCode = CheatCode;
            }
            IsShowUpdateConfirm = GetJsonInfo("ShowConfirm") == "1" ? true : false;
            string ChangeChannel = "";
            if (TryGetJsonInfo("ChangeChannel", out ChangeChannel))
            {
                int newChannel = 0;
                if (Int32.TryParse(ChangeChannel, out newChannel))
                {
                    this.Channel = (eChannel)newChannel;
                }
            }
            string allCheat = GetJsonInfo("AllCheat");
            if (allCheat == "1")
                this.isCheat = true;
            else if (allCheat == "0")
                this.isCheat = false;
            string testIdfa = "";
            if (TryGetJsonInfo("testIdfa", out testIdfa))
            {
                if (testIdfa.IndexOf(SystemInfoUtil.deviceUniqueIdentifier) > -1)
                {
                    string testPatch = "";
                    if (TryGetJsonInfo("testPatch", out testPatch))
                    {
                        int testPatchV = 0;
                        Int32.TryParse(testPatch, out testPatchV);
                        if (testPatchV > this.NetResVersion)
                            this.NetResVersion = testPatchV;
                    }
                    if (GetJsonInfo("IsCheat") == "true")
                        this.isCheat = true;
                }
            }
            MPrefs.SetString("_changeGetUrl", GetJsonInfo("changeGetUrl"));
        }
        catch (Exception e)
        {
            b = true;
            msg = "param:" + msg + "\n error:" + e.ToString();
            Debug.LogError(msg);
        }
        excCallBakc(msg, b);
        //string a1 = arr[0].asDict()["1"].AsString();
        //string a2 = arr[1].asDict()["2"].AsString();
        //string a3 = arr[2].asDict()["3"].AsString();
    }
    void excCallBakc(string msg, bool isError)
    {
        if (mCallBack != null)
            mCallBack(msg, isError);
        mCallBack = null;
    }
    /// <summary>
    /// get服json数组，参数key，返回value
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetJsonInfo(string key, string def = "")
    {
        string info = "";
        this.iniData.TryGetKey(key, out info);
        if (string.IsNullOrEmpty(info))
             info = def;
        return info;
    }
    public bool EqualString(string key, string v)
    {
        return GetJsonInfo(key) == v;
    }
    public int GetJsonInfo(string key, int def)
    {
        int intV = def;
        string v = "";
        if (TryGetJsonInfo(key, out v))
        {
            Int32.TryParse(v, out intV);
        }
        return intV;
    }
    public bool TryGetJsonInfo(string key, out string v)
    {
        v = GetJsonInfo(key);
        return string.IsNullOrEmpty(v) ? false : true;
    }
    public string HotFixKey
    {
        get
        {
            int forcePatch = GetJsonInfo("ForcePatch", 0);
            if (forcePatch > this.ResVersion)
                return "Force";
            return GetJsonInfo("HotFixKey", "Force");
        }
    }
    public bool VersionBackOff
    {
        get
        {
            int backOff = GetJsonInfo("VersionBackOff", 1);
            return backOff == 1;
        }
    }
}
    public class UserInfo
    {
        public string password = "";
        public bool success = true;
        public string username = "";
    }
