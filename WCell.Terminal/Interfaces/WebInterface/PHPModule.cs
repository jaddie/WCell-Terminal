using System;
using System.IO;
using HttpServer;
using HttpServer.Sessions;
using HttpServer.HttpModules;

namespace WCell.Terminal
{
	/// <summary>
	/// Module to invoke php scripts.
	/// </summary>
	public class PhpModule : HttpModule
	{
		private readonly string _basePath;

		/// <summary>
		/// Initializes a new instance of the <see cref="PhpModule"/> class.
		/// </summary>
		/// <param name="basePath">Absolute path to the "public"/"inetpub" directory.</param>
		public PhpModule(string basePath)
		{
			_basePath = basePath;
			if (_basePath.EndsWith("\\"))
			{
				_basePath = _basePath.Remove(0, _basePath.Length - 1);
			}
		}

		/// <summary>
		/// Method that process the url
		/// </summary>
		/// <param name="request">Information sent by the browser about the request</param>
		/// <param name="response">Information that is being sent back to the client.</param>
		/// <param name="session">Session used to </param>
		/// <returns>true if this module handled the request.</returns>
		public override bool Process(IHttpRequest request, IHttpResponse response, IHttpSession session)
		{
			if (!request.Uri.IsFile || !request.Uri.AbsolutePath.EndsWith(".php"))
				return false;

			string path = _basePath + request.Uri.AbsolutePath.Replace('/', '\\').Replace("..", "");
			return File.Exists(path) && InvokePhpScript(request, response, session);
		}

		private bool InvokePhpScript(IHttpRequest request, IHttpResponse response, IHttpSession session)
		{
			return false;
		}
	}
}