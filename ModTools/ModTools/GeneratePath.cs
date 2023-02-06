using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ModTools;

public class GeneratePath : Task
{
	[Required]
	public ITaskItem[] AssetFiles { get; set; }

	[Required]
	public string OutputDirectory { get; set; }

	[Required]
	public string ModName { get; set; }

	public override bool Execute()
	{
		var sb = new StringBuilder();
		foreach (var file in AssetFiles)
		{
			var identity = file.GetMetadata("Identity");
			string fieldName = file.GetMetadata("Filename").Replace('-', '_').Replace(" ", string.Empty);
			if (char.IsNumber(fieldName[0]))
			{
				fieldName = '_' + fieldName;
			}

			if (GetFileType(file.ItemSpec) is string type)
			{
				sb.AppendLine(
				$"""
					public const string {fieldName}Path = @"{ModName}/{identity[0..^Path.GetExtension(identity).Length].Replace('\\', '/')}";
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
			Path.Combine(OutputDirectory, "ModPath.g.cs"),
			$$"""
			using Microsoft.Xna.Framework.Graphics;
			using ReLogic.Content;
			using Terraria.ModLoader;

			namespace {{ModName}};

			public static class ModPath
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