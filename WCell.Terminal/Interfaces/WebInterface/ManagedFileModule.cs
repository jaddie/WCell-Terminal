using System;
using HttpServer;
using HttpServer.HttpModules;

namespace WCell.Terminal
{
	class ManagedFileModule : FileModule
	{
		public ManagedFileModule(string baseUri, string basePath, bool useLastModifiedHeader): base(baseUri, basePath, useLastModifiedHeader)
		{}

		public ManagedFileModule(string baseUri, string basePath) : this(baseUri, basePath, false)
		{}

		public override bool Process(IHttpRequest request, IHttpResponse response, HttpServer.Sessions.IHttpSession session)
		{
			if (request.Secure)
			{
				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine("({0}) <Web Interface> Secure request from {1} {2} {3} {4}", DateTime.Now.ToString("hh:mm"), request.RemoteEndPoint, request.HttpVersion, request.Method, request.UriPath);
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine("({0}) <Web Interface> Request from {1} {2} {3} {4}", DateTime.Now.ToString("hh:mm"), request.RemoteEndPoint, request.HttpVersion, request.Method, request.UriPath);
			}
			return base.Process(request, response, session);
		}
	}
}
