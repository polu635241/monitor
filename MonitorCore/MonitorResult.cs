using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorCore
{
    [Serializable]
    public class ClientReqResult 
    {
        public string computerName;
    }

    [Serializable]
    public class MonitorResult
    {
        public List<MonitorData> monitorDatas = new List<MonitorData> ();
    }

    [Serializable]
    public class MonitorData
    {
        public string appName;

        public List<int> pids = new List<int> ();
    }
}
