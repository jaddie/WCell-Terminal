using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WCell.Terminal
{
	class TerminalMain : ApplicationContext
	{
		delegate bool ConsoleEventHandlerDelegate(ConsoleHandlerEventCode eventCode);

		[DllImport("kernel32.dll")]
		static extern bool SetConsoleCtrlHandler(ConsoleEventHandlerDelegate handlerProc, bool add);

		enum ConsoleHandlerEventCode : uint
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}
		static ConsoleEventHandlerDelegate consoleHandler;
		public static SysTrayNotifyIcon notification = new SysTrayNotifyIcon();
		String dateTime = DateTime.Now.ToString("hh:mm");

		#region Config
		private static TerminalConfiguration m_configuration;
		public TerminalConfiguration Configuration
		{
			get { return m_configuration; }
		}

		private static string s_entryLocation;

		private static string EntryLocation
		{
			get
			{
				if (s_entryLocation == null)
				{
					var asm = Assembly.GetEntryAssembly();
					if (asm != null)
					{
						s_entryLocation = asm.Location;
					}
				}
				return s_entryLocation;
			}
			set { s_entryLocation = value; }
		}
		#endregion

		public TerminalMain()
		{
			consoleHandler = new ConsoleEventHandlerDelegate(ConsoleEventHandler);
			SetConsoleCtrlHandler(consoleHandler, true);

			Version vrs = new Version(Application.ProductVersion);
			Console.Title = String.Format("WCell.Terminal v{0}.{1}", vrs.Major, vrs.Minor);
			Console.ForegroundColor = ConsoleColor.White;
			notification.Visible = true;

			m_configuration = new TerminalConfiguration(EntryLocation);

			var connection = new TerminalIrcClient
			{
				Nicks = new[] { TerminalConfiguration.DefaultNick, TerminalConfiguration.AlternateNick1, TerminalConfiguration.AlternateNick2 },
				UserName = TerminalConfiguration.DefaultUserName,
				Info = TerminalConfiguration.DefaultInfo
			};
			if (TerminalIrcClient.IRCEnabled)
			{
				connection.BeginConnect(TerminalConfiguration.DefaultServer, TerminalConfiguration.DefaultPort);
			}
			else
			{
				Console.WriteLine("({0}) <IRC Interface> Disabled", dateTime);
			}

			var webinterface = new WebInterface();

			if (TerminalConfiguration.AutoStartAuthServer)
			{
				ProcessRunner AuthServer = new ProcessRunner(TerminalConfiguration.AuthServerPath);
				ProcessOutputEventHandler AuthServerOutputHandler = delegate(object o, ProcessOutputEventArgs ex)
				{
					Console.WriteLine("({0}) <AuthServer> {1}", dateTime, ex.Data);
				};
				AuthServer.OutputReceived += AuthServerOutputHandler;
				AuthServer.Start();
			}

			if (TerminalConfiguration.AutoStartRealmServer)
			{
				ProcessRunner RealmServer = new ProcessRunner(TerminalConfiguration.RealmServerPath);
				ProcessOutputEventHandler RealmServerOutputHandler = delegate(object o, ProcessOutputEventArgs ex)
				{
					Console.WriteLine("({0}) <RealmServer> {1}", dateTime, ex.Data);
				};
				RealmServer.OutputReceived += RealmServerOutputHandler;
				RealmServer.Start();
			}
		}

		static bool ConsoleEventHandler(ConsoleHandlerEventCode eventCode)
		{
			switch (eventCode)
			{
				case ConsoleHandlerEventCode.CTRL_C_EVENT:
				case ConsoleHandlerEventCode.CTRL_BREAK_EVENT:
				case ConsoleHandlerEventCode.CTRL_CLOSE_EVENT:
				case ConsoleHandlerEventCode.CTRL_LOGOFF_EVENT:
				case ConsoleHandlerEventCode.CTRL_SHUTDOWN_EVENT:
				default:
					notification.Dispose();
					break;
			}
			return (false);
		}

		static void Main(string[] args)
		{
			Application.Run(new TerminalMain());
		}
	}
}