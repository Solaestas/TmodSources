using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;

namespace XNBCompiler;

public class BuildEngine : IBuildEngine
{
	private string logFile = "";

	private StreamWriter logWriter;

	private List<string> errors;

	public bool log;

	public int ColumnNumberOfTaskNode => 0;

	public bool ContinueOnError => true;

	public int LineNumberOfTaskNode => 0;

	public string ProjectFileOfTaskNode => string.Empty;

	public BuildEngine()
	{
		errors = new List<string>();
		log = true;
	}

	public BuildEngine(string logFile)
	{
		this.logFile = logFile;
		log = true;
		try
		{
			logWriter = new StreamWriter(logFile, append: true);
		}
		catch
		{
			log = false;
		}
	}

	public void Begin()
	{
		if (log)
		{
			errors = new List<string>();
		}
	}

	private void Log(string message)
	{
		if (log)
		{
			try
			{
				Program.lastMessage = message;
				logWriter.WriteLine(message);
			}
			catch
			{
			}
		}
	}

	public void End()
	{
		if (log)
		{
			try
			{
				logWriter.Flush();
				logWriter.Close();
			}
			catch
			{
			}
		}
	}

	public List<string> GetErrors()
	{
		return errors;
	}

	public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
	{
		return true;
	}

	public void LogCustomEvent(CustomBuildEventArgs e)
	{
		if (log)
		{
			Log("Custom Event at " + DateTime.Now.ToString() + ": " + e.Message);
		}
	}

	public void LogErrorEvent(BuildErrorEventArgs e)
	{
		if (log)
		{
			Log("Error at " + DateTime.Now.ToString() + ": " + e.Message);
		}
		errors.Add(DateTime.Now.ToString() + ": " + e.Message);
	}

	public void LogMessageEvent(BuildMessageEventArgs e)
	{
		if (log)
		{
			Log("Message at " + DateTime.Now.ToString() + ": " + e.Message);
		}
	}

	public void LogWarningEvent(BuildWarningEventArgs e)
	{
		if (log)
		{
			Log("Warning at " + DateTime.Now.ToString() + ": " + e.Message);
		}
	}
}
