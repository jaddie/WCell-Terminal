using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using WCell.Util;

namespace WCell.Terminal
{
	class TerminalMain : ApplicationContext
	{
		public static SysTrayNotifyIcon notification;

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

		static void ConsoleEventHandler()
		{
			notification.Dispose();
		}

		public TerminalMain()
		{
			WCell.Util.AppUtil.AddApplicationExitHandler(ConsoleEventHandler);
			Version vrs = new Version(Application.ProductVersion);
			try
			{
				String ExePath = Directory.GetCurrentDirectory();
				String SourcePath = Directory.GetParent(Directory.GetParent(ExePath).FullName).FullName;
				int Revision = WCell.Util.SvnUtil.GetVersionNumber(SourcePath);
				Console.Title = String.Format("WCell.Terminal v{0}.{1} rev {2}", vrs.Major, vrs.Minor, Revision);
			}
			catch (DirectoryNotFoundException)
			{
				Console.Title = String.Format("WCell.Terminal v{0}.{1} rev unknown", vrs.Major, vrs.Minor);
			}
			Console.ForegroundColor = ConsoleColor.White;
			notification = new SysTrayNotifyIcon();
			notification.Visible = true;

			m_configuration = new TerminalConfiguration(EntryLocation);

			if (TerminalConfiguration.ConsoleCenterOnScreen)
			{
				ConsoleUtil.CenterConsoleWindow(TerminalConfiguration.ConsoleWidth, TerminalConfiguration.ConsoleHeight);
			}
			
			if (TerminalConfiguration.ConsoleWidth != 80)
			{
				Console.BufferWidth = TerminalConfiguration.ConsoleWidth;
				Console.WindowWidth = TerminalConfiguration.ConsoleWidth;
			}

			if (TerminalConfiguration.ConsoleHeight != 25)
			{
				Console.WindowHeight = TerminalConfiguration.ConsoleHeight;
			}			
			Console.BufferHeight = Int16.MaxValue - 1;

			var connection = new IRCInterface
			{
				Nicks = new[] { IRCInterface.DefaultNick, IRCInterface.AlternateNick1, IRCInterface.AlternateNick2 },
				UserName = IRCInterface.DefaultUserName,
				Info = IRCInterface.DefaultInfo
			};
			if (IRCInterface.IRCEnabled)
			{
				connection.BeginConnect(IRCInterface.DefaultServer, IRCInterface.DefaultPort);
			}
			else
			{
				Console.WriteLine("({0}) <IRC Interface> Disabled", DateTime.Now.ToString("hh:mm"));
			}

			var webinterface = new WebInterface();

			if (TerminalConfiguration.AutoStartAuthServer)
			{
				ProcessRunner AuthServer = new ProcessRunner(TerminalConfiguration.AuthServerPath);
				ProcessOutputEventHandler AuthServerOutputHandler = delegate(object o, ProcessOutputEventArgs ex)
				{
					if (ex.Data.Contains("[Error]"))
					{
						Console.ForegroundColor = ConsoleColor.Red;
					}
					else if (ex.Data.Contains("[Warn]"))
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
					}
					else if (ex.Data.Contains("[Debug]"))
					{
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.White;
					}
					Console.WriteLine("({0}) <AuthServer> {1}", DateTime.Now.ToString("hh:mm"), ex.Data);
				};
				AuthServer.OutputReceived += AuthServerOutputHandler;
				AuthServer.Start();
			}

			if (TerminalConfiguration.AutoStartRealmServer)
			{
				ProcessRunner RealmServer = new ProcessRunner(TerminalConfiguration.RealmServerPath);
				ProcessOutputEventHandler RealmServerOutputHandler = delegate(object o, ProcessOutputEventArgs ex)
				{
					if (ex.Data.Contains("[Error]"))
					{
						Console.ForegroundColor = ConsoleColor.Red;
					}
					else if (ex.Data.Contains("[Warn]"))
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
					}
					else if (ex.Data.Contains("[Debug]"))
					{
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.White;
					}
					Console.WriteLine("({0}) <RealmServer> {1}", DateTime.Now.ToString("hh:mm"), ex.Data);
				};
				RealmServer.OutputReceived += RealmServerOutputHandler;
				RealmServer.Start();
			}
		}

		static void Main(string[] args)
		{
			Application.Run(new TerminalMain());
		}
	}
}