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
using System.Windows.Media;

namespace MonitorServer
{
    public class CacheClient : INotifyPropertyChanged
    {
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
            channelTransport.BindEvent<ClientReqResult> (SysEvents.ClientReq, OnClientReq);
        }

        Action onModify;

        void OnClientReq (ClientReqResult req) 
        {
            SetComputerName (req.computerName);

            channelTransport.SendMsg (SysEvents.UpdateMonitorSetting, monitorSetting);
        }

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
                    OnConnect ();
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

        void OnConnect () 
        {
            IsConnect = true;

            OnConnectStatusChange ();
        }

        void OnConnectStatusChange () 
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke (this, new PropertyChangedEventArgs (nameof (NetStatusMsg)));
                PropertyChanged.Invoke (this, new PropertyChangedEventArgs (nameof (UIEnable)));
                PropertyChanged.Invoke (this, new PropertyChangedEventArgs (nameof (NetStatusColor)));
            }
        }

        void SetComputerName (string computerName)
        {
            this.computerName = computerName;

            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke (this, new PropertyChangedEventArgs (nameof (CacheClient.computerName)));
            }
        }

        void OnDisconnect () 
        {
            IsConnect = false;

            SetComputerName ("");

            OnConnectStatusChange ();

            RevertToTemp ();

            if (channelTransport.ManualDisconnect == false)
            {
                DoConnect ();
            }
        }

        IAsyncResult IAsyncResult;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool UIEnable => IsConnect;

        public Brush NetStatusColor
        {
            get
            {
                Color color = default;

                if (IsConnect)
                {
                    color = (Color)ColorConverter.ConvertFromString ("green");
                }
                else
                {
                    color = (Color)ColorConverter.ConvertFromString ("red");
                }

                return new SolidColorBrush (color);
            }
        }

        public string computerName { get; private set; } = "";

        public string NetStatusMsg 
        {
            get 
            {
                if (IsConnect) 
                {
                    return "已成功連線";
                }
                else
                {
                    return "尚未連線";
                }
            }
        }

        public bool IsConnect { get; private set; }

        public void Dispose () 
        {
            if (IsConnect) 
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
