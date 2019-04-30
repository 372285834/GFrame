using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class MSocketSendThread
{
    MUnitySocket muSocket;
    MSocketService socketService;

    public Thread sendThread = null;

    Queue<byte[]> LPacketRequestList = new Queue<byte[]>(0);
    public MSocketSendThread(MUnitySocket muSocket)
    {        
        this.muSocket = muSocket;
        this.socketService = muSocket.socketService;
        this.isRunning = true;
        sendThread = new Thread(new ThreadStart(run));
        sendThread.Start();
    }

    //Thread threadPost;
    //public void Send()
    //{
    //    SendRequest();
    //    //		threadPost = new Thread(SendRequest);
    //    //		threadPost.Start();

    //}

    public bool isRunning = false;
	public bool isMannal = false;

    public void run()
    {
        while (isRunning)
        {
            try
            {
                while (isRunning && LPacketRequestList.Count > 0)
                {
                    byte[] bts = null;
                    lock (LPacketRequestList)
                    {
                        if (LPacketRequestList.Count > 0)
                            bts = LPacketRequestList.Dequeue();
                    }
                    if (!isRunning)
                        break;
                    this.muSocket.Send(bts);
                }
                Thread.Sleep(10);
            }
            catch (Exception e)
            {
				if (!isMannal) {
					Close();
                    socketService.SocketError (MSocketService.SocketStatus.SendError, e);
					//MSocketService.Instance.SendError(e);
					//MUserService.Instance.SocketSendError(e);                
					//Debuger.LogError("Send err:" + e.Message);
					//Debuger.LogError("Send err:" + e.StackTrace);                
					break;
				}                

            }
        }
    }

    public void AddRequest(byte[] bts)
    {
        lock (LPacketRequestList)
        {
            LPacketRequestList.Enqueue(bts);
        }
    }


    public void Close() {
		isMannal = true;
		isRunning = false;
        lock (LPacketRequestList)
        {
            LPacketRequestList.Clear();
        }
        //sendThread.Abort();               
    }
    //public void SendRequest()
    //{
    //    try
    //    {
    //        //this.muSocket.Send(this.socketService.LPacketRequestList[0]);
    //        //this.socketService.LPacketRequestList.RemoveAt(0);
    //        //MSocketService.IsSend = true;
    //    }
    //    catch (Exception e)
    //    {
    //        Debuger.Log(e.Message);
    //        this.socketService.SendError(e);
    //        //MUserService.Instance.ReConnect();
    //    }
    //}
}
