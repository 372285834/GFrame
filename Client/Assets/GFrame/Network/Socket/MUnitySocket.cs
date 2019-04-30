using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Text;
using System.Threading;
using MSocket.Type;
using System.IO;

public class MUnitySocket
{
    public Socket mSocket = null;
    public Thread readThread = null;
    public Thread sendThread = null;
    private MSocketResponThread socketRespon = null;
    private MSocketSendThread socketSend = null;
    string sLocalIP;
    int iLocalPort;
    public MSocketService socketService;
    public MUnitySocket(MSocketService socketService)
    {
        receivePool = new byte[bufferSize];
        memStream = new MemoryStream();
        reader = new BinaryReader(memStream);
        this.socketService = socketService;
    }
    public void SocketConnection(string LocalIP, int LocalPort)
    {
        sLocalIP = LocalIP;
        iLocalPort = LocalPort;
        Thread cThread = new Thread(Connect);
        cThread.Start();
    }
    /// <summary>
    /// 创建一个SOCKET连接
    /// </summary>
    public void Connect()
    {
        Debug.Log("["+ socketService.Name+"] StartConnect " +sLocalIP + ":" + iLocalPort);
        //mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            String newServerIp = "";
            AddressFamily newAddressFamily = AddressFamily.InterNetwork;
            IPv6SupportMidleware.getIPType(sLocalIP, iLocalPort.ToString(), out newServerIp, out newAddressFamily);
            if (!string.IsNullOrEmpty(newServerIp)) { sLocalIP = newServerIp; }
            mSocket = new Socket(newAddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //mSocket.Blocking = false;
            //IPAddress ip = IPAddress.Parse(sLocalIP);
            //IPEndPoint ipe = new IPEndPoint(ip, iLocalPort);
            //mSocket.Connect(ipe);
            mSocket.Connect(sLocalIP, iLocalPort);
            
            //启动接受线程
            //mSocket.BeginReceive(GameReadPacket, 0, 10240, SocketFlags.None, new System.AsyncCallback(ReceiveGameServerCallBack), GameReadPacket);
            socketRespon = null;
            socketRespon = new MSocketResponThread(this);

            socketSend = null;
            socketSend = new MSocketSendThread(this);
            
            socketService.ConnectComplted();
        }
        catch (Exception e)
        {
            //Debug.LogError("Socket connect error...." + e.Message);
            //            ConnectReTry();
            mSocket = null;
            socketRespon = null;
            readThread = null;
            socketService.SocketError(MSocketService.SocketStatus.ConnetError, e);
            //								MonoBehaviour.print (e.ToString ());  
        }
    }
	public void Close()
    {
		if (mSocket == null)
			return;		
        if (socketRespon != null)
        {
            socketRespon.Close();
            socketRespon = null;
        }

        if (socketSend!= null)
        {
            socketSend.Close();
            socketSend = null;
        }		
		mSocket.Close(0);
		mSocket = null;
    }

    /// <summary>
    /// 添加一个上行请求
    /// </summary>
    /// <param name="mRequest"></param>
    public void AddRequest(byte[] bts)
    {
        if (socketSend != null)
            socketSend.AddRequest(bts);
    }
    public static long lPacketSendCount = 0;
    /// <summary>
    /// Send the specified packet.
    /// 发送协议包
    /// </summary>
    /// <param name="packet">Packet.</param>
    public void Send(byte[] bts)
    {
        if (bts == null)
            return;
        byte[] data = bts;
        //		MTools.PrintByteArray(data);
        if(data != null)
        {
            mSocket.Send(data);
            int lenSend = data.Length;
            socketService.ByteSizeSend += lenSend;
            lPacketSendCount = lPacketSendCount + lenSend;
            //MDebuger.Log((lPacketSendCount / 1024 + "kb") + ":[Send]:" + lenSend);
        }
        else
        {
            Debug.LogError("Error: Null data Send Request ..");
        }
        //if (packet.ProtoBuf != null)
        //    Debuger.Log(packet.ProtoBuf.ProtoCMD + ":" + data.Length + ",S:" + ":Send ok...");
    }
    public static long lPacketReceiveCount = 0;
    byte[] shortByte = new byte[2];
    byte[] intByte = new byte[4];
    int HeaderSize = 4;
    private MemoryStream memStream;
    private BinaryReader reader;
    static int bufferSize = 65536;//64k is default setting
    byte[] receivePool;
    public void OnReceive()
    {
        int len = mSocket.Receive(receivePool, bufferSize, SocketFlags.None);
        memStream.Seek(0, SeekOrigin.End);
        memStream.Write(receivePool, 0, len);
        //Reset to beginning
        memStream.Seek(0, SeekOrigin.Begin);
        long oldSize = remainingBytes;
        while (remainingBytes >= HeaderSize)
        {
            byte[] pHead = reader.ReadBytes(HeaderSize);
            ushort pValue = TypeConvert.getUShort(pHead, 0);// BitConverter.ToUInt16(pHead, 0);
            int msgLength = pValue - 2;
            if (remainingBytes >= msgLength)
            {
                ushort id = TypeConvert.getUShort(pHead, 2); //BitConverter.ToUInt16(pHead, 2);
                byte[] bytes = reader.ReadBytes(msgLength);
                socketService.AddRespon(bytes, id);
            }
            else
            {
                memStream.Position -= HeaderSize;
                break;
            }
        }
        long newSize = remainingBytes;
        if (oldSize != newSize)
        {
            if (newSize > 0)
            {
                byte[] leftover = reader.ReadBytes((int)newSize);
                memStream.SetLength(0);
                memStream.Write(leftover, 0, leftover.Length);
            }
            else
            {
                memStream.SetLength(0);
            }
        }
    }
    private long remainingBytes
    {
        get { return memStream.Length - memStream.Position; }
    }
    public byte[] ReceiveSocketInfo(out ushort id)
    {
        byte[] pHead = ReadSocket(HeaderSize);
        ushort pValue = TypeConvert.getUShort(pHead, 0); //BitConverter.ToUInt16(pHead, 0);
        id = 0;
        id = TypeConvert.getUShort(pHead, 2);//BitConverter.ToUInt16(pHead, 2);//
        pValue -= 2;
        if (pValue <= 0)
        {
            Debug.LogError("【SocketPacket length is 0】");
            return null;
        }
        byte[] dataPacket = ReadSocket(pValue);
        return dataPacket;
    }
    public byte[] ReadSocket(int lenPacket)
    {
        lPacketReceiveCount = lPacketReceiveCount + lenPacket;
        socketService.ByteSizeReceive += lenPacket;
        int lenReveive = 0;
        byte[] dataPacket = new byte[lenPacket];
        while (true)
        {
            if (lenReveive >= lenPacket)
                break;
            int lenLeft = lenPacket - lenReveive;
            int readSize = lenLeft >= bufferSize ? bufferSize : lenLeft;
            int lenRead = mSocket.Receive(receivePool, readSize, SocketFlags.None);
            if(lenRead > 0)
            {
                Buffer.BlockCopy(receivePool, 0, dataPacket, lenReveive, lenRead);
                lenReveive += lenRead;
            }
            //Array.Copy(receivePool, 0, dataPacket, lenReveive, lenRead);
            //for (int i = lenReveive; i < lenReveive + lenRead; i++)
            //{
            //    dataPacket[i] = receivePool[i - lenReveive];
            //}
            // Thread.Sleep(10);   
        }
        return dataPacket;
    }
    public static void ClearBytes(byte[] bts)
    {
        int num = bts.Length;
        for (int i = 0; i < num; i++)
        {
            bts[i] = 0;
        }
    }

}  
