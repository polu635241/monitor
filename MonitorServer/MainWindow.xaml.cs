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

namespace MonitorServer
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public bool inEditor { get; private set; }

        public MainWindow ()
        {
            var args = Environment.GetCommandLineArgs ();

            inEditor = args.Contains ("inEditor");

               Instance = this;
            InitializeComponent ();

            this.Closing += Window_Closing;

            NetworkSetting networkSetting = ParseManager.LoadJson<NetworkSetting> ();
            MonitorSetting monitorSetting = ParseManager.LoadJson<MonitorSetting> ();

            cacheClients = networkSetting.clients.ConvertAll (client=> 
            {
                CacheClient cacheClient = new CacheClient ();
                cacheClient.Init (client, networkSetting.reconnectTime, monitorSetting, OnModify, OnConnectChange);
                return cacheClient;
            });

            cacheClients.ForEach (c => c.RevertToTemp ());

            OnConnectChange ();

            cacheClients.ForEach (c => c.DoConnect ());

            CacheClientControls.ItemsSource = cacheClients;

            curSelectIndex = 0;

            if (cacheClients.Count > 0) 
            {
                CacheClientControls.SelectedIndex = 0;

                UpdateSelectData ();
            }
        }

        int curSelectIndex = 0;

        void OnConnectChange () 
        {
            var connectCount = cacheClients.Count (c => c.IsConnect);

            ConnectStatusText.Content = $"{connectCount}/{cacheClients.Count}";
        }

        void OnModify () 
        {
            MainWindow.Instance.Dispatcher.Invoke (() =>
            {
                CacheClientControls.ItemsSource = new List<CacheClient> ();

                CacheClientControls.ItemsSource = cacheClients;

                UpdateSelectData ();
            });
        }

        List<CacheClient> cacheClients = new List<CacheClient> ();

        void Window_Closing (object sender, CancelEventArgs e) 
        {
            cacheClients.ForEach (c => c.Dispose ());
        }

        void onClientSelect (object sender, SelectionChangedEventArgs e)
        {
            var index = CacheClientControls.SelectedIndex;

            if (index >= 0) 
            {
                curSelectIndex = index;
                UpdateSelectData ();
            }
        }

        void UpdateSelectData () 
        {
            SelectClient.DataContext = null;
            SelectClient.DataContext = cacheClients[curSelectIndex];
        }
    }
}
