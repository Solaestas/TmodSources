using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Setup;

public static class Utils
{
	public static string FindModLoader()
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
		else if(OperatingSystem.IsLinux())
		{ 
			path = Path.Combine("~", ".local", "share", "Steam");
		}
		else
		{
			Console.WriteLine("Unknown OperatingSystem, try enter the tml path");
			path = Console.ReadLine()!;
		}
		path = Path.Combine(path, "steamapps", "common", "tModLoader");

		if(!Directory.Exists(path))
		{
			throw new Exception("tModLoader not found");
		}
		return path;
	}
}