using MonitorCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Management;
using System.Net.NetworkInformation;

namespace CycleLauncher
{
    class Program
    {
        static void Main (string[] args)
        {
            while (true) 
            {
                Console.ReadLine ();
            }
        }

        static void CycleTask () 
        {
            Config config = ParseManager.LoadJson<Config> ();

            Console.WriteLine ($"目標程序");
            Console.WriteLine ($"監測秒數為 -> {config.rate}");

            config.paths.ForEach (path=> 
            {
                Console.WriteLine ($"{path}");
            });

            List<(string path, string fileName)> fileTable = config.paths.ConvertAll (path=> 
            {
                var fileName = Path.GetFileNameWithoutExtension (path);

                return (path, fileName);
            });


            while (true) 
            {
                var processes = Process.GetProcesses ().ToList ();
                processes.Sort ((a, b) => a.ProcessName.CompareTo (b.ProcessName));
                var waitProcessTable = fileTable.ToList ();

                processes.ForEach (process=> 
                {
                    var pairs = waitProcessTable.FindAll (pair => pair.fileName == process.ProcessName);

                    waitProcessTable.RemoveAll (waitPair => waitPair.fileName == process.ProcessName);

                    if (pairs.Count > 0) 
                    {
                        Console.WriteLine ($"成功尋找到監控程序 {process.ProcessName}");
                    }
                });

                if (waitProcessTable.Count > 0) 
                {
                    waitProcessTable.ForEach (pair=> 
                    {
                        var path = pair.path;

                        if (File.Exists (path)) 
                        {
                            Process.Start (path);
                            Console.WriteLine ($"自動開啟程序 {path}");
                        }
                        else
                        {
                            Console.WriteLine ("路徑不存在無法開啟");
                        }
                    });
                }

                Thread.Sleep (config.rate * 1000);
            }
        }
    }
}
