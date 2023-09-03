using MonitorCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonitorServer
{
    public class CacheClient : INotifyPropertyChanged
    {
        public bool isConnect;

        Socket clientSocket;

        public void Init (string ip, float reconnectTime, MonitorSetting monitorSetting, Action onModify)
        {
            this.onModify = onModify;

            this.ip = ip;
            this.reconnectTime = reconnectTime;

            this.monitorSetting = monitorSetting;

            RevertToTemp ();

            channelTransport = new ChannelTransport ();
            channelTransport.BindEvent<MonitorResult> (SysEvents.UpdateMonitorResult, OnMonitorResultUpdate);
        }

        Action onModify;

        void OnMonitorResultUpdate (MonitorResult newRes) 
        {
            this.MonitorResult = newRes;

            RebuildRunTimeMonitorDatas ();
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

            RebuildRunTimeMonitorDatas ();
        }

        float reconnectTime;

        public string clientIP => ip;

        public ICommand OnClickReboot 
        {
            get 
            {
                return new GenericCmd (OnClickRebootCmd);
            }
        }

        void OnClickRebootCmd () 
        {
            RebootCmd cmd = new RebootCmd ();
            channelTransport.SendMsg (SysEvents.RebootCmd, cmd);
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsConnect { get; private set; }

        public void Dispose () 
        {
            if (isConnect) 
            {
                channelTransport.Disconnect ();
            }
        }

        void RebuildRunTimeMonitorDatas ()
        {
            runTimeMonitorDatas = MonitorResult.monitorDatas.ConvertAll (data => new RunTimeMonitorData (data));

            onModify.Invoke ();
        }

        public List<RunTimeMonitorData> runTimeMonitorDatas;
    }

    public class RunTimeMonitorData : INotifyPropertyChanged
    {
        public RunTimeMonitorData (MonitorData monitorData) 
        {
            this.bindingData = monitorData;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        MonitorData bindingData;

        public string appName => bindingData.appName;

        public int appCount => pids.Count;

        public List<RunTimeMonitorPID> pids => bindingData.pids.ConvertAll (pid => new RunTimeMonitorPID (pid));
    }

    public class RunTimeMonitorPID : INotifyPropertyChanged
    {
        public RunTimeMonitorPID (int pid) 
        {
            this.pid = pid.ToString ();
        }

        public string pid { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
