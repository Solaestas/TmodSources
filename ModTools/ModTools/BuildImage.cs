using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ModTools;

public class BuildImage : Microsoft.Build.Utilities.Task
{
	/// <summary>
	/// 输入文件
	/// </summary>
	[Required]
	public ITaskItem[] InputFiles { get; set; }

	/// <summary>
	/// 输出文件夹
	/// </summary>
	[Required]
	public string OutputDirectory { get; set; }

	public override bool Execute()
	{
		Log.LogMessage(MessageImportance.High, "Building Images...");
		foreach(var file in InputFiles)
		{
			var relativeDir = file.GetMetadata("RelativeDir");
			var filename = file.GetMetadata("Filename");
			var identity = file.GetMetadata("Identity");

			string dir = Path.Combine(OutputDirectory, relativeDir);
			string filePath = Path.Combine(dir, $"{filename}.rawimg");
			Log.LogMessage(MessageImportance.High, $"Building {identity} -> {filePath}");

			Directory.CreateDirectory(dir);
			using var output = File.Create(filePath);
			using var input = File.OpenRead(file.ItemSpec);
			ImageIO.ToRaw(input, output);
		};
		return true;
	}
}

/// <summary>
/// Copy From tModLoader
/// </summary>
public static class ImageIO
{
	public static void ToRaw(Stream source, Stream destination)
	{
		var image = Image.Load<Rgba32>(source);
		using BinaryWriter writer = new BinaryWriter(destination);

		// 不知道为啥要写一个1进去
		writer.Write(1);
		int width = image.Width;
		int height = image.Height;
		writer.Write(width);
		writer.Write(height);

		image.ProcessPixelRows(accessor =>
		{
			for (int i = 0; i < accessor.Height; i++)
			{
				foreach (var color in accessor.GetRowSpan(i))
				{
					if (color.A == 0)
					{
						// 直接写入四字节的int 0
						writer.Write(0);
						continue;
					}

					writer.Write(color.R);
					writer.Write(color.G);
					writer.Write(color.B);
					writer.Write(color.A);
				}
			}
		});
	}
}