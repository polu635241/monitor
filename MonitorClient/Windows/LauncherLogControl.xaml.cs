using MonitorCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace MonitorClient.Windows
{
	/// <summary>
	/// LauncherLogControl.xaml 的互動邏輯
	/// </summary>
	public partial class LauncherLogControl : UserControl, INotifyPropertyChanged
	{
		public LauncherLogControl ()
		{
			InitializeComponent ();
			logBuilder = new StringBuilder ();

			//這樣初始化會先寫入log
			logUpdateTime = LoggerRouter.LogUpdateRate;
			cacheLogs = new List<string> ();

			var logName = $"Launcher_{LoggerRouter.GetLogNameByNow ()}";
			var dirPath = FileUtility.CurrentDriectory;

			//創建LauncherLogs資料夾
			var targetLauncherLogsPath = Path.Combine (LoggerRouter.LogFolderPath, "LauncherLogs");

			if (Directory.Exists (targetLauncherLogsPath) == false)
			{
				Directory.CreateDirectory (targetLauncherLogsPath);
			}

			//創建當天日期資料夾
			var Daytime = DateTime.Now.ToString (LoggerRouter.DayFormat);
			var targetDaytimePath = Path.Combine (targetLauncherLogsPath, Daytime);

			if (Directory.Exists (targetDaytimePath) == false)
			{
				Directory.CreateDirectory (targetDaytimePath);
			}

			//logPath = Path.Combine (dirPath, logName);
			logPath = Path.Combine (targetDaytimePath, logName);

			LoggerRouter.BindLogEvent ((line) =>
			{
				ReceiveLog (line);
			});
			DataContext = this;
		}

		void ReceiveLog (string line)
		{
			var time = DateTime.Now.ToString (LoggerRouter.TimeFormatSecond);

			var processLine = $"{time} -> {line}";

			MainWindow.Instance.Dispatcher.Invoke (() =>
			{
				cacheLogs.Add (processLine);
			});

			using (StreamWriter writer = new StreamWriter (logPath, true, Encoding.UTF8))
			{
				writer.WriteLine (processLine);
			}
		}

		string logPath;

		List<string> cacheLogs;

		StringBuilder logBuilder;

		public void Update (float deltaTime)
		{
			logUpdateTime += deltaTime;

			if (logUpdateTime > LoggerRouter.LogUpdateRate)
			{
				logUpdateTime = 0f;

				if (cacheLogs.Count > 0)
				{
					cacheLogs.ForEach (cacheLog =>
					{
						logBuilder.AppendLine (cacheLog);
					});

					cacheLogs.Clear ();

					if (PropertyChanged != null)
					{
						PropertyChanged.Invoke (this, new PropertyChangedEventArgs (nameof (FullLog)));
					}
				}
			}
		}

		float logUpdateTime;

		public string FullLog => logBuilder.ToString ();

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
