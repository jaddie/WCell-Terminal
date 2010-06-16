using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using HttpServer;
using HttpServer.Routing;
using HttpServer.Resources;
using HttpServer.Authentication;
using HttpServer.Modules;
using HttpServer.Headers;
using HttpServer.Messages;
using HttpServer.Logging;
using HttpServer.Tools;
using HttpListener = HttpServer.HttpListener;

namespace WCell.Terminal
{
	class WebInterface
	{
		private X509Certificate2 certificate;
		private Server server;
		private FileResources resource;
		private FileModule reader;
		private HttpListener listener;
		private SecureHttpListener securelistener;
		private ManagedFileModule interpreter;

		public static bool WebInterfaceEnabled = true;
		public static bool WebInterfaceDebug = false;
		public static string ListenAddress = IPAddress.Any.ToString();
		public static int Port = 8080;
		public static bool UseSSL = false;
		public static bool UseDigestAuth = false;
		public static string CertificatePath = Path.Combine(Directory.GetCurrentDirectory(), "certificate.cer");
		public static string Username = "Admin";
		public static string Password = "passw0rd";
		public static string[] AllowedMimeTypes = {
			"txt,text/plain",
			"htm,text/html",
			"html,text/html",
			"css,text/css",
			"js,application/x-javascript",
			"ico,image/x-icon",
			"png,image/png",
			"jpg,image/jpeg",
			"gif,image/gif",
			"wav,audio/x-wav",
			"jar,application/java-archive",
			"swf,application/x-shockwave-flash"
		};

		public WebInterface()
		{
			if (WebInterfaceEnabled)
			{
				this.server = new Server();
				this.interpreter = new ManagedFileModule();
				this.server.Add(this.interpreter);
				this.reader = new FileModule();
				this.resource = new FileResources("/", Path.Combine(Directory.GetCurrentDirectory(), "WebInterface"));
				this.reader.Resources.Add(resource);
				this.server.Add(this.reader);
				this.server.Add(new SimpleRouter("/", "/index.html"));

				if (WebInterfaceDebug)
				{
					if (UseSSL)
					{
						//
					}
					else
					{
						//
					}
				}

				if (UseSSL)
				{
					try
					{
						this.certificate = new X509Certificate2(CertificatePath);
					}
					catch (DirectoryNotFoundException)
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine("({0}) <Web Interface> Error: The directory specified could not be found.", DateTime.Now.ToString("hh:mm"));
					}
					catch (IOException)
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine("({0}) <Web Interface> Error: A file in the directory could not be accessed.", DateTime.Now.ToString("hh:mm"));
					}
					catch (NullReferenceException)
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine("({0}) <Web Interface> File must be a .cer file. Program does not have access to that type of file.", DateTime.Now.ToString("hh:mm"));
					}
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("({0}) <Web Interface> Loaded Certificate: {1} Valid Date: {2} Expiry Date: {3}", DateTime.Now.ToString("hh:mm"), Path.GetFileName(CertificatePath), this.certificate.NotBefore, this.certificate.NotAfter);
					this.securelistener = (SecureHttpListener) HttpListener.Create(IPAddress.Parse(ListenAddress), Port, this.certificate);
					this.securelistener.UseClientCertificate = true;
					this.server.Add(this.securelistener);
				}
				else
				{
					this.listener = HttpListener.Create(IPAddress.Parse(ListenAddress), Port);
					this.server.Add(this.listener);
				}
				this.reader.ContentTypes.Clear();
				this.reader.ContentTypes.Add("default", new ContentTypeHeader("application/octet-stream"));
				foreach (var mimetype in AllowedMimeTypes)
				{
					var sbstr = mimetype.Split(',');
					switch (sbstr.Length)
					{
						case 2:
							if (sbstr[0].Length != 0 && sbstr[1].Length != 0)
							{
								try
								{
									this.reader.ContentTypes.Add(sbstr[0], new ContentTypeHeader(sbstr[1]));
								}
								catch (ArgumentException)
								{
									Console.ForegroundColor = ConsoleColor.Yellow;
									Console.WriteLine("({0}) <Web Interface> Config.xml contains duplicate Mime Types.", DateTime.Now.ToString("hh:mm"));
								}
							}
							else
							{
								Console.ForegroundColor = ConsoleColor.Yellow;
								Console.WriteLine("({0}) <Web Interface> Config.xml contains invalid Mime Types.", DateTime.Now.ToString("hh:mm"));
							}
							break;
						default:
							Console.ForegroundColor = ConsoleColor.Yellow;
							Console.WriteLine("({0}) <Web Interface> Config.xml contains invalid Mime Types.", DateTime.Now.ToString("hh:mm"));
							break;
					}
				}
				try
				{
					this.server.Start(5);
					SessionManager.Start(this.server);
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("({0}) <Web Interface> Running on Port {1}...", DateTime.Now.ToString("hh:mm"), Port);
				}
				catch (SocketException e)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("({0}) <Web Interface> {1}", DateTime.Now.ToString("hh:mm"), e);
				}
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("({0}) <Web Interface> Disabled", DateTime.Now.ToString("hh:mm"));
			}
		}
	}
}
