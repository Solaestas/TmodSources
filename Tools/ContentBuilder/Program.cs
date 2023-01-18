// See https://aka.ms/new-console-template for more information
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            "Microsoft.Xna.Framework.dll",
            "Microsoft.Xna.Framework.Content.Pipeline.dll",
            "Microsoft.Xna.Framework.Content.Pipeline.EffectImporter.dll",
        };

        var inputFiles = args[0].Split(';');    //输入文件路径
        var intermediateDirectory = args[1];
        var outputDir = args[2];                //输出文件夹
        var asmDir = args[3];                   //程序集路径
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
    public bool ContinueOnError => false;

    public int LineNumberOfTaskNode => 0;

    public int ColumnNumberOfTaskNode => 0;

    public string ProjectFileOfTaskNode => string.Empty;

    public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
    {
        throw new NotImplementedException("Not Implemented");
    }

    public void LogCustomEvent(CustomBuildEventArgs e) => throw new NotImplementedException();

    public void LogErrorEvent(BuildErrorEventArgs e) => Console.WriteLine(JsonConvert.SerializeObject(e, new BuildEventArgsConverter()));

    public void LogMessageEvent(BuildMessageEventArgs e) => Console.WriteLine(JsonConvert.SerializeObject(e, new BuildEventArgsConverter()));

    public void LogWarningEvent(BuildWarningEventArgs e) => Console.WriteLine(JsonConvert.SerializeObject(e, new BuildEventArgsConverter()));
}

public enum MessageType
{
    Error,
    Warning,
    Message
}

public class BuildEventArgsConverter : JsonConverter<LazyFormattedBuildEventArgs>
{
    public override LazyFormattedBuildEventArgs ReadJson(JsonReader reader, Type objectType, LazyFormattedBuildEventArgs existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (hasExistingValue)
        {
            throw new NotImplementedException();
        }

        var obj = JObject.Load(reader);
        return (MessageType)obj["Type"].Value<int>() switch
        {
            MessageType.Error => new BuildErrorEventArgs(
                obj[nameof(BuildErrorEventArgs.Subcategory)].Value<string>(),
                obj[nameof(BuildErrorEventArgs.Code)].Value<string>(),
                obj[nameof(BuildErrorEventArgs.File)].Value<string>(),
                obj[nameof(BuildErrorEventArgs.LineNumber)].Value<int>(),
                obj[nameof(BuildErrorEventArgs.ColumnNumber)].Value<int>(),
                obj[nameof(BuildErrorEventArgs.EndColumnNumber)].Value<int>(),
                obj[nameof(BuildErrorEventArgs.EndColumnNumber)].Value<int>(),
                obj[nameof(BuildErrorEventArgs.Message)].Value<string>(),
                obj[nameof(BuildErrorEventArgs.HelpKeyword)].Value<string>(),
                obj[nameof(BuildErrorEventArgs.SenderName)].Value<string>(),
                obj[nameof(BuildErrorEventArgs.Timestamp)].ToObject<DateTime>()),
            MessageType.Warning => new BuildWarningEventArgs(
                obj[nameof(BuildWarningEventArgs.Subcategory)].Value<string>(),
                obj[nameof(BuildWarningEventArgs.Code)].Value<string>(),
                obj[nameof(BuildWarningEventArgs.File)].Value<string>(),
                obj[nameof(BuildWarningEventArgs.LineNumber)].Value<int>(),
                obj[nameof(BuildWarningEventArgs.ColumnNumber)].Value<int>(),
                obj[nameof(BuildWarningEventArgs.EndColumnNumber)].Value<int>(),
                obj[nameof(BuildWarningEventArgs.EndColumnNumber)].Value<int>(),
                obj[nameof(BuildWarningEventArgs.Message)].Value<string>(),
                obj[nameof(BuildWarningEventArgs.HelpKeyword)].Value<string>(),
                obj[nameof(BuildWarningEventArgs.SenderName)].Value<string>(),
                obj[nameof(BuildWarningEventArgs.Timestamp)].ToObject<DateTime>()),
            MessageType.Message => new BuildMessageEventArgs(
                obj[nameof(BuildMessageEventArgs.Message)].Value<string>(),
                obj[nameof(BuildMessageEventArgs.HelpKeyword)].Value<string>(),
                obj[nameof(BuildMessageEventArgs.SenderName)].Value<string>(),
                (MessageImportance)obj[nameof(BuildMessageEventArgs.Importance)].Value<int>(),
                obj[nameof(BuildMessageEventArgs.Timestamp)].ToObject<DateTime>()
                ),
            _ => throw new NotImplementedException(),
        };
    }

    public override void WriteJson(JsonWriter writer, LazyFormattedBuildEventArgs value, JsonSerializer serializer)
    {
        var json = JObject.FromObject(value);
        json.Add(new JProperty("Type", value switch
        {
            BuildMessageEventArgs => MessageType.Message,
            BuildWarningEventArgs => MessageType.Warning,
            BuildErrorEventArgs => MessageType.Error,
            _ => throw new NotImplementedException()
        }));
        json.WriteTo(writer);
    }
}