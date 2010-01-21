using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace WCell.Terminal
{
	/// <summary>
	/// Creates/Spawns a process with the standard error/out/in all mapped.  Subscribe to
	/// the OutputReceived event prior to start/run to listen to the program output, write
	/// to the StandardInput for input.
	/// </summary>
	public class ProcessRunner
	{
		private static readonly string[] EmptyArgList = new string[0];

		private readonly ManualResetEvent _mreProcessExit = new ManualResetEvent(true);
		private readonly ManualResetEvent _mreOutputDone = new ManualResetEvent(true);
		private readonly ManualResetEvent _mreErrorDone = new ManualResetEvent(true);

		private readonly string _executable;
		private readonly string[] _arguments;

		public string _workindir;

		private event ProcessOutputEventHandler _outputReceived;

		private volatile int _exitCode;
		private Process _running;
		private TextWriter _stdIn;

		/// <summary>Creates a ProcessRunner for the given executable </summary>
		public ProcessRunner(string executable)
			: this(executable, EmptyArgList)
		{ }
		/// <summary>Creates a ProcessRunner for the given executable and arguments </summary>
		public ProcessRunner(string executable, params string[] args)
		{
			_executable = Utils.FindFullPath(Check.NotEmpty(executable));
			_arguments = args == null ? EmptyArgList : args;
			_workindir = Path.GetDirectoryName(executable);
			_exitCode = 0;
			_running = null;
			_stdIn = null;
		}

		/// <summary> Notifies caller of writes to the std::err or std::out </summary>
		public event ProcessOutputEventHandler OutputReceived
		{
			add { lock (this) _outputReceived += value; }
			remove { lock (this) _outputReceived -= value; }
		}

		/// <summary> Allows writes to the std::in for the process </summary>
		public TextWriter StandardInput { get { return Check.NotNull(_stdIn); } }
		/// <summary> Waits for the process to exit and returns the exit code </summary>
		public int ExitCode { get { WaitForExit(); return _exitCode; } }

		/// <summary> Kills the process if it is still running </summary>
		public void Kill()
		{
			if (IsRunning)
			{
				try { if (_running != null && !_running.HasExited) _running.Kill(); }
				catch (System.InvalidOperationException) { }
			}
		}

		/// <summary> Closes std::in and waits for the process to exit </summary>
		public void WaitForExit()
		{
			WaitForExit(TimeSpan.MaxValue, true);
		}

		/// <summary> Closes std::in and waits for the process to exit, returns false if the process did not exit in the time given </summary>
		public bool WaitForExit(TimeSpan timeout) { return WaitForExit(timeout, true); }
		/// <summary> Waits for the process to exit, returns false if the process did not exit in the time given </summary>
		public bool WaitForExit(TimeSpan timeout, bool closeStdInput)
		{
			if (_stdIn != null && closeStdInput)
			{ _stdIn.Close(); _stdIn = null; }

			int waitTime = (int)Math.Min(int.MaxValue, timeout.TotalMilliseconds);
			return WaitHandle.WaitAll(new WaitHandle[] { _mreErrorDone, _mreOutputDone, _mreProcessExit }, waitTime);
		}

		/// <summary> Returns true if this instance is running a process </summary>
		public bool IsRunning { get { return !WaitForExit(TimeSpan.FromTicks(0), false); } }

		#region Run, Start, & Overloads
		/// <summary> Runs the process and returns the exit code. </summary>
		public int Run()
		{ return InternalRun(_arguments); }

		/// <summary> Runs the process with additional arguments and returns the exit code. </summary>
		public int Run(params string[] moreArguments)
		{
			List<string> args = new List<string>(_arguments);
			args.AddRange(moreArguments == null ? EmptyArgList : moreArguments);
			return InternalRun(args.ToArray());
		}

		/// <summary> 
		/// Calls String.Format() for each argument this runner was constructed with giving the object
		/// array as the arguments.  Once complete it runs the process with the new set of arguments and
		/// returns the exit code.
		/// </summary>
		public int RunFormatArgs(params object[] formatArgs)
		{
			Check.NotNull(formatArgs);
			List<string> args = new List<string>();
			foreach (string arg in _arguments)
				args.Add(String.Format(arg, formatArgs));
			return InternalRun(args.ToArray());
		}

		/// <summary> Starts the process and returns. </summary>
		public void Start()
		{ InternalStart(_arguments); }

		/// <summary> Starts the process with additional arguments and returns. </summary>
		public void Start(params string[] moreArguments)
		{
			List<string> args = new List<string>(_arguments);
			args.AddRange(moreArguments == null ? EmptyArgList : moreArguments);
			InternalStart(args.ToArray());
		}

		/// <summary> 
		/// Calls String.Format() for each argument this runner was constructed with giving the object
		/// array as the arguments.  Once complete it starts the process with the new set of arguments and
		/// returns.
		/// </summary>
		public void StartFormatArgs(params object[] formatArgs)
		{
			Check.NotNull(formatArgs);
			List<string> args = new List<string>();
			foreach (string arg in _arguments)
				args.Add(String.Format(arg, formatArgs));
			InternalStart(args.ToArray());
		}
		#endregion Run, Start, & Overloads
		
		private int InternalRun(string[] arguments)
		{
			InternalStart(arguments);
			WaitForExit();
			return ExitCode;
		}

		private void InternalStart(params string[] arguments)
		{
			if (IsRunning)
				throw new InvalidOperationException("The running process must first exit.");

			_mreProcessExit.Reset();
			_mreOutputDone.Reset();
			_mreErrorDone.Reset();
			_exitCode = 0;
			_stdIn = null;
			_running = new Process();

			string stringArgs = ArgumentList.Join(arguments);
			ProcessStartInfo psi = new ProcessStartInfo(_executable, stringArgs);
			if (_workindir != null)
			{
				psi.WorkingDirectory = Environment.CurrentDirectory;
			}
			else
			{
				psi.WorkingDirectory = _workindir;
			}

			psi.RedirectStandardInput = true;
			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;
			psi.CreateNoWindow = true;
			psi.UseShellExecute = false;
			psi.ErrorDialog = false;

			_running.StartInfo = psi;

			_running.Exited += process_Exited;
			_running.OutputDataReceived += process_OutputDataReceived;
			_running.ErrorDataReceived += process_ErrorDataReceived;

			_running.EnableRaisingEvents = true;
			Trace.TraceInformation("EXEC: {0} {1}", _running.StartInfo.FileName, _running.StartInfo.Arguments);
			_running.Start();

			_running.BeginOutputReadLine();			
			_running.BeginErrorReadLine();
			_stdIn = _running.StandardInput;
		}

		private void OnOutputReceived(ProcessOutputEventArgs args)
		{
			lock (this)
			{
				if (_outputReceived != null)
					_outputReceived(this, args);
			}
		}

		void process_Exited(object o, EventArgs e)
		{
			Trace.TraceInformation("EXIT: {0}", _running.StartInfo.FileName);
			_exitCode = _running.ExitCode;
			_mreProcessExit.Set();
		}

		void process_OutputDataReceived(object o, DataReceivedEventArgs e)
		{
			if (e.Data != null)
				OnOutputReceived(new ProcessOutputEventArgs(e.Data, false));
			else
				_mreOutputDone.Set();
		}

		void process_ErrorDataReceived(object o, DataReceivedEventArgs e)
		{
			if (e.Data != null)
				OnOutputReceived(new ProcessOutputEventArgs(e.Data, true));
			else
				_mreErrorDone.Set();
		}
	}
}