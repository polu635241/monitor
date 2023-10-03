using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorCore;

namespace MonitorClient
{
    public class BindingCache
    {
        public MonitorSetting setting;
        public Action<MonitorResult> onModify;

        public BindingCache (MonitorSetting setting, Action<MonitorResult> onModify) 
        {
            this.setting = setting;
            this.onModify = onModify;
        }
    }
}
