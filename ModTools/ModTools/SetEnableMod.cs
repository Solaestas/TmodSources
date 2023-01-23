using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ModTools;

public class SetEnableMod : Task
{
	[Required]
	public string BuildingMod { get; set; }

	/// <summary>
	/// 用于调试Mod的工具Mod，如Hero，CheatSheet，用;分开
	/// </summary>
	public string HelpMods { get; set; }

	/// <summary>
	/// 是否禁用其他Mod
	/// </summary>
	public bool DisableOtherMod { get; set; }

	public override bool Execute()
	{
		JArray json = new JArray();
		var path = Path.Combine(Config.ModDirectory, "enabled.json");
		if (File.Exists(path))
		{
			json = JArray.Parse(File.ReadAllText(path));
		}
		using var writer = File.CreateText(path);
		if (DisableOtherMod)
		{
			json = new JArray(new List<string>(HelpMods.Split(';'))
			{
				BuildingMod,
			}.ToArray());
		}
		else
		{
			if (!json.Contains(BuildingMod))
			{
				json.Add(BuildingMod);
			}
			if (HelpMods != null)
			{
				foreach (var mod in HelpMods)
				{
					if (!json.Contains(mod))
					{
						json.Add(mod);
					}
				}
			}
		}

		json.WriteTo(new JsonTextWriter(writer));
		return true;
	}
}