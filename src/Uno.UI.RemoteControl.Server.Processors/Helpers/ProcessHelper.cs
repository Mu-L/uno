﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Uno.Extensions;

namespace Uno.UI.RemoteControl.Helpers
{
	internal class ProcessHelper
	{
		private static readonly ILogger _log = typeof(ProcessHelper).Log();

		public static (int exitCode, string output, string error) RunProcess(string executable, string parameters, string? workingDirectory = null)
		{
			if (!OperatingSystem.IsWindows()
				&& executable.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			{
				executable = Path.GetFileNameWithoutExtension(executable);
			}

			if (_log.IsEnabled(LogLevel.Trace))
			{
				_log.LogTrace($"Executing '{executable} {parameters}' in '{workingDirectory ?? Environment.CurrentDirectory}'");
			}

			var p = new Process
			{
				StartInfo =
				{
					UseShellExecute = false,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					FileName = executable,
					Arguments = parameters
				}
			};

			if (workingDirectory != null)
			{
				p.StartInfo.WorkingDirectory = workingDirectory;
			}

			var output = new StringBuilder();
			var error = new StringBuilder();
			var elapsed = Stopwatch.StartNew();
			p.OutputDataReceived += (s, e) => { if (e.Data != null) { /* Log.LogMessage($"[{elapsed.Elapsed}] {e.Data}");*/ output.AppendLine(e.Data); } };
			p.ErrorDataReceived += (s, e) => { if (e.Data != null) { /*Log.LogError($"[{elapsed.Elapsed}] {e.Data}");*/ error.AppendLine(e.Data); } };

			if (p.Start())
			{
				p.BeginOutputReadLine();
				p.BeginErrorReadLine();
				p.WaitForExit();
				var exitCore = p.ExitCode;
				p.Close();

				return (exitCore, output.ToString(), error.ToString());
			}
			else
			{
				throw new Exception($"Failed to start [{executable}]");
			}
		}
	}
}
