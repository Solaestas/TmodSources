using System;
using System.Collections.Generic;
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

    /// <summary>
    /// 是否使用BuildIgnore来决定是否包含资源
    /// </summary>
    public bool IgnoreBuildFile { get; set; }

    public override bool Execute()
    {
        bool success = true;
        ModName ??= Path.GetFileName(Path.GetDirectoryName(ModSourceDirectory));
        Log.LogMessage(MessageImportance.High, "Building Mod...");
        Log.LogMessage(MessageImportance.High, $"Building {ModName} -> {Path.Combine(ModDirectory, $"{ModName}.tmod")}");

        var property = BuildProperties.ReadBuildFile(ModSourceDirectory);
        var tmod = new TmodFile(Path.Combine(ModDirectory, $"{ModName}.tmod"), ModName, property.Version);

        var assetDirectory = Path.Combine(OutputDirectory, "Assets") + Path.DirectorySeparatorChar;
        Parallel.ForEach(AssetFiles, file =>
        {
            if (!File.Exists(file.ItemSpec))
            {
                Log.LogError("File {file} not exist!", file.ItemSpec);
                success = false;
                return;
            }
            tmod.AddFile(file.ItemSpec.Replace(assetDirectory, ""), File.ReadAllBytes(file.ItemSpec));
        });

        if (IgnoreBuildFile)
        {
            Log.LogMessage(MessageImportance.High, "Ignore Build File");
            Parallel.ForEach(ResourceFiles, file =>
            {
                if (!File.Exists(file.ItemSpec))
                {
                    Log.LogError("File {file} not exist!", file.ItemSpec);
                    success = false;
                    return;
                }
                tmod.AddFile(file.GetMetadata("Identity"), File.ReadAllBytes(file.ItemSpec));
            });
        }
        else
        {
            Parallel.ForEach(Directory.GetFiles(ModSourceDirectory, "*", SearchOption.AllDirectories)
                .Select(s => (Identity: s.Replace(ModSourceDirectory, ""), FullPath: s))
                .Where(s => !property.IgnoreFile(s.Identity) && !IgnoreFile(s.Identity))
                , file =>
                {
                    tmod.AddFile(file.Identity, File.ReadAllBytes(file.FullPath));
                });
        }

        //Add dll and pdb
        var set = new HashSet<string>(property.DllReferences);
        var modref = new HashSet<string>(property.ModReferences.Select(mod => mod.Mod));
        Parallel.ForEach(Directory.GetFiles(OutputDirectory, "*", SearchOption.TopDirectoryOnly)
            .Where(s => !DefaultDependency.Contains(Path.GetFileNameWithoutExtension(s)))
            .Where(s => !modref.Contains(Path.GetFileNameWithoutExtension(s)))
            , file =>
        {
            var ext = Path.GetExtension(file);
            var name = Path.GetFileNameWithoutExtension(file);

            if (name == ModName)
            {
                if (ext is ".dll")
                {
                    tmod.AddFile(name + ext, File.ReadAllBytes(file));
                }
                else if (ext is ".pdb")
                {
                    tmod.AddFile(name + ext, File.ReadAllBytes(file));
                    property.EacPath = file;
                }
                return;
            }

            if (ext == ".dll")
            {
                tmod.AddFile($"lib/{name}.dll", File.ReadAllBytes(file));
                if (!set.Contains(name))
                {
                    set.Add(name);
                }
            }
        });
        property.DllReferences = set.ToArray();
        tmod.AddFile("Info", property.ToBytes());
        tmod.Save();
        return success;
    }

    public bool IgnoreFile(string path)
    {
        var ext = Path.GetExtension(path);

        if (path[0] == '.')
        {
            return true;
        }

        if (path[0] == '.' || path.StartsWith("bin", StringComparison.Ordinal) || path.StartsWith("obj", StringComparison.Ordinal) || path.StartsWith("Properties", StringComparison.Ordinal))
        {
            return true;
        }

        if (path == "icon.png")
        {
            return false;
        }
        // Include in following ignore
        //if(path == "build.txt" || path == "description.txt" || path == "description_workshuo.txt")
        //{
        //	return false;
        //}

        if (ext is ".png" or ".cs" or ".md" or ".txt" or ".cache" or ".fx")
        {
            return true;
        }

        return false;
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