using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChannelData : ScriptableObject
{
    public bool isHide;
    public ePlatform platform;
    public eChannel Channel = eChannel.None;
    public bool IsSDKLogin = true;
    public string appid;
    public string appkey;
    public string privatekey;
    public string bundleDisplayName;
    public string bundleName;
    public bool isSupportedLogin = true;
    public bool isSupportedLogOut = true;
    public bool isSupportedPay = true;
    public bool hasExitDialog = true;
    public bool isSupportedSwitchAccount = true;
    public bool isSupportedSubmitData = true;
    public bool isSupportedFloat = true;
}
