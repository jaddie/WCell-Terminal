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
		#region Terminal Default Config
		public static int ConsoleLeft = 0;
		public static int ConsoleTop = 0;
		public static int ConsoleWidth = 80;
		public static int ConsoleHeight = 25;
		public static bool ConsoleCenterOnScreen = true;
		public static bool TransparentConsole = false;
		public static bool AutoStartAuthServer = true;
		public static bool AutoStartRealmServer = true;
		public static string AuthServerPath = Path.Combine(Directory.GetCurrentDirectory(), "WCell.AuthServerConsole.exe");
		public static string RealmServerPath = Path.Combine(Directory.GetCurrentDirectory(), "WCell.RealmServerConsole.exe");
		#endregion

		private static bool init;
		private const string ConfigFilename = "Config.xml";

		private static TerminalConfiguration s_instance;
		public static TerminalConfiguration Instance
		{
			get { return s_instance; }
		}

		protected TerminalConfiguration() : base(OnError)
		{
			RootNodeName = "WCellConfig";
			s_instance = this;
		}

		public TerminalConfiguration(string executablePath)	: this()
		{
			RootNodeName = "WCellConfig";
			s_instance = this;
			Console.WriteLine("({0}) <Config> Initializing...", DateTime.Now.ToString("hh:mm"));
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
			Console.WriteLine("({0}) <Config> Initialized...", DateTime.Now.ToString("hh:mm"));
			if (!init)
			{
				init = true;

				s_instance.AddVariablesOfAsm<VariableAttribute>(typeof(TerminalConfiguration).Assembly);
				try
				{
					if (!s_instance.Load())
					{
						s_instance.Save(true, false);
						Console.WriteLine("({0}) <Config> Config-file \"{1}\" not found - Created new file.", DateTime.Now.ToString("hh:mm"), Instance.Filename);
						Console.WriteLine("({0}) <Config> Please take a little time to configure your server and then restart the Application.", DateTime.Now.ToString("hh:mm"));
						Console.WriteLine("({0}) <Config> See http://wiki.wcell.org/index.php/Configuration for more information.", DateTime.Now.ToString("hh:mm"));
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
					Console.WriteLine("({0}) <Config> {1}", DateTime.Now.ToString("hh:mm"), e.ToString());
					return false;
				}
			}
			return true;
		}

		internal static void OnError(string msg)
		{
			Console.WriteLine("({0}) <Config> {1}", DateTime.Now.ToString("hh:mm"), msg);
		}

		internal static void OnError(string msg, params object[] args)
		{
			Console.WriteLine("({0}) <Config> {1}", DateTime.Now.ToString("hh:mm"), String.Format(msg, args));
		}
	}
}
