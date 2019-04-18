using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System;

public class MDESHelper  {

    /// <summary>
    /// 进行DES加密。
    /// </summary>
    /// <param name="pToEncrypt">要加密的字符串。</param>
    /// <param name="sKey">密钥，且必须为8位。</param>
    /// <returns>以Base64格式返回的加密字符串。</returns>
    public static string DESEncrypt(string encryptString, string encryptKey)
    {
        DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
        byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey);
        byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
        MemoryStream mStream = new MemoryStream();
        CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, new byte[8]), CryptoStreamMode.Write);
        cStream.Write(inputByteArray, 0, inputByteArray.Length);
        cStream.FlushFinalBlock();

        return Convert.ToBase64String(mStream.ToArray());
    }

    /// <summary>
    /// 进行DES解密。
    /// </summary>
    /// <param name="pToDecrypt">要解密的以Base64</param>
    /// <param name="sKey">密钥，且必须为8位。</param>
    /// <returns>已解密的字符串。</returns>
    public static string DESDecrypt(string decryptString, string decryptKey)
    {
        byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey);
        byte[] inputByteArray = Convert.FromBase64String(decryptString);
        DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
        MemoryStream mStream = new MemoryStream();
        CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, new byte[8]), CryptoStreamMode.Write);
        cStream.Write(inputByteArray, 0, inputByteArray.Length);
        cStream.FlushFinalBlock();
        return Encoding.UTF8.GetString(mStream.ToArray());
    }

    /// <summary>
    /// 进行DES加密字节数组。
    /// </summary>
    /// <param name="pToEncrypt">要加密的字节数组。</param>
    /// <param name="sKey">密钥，且必须为8位。</param>
    /// <returns>以Base64格式返回的加密字符串。</returns>
    public static byte[] DESEncryptByte(byte[] encryptByte, string encryptKey)
    {
        DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
        byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey);
        byte[] inputByteArray = encryptByte;
        MemoryStream mStream = new MemoryStream();
        CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, new byte[8]), CryptoStreamMode.Write);
        cStream.Write(inputByteArray, 0, inputByteArray.Length);
        cStream.FlushFinalBlock();

        return mStream.ToArray();
    }
}
