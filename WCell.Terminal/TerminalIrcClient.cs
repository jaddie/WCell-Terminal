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
	public class TerminalIrcClient : IrcClient
	{
		String dateTime;

		public static bool IRCEnabled = true;
		public static bool HideChatting = false;
		public static bool HideIncomingIrcPackets = false;
		public static bool HideOutgoingIrcPackets = false;

		public TerminalIrcClient()
		{
			dateTime = DateTime.Now.ToString("hh:mm");
		}

		protected override void OnNick(IrcUser user, string oldNick, string newNick)
		{
			Console.WriteLine("({0}) <IRC Interface> * Connecting to {1}:{2} ...", dateTime, Client.RemoteAddress, Client.RemotePort);
		}

		protected override void Perform()
		{
			CommandHandler.Msg("Q@CServe.QuakeNet.org", "AUTH" + " " + "WCellHeroesBot" + " " + "UzA2fevSLe");
			CommandHandler.Mode(Me.Nick + " +x");
			//CommandHandler.Join("#wcell.dev", "wcellrulz");
			CommandHandler.Join("#wcellheroes");
		}

        protected override void OnConnecting()
        {
			Console.WriteLine("({0}) <IRC Interface> * Connecting to {1} ({2})", dateTime, Client.RemoteAddress, Client.RemotePort);
        }

		protected override void OnConnected()
		{
			Console.WriteLine("({0}) <IRC Interface> * Connected to {1} ({2})", dateTime, Client.RemoteAddress, Client.RemotePort);
		}

		protected override void OnConnectFail(Exception ex)
		{
			Console.WriteLine("({0}) <IRC Interface> * Connection failed: {1}", dateTime, ex);
		}

		protected void OnReceive(IrcPacket packet)
		{
			if (HideIncomingIrcPackets != true)
			{
				Console.WriteLine("({0}) <IRC Interface> <-- {1}", dateTime, packet);
			}
		}

		protected override void OnBeforeSend(string text)
		{
			if (HideOutgoingIrcPackets != true)
			{
				Console.WriteLine("({0}) <IRC Interface> --> {1}", dateTime, text);
			}
		}

		protected override void OnError(IrcPacket packet)
		{
			Console.WriteLine("({0}) <IRC Interface> ERROR: {1}", dateTime, packet.ToString());
		}

		protected override void OnDisconnected(bool conLost)
		{
			Console.WriteLine("({0}) <IRC Interface> * Disconnected" + (conLost ? " (Connection lost)" : ""), dateTime);
		}

		protected override void OnTopic(IrcUser user, IrcChannel chan, string text, bool initial)
		{
			if (initial)
			{
				Console.WriteLine("({0}) <IRC Interface> * The topic for channel {1} is {2}", dateTime, chan.Name, chan.Topic);
			}
			else
			{
				Console.WriteLine("({0}) <IRC Interface> * {1} changed topic in channel {2} to: {3}", dateTime, user.Nick, chan.Name, text);
			}
		}

		protected override void OnText(IrcUser user, IrcChannel chan, StringStream text)
		{
			if (HideChatting != true)
			{
				Console.WriteLine("({0}) <IRC Interface> {1} <{2}> {3}", dateTime, chan, user, text);
			}
		}

		protected override void OnJoin(IrcUser user, IrcChannel chan)
		{
			if (user == Me)
			{
				Console.WriteLine("({0}) <IRC Interface> * Bot joined channel {1}", dateTime, chan.Name);
			}
			else
			{
				Console.WriteLine("({0}) <IRC Interface> * {1} joined channel {2}", dateTime, user.Nick, chan.Name);
			}
		}

		protected override void OnPart(IrcUser user, IrcChannel chan, string reason)
		{
			if (user == Me)
			{
				Console.WriteLine("({0}) <IRC Interface> * Bot parts channel {1}", dateTime, chan.Name);
			}
			else
			{
				Console.WriteLine("({0}) <IRC Interface> * {1} parts channel {2}", dateTime, user.Nick, chan.Name);
			}
		}

		protected override void OnQuit(IrcUser user, string reason)
		{
			if (user == Me)
			{
				Console.WriteLine("({0}) <IRC Interface> * Bot quits ({1})", dateTime, reason);
			}
			else
			{
				Console.WriteLine("({0}) <IRC Interface> * {1} quits ({2})", dateTime, user.Nick, reason);
			}
		}

		protected override void OnCannotJoin(IrcChannel chan, string reason)
		{
			Console.WriteLine("({0}) <IRC Interface> * Cannot join channel {1} ({2})", dateTime, chan.Name, reason);
		}

		/*protected override void OnModeAdded(IrcUser user, IrcChannel chan, string mode, string param)
		{
			Console.WriteLine("({0}) <IRC Interface> * {1} sets mode +{2} {3} on channel {4}", dateTime, user.Nick, mode, param, chan.Name);
		}

		protected override void OnModeDeleted(IrcUser user, IrcChannel chan, string mode, string param)
		{
			Console.WriteLine("({0}) <IRC Interface> * {1} sets mode -{2} {3} on channel {4}", dateTime, user.Nick, mode, param, chan.Name);
		}*/

		protected override void OnFlagAdded(IrcUser user, IrcChannel chan, Privilege priv, IrcUser target)
		{
			Console.WriteLine("({0}) <IRC Interface> * {1} sets mode +{2} for {3} on channel {4}", dateTime, user.Nick, priv, target.Nick, chan.Name);
		}

		protected override void OnFlagDeleted(IrcUser user, IrcChannel chan, Privilege priv, IrcUser target)
		{
			Console.WriteLine("({0}) <IRC Interface> * {1} sets mode -{2} for {3} on channel {4}", dateTime, user.Nick, priv, target.Nick, chan.Name);
		}

		protected override void OnExceptionRaised(Exception e)
		{
			Console.WriteLine("({0}) <IRC Interface> {1}", dateTime, e.ToString());
		}

		protected override void OnCommandFail(CmdTrigger trigger, Exception ex)
		{
			Console.WriteLine("({0}) <IRC Interface> * Unknown command {1} ({2})", dateTime, trigger, ex);
		}
	}
}