using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;

namespace ModGenerator;

[Generator]
public class PathGenerator : ISourceGenerator
{
	public void Execute(GeneratorExecutionContext context)
	{
		var sb = new StringBuilder();
		var directory = Environment.CurrentDirectory;   //似乎是sln的位置
		string modName = context.Compilation.AssemblyName;
		//找一下csproj位置
		//directory = Path.GetDirectoryName(Directory.EnumerateFiles(directory, $"{modName}.csproj", SearchOption.AllDirectories).First());
		foreach (var file in Directory.EnumerateFiles(directory, "*.png", SearchOption.AllDirectories))
		{
			var identity = file.Replace(directory, "")[1..^4];//去掉前缀斜杠与后缀名
			string fieldName = Path.GetFileNameWithoutExtension(file);
			if (char.IsNumber(fieldName[0]))
			{
				fieldName = '_' + fieldName;
			}
			sb.AppendLine($@"	public const string {fieldName}Path = @""{modName}/{identity.Replace('\\', '/')}""");
			sb.AppendLine($@"	public const string {fieldName} = ModContent.Request<Texture2D>({fieldName}Path)");
		}

		foreach (var file in Directory.EnumerateFiles(directory, "*.fx", SearchOption.AllDirectories))
		{
			var identity = file.Replace(directory, "")[1..^3];//去掉前缀斜杠与后缀名
			string fieldName = Path.GetFileNameWithoutExtension(file);
			if (char.IsNumber(fieldName[0]))
			{
				fieldName = '@' + fieldName;
			}
			sb.AppendLine($@"	public const string {fieldName}Path = @""{modName}/{identity.Replace('\\', '/')}""");
			sb.AppendLine($@"	public const string {fieldName} = ModContent.Request<Effect>({fieldName}Path, ReLogic.Content.AssetRequestMode.ImmediateLoad)");
		}

		var source =
$$"""
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace {{modName}};

public static class ModPath
{
	public const string ModSourcePath = @"{{directory}}";
{{sb}}
}
"""
;
		context.AddSource("ModPath.g.cs", source);
	}

	public void Initialize(GeneratorInitializationContext context)
	{
	}
}