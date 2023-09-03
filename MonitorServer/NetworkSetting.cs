using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorServer
{
    [Serializable]
    public class NetworkSetting
    {
        public List<string> clients = new List<string> ();

        public float reconnectTime;
    }
}
