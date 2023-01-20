// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using Microsoft.Win32;

if (OperatingSystem.IsWindows())
{
    static RegistryKey OpenRegistryKey(string path)
    {
        var keys = path.Split('\\');
        var rkey = keys[0] switch
        {
            "HKLM:" => Registry.LocalMachine,
            "HKCU:" => Registry.CurrentUser,
            _ => throw new NotImplementedException()
        };
        foreach (var key in keys[1..])
        {
            rkey = rkey.OpenSubKey(key);
            Debug.Assert(rkey != null);
        }
        return rkey;
    }
    List<string> UninstallPaths = new()
    {
        "HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall",
        "HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall"
    };

    if (Environment.Is64BitOperatingSystem)
    {
        UninstallPaths.Add("HKLM:\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
    }

    foreach (var path in UninstallPaths)
    {
        var key = OpenRegistryKey(path);
        foreach (var sub in key.GetSubKeyNames().ToArray().Select(name =>
        {
            Debug.Assert(OperatingSystem.IsWindows());
            return key.OpenSubKey(name);
        }))
        {
            if ((string?)sub?.GetValue("DisplayName") == "Steam")
            {
                Console.WriteLine(
                    Path.Combine(
                        Path.GetDirectoryName((string?)sub.GetValue("DisplayIcon")) ?? throw new Exception("Bad"),
                        "steamapps",
                        "common",
                        "tModLoader")
                    );
            }
        }
    }
}
else
{
    throw new NotImplementedException("摸了");
}