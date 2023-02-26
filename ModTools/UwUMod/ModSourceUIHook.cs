using System.Diagnostics;
using System.Text;
using System.Xml;
using ModTools;
using MonoMod.RuntimeDetour;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;

namespace UwUMod;

public class ModSourceUIHook : ILoadable
{
	private List<IDisposable> _hooks = new();

	public void Load(Mod mod)
	{
		MonoModHooks.RequestNativeAccess();
		var method = typeof(UIModSources).GetMethod(
			nameof(UIModSources.Populate),
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
		_hooks.Add(new Hook(method, (Action<UIModSources> orig, UIModSources self) =>
		{
			var modSources = Directory.GetDirectories(Path.Combine(Config.ThisProjectDirectory, "ModSources"));
			foreach (var sourcePath in modSources)
			{
				try
				{
					var csproj = Directory.GetFiles(sourcePath, "*.csproj", SearchOption.TopDirectoryOnly).First();
					var xml = new XmlDocument();
					xml.Load(csproj);
					var modName = xml.SelectSingleNode("//AssemblyName")!.InnerText;
					LocalMod? builtMod = null;
					if (File.Exists(Path.Combine(Config.ModDirectory, $"{modName}.tmod")))
					{
						var file = new TmodFile(Path.Combine(Config.ModDirectory, $"{modName}.tmod"));
						using (file.Open())
						{
							builtMod = new LocalMod(file);
						}
					}
					self._items.Add(new CustomSourceItem(sourcePath, modName, csproj, builtMod!));
				}
				catch
				{
					continue;
				}
			}

			orig(self);
		}));

		method = typeof(ModCompile).GetMethod(
			nameof(ModCompile.Build),
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
			new Type[] { typeof(ModCompile.BuildingMod) })!;
		_hooks.Add(new Hook(method, (Action<ModCompile, ModCompile.BuildingMod> orig, ModCompile self, ModCompile.BuildingMod mod) =>
		{
			if (!Path.GetFullPath(mod.path).StartsWith(Config.ThisProjectDirectory, StringComparison.OrdinalIgnoreCase))
			{
				orig(self, mod);
				return;
			}

			try
			{
				self.status.SetStatus(Language.GetTextValue("tModLoader.Building", mod.Name));
				var proc = new Process()
				{
					StartInfo = new ProcessStartInfo("dotnet/6.0.0/dotnet")
					{
						Arguments = $"build {mod.path}",
						CreateNoWindow = true,
						UseShellExecute = false,
						RedirectStandardError = true,
						RedirectStandardOutput = true,
						StandardOutputEncoding = Encoding.UTF8,
						StandardErrorEncoding = Encoding.UTF8,
					},
				};
				var sb = new StringBuilder();
				proc.Start();
				var output = proc.StandardOutput;
				var error = proc.StandardError;
				Task.WaitAll(
					proc.WaitForExitAsync(),
					Task.Run(() =>
					{
						do
						{
							var msg = output.ReadLine() ?? string.Empty;
							if (msg.Contains("): warning"))
							{
								continue;
							}
							var index = msg.IndexOf("->");
							if (index >= 0)
							{
								msg = msg[..index];
							}
							self.status.SetStatus(msg);
						}
						while (!output.EndOfStream || !proc.HasExited);
					}),
					Task.Run(() =>
					{
						do
						{
							string? msg = error.ReadLine();
							sb.AppendLine(msg);
						}
						while (!error.EndOfStream || !proc.HasExited);
					}));

				if (sb.Length > 2)
				{
					throw new AggregateException($"Compile Error\n{sb}");
				}
				ModLoader.EnableMod(mod.Name);
			}
			catch (Exception e)
			{
				e.Data["mod"] = mod.Name;
				throw;
			}
		}));
	}

	public void Unload()
	{
		foreach (var hook in _hooks)
		{
			hook.Dispose();
		}
	}
}