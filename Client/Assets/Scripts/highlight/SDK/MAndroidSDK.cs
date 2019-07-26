using UnityEngine;
using System.Collections;
using System;
using SDK;
using System.Collections.Generic;
using Newtonsoft.Json;

public class MyUnityAndroidSDK: MonoBehaviour {

    private static string stringToEdit = "初始化 -> 登录 -> 支付";
    static AndroidJavaClass ajc_UnityPlayer = null;
    static AndroidJavaObject currentActivity = null;
    private static AndroidJavaClass _ajc_SDKCall = null;
   
    static AndroidJavaClass ajc_SDKCall
    {
    get
        {
            if(_ajc_SDKCall == null)
            {
                _ajc_SDKCall = new AndroidJavaClass("com.youka.msgame.main.UAMain");
            }
            return _ajc_SDKCall;
        }
    }
    public static void initGCloudVoiceEngine()
    {
#if UNITY_ANDROID && !UNITY_EDITOR 
        ajc_SDKCall.CallStatic("initGCloudVoiceEngine");
#endif
    }
    public static string GameObjectName = "_MAndroidSDK_";
    static int channel;
    public static void init(eChannel ec, string appid, string appkey, string privatekey)
    {
        GameObject go = new GameObject(GameObjectName);
        MyUnityAndroidSDK sdk = go.AddComponent<MyUnityAndroidSDK>();
        //获取context
        ajc_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = ajc_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        channel = (int)ec;
        string json = "{'channel':'" + channel + "','debugmode':1,'appid':'" + appid + "','appkey':'" + appkey + "','privatekey':'" + privatekey + "','islandscape':true}";
        //json = string.Format(json, channel, appid, appkey);
        ajc_SDKCall.CallStatic("uaInit", json);
    }

    /// <summary>
    /// 调用SDK的用户登录 
    /// </summary>
    public static void login(string json)
    {
        ajc_SDKCall.CallStatic("uaLogin", json);
    }

    /// <summary>
    /// 退出当前登录的账号
    /// </summary>
    public static void logout()
    {
        ajc_SDKCall.CallStatic("uaLogout");
    }
    /// <summary>
    /// 设置玩家选择的游戏分区及角色信息 
    /// </summary>
    public static void submitRoleData(string roleId, string name, long lv, long roleCTime, string serverid, string serverName)
    {
        //__SubmitRoleData(serverid, serverName, roleId, name, lv.ToString(), roleCTime.ToString());
        string json = "{'serverid':'" + serverid + "','serverName':'" + serverName + "','roleId':'" + roleId + "','name':'" + name + "','lv':'" + lv + "','roleCTime':'" + roleCTime + "'}";
        //json = string.Format(json, serverid, serverName, roleId, name, lv, roleCTime);
        ajc_SDKCall.CallStatic("uaUpUserInfo", json);
    }
    /// <summary>
    /// 支付
    /// </summary>
    public static void pay(SDK.PayInfo info, SDK.RoleInfo role)
    {
        //__Pay(aOrderId, aProductId, aProductName, aPrice.ToString(), aNumber.ToString(), sign, serverId, charId);
        string json = "{'aOrderId':'" + info.aOrderId + "','aProductId':'" + info.aProductId + "','aProductName':'" + info.aProductName
            + "','aPrice':'" + info.aPrice + "','aNumber':'" + info.aNumber + "','sign':'" + info.norifyuri + "','serverId':'" + role.sId + "','charId':'" + role.id + "'}";
        //json = string.Format(json, aOrderId, aProductId, aProductName, aPrice, aNumber, sign, serverId, charId);
        ajc_SDKCall.CallStatic("uaPay", json);
    }
    public static void showFloatWindow(bool b)
    {

    }
    public static void showPage()
    {

    }
    /// <summary>
    /// 退出SDK，游戏退出前必须调用此方法，以清理SDK占用的系统资源。如果游戏退出时不调用该方法，可能会引起程序错误。
    /// </summary>
    public static void exitSDK()
    {
        ajc_SDKCall.CallStatic("uaExit");
    }

    // -- SDK CallBack --
    //code: 0 成功，1失败
    void OnGameSdkCallback(string str)
    {
        Debug.Log("OnGameSdkCallback str is " + str);
        //Dictionary<string, string> mDic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
        MyJson.JsonNode_Object dic = MyJson.Parse(str) as MyJson.JsonNode_Object;
        string callbackType = dic.GetString("callbackType");
        string code = dic.GetString("code");
        string msg = dic.GetString("msg");
        if(code != "0")
        {
            Debug.LogError("sdkError:" + str);
        }
        if (callbackType == "init")
        {
            if (code == "0")
                QGameSDK.Instance.OnInitCall();
        }
        else if (callbackType == "login")
        {
            if (code == "0")
            {
                string sid = dic.GetString("sid");
                string token = dic.GetString("token");
                QGameSDK.Instance.LoginCallbackToLua(sid, token, true);
            }
            else
            {
                QGameSDK.Instance.LoginCallbackToLua("", "", false, msg);
            }
        }
        else if (callbackType == "logout")
        {
            if (code == "0")
            {
                QGameSDK.Instance.LoginOutCallBack(true);
            }
            else
            {
                QGameSDK.Instance.LoginOutCallBack(false, msg);
            }
        }
        else if (callbackType == "pay")
        {
            string orderId = dic.GetString("orderId");
            if (code == "0")
            {
                string signData = "";
                string signature = "";
                if (dic.ContainsKey("SigData"))
                    signData = dic.GetString("SigData");
                if (dic.ContainsKey("Signature"))
                    signature = dic.GetString("Signature");
                QGameSDK.Instance.OnPayCall("", orderId, QGameSDK.PayResult.Success, signData, signature);
            }
            else
            {
                QGameSDK.Instance.OnPayCall("", orderId, QGameSDK.PayResult.Cancel);
            }
        }
        else if (callbackType == "exit")
        {
            Application.Quit();
        }
        else if (callbackType == "IsLostOrder") //是否丢失订单
        {
            UnityEngine.Debug.Log("IsLostOrder Token: " + msg);

        }
        else if (callbackType == "RefreshToken")
        {
            //主要是firebase 的推送使用的tokn、
            UnityEngine.Debug.Log("Received Registration Token: " + msg);//  NotificationMgr.token = msg;
          //  NotificationMgr.token = msg;
        }
    }
    
    public static void Killorder(string orderid)
    { 
#if UNITY_ANDROID && !UNITY_EDITOR
        ajc_SDKCall.CallStatic("uakillorder", orderid);
#endif
    }

    //void OnGUI()  
    //{  
    //    // Logout
    //    if(GUI.Button(new Rect(100,700,450,300),"其他测试1"))
    //    {
    //        string json = "{'status':'3'}";
    //        ajc_SDKCall.CallStatic("uaLifeCycle",json);
    //    }

    //    // Pay
    //    if(GUI.Button(new Rect(600,700,450,300),"其他测试2"))
    //    {
    //        string json = "{'status':'2'}";
    //        ajc_SDKCall.CallStatic("uaLifeCycle",json);
    //    }
    //} 

    //void OnApplicationPause(bool isPause)
    //{
    //    if (isPause) {
    //        string json = "{'status':'3'}";
    //        ajc_SDKCall.CallStatic("uaLifeCycle",json);
    //    }
    //}

    //void OnApplicationFocus(bool isFocus)
    //{
    //    if (isFocus)
    //    {
    //        if (ajc_SDKCall != null){
    //            string json = "{'status':'1'}";
    //            ajc_SDKCall.CallStatic("uaLifeCycle",json);
    //            json = "{'status':'2'}";
    //            ajc_SDKCall.CallStatic("uaLifeCycle",json);
    //        }
    //    }
    //}

    //void OnApplicationQuit()
    //{
    //    string json = "{'status':'5'}";
    //    ajc_SDKCall.CallStatic("uaLifeCycle", json);
    //}
}
