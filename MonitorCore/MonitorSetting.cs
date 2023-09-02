using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorCore
{
    [Serializable]
    public class MonitorSetting
    {
        public List<ApplicationSetting> monitorApps = new List<ApplicationSetting> ();
    }

    [Serializable]
    public class ApplicationSetting
    {
        public string appName;
    }
}
