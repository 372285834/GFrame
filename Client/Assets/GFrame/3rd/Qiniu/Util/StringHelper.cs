using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Qiniu.Util
{
    /// <summary>
    /// 字符串处理工具
    /// </summary>
    public class StringHelper
    {
        

        /// <summary>
        /// URL编码
        /// </summary>
        /// <param name="text">源字符串</param>
        /// <returns>URL编码字符串</returns>
        public static string UrlEncode(string text)
        {
            return Uri.EscapeDataString(text);
        }

        /// <summary>
        /// URL键值对编码
        /// </summary>
        /// <param name="values">键值对</param>
        /// <returns>URL编码的键值对数据</returns>
        public static string UrlFormEncode(Dictionary<string, string> values)
        {
            StringBuilder urlValuesBuilder = new StringBuilder();
           
            foreach (KeyValuePair<string, string> kvp in values)
            {
                urlValuesBuilder.AppendFormat("{0}={1}&", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value));
            }
            string encodedStr=urlValuesBuilder.ToString();
            return encodedStr.Substring(0, encodedStr.Length - 1);
        }


        /// <summary>
        /// 计算SHA1
        /// </summary>
        /// <param name="data">字节数据</param>
        /// <returns>SHA1</returns>
        public static byte[] calcSHA1(byte[] data)
        {
#if WINDOWS_UWP
            var sha = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            var buf = CryptographicBuffer.CreateFromByteArray(data);
            var digest = sha.HashData(buf);
            var hashBytes = new byte[digest.Length];
            CryptographicBuffer.CopyToByteArray(digest, out hashBytes);
            return hashBytes;
#else
            SHA1 sha1 = SHA1.Create();
            return sha1.ComputeHash(data);
#endif
        }

        /// <summary>
        /// 计算MD5哈希(可能需要关闭FIPS)
        /// </summary>
        /// <param name="str">待计算的字符串</param>
        /// <returns>MD5结果</returns>
        public static string calcMD5(string str)
        {
#if WINDOWS_UWP
            var md5 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var buf = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            var digest = md5.HashData(buf);
            return CryptographicBuffer.EncodeToHexString(digest);
#else
            MD5 md5 = MD5.Create();
            byte[] data = Encoding.UTF8.GetBytes(str);
            byte[] hashData = md5.ComputeHash(data);
            StringBuilder sb = new StringBuilder(hashData.Length * 2);
            foreach (byte b in hashData)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
#endif
        }

        /// <summary>
        /// 计算MD5哈希(第三方实现)
        /// </summary>
        /// <param name="str">待计算的字符串,避免FIPS-Exception</param>
        /// <returns>MD5结果</returns>
        public static string CalcMD5X(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            LabMD5 md5 = new LabMD5();
            return md5.ComputeHash(data);
        }



        /// <summary>
        /// 获取字符串Url安全Base64编码值
        /// </summary>
        /// <param name="text">源字符串</param>
        /// <returns>已编码字符串</returns>
        public static string urlSafeBase64Encode(string text)
        {
            return urlSafeBase64Encode(Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// URL安全的base64编码
        /// </summary>
        /// <param name="data">需要编码的字节数据</param>
        /// <returns></returns>
        public static string urlSafeBase64Encode(byte[] data)
        {
            return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_');
        }

        /// <summary>
        /// bucket:key 编码
        /// </summary>
        /// <param name="bucket">空间名称</param>
        /// <param name="key">文件key</param>
        /// <returns>编码</returns>
        public static string urlSafeBase64Encode(string bucket, string key)
        {
            return urlSafeBase64Encode(bucket + ":" + key);
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="text">待解码的字符串</param>
        /// <returns>已解码字符串</returns>
        public static byte[] urlsafeBase64Decode(string text)
        {
            return Convert.FromBase64String(text.Replace('-', '+').Replace('_', '/'));
        }
    }
}