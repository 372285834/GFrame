/*资源更新类
 * 1.游戏启动后第一个界面
 * 2.显示游戏的初始化进度
 * 3.检测资源版本
 * 4.进入游戏
 */



using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.IO;

public class MResUpdate : MonoBehaviour {
    public Camera mCamera;
    public Text tfTile;
    public Image bgPanel;
    public Slider mSlider;
    public Text TxtSlider;
    public Text TxtTitle;
    public Text TxtInfo;
    //public MSlider mSlider;
    public GameObject PopupObj;
	public GameObject GoBTConfirm;
	public GameObject GoBTClose;
    public Button mUpdateBtn;
    public Text PopupTitle;
    public Text PopupInfo;
    //public GameObject SendObj;
    //public Text SendInfo;
    public Button btnCheat;
    public InputField inputNum;
    public Text tfWifi;

    public GameObject WaitIcon;
    public Text tfWait;
    static Action mOkCallback;
    static Action mCloseCallback;

    static string StrTitle;
    static string StrInfo;
    static string StrTitlePopup;
    static string StrInfoPopup;
    static bool IsShowInitInfo = false;
    static bool IsShowPopup = false;
   // public static bool IsSendStart = false;
    //public static bool IsSendStop = false;
    public Text tVersion;
    public Text tQaButton;
    public Button mHelpShiftBtn;

    public Text tfcdTime;
    //public static bool IsGateClose = false; //是否是Gate关闭的状态 
    public static MResUpdate Instance;
    public static float cdTime = -100f;
    public static float totalTime = 100f;
    [HideInInspector]
    public static float progressValue = 0f;
    public static void AddProgress(float v,float max)
    {
        if(progressValue < max)
        {
            progressValue += v;
            if (progressValue > max)
                progressValue = max;
        }
    }
    static VersionStyle mStyle { get { return VersionManager.Instance.mStyle; } }
    // Use this for initialization
    void Awake() {
        mCamera.depth = 100;
        Instance = this;
        this.PopupObj.SetActive(false);
        this.mSlider.gameObject.SetActive(false);
        ShowWait(true);
    }
    void Start()
    {
		Instance = this;
        cdTime = -100f;
        //UpdateLangStyle Lang = VersionManager.Instance.LangStyle;
        this.tfWait.text = MUtil.NetAvailable ? "初始化网络" : "没有网络";
        //this.tfWifi.text = Lang.wifi;
	}
    bool isInit = false;
    public void Init()
    {
        if (isInit)
            return;
        isInit = true;
        this.mSlider.gameObject.SetActive(true);
        ShowWait(false);
    }
    public void SetState(VersionStyle.eServerState state)
    {
        ShowWait(false);
    }
    public void EnterGame()
    {
        this.mSlider.gameObject.SetActive(false);
        ShowWait(true);
        this.tfWait.text = "进入游戏";
    }
    public static void ShowWait(bool b)
    {
        if (Instance == null)
            return;
        Instance.WaitIcon.SetActive(b);
    }
    public void onClickUpdate()
    {
        MUtil.SetActiveSelf(PopupObj, false);
        VersionManager.Instance.RequestGet();
    }
    public static void SetCdTime()
    {
        if (Instance == null)
            return;
        DateTime dtEnd;
        DateTime dtStart;
        DateTime dtCur;
        DateTime.TryParse(mStyle.StartTime, out dtStart);
        string sTime = mStyle.GetJsonInfo("ServerTime");
        if (!DateTime.TryParse(sTime, out dtCur))
            dtCur = System.DateTime.Now;
        if (DateTime.TryParse(mStyle.EndTime, out dtEnd))//"2006-4-23   12:22:05"
        {
            TimeSpan ts = dtEnd - dtCur;
            cdTime = (float)ts.TotalSeconds;
            lastCDTime = cdTime + 10f;
        }
        totalTime = (float)(dtEnd - dtStart).TotalSeconds;
    }
	// Update is called once per frame(对Unity的操作全部放到Update中，防止线程操作)
    static float lastCDTime = 0;
	public void Update () 
    {
        this.tVersion.text = VersionManager.Instance.mStyle.Version + "\n" + VersionManager.Instance.mStyle.build;//VersionManager.Instance.LangStyle.版本号 + 
        if (IsShowInitInfo)//显示初始化信息
        {
            IsShowInitInfo = false;
            TxtTitle.text = StrTitle;
            TxtInfo.text = StrInfo;
        }
        if (IsShowPopup)//弹窗
        {
            mUpdateBtn.gameObject.SetActive(false);
            IsShowPopup = false;
            PopupTitle.text = StrTitlePopup;
            PopupInfo.text = StrInfoPopup;
            //tfcdReward.text = Lang.累计补偿;
            MUtil.SetActiveSelf(PopupObj, true);
            // 如果是GATE维护状态则隐藏掉关闭按钮，将确认按钮居中显示
            MUtil.SetActiveSelf(this.GoBTConfirm, mOkCallback != null);
            MUtil.SetActiveSelf(this.GoBTClose, mCloseCallback != null);
            this.tfWifi.gameObject.SetActive(false);
            tfcdTime.gameObject.SetActive(cdTime >= 0f);
        }

        if(PopupObj.activeInHierarchy)
        {
            if(!VersionManager.Instance.mStyle.IsNormal())
            {
                if(cdTime > 0f)
                {
                    cdTime -= Time.deltaTime;
                    if (cdTime <= 0f)
                        cdTime = 0f;
                    if (lastCDTime >= cdTime + 1f)
                    {
                        lastCDTime = cdTime;
                        tfcdTime.text = highlight.Util.parseTimeDHMBySecond((int)cdTime);
                    }
                    
                }
                else if(cdTime > -10f)
                {
                    mUpdateBtn.gameObject.SetActive(true);
                    cdTime = -100f;
                }
            }
            
        }
        if (MResTools.IsDecompression)//正在解压文件
        {
            if (MResTools.DecompressionInfo!=null)
            {
                mSlider.value = (float)MResTools.DecompressionProgress[0] / MResTools.DecompressionInfo.count;
                TxtSlider.text = mSlider.value.ToString("p");
                //mSlider.SetValue();
            }
        }
        else
        {
            mSlider.value = progressValue;
            TxtSlider.text = mSlider.value.ToString("p");
        }
        //if (IsSendStart)
        //{
        //    IsSendStart = false;
        //    MUGUITools.SetActiveSelf(SendObj, true);
        //    time = 0;
        //    InvokeRepeating("SendTime", 0,0.5f);           
        //}
        //if (IsSendStop)
        //{
        //    IsSendStop = false;
        //    MUGUITools.SetActiveSelf(SendObj, false);
        //    CancelInvoke("SendTime");
        //}
        if (!string.IsNullOrEmpty(curInpuCode))
        {
            if(curInpuCode == "clear")
            {
                VersionManager.DeleteLocalCache();
            }
            if (curInpuCode == cheatCode || curInpuCode == "nosdk")
            {
                bool noSDK = curInpuCode == "nosdk";
                //IsCheat = true;
                MUtil.SetActiveSelf(PopupObj, false);
                this.inputNum.gameObject.SetActive(false);
                curInpuCode = "";
                if (CheatCallBack != null)
                    CheatCallBack(noSDK);
                CheatCallBack = null;
            }
        }
	}
    public void OpenQaBtn()
    {
        //HelpshiftMgr.Login(SystemInfoUtil.deviceUniqueIdentifier, "", "");
        //HelpshiftMgr.ShowConversation();
    }

    public void OnClickOK()
    {
        MUtil.SetActiveSelf(PopupObj, false);
        if (mOkCallback != null)
        {
            mOkCallback();
            mOkCallback = null;
        }
    }
    
    public void OnClickClose()
    {
        MUtil.SetActiveSelf(PopupObj, false);
        if (mCloseCallback != null)
        {
            mCloseCallback();
            mCloseCallback = null;
        }
    }
    [NonSerialized]
    public static Action<bool> CheatCallBack = null;
    int showNum = 0;
    public void ShowCheat()
    {
        showNum++;
        if(showNum >= 3)
        {
            this.inputNum.gameObject.SetActive(true);

        }
    }
    public static string cheatCode = "five";
    public static string curInpuCode = "";
    public void CheatValueChanged(string s)
    {
        curInpuCode = this.inputNum.text;
    }
    /// <summary>
    /// 关闭第一个界面
    /// </summary>
    public static void CloseFirstPanel()
    {
        if (Instance == null)
			return;
        //MUGUITools.ClearImage(mGameobject);
        DestroyImmediate(Instance.gameObject);
        Instance = null;
        //System.GC.Collect();
    }

    void OnDestroy()
    {
        //GlobalGenerator.Intance.goUpdatePanel = null;
        Instance = null;
        mOkCallback = null;
        mCloseCallback = null;

        StrTitle = null;
        StrInfo = null;
        StrTitlePopup = null;
        StrInfoPopup = null;

        IsShowInitInfo = false;
        IsShowPopup = false;

        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// 显示弹窗
    /// </summary>
    /// <param name="title"></param>
    /// <param name="info"></param>
    /// <param name="okCallback"></param>
    /// <param name="closeCallback"></param>
    public static void ShowPopup(string title, string info, Action okCallback, Action closeCallback)
    {
        mOkCallback = okCallback;
        mCloseCallback = closeCallback;
        StrTitlePopup = title;
        StrInfoPopup = info;
        IsShowPopup = true; 
    }



    /// <summary>
    /// 显示初始化信息
    /// </summary>
    /// <param name="title"></param>
    /// <param name="info"></param>
    public static void ShowInitInfo(string title, string info,float progress = 0f)
    {
        StrTitle = title;
        StrInfo = info;
        IsShowInitInfo = true;
    }

}
