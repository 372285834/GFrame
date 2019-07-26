using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using SDK;
using System.Collections.Generic;
public class MIOSSDK : MonoBehaviour
{
    /// <summary>
    /// 进入游戏调用登录
    /// </summary>
    public void onInitSucc()
    {
        log("初始化成功");
        QGameSDK.Instance.OnInitCall();
    }

    /// <summary>
    /// 初始化失败游戏
    /// </summary>
    /// <param name="msg">Message.</param>
    public void onInitFailed(string msg)
    {
        Debug.LogError("初始化失败：" + msg);
    }

    /// <summary>
    /// 登录成功
    /// </summary>
    /// <param name="sid">Sid.</param>
    public void onLoginSucc(string msg)
    {
        log("账号登录成功 msg:" + msg);
        Dictionary<string, string> dic = ParseMsg(msg);
        string sid = "";
        string token = "";
        eChannel eType = VersionManager.Instance.mStyle.Channel;
        if (dic != null)
        {
            if (eType == eChannel.YOUKA_iOS)
            {
                dic.TryGetValue("sid", out sid);
                dic.TryGetValue("token", out token);
            }
        }
        QGameSDK.Instance.LoginCallbackToLua(sid, token, true);
    }

    /// <summary>
    /// 登录界面退出，返回到游戏画面
    /// </summary>
    /// <param name="msg">Message.</param>
    public void onLoginFailed(string msg)
    {
        Debug.LogError("账号登录失败：" + msg);
        QGameSDK.Instance.LoginCallbackToLua("", "", false, msg);
    }

    /// <summary>
    /// 当前登录用户已退出，应将游戏切换到未登录的状态。
    /// </summary>
    public void onLogoutSucc()
    {
        log("账号退出成功");
        QGameSDK.Instance.LoginOutCallBack(true);
    }

    /// <summary>
    /// 退出失败
    /// </summary>
    /// <param name="msg">Message.</param>
    public void onLogoutFailed(string msg)
    {
        Debug.LogError("账号退出失败：" + msg);
        QGameSDK.Instance.LoginOutCallBack(false, msg);
    }

    /// <summary>
    /// 退出游戏成功
    /// </summary>
    public void onExitSucc()
    {
        log("退出游戏成功");
        Application.Quit();
    }

    /// <summary>
    /// 用户取消退出游戏
    /// </summary>
    /// <param name="msg">Message.</param>
    public void onExitCanceled(string msg)
    {
        log("退出游戏失败：" + msg);
    }

    /// <summary>
    /// 创建订单成功
    /// </summary>
    /// <param name="orderInfo">Order info.</param>
    public void onCreateOrderSucc(string orderInfo)
    {
        log("支付订单成功：订单号=" + orderInfo);
        Dictionary<string, string> dic = ParseMsg(orderInfo);
        string orderId = "";
        if (dic != null)
            dic.TryGetValue("orderId", out orderId);
        QGameSDK.Instance.OnPayCall("", orderId, QGameSDK.PayResult.Success);
    }

    /// <summary>
    /// 用户取消订单支付
    /// </summary>
    /// <param name="orderInfo">Order info.</param>
    public void onPayUserExit(string orderInfo)
    {
        log("用户取消支付：订单号=" + orderInfo);
        Dictionary<string, string> dic = ParseMsg(orderInfo);
        string orderId = "";
        if (dic != null)
            dic.TryGetValue("orderId", out orderId);
        QGameSDK.Instance.OnPayCall("", orderId, QGameSDK.PayResult.Cancel);
    }


    public static string GameObjectName = "_MIOSSDK_";
#if UNITY_IPHONE
    //代码区A BEGIN
    [DllImport("__Internal")]
    private static extern void __SetDebugMode();
    [DllImport("__Internal")]
    private static extern void __InitSDK(string appid, string appkey);
    [DllImport("__Internal")]
    private static extern void __Login();
    [DllImport("__Internal")]
    private static extern void __LogOut();
    [DllImport("__Internal")]
    private static extern void __Pay(string aOrderId, string aProductId, string aProductName, string aPrice, string aNumber, string sign,string serverId,string charId);
    [DllImport("__Internal")]
    private static extern void __SubmitRoleData(string serverid, string serverName, string charId, string roleName, string roleLevel, string time);
    [DllImport("__Internal")]
    private static extern void __ShowFloatWindow(bool b);
#endif
    //代码区A END
    public static void init(string appid, string appkey)
    {
        GameObject go = new GameObject(GameObjectName);
        go.AddComponent<MIOSSDK>();
#if UNITY_IPHONE
        __InitSDK(appid, appkey);
#endif
    }

    /// <summary>
    /// 调用SDK的用户登录 
    /// </summary>
    public static void login()
    {
#if UNITY_IPHONE
        __Login();
#endif
    }

    /// <summary>
    /// 退出当前登录的账号
    /// </summary>
    public static void logout()
    {
#if UNITY_IPHONE
        __LogOut();
#endif
    }
    /// <summary>
    /// 设置玩家选择的游戏分区及角色信息 
    /// </summary>
    public static void submitRoleData(SDK.RoleInfo info)//string roleId, string name, long lv, long roleCTime, string serverid, string serverName)
    {
#if UNITY_IPHONE
        __SubmitRoleData(info.sId, info.sName, info.id, info.roleName, info.lv.ToString(), info.creatTime.ToString());
#endif
    }
    /// <summary>
    /// 支付
    /// </summary>
    public static void pay(SDK.PayInfo info, SDK.RoleInfo role)
    {
#if UNITY_IPHONE
        __Pay(info.aOrderId, info.aProductId, info.aProductName, info.aPrice.ToString(), info.aNumber.ToString(), info.norifyuri, role.sId, role.id);
#endif
    }
    public static void showFloatWindow(bool b)
    {
#if UNITY_IPHONE
        __ShowFloatWindow(b);
#endif
    }
    public static void showPage()
    {

    }
    /// <summary>
    /// 退出SDK，游戏退出前必须调用此方法，以清理SDK占用的系统资源。如果游戏退出时不调用该方法，可能会引起程序错误。
    /// </summary>
    public static void exitSDK()
    {

    }

    private static void callSdkApi(string apiName, params object[] args)
    {
        log("Unity3D " + apiName + " calling...");

    }
    public static void log(string msg)
    {
        Debug.Log(msg);
        //Text mText = GameObject.Find("MsgText").GetComponent<Text>();
        //if (msgText != null)
        //{
        //    msgText.text = msgText.text + System.Environment.NewLine + msg;
        //}
    }

    static Dictionary<string, string> ParseMsg(string msg)
    {
        if (null == msg || 0 == msg.Length)
        {
            return null;
        }
        Dictionary<string, string> dicMsg = new Dictionary<string, string>();
        string[] msgArray = msg.Split('&');
        for (int i = 0; i < msgArray.Length; i++)
        {
            string[] elementArray = msgArray[i].Split('=');
            dicMsg.Add(elementArray[0], elementArray[1]);
        }
        return dicMsg;
    }
}
