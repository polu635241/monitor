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

        public void Init () 
        {
            monitor = new ApplicationMonitor ();

            monitor.BindOnMonitor (OnMonitorAppModify);

            IPEndPoint ipEndPoint = new IPEndPoint (IPAddress.Any, ChannelTransport.port);
            receiverSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            receiverSocket.Bind (ipEndPoint);
            receiverSocket.Listen (int.MaxValue);

            BeginWaitServer ();
        }

        void OnMonitorAppModify (MonitorResult monitorResult)
        {
            LoggerRouter.WriteLine (JsonUtility.ToJson (monitorResult));

            serverCaches.ForEach (server => server.OnMonitorAppModify (monitorResult));
        }

        /// <summary>
        /// 等待Server連線進來
        /// </summary>
        void BeginWaitServer ()
        {
            Task.Run (() =>
            {
                var newServerSocket = receiverSocket.Accept ();

                LoggerRouter.WriteLine ($"server連線成功 {newServerSocket.RemoteEndPoint}");
                ServerCache serverCache = new ServerCache (newServerSocket, locker, this);

                lock (locker) 
                {
                    serverCaches.Add (serverCache);
                }

                serverCache.SendInitPack ();
            });
        }

        object locker = new object ();

        public void OnReceiveRebootCmd (RebootCmd rebootCmd) 
        {
            if (MainWindow.Instance.inEditor == false)
            {
                Process.Start ("shutdown", "/r /t 0 /f");
            }
        }

        public void OnAnyServerSettingModify (bool checkImmediately) 
        {
            var settings = serverCaches.ConvertAll (server => server.MonitorSetting);

            monitor.UpdateSettings (settings, checkImmediately);
        }

        public void OnServerDisconnect (ServerCache serverCache) 
        {
            serverCaches.Remove (serverCache);

            OnAnyServerSettingModify (false);
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
            receiverSocket.Close ();

            serverCaches.ForEach (serverCache => serverCache.Dispose ());
            serverCaches.Clear ();
        }

        List<ServerCache> serverCaches = new List<ServerCache> ();
    }

}
