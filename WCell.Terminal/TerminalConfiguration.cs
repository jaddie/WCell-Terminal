using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
using System.IO;
using WCell.Util.Variables;

namespace WCell.Terminal
{
	/// <summary>
	/// Configuration for the terminal server
	/// </summary>
	[XmlRoot("WCellConfig")]
	public class TerminalConfiguration : WCellConfig<TerminalConfiguration>
	{
		#region Terminal Config
		public static Boolean AutoStartAuthServer = true;
		public static Boolean AutoStartRealmServer = true;
		public static String AuthServerPath = Path.Combine(Directory.GetCurrentDirectory(), "WCell.AuthServerConsole.exe");
		public static String RealmServerPath = Path.Combine(Directory.GetCurrentDirectory(), "WCell.RealmServerConsole.exe");
		public static String DefaultServer = "euroserv.fr.quakenet.org";
		public static int DefaultPort = 6667;
		public static String DefaultNick = "Sylvanas";
		public static String AlternateNick1 = "Sylvanas2";
		public static String AlternateNick2 = "Sylvanas3";
		public static String DefaultUserName = "Sylvanas@dekadence.ro";
		public static String DefaultInfo = "Sylvanas Windrunner";
		#endregion

		private static bool init;
		private const string ConfigFilename = "Config.xml";
		private static string dateTime;

		private static TerminalConfiguration s_instance;
		public static TerminalConfiguration Instance
		{
			get { return s_instance; }
		}

		protected TerminalConfiguration()
			: base(OnError)
		{
			RootNodeName = "WCellConfig";
			s_instance = this;
		}

		public TerminalConfiguration(string executablePath)
			: this()
		{
			RootNodeName = "WCellConfig";
			s_instance = this;
			dateTime = DateTime.Now.ToString("hh:mm");
			Console.WriteLine("({0}) <Config> Initializing...", dateTime);
			Initialize();
		}

		public override string Filename
		{
			get
			{
				return Path.Combine(Directory.GetCurrentDirectory(), ConfigFilename);
			}
			set
			{
				throw new InvalidOperationException("Cannot modify Filename");
			}
		}

		public static bool Initialize()
		{
			Console.WriteLine("({0}) <Config> Initialized...", dateTime);
			if (!init)
			{
				init = true;

				s_instance.AddVariablesOfAsm<VariableAttribute>(typeof(TerminalConfiguration).Assembly);
				try
				{
					if (!s_instance.Load())
					{
						s_instance.Save(true, false);
						Console.WriteLine("({0}) <Config> Config-file \"{1}\" not found - Created new file.", dateTime, Instance.Filename);
						Console.WriteLine("({0}) <Config> Please take a little time to configure your server and then restart the Application.", dateTime);
						Console.WriteLine("({0}) <Config> See http://wiki.wcell.org/index.php/Configuration for more information.", dateTime);
						return false;
					}
					else
					{
						if (s_instance.AutoSave)
						{
							s_instance.Save(true, true);
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("({0}) <Config> {1}", dateTime, e.ToString());
					return false;
				}
			}
			return true;
		}

		internal static void OnError(string msg)
		{
			Console.WriteLine("({0}) <Config> {1}", dateTime, msg);
		}

		internal static void OnError(string msg, params object[] args)
		{
			Console.WriteLine("({0}) <Config> {1}", dateTime, String.Format(msg, args));
		}
	}
}
