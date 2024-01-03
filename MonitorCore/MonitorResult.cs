using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorCore
{
    [Serializable]
    public class MonitorResult
    {
        public List<MonitorData> monitorDatas = new List<MonitorData> ();

        public void CreateTempFromSetting (MonitorSetting monitorSetting) 
        {
            this.monitorDatas = monitorSetting.monitorApps.ConvertAll (app=> 
            {
                MonitorData data = new MonitorData ();
                data.appName = app.appName;
                data.pids = new List<int> ();
                return data;
            });
        }
    }

    [Serializable]
    public class MonitorData
    {
        public string appName;

        public List<int> pids = new List<int> ();
    }
}
