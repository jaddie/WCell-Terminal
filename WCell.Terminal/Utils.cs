using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;

namespace WCell.Terminal
{
	class Utils
	{
		private static readonly string FileNotFoundMessage = new FileNotFoundException().Message;
		private static readonly char[] IllegalFileNameChars = new char[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };

		/// <summary>
		/// Returns the fully qualified path to the file if it is fully-qualified, exists in the current directory, or 
		/// in the environment path, otherwise generates a FileNotFoundException exception.
		/// </summary>
		public static string FindFullPath(string location)
		{
			string result;
			if (TrySearchPath(location, out result))
				return result;
			throw new FileNotFoundException(FileNotFoundMessage, location);
		}

		/// <summary>
		/// Returns true if the file is fully-qualified, exists in the current directory, or in the environment path, 
		/// otherwise generates a FileNotFoundException exception.  Will not propagate errors.
		/// </summary>
		public static bool TrySearchPath(string location, out string fullPath)
		{
			fullPath = null;

			try
			{
				if (File.Exists(location))
				{
					fullPath = Path.GetFullPath(location);
					return true;
				}

				if (Path.IsPathRooted(location))
					return false;
				if (location.IndexOfAny(IllegalFileNameChars) >= 0 || location.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
					return false;

				foreach (string pathentry in Environment.GetEnvironmentVariable("PATH").Split(';'))
				{
					string testPath = pathentry.Trim();
					if (testPath.Length > 0 && Directory.Exists(testPath) && File.Exists(Path.Combine(testPath, location)))
					{
						fullPath = Path.GetFullPath(Path.Combine(testPath, location));
						return true;
					}
				}
			}
			catch (System.Threading.ThreadAbortException) { throw; }
			catch (Exception error) { Trace.TraceError("{0}", error); }

			return false;
		}
	}
}