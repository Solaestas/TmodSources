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
		var str = ModPath.ModSourcePath;
		//测试
		Main.NewText("Test");
		(Cli.Wrap(@"powershell") | PipeTarget.ToDelegate(s =>
		{ 
			Main.NewText(s);
		}))
		.ExecuteAsync();
		
	}
}
