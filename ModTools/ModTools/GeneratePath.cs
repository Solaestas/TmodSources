using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ModTools;

public sealed class PathField
{
	public PathField(string filename, string identity)
	{
		Filename = filename;
		Identity = identity;
	}

	public string Filename { get; set; }

	public List<string> Prefixes { get; set; }

	public string[] Parents { get; set; }

	public string Identity { get; set; }

	public int Index { get; set; }

	public bool Conflicted { get; set; } = false;

	public void Conflict()
	{
		if (Conflicted)
		{
			return;
		}
		Conflicted = true;
		Prefixes = new List<string>();
		Parents = Identity.Split('/');
		Index = Parents.Length - 1;
	}

	public string GetFieldName()
	{
		if (!Conflicted)
		{
			return Filename;
		}

		return string.Join("_", Prefixes.Concat(new[] { Filename }));
	}
}

public class GeneratePath : Task
{
	[Required]
	public ITaskItem[] AssetFiles { get; set; }

	[Required]
	public string OutputDirectory { get; set; }

	[Required]
	public string Namespace { get; set; }

	public string ClassName { get; set; } = "ModAsset";

	public override bool Execute()
	{
		var sb = new StringBuilder();
		var assetPrefix = Namespace.Replace('.', '/');
		Dictionary<string, PathField> fields = new();
		foreach (var file in AssetFiles)
		{
			var filename = file.GetMetadata("Filename");
			var field = new PathField(filename, file.GetMetadata("Identity").Replace('\\', '/'));
			if (fields.TryGetValue(filename, out var exist))
			{
				field.Conflict();
				exist.Conflict();
				while (true)
				{
					var prefix = field.Index > 0 ? field.Parents[--field.Index] : null;
					var eprefix = exist.Index > 0 ? exist.Parents[--exist.Index] : null;
					if (prefix != eprefix)
					{
						if (eprefix != null)
							exist.Prefixes.Insert(0, eprefix);
						if (prefix != null)
							field.Prefixes.Insert(0, prefix);
						fields.Remove(filename);
						fields.Add(exist.GetFieldName(), exist);
						fields.Add(field.GetFieldName(), field);
						break;
					}
				}
			}
			else
			{
				fields.Add(filename, field);
			}
		}

		foreach (var pair in fields)
		{
			var identity = pair.Value.Identity;
			string fieldName = pair.Value.GetFieldName();
			if (char.IsNumber(fieldName[0]))
			{
				fieldName = '_' + fieldName;
			}
			if (GetFileType(identity) is string type)
			{
				sb.AppendLine(
				$"""
					public const string {fieldName}Path = @"{assetPrefix}/{identity[0..^Path.GetExtension(identity).Length]}";
				""");
				sb.AppendLine($"\tpublic static Asset<{type}> {fieldName} => ModContent.Request<{type}>({fieldName}Path, AssetRequestMode.ImmediateLoad);");
			}
			else
			{
				sb.AppendLine(
				$"""
					public const string {fieldName}Path = @"{identity}";
				""");
			}
		}

		File.WriteAllText(
			Path.Combine(OutputDirectory, "ModAsset.g.cs"),
			$$"""
			using Microsoft.Xna.Framework.Graphics;
			using ReLogic.Content;
			using Terraria.ModLoader;

			namespace {{Namespace}};

			public static class {{ClassName}}
			{
			{{sb}}
			}
			""");
		return true;
	}

	public string GetFileType(string file) => Path.GetExtension(file) switch
	{
		".png" => "Texture2D",
		".fx" => "Effect",
		_ => null
	};
}