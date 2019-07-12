using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading;
#if UNITY_EDITOR
using Aliyun.OSS;
using Aliyun.OSS.Common;
using Aliyun.OSS.Util;
#endif
public static class AliyunMgr
{
#if UNITY_EDITOR
    public static string endpoint = "oss-cn-shanghai.aliyuncs.com";
    public static string accessKeyId = "LTAIy6kV6GBUo3mK";
    public static string accessKeySecret = "7cEx6eXO5rSmoDVy4mkLLjsebmu2Va";
    static AutoResetEvent _event = new AutoResetEvent(false);

    // 创建OssClient实例。
    static OssClient client = new OssClient(endpoint, accessKeyId, accessKeySecret);
#endif
    public static string Bucket = "hzg-jzsy";
    static Action UploadCallback;
    public static void UploadFile(string fileUrl, string key,Action callback)
    {
        UploadCallback = callback;
#if UNITY_EDITOR
       // try
        {
            // 计算MD5。
            string md5;
            using (var fs = File.Open(fileUrl, FileMode.Open))
            {
                md5 = OssUtils.ComputeContentMd5(fs, fs.Length);
                var objectMeta = new ObjectMetadata
                {
                   // ContentType = "application/octet-stream",
                    ContentMd5 = md5
                };
                string result = "Notice user: put object finish";
                // 上传文件。
                client.BeginPutObject(Bucket, key, fs, objectMeta, PutObjectCallback, result.ToCharArray());
                // Console.WriteLine("Put object succeeded");
                _event.WaitOne();
            }
        }
        //catch (OssException ex)
        //{
        //    Debug.LogError(string.Format("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
        //            ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId));
        //}
        //catch (Exception ex)
        //{
        //     Debug.LogError("Put object failed, :"+ ex.Message + "\n" + ex.StackTrace);
        //}
#endif
    }
#if UNITY_EDITOR
    private static void PutObjectCallback(IAsyncResult ar)
    {
        try
        {
            client.EndPutObject(ar);
            Debug.Log("AsyncState:" + ar.AsyncState as string);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
        }
        finally
        {
            _event.Set();
            if (UploadCallback != null)
                UploadCallback();
        }
    }
#endif
}
