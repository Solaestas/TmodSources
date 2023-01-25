using CliWrap;

using Terraria;
using Terraria.ModLoader;

namespace UwUMod;
public class UwUMod : Mod
{
}

/// <summary>
/// 测试NuGet包
/// </summary>
class TestSystem : ModPlayer
{
	public override void OnEnterWorld(Player player)
	{
		//测试
		var type = typeof(ModPath);
		Main.NewText("Test");
		(Cli.Wrap(@"powershell") | PipeTarget.ToDelegate(s =>
		{ 
			Main.NewText(s);
		}))
		.ExecuteAsync();

		var test = typeof(Terraria.ModLoader.Core.AssemblyManager.ModLoadContext);


	}
}
