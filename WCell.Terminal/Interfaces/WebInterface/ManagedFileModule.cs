using System;
using System.Text;
using System.IO;
using System.Net;
using HttpServer;
using HttpServer.Headers;
using HttpServer.Messages;
using HttpServer.Modules;

namespace WCell.Terminal
{
	public class ManagedFileModule : IModule
	{
		public ProcessingResult Process(RequestContext requestcontext)
		{
			IRequest request = requestcontext.Request;
			IResponse response = requestcontext.Response;
			IHttpContext context = requestcontext.HttpContext;

			if (SessionManager.Current.SessionId == null)
			{
				SessionManager.Create();
				SessionManager.Current.SessionId = Guid.NewGuid().ToString().Replace("-", String.Empty);
			}
			if (request.Method == Method.Post && request.Parameters.Count == 3)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("({0}) <Web Interface> AJAX request from {1} {2} {3} {4}", DateTime.Now.ToString("hh:mm"), context.RemoteEndPoint, request.HttpVersion, request.Method, request.Uri.AbsoluteUri);
				Console.WriteLine("({0}) <Web Interface> AJAX parameters width={1}&height={2}&rooturl={3}", DateTime.Now.ToString("hh:mm"), request.Parameters.Get("width").Value, request.Parameters.Get("height").Value, request.Parameters.Get("rooturl").Value);
				Console.WriteLine("({0}) <Web Interface> AJAX session id {1}", DateTime.Now.ToString("hh:mm"), SessionManager.Current.SessionId);
				response.ContentType = new ContentTypeHeader("application/json");
				response.Status = HttpStatusCode.OK;
				var stream = new MemoryStream(ASCIIEncoding.Default.GetBytes("{\"session\":\"" + SessionManager.Current.SessionId + "\",\"data\":\"Connected to server-side. Welcome to WCell.Terminal\"}"));
				response.ContentLength.Value = stream.Length;
				var generator = new ResponseWriter();
				generator.SendHeaders(context, response);
				generator.SendBody(context, stream);
				Console.WriteLine("({0}) <Web Interface> AJAX response {1}", DateTime.Now.ToString("hh:mm"), "{\"session\":\"" + SessionManager.Current.SessionId + "\",\"data\":\"Connected to server-side. Welcome to WCell.Terminal\"}");
				return ProcessingResult.Abort;
			}
			else
			{
				if (context.IsSecure)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("({0}) <Web Interface> Secure HTTP request from {1} {2} {3} {4}", DateTime.Now.ToString("hh:mm"), context.RemoteEndPoint, request.HttpVersion, request.Method, request.Uri.AbsoluteUri);
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("({0}) <Web Interface> HTTP request from {1} {2} {3} {4}", DateTime.Now.ToString("hh:mm"), context.RemoteEndPoint, request.HttpVersion, request.Method, request.Uri.AbsoluteUri);
				}
			}
			return ProcessingResult.Continue;
		}
	}
}