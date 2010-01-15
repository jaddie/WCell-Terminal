using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Win32.Targets;
using System.Threading;
using System.Diagnostics;

namespace WCell.Util.NLog
{

	public static class LogUtil
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		private static int _streamNum;

		public static Action<Action<string>> SystemInfoLogger;

		public static event Action<string, Exception> ExceptionRaised;

		/// <summary>
		/// Will enable logging to the console
		/// </summary>
		public static void SetupConsoleLogging()
		{
			var config = LogManager.Configuration ?? new LoggingConfiguration();

			var consoleTarget = new ColoredConsoleTarget
			{
				Layout = "${processtime} [${level}] ${message} ${exception:format=tostring}"
			};
			config.AddTarget("console", consoleTarget);

			config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));

			LogManager.Configuration = config;
			LogManager.EnableLogging();
		}

		/// <summary>
		/// Will enable logging to the console and (if not null) the specified file
		/// </summary>
		public static void SetupStreamLogging(TextWriter stream)
		{
			var config = LogManager.Configuration ?? new LoggingConfiguration();

			var streamTarget = new StreamTarget
			{
				StreamName = "Stream" + ++_streamNum,
				Stream = stream,
				Layout = @"${processtime} [${level}] ${message} ${exception:format=tostring}"
			};
			config.AddTarget(streamTarget.Name, streamTarget);

			config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, streamTarget));

			LogManager.Configuration = config;
			LogManager.EnableLogging();
		}

		public static void ErrorException(Exception e)
		{
			ErrorException(e, false);
		}

		public static void ErrorException(Exception e, bool addSystemInfo)
		{
			ErrorException(e, addSystemInfo, "");
		}

		public static void ErrorException(string msg, params object[] format)
		{
			ErrorException(false, msg, format);
		}

		public static void ErrorException(bool addSystemInfo, string msg, params object[] format)
		{
			LogException(log.Error, null, addSystemInfo, msg, format);
		}

		public static void ErrorException(Exception e, string msg, params object[] format)
		{
			ErrorException(e, true, msg, format);
		}

		public static void ErrorException(Exception e, bool addSystemInfo, string msg, params object[] format)
		{
			LogException(log.Error, e, addSystemInfo, msg, format);
		}

		public static void FatalException(Exception e, string msg, params object[] format)
		{
			FatalException(e, true, msg, format);
		}

		public static void FatalException(Exception e, bool addSystemInfo)
		{
			FatalException(e, addSystemInfo, "");
		}

		public static void FatalException(Exception e, bool addSystemInfo, string msg, params object[] format)
		{
			LogException(log.Fatal, e, addSystemInfo, msg, format);
		}

		public static void LogException(Action<string> logger, Exception e, bool addSystemInfo, string msg, params object[] format)
		{
			if (!string.IsNullOrEmpty(msg))
			{
				msg = string.Format(msg, format);
				logger(msg);
			}

			if (e != null)
			{
				LogStacktrace(logger);
				logger("");
				logger(e.ToString());
				//logger(new StackTrace(e, true));
			}

			if (addSystemInfo)
			{
				logger("");
				SystemInfoLogger(logger);
			}

			if (e != null)
			{
				logger("");
				logger(e.GetAllMessages().ToString("\n\t"));
			}

			var evt = ExceptionRaised;
			if (evt != null)
			{
				evt(msg, e);
			}
		}

		public static void LogStacktrace(Action<string> logger)
		{
			logger(new StackTrace(Thread.CurrentThread, true).GetFrames().ToString("\n\t", frame => frame.ToString().Trim()));
		}
	}
}
