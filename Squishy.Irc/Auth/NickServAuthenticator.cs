using Squishy.Irc.Protocol;

namespace Squishy.Irc.Auth
{
    class NickServAuthenticator : AsyncIrcAuthenticator
    {
        public override string ServiceName
        {
            get { return "NickServ"; }
        }

        public override string AuthOpcode
        {
            get { return "307"; }
        }

        public override void ResolveAuth(IrcUser user, IrcUserAuthResolvedHandler authResolvedHandler)
        {
            user.IrcClient.CommandHandler.Whois(user.Nick);
            base.ResolveAuth(user, authResolvedHandler);
        }

        protected override string ResolveAuth(IrcUser user, IrcPacket packet)
        {
            packet.Content.SkipWord();

            var nick = packet.Content.NextWord();
            if (nick == user.Nick)
            {
                user.AuthName = nick;
                return nick;
            }
            return null;
        }
    }
}
