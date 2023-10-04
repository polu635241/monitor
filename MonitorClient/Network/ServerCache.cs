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
    public class ServerCache
    {
        public ServerCache (Socket serverSocket, object locker, NetworkClient networkClient)
        {
            this.serverSocket = serverSocket;
            this.locker = locker;
            this.networkClient = networkClient;

            channelTransport = new ChannelTransport ();
            channelTransport.BindingSocket (serverSocket, OnServerDisconnect);
            channelTransport.BindEvent<MonitorSetting> (SysEvents.UpdateMonitorSetting, OnMonitorSettingUpdate);
            channelTransport.BindEvent<RebootCmd> (SysEvents.RebootCmd, networkClient.OnReceiveRebootCmd);
        }

        public void SendHeartBeat () 
        {
            if (channelTransport.isConnect) 
            {
                HeartBeatMsg heartBeatMsg = new HeartBeatMsg ();
                heartBeatMsg.ticks = DateTime.Now.Ticks;

                channelTransport.SendMsg (SysEvents.HeartBeat, heartBeatMsg);
            }
        }

        public void SendInitPack () 
        {
            ClientReqResult clientReqResult = new ClientReqResult ();
            clientReqResult.computerName = Environment.MachineName;

            channelTransport.SendMsg (SysEvents.ClientReq, clientReqResult);
        }

        NetworkClient networkClient;

        Socket serverSocket = null;

        object locker;

        ChannelTransport channelTransport;

        void OnMonitorSettingUpdate (MonitorSetting monitorSetting) 
        {
            this.MonitorSetting = monitorSetting;

            lock (locker) 
            {
                networkClient.OnAnyServerSettingModify (true);
            }
        }

        public MonitorSetting MonitorSetting { get; private set; } = new MonitorSetting ();

        public void OnMonitorAppModify (MonitorResult monitorResult, out bool hasModify) 
        {
            var localResult = new MonitorResult ();
            localResult.monitorDatas = monitorResult.monitorDatas.FindAll (data => MonitorSetting.monitorApps.Exists (app=> 
            {
                return data.appName == app.appName;
            }));

            var modfiy = cacheResult.GetModify (localResult);

            hasModify = modfiy.HasValue;

            cacheResult = localResult;

            if (modfiy.HasValue) 
            {
                try
                {
                    if (channelTransport.isConnect)
                    {
                        channelTransport.SendMsg (SysEvents.UpdateMonitorResult, localResult);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine (e.StackTrace);
                    Console.WriteLine (e.Message);
                }
            }
            
        }

        MonitorResult cacheResult = new MonitorResult ();

        void OnServerDisconnect ()
        {
            if (channelTransport.ManualDisconnect == false)
            {
                LoggerRouter.WriteLine ($"連接已斷開{serverSocket.RemoteEndPoint}");
            }
            serverSocket = null;

            lock (locker) 
            {
                networkClient.OnServerDisconnect (this);
            }
        }

        public void Dispose ()
        {
            channelTransport.Disconnect ();
        }
    }
}
