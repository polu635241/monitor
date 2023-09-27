using MonitorCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            monitor = new ApplicationMonitor ();

            monitor.BindOnModify (OnMonitorAppModify);

            PrepareEvents ();

            //Client等待接收Server訊息
            IPAddress ipAddress = IPAddress.Parse ("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint (IPAddress.Any, ChannelTransport.port);

            receiverSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            receiverSocket.Bind (ipEndPoint);
            receiverSocket.Listen (1);

            BeginWaitServer ();
        }

        void OnMonitorAppModify (MonitorResult monitorResult) 
        {
            LoggerRouter.WriteLine (JsonUtility.ToJson (monitorResult));

            channelTransport.SendMsg (SysEvents.UpdateMonitorResult, monitorResult);
        }

        /// <summary>
        /// 等待Server連線進來
        /// </summary>
        void BeginWaitServer ()
        {
            Task.Run (() =>
            {
                serverSocket = receiverSocket.Accept ();
                ServerIsConnect = true;

                LoggerRouter.WriteLine ($"server連線成功 {serverSocket.RemoteEndPoint}");

                //本來就獨立線了
                //不用再開一條
                channelTransport.BindingSocket (serverSocket, OnServerDisconnect);
            });
        }

        Socket serverSocket = null;

        void OnServerDisconnect () 
        {
            if (channelTransport.ManualDisconnect == false)
            {
                LoggerRouter.WriteLine ($"連接已斷開{serverSocket.RemoteEndPoint}");
            }
            serverSocket = null;
            ServerIsConnect = false;
            monitor.ClearSetting ();

            if (channelTransport.ManualDisconnect == false) 
            {
                BeginWaitServer ();
            }
        }

        public bool ServerIsConnect { get; private set; } = false;

        void PrepareEvents () 
        {
            channelTransport = new ChannelTransport ();
            channelTransport.BindEvent<MonitorSetting> (SysEvents.UpdateMonitorSetting, monitor.UpdateSetting);
            channelTransport.BindEvent<RebootCmd> (SysEvents.RebootCmd, OnReceiveRebootCmd);
        }

        void OnReceiveRebootCmd (RebootCmd rebootCmd) 
        {
            if (MainWindow.Instance.inEditor == false)
            {
                Process.Start ("shutdown", "/r /t 0 /f");
            }
        }

        ApplicationMonitor monitor;

        public void Update (float deltaTime) 
        {
            if (monitor != null) 
            {
                monitor.Update (deltaTime);
            }
        }

        public void Dispose () 
        {
            if (channelTransport.isConnect) 
            {
                //receiverSocket.Shutdown (SocketShutdown.Both);
            }

            receiverSocket.Close ();

            channelTransport.Disconnect ();
        }
    }

}
