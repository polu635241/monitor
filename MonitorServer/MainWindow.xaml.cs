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

        public MainWindow ()
        {
            Instance = this;
            InitializeComponent ();

            this.Closing += Window_Closing;

            NetworkSetting networkSetting = ParseManager.LoadJson<NetworkSetting> ();

            cacheClients = networkSetting.clients.ConvertAll (client=> 
            {
                CacheClient cacheClient = new CacheClient ();
                cacheClient.Init (client, networkSetting.reconnectTime);
                return cacheClient;
            });

            cacheClients.ForEach (c => c.DoConnect ());
        }

        List<CacheClient> cacheClients = new List<CacheClient> ();

        void Window_Closing (object sender, CancelEventArgs e) 
        {
            cacheClients.ForEach (c => c.Dispose ());
        }
    }
}
