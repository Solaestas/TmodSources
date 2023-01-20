using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Setup;

public static class Utils
{
	public static string FindModLoaderDirectory()
	{
		string path;
		if (OperatingSystem.IsWindows())
		{
			using var key = Registry.LocalMachine.CreateSubKey(
				Environment.Is64BitOperatingSystem ?
				@"SOFTWARE\Wow6432Node\Valve\Steam" :
				@"SOFTWARE\Valve\Steam");
			path = (string)key.GetValue("InstallPath")!;
		}
		else if (OperatingSystem.IsMacOS())
		{
			path = Path.Combine("~", "Library", "Application Support", "Steam");
		}
		else if (OperatingSystem.IsLinux())
		{
			path = Path.Combine("~", ".local", "share", "Steam");
		}
		else
		{
			Console.WriteLine("Unknown OperatingSystem, try enter the tml path");
			path = Console.ReadLine()!;
		}
		path = Path.Combine(path, "steamapps", "common", "tModLoader");

		if (!Directory.Exists(path))
		{
			Console.WriteLine("tModLoader not found, try enter the tml path");
			path = Console.ReadLine()!;
			if (!Directory.Exists(path))
			{
				throw new Exception("Not Found");
			}
		}
		return path + Path.DirectorySeparatorChar;
	}

	public static string FindModDirectory()
	{
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Terraria", "tModLoader", "Mods") + Path.DirectorySeparatorChar;
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
		var paths = new List<string>(runtimeAssemblies);
		paths.Add(tmlPath);

		// Create PathAssemblyResolver that can resolve assemblies using the created list.
		var resolver = new PathAssemblyResolver(paths);
		using var mlc = new MetadataLoadContext(resolver);
		var asm = mlc.LoadFromAssemblyName("tModLoader");
		return (string)asm!.GetCustomAttributesData()
			.First(c => c.AttributeType.Name == nameof(AssemblyInformationalVersionAttribute))
			.ConstructorArguments[0].Value!;
	}
}