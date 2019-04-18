using System.Collections;
using System.Collections.Generic;
//using System.Net.NetworkInformation;
using UnityEngine;

public class PingData
{
   // public static int TimeOut = 2000;
    //private Ping ping;
    public string url;
    public string dnsIp;
    public string defIp;
//     public string localIp
//     {
//         get { return MPrefs.GetString(url + "pingValue", this.defIp); }
//         set { MPrefs.SetString(url + "pingValue", value); }
//     }
//     public int localPingTime
//     {
//         get { return MPrefs.GetInt(url + "pingTime", TimeOut); }
//         set { MPrefs.SetString(url + "pingTime", value); }
//     }
    public int socketPort = 0;
    public string finalUrl;
    public System.Uri uri;
    //public string host { get { return uri == null ? url : uri.Host; } }
    //public long pingTime = 0;
    //public long dnsTime = 0;
    public bool isErrorDNS { get { return this.dnsIp == MUtil.ErrorDNS; } }
    //public long ticks { get { return System.DateTime.Now.Ticks; } }
    public bool IsSocket { get { return socketPort > 0; } }
    //private long tempTime = 0;
    public bool isOk = false;
    public bool isHttps { get { return finalUrl.StartsWith("https"); } }
    public PingData(string _url,string _defIp,int _socketPort = 0)
    {
        //if(_socketPort == 0)
        //    _url = _defIp;
        finalUrl = url = _url;
        defIp = _defIp;
        socketPort = _socketPort;
        isOk = true;
        //tempTime = System.DateTime.Now.Ticks;
        if(IsSocket)
        {
            dnsIp = MUtil.DNSHost2IP(url);
            uri = null;
        }
        else
        {
            try
            {
                uri = new System.Uri(url);
                //dnsIp = MUtil.DNSHost2IP(uri.Host) + ":" + uri.Port;
                dnsIp = MUtil.DNSUrl2IP(url);
            }
            catch (System.Exception)
            {
                uri = null;
                dnsIp = MUtil.ErrorDNS;
                Debug.LogError("Uri解析错误：" + url);
            }
        }
        if (!MUtil.NetAvailable)
        {
            isOk = true;
            string info = "【没有网络】";
            Debug.LogError(info);
            return;
        }
        //if (this.isErrorDNS)
        //{
        //    string category = IsSocket ? "SocketDNS【error】" : "WWWDNS【error】";
        //    UnityEngine.Debug.LogError(category);
        //    GoogleAnsSdk.LogEvent(category, host, dnsIp, 0);
        //}
        //else
        //{
        //    dnsTime = (ticks - tempTime) / 10000;
        //    string category = IsSocket ? "SocketDNS" : "WWWDNS";
        //    GoogleAnsSdk.LogEvent(category, host, dnsIp, dnsTime, dnsIp);
        //}
        //finalUrl = dnsIp;
        //if (isErrorDNS)
        //    finalUrl = localIp;
        //if (string.IsNullOrEmpty(finalUrl))
        //    finalUrl = this.url;
        //if (finalUrl.StartsWith("https"))
        //    finalUrl = url;
    }
    /*
    public bool Update()
    {
        if (isOk)
            return true;
        try
        {
            isOk = updateCheck();
        }
        catch(System.Exception e)
        {
            Debug.LogError("PingError:" + e.Message);
            isOk = true;
        }
        return isOk;
    }
    bool updateCheck()
    {
        string category = "";
        if (ping == null)
        {
            tempTime = ticks;
            if (IsSocket)
                ping = new Ping(finalUrl);
            else// if (isHttps)
            {
                System.Uri uri = new System.Uri(finalUrl);
                ping = new Ping(uri.Host);
            }
            //else
            //    ping = new Ping(finalUrl);
            return false;
        }

        if (!ping.isDone)
        {
            long pt = (ticks - tempTime) / 10000;
            if (pt < TimeOut)
                return false;
            category = IsSocket ? "PingSocket【超时】" : "PingWWW【超时】";
            GoogleAnsSdk.LogEvent(category, host, defIp, pt, dnsIp);
            Debug.LogError(category + ":" + ping.ip + ",  " + pt);
            if(!string.IsNullOrEmpty(defIp) && finalUrl != defIp)
            {
                finalUrl = defIp;
                ping = null;
                return false;
            }
        }
        pingTime = (ticks - tempTime) / 10000;
        if (pingTime >= TimeOut)
        {
            return true;
        }
        category = IsSocket ? "PingSocket" : "PingWWW";
        GoogleAnsSdk.LogEvent(category, host, defIp, pingTime, dnsIp);
        //if (pingTime < this.localPingTime)
        //{
        //    this.localPingTime = (int)pingTime;
        //    localIp = finalUrl;
        //}
        //else if (!isHttps && !string.IsNullOrEmpty(localIp))
        //{
        //    finalUrl = localIp;
        //}
        //finalUrl = this.url;
        return true;
    }
    public void ClearPrefs()
    {
        //if (localIp == this.finalUrl)
        //{
        //    localIp = "";
        //    localPingTime = TimeOut;
        //}
    }
    */
}
