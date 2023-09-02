using MonitorCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MonitorClient
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow ()
        {
            Instance = this;

            InitializeComponent ();

            updateTimer.Interval = TimeSpan.FromSeconds (deltaTime);
            updateTimer.Tick += OnUpdate;
            updateTimer.Start ();

            monitorSetting = ParseManager.LoadJson<MonitorSetting> ();

            monitor = new ApplicationMonitor ();

            monitor.BindOnModify ((res) =>
            {
                LoggerRouter.WriteLine (JsonUtility.ToJson (res));
            });
            monitor.UpdateSetting (monitorSetting);
        }

        ApplicationMonitor monitor;

        const float deltaTime = 1 / 10f;

        DispatcherTimer updateTimer = new DispatcherTimer ();

        MonitorSetting monitorSetting;

        void OnUpdate (object sender, EventArgs e)
        {
            LauncherLogControl.Update (deltaTime);
            monitor.Update (deltaTime);
        }
    }
}
