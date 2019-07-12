//using UnityEngine;
//using System.Collections;
//using System.Management;
//using System.Management.Instrumentation;

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine;
public class SystemInfoUtil
{

    //    public static string GetMac()
    //    {
    //        string mac = "";
    //        ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
    //        ManagementObjectCollection moc2 = mc.GetInstances();
    //        foreach (ManagementObject mo in moc2)
    //        {
    //            if ((bool)mo["IPEnabled"] == true)
    //                mac = mo["MacAddress"].ToString();
    //            //Response.Write("MAC address\t{0}" + mo["MacAddress"].ToString());
    //            mo.Dispose();
    //        }
    //        return mac;
    //    }
    public static String GetAndroidID()
    {
        string android_id = "";
#if UNITY_ANDROID
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
        android_id = secure.CallStatic<string>("getString", contentResolver, "android_id");
#endif
        return android_id;
    }
    public static string GetDevice()
    {
        return SystemInfoUtil.deviceModel + "_" + SystemInfoUtil.deviceUniqueIdentifier;
    }
    public static bool supportsInstancing
    {
        get { return SystemInfo.supportsInstancing; }
    }
    public static string graphiceDeviceType
    {
        get { return SystemInfo.graphicsDeviceType.ToString(); }
    }
    public static bool IsHdr
    {
        get
        {
            return SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR);
        }
    }
    public static string GetMac()
    {

        return SystemInfo.deviceType + " " + SystemInfo.deviceName;
        //string mac = "";
        //mac = GetMacAddressBySendARP();
        //return mac;
    }
    public static string deviceModel
    {
        get {
#if UNITY_IPHONE
            return UnityEngine.iOS.Device.generation.ToString(); 
#else
            return SystemInfo.deviceModel; 
#endif

            }
    }
    //iPhone10,3  国行(A1865)、日行(A1902)iPhone X
    //iPhone10,6  美版(Global/A1901)iPhone X
    public static bool IsIphoneX()
    {
        string dev = SystemInfo.deviceModel;
        return dev == "iPhone10,3" || dev == "iPhone10,6";
    }
    static string idfa = "";
    public static string deviceUniqueIdentifier
    {
        get 
        {
            if (!string.IsNullOrEmpty(idfa))
                return idfa;
#if UNITY_IPHONE
            idfa = UnityEngine.iOS.Device.advertisingIdentifier; //这里为空，平台IOS9.2，iphone6
#endif
            if (String.IsNullOrEmpty(idfa) || idfa == "00000000-0000-0000-0000-000000000000")
                idfa = SystemInfo.deviceUniqueIdentifier;
            idfa = idfa.Replace("-", "").ToLower();
            return idfa;
            //return CalculateChecksum(deviceID);
            //return SystemInfo.deviceUniqueIdentifier; 
        }
    }
    public static string deviceType
    {
        get { return SystemInfo.deviceType.ToString(); }
    }


    
    [System.Runtime.InteropServices.DllImport("Iphlpapi.dll")]
    static extern int SendARP(Int32 DestIP, Int32 SrcIP, ref Int64 MacAddr, ref Int32 PhyAddrLen);
    /// <summary>  
    /// SendArp获取MAC地址  
    /// </summary>  
    /// <returns></returns>  
    public static string GetMacAddressBySendARP()
    {
        StringBuilder strReturn = new StringBuilder();
        try
        {
            System.Net.IPHostEntry Tempaddr = (System.Net.IPHostEntry)System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            System.Net.IPAddress[] TempAd = Tempaddr.AddressList;
            Int32 remote = BitConverter.ToInt32(TempAd[0].GetAddressBytes(),0);
            Int64 macinfo = new Int64();
            Int32 length = 6;
            SendARP(remote, 0, ref macinfo, ref length);
            string temp = System.Convert.ToString(macinfo, 16).PadLeft(12, '0').ToUpper();
            int x = 12;
            for (int i = 0; i < 6; i++)
            {
                if (i == 5) { strReturn.Append(temp.Substring(x - 2, 2)); }
                else { strReturn.Append(temp.Substring(x - 2, 2) + ":"); }
                x -= 2;
            }
            return strReturn.ToString();
        }
        catch
        {
            return "";
        }
    }

    public static string CalculateChecksum(string dataToCalculate)
    {
        byte[] byteToCalculate = Encoding.ASCII.GetBytes(dataToCalculate);
        int checksum = 0;
        foreach (byte chData in byteToCalculate)
        {
            checksum += chData;
        }
        checksum &= 0xff;
        return checksum.ToString("X2");
    }

    public static int CountryCode()
    {
        return 0;
    }
    public static string GetIP()
    {
        string ip = "";
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces(); ;
        foreach (NetworkInterface adapter in adapters)
        {
            bool Pd1 = (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet); //判断是否是以太网连接  
            if (Pd1)  
            {
                IPInterfaceProperties ipProp = adapter.GetIPProperties();     //IP配置信息
                UnicastIPAddressInformationCollection uniCast = ipProp.UnicastAddresses;
                if (uniCast.Count > 0)
                {
                    foreach (UnicastIPAddressInformation uni in uniCast)
                    {
                        //得到IPv4的地址。 AddressFamily.InterNetwork指的是IPv4
                        if (uni.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            ip = uni.Address.ToString();
                        }
                    }
                }
            }
        }
        return ip;
    }
    static string _IpDNS2 = "";
    public static string GetIPDns2()
    {
        if (!string.IsNullOrEmpty(_IpDNS2))
            return _IpDNS2;
        string ipStr = "";
        string dnsName = Dns.GetHostName();
        if (string.IsNullOrEmpty(dnsName))
        {
            return "NullDNSName";
        }
        IPAddress[] ips = Dns.GetHostAddresses(dnsName);   //Dns.GetHostName()获取本机名Dns.GetHostAddresses()根据本机名获取ip地址组
        foreach (IPAddress ip in ips)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ipStr += "v4," + ip.ToString() + ", ";  //ipv4
                break;
            }
            else if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                ipStr += "v6," + ip.ToString() + ", "; //ipv6
                break;
            }
        }
        try
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces(); ;
            foreach (NetworkInterface adapter in adapters)
            {
                bool Pd1 = (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet); //判断是否是以太网连接  
                if (Pd1)
                {
                    IPInterfaceProperties ipProp = adapter.GetIPProperties();     //IP配置信息
                    int DnsCount = ipProp.DnsAddresses.Count;
                    if (DnsCount > 0)
                        ipStr += ipProp.DnsAddresses[0].ToString() + ", "; //主DNS  
                    if (DnsCount > 1)
                        ipStr += ipProp.DnsAddresses[1].ToString(); //备用DNS地址  

                }
            }
        }
        catch (Exception er)
        {
            Debug.LogError("DNSError:" + er.Message + "\n" + er);
        }
        _IpDNS2 = ipStr;
        return ipStr;
    }
    public static string GetIPDataDef()
    {
        string info = MPrefs.GetString("_IPINFO_","");
        if (string.IsNullOrEmpty(info))
        {
            info = GetIPData();
            MPrefs.SetString("_IPINFO_", info);
        }
        return info;
    }
    public static void ClearIPData()
    {
        MPrefs.SetString("_IPINFO_", "");
    }
    public static string GetIPData(string ip = "", string datatype = "txt")
    {
        Debug.Log("【GetIPData】");
        string token = "8afb2a7d829d7207ff0c1ee7316fa4d2";
        //if (string.IsNullOrEmpty(ip))
        //{
        //    ip = HttpContext.Current.Request.UserHostAddress;
        //}
        long time = System.DateTime.Now.Ticks;
        string ipInfo = "";
        try
        {
            string url = string.Format("http://api.ip138.com/query/?ip={0}&datatype={1}&token={2}", ip, datatype, token);
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                string result = client.DownloadString(url);
                ipInfo = result.Trim();
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
        time = (System.DateTime.Now.Ticks - time) / 10000;
        if (!string.IsNullOrEmpty(ipInfo) && ipInfo.IndexOf("\t") > -1)
        {
            string[] ips = ipInfo.Split('\t');
         //   GoogleAnsSdk.LogEvent("ip查询成功", ips[0], ips[1], time);
            Debug.Log("【ip查询成功" + time + "】" + ipInfo);
        }
        else
        {
        //    GoogleAnsSdk.LogEvent("ip查询失败", SystemInfoUtil.deviceUniqueIdentifier, "", time);
            Debug.Log("【ip查询失败】");
        }
        return ipInfo;
    }
    /// <summary>
    /// 是否是无线
    /// </summary>
    public static bool IsWifi
    {
        get
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }
    }
    public static string Resolution
    {
        get
        {
            return string.Format("{0}x{1}", Screen.width, Screen.height);
        }
    }
    public static string Platform
    {
        get { return Application.platform.ToString(); }
    }
    public static string UrlEncode(string str)
    {
        return UrlEncode(str, "UTF-8");
    }
    public static string UrlEncode(string str, string encode)
    {
        StringBuilder sb = new StringBuilder();
        byte[] byStr = System.Text.Encoding.GetEncoding(encode).GetBytes(str);
        for (int i = 0; i < byStr.Length; i++)
        {
            sb.Append(@"%" + Convert.ToString(byStr[i], 16));
        }
        return sb.ToString();
    }
}
