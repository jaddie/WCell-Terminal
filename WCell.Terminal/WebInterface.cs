﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.IO;
using HttpServer;
using HttpServer.Authentication;
using HttpServer.Exceptions;
using HttpServer.HttpModules;
using HttpServer.Rules;
using HttpListener = HttpServer.HttpListener;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace WCell.Terminal
{
	class WebInterface
	{
		class User
		{
			public int id;
			public string userName;
			public User(int id, string userName)
			{
				this.id = id;
				this.userName = userName;
			}
		}

		private HttpServer.HttpServer _server;
		private X509Certificate2 _cert;

		public static bool WebInterfaceEnabled = true;
		public static string ListenAddress = IPAddress.Any.ToString();
		public static bool UseSSL = false;
		public static string CertificatePath = Path.Combine(Directory.GetCurrentDirectory(), "certificate.cer");
		public static int Port = 8080;
		public static string Username = "Admin";
		public static string Password = "passw0rd";
		public static string[] AllowedMimeTypes = {
			"txt,text/plain",
			"htm,text/html",
			"html,text/html",
			"css,text/css",
			"js,application/x-javascript",
			"ico,image/x-icon",
			"bmp,image/bmp",
			"dib,image/bmp",
			"png,image/png",
			"pnz,image/png",
			"jpe,image/jpeg",
			"jpeg,image/jpeg",
			"jpg,image/jpeg",
			"jfif,image/jpeg",
			"gif,image/gif",
			"tif,image/tiff",
			"tiff,image/tiff",
			"jar,application/java-archive",
			"swf,application/x-shockwave-flash"
		};

		public WebInterface()
		{
			if (WebInterfaceEnabled)
			{
				_server = new HttpServer.HttpServer();
				_server.ServerName = "WCell.Terminal WebInterface Embedded WebServer";
				DigestAuthentication auth = new DigestAuthentication(OnAuthenticate, OnAuthenticationRequired);
				_server.AuthenticationModules.Add(auth);
				_server.ExceptionThrown += OnException;
				FileModule _module = new FileModule(@"/", Path.Combine(Directory.GetCurrentDirectory(), "WebInterface"));
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
									_module.MimeTypes.Add(sbstr[0], sbstr[1]);
								}
								catch (ArgumentException)
								{
									Console.ForegroundColor = ConsoleColor.Blue;
									Console.WriteLine("({0}) <Web Interface> Config.xml contains duplicate Mime Types.", DateTime.Now.ToString("hh:mm"));
								}
							}
							else
							{
								Console.ForegroundColor = ConsoleColor.Blue;
								Console.WriteLine("({0}) <Web Interface> Config.xml contains invalid Mime Types.", DateTime.Now.ToString("hh:mm"));
							}
							break;
						default:
							Console.ForegroundColor = ConsoleColor.Blue;
							Console.WriteLine("({0}) <Web Interface> Config.xml contains invalid Mime Types.", DateTime.Now.ToString("hh:mm"));
							break;
					}
				}
				_server.Add(_module);
				if (UseSSL)
				{
					try
					{
						_cert = new X509Certificate2(CertificatePath);
						Console.ForegroundColor = ConsoleColor.Blue;
						Console.WriteLine("({0}) <Web Interface> Loaded Certificate: {1} Valid Date: {2} Expiry Date: {3}", DateTime.Now.ToString("hh:mm"), Path.GetFileName(CertificatePath), _cert.NotBefore, _cert.NotAfter);
						try
						{
							_server.Start(IPAddress.Parse(ListenAddress), Port, _cert, SslProtocols.Default, ValidateServerCertificate, false);
							Console.ForegroundColor = ConsoleColor.Blue;
							Console.WriteLine("({0}) <Web Interface> Running on Port {1}...", DateTime.Now.ToString("hh:mm"), Port);
						}
						catch (SocketException e)
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("({0}) <Web Interface> {1}", DateTime.Now.ToString("hh:mm"), e);
						}
					}
					catch (DirectoryNotFoundException)
					{
						Console.ForegroundColor = ConsoleColor.Blue;
						Console.WriteLine("({0}) <Web Interface> Error: The directory specified could not be found.", DateTime.Now.ToString("hh:mm"));
					}
					catch (IOException)
					{
						Console.ForegroundColor = ConsoleColor.Blue;
						Console.WriteLine("({0}) <Web Interface> Error: A file in the directory could not be accessed.", DateTime.Now.ToString("hh:mm"));
					}
					catch (NullReferenceException)
					{
						Console.ForegroundColor = ConsoleColor.Blue;
						Console.WriteLine("({0}) <Web Interface> File must be a .cer file. Program does not have access to that type of file.", DateTime.Now.ToString("hh:mm"));
					}
				}
				else
				{
					try
					{
						_server.Start(IPAddress.Parse(ListenAddress), Port);
						Console.ForegroundColor = ConsoleColor.Blue;
						Console.WriteLine("({0}) <Web Interface> Running on Port {1}...", DateTime.Now.ToString("hh:mm"), Port);
					}
					catch (SocketException e)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("({0}) <Web Interface> {1}", DateTime.Now.ToString("hh:mm"), e);
					}
				}
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine("({0}) <Web Interface> Disabled", DateTime.Now.ToString("hh:mm"));
			}
		}

		~WebInterface()
		{
			_server.Stop();
		}

		public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None)
			{
				return true;
			}
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("{0}) <Web Interface> Certificate error: {1}", DateTime.Now.ToString("hh:mm"), sslPolicyErrors);
			return false;
		}

		private void OnException(object source, Exception e)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("({0}) <Web Interface> {1}", DateTime.Now.ToString("hh:mm"), e);
		}

		private bool OnAuthenticationRequired(IHttpRequest request)
		{
			return request.Uri.AbsolutePath.StartsWith("/");
		}

		private void OnAuthenticate(string realm, string userName, ref string password, out object login)
		{
			if (userName == Username)
			{
				password = Password;
				login = new User(1, Username);
				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine("({0}) <Web Interface> User {1} logged in.", DateTime.Now.ToString("hh:mm"), userName);
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("({0}) <Web Interface> User {1} tryied to login with invalid password {2}.", DateTime.Now.ToString("hh:mm"), userName, password);
				password = string.Empty;
				login = null;
			}
		}

		/*private void OnRequest(object source, RequestEventArgs args)
		{
			IHttpClientContext context = (IHttpClientContext)source;
			IHttpRequest request = args.Request;
			IHttpResponse response = request.CreateResponse(context);
			if (request.Secure)
			{
				Console.WriteLine("({0}) <Web Interface> Secure request from {1} {2} {3} {4}", DateTime.Now.ToString("hh:mm"), request.RemoteEndPoint, request.HttpVersion, request.Method, request.UriPath);				
			}
			else
			{
				Console.WriteLine("({0}) <Web Interface> Request from {1} {2} {3} {4}", DateTime.Now.ToString("hh:mm"), request.RemoteEndPoint, request.HttpVersion, request.Method, request.UriPath);
			}

			if (request.Uri.AbsolutePath == "/hello")
			{
				context.Respond("Hello to you too!");
			}
			else if (request.UriParts.Length == 1 && request.UriParts[0] == "goodbye")
			{
				StreamWriter writer = new StreamWriter(response.Body);
				writer.WriteLine("Goodbye to you too!");
				writer.Flush();
				response.Send();
			}
			else
			{
				byte[] body = Encoding.UTF8.GetBytes("Hello secure you!");
				response.Body.Write(body, 0, body.Length);
				response.Send();
			}
		}*/
	}
}