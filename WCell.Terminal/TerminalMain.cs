using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Squishy.Irc;
using Squishy.Irc.Auth;
using Squishy.Irc.Commands;
using Squishy.Irc.Protocol;
using Squishy.Network;
using WCell.Util.Variables;

namespace WCell.Terminal
{
	class TerminalMain
	{
		public static StreamWriter terminaloutput = null;

		private static TerminalConfiguration m_configuration;
		public TerminalConfiguration Configuration
		{
			get { return m_configuration; }
		}

		protected static string s_entryLocation;

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

		static void Main(string[] args)
		{
			String command;
			String dateTime = DateTime.Now.ToString("hh:mm");
			Console.Title = "WCell Terminal v0.1";
			Console.ForegroundColor = ConsoleColor.White;

			m_configuration = new TerminalConfiguration(EntryLocation);

			var connection = new TerminalIrcClient{
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
				ProcessOutputEventHandler AuthServerOutputHandler = delegate(object o, ProcessOutputEventArgs e)
				{
					Console.WriteLine("({0}) <AuthServer> {1}", dateTime, e.Data);
				};
				AuthServer.OutputReceived += AuthServerOutputHandler;
				AuthServer.Start();
			}

			if (TerminalConfiguration.AutoStartRealmServer)
			{
				ProcessRunner RealmServer = new ProcessRunner(TerminalConfiguration.RealmServerPath);
				ProcessOutputEventHandler RealmServerOutputHandler = delegate(object o, ProcessOutputEventArgs e)
				{
					Console.WriteLine("({0}) <RealmServer> {1}", dateTime, e.Data);
				};
				RealmServer.OutputReceived += RealmServerOutputHandler;
				RealmServer.Start();
			}

			do
			{
				command = Console.ReadLine();
				Console.WriteLine("> {0}", command);
			} while (command != "quit");
		}
	}
}