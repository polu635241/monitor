using MonitorCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorServer
{
    public class CacheClient
    {
        public bool isConnect;

        Socket clientSocket;

        public void Init (string ip, float reconnectTime) 
        {
            this.ip = ip;
            this.reconnectTime = reconnectTime;

            channelTransport = new ChannelTransport ();
        }

        float reconnectTime;

        string ip;

        TcpClient tcpClient;

        ChannelTransport channelTransport;

        public void DoConnect () 
        {
            tcpClient = new TcpClient ();

            IAsyncResult = tcpClient.BeginConnect (ip, ChannelTransport.port, (ar) =>
            {
                if (tcpClient.Connected)
                {
                    LoggerRouter.WriteLine ($"{ip} 連線成功");
                    channelTransport.BindingSocket (tcpClient.Client, OnDisconnect);
                }
                else
                {
                    LoggerRouter.WriteLine ($"{ip} 連線失敗 等待{reconnectTime}秒後重新連線");
                    Thread.Sleep ((int)(reconnectTime * 1000));

                    DoConnect ();
                }
            }, tcpClient);
        }

        void OnDisconnect () 
        {
            DoConnect ();
        }

        IAsyncResult IAsyncResult;

        public bool IsConnect { get; private set; }

        public void Dispose () 
        {
            if (isConnect) 
            {
                clientSocket.Close ();
            }
        }
    }
}
