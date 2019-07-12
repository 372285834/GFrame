using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using System.Text;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

public class WWWHttpData
{
    // Based on https://www.owasp.org/index.php/Certificate_and_Public_Key_Pinning#.Net
    //https://answers.unity.com/questions/1479862/unitywebrequest-tomcat-redirect-and-self-signed-ss.html
    class AcceptAllCertificatesSignedWithASpecificPublicKey : CertificateHandler
    {
        // Encoded RSAPublicKey
        //private static string PUB_KEY = "mypublickey";
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            //X509Certificate2 certificate = new X509Certificate2(certificateData);
            //string pk = certificate.GetPublicKeyString();
            //if (pk.ToLower().Equals(PUB_KEY.ToLower()))
            //{
            //    return true;
            //}
            //return false;
            Debug.Log("验证强制通过：" + certificateData.Length);
            return true;
        }
    }
    public enum WWWKeyType
    {
        MD=5,
        MD5_KEY=0,
        YOUKA=4,
        KP=6,
    }
    public long dnsTime = 0;
    public WWWHttpData(string ajson, string _url)
    {
        json = ajson;
        url = _url;
        defUrl = "";
    }
    public bool IsJson = true;
    public Action<WWWHttpData> aCallback = null;
    //public MBinaryReader mBinary = null;
    public string json = "";
    public string url = "";
    private string mDefUrl;
    public string defUrl 
    { 
        get { return mDefUrl;} 
        set
        { 
            if(string.IsNullOrEmpty(value))
            {
                mDefUrl = "";
                return;
            }
            Uri uri = new Uri(url);
            mDefUrl = url.Replace(uri.Authority, value);
        } 
    }
    public string Message = "";
    public bool isError = false;
    public float TimeOut = 15f;
    public float delayTime = 0f;
    public string error = "";
    public UnityWebRequest mWWW = null;
    public bool IsDispose = false;
    public bool isToGet = false;
    public WWWForm GetFrom()
    {
        WWWForm form = new WWWForm();
        if (!IsJson)
        {
            //form.AddField("Content-Type", "; charset=UTF-8");
            form.AddField("key", json, System.Text.Encoding.UTF8);
        }
        else
        {
            form.AddField("data", json, System.Text.Encoding.UTF8);
        }
        return form;
    }
    public UnityWebRequest CreatWWW(string _url)
    {
        if(string.IsNullOrEmpty(json))
        {
            mWWW = UnityWebRequest.Get(_url);
        }
        else
        {
            mWWW = UnityWebRequest.Post(_url, GetFrom());
        }
        mWWW.certificateHandler = new AcceptAllCertificatesSignedWithASpecificPublicKey();
        mWWW.timeout = (int)this.TimeOut;
        mWWW.SendWebRequest();
        return mWWW;
    }
    public long time = 0;
    private long tempTime;
    public PingData ping = null;
    public void Update()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            if (delayTime > 0)
            {
                delayTime -= Time.deltaTime;
                return;
            }
        }
        if (ping == null)
            ping = new PingData(url, defUrl);
        //if (!ping.Update())
        //    return;
        
        if (mWWW == null)
        {
            Debug.Log("WWW:【" + ping.finalUrl + "】:" + ping.dnsIp + "     " + SystemInfoUtil.GetIPDns2() + "\n" + this.json);
            tempTime = System.DateTime.Now.Ticks;
            try
            {
               // GoogleAnsSdk.LogEvent("短连Start", ping.finalUrl, json, 0, ping.dnsIp);
                //GoogleAnsSdk.LogEvent("Start", ipUrl, ping.dnsIp);
                CreatWWW(ping.finalUrl);
              //  RequestInfo reInfo = new RequestInfo(ping.finalUrl);
              //  reInfo.CompleteFun = delegate (RequestInfo rif) { Debug.Log(rif.text); };
              //  reInfo.request();
            }
            catch(Exception e)
            {
               // ping.ClearPrefs();
                string err = "CreatWWWError:" + e.Message;
                Debug.LogError(err);
                this.SetMessage(err, true);
                excCallBakc();
                return;
            }
        }
        this.TimeOut -= Time.deltaTime;
        bool isEnd = false;
        if (mWWW.isHttpError || mWWW.isNetworkError || !string.IsNullOrEmpty(mWWW.error))
        {
            Debug.LogError("responseCode:" + mWWW.responseCode);
            if (IsTimeOut())
            {
                if (!string.IsNullOrEmpty(ping.defIp) && ping.finalUrl != ping.defIp)
                {
                    Debug.Log("超时:" + ping.finalUrl);
                    SendEvent("TimeOut1");
                    ping.finalUrl = ping.defIp;
                    //ping = null;
                    mWWW = null;
                    TimeOut = 10f;
                    return;
                }
                isEnd = true;
                this.SetMessage("TimeOut," + mWWW.error, true);
            }
            else
            {
                isEnd = true;
                this.SetMessage("error:" + mWWW.error, true);
            }
        }
        else if (mWWW.isDone)
        {
            isEnd = true;
            //System.IO.MemoryStream ms = new System.IO.MemoryStream(mWWW.bytes);
            //this.mBinary = new MBinaryReader(ms);
            string msg = GetUTF8String(mWWW.downloadHandler.data);
            if (string.IsNullOrEmpty(msg))
                msg = "error_ok:" + mWWW.downloadHandler.text;
            this.SetMessage(msg, false);
        }
        if(isEnd)
        {
            if (string.IsNullOrEmpty(Message))
                Message = "";
            if (this.isError)
            {
                Debug.LogError("【return error】:" + this.Message);
            }
            else
            {
                time = (System.DateTime.Now.Ticks - tempTime) / 10000;
                Debug.Log("【return:" + time + "】:\n" + this.Message);
                //   GoogleAnsSdk.LogWWWEvent(ping.finalUrl, ping.defIp + " " + this.Message, time, ping.dnsIp);
            }
            excCallBakc();
        }
    }
    public bool IsTimeOut()
    {
        //if (mWWW.url == "http://all-get-aoa.ldoverseas.com/gate/gs")
        //    return true;
        //if (mWWW.url != defUrl && !string.IsNullOrEmpty(defUrl))
        //    return true;
        return this.TimeOut <= 0f;
    }
    
    public void SetMessage(string content, bool _isError)
    {
        if (_isError || content.Contains("return_message") || string.IsNullOrEmpty(content))
        {
          //  ping.ClearPrefs();
            content = SendEvent(content);
            string ipUrl = ping.finalUrl + " " + ping.dnsIp + " " + SystemInfoUtil.deviceUniqueIdentifier;
            error = content + " " + ipUrl + "\n" + SystemInfoUtil.GetIPDns2();
            isError = true;
            Message = error;
            //excCallBakc(content, true);
            return;
        }
        bool b = false;
        string msg = content;
        if (IsJson)
        {
            try
            {
                //解密
                string responseJson = content;// MUtil.DesDecrypt(content, SECRET_KEY(isToGet));
                //Debug.Log(responseJson);
                msg = responseJson;
            }
            catch (Exception e)
            {
                b = true;
                error = "【DesError】:" + content + ",url:" + url + "\n" + this.json;
            }
        }

        isError = b;
        Message = msg;
        //excCallBakc(msg,b);
    }
    string SendEvent(string content)
    {
        content += "," + (int)Application.internetReachability + ",";
        if (!MUtil.NetAvailable)
            content += "【没有网络】";
        else if (ping.isErrorDNS)
            content += "【ErrorDNS】";
        if (ping.finalUrl == this.defUrl)
            content += "【DefUrl】";
        string ipUrl = ping.finalUrl + " " + ping.dnsIp;
      //  GoogleAnsSdk.LogWWWException(content, ipUrl, ping.dnsIp);
        return content;
    }
    void excCallBakc()
    {
      //  if (!isError && mBinary != null)
      //      mBinary.SetMsg(Message);
        if (aCallback != null)
            aCallback(this);
        aCallback = null;
        Close();
    }
    void Close()
    {
       // if (mBinary != null)
      //      mBinary = null;
        if (mWWW != null)
        {
            mWWW.Dispose();
            mWWW = null;
        }
        IsDispose = true;
    }
    /// <summary>
    /// 去除文件bom头后的字符
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static string GetUTF8String(byte[] buffer)
    {
        if (buffer == null)
            return null;

        if (buffer.Length <= 3)
        {
            return Encoding.UTF8.GetString(buffer);
        }

        byte[] bomBuffer = new byte[] { 0xef, 0xbb, 0xbf };

        if (buffer[0] == bomBuffer[0]
          && buffer[1] == bomBuffer[1]
          && buffer[2] == bomBuffer[2])
        {
            return new UTF8Encoding(false).GetString(buffer, 3, buffer.Length - 3);
        }

        return Encoding.UTF8.GetString(buffer);
    }
}
