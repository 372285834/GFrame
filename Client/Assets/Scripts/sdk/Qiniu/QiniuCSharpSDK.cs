using Qiniu.Http;
using Qiniu.Storage;
using Qiniu.Util;
using UnityEngine;
/// <summary>
/// Qiniu (Cloud) C# SDK for .NET Framework 2.0+/Core/UWP
/// Modules in this SDK:
/// "Storage" 存储相关功能，上传，下载，数据处理，资源管理
/// "CDN",    Fusion CDN, 融合CDN加速; 
/// "Util",   Utilities such as MD5 hashing, 实用工具(如MD5哈希计算等);
/// "Http", HTTP Request Manager, HTTP请求管理器
/// </summary>
public class QiniuCSharpSDK
{
    /// <summary>
    /// SDK名称
    /// </summary>
    public const string ALIAS = "QiniuCSharpSDK";

    /// <summary>
    /// 目标框架
    /// </summary>
#if Net20
    public const string RTFX = "NET20";
#elif Net35
    public const string RTFX = "NET35";
#elif Net40
    public const string RTFX = "NET40";
#elif Net45
    public const string RTFX = "NET45";
#elif Net46
    public const string RTFX = "NET46";
#elif NetCore
    public const string RTFX = "NETCore";
#elif WINDOWS_UWP
    public const string RTFX = "UWP";
#else
    public const string RTFX = "UNKNOWN";
#endif

    /// <summary>
    /// SDK版本号
    /// </summary>
    public const string VERSION = "7.3.0";
#if UNITY_EDITOR

    public static string AccessKey = "eJzgFEEPoSXySi-Y7M-hOZHhju1_djMj_Bu6-9sY";
    public static string SecretKey = "cmNJJ3aGkMx0qQnYPC-R4JDgKVYEg4liCuGUuKX-";
#endif
    public static string Bucket = "five6";
    public static HttpCode UploadFileTest(string fileUrl, string key)
    {
#if UNITY_EDITOR
        Mac mac = new Mac(AccessKey, SecretKey);
        PutPolicy putPolicy = new PutPolicy();
        putPolicy.Scope = Bucket + ":" + key;
        putPolicy.SetExpires(3600);
        //putPolicy.DeleteAfterDays = 1;
        string token = Auth.CreateUploadToken(mac, putPolicy.ToJsonString());
        Config config = new Config();
        config.Zone = Zone.ZONE_CN_East;
        config.UseHttps = true;
        config.UseCdnDomains = true;
        config.ChunkSize = ChunkUnit.U512K;
        FormUploader target = new FormUploader(config);
        HttpResult result = target.UploadFile(fileUrl, key, token, null);
        Debug.Log("form upload result: " + result.ToString());
        return (HttpCode)result.Code;
#else
        return HttpCode.BAD_REQUEST;
#endif
    }
}
