using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using ModTools.ModLoader;

namespace ModTools;

public class BuildMod : Microsoft.Build.Utilities.Task
{
	/// <summary>
	/// 模组源码文件夹
	/// </summary>
	[Required]
	public string ModSourceDirectory { get; set; }

	/// <summary>
	/// 项目输出文件夹
	/// </summary>
	[Required]
	public string OutputDirectory { get; set; }

	/// <summary>
	/// 模组文件夹文件夹
	/// </summary>
	[Required]
	public string ModDirectory { get; set; }

	/// <summary>
	/// 模组名，默认为文件夹名 <br/> 模组名应该与ILoadable的命名空间和程序集名称相同
	/// </summary>
	public string ModName { get; set; }
	/// <summary>
	/// Debug or Release, 决定是否包含PDB
	/// </summary>
	public string Configuration { get; set; }
	/// <summary>
	/// 无预处理的资源文件，直接从源码中复制
	/// </summary>
	[Required]
	public ITaskItem[] ResourceFiles { get; set; }
	/// <summary>
	/// 需要经过预处理的特殊资源文件
	/// </summary>
	[Required]
	public ITaskItem[] AssetFiles { get; set; }

	public override bool Execute()
	{
		Log.LogMessage(MessageImportance.High, "Building Mod...");
		Log.LogMessage(MessageImportance.High, $"Building {ModName} -> {Path.Combine(ModDirectory, $"{ModName}.tmod")}");
		ModName ??= Path.GetFileName(ModSourceDirectory);

		Log.LogMessage(MessageImportance.High, "Reading Properties");
		var property = BuildProperties.ReadBuildFile(ModSourceDirectory);
		var tmod = new TmodFile(Path.Combine(ModDirectory, $"{ModName}.tmod"), ModName, property.Version);
		tmod.AddFile("Info", property.ToBytes());

		Log.LogMessage(MessageImportance.High, "Reading Assets");
		var assetDirectory = Path.Combine(OutputDirectory, "Assets") + Path.DirectorySeparatorChar;
		Parallel.ForEach(AssetFiles, file =>
		{
			tmod.AddFile(file.ItemSpec.Replace(assetDirectory, ""), File.ReadAllBytes(file.ItemSpec));
		});

		Log.LogMessage(MessageImportance.High, "Reading Resources");
		Parallel.ForEach(ResourceFiles, file =>
		{
			tmod.AddFile(file.GetMetadata("Identity"), File.ReadAllBytes(file.ItemSpec));
		});

		//Add dll and pdb
		Log.LogMessage(MessageImportance.High, "Reading Assemblies");
		Parallel.ForEach(Directory.GetFiles(OutputDirectory, "*", SearchOption.TopDirectoryOnly), file =>
		{
			var ext = Path.GetExtension(file);
			var name = Path.GetFileNameWithoutExtension(file);
			if (DefaultDependency.Contains(name))
			{
				return;
			}

			if (ext == ".dll")
			{
				if (name == ModName)
				{
					tmod.AddFile($"{name}.dll", File.ReadAllBytes(file));
				}
				else
				{
					tmod.AddFile($"lib/{name}.dll", File.ReadAllBytes(file));
				}
			}
			else if (ext == ".pdb" && Configuration != "Release")
			{
				tmod.AddFile($"{name}.pdb", File.ReadAllBytes(file));
			}
		});

		Log.LogMessage(MessageImportance.High, "Saving tMod File");
		tmod.Save();
		return true;
	}

	private static readonly string[] DefaultDependency = new string[]
	{
		"Basic.Reference.Assemblies.Net60",
		"CsvHelper",
		"FNA",
		"Hjson",
		"Humanizer",
		"Ionic.Zip.Reduced",
		"log4net",
		"Microsoft.Bcl.AsyncInterfaces",
		"Microsoft.Build.Locator",
		"Microsoft.CodeAnalysis.CSharp",
		"Microsoft.CodeAnalysis.CSharp.Workspaces",
		"Microsoft.CodeAnalysis",
		"Microsoft.CodeAnalysis.FlowAnalysis.Utilities",
		"Microsoft.CodeAnalysis.Workspaces",
		"Microsoft.CodeAnalysis.Workspaces.MSBuild",
		"Microsoft.Win32.SystemEvents",
		"Mono.Cecil",
		"Mono.Cecil.Mdb",
		"Mono.Cecil.Pdb",
		"Mono.Cecil.Rocks",
		"MonoMod.RuntimeDetour",
		"MonoMod.Utils",
		"MP3Sharp",
		"Newtonsoft.Json",
		"NVorbis",
		"OdeMod",
		"RailSDK.Net",
		"ReLogic",
		"Steamworks.NET",
		"SteelSeriesEngineWrapper",
		"System.CodeDom",
		"System.Composition.AttributedModel",
		"System.Composition.Convention",
		"System.Composition.Hosting",
		"System.Composition.Runtime",
		"System.Composition.TypedParts",
		"System.Configuration.ConfigurationManager",
		"System.Diagnostics.PerformanceCounter",
		"System.Drawing.Common",
		"System.IO.Pipelines",
		"System.Reflection.MetadataLoadContext",
		"System.Security.Cryptography.ProtectedData",
		"System.Security.Permissions",
		"System.Windows.Extensions",
		"TerrariaHooks",
		"tModLoader",
		"tModPorter",
		"UtfUnknown",
	};
}