using UnityEngine;
using System.Collections;
using System;
using System.Threading;

class MSocketResponThread
{

    MUnitySocket socket;
    public Thread readThread = null;

    public MSocketResponThread(MUnitySocket socket)
    {
        this.socket = socket;
        this.socketService = socket.socketService;
        this.isRunning = true;
        readThread = new Thread(new ThreadStart(run));
        readThread.Start();
        numsNull = 0;
    }
    public MSocketService socketService;

    public bool isRunning = true;
	public bool isMannal = false;
    public int numsNull = 0;
    public void run()
    {
        while (isRunning)
        {
            try
            {
                if (!isRunning)
                {
                    break;
                }
                 socket.OnReceive();
                //if (numsNull > 100)
                //{
                //    isRunning = false;
                //    //socketService.Close(null);
                //    throw new Exception("[packet length is 0]");
                //}
                ////Debug.Log("Reading......");
                //ushort id = 0;
                //byte[] dataRecive = socket.ReceiveSocketInfo(out id);
                //if (dataRecive != null)
                //{
                //    numsNull = 0;
                //    socketService.AddRespon(dataRecive, id);
                //    //MDebuger.Log("[Recive]:" + dataRecive.Length);
                //}
                //else
                //{
                //    numsNull++;
                //}
                //Thread.Sleep(1);
            }
            catch (Exception e)
            {
				if (!isMannal) {
					Close();
                    //MSocketService.Instance.ReadError(e);
                    socketService.SocketError (MSocketService.SocketStatus.ReadError, e);
					//Debuger.LogError("0Read err:" + e.Message);
					//Debuger.LogError("1Read err:" + e.StackTrace);                
					//socketService.Close();
					break;
				}                
            }
        }
    }


    public void Close()
    {
		isMannal = true;
        isRunning = false;
        //readThread.Abort();
    }


}