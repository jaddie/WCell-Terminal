using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squishy.Irc;
using Squishy.Irc.Auth;
using Squishy.Irc.Commands;
using Squishy.Irc.Protocol;
using Squishy.Network;
using StringStream = Squishy.Network.StringStream;

namespace WCell.Terminal
{
	public class IRCInterface : IrcClient
	{
		#region IRC Interface Default Config
		public static bool IRCInterfaceEnabled = true;
		public static bool HideChatting = false;
		public static bool HideIncomingIrcPackets = false;
		public static bool HideOutgoingIrcPackets = false;
		public static string DefaultServer = "euroserv.fr.quakenet.org";
		public static int DefaultPort = 6667;
		public static string DefaultNick = "Sylvanas";
		public static string AlternateNick1 = "Sylvanas2";
		public static string AlternateNick2 = "Sylvanas3";
		public static string DefaultUserName = "Sylvanas@dekadence.ro";
		public static string DefaultInfo = "Sylvanas Windrunner";
		public static string[] ChannelList = { "#ChannelName,ChannelKey,Disabled" };
		public static string[] PerformList = { "Action,Target,Message" };
		#endregion

		protected override void Perform()
		{
			foreach (var action in IRCInterface.PerformList)
			{
				var sbstr = action.Split(',');
				if (sbstr[0].Length != 0 && sbstr[0] != "Action" && sbstr[1].Length != 0 && sbstr[1] != "Target" && sbstr[2].Length != 0 && sbstr[2] != "Message")
				{
					switch (sbstr[0])
					{
						case "MSG":
							CommandHandler.Msg(sbstr[1], sbstr[2]);
							break;
						case "MODE":
							if (sbstr[1].ToLower() == "self")
							{
								CommandHandler.Mode(Me.Nick + " " + sbstr[2]);
							}
							else
							{
								CommandHandler.Mode(sbstr[2], sbstr[1]);
							}
							break;
					}
				}
			}

			foreach (var channel in IRCInterface.ChannelList)
			{
				var sbstr = channel.Split(',');
				switch (sbstr.Length)
				{
					case 1:
						if (sbstr[0].Length != 0 && sbstr[0] != "#ChannelName")
						{
							CommandHandler.Join(sbstr[0]);
						}
						break;
					case 2:
						if (sbstr[0].Length != 0 && sbstr[0] != "#ChannelName" && sbstr[1].Length != 0 && sbstr[1] != "ChannelKey")
						{
							CommandHandler.Join(sbstr[0], sbstr[1]);
						}
						break;
					case 3:
						if (sbstr[0].Length != 0 && sbstr[0] != "#ChannelName" && sbstr[1].Length != 0 && sbstr[1] != "ChannelKey" && sbstr[2].Length != 0 && sbstr[2] != "Disabled")
						{
							CommandHandler.Join(sbstr[0], sbstr[1]);
						}
						break;
				}
			}
		}

        protected override void OnConnecting()
        {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("({0}) <IRC Interface> * Connecting to {1} ({2})", DateTime.Now.ToString("hh:mm"), Client.RemoteAddress, Client.RemotePort);
        }

		protected override void OnConnected()
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("({0}) <IRC Interface> * Connected to {1} ({2})", DateTime.Now.ToString("hh:mm"), Client.RemoteAddress, Client.RemotePort);
		}

		protected override void OnConnectFail(Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("({0}) <IRC Interface> * Connection failed: {1}", DateTime.Now.ToString("hh:mm"), ex);
		}

		protected void OnReceive(IrcPacket packet)
		{
			if (HideIncomingIrcPackets != true)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("({0}) <IRC Interface> <-- {1}", DateTime.Now.ToString("hh:mm"), packet);
			}
		}

		protected override void OnBeforeSend(string text)
		{
			if (HideOutgoingIrcPackets != true)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("({0}) <IRC Interface> --> {1}", DateTime.Now.ToString("hh:mm"), text);
			}
		}

		protected override void OnError(IrcPacket packet)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("({0}) <IRC Interface> ERROR: {1}", DateTime.Now.ToString("hh:mm"), packet.ToString());
		}

		protected override void OnDisconnected(bool conLost)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("({0}) <IRC Interface> * Disconnected" + (conLost ? " (Connection lost)" : ""), DateTime.Now.ToString("hh:mm"));
		}
		
		protected override void OnNick(IrcUser user, string oldNick, string newNick)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("({0}) <IRC Interface> * {1} is now known as {2}", DateTime.Now.ToString("hh:mm"), oldNick, newNick);
		}

		protected override void OnTopic(IrcUser user, IrcChannel chan, string text, bool initial)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			if (initial)
			{
				Console.WriteLine("({0}) <IRC Interface> * The topic for channel {1} is {2}", DateTime.Now.ToString("hh:mm"), chan.Name, chan.Topic);
			}
			else
			{
				Console.WriteLine("({0}) <IRC Interface> * {1} changed topic in channel {2} to: {3}", DateTime.Now.ToString("hh:mm"), user.Nick, chan.Name, text);
			}
		}

		protected override void OnText(IrcUser user, IrcChannel chan, StringStream text)
		{
			if (HideChatting != true)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("({0}) <IRC Interface> {1} <{2}> {3}", DateTime.Now.ToString("hh:mm"), chan, user, text);
			}
		}

		protected override void OnJoin(IrcUser user, IrcChannel chan)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			if (user == Me)
			{
				Console.WriteLine("({0}) <IRC Interface> * Bot joined channel {1}", DateTime.Now.ToString("hh:mm"), chan.Name);
			}
			else
			{
				Console.WriteLine("({0}) <IRC Interface> * {1} joined channel {2}", DateTime.Now.ToString("hh:mm"), user.Nick, chan.Name);
			}
		}

		protected override void OnPart(IrcUser user, IrcChannel chan, string reason)
		{
			if (user == Me)
			{
				Console.WriteLine("({0}) <IRC Interface> * Bot parts channel {1}", DateTime.Now.ToString("hh:mm"), chan.Name);
			}
			else
			{
				Console.WriteLine("({0}) <IRC Interface> * {1} parts channel {2}", DateTime.Now.ToString("hh:mm"), user.Nick, chan.Name);
			}
		}

		protected override void OnQuit(IrcUser user, string reason)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			if (user == Me)
			{
				Console.WriteLine("({0}) <IRC Interface> * Bot quits ({1})", DateTime.Now.ToString("hh:mm"), reason);
			}
			else
			{
				Console.WriteLine("({0}) <IRC Interface> * {1} quits ({2})", DateTime.Now.ToString("hh:mm"), user.Nick, reason);
			}
		}

		protected override void OnCannotJoin(IrcChannel chan, string reason)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("({0}) <IRC Interface> * Cannot join channel {1} ({2})", DateTime.Now.ToString("hh:mm"), chan.Name, reason);
		}

		protected override void OnModeAdded(IrcUser user, IrcChannel chan, string mode, string param)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("({0}) <IRC Interface> * {1} sets mode +{2} {3} on channel {4}", DateTime.Now.ToString("hh:mm"), user.Nick, mode, param, chan.Name);
		}

		protected override void OnModeDeleted(IrcUser user, IrcChannel chan, string mode, string param)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("({0}) <IRC Interface> * {1} sets mode -{2} {3} on channel {4}", DateTime.Now.ToString("hh:mm"), user.Nick, mode, param, chan.Name);
		}

		protected override void OnFlagAdded(IrcUser user, IrcChannel chan, Privilege priv, IrcUser target)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("({0}) <IRC Interface> * {1} sets mode +{2} for {3} on channel {4}", DateTime.Now.ToString("hh:mm"), user.Nick, priv, target.Nick, chan.Name);
		}

		protected override void OnFlagDeleted(IrcUser user, IrcChannel chan, Privilege priv, IrcUser target)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("({0}) <IRC Interface> * {1} sets mode -{2} for {3} on channel {4}", DateTime.Now.ToString("hh:mm"), user.Nick, priv, target.Nick, chan.Name);
		}

		protected override void OnExceptionRaised(Exception e)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("({0}) <IRC Interface> {1}", DateTime.Now.ToString("hh:mm"), e.ToString());
		}

		protected override void OnCommandFail(CmdTrigger trigger, Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("({0}) <IRC Interface> * Unknown command {1} ({2})", DateTime.Now.ToString("hh:mm"), trigger, ex);
		}
	}
}