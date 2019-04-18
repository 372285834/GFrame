using System;
using System.Collections.Generic;
using System.Text;
//using QCloudAPI_SDK.Center;
//using QCloudAPI_SDK.Module;
using System.IO;
using UnityEngine;
//using QCloudAPI_SDK.Common;
using System.Security.Cryptography;
using System.Linq;
using System.Net;
using System.Threading;
using System.Collections;
using UnityEngine.Networking;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Timers;
//namespace QCloudAPI_SDK
//{
    public class QCloudUpload
    {
        public string serverHost = "";
        public string serverUri = "/";
        public string secretId = "";
        public string secretKey = "";
        //public string region = "";
        public string requestMethod = "PUT";
        public string param = "";

        public string Url
        {
            get
            {
                string aUrl = "https://" + serverHost + serverUri;
                if(!string.IsNullOrEmpty(param))
                    aUrl += "?" + param;
                return aUrl;
            }
        }
        public QCloudUpload(UploadInfo info)
        {
            this.uInfo = info;
             #if UNITY_EDITOR
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
#endif
            serverHost = QQCloudMgr.uploadHost;// "cdn.api.qcloud.com";
            //serverUri = QCloudMgr.serverUri;
            //region = QQCloudMgr.region;
            secretId = "AKIDqa8Znv2eFerv47kWD6tW0KSOKSdhcWFm";
            secretKey = "TnxyM8qQq4CAwb47cyAB0fAUffXIvMdh";
        }
       // public LuaFunction callbackFinish;
       // public LuaFunction callbackProcess;
        
        public SortedDictionary<string, object> requestParams = new SortedDictionary<string, object>(StringComparer.Ordinal);
        string author = "";
        public string Authorization
        {
            get
            {
                if (string.IsNullOrEmpty(author))
                    author = QQCloudMgr.Authorization(requestMethod, this.secretId, this.secretKey, this.serverUri, this.param, this.requestParams);
                return author;
            }
        }
        public bool isSendOk = false;
       // public Init(SortedDictionary<string, object> config)
       // {
            //SortedDictionary<string, object> config = new SortedDictionary<string, object>(StringComparer.Ordinal);
           // config["SecretId"] = "你的secretId";
           // config["SecretKey"] = "你的secretKey";
            //config["RequestMethod"] = "POST";
            //config["DefaultRegion"] = "aoa2018cos-1251001060";
        //    this.setConfig(config);
        //}
        public UploadInfo uInfo;
        public void DoUpload()
        {
            uInfo.url = this.Url;
            if (uInfo.bts == null || uInfo.bts.Length == 0)
            {
                Debug.Log("this.bts == null || this.bts.Length == 0:" + uInfo.fName);
                if (uInfo.ErrorFun != null)
                    uInfo.ErrorFun("");
                return;
            }
            requestMethod = "PUT";
            this.serverUri = "/" + uInfo.fName;
            isSendOk = false;
            //if (ResourceManager.IsNormalScene())
            //{
            //    //Loom.RunAsync(UpdateCloud);
            //    BundleManager.obsUpdate.AddObserver(update);
            //}
            //else
            {
                #if UNITY_EDITOR
             //   Send(this);
                //Util.AddUpdate(update);
#endif
            }
            //if (Loom.initialized)
            //{
            //    Loom.RunAsync(sendThread);
            //}
            //else
            //{
            //    sendThread();
            //}
        }
        UnityWebRequest www;
        void update()
        {
            if (www == null)
            {
                //Util.RemoveUpdate(sendThread);
                www = UnityWebRequest.Put(Url, uInfo.bts);
                //Debug.Log(Authorization);
                www.SetRequestHeader("Authorization", this.Authorization);//);
                www.Send();
                isSendOk = true;
            }
            else
            {
                if (!www.isDone)
                {
                    uInfo.Progress((long)www.uploadedBytes, (long)uInfo.bts.Length);
                    return;
                }
                //if (ResourceManager.IsNormalScene())
                //{
                //    BundleManager.obsUpdate.RemoveObserver(update);
                //}
                //else
                {
                    #if UNITY_EDITOR
                  //  Util.RemoveUpdate(update);
#endif
                }
                if (www.isNetworkError || !string.IsNullOrEmpty(www.error))
                {
                    Debug.LogError("PUT ERROR: " + www.error + "\n" + www.downloadHandler.text);
                    Debug.LogError(this.Authorization);
                    uInfo.Error(www.error);
                }
                else if (www.isDone)
                {
                    uInfo.Complete();
                }
            }
        }
        //void sendThread()
        //{
        //    Send(this);
        //}
        /*
        public static void Send(QCloudUpload info)
        {
            using (QiniuWebClient qwc = new QiniuWebClient())
            {
                qwc.SetQCloudToken(info.Authorization);
                qwc.UploadDataCompleted += (sender, e) =>
                {
                    if (e.Error != null && e.Error is WebException)
                    {
                        Debug.LogError(e.Error.Message);
                        info.uInfo.ErrorFun(e.Error.Message);
                        if (e.Error is WebException)
                        {
                            WebException qwe = e.Error as WebException;
                            Stream stream = qwe.Response.GetResponseStream();
                            StreamReader readStream = new StreamReader(stream, Encoding.UTF8);
                            string rets = readStream.ReadToEnd();
                            Debug.LogError(rets + "\n" + qwe.Message);
                        }
                    }
                    else
                    {
                        info.uInfo.Complete();
                    }
                };
                qwc.UploadProgressChanged += (sender, e) =>
                {
                    info.uInfo.ProgressFun(e.BytesSent, e.TotalBytesToSend);
                };
                //qwc.Headers.Add("Content-Type", "application/octet-stream");
                qwc.UploadDataAsync(new Uri(info.Url), "PUT", info.uInfo.bts);
            }
        }
        */
        /*
        public static void Send(QCloudUpload info)
        {
            using (WebClient qwc = new WebClient())
            {
                //qwc.Headers.Add("Content-Type", "application/octet-stream");
                //qwc.Headers.Add("Content-Type", "multipart/form-data");
                //qwc.Headers.Add("ContentLength", info.bts.Length.ToString());
                qwc.Headers.Add("Authorization", info.Authorization);
                //qwc.Headers.Add("Host", info.serverHost);
                qwc.UploadDataCompleted += (object sender, UploadDataCompletedEventArgs e) =>
                {
                    if (e.Error != null)
                    {
                        Debug.LogError(e.Error.Message);
                        if (e.Error is WebException)
                        {
                            WebException qwe = e.Error as WebException;
                            Stream stream = qwe.Response.GetResponseStream();
                            StreamReader readStream = new StreamReader(stream, Encoding.UTF8);
                            string rets = readStream.ReadToEnd();
                            Debug.LogError(rets + "\n" + qwe.Message);
                            info.ErrorFun(qwe.Message);
                        }
                        else
                        {
                            info.ErrorFun(e.Error.Message);
                        }
                    }
                    else
                    {
                        info.CompleteFun(info);
                    }
                };

                qwc.UploadProgressChanged += (sender, e) =>
                {
                    info.ProgressFun(e.BytesSent, e.TotalBytesToSend);
                };
                qwc.UploadDataAsync(new Uri(info.Url), info.requestMethod, info.bts);
            }
        }
         * */
        static QCloudUpload()
        {
            ServicePointManager.DefaultConnectionLimit = 100;
        }
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //直接确认，否则打不开  
            return true;
        }
         
    }
public class UploadInfo
{
    public Action<UploadInfo> CompleteFun { set; get; }
    public Action<long, long> ProgressFun { set; get; }
    public Action<string> ErrorFun { set; get; }
    public byte[] bts;
    public string fName;
    public string url="";
    public void Complete()
    {
        if (CompleteFun != null)
            CompleteFun(this);
    }
    public void Error(string e)
    {
        if (ErrorFun != null)
            ErrorFun(e);
    }
    public void Progress(long a, long b)
    {
        if (ProgressFun != null)
            ProgressFun(a,b);
    }
}
public class QQCloudMgr
{
    private static string VERSION = "SDK_DOTNET_1.1";
    //public static string serverUri = "/v2/index.php";
    //public static string region = "ap-hongkong";
    public static string uploadHost = "aoa2018us-1251001060.cos.na-siliconvalley.myqcloud.com";//"aoa2018cos-1251001060.cos.ap-hongkong.myqcloud.com";//"aoa2018cos-1251001060.coshk.myqcloud.com";//
    public static string downloadHost = "all-dl-aoa.ldoverseas.com";
    public static string HotFixUrl
    {
        get
        {
            if (IsAWS)
                return AWSHotUrl;
            if (IsQCloud)
                return "https://" + downloadHost + "/";
            return HotUpdateUrl;
        }
    }
    public static string imageDownUrl
    {
        get
        {
            if (IsQCloud || IsAWS)
                return HotFixUrl;
            return NetPNGUrl;
        }
    }
    public static bool IsQiniu { get { return CloudState == "0"; } }
    public static bool IsQCloud { get { return CloudState == "1"; } }
    public static bool IsAWS { get { return CloudState == "2"; } }
    public static string HotUpdateUrl = "http://plbi5f0xz.bkt.clouddn.com/";//"http://o6ssva6nz.bkt.clouddn.com/";// + Util.PlatformDir; 
    public static string NetPNGUrl = "http://7xrnqw.com2.z0.glb.qiniucdn.com/";
    public static string AWSHotUrl = "https://d3vzdazntu2cw4.cloudfront.net/";
    public static string CloudState = "0";
    public static void Init(VersionStyle style)
    {
        CloudState = style.GetJsonInfo("IsQCloud", CloudState);
        HotUpdateUrl = style.GetJsonInfo("HotUpdateUrl", HotUpdateUrl);
        AWSHotUrl = style.GetJsonInfo("HotUpdateUrl", AWSHotUrl);
        uploadHost = style.GetJsonInfo("uploadHost", uploadHost);
        downloadHost = style.GetJsonInfo("downloadHost", downloadHost);
    }
    public static void DoUpload(UploadInfo data)
    {
        if (IsQCloud)
        {
            QCloudUpload qfile = new QCloudUpload(data);
            qfile.DoUpload();
        }
        else if (IsAWS)
        {
            // AWSS3Manager.UpLoad(data);
        }
    }
    public static string Authorization(string requestMethod, string SecretID, string SecretKey, string HttpURI, string param, SortedDictionary<string, object> requestParams)
    {
        int start = (int)GetTimeStamp() - 43200;
        int end = start + 86400;
        string strTime = start + ";" + end;
        string SignKey = HashHMACSHA1(SecretKey, strTime);
        string HttpHeaders = BuildQuery(requestParams);
        string HttpString = requestMethod.ToLower() + "\n" + HttpURI + "\n" + param + "\n" + HttpHeaders + "\n";
        string httpSha1 = SHA1Convert(HttpString);
        string StringToSign = "sha1\n" + strTime + "\n" + httpSha1 + "\n";
        string signStr = HashHMACSHA1(SignKey, StringToSign);
        string author = "q-sign-algorithm=sha1&q-ak=" + SecretID + "&q-sign-time=" + strTime + "&q-key-time=" + strTime + "&q-header-list=&q-url-param-list=&q-signature=" + signStr;//content-type;contentlength;host
                                                                                                                                                                                     //Debug.Log(httpSha1 + "\n" + HttpString + author);
        return author;
    }
    public static string BuildQuery(SortedDictionary<string, object> requestParams)
    {
        string paramStr = "";
        foreach (string key in requestParams.Keys)
        {
            //paramStr += string.Format("{0}={1}&", key, HttpUtility.UrlEncode(requestParams[key].ToString()));
            paramStr += string.Format("{0}={1}&", key.ToLower(), WWW.EscapeURL(requestParams[key].ToString()));//UrlEncode(requestParams[key].ToString()));//
        }
        paramStr = paramStr.TrimEnd('&');
        return paramStr;
    }
    private static string HashHMACSHA1(string key, string content)
    {
        //var buff = new HMACSHA1(Encoding.UTF8.GetBytes(key)).ComputeHash(Encoding.UTF8.GetBytes(content));
        //return string.Concat(buff.Select(k => k.ToString("x2")));
        using (HMACSHA1 mac = new HMACSHA1(Encoding.UTF8.GetBytes(key)))
        {
            byte[] hash = mac.ComputeHash(Encoding.UTF8.GetBytes(content));
            string result = BitConverter.ToString(hash);
            result = result.Replace("-", "");
            return result.ToLower();
            //return BitConverter.ToString(hash); //Convert.ToBase64String(hash);
        }
    }
    public static string SHA1Convert(string content)
    {
        return SHA1Convert(content, Encoding.UTF8);
    }
    public static string SHA1Convert(string content, Encoding encode)
    {
        try
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] bytes_in = encode.GetBytes(content);
            byte[] bytes_out = sha1.ComputeHash(bytes_in);
            //sha1.Dispose();
            string result = BitConverter.ToString(bytes_out);
            result = result.Replace("-", "");
            return result.ToLower();
        }
        catch (Exception ex)
        {
            throw new Exception("SHA1加密出错：" + ex.Message);
        }
    }
    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }

    public static void Send(QCloudUpload info)
    {
        RequestInfo reInfo = new RequestInfo(info.Url);
        reInfo.AddHeaders("Authorization", info.Authorization);
        // foreach(var k in info.requestParams.Keys)
        // {
        //     reInfo.AddHeaders(k, info.requestParams[k].ToString());
        //}
        reInfo.PostData = info.uInfo.bts;
        reInfo.CompleteFun = delegate (RequestInfo eInfo)
        {
            Debug.Log("上传成功：" + info.Url);
            info.uInfo.Complete();
        };
        reInfo.ProgressFun = info.uInfo.ProgressFun;
        //reInfo.ErrorFun = info.uInfo.ErrorFun;
        reInfo.request();
    }
    /*
    public static QCloudUpload GenerateUrl(string resName, string param="v")
    {
        QCloudUpload info = new QCloudUpload();
        info.serverUri = "/" + resName;
        //if (param == "v")
        //{
        //    float rnd = UnityEngine.Random.Range(0.1f, 1.0f);
        //    info.param = "v=" + rnd;
        //}
        //else
        //    info.param = param;
        info.requestParams["Host"] = uploadHost;
        info.requestMethod = "GET";
        return info;
        //return GenerateUrl(info.requestParams, info.secretId, info.secretKey, info.requestMethod, info.serverHost, info.serverUri);
    }
     * */

    //public static string Signature(string signStr, string secret, string SignatureMethod)
    //{
    //    if (SignatureMethod == "HmacSHA256")
    //        using (HMACSHA256 mac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
    //        {
    //            byte[] hash = mac.ComputeHash(Encoding.UTF8.GetBytes(signStr));
    //            return Convert.ToBase64String(hash);
    //        }
    //    else
    //        using (HMACSHA1 mac = new HMACSHA1(Encoding.UTF8.GetBytes(secret)))
    //        {
    //            byte[] hash = mac.ComputeHash(Encoding.UTF8.GetBytes(signStr));
    //            return Convert.ToBase64String(hash);
    //        }
    //}
    //protected static string BuildParamStr(SortedDictionary<string, object> requestParams, string requestMethod = "GET")
    //{
    //    string retStr = "";
    //    foreach (string key in requestParams.Keys)
    //    {
    //        if (key == "Signature")
    //        {
    //            continue;
    //        }
    //        if (requestMethod == "POST" && requestParams[key].ToString().Substring(0, 1).Equals("@"))
    //        {
    //            continue;
    //        }
    //        retStr += string.Format("{0}={1}&", key.Replace("_", "."), requestParams[key]);
    //    }
    //    return "?" + retStr.TrimEnd('&');
    //}
    //public static string UrlEncode(string str)
    //{
    //    return UrlEncode(str, "UTF-8");
    //}
    //public static string UrlEncode(string str, string encode)
    //{
    //    StringBuilder sb = new StringBuilder();
    //    byte[] byStr = System.Text.Encoding.GetEncoding(encode).GetBytes(str);
    //    for (int i = 0; i < byStr.Length; i++)
    //    {
    //        sb.Append(@"%" + Convert.ToString(byStr[i], 16));
    //    }
    //    return sb.ToString();
    //}



    //public static string MakeSignPlainText(SortedDictionary<string, object> requestParams, string requestMethod = "GET",
    //    string requestHost = "cvm.api.qcloud.com", string requestPath = "/v2/index.php")
    //{
    //    string retStr = "";
    //    retStr += requestMethod;
    //    retStr += requestHost;
    //    retStr += requestPath;
    //    retStr += BuildParamStr(requestParams);
    //    return retStr;
    //}

    //public static string GenerateUrl(SortedDictionary<string, object> requestParams, string secretId, string secretKey,
    //    string requestMethod, string requestHost, string requestPath)
    //{
    //    GetParams(requestParams, secretId, secretKey, requestMethod, requestHost, requestPath);
    //    string url = "https://" + requestHost + requestPath;
    //    if (requestMethod == "GET")
    //    {
    //        url += "?" + BuildQuery(requestParams);
    //    }
    //    return url;
    //}
    //public static void GetParams(SortedDictionary<string, object> requestParams, string secretId, string secretKey,
    //    string requestMethod, string requestHost, string requestPath)
    //{
    //    requestParams.Add("SecretId", secretId);
    //    System.Random rand = new System.Random();
    //    requestParams.Add("Nonce", rand.Next(Int32.MaxValue).ToString());
    //    DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
    //    DateTime nowTime = DateTime.Now;
    //    long unixTime = (long)Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
    //    requestParams.Add("Timestamp", (unixTime / 1000).ToString());
    //    requestParams.Add("RequestClient", VERSION);
    //    string plainText = MakeSignPlainText(requestParams, requestMethod, requestHost, requestPath);
    //    string SignatureMethod = "HmacSHA1";
    //    if (requestParams.ContainsKey("SignatureMethod") && requestParams["SignatureMethod"] == "HmacSHA256")
    //    {
    //        SignatureMethod = "HmacSHA256";
    //    }
    //    string sign = Signature(plainText, secretKey, SignatureMethod);
    //    requestParams.Add("Signature", sign);
    //}

    //public static void Send(FileInfo fInfo)
    //{
    //    SortedDictionary<string, object> config = new SortedDictionary<string, object>(StringComparer.Ordinal);
    //    config["SecretId"] = "AKIDqa8Znv2eFerv47kWD6tW0KSOKSdhcWFm";
    //    config["SecretKey"] = "TnxyM8qQq4CAwb47cyAB0fAUffXIvMdh";
    //    config["RequestMethod"] = "PUT";
    //    config["DefaultRegion"] = "ap-hongkong";

    //    SortedDictionary<string, object> requestParams = new SortedDictionary<string, object>(StringComparer.Ordinal);
    //    requestParams["entityFileName"] = fInfo.Name;
    //    requestParams["entityFile"] = fInfo.FullName;
    //    Cdn cdn = new Cdn();
    //    cdn.serverHost = "aoa2018cos-1251001060.cos.ap-hongkong.myqcloud.com";
    //    cdn.serverUri = "/" + fInfo.Name;
    //    QCloudAPIModuleCenter module = new QCloudAPIModuleCenter(cdn, config);
    //    Debug.Log(module.Call("UploadCdnEntity", requestParams));
    //}
    //static void Main(string[] args)
    //{
    //SortedDictionary<string, object> config = new SortedDictionary<string, object>(StringComparer.Ordinal);
    //config["SecretId"] = "你的secretId";
    //config["SecretKey"] = "你的secretKey";
    //config["RequestMethod"] = "GET";
    //config["DefaultRegion"] = "gz";

    //QCloudAPIModuleCenter module = new QCloudAPIModuleCenter(new Cvm(), config);
    //SortedDictionary<string, object> requestParams = new SortedDictionary<string, object>(StringComparer.Ordinal);
    //requestParams["offset"] = 0;
    //requestParams["limit"] = 3;
    //您可以在这里指定签名算法，不指定默认为HmacSHA1		
    //requestParams["SignatureMethod"] = "HmacSHA256";
    //Console.WriteLine(module.GenerateUrl("DescribeInstances", requestParams));
    //Console.WriteLine(module.Call("DescribeInstances", requestParams));



    //UploadCdnEntity
    //string fileName = "c:\\del.bat";
    //SortedDictionary<string, object> requestParams = new SortedDictionary<string, object>(StringComparer.Ordinal);
    //requestParams["entityFileName"] = "/upload/del.bat";
    //requestParams["entityFile"] = fileName;
    //QCloudAPIModuleCenter module = new QCloudAPIModuleCenter(new Cdn(), config);
    //Console.WriteLine(module.Call("UploadCdnEntity", requestParams));

    //Console.ReadKey();

    //http://{bucket}.cos.{region}.myqcloud.com/{ObjectName}
    //{ObjectName}?response-content-type=[ContentType]&response-content-language=[ContentLanguage]&response-expires=[ResponseExpires]&response-cache-control=[ResponseCacheControl]&response-content-disposition=[ResponseContentDisposition]&response-content-encoding=[ResponseContentEncoding] 
    //}


    //        public static string publishPath
    //        {
    //            get
    //            {
    //                ePublish ep = ePublish.RELEASE;
    //                if (Application.isPlaying && VersionManager.Instance != null)
    //                    ep = VersionManager.Instance.mStyle.Publish;
    //                else
    //                {
    //#if UNITY_EDITOR
    //                    VersionStyle st = UnityEditor.AssetDatabase.LoadAssetAtPath(@"Assets/Resources/VersionStyle.asset", typeof(Frame.VersionStyle)) as Frame.VersionStyle;
    //                    ep = st.Publish;
    //#endif
    //                }
    //                string pb = Util.PlatformDir + "_" + ep.ToString() + "_" + Frame.VersionStyle.FrameVersion;
    //                return Application.dataPath + "/../../Other/Publish/" + pb + "/";
    //            }
    //        }
    //static VersionData mData = null;
    //public static VersionData vData
    //{
    //    get
    //    {
    //        if (mData == null)
    //            mData = new VersionData(publishPath + "version.txt");
    //        return mData;
    //    }
    //}
    public static bool uploadOk = false;
    public static bool isUpLoading = false;
    public static List<string> uploadStartUp()
    {
        string path = MPrefs.GetString("_GenVersionToPath_", "");
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Path==null");
            return null;
        }
        List<string> list = new List<string>();
        //path = string.IsNullOrEmpty(path) ? publishPath + vData.Version + "/" : path;
        if (Directory.Exists(path))
        {
            DirectoryInfo from = new DirectoryInfo(path);
            FileInfo[] fls = from.GetFiles("*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < fls.Length; i++)
            {
                if (fls[i].FullName.Contains(".DS_Store"))
                    continue;
                list.Add(fls[i].FullName);
            }
        }
        else
        {
            Debug.LogError("目录不存在：" + path);
            return null;
        }
        AllNum = list.Count;
        Debug.Log("【start upload】：" + AllNum);
        uploadQiNiuIndex(list);
        return list;
    }
    public static int AllNum = 0;
    public static int curNum = 0;
    public static FileInfo curFl;
    static void uploadQiNiuIndex(List<string> list, int index = 0)
    {
        if (index >= list.Count)
        {
            uploadOk = true;
            isUpLoading = false;
            return;
        }
        isUpLoading = true;
        curNum = index;
        uploadOk = false;
        FileInfo file = new FileInfo(list[index]);
        if (file.Exists)
        {
            curFl = file;

            if (QQCloudMgr.IsQiniu)
                UpLoadQiniuCDN(file, list, index);
            else
            {
                byte[] bytes = null;
                FileStream fs = file.OpenRead();//new FileStream("data.bin", FileMode.OpenOrCreate); 
                BinaryReader r = new BinaryReader(fs);
                using (MemoryStream ms = new MemoryStream())
                {
                    int b;
                    while ((b = fs.ReadByte()) != -1)
                    {
                        ms.WriteByte((byte)b);
                    }
                    bytes = ms.ToArray();
                }
                fs.Close();
                UpLoadQCloud(file, bytes, list, index);
            }


        }
    }
    static void UpLoadQCloud(FileInfo file, byte[] bts, List<string> list, int index)
    {
        //uploadOk = true;
        //isUpLoading = false;
        //QCloudMgr.Send(file);
        //return;
        UploadInfo info = new UploadInfo();
        info.bts = bts;
        info.fName = file.Name;
        //上传完成
        info.CompleteFun = delegate (UploadInfo response)
        {
            index++;
            Debug.Log("上传成功：" + info.url + " " + info.fName + "   " + AllNum + "/ " + index);
            if (index >= list.Count)
                Debug.Log("上传完毕。" + list.Count);
            uploadQiNiuIndex(list, index);
        };
        info.ErrorFun = delegate (string e)
        {
            uploadOk = true;
            isUpLoading = false;
            Debug.LogError("上传失败：" + e);
        };
        QQCloudMgr.DoUpload(info);
    }
    public static string AccessKey;
    public static string SecretKey;
    public static string Bucket;
    static void UpLoadQiniuCDN(FileInfo file, List<string> list, int index)
    {

        Qiniu.Http.HttpCode code = QiniuCSharpSDK.UploadFileTest(file.FullName,file.Name,Bucket, AccessKey,SecretKey);
        index++;
        if (code != Qiniu.Http.HttpCode.OK)
        {
            Debug.LogError("上传失败:" + code + "  " + file.Name + "  " + AllNum + "/ " + index);
        }
        else
        {
            Debug.Log("上传成功：" + file.Name + "   " + AllNum + "/ " + index);
        }
        if (index >= list.Count)
            Debug.Log("上传完毕。" + AllNum + "  " + Bucket);
        uploadQiNiuIndex(list, index);
    }
}

//}
