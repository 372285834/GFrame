using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.IO;
using System.Text;
//using TcpSocket;

public class MSocketService : MonoBehaviour
{

    public bool isShowDebug = false;
    /// <summary>
    /// 接受到的数据量
    /// </summary>
    public long ByteSizeReceive = 0;
    /// <summary>
    /// 发送的数据量
    /// </summary>
    public long ByteSizeSend = 0;
    public string Name;
    public static MSocketService CreatSocket(string name)
    {
        MSocketService socketGo = new GameObject(name).AddComponent<MSocketService>();
        socketGo.Name = name;
        //socketGo.msgHandler = new MsgHandler(socketGo);
        DontDestroyOnLoad(socketGo.gameObject);
        return socketGo;
    }

    Queue<KeyValuePair<ushort, byte[]>> LPacketRecevieList = new Queue<KeyValuePair<ushort, byte[]>>(0);
    public void Send(byte[] bts)
    {
        if (Status != SocketStatus.Connecting)
            Debug.LogError("SendProBuf:" + Status.ToString() + "");
        if (muSocket != null)
        {
            muSocket.AddRequest(bts);
        }
        else
        {
            Debug.LogError("Socket is closed..............");
        }
    }
    /// <summary>
    /// Adds the recive.
    /// 收到服务器返回的请求，然后进行处理。
    /// </summary>
    /// <param name='mPacket'>
    /// M packet.
    /// </param>
    public void AddRespon(byte[] mPacket,ushort id)
    {
        //Debuger.Log("IS return");
        lock (LPacketRecevieList)
        {
            this.LPacketRecevieList.Enqueue(new KeyValuePair<ushort, byte[]>(id, mPacket));
        }
    }

    public void ClearRequest() {
        lock (LPacketRecevieList)
        {
            this.LPacketRecevieList.Clear();
        }
    }
    public enum SocketStatus
    {
        Close,
        ConnectDone,
        Connecting,
        ConnetError,
		ReadError,
		SendError,
        Ping,
        StartConnect,
    }
    public SocketStatus Status = SocketStatus.Close;
    public Exception exceptionError;
    public bool IsConnected { get { return Status == SocketStatus.Connecting; } }

    //public string SocketIpInfo = "";
    public string ipInfo = "";
    public long endConnectTime = 0;
    public string defUrl;
    public void Update()
    {
        switch (Status)
        {
            case SocketStatus.Ping:
                ipInfo = host + ":" + port;
               // if (pingData == null)
                //{
                    //defUrl = "49.51.66.11";
                    //ipUrl = "123fdsall-logic-aoa.ldoverseas.com";
                    //pingData = new PingData(host, defUrl, port);
                    //SocketIpInfo = pingData.dnsIp + " " + ipInfo;
                //}
                Status = SocketStatus.StartConnect;
                //Debug.Log("【PingSocket:" + ipInfo + "】(" + pingData.pingTime + "," + pingData.dnsTime + "):" + pingData.dnsIp + "\n" + SystemInfoUtil.GetIPDns2());
                endConnectTime = System.DateTime.Now.Ticks;
              //  GoogleAnsSdk.LogEvent("SocketStart", pingData.finalUrl, SocketIpInfo, pingData.dnsTime, pingData.dnsIp);
                muSocket.SocketConnection(host, port);
                break;
            case SocketStatus.StartConnect:
                break;
            case SocketStatus.Connecting:
                DealResponPacket();
                break;
            case SocketStatus.ConnectDone:
                endConnectTime = (System.DateTime.Now.Ticks - endConnectTime) / 10000;
                Debug.Log("[SocketConnect Ok]" + endConnectTime);
                Status = SocketStatus.Connecting;
           //     GoogleAnsSdk.LogEvent("SocketOK", MSocketService.ipInfo, pingData.finalUrl, endConnectTime, pingData.dnsIp + ":" + ipPort);
                ConnectCall();                
                break;
            case SocketStatus.ConnetError:
                if (!string.IsNullOrEmpty(defUrl) && host != defUrl)
                {
                    Debug.Log("Socket连接Error:" + host);
                    //       if (exceptConnect != null)
                    //            GoogleAnsSdk.LogEvent("SocketError1", exceptConnect.Message, SocketIpInfo, pingData.dnsTime, pingData.dnsIp);
                    host = defUrl;
                    Status = SocketStatus.Ping;
                    return;
                }
                //UIFactory.Instance.FloatTipError("NetError4", true);         
                ConnectErrorCall(exceptionError);
                Close(false);
                //GameManager.LuaCallMethod("uifactory.AlertTip", "Error:" + exceptConnect.Message);
                //UIFactory.Instance.HiddenWaitBar();
                break;
			case SocketStatus.ReadError:
				this.ConnectErrorCall(exceptionError);
                Close(false);
                break;
			case SocketStatus.SendError:
				this.ConnectErrorCall(exceptionError);
                Close(false);
                break;
            default:
                break;
        }
    }

    //StringBuilder sb = new StringBuilder();
    static int sbNum = 0;
    static float curT;
    static int lastNum = 0;
    static float curQps = 0f;
    public void DealResponPacket()
    {
        //curT += Time.deltaTime;
        //if (curT >= 1f)
        //{
        //    curQps = (sbNum - lastNum) / curT;
        //    curT = 0f;
        //    lastNum = sbNum;
        //    Debug.Log("Qps:" + curQps + ",index:" + sbNum + ",size:" + MUnitySocket.lPacketReceiveCount);
        //}
        //MUtil.BeginSample("Qps:" + curQps + ",index:" + sbNum + ",size:" + MUnitySocket.lPacketReceiveCount + ",time:" + t.ToString("f3"));
        //MUtil.EndSample();
        if (LPacketRecevieList.Count > 0)
        {
            lock (LPacketRecevieList)
            {
                while (LPacketRecevieList.Count > 0)
                {
                    KeyValuePair<ushort, byte[]> mpr = LPacketRecevieList.Dequeue();
                    try
                    {
                        //    string str = System.DateTime.Now.ToString("HH：mm：ss") + ":" + "[" + mpr.Length + "]," + BitConverter.ToString(mpr).Replace("-","");
                        sbNum++;
                        //MUtil.BeginSample("[Socket]");
                        if (OnRespon != null)
                            OnRespon(mpr.Key, mpr.Value);
                        //MUtil.EndSample();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                    //LPacketRecevieList.RemoveAt(0);
                }
            }
        }
    }
    public MUnitySocket muSocket;
    //private MsgHandler msgHandler;
    //public MsgHandler GetMsgHandler() { return msgHandler; }
    public Action OnConnected;
    public Action<MSocketService.SocketStatus, Exception> OnError;
    public Action OnDisconnected;
    public Action<ushort, byte[]> OnRespon;
    public string host = "";
    public int port = 0;
    public void Connect(string _host,int _port)
    {
        this.host = _host;
        this.port = _port;
        this.Connect();
    }
    public void Connect()
    {
        if (muSocket != null) {
            muSocket.Close();
        }
		muSocket = null;
        if (muSocket == null)
            muSocket = new MUnitySocket(this);
        this.Status = SocketStatus.Ping;
    }
    public void ConnectComplted()
    {
        Status = SocketStatus.ConnectDone;
    }
    public void SocketError(SocketStatus statusError, Exception expError)
    {
        this.Status = statusError;
        exceptionError = expError;
    }
    public void ConnectCall()
    {
        if (OnConnected != null)
            OnConnected();
    }
    public void ConnectErrorCall(Exception e)
    {
        //   GoogleAnsSdk.LogEvent("SocketError2", exceptConnect.Message, SocketIpInfo, pingData.dnsTime, pingData.dnsIp);
        Debug.LogError("["+ this.Status + "]:" + e.Message + "\n" + e.StackTrace);
        if (this.OnError != null)
            this.OnError(this.Status, e);
    }

    void OnDestroy()
    {
        OnApplicationQuit();
    }

    public void OnApplicationQuit()
    {
        this.Close();
    }

    public void Close(bool isManul = true)
    {
        if (Status == SocketStatus.Close)
        {
            return;
        }
        Debug.Log(this.Name + " Socket service close.............................");
        Status = SocketStatus.Close;
        if (muSocket != null)
        {
            muSocket.Close();
            muSocket = null;
        }
        if (isManul && OnDisconnected != null)
            OnDisconnected();
    }
}
