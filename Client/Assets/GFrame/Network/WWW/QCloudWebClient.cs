using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

[Serializable]
public class RequestInfo
{
    public RequestInfo(string url)
    {
        Url = url;
        AllowAutoRedirect = false;
    }
    public System.Object obj;
    public string Url { set; get; }
    public byte[] PostData { set; get; }
    public int length { get { return PostData == null ? 0 : PostData.Length; } }
    public int size;
    public float progerss { get { return (float)size / length; } }
    public WebHeaderCollection Headers { set; get; }
    public bool AllowAutoRedirect { set; get; }
    public Dictionary<string, string> ExternalData { set; get; }
    public int timeOut = 10000;//设置连接超时时间，默认10秒，用户可以根据具体需求适当更改timeOut的值
    public Action<RequestInfo> CompleteFun { set; get; }
    public ResponseInfo response;
    public Action<long, long> ProgressFun { set; get; }
    public Action<RequestInfo> ErrorFun { set; get; }
    public bool isEnd = false;
    public string error = "";
    public void Error()
    {
        if (ErrorFun != null)
        {
            ErrorFun(this);
        }
    }
    public void AddHeaders(string k,string v)
    {
        if (Headers == null)
            Headers = new WebHeaderCollection();
        Headers.Add(k, v);
    }
    public RequestHttpWebRequest request()
    {
        WWWHttpHelper.SendRequest(this);
        var wrequest = new RequestHttpWebRequest();
        wrequest.GetResponseAsync(this);
        return wrequest;
    }
    private float curTime = 0f;
    public float interval = 20f;
    private float lastTime = 0f;
    public bool Update(float deltaTime)
    {
        curTime += deltaTime;
        //if (curTime >= timeOut * 0.001)
        //    error = "TimeOut";
        if(!string.IsNullOrEmpty(error))
        {
            isEnd = true;
            Dispose();
            Error();
            return isEnd;
        }
        if (this.ProgressFun != null)
        {
            this.ProgressFun(this.size, allSize);
            if (curTime - lastTime > interval)
            {
                lastTime = curTime;

            }
        }
        if(this.isEnd)
        {
            if (this.CompleteFun != null)
            {
                this.CompleteFun(this);
            }
            return isEnd;
        }
        return isEnd;
    }
    private byte[] lbts = null;
    public byte[] loadBytes
    {
        get
        {
            if (lbts == null)
                lbts = response.GetBytes();
            return lbts;
        }
    }
    public int downloadSize
    {
        get
        {
            if(FStream != null)
                return (int)FStream.Length;
            return loadBytes.Length;
        }
    }
    public string saveUrl = "";
    public FileStream FStream;
    public int allSize = 0;
    public bool SetSaveUrl(string _url,int all,bool isBreakPoint = false)
    {
        saveUrl = _url;
        allSize = all;
        if (!isBreakPoint)
            return false;
        if (File.Exists(saveUrl))
        {
            FStream = File.OpenWrite(saveUrl);
            rangPoint = (int)FStream.Length;
            if (rangPoint == all)
            {//文件是完整的，直接结束下载任务
                return true;
            }
            FStream.Seek(rangPoint, SeekOrigin.Current);
        }
        else
        {
            //文件不保存创建一个文件
            FStream = new FileStream(_url, FileMode.Create);
            rangPoint = 0;
        }
        size = rangPoint;
        return false;
    }
    public int rangPoint = 0;
    public void Save()
    {
        if (FStream == null)
            highlight.FileUtils.WriteFileStream(loadBytes, saveUrl);
        Dispose();
    }
    public void Dispose()
    {
        if (FStream != null)
        {
            FStream.Close();
            FStream.Dispose();
        }
        if (response.ResponseContent != null)
            response.ResponseContent.Dispose();
    }
}

[Serializable]
public class ResponseInfo
{
    public RequestInfo RequestInfo { set; get; }
    public MemoryStream ResponseContent { set; get; }
    public HttpStatusCode StatusCode { set; get; }
    public WebHeaderCollection Headers { set; get; }
    public ResponseInfo(RequestInfo eInfo)
    {
        ResponseContent = new MemoryStream();
        RequestInfo = eInfo;
        eInfo.response = this;
    }
    public byte[] GetBytes()
    {
        return ResponseContent.ToArray();
    }
}

internal class StateObject
{
    //public byte[] Buffer { set; get; }
    public ResponseInfo ResponseInfo { set; get; }
    public Stream ReadStream { set; get; }
    public HttpWebRequest HttpWebRequest { set; get; }

}

public class RequestHttpWebRequest
{
    public static byte[] Buffer = new byte[1024 * 1024];
    static RequestHttpWebRequest()
    {
        ServicePointManager.DefaultConnectionLimit = 100;
    }
    public RequestHttpWebRequest()
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
    }
    private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
    {
        //直接确认，否则打不开  
        return true;
    }
    public HttpWebRequest GetResponseAsync(RequestInfo info)
    {
        HttpWebRequest webRequest=null;
        StateObject state = null;
        InitWebRequest(info, out webRequest, out state);
        try
        {
            if (info.PostData != null && info.PostData.Length > 0)
            {
                webRequest.Method = "PUT";

                webRequest.ContentType = "multipart/form-data";
               // webRequest.ContentType = info.ContentType;// "application/x-www-form-urlencoded";
                webRequest.ContentLength = info.length;
                string sendStr = webRequest.Method + " " + webRequest.RequestUri.LocalPath + "\n" + "Host:" + webRequest.RequestUri.Host + "\n";
                sendStr += webRequest.Headers.ToString();
                //UnityEngine.Debug.LogError(sendStr);
                webRequest.BeginGetRequestStream(EndRequest, state);
               // webRequest.Headers.Add("Host", "aoa2018cos-1251001060.cos.ap-hongkong.myqcloud.com");

                //var requestStream = webRequest.GetRequestStream();
                //requestStream.Write(info.PostData, 0, info.PostData.Length);
                //requestStream.Close();
                //var response = webRequest.GetResponse();
                //using (var s = response.GetResponseStream())
                //{
                //    var reader = new StreamReader(s, Encoding.UTF8);
                //    UnityEngine.Debug.Log(reader.ReadToEnd());
                //}
            }
            else
            {
                if (info.rangPoint > 0)
                    webRequest.AddRange(info.rangPoint);
                webRequest.BeginGetResponse(EndResponse, state);
            }

        }
        catch (Exception ex)
        {
            HandException(ex, state);
        }
        return webRequest;
    }
    void EndRequest(IAsyncResult ar)
    {
        StateObject state = ar.AsyncState as StateObject;
        try
        {
            HttpWebRequest webRequest = state.HttpWebRequest as HttpWebRequest;
            using (Stream stream = webRequest.EndGetRequestStream(ar))
            {
                byte[] data = state.ResponseInfo.RequestInfo.PostData;
                stream.Write(data, 0, data.Length);
            }
            //System.ComponentModel.BackgroundWorker
            webRequest.BeginGetResponse(EndResponse, state);

        }
        catch (Exception ex)
        {
            HandException(ex, state);
        }
    }
    void EndResponse(IAsyncResult ar)
    {
        StateObject state = ar.AsyncState as StateObject;
        try
        {
            HttpWebResponse webResponse = state.HttpWebRequest.EndGetResponse(ar) as HttpWebResponse;
            state.ResponseInfo.StatusCode = webResponse.StatusCode;
            state.ResponseInfo.Headers = new WebHeaderCollection();
            foreach (string key in webResponse.Headers.AllKeys)
            {
                state.ResponseInfo.Headers.Add(key, webResponse.Headers[key]);
            }
            state.ReadStream = webResponse.GetResponseStream();
            state.ReadStream.BeginRead(Buffer, 0, Buffer.Length, ReadCallBack, state);
        }
        catch (Exception ex)
        {
            HandException(ex, state);

        }
    }
    void ReadCallBack(IAsyncResult ar)
    {
        StateObject state = ar.AsyncState as StateObject;
        try
        {
            RequestInfo info = state.ResponseInfo.RequestInfo;
            int read = state.ReadStream.EndRead(ar);
            info.size += read;
           
            if (read > 0)
            {
                if (info.FStream != null)
                    info.FStream.Write(Buffer, 0, read);
                else
                    state.ResponseInfo.ResponseContent.Write(Buffer, 0, read);
                state.ReadStream.BeginRead(Buffer, 0, Buffer.Length, ReadCallBack, state);
            }
            else
            {
                state.ReadStream.Close();
                state.HttpWebRequest.Abort();
                info.isEnd = true;
            }
        }
        catch (Exception ex)
        {
            HandException(ex, state);
        }
    }
    private void InitWebRequest(RequestInfo info, out HttpWebRequest webRequest, out StateObject state)
    {

        webRequest = HttpWebRequest.CreateDefault(new Uri(info.Url)) as HttpWebRequest;
        //webRequest.Accept = "*/*";
        webRequest.KeepAlive = true;
        webRequest.AllowAutoRedirect = info.AllowAutoRedirect;
        webRequest.Timeout = info.timeOut;
        //webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1;SV1)";

        if (info.Headers != null && info.Headers.Count > 0)
        {
            foreach (string key in info.Headers.Keys)
            {
                webRequest.Headers.Add(key, info.Headers[key]);
            }
        }
        //webRequest.Proxy = WebProxy.GetDefaultProxy();
        //webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;  
        // webResponse.Headers.Get("Set-Cookie");
        ResponseInfo respon = new ResponseInfo(info);
        state = new StateObject {
            //Buffer = new byte[1024 * 1024],
            HttpWebRequest = webRequest,
            ResponseInfo = respon
        };
    }

    private void HandException(Exception ex, StateObject state)
    {
        string message = "";
        if(ex is System.Net.WebException)
        {
            message += (ex as System.Net.WebException).Status.ToString() + ",";
            //WebResponse resp = (ex as System.Net.WebException).Response;
           // UnityEngine.Debug.LogError(resp.Headers.ToString());
        }
        RequestInfo info = state.ResponseInfo.RequestInfo;
        message += ex.Message;
        UnityEngine.Debug.LogError(state.ResponseInfo.RequestInfo.Url + " : " + message + "\n" + ex.StackTrace);
        info.error = message;
        //Console.WriteLine(message);
    }
}