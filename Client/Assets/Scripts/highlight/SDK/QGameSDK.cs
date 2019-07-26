using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using highlight;
using XLua;

namespace SDK
{

    public class RoleInfo
    {
        public string id;
        public string roleName;
        public string sId;
        public string sName;
        public long lv = 1;
        public long creatTime;
        public string action = "LevelUp";
        public RoleInfo() { }
        public void SetRoleInfo(LuaTable lua)
        {
            this.id = lua["uid"].ToString();
            this.roleName = lua["roleName"].ToString();
            this.sId = lua["sid"].ToString();
            this.sName = lua["sName"].ToString();
            string level = lua["lv"].ToString();
            long.TryParse(level, out this.lv);
            string ct = lua["creatTime"].ToString();
            long.TryParse(ct, out this.creatTime);
            this.action = lua["act"].ToString();
           // if (creatTime <= 0)
           //     creatTime = (long)MDate.GetCurrentDateSec();
        }
        public new string ToString()
        {
            return string.Format("RoleInfo:{0},{1},{2},{3},{4},{5}", id, roleName, sId, sName, lv, creatTime);
        }
    }
    public class PayInfo
    {
        public string aOrderId;
        public string aProductId;
        public string aProductName;
        public float aPrice;
        public int aNumber;
        public string norifyuri;
        public string sign;
        public PayInfo(string aOrderId, string aProductId, string aProductName, float aPrice, int aNumber, string norifyuri, string sign)
        {
            this.aOrderId = aOrderId;
            this.aProductId = aProductId;
            this.aProductName = aProductName;
            this.aPrice = aPrice;
            this.aNumber = aNumber;
            this.norifyuri = norifyuri;
            this.sign = sign;
        }
    }
    /// <summary>
    /// SDK接入统一接口
    /// </summary>
    public class QGameSDK
    {
        public enum PayResult
        {
            Success = 0,
            Failed,
            Cancel,
        }

        public enum LoginResult
        {
            Login = 0,
            Switch = 1,
            Binding = 2,
        }

        public enum ExitResult
        {
            SDKEXIT = 0,
            SDKEXIT_NO_PROVIDE,
        }
        static QGameSDK _instance = null;
        private static GameObject container;

        public AcHandler Ac_Init;
        public LuaFunction loginCallback;
        public LuaFunction logoutCallback;
        public LuaFunction paymentCallback;
        public string sdkId;
        public RoleInfo roleInfo = new RoleInfo();
        public static QGameSDK Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new QGameSDK();
                    //container = new GameObject();
                    //container.name = "QGameSDK";
                    //_instance = container.AddComponent(typeof(QGameSDK)) as QGameSDK;
                }

                return _instance;
            }
        }
        private VersionStyle mStyle { get { return VersionStyle.Instance; } }
        private ChannelData data { get { return mStyle.channelData; } }
        private eChannel channel { get { return data.Channel; } }
        public string appId = "";
        public string appKey = "";
        /// <summary>
        /// 初始化sdk
        /// </summary>
        /// <param name="aCallback">初始化成功或者失败的回调</param>
        public void Init(AcHandler ac)//LuaFunction aCallback
        {
            if (Application.isEditor)
            {
                ac();
                return;
            }
            Ac_Init = ac;
            VersionStyle style = VersionManager.Instance.mStyle;
            appId = style.getAppId();
            appKey = style.getAppKey();
            Debug.Log("SDKInit >>>>>>>>>1:" + channel.ToString() + "," + appId + "," + appKey);
            string privateKey = style.getPrivateKey();
            switch (channel)
            {
                case eChannel.YOUKA_Android:
                    MyUnityAndroidSDK.init(channel, appId, appKey, privateKey);
                    break;
                case eChannel.YOUKA_iOS:
                    MIOSSDK.init(appId, appKey);
                    break;
                default:
                    Ac_Init();
                    break;
            }
        }

        /// <summary>
        /// 调用sdk登陆
        /// </summary>
        /// <param name="aCallback">登陆成功或失败后的回调</param>
        public bool Login(int aType = 1)
        {
            eChannel tmp = this.channel;
            Debug.Log("Login >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>:" + tmp.ToString() + ", " + aType);
            if (!isSupportedLogin())
                return false;
            switch (tmp)
            {
                case eChannel.YOUKA_Android:
                    MyUnityAndroidSDK.login("{'yokaAreaId':'" + aType + "'}");
                    break;
                case eChannel.YOUKA_iOS:
                    MIOSSDK.login();
                    break;
                default:
                    break;
            }
            return true;
        }

        /// <summary>
        /// 注销
        /// </summary>
        public bool Logout()
        {
            if (!this.isSupportedLogOut())
                return false;
            Debug.Log("Logout>>>>>>>> " + VersionManager.Instance.mStyle.channelId);
            switch (VersionManager.Instance.mStyle.Channel)
            {
                case eChannel.YOUKA_Android:
                    MyUnityAndroidSDK.logout();
                    break;
                case eChannel.YOUKA_iOS:
                    break;
                default:
                    break;
            }
            return true;
        }
        public bool isSupportedLogin()
        {
            if (Application.isEditor || data == null)
                return false;
            return data.isSupportedLogin;
           // else if (channel == eChannel.SuperSDK)
          //      b = supersdkUnity.SuperSDKUnity3D.isSupported(supersdkUnity.SuperSDKDataKeys.Function.LOGIN);
        }
        public bool isSupportedPay()
        {
            if (Application.isEditor || data == null)
                return false;
            //if (channel == eChannel.SuperSDK)
            //    b = supersdkUnity.SuperSDKUnity3D.isSupported(supersdkUnity.SuperSDKDataKeys.Function.PAY);
            return data.isSupportedPay;
        }
        public bool isSupportedLogOut()
        {
            if (Application.isEditor || data == null)
                return false;
            return data.isSupportedLogOut;
        }
        public bool hasExitDialog()
        {
            if (Application.isEditor || data == null)
                return false;
            return data.hasExitDialog;
        }
        public bool isSupportedSwitchAccount()
        {
            if (Application.isEditor || data == null)
                return false;
            return data.isSupportedSwitchAccount;
        }
        public bool isSupportedSubmitData()
        {
            if (Application.isEditor || data == null)
                return false;
            return data.isSupportedSubmitData;
        }
        public bool isSupportedFloat()
        {
            if (Application.isEditor || data == null)
                return false;
            return data.isSupportedFloat;
        }
        string lastSetDataInfo = "";
        /// <summary>
        /// 设置玩家数据
        /// </summary>
        /// <param name="aRoleId">角色id.</param>
        /// <param name="aRoleName">角色名字.</param>
        /// <param name="aLevel">角色等级.</param>
        /// <param name="roleCTime"></param>
        /// <param name="aServerId">服务器id.</param>
        /// <param name="aServerName">服务器名字.</param>
        public void SetData(LuaTable lua)
        {
            roleInfo.SetRoleInfo(lua);
            if (!isSupportedSubmitData())
                return;
            //string str = info.ToString();
            //if (str == lastSetDataInfo)
            //    return;
            //lastSetDataInfo = str;
            //Debug.Log(info.ToString());
            switch (VersionManager.Instance.mStyle.Channel)
            {
                case eChannel.YOUKA_Android:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 显示SDK用户中心
        /// </summary>
        public void ShowCenter(int type = 1)
        {
            Debug.Log("ShowCenter>>>>>>>>" + VersionManager.Instance.mStyle.channelId);
            switch (VersionManager.Instance.mStyle.Channel)
            {
                case eChannel.YOUKA_Android:
                    break;
                default:
                    break;
            }
        }
        public string curName;
        public string curPwd;
        public void Binding(string type, string _curName, string _curPwd)
        {
            curName = _curName;
            curPwd = _curPwd;
            Debug.Log("Binding>>>>>>>>" + VersionManager.Instance.mStyle.channelId);
            switch (VersionManager.Instance.mStyle.Channel)
            {
                case eChannel.YOUKA_Android:
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 切换账号
        /// </summary>
        public void SwitchAccount(string sdk)
        {
            if (!this.isSupportedSwitchAccount())
                return;
            Debug.Log("SwitchAccount>>>>>>>>" + VersionManager.Instance.mStyle.channelId);
            switch (VersionManager.Instance.mStyle.Channel)
            {
                case eChannel.YOUKA_Android:

                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 显示暂停页面，在OnApplicationPause()中调用
        /// </summary>
        public void ShowPausePage()
        {
            //Debug.Log("ShowPausePage>>>>>>>>" + VersionManager.Instance.mStyle.channelId);
            //switch (VersionManager.Instance.mStyle.Channel)
            //{

            //}
        }
        static string defaultNorifyuri = "http://www.nbrpg.com:8081/pay/yh";
        /// <summary>
        /// 购买内购商品
        /// </summary>
        /// <param name="aOrderId">订单号</param>
        /// <param name="aProductId">产品id</param>
        /// <param name="aProductName">产品名称</param>
        /// <param name="aPrice">价格</param>
        /// <param name="aNumber">购买数量</param>
        public bool Payment(string aOrderId, string aProductId, string aProductName, float aPrice, int aNumber)  /// <param name="payDes">服务器透传值</param>
        {
            if (!isSupportedPay())
                return false;
            VersionStyle style = VersionManager.Instance.mStyle;
            string norifyuri = style.GetJsonInfo("notifyuri");
            if (string.IsNullOrEmpty(norifyuri))
                norifyuri = defaultNorifyuri;
            string sign = style.GetJsonInfo("sign");
            Debug.Log("Payment>>>>>>>>:" + aOrderId + "," + aProductId + "," + aProductName + "," + aPrice + "," + aNumber + "," + norifyuri + "," + sign);
            PayInfo info = new PayInfo(aOrderId, aProductId, aProductName, aPrice, aNumber, norifyuri, sign);
            switch (channel)
            {
                case eChannel.YOUKA_Android:
                    MyUnityAndroidSDK.pay(info, this.roleInfo);
                    break;
                case eChannel.IAP:
                    IAPManager.Inst.Payment(info);
                    break;
                case eChannel.YOUKA_iOS:
                    MIOSSDK.pay(info,this.roleInfo);
                    break;
                default:
                    break;
            }
            return true;
        }

        public void ShowFloatWindow(bool b)
        {
            if (!isSupportedFloat())
                return;
            switch (VersionManager.Instance.mStyle.Channel)
            {
                case eChannel.YOUKA_Android:

                    break;
                case eChannel.IAP:
                case eChannel.YOUKA_iOS:
                    MIOSSDK.showFloatWindow(b);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 使用sdk的退出功能，返回false必须调用我们自己的退出功能
        /// </summary>
        public bool ExitApp()
        {
            if (Application.isEditor)
                return false;
            Debug.Log("ExitApp>>>>>>>>" + VersionManager.Instance.mStyle.channelId);
            bool b = false;
            switch (VersionManager.Instance.mStyle.Channel)
            {
                case eChannel.YOUKA_Android:

                    b = true;
                    break;
                case eChannel.SuperSDK:

                    break;
                default:
                    break;
            }
            return b;
        }
        public void OnInitCall()
        {
            if (this.Ac_Init != null)
                this.Ac_Init();
        }
        public void OnPayCall(string iosInfo, string ggInfo, PayResult result, string signData = "", string signature = "")
        {
            if (paymentCallback != null)
                paymentCallback.Call(iosInfo, ggInfo, (int)result, signData, signature);
        }
        public void LoginOutCallBack(bool isSuccess, string errStr = "")
        {
            if (this.logoutCallback != null)
            {
                this.logoutCallback.Call(isSuccess,errStr);
            }
        }
        public void LoginCallbackToLua(string sid, string token, bool isSuccess, string errStr = "")
        {
            LoginResultCallBack(LoginResult.Login, sid, token, isSuccess, errStr);
        }
        /// <summary>
        /// 登陆回调到lua
        /// </summary>
        public void LoginResultCallBack(LoginResult aType,string sid, string token, bool isSuccess, string errStr = "")
        {
            SDKUser user = null;
            if (isSuccess)
            {
                sdkId = sid;
                user = new SDKUser(sid, token, appId, appKey);
                user.curName = curName;
                user.curPwd = curPwd;
                user.isSuccess = isSuccess;
                user.errStr = errStr;
                user.aType = aType;
            }
            if(loginCallback != null)
            {
                loginCallback.Call((int)aType, user, isSuccess, errStr);
            }
           // GameManager.LuaCallMethod("UserConfig.LoginResultCallBack", (int)aType, jsonStr, isSuccess, errStr);
        }
        private Dictionary<string, string> kvDic = new Dictionary<string, string>();
        public string getUserDefinedValue(string key)
        {
            if (Application.isEditor)
                return "";
            if (kvDic.ContainsKey(key))
                return kvDic[key];
            string value = "";
            kvDic[key] = value;
            return value;
        }
    }
}
