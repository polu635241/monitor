using MonitorCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            networkClient = new NetworkClient ();
            networkClient.Init ();

            updateTimer.Interval = TimeSpan.FromSeconds (deltaTime);
            updateTimer.Tick += OnUpdate;
            updateTimer.Start ();


            this.Closing += Window_Closing;
        }

        NetworkClient networkClient;

        const float deltaTime = 1 / 10f;

        DispatcherTimer updateTimer = new DispatcherTimer ();

        void OnUpdate (object sender, EventArgs e)
        {
            LauncherLogControl.Update (deltaTime);
            networkClient.Update (deltaTime);
        }

        void Window_Closing (object sender, CancelEventArgs e)
        {
            networkClient.Dispose ();
        }
    }
}
