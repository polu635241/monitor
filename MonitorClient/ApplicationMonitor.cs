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
        public void BindOnMonitor (Action<MonitorResult> onMonitor)
        {
            this.onMonitor = onMonitor;
        }

        /// <summary>
        /// 每個Server會各自提出自己想檢測的部分,
        /// 進來後動態合併
        /// </summary>
        /// <param name="setting"></param>
        public void UpdateSettings (List<MonitorSetting> settings, bool checkImmediately)
        {
            this.settings = settings.ToList ();

            OnSettingUpdate (checkImmediately);
        }

        List<MonitorSetting> settings = new List<MonitorSetting> ();

        Action<MonitorResult> onMonitor;

        void OnSettingUpdate (bool checkImmediately)
        {
            //目前setting裡面只有name,
            //先用name產生唯一列表再轉回來
            var appNames = settings.SelectMany (setting => setting.monitorApps).ToList ().ConvertAll (setting => setting.appName);
            appNames = appNames.Distinct ().ToList ();

            this.mergeSetting = new MonitorSetting ();
            this.mergeSetting.monitorApps = appNames.ConvertAll (appName => new ApplicationSetting (appName));

            if (checkImmediately) 
            {
                DoCheck ();
                timer = 0f;
            }
        }

        public void Update (float deltaTime) 
        {
            if (mergeSetting != null)
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

        MonitorSetting mergeSetting;

        void DoCheck () 
        {
            var processes = System.Diagnostics.Process.GetProcesses ().ToList ();

            MonitorResult result = new MonitorResult ();

            mergeSetting.monitorApps.ForEach (app=> 
            {
                var pids = processes.FindAll (p => p.ProcessName == app.appName).ConvertAll (p => p.Id);
                pids.Sort ();

                MonitorData monitorData = new MonitorData ()
                {
                    appName = app.appName,
                    pids = pids.ToList ()
                };

                result.monitorDatas.Add (monitorData);
            });

            onMonitor.Invoke (result);
        }
    }
}
