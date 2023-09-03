using MonitorCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorClient
{
    public class NetworkClient
    {
        Socket receiverSocket;

        ChannelTransport channelTransport;

        public void Init () 
        {
            PrepareEvents ();

            monitor = new ApplicationMonitor ();

            monitor.BindOnModify ((res) =>
            {
                LoggerRouter.WriteLine (JsonUtility.ToJson (res));
            });

            //Client等待接收Server訊息
            IPAddress ipAddress = IPAddress.Parse ("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint (ipAddress, ChannelTransport.port);

            receiverSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            receiverSocket.Bind (ipEndPoint);
            receiverSocket.Listen (1);

            BeginWaitServer ();
        }

        /// <summary>
        /// 等待Server連線進來
        /// </summary>
        void BeginWaitServer ()
        {
            Task.Run (() =>
            {
                serverSocket = receiverSocket.Accept ();
                serverIsConnect = true;

                LoggerRouter.WriteLine ($"server連線成功 {serverSocket.RemoteEndPoint}");

                //本來就獨立線了
                //不用再開一條
                channelTransport.BindingSocket (serverSocket, OnServerDisconnect);
            });
        }

        Socket serverSocket = null;

        void OnServerDisconnect () 
        {
            LoggerRouter.WriteLine ($"連接已斷開{serverSocket.RemoteEndPoint.ToString ()}");
            serverSocket = null;
            serverIsConnect = false;
            monitor.ClearSetting ();

            BeginWaitServer ();
        }

        bool serverIsConnect = false;

        void PrepareEvents () 
        {
            channelTransport = new ChannelTransport ();
            channelTransport.BindEvent<MonitorSetting> (SysEvents.UpdateMonitorSetting, monitor.UpdateSetting);
        }

        ApplicationMonitor monitor;

        public void Update (float deltaTime) 
        {
            monitor.Update (deltaTime);
        }

        public void Dispose () 
        {
            receiverSocket.Disconnect (true);
        }
    }

}
