using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Setup;

public static partial class Utils
{
	public const int TerrariaAppId = 105600;

	public readonly static string TerrariaManifestFile = $"appmanifest_{TerrariaAppId}.acf";

	private readonly static Regex SteamLibraryFoldersRegex = GetSteamLibraryFoldersRegex();
	private readonly static Regex SteamManifestInstallDirRegex = GetSteamManifestInstallDirRegex();
	public static string EnsureDirectory(string path)
	{
		return path switch
		{
			[_, '\\'] or [_, '/'] => path,
			_ => path + Path.DirectorySeparatorChar
		};
	}
	public static string FindModLoaderDirectory()
	{
		string steamDirectory;
		if (OperatingSystem.IsWindows())
		{
			using var key = Registry.LocalMachine.CreateSubKey(
				Environment.Is64BitOperatingSystem ?
				@"SOFTWARE\Wow6432Node\Valve\Steam" :
				@"SOFTWARE\Valve\Steam");
			steamDirectory = (string)key.GetValue("InstallPath")!;
		}
		else if (OperatingSystem.IsMacOS())
		{
			steamDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "Steam");
		}
		else if (OperatingSystem.IsLinux())
		{
			steamDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "Steam");
		}
		else
		{
			Console.WriteLine("Unknown OperatingSystem, try enter steam path");
			steamDirectory = Console.ReadLine()!;
		}
		if (!Directory.Exists(steamDirectory))
		{
			throw new ArgumentException("Invalid Steam Path");
		}

		Console.WriteLine($"Find steam in {steamDirectory}");
		string steamApps = Path.Combine(steamDirectory, "steamapps");

		var libraries = new List<string>() {
				steamApps
			};

		string libraryFoldersFile = Path.Combine(steamApps, "libraryfolders.vdf");

		if (File.Exists(libraryFoldersFile))
		{
			string contents = File.ReadAllText(libraryFoldersFile);

			var matches = SteamLibraryFoldersRegex.Matches(contents);
			libraries.AddRange(from Match match in matches
							   let directory = Path.Combine(match.Groups[2].Value.Replace(@"\\", @"\"), "steamapps")
							   where Directory.Exists(directory)
							   select directory);
		}

		string trPath = string.Empty;
		for (int i = 0; i < libraries.Count; i++)
		{
			string directory = libraries[i];
			string manifestPath = Path.Combine(directory, TerrariaManifestFile);

			if (File.Exists(manifestPath))
			{
				string contents = File.ReadAllText(manifestPath);
				var match = SteamManifestInstallDirRegex.Match(contents);

				if (match.Success)
				{
					trPath = Path.Combine(directory, "common", match.Groups[1].Value);

					if (!Directory.Exists(trPath))
					{
						throw new ArgumentException("can't find terraria");
					}
				}
			}
		}

		if (trPath == string.Empty)
		{
			throw new ArgumentException("Can't find terraria");
		}
		Console.WriteLine($"Find terraria in {trPath}");
		return Path.GetFullPath(Path.Combine(trPath, "..", "tModLoader")) + Path.DirectorySeparatorChar;
	}

	public static string FindModDirectory()
	{
		string path;
		if (OperatingSystem.IsWindows())
		{
			path = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				"My Games",
				"Terraria",
				"tModLoader-preview",
				"Mods") + Path.DirectorySeparatorChar;
		}
		else if (OperatingSystem.IsLinux())
		{
			path = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
				".local",
				"share",
				"Terraria",
				"tModLoader-preview",
				"Mods") + Path.DirectorySeparatorChar;
		}
		else
		{
			Console.WriteLine("Unknown operating system, please enter the mod folder path :");
			path = Console.ReadLine()!;
			if (!Directory.Exists(path))
			{
				throw new ArgumentException("Invalid Directory");
			}
		}

		if (!Directory.Exists(path))
		{
			Console.WriteLine("Mod Folder not found, try enter the tml path");
			path = Console.ReadLine()!;
			if (!Directory.Exists(path))
			{
				throw new Exception("Not Found");
			}
		}
		return path;
	}

	public static string GetBuildIdentifier(string tmlPath)
	{
		// Get the array of runtime assemblies.
		string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

		// Create the list of assembly paths consisting of runtime assemblies and the inspected assembly.
		var paths = new List<string>(runtimeAssemblies)
		{
			tmlPath
		};

		// Create PathAssemblyResolver that can resolve assemblies using the created list.
		var resolver = new PathAssemblyResolver(paths);
		using var mlc = new MetadataLoadContext(resolver);
		var asm = mlc.LoadFromAssemblyName("tModLoader");
		return (string)asm!.GetCustomAttributesData()
			.First(c => c.AttributeType.Name == nameof(AssemblyInformationalVersionAttribute))
			.ConstructorArguments[0].Value!;
	}

	[GeneratedRegex("\"installdir\"[^\\S\\r\\n]+\"([^\\r\\n]+)\"", RegexOptions.Compiled)]
	private static partial Regex GetSteamManifestInstallDirRegex();
	[GeneratedRegex("\"(\\d+)\"[^\\S\\r\\n]+\"(.+)\"", RegexOptions.Compiled)]
	private static partial Regex GetSteamLibraryFoldersRegex();
}