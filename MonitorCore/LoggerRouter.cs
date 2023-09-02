using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Threading;

namespace MonitorCore
{
	public static class LoggerRouter
	{
		static LoggerRouter()
		{
			LogFolderPath = Path.Combine (FileUtility.CurrentDriectory, LogFolderName);

			if (Directory.Exists (LogFolderPath) == false) 
			{
				Directory.CreateDirectory (LogFolderPath);
			}

			logLocker = new ReaderWriterLockSlim();
			logBuilder = new StringBuilder();

			var time = DateTime.Now.ToString (TimeFormatSecond);
			var logPath = Path.Combine (LogFolderPath, $"{time}.log");
			OnWriteLine = (msg) =>
			{
				logBuilder.AppendLine(msg);
				Console.WriteLine(msg);

				using (StreamWriter writer = new StreamWriter (logPath, true))
				{
					writer.WriteLine (msg);
				}
			};
		}

		const string LogFolderName = "Logs";

		/// <summary>
		/// 所有專案統一存放log的資料夾
		/// </summary>
		public static string LogFolderPath { get; private set; }

		/// <summary>
		/// 截到分就好, 自己人看的顯示當地時間就好
		/// </summary>
		public const string TimeFormatMinute = "yyyy-MM-dd-HH-mm";

		/// <summary>
		/// 截到秒就好
		/// </summary>
		public const string TimeFormatSecond = "yyyy-MM-dd-HH-mm-ss";

		public const string DayFormat = "yyyy-MM-dd";

		public static string GetLogFolderNameByNow () 
		{
			var logName = DateTime.Now.ToString (DayFormat);

			return logName;
		}

		public static string GetLogNameByNow()
		{
			var logName = DateTime.Now.ToString(TimeFormatMinute);
			logName += ".log";

			return logName;
		}

		static ReaderWriterLockSlim logLocker;

		public const float LogUpdateRate = 0.3f;

		public static void BindLogEvent(Action<string> callback)
		{
			logLocker.EnterWriteLock();

			var history = logBuilder.ToString();
			if (string.IsNullOrEmpty(history) == false)
			{
				//每次AppednLine都會移到前面, 所以要把最後的\r\n消掉
				var processHistory = new char[history.Length - 2];
				history.CopyTo(0, processHistory, 0, processHistory.Length);

				callback.Invoke(history);
			}

			OnWriteLine += callback;
			logLocker.ExitWriteLock();
		}

		static StringBuilder logBuilder;

		static event Action<string> OnWriteLine;

		public static void WriteLine(object lineObj)
		{
			string line = lineObj.ToString();

			logLocker.EnterWriteLock();
			logBuilder.AppendLine(line);
			OnWriteLine.Invoke(line);
			logLocker.ExitWriteLock();
		}
	}
}
