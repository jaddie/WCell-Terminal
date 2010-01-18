using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using Microsoft.Win32;
using WCell.Util;

namespace WCell.Terminal
{
	class TerminalMain : ApplicationContext
	{
		public static SysTrayNotifyIcon notification;
		public static ProcessRunner AuthServer;
		public static ProcessRunner RealmServer;

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
			notification = new SysTrayNotifyIcon();
			notification.Visible = true;
			m_configuration = new TerminalConfiguration(EntryLocation);

			if (TerminalConfiguration.ConsoleCenterOnScreen)
			{
				ConsoleUtil.CenterConsoleWindow(TerminalConfiguration.ConsoleWidth, TerminalConfiguration.ConsoleHeight);
			}
			else
			{
				ConsoleUtil.PositionConsoleWindow(TerminalConfiguration.ConsoleLeft, TerminalConfiguration.ConsoleTop, TerminalConfiguration.ConsoleWidth, TerminalConfiguration.ConsoleHeight);
			}

			try
			{
				Console.WindowWidth = TerminalConfiguration.ConsoleWidth;
				Console.WindowHeight = TerminalConfiguration.ConsoleHeight;
				Console.BufferWidth = TerminalConfiguration.ConsoleWidth;
				Console.BufferHeight = Int16.MaxValue - 1;
			}
			catch (ArgumentOutOfRangeException)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("({0}) <Terminal> Failed to initialize properly. Check Config.xml settings.", DateTime.Now.ToString("hh:mm"));
				Console.ResetColor();
			}
			finally
			{
				Console.WriteLine("({0}) <Terminal> Initialized...", DateTime.Now.ToString("hh:mm"));
			}

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
				try
				{
					AuthServer = new ProcessRunner(TerminalConfiguration.AuthServerPath);
				}
				catch (FileNotFoundException)
				{
					Console.WriteLine("({0}) <Terminal> Failed to start AuthServer. Check Config.xml settings.", DateTime.Now.ToString("hh:mm"));
				}
				finally
				{
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
						else if (ex.Data.Contains("Unhandled Exception"))
						{
							Console.ForegroundColor = ConsoleColor.DarkRed;
						}
						else
						{
							Console.ForegroundColor = ConsoleColor.White;
						}
						Console.WriteLine("({0}) <AuthServer> {1}", DateTime.Now.ToString("hh:mm"), ex.Data);
					};
					try
					{
						AuthServer.OutputReceived += AuthServerOutputHandler;
						AuthServer.Start();
					}
					catch (NullReferenceException)
					{
						Console.WriteLine("({0}) <Terminal> Failed to start AuthServer. Check Config.xml settings.", DateTime.Now.ToString("hh:mm"));
					}
				}
			}

			if (TerminalConfiguration.AutoStartRealmServer)
			{
				try
				{
					RealmServer = new ProcessRunner(TerminalConfiguration.RealmServerPath);
				}
				catch (FileNotFoundException)
				{
					Console.WriteLine("({0}) <Terminal> Failed to start RealmServer. Check Config.xml settings.", DateTime.Now.ToString("hh:mm"));
				}
				finally
				{
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
						else if (ex.Data.Contains("Unhandled Exception"))
						{
							Console.ForegroundColor = ConsoleColor.DarkRed;
						}
						else
						{
							Console.ForegroundColor = ConsoleColor.White;
						}
						Console.WriteLine("({0}) <RealmServer> {1}", DateTime.Now.ToString("hh:mm"), ex.Data);
					};
					try
					{
						RealmServer.OutputReceived += RealmServerOutputHandler;
						RealmServer.Start();
					}
					catch (NullReferenceException)
					{
						Console.WriteLine("({0}) <Terminal> Failed to start RealmServer. Check Config.xml settings.", DateTime.Now.ToString("hh:mm"));
					}
				}
			}
		}

		static void Main(string[] args)
		{
			ConsoleUtil.ApplyCustomFont();
			ConsoleUtil.AllocConsole();
			ConsoleUtil.InstallCustomFontAndApply();
			Assembly a = Assembly.GetExecutingAssembly();
			Icon icon = new Icon(a.GetManifestResourceStream("WCell.Terminal.Resources.WCell.Terminal.ico"));
			ConsoleUtil.SetConsoleIcon(icon);
			AppUtil.AddApplicationExitHandler(ConsoleEventHandler);
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("({0}) <Terminal> Initializing...", DateTime.Now.ToString("hh:mm"));
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
			catch (NullReferenceException)
			{
				Console.Title = String.Format("WCell.Terminal v{0}.{1} rev unknown", vrs.Major, vrs.Minor);
			}			
			Application.Run(new TerminalMain());			
		}
	}
}