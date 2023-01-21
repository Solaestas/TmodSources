using System;
using System.Linq;
using CliWrap;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ModTools;

public class BuildEffect : Task
{
	/// <summary>
	/// 输入文件
	/// </summary>
	[Required]
	public string InputFiles { get; set; }

	/// <summary>
	/// 中间文件夹
	/// </summary>
	[Required]
	public string IntermediateDirectory { get; set; }

	/// <summary>
	/// 输出文件夹
	/// </summary>
	[Required]
	public string OutputDirectory { get; set; }

	/// <summary>
	/// Builder路径
	/// </summary>
	[Required]
	public string BuilderDirectory { get; set; }

	/// <summary>
	/// Support Platform : Windows, Xbox360, WindowsPhone
	/// </summary>
	public string TargetPlatform { get; set; }

	/// <summary> Support Profile : HiDef, Reach <br> And I don't know what they mean </summary>
	public string TargetProfile { get; set; }

	/// <summary>
	/// Configuration
	/// </summary>
	public string BuildConfiguration { get; set; }

	public override bool Execute()
	{
		bool success = true;
		Log.LogMessage(MessageImportance.High, "Building Effects...");
		var cli = Cli.Wrap($"{BuilderDirectory}ContentBuilder.exe")
		.WithArguments(new string[]
		{
			InputFiles,
			IntermediateDirectory,
			OutputDirectory,
			BuilderDirectory,
			TargetPlatform,
			TargetProfile,
			BuildConfiguration
		})
		| PipeTarget.ToDelegate(s =>
		{
			var args = JsonConvert.DeserializeObject<LazyFormattedBuildEventArgs>(s, new BuildEventArgsConverter());
			if (args is BuildMessageEventArgs msg)
			{
				BuildEngine.LogMessageEvent(msg);
			}
			else if (args is BuildWarningEventArgs warning)
			{
				BuildEngine.LogWarningEvent(warning);
			}
			else if (args is BuildErrorEventArgs error)
			{
				Log.LogMessage(MessageImportance.High, $"{DateTime.Now} : {error.Message}");
				BuildEngine.LogErrorEvent(error);
				success = false;
			}
		});
		cli.ExecuteAsync().Task.Wait();
		return success;
	}
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
				obj[nameof(BuildErrorEventArgs.EndLineNumber)].Value<int>(),
				obj[nameof(BuildErrorEventArgs.EndColumnNumber)].Value<int>(),
				//string.Join("\n", obj[nameof(BuildErrorEventArgs.Message)].Value<string>().Split('\n').Where(s => !s.Contains("Errors compiling"))),
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
				obj[nameof(BuildWarningEventArgs.EndLineNumber)].Value<int>(),
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