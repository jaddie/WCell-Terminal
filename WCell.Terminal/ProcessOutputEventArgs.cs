using System;
using System.Collections.Generic;

namespace WCell.Terminal
{
	/// <summary> A delegate that handles the write to either std::out or std::in for a process </summary>
	public delegate void ProcessOutputEventHandler(object sender, ProcessOutputEventArgs args);
	/// <summary> 
	/// The event args that contains information about the line of text written to either
	/// std::out or std::in on the created process. 
	/// </summary>
	public sealed class ProcessOutputEventArgs : EventArgs
	{
		private readonly bool _isError;
		private readonly string _data;

		internal ProcessOutputEventArgs(string output, bool iserror)
		{
			_data = output;
			_isError = iserror;
		}

		/// <summary> Returns the line of text written to standard out/error  </summary>
		public String Data { get { return _data; } }
		/// <summary> Returns true if the line of text was written to std::error </summary>
		public bool Error { get { return _isError; } }
	}
}