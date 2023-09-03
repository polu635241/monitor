using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorCore;

namespace MonitorClient
{
    public class ApplicationMonitor
    {
        public void BindOnModify (Action<MonitorResult> onModify) 
        {
            this.onModify = onModify;
        }

        Action<MonitorResult> onModify;

        public void UpdateSetting (MonitorSetting setting)
        {
            this.setting = setting;

            DoCheck ();
            timer = 0f;
        }

        /// <summary>
        /// 清除Setting
        /// 下次連上才會補傳當前狀況
        /// </summary>
        public void ClearSetting () 
        {
            this.setting = null;
            this.cacheMD5 = "";
        }

        public void Update (float deltaTime) 
        {
            if (setting != null)
            {
                timer += deltaTime;

                if (timer >= CheckRate)
                {
                    DoCheck ();
                    timer = 0f;
                }
            }
        }

        const float CheckRate = 3f;

        float timer;

        MonitorSetting setting;

        void DoCheck () 
        {
            var processes = System.Diagnostics.Process.GetProcesses ().ToList ();

            MonitorResult result = new MonitorResult ();

            setting.monitorApps.ForEach (app=> 
            {
                var pids = processes.FindAll (p => p.ProcessName == app.appName).ConvertAll (p => p.Id);

                MonitorData monitorData = new MonitorData ()
                {
                    appName = app.appName,
                    pids = pids.ToList ()
                };

                result.monitorDatas.Add (monitorData);
            });

            var json = JsonUtility.ToJson (result);

            var newMD5 = MD5Tool.ToMD5 (json);

            if (newMD5 != cacheMD5)
            {
                cacheMD5 = newMD5;

                if (onModify != null)
                {
                    onModify.Invoke (result);
                }
            }
        }


        //用來讓json比對是否有變化的
        string cacheMD5 = "";
    }
}
