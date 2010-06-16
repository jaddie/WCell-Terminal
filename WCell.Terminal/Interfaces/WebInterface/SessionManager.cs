using System;
using HttpServer;
using HttpServer.Tools;

namespace WCell.Terminal
{
	[Serializable]
	class SessionManager : Session
	{
		private static readonly SessionProvider<SessionManager> SessionProvider = new SessionProvider<SessionManager>();

		static SessionManager()
		{
			SessionProvider.Cache = true;
		}

		public static SessionManager Current
		{
			get { return SessionProvider.Current ?? new SessionManager(); }
		}

		public static SessionManager Create()
		{
			return SessionProvider.Create();
		}

		internal static void Start(Server webServer)
		{
			SessionProvider.Start(webServer);
		}
	}
}