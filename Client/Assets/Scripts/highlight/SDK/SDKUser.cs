using UnityEngine;
using System.Collections;
using System;
namespace SDK
{
    public class SDKUser
    {

        private long id;
        private string channelUserId;
        private string appid;
        private string appkey;

        private string userName;
        private string token;
        private string productCode;
        public bool isSuccess = true;
        public string errStr = "";
        public string curName;
        public string curPwd;
        public QGameSDK.LoginResult aType = QGameSDK.LoginResult.Login;
        public SDKUser(string channelUserId)
        {
            this.channelUserId = channelUserId;
        }
        public SDKUser(string channelUserId, string token, string appid, string appkey)
        {
            this.channelUserId = channelUserId;
            this.appid = appid;
            this.appkey = appkey;
            this.token = token;
        }

        public SDKUser(long id, string channelUserId,
                string userName, string token, string productCode)
        {
            this.id = id;
            this.channelUserId = channelUserId;

            this.userName = userName;
            this.token = token;
            this.productCode = productCode;
        }

        public long getId()
        {
            return id;
        }

        public void setId(long id)
        {
            this.id = id;
        }

        public string getChannelUserId()
        {
            return channelUserId;
        }

        public void setChannelUserId(string channelUserId)
        {
            this.channelUserId = channelUserId;
        }

        public string getUserName()
        {
            return userName;
        }

        public void setUserName(string userName)
        {
            this.userName = userName;
        }

        public string getToken()
        {
            return token;
        }

        public void setToken(string token)
        {
            this.token = token;
        }

        public string getProductCode()
        {
            return productCode;
        }

        public void setProductCode(string productCode)
        {
            this.productCode = productCode;
        }

        public string ToJson()
        {
            string jsonStr = "";
            return jsonStr;
        }
    }

}