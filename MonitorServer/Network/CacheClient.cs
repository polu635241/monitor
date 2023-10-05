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
        public void Init (string ip, float reconnectTime, MonitorSetting monitorSetting, Action onModify, Action onConnectChange)
        {
            this.onModify = onModify;
            this.onConnectChange = onConnectChange;

            this.ip = ip;
            this.reconnectTime = reconnectTime;

            this.monitorSetting = monitorSetting;

            MonitorResult = new MonitorResult ();
            MonitorResult.CreateTempFromSetting (monitorSetting);

            channelTransport = new ChannelTransport ();
            channelTransport.BindEvent<MonitorResult> (SysEvents.UpdateMonitorResult, OnMonitorResultUpdate);
            channelTransport.BindEvent<ClientReqResult> (SysEvents.ClientReq, OnClientReq);
            channelTransport.BindEvent<HeartBeatMsg> (SysEvents.HeartBeat, OnHeartBeat);
        }

        Action onModify;

        Action onConnectChange;

        void OnHeartBeat (HeartBeatMsg heartBeatMsg)
        {
            DateTime dateTime = new DateTime (heartBeatMsg.ticks);

            var msg = dateTime.ToString ("yyyy-MM-dd HH:mm:ss");

            LoggerRouter.WriteLine ($"收到心跳包 -> {msg}");
        }

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
        public void RevertToTemp ()
        {
            MonitorResult = new MonitorResult ();

            MonitorResult.monitorDatas = monitorSetting.monitorApps.ConvertAll (app =>
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
            MainWindow.Instance.Dispatcher.Invoke (() =>
            {
                IsConnect = true;

                OnConnectStatusChange ();

                onConnectChange.Invoke ();
            });
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

            if (channelTransport.ManualDisconnect == false)
            {
                DoConnect ();
            }

            MainWindow.Instance.Dispatcher.Invoke (() =>
            {
                SetComputerName ("");

                OnConnectStatusChange ();

                RevertToTemp ();

                onConnectChange.Invoke ();
            });
        }

        IAsyncResult IAsyncResult;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool UIEnable => IsConnect;

        public string NetStatusMsg
        {
            get
            {
                if (IsConnect)
                {
                    return "上線";
                }
                else
                {
                    return "下線";
                }
            }
        }

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
                    color = (Color)ColorConverter.ConvertFromString ("#FF2D2D");
                }

                return new SolidColorBrush (color);
            }
        }

        public string AppStatusMsg
        {
            get
            {
                if (EveryActive)
                {
                    return "正常";
                }
                else
                {
                    return "異常";
                }
            }
        }

        public Brush AppStatusColor
        {
            get
            {
                Color color = default;

                if (EveryActive)
                {
                    color = (Color)ColorConverter.ConvertFromString ("green");
                }
                else
                {
                    color = (Color)ColorConverter.ConvertFromString ("orange");
                }

                return new SolidColorBrush (color);
            }
        }

        /// <summary>
        /// 整合了所有狀況的
        /// </summary>
        public Brush FullStatusColor
        {
            get
            {
                Color color = default;

                if (IsConnect) 
                
                {
                    return AppStatusColor;
                }
                else
                {
                    color = (Color)ColorConverter.ConvertFromString ("#FF2D2D");
                }

                return new SolidColorBrush (color);
            }
        }

        public bool IsSelect;

        /// <summary>
        /// 這個物件的顏色
        /// 包含是否選擇
        /// </summary>
        public Brush CellStatusColor
        {
            get
            {
                Color color = default;

                if (IsSelect) 
                {
                    color = (Color)ColorConverter.ConvertFromString ("yellow");
                }
                else
                {
                    
                }

                if (IsConnect)

                {
                    return AppStatusColor;
                }
                else
                {
                    color = (Color)ColorConverter.ConvertFromString ("#FF2D2D");
                }

                return new SolidColorBrush (color);
            }
        }

        public string computerName { get; private set; } = "未連接";

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

        bool EveryActive 
        {
            get 
            {
                if (runTimeMonitorDatas != null) 
                {
                    if (runTimeMonitorDatas.TrueForAll (data => data.HasActive)) 
                    {
                        return true;
                    }
                }

                return false;
            }
        }
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

        public List<int> pids => bindingData.pids.ToList ();

        public bool HasActive => appCount > 0;

        public string appStatus 
        {
            get 
            {
                if (HasActive) 
                {
                    return "已啟動";
                }
                else
                {
                    return "未啟動";
                }
            }
        }

        public Brush appStatusColor
        {
            get
            {
                Color color = default;

                if (HasActive)
                {
                    color = (Color)ColorConverter.ConvertFromString ("green");
                }
                else
                {
                    color = (Color)ColorConverter.ConvertFromString ("#FF2D2D");
                }

                return new SolidColorBrush (color);
            }
        }
    }
}
