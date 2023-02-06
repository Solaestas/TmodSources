// See https://aka.ms/new-console-template for more information
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Xna.Framework.Content.Pipeline.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal class Program
{
	private static void Main(string[] args)
	{
		string[] asmNames =
		{
			"Microsoft.Xna.Framework.Content.Pipeline.EffectImporter.dll",
		};

		var inputFiles = args[0].Split(';');    //输入文件路径
		var intermediateDirectory = args[1];
		var outputDir = args[2];                //输出文件夹
		var asmDir = Path.Combine(args[3], "Runtimes") + Path.DirectorySeparatorChar;                   //程序集路径
		var targetPlatform = args.ElementAtOrDefault(4);
		var targetProfile = args.ElementAtOrDefault(5);
		var buildConfiguration = args.ElementAtOrDefault(6);
		new BuildContent
		{
			SourceAssets = inputFiles.Select(path => new TaskItem(path, new Dictionary<string, string>
			{
				["Name"] = Path.GetFileNameWithoutExtension(path),
				["Importer"] = "EffectImporter",
				["Processor"] = "EffectProcessor",
			})).ToArray(),
			IntermediateDirectory = intermediateDirectory,
			OutputDirectory = outputDir,
			HostObject = null,
			BuildEngine = new BuildEngine(),
			PipelineAssemblies = asmNames.Select(name => new TaskItem($"{asmDir}{name}")).ToArray(),
			TargetPlatform = targetPlatform,
			TargetProfile = targetProfile,
			BuildConfiguration = buildConfiguration,
		}.Execute();

	}
}

public class BuildEngine : IBuildEngine
{
	private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
	{
		Converters = { new BuildEventArgsConverter() },
		Formatting = Formatting.None, // Must be None
	};
	public bool ContinueOnError => false;

	public int LineNumberOfTaskNode => 0;

	public int ColumnNumberOfTaskNode => 0;

	public string ProjectFileOfTaskNode => string.Empty;

	public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
	{
		throw new NotImplementedException("Not Implemented");
	}

	public void LogCustomEvent(CustomBuildEventArgs e) => throw new NotImplementedException();

	public void LogErrorEvent(BuildErrorEventArgs e) => Console.WriteLine(JsonConvert.SerializeObject(e, settings));

	public void LogMessageEvent(BuildMessageEventArgs e) => Console.WriteLine(JsonConvert.SerializeObject(e, settings));

	public void LogWarningEvent(BuildWarningEventArgs e) => Console.WriteLine(JsonConvert.SerializeObject(e, settings));
}