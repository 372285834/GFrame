using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using highlight;
using System.Text;

public class VersionManager : MonoBehaviour
{
    public class ResData
    {
        public string saveDir = "";
        public string toDir = "";
        public int Length { get { return patchList.Count; } }
        public List<PatchResData> patchList = new List<PatchResData>();
        public float sizeShow;
        public long size;
        public highlight.AcHandler CompleteFun;
        //错误函数;
        public Action<RequestInfo> ErrorFun;
        //public Action<string> ErrorFun;
        //进度函数;
        public Action<float, float> ProgressFun;
        public ResData(string info)
        {
            saveDir = Application.persistentDataPath + "/" + Util.PlatformDir + "." + Instance.netVersion + "/";
            toDir = Application.persistentDataPath + "/" + Util.PlatformDir + "/";
            patchList = PatchResData.CreatPatchList(info, out size);
            sizeShow = (float)size / 1024f;
        }
        public PatchResData curData { get { return patchList[loadIndex]; } }
        /// <summary>
        /// 请求最新的资源路径;
        /// </summary>
        public void startLoad()
        {
            if (loadIndex >= Length)
            {
                if (loadSize != size)
                {
                    Debug.LogError("【下载异常】:" + loadSize + "-" + size);
                }
                if (CompleteFun != null)
                    CompleteFun();
                return;
            }
            string curNetName = curData.resName + "?v=" + VersionManager.Instance.mStyle.CDNVersions;
            VersionManager.Instance.SendEvent("ResStart", curData.resName);

            RequestInfo reInfo = new RequestInfo(QQCloudMgr.HotFixUrl + curNetName);
            Debug.Log("[热更新]:" + reInfo.Url + "  " + curData.size + "  " + (loadIndex + 1) + "/" + Length);
            reInfo.obj = curData;
            reInfo.SetSaveUrl(saveDir + curData.toName, curData.size);
            reInfo.CompleteFun = loadResComplete2;
            reInfo.ProgressFun = loadResProgress2;
            reInfo.ErrorFun = ErrorFun;
            reInfo.request();
        }
        protected void loadResProgress2(long cur, long all)
        {
            float p = (float)(loadSize + cur) / 1024f;
            if (ProgressFun != null)
                ProgressFun(p, sizeShow);
        }
        protected void loadResComplete2(RequestInfo info)
        {
            PatchResData db = info.obj as PatchResData;
            int wSize = info.downloadSize;
            if (wSize != db.size)
            {
                info.error = "sizeError:" + wSize + "-" + db.size;
                info.Error();
                return;
            }
            VersionManager.reLoadNum = 3;
            VersionManager.Instance.SendEvent("ResOK", db.resName + " " + (loadIndex + 1) + "  " + db.size);
            info.Save();
            //string outFile = FileUtils.WriteFileStream(bts, curSaveUrl);
            loadIndex++;
            loadSize += wSize;
            startLoad();
        }
        //protected void loadResProgress(ABLoaderInfo data)
        //{
        //    int pSize = data.Size;
        //    float p = (float)(curSize + pSize) / 1024f;
        //    ProgressFun(p);
        //}
        //protected void loadResComplete(ABLoaderInfo data)
        //{
        //    WWW www = data.www as WWW;
        //    if (!string.IsNullOrEmpty(www.error))
        //        return;
        //    int wSize = www.bytes.Length;
        //    PatchResData db = data.obj as PatchResData;
        //    if (wSize != db.size)
        //    {
        //        data.Error();
        //        return;
        //    }
        //    VersionManager.Instance.SendEvent("ResOK", db.resName, data.time);
        //    curSize += wSize;
        //    string outFile = FileUtils.WriteFileStream(www.bytes, curSaveUrl);
        //    idx++;
        //    MPrefs.SetString("v_loadIndex", idx);
        //    MPrefs.SetString("v_curLoadSize", curSize);
        //    startLoad();
        //}
        public void MoveFile()
        {
            loadIndex = 0;
            loadSize = 0;
            if (!Directory.Exists(saveDir))
                return;
            Debug.Log("下载MoveFile:" + saveDir + ", " + patchList.Count);
            int num = 0;
            for (int i = 0; i < patchList.Count; i++)
            {
                string source = saveDir + patchList[i].toName;
                string to = toDir + patchList[i].toName;
                bool b = MFileUtils.MoveFile(source, to);
                if (b)
                    num++;
            }
            try
            {
                Directory.Delete(saveDir, true);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
            }
            VersionManager.Instance.SendEvent("MoveFile", saveDir + " " + num + "-" + patchList.Count);
        }
    }
    static VersionManager mVersionManager;//单例

    public VersionStyle mStyle
    {
        get
        {
            return VersionStyle.Instance;
        }
    }

    static int _loadIndex = -1;
    public static int loadIndex
    {
        get
        {
            if (Instance == null)
                return 0;
            if (_loadIndex < 0)
                _loadIndex = MPrefs.GetInt("loadIndex_" + Instance.netVersion, 0);
            return _loadIndex;
        }
        set
        {
            if (Instance == null)
                return;
            _loadIndex = value;
            MPrefs.SetString("loadIndex_" + Instance.netVersion, value);
        }
    }
    static int _loadSize = -1;
    public static int loadSize
    {
        get
        {
            if (Instance == null)
                return 0;
            if (_loadSize < 0)
                _loadSize = MPrefs.GetInt("loadSize_" + Instance.netVersion, 0);
            return _loadSize;
        }
        set
        {
            if (Instance == null)
                return;
            _loadSize = value;
            MPrefs.SetString("loadSize_" + Instance.netVersion, _loadSize);
        }
    }
    /// <summary>
    /// 是否作弊
    /// </summary>
    public bool isCheat { get { return mStyle.isCheat; } }

    /// <summary>
    /// 单例
    /// </summary>
    public static VersionManager Instance
    {
        get
        {
            return mVersionManager;
        }
    }
    private long tempTime = 0;
    /// <summary>
    /// Awake
    /// </summary>
    void Awake()
    {
        mVersionManager = this;
    }
    //本地版本号;
    public string localVersion { get { return mStyle.Version; } }
    //网络版本号;
    public string netVersion { get { return VersionStyle.FrameVersion + "." + mStyle.NetResVersion; } }// Config.Vesrion; 

    //持久数据目录下版本路径;
    protected string localVersionPath
    {
        get
        {
            return mPath + "/version.txt";
        }
    }

    //客户端资源总量;
    //protected int totalNum;
    //当前已经加载的资源量;
    //protected int currentNum;
    //待解压资源;
    //protected List<string> toDecompressList = new List<string>();

    Action mCheckComplete;
    /// <summary>
    /// 资源路径
    /// </summary>
    private string mPath = "";
    private string package7zUrl
    {
        get
        {
            //return mPath +".7z";
            return Application.persistentDataPath + "/" + resName + ".7z";
        }
    }
    private string resName;
    public ResData mResData;
    private UpdateLangStyle mLangStyle;
    public UpdateLangStyle LangStyle
    {
        get
        {
            if (mLangStyle == null)
            {
                mLangStyle = new UpdateLangStyle();
                mLangStyle.Init();
            }
            return mLangStyle;
        }
    }
    // public static GoogleEvent VersionEvt;
    /// <summary>
    /// 检测游戏版本;
    /// </summary>
    public void CheckVersion(Action callBack)
    {
        mPath = Application.persistentDataPath + "/" + highlight.Util.PlatformDir;
        mStyle.Size = 0;
        Debug.Log("persistentDataPath:" + mPath + ",IsVersionUpdate:" + mStyle.IsUpdateVersion + ",idfa>>>:" + SystemInfoUtil.deviceUniqueIdentifier);
        //            #if UNITY_IPHONE
        //            Debug.Log("========vendorIdentifier：" + UnityEngine.iOS.Device.vendorIdentifier);
        //            Debug.Log("========advertisingIdentifier：" + UnityEngine.iOS.Device.advertisingIdentifier);
        //            Debug.Log("========deviceUniqueIdentifier：" + SystemInfo.deviceUniqueIdentifier);
        //#endif
        if (!System.IO.Directory.Exists(mPath))
            System.IO.Directory.CreateDirectory(mPath);

        MResUpdate.ShowInitInfo("", "");
        mStyle.Init(localVersionPath);
        mCheckComplete = callBack;

        if (!mStyle.IsUpdateVersion)
        {
          //  SDK.QGameSDK.Instance.Init(delegate() { this.end(); });
            return;
        }
        //oldMd5 = new CompareMD5(mPath + "/", "resFiles.txt");
        //VersionLoadingLogic.Instance.Open();
        IsGetVersion = true;
    }
    static bool IsGetVersion = false;//get版本数据
    static bool IsStartDownload = false;//开始下载资源
    static bool IsResUpdateOK = false;
    public void Update()
    {
        if (IsResUpdateOK)
        {
            IsResUpdateOK = false;
            end();
        }
        else if (IsGetVersion)
        {
            IsGetVersion = false;
            //  GoogleAnsSdk.LogScreen("GetStart");
            mStyle.InitGet(delegate (string msg, bool IsError)
            {
                MResUpdate.progressValue = 0f;
                // WWWHttpHelper.Log("获取Get服数据，检测版本号");
                if (IsError)
                {
                    // GoogleAnsSdk.LogScreen("GetError");
                    string popupInfo = LangStyle.版本号下载异常info;
                    if (!mStyle.IsSDK)
                        msg = LangStyle.版本号下载异常info + "idef：" + SystemInfoUtil.deviceUniqueIdentifier + "\n" + msg;
                    MResUpdate.ShowPopup(LangStyle.版本号下载异常, msg, null, QuitGame);
                    return;
                }
                SendEvent(mStyle.State.ToString(), this.netVersion);
                //   GoogleAnsSdk.LogScreen("GetOK");
                VersionStyle.SendEvent("Init");
               // SDK.QGameSDK.Instance.Init(sdkInitCallBack);
                    //sdkInitCallBack();
            });
            //readNetVersion();
        }
        else if (IsStartDownload)//开始下载资源
        {
            //WWWHttpHelper.Log("开始下载资源");
            IsStartDownload = false;
            if (mStyle.IsShowUpdateConfirm && loadIndex <= 0)
            {
                string sizeStr = StringX.KBToSizeStr((float)mStyle.Size / 1024f);
                string patchInfo = mStyle.GetJsonInfo("PatchInfo");
                patchInfo = patchInfo.Replace("\\n", "\n");
               // patchInfo = patchInfo.Replace("\\\"", "\"");
                MResUpdate.ShowPopup(LangStyle.版本更新 + localVersion + "-" + netVersion + " " + "(" + sizeStr + ")", patchInfo, loadNetRes, QuitGame);
            }
            else
            {
                loadNetRes();
            }
        }
    }
    void sdkInitCallBack()
    {
        MResUpdate.Instance.SetState(mStyle.State);
        if (mStyle.State == VersionStyle.eServerState.Close)
        {
            MResUpdate.SetCdTime();
            MResUpdate.ShowPopup(LangStyle.版本维护, mStyle.Log, null, QuitGame);
            MResUpdate.CheatCallBack = cheatEnter;
        }
        else if (mStyle.State == VersionStyle.eServerState.NewApp || mStyle.build < mStyle.GetJsonInfo("build", 0))
        {
            MResUpdate.ShowPopup(LangStyle.版本异常, LangStyle.版本异常info + "\n" + mStyle.Log, OpenDownloadUrl, QuitGame);
        }
        else if (mStyle.State == VersionStyle.eServerState.Select)
        {
            MResUpdate.ShowPopup(LangStyle.版本异常, LangStyle.版本异常info, OpenDownloadUrl, vaildCheck);
        }
        else if (mStyle.IsNormal())
            vaildCheck();

        //GateUrl = gate;
    }
    public void RequestGet()
    {
        IsGetVersion = true;
    }
    void cheatEnter(bool noSDK)
    {
        if (noSDK)
            this.mStyle.IsSDK = false;
        this.mStyle.IsGetCheat = true;
        vaildCheck();
    }
    public static int reLoadNum = 3;
    private bool isBackOffResVersion = false;
    public void backOneCheck(bool force)
    {
        isBackOffResVersion = true;
        int flag = mStyle.GetResFlag(mStyle.ResVersion);
        if (flag < 0 || force)
        {
            DeleteLocalCache();
            mStyle.SetResVersion(mStyle.SourceResVersion);
        }
        else
        {
            DeleteLocal7z();
            mStyle.SetResVersion(mStyle.ResVersion - 1);
        }
        vaildCheck();
    }
    /// <summary>
    /// 版本号有效性检测;
    /// </summary>
    protected void vaildCheck()
    {
        //mStyle.NetResVersion = 4;
        resName = VersionStyle.PatchTag + "_" + this.localVersion + "_" + netVersion;
        MResUpdate.Instance.Init();
        Debug.Log("版本号本地_网络：" + resName + ", 下载标记：" + loadIndex + ", " + loadSize + ", " + mStyle.HotFixKey);
        SendEvent("init", localVersion + "-" + netVersion);
        //Debug.Log(Frame.Util.PlatformDir + "版本号：" + Config.ConfigData.version);
        if (mStyle.IsFirstStart)
        {
            mStyle.SaveBundleTimestamp();
            DeleteLocalCache();
            mStyle.IsFirstStart = false;
        }
        if (mStyle.NetResVersion < mStyle.SourceResVersion)
        {
            Debug.Log("版本号异常,mStyle.NetResVersion < mStyle.SourceResVersion, local-net-SourceResVersion：" + localVersion + "-" + netVersion + "-" + mStyle.SourceResVersion);
            end(false);
            return;
        }
        if (mStyle.NetResVersion == mStyle.ResVersion)
        {
            if (mStyle.isNeedClearVersion)//问题已解决，回退1个版本，再次热更。
            {
                Debug.Log("问题已解决，回退1个版本，再次热更。");
                backOneCheck(false);
                return;
            }
            Debug.Log("版本为最新");
            end();
            return;
        }
        if (mStyle.NetResVersion < mStyle.ResVersion)//问题未解决，回退到初始版本，从初始版本开始更新。
        {
            //if (mStyle.VersionBackOff)
            Debug.Log("问题未解决，回退到初始版本，从初始版本开始更新 local-net：" + localVersion + "-" + netVersion);
            backOneCheck(true);
            return;
        }
        if (!isBackOffResVersion && mStyle.isNeedClearVersion)//问题已解决，回退1个版本，再次热更。
        {
            Debug.Log("问题已解决，回退1个版本，再次热更。");
            backOneCheck(false);
            return;
        }
        MResUpdate.ShowInitInfo(string.Format(LangStyle.版本更新2, localVersion, netVersion), mStyle.Is7ZPatch ? "....." : "...");
        {
            string rName = mStyle.Is7ZPatch ? resName + "_7z" : resName;
            string flag = "?v=" + mStyle.CDNVersions;
            int resFlag = mStyle.GetResFlag(mStyle.NetResVersion);
            flag += "." + resFlag;
            string sizeInfo = QQCloudMgr.HotFixUrl + rName + ".txt" + flag;
            SendEvent(resName + "Start", "");
            WWWHttpHelper.ToGet(sizeInfo, delegate (WWWHttpData data)
            {
                if (data.isError)
                {
                    if(MUtil.NetAvailable)
                    {
                        bool isTimeOut = data.error.IndexOf("TimeOut") > -1;
                        if (isTimeOut && reLoadNum > 0)
                        {
                            reLoadNum--;
                            vaildCheck();
                            return;
                        }
                        //if (!isTimeOut && !isBackOffResVersion && mStyle.ResVersion > 0)
                        //{
                        //    isBackOffResVersion = true;
                        //    mStyle.SetResVersion(mStyle.ResVersion-1);
                        //    vaildCheck();
                        //    return;
                        //}
                    }
                    //isBackOffResVersion = false;
                    SendEvent(resName + "Error", data.error);
                    MResUpdate.ShowPopup(LangStyle.下载失败, data.error + "\n" + localVersion + "-" + netVersion, vaildCheck, QuitGame);
                }
                else
                {
                    reLoadNum = 3;
                    SendEvent(resName + "OK", "", data.time);
                    string str = data.Message.TrimEnd();
                    if (mStyle.Is7ZPatch)
                    {
                        int mSize = 0;
                        Int32.TryParse(str, out mSize);
                        mStyle.Size = mSize;
                    }
                    else
                    {
                        mResData = new ResData(str);
                        mStyle.Size = (int)mResData.size;
                    }
                    IsStartDownload = true;
                }
            }, 10f);

            //ABLoaderInfo sizeAB = new ABLoaderInfo(sizeInfo, true);
            //BundleManager.Instance.LoadResource(sizeAB);
            //sizeAB.CompleteFun = delegate(ABLoaderInfo ab)
            //{
            //    string str = ab.www.text.TrimEnd();
            //    mResData = new ResData(str);
            //    mStyle.Size = mResData.size;
            //    IsStartDownload = true;
            //};
            //sizeAB.ErrorFun = delegate(ABLoaderInfo ab)
            //{
            //    MResUpdate.ShowPopup(LangStyle.下载失败, ab.www.error + "\n" + localVersion + "-" + netVersion, null, QuitGame);
            //};
        }
    }

    /// <summary>
    /// 请求最新的资源路径;
    /// </summary>
    protected void loadNetRes()
    {
        if (mStyle.Is7ZPatch)
        {
            DownLoad7Z();
            return;
        }
        int lIndex = loadIndex;
        string hotFixKey = mStyle.HotFixKey;
        if (lIndex >= mResData.Length)
        {
            if (hotFixKey != "Down")
            {
                mResData.MoveFile();
                IsResUpdateOK = true;
                return;
            }
            else
                end(false);
            return;
        }
        if (hotFixKey == "Force")
            mResData.ProgressFun = loadResProgress;
        else
            end(false);
        mResData.ErrorFun = loadResError;
        mResData.CompleteFun = delegate ()
        {
            if (hotFixKey == "Force")
            {
                mResData.MoveFile();
                IsResUpdateOK = true;
            }
            tempTime = (System.DateTime.Now.Ticks - tempTime) / 10000;
            SendEvent("下载完毕." + lIndex, "", tempTime);
            Debug.Log("下载完毕:" + tempTime);
        };
        tempTime = System.DateTime.Now.Ticks;
        SendEvent("开始下载." + lIndex);
        mResData.startLoad();
    }
    public void SendEvent(string info, string lable = "", long v = 0)
    {
        VersionStyle.SendEvent(info, lable);
        //     if(VersionEvt == null)
        //          VersionEvt = new GoogleEvent("Version_" + VersionStyle.PatchTag + "_" + this.localVersion + " " + mStyle.HotFixKey, 0, -1);
        //      VersionEvt.SendEvent(info, v, lable);
    }
    float curTime = 0f;
    public float speed;
    float lastProgress;
    /// <summary>
    /// 下载进度;
    /// </summary>
    protected void loadResProgress(float curSize, float allSize)
    {
        float curProgress = curSize / allSize;
        MResUpdate.progressValue = curProgress;
        curTime -= Time.deltaTime;
        if (curTime > 0f)
            return;
        curTime = 1f;
        string per = "";
        if (allSize > 0)
        {
            float size = allSize;
            speed = (curProgress - lastProgress) * size;
            per = StringX.KBToSizeStr(curSize) + "/" + StringX.KBToSizeStr(size) + "   " + LangStyle.下载速度 + StringX.KBToSizeStr(speed) + "/s";
        }
        MResUpdate.ShowInitInfo(LangStyle.版本更新 + localVersion + "-" + netVersion, LangStyle.版本更新2info + per);//,MResUpdate.progressValue.ToString("p")
        lastProgress = curProgress;
        //SystemInfo.systemMemorySize;
        //VersionLoadingLogic.Instance.ShowMessage("下载资源:" + currentNum + "/" + totalNum);
        //VersionLoadingLogic.Instance.ShowPercent(percent);
    }
    /// <summary>
    /// 加载网络版本号错误;
    /// </summary>
    /// <param name="param"></param>
    protected void loadResError(RequestInfo data)
    {
        if (!MUtil.NetAvailable && reLoadNum > 0 && data.error.IndexOf("TimeOut") > -1)
        {
            reLoadNum--;
            ReDownload();
            return;
        }
        //WWW www = data.www as WWW;
        Debug.LogError("加载资源异常：" + data.error + " 路径:" + data.Url);
        if (mStyle.HotFixKey == "Force")
            MResUpdate.ShowPopup(LangStyle.下载失败, data.error + ":" + (loadIndex + 1) + "," + data.Url + "\n" + SystemInfoUtil.deviceUniqueIdentifier + "\n" + LangStyle.下载失败info, ReDownload, QuitGame);
        SendEvent("ResError", data.Url + " " + data.error, 0);
        //end();
    }
    //protected void loadResError(string error)
    //{
    //    Debug.Log("加载资源异常：" + error);
    //    MResUpdate.ShowPopup(LangStyle.下载失败, error, ReDownload, QuitGame);
    //}

    string openNewApp = null;
    void OnDestroy()
    {
        if (!string.IsNullOrEmpty(openNewApp))
            Application.OpenURL(openNewApp);
    }
    public void OpenDownloadUrl()
    {
        openNewApp = mStyle.GetJsonInfo("NewAppUrl");
        QuitGame();
    }
    /// <summary>
    /// 存储版本号;
    /// </summary>
    protected void saveResVersion(int resV)
    {
        StreamWriter sw = new StreamWriter(localVersionPath, false);
        sw.Write(resV);
        sw.Flush();
        sw.Close();
        sw.Dispose();
    }

    void end(bool save = true)
    {
        SendEvent("结束");
        if (save)
        {
            mStyle.SetResVersion(this.mStyle.NetResVersion);
        }
        isBackOffResVersion = false;
        //    BuglyInit.SetScene(this.mStyle.ResVersion);
        if (mCheckComplete != null)
            mCheckComplete();
        mCheckComplete = null;
        this.enabled = false;
    }
    public void ShowPopup(string title, string info)
    {
        MResUpdate.ShowPopup(title, info, null, QuitGame);
    }

    public void DownLoad7Z()
    {
        string hotFixKey = mStyle.HotFixKey;
        string pkg7zUrl = package7zUrl;
        if (File.Exists(pkg7zUrl))
        {
            int pkgSize = PlayerPrefs.GetInt(pkg7zUrl, 0);
            if(pkgSize > 0 && pkgSize != mStyle.Size)
            {
                File.Delete(pkg7zUrl);
            }
            else
            {
                FileInfo fl = new FileInfo(pkg7zUrl);
                if (fl.Length == mStyle.Size)
                {
                    if (hotFixKey != "Down")
                    {
                        Decompression();
                        return;
                    }
                    else
                        end(false);
                }
            }
            
        }
        PlayerPrefs.SetInt(pkg7zUrl, mStyle.Size);
        string flag = "?v=" + mStyle.CDNVersions;
        int resFlag = mStyle.GetResFlag(mStyle.NetResVersion);
        flag += "." + resFlag;
        string resUrl = resName + ".7z" + flag;
        SendEvent("ResStart7z", resUrl);
        RequestInfo reInfo = new RequestInfo(QQCloudMgr.HotFixUrl + resUrl);
        reInfo.SetSaveUrl(pkg7zUrl, mStyle.Size, true);
        Debug.Log("[热更新]:" + reInfo.Url + "  " + mStyle.Size);
        reInfo.CompleteFun = complete7z;
        reInfo.ErrorFun = loadResError;
        if (hotFixKey == "Force")
            reInfo.ProgressFun = delegate (long cur, long all) { loadResProgress((float)cur / 1024f, (float)all / 1024f); };
        reInfo.request();

        if (hotFixKey != "Force")
            end(false);
    }
    protected void complete7z(RequestInfo info)
    {
        int wSize = info.downloadSize;
        if (wSize != mStyle.Size)
        {
            info.error = "sizeError:" + wSize + "-" + mStyle.Size;
            info.Error();
            return;
        }
        reLoadNum = 3;
        SendEvent("ResOK7z", resName + ".7z" + " " + mStyle.Size);
        info.Save();
        if (mStyle.HotFixKey == "Force")
            Decompression();
    }
    public void Decompression()
    {
        if (!File.Exists(package7zUrl))
        {
            Debug.LogError("无可解压文件：" + package7zUrl);
            return;
        }
        //UncompressCommand cmd = new UncompressCommand(package7zUrl, Application.persistentDataPath + "/", DecompressionResOk, true);
        MResTools.FileDecompression(package7zUrl, Application.persistentDataPath + "/", DecompressionResOk);
    }
    //string decompressErrorInfo = "";
    void DecompressionResOk(bool IsOK, string errorInfo)
    //void DecompressionResOk(UncompressCommand cmd)
    {
        if (string.IsNullOrEmpty(errorInfo))
        {
            Debug.Log("解压完毕");
            SendEvent("解压完毕");
            string dirPath = package7zUrl.Replace(".7z", "/");
            MResUpdate.ShowInitInfo(LangStyle.解压完毕, LangStyle.解压完毕);
            DirectoryInfo pack = new DirectoryInfo(dirPath);
            if(!pack.Exists)
            {
                Debug.LogError("找不到解压文件夹：" + dirPath);
                return;
            }
            string toDir = Application.persistentDataPath + "/" + Util.PlatformDir;
            FileInfo[] fls = pack.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < fls.Length; i++)
            {
                FileInfo file = fls[i];
                string[] fNames = file.Name.Split(PatchResData.Split);
                if (fNames.Length < 2)
                {
                    SendEvent("资源名错误", file.Name);
                    continue;
                }
                string outFile = toDir;
                for (int j = 1; j < fNames.Length; j++)
                {
                    outFile += "/" + fNames[j];
                }
                //string outFile = toDir + fNames[1] + "/" + fNames[2];//to + "/" + key + "_" + vData.NextVersion + "/" + url;
                string dir = Path.GetDirectoryName(outFile);
                //判断目录是否存在;
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                if (File.Exists(outFile))
                    File.Delete(outFile);
                File.Move(file.FullName, outFile);
                //fls[i].MoveTo(this.mPath + "/");
            }
            pack.Delete(true);
            File.Delete(package7zUrl);
            SendEvent("MoveFile");
            IsResUpdateOK = true;
            //IsOKDecompression = true;
        }
        else
        {
            //decompressErrorInfo = " " + errorInfo;
            Debug.LogError("解压报错:" + errorInfo);
            SendEvent("解压报错", errorInfo);
            //end();
        }
    }
    /*
     * 
    /// <summary>
    /// 写入7zip文件;
    /// </summary>
    /// <param name="bytes"></param>
    protected string write7Zip(byte[] bytes, string outPath)
    {
        string outFile = Application.persistentDataPath + "/" + outPath;
        //string dir = Path.GetDirectoryName(outFile);
        ////判断目录是否存在;
        //if (!System.IO.Directory.Exists(dir))
        //    System.IO.Directory.CreateDirectory(dir);
        //FileStream newFileStream = new FileStream(outFile, FileMode.OpenOrCreate, FileAccess.Write);
        //int arraySize = bytes.Length;
        //newFileStream.Write(bytes, 0, arraySize);
        //newFileStream.Close();
        return outFile;
    }

    /// <summary>
    /// 解压缩文件;
    /// </summary>
    /// <param name="inFile"></param>
    /// <param name="outFile"></param>
    public void decompressFile(string inFile, string outFile)
    {
        //if (!File.Exists(inFile))
        //{
        //    startDecompress(null);
        //    return;
        //}
        //string dir = Path.GetDirectoryName(outFile);
        ////判断目录是否存在;
        //if (!System.IO.Directory.Exists(dir))
        //    System.IO.Directory.CreateDirectory(dir);
        //SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
        //FileStream input = new FileStream(inFile, FileMode.Open);
        //FileStream output = new FileStream(outFile, FileMode.Create);

        //// Read the decoder properties
        //byte[] properties = new byte[5];
        //input.Read(properties, 0, 5);

        //// Read in the decompress file size.
        //byte[] fileLengthBytes = new byte[8];
        //input.Read(fileLengthBytes, 0, 8);
        //long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

        //// Decompress the file.
        //coder.SetDecoderProperties(properties);
        //coder.Code(input, output, input.Length, fileLength, null);
        //output.Flush();
        //output.Close();
        //input.Close();
        //delZipFile(inFile);
        //TimerManager.doOnce(0.1f, startDecompress);
    }

    /// <summary>
    /// 读取服务器返回的版本号;
    /// </summary>
    public void readNetVersion()
    {
        //float rnd = UnityEngine.Random.Range(0.1f, 1.0f);
        //data.Url = "http://" + serverPath + "version.txt" + "?r=" + rnd;
        ABLoaderInfo data = new ABLoaderInfo("version.txt?v=" + rnd, true);
        Debug.Log("请求版本号：" + data.Url);
        data.ErrorFun = onLoadVersionError;
        data.CompleteFun = onLoadVersionComplete;
        BundleManager.Instance.LoadResource(data);
    }
    //public QGame.GateUrl GateUrl = null;

    /// <summary>
    /// 加载网络版本号错误;
    /// </summary>
    /// <param name="param"></param>
    protected void onLoadVersionError(ABLoaderInfo data)
    {
        Debug.LogError("请求网络版本号错误： 地址:" + data.Url);
        MResUpdate.ShowPopup(LangStyle.版本号下载异常, LangStyle.版本号下载异常info + "\n" + data.www.error, QuitGame, QuitGame);
        //IsResUpdateOK = true;
    }
    //public VersionConfig Config = null;
    /// <summary>
    /// 加载网络版本号成功;
    /// </summary>
    /// <param name="param"></param>
    protected void onLoadVersionComplete(ABLoaderInfo data)
    {
        WWW www = data.www as WWW;
        string str = (www.text as string).Trim();
        //Config = JsonUtility.FromJson<VersionConfig>(str);
        //BundleManager.ServerRes = Config.PatchUrl;
        vaildCheck();
    }

    /// <summary>
    /// 重新解压
    /// </summary>
    void ReDecompression()
    {
        IsStartDecompression = true;
    }
     * */
    /// <summary>
    /// 重新下载
    /// </summary>
    void ReDownload()
    {
        IsStartDownload = true;
    }
    public static void DeleteLocalCache(string temp = "")
    {
        string localUrl = Application.persistentDataPath + "/" + highlight.Util.PlatformDir;
        if (System.IO.Directory.Exists(localUrl))
        {
            System.IO.Directory.Delete(localUrl, true);
            System.IO.Directory.CreateDirectory(localUrl);
        }
        Debug.Log("删除本地缓存资源。" + localUrl);
        DeleteLocal7z();
    }
    public static void DeleteLocal7z()
    {
        string localUrl = Application.persistentDataPath + "/";
        if (System.IO.Directory.Exists(localUrl))
        {
            DirectoryInfo dir = new DirectoryInfo(localUrl);
            FileInfo[] fls = dir.GetFiles("*.7z", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < fls.Length; i++)
            {
                Debug.Log("删除本地7z资源。" + fls[i].FullName);
                fls[i].Delete();
            }
        }
        
    }
    /// <summary>
    /// 退出游戏
    /// </summary>
    public static void QuitGame()
    {
        //      BuglyAgent.ConfigAutoQuitApplication(true);
        Debug.LogError("【退出游戏】");
        Application.Quit();
    }

    public static void SendStep(string info)
    {

    }

    public static void PrintFileInfo()
    {
        string path = Application.persistentDataPath;
        DirectoryInfo persistenDir = new DirectoryInfo(path);
        if(!persistenDir.Exists)
        {
            Debug.LogError("not find DirectoryInfo:" + path);
            return;
        }
        StringBuilder sb = new StringBuilder();
        FileInfo[] fls = persistenDir.GetFiles("*", SearchOption.AllDirectories);
        sb.AppendLine(path + ",num:" + fls.Length);
        for (int i = 0; i < fls.Length; i++)
        {
            FileInfo file = fls[i];
            sb.AppendLine(file.FullName.Substring(path.Length+1));
        }
        Debug.LogError(sb.ToString());
    }
}
