using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MonitorCore
{
    public static class CmdTool
    {
        public static void ProcessCmdAsync (string cmdExeName, string args = "", Action<string> onLog = null, Action<string> onErrorLog = null)
        {
            Task.Run (()=> 
            {
                ProcessCmd (cmdExeName, args, onLog, onErrorLog);
            });
        }

        /// <summary>
        /// 有些指令成功後訊息會帶success字樣
        /// 有些就是單存沒有error
        /// </summary>
        /// <param name="args"></param>
        /// <param name="needCheckSuccess"></param>
        /// <param name="onResult"></param>
        public static void ProcessCmd (string cmdExeName, string args, Action<string> onLog = null, Action<string> onErrorLog = null)
        {
            bool errorHasMsg = false;

            DoCmdCommand (cmdExeName, args, (output) =>
             {
                 var resultMsg = output.ReadToEnd ().ToLower ();

                 if (string.IsNullOrEmpty (resultMsg) == false)
                 {
                     LoggerRouter.WriteLine (resultMsg);
                 }

                 onLog?.Invoke (resultMsg);

             }, (errorResult) =>
             {
                 var errorMsg = errorResult.ReadToEnd ().ToLower ();
                 errorHasMsg = string.IsNullOrEmpty (errorMsg) == false;

                 if (errorHasMsg)
                 {
                     LoggerRouter.WriteLine (errorMsg);
                 }

                 onErrorLog?.Invoke (errorMsg);
             });
        }

        /// <summary>
        /// 完成之前會阻塞thread,
        /// 安裝或是反安裝之類的要用task來做
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="onResult"></param>
        /// <param name="onProcessOutput"></param>
        /// <param name="onProcessError"></param>
        static void DoCmdCommand (string cmdExeName,string arguments, Action<StreamReader> onProcessOutput, Action<StreamReader> onProcessError = null)
        {
            LoggerRouter.WriteLine ($"cmd -> {arguments}");

            System.Diagnostics.Process adbProcess = new System.Diagnostics.Process ();
            adbProcess.StartInfo.CreateNoWindow = true;
            adbProcess.StartInfo.FileName = cmdExeName;
            adbProcess.StartInfo.Arguments = arguments;
            adbProcess.StartInfo.UseShellExecute = false;

            adbProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            adbProcess.StartInfo.RedirectStandardOutput = true;

            if (onProcessError != null)
            {
                adbProcess.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                adbProcess.StartInfo.RedirectStandardError = true;
            }

            adbProcess.Start ();

            using (adbProcess.StandardOutput)
            {
                onProcessOutput.Invoke (adbProcess.StandardOutput);
            }

            if (onProcessError != null)
            {
                using (adbProcess.StandardError)
                {
                    onProcessError.Invoke (adbProcess.StandardError);
                }
            }

            adbProcess.WaitForExit ();

            LoggerRouter.WriteLine ($"{cmdExeName} {arguments} is finish");
        }
    }
}
