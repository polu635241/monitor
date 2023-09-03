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

namespace MonitorServer.Windows
{
    /// <summary>
    /// AppMonitorResult.xaml 的互動邏輯
    /// </summary>
    public partial class AppMonitorResult : UserControl
    {
        public AppMonitorResult ()
        {
            InitializeComponent ();

            this.DataContextChanged += OnDataContextChange;
        }

        void OnDataContextChange (object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null)
            {
                runTimeMonitorData = DataContext as RunTimeMonitorData;

                PIDs.ItemsSource = runTimeMonitorData.pids.ToList ();
            }
        }

        RunTimeMonitorData runTimeMonitorData;
    }
}
