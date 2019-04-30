using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// 这个脚本只是演示如何使用ipv6SupportMiddleWare，不用导入到你的工程中去。
/// </summary>
public class TestHowToDo
{
    private Socket gameSocket;
    private byte[] GameReadPacket = new byte[10240];

    public bool ConectToServer(string ipv4, int port)
    {
     
        try
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                //use ipv6 to connect to gameserver
                String newServerIp = "";
                AddressFamily newAddressFamily = AddressFamily.InterNetwork;
                IPv6SupportMidleware.getIPType(ipv4, port.ToString(), out newServerIp, out newAddressFamily);
                if (!string.IsNullOrEmpty(newServerIp)) { ipv4 = newServerIp; }

                gameSocket = new Socket(newAddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Debug.Log("Socket AddressFamily :" + newAddressFamily.ToString() + "ServerIp:" + ipv4);
                gameSocket.Connect(ipv4, port);
                gameSocket.BeginReceive(GameReadPacket, 0, 10240, SocketFlags.None, new System.AsyncCallback(ReceiveGameServerCallBack), GameReadPacket);

                Debug.Log("连接游戏服务器成功！");
                return true;
            }
            else
            {
                Debug.LogError("连接游戏服务器失败:当前无网络连接！");
                return false;
            }
        }
        catch (SocketException _sex)
        {
            //网络连接异常
            Debug.LogError("连接游戏服务器异常！详细描述：" + _sex.Message);
            return false;
        }
    }


    /// <summary>
    /// 接收游戏服务器网络消息回调函数
    /// </summary>
    /// <param name="ar"></param>
    public void ReceiveGameServerCallBack(System.IAsyncResult ar)
    {
       
    }
}
