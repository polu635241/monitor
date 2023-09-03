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

        public void Init (string ip, float reconnectTime, MonitorSetting monitorSetting) 
        {
            this.ip = ip;
            this.reconnectTime = reconnectTime;

            this.monitorSetting = monitorSetting;

            RevertToTemp ();

            channelTransport = new ChannelTransport ();
            channelTransport.BindEvent<MonitorResult> (SysEvents.UpdateMonitorResult, OnMonitorResultUpdate);
        }

        void OnMonitorResultUpdate (MonitorResult newRes) 
        {
            this.MonitorResult = newRes;
        }

        MonitorSetting monitorSetting;

        public MonitorResult MonitorResult { get; private set; }

        /// <summary>
        /// 把result清成空的
        /// </summary>
        void RevertToTemp ()
        {
            MonitorResult = new MonitorResult ();

            MonitorResult.monitorDatas = monitorSetting.monitorApps.ConvertAll (app=> 
            {
                MonitorData monitorData = new MonitorData ();
                monitorData.appName = app.appName;
                return monitorData;
            });

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
                    channelTransport.SendMsg (SysEvents.UpdateMonitorSetting, monitorSetting);
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
            RevertToTemp ();

            DoConnect ();
        }

        IAsyncResult IAsyncResult;

        public bool IsConnect { get; private set; }

        public void Dispose () 
        {
            if (isConnect) 
            {
                channelTransport.Disconnect ();
            }
        }
    }
}
