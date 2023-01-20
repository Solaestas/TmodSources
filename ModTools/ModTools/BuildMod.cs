using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ModTools;
public class BuildMod : Task
{
	/// <summary>
	/// 模组源码文件夹
	/// </summary>
	[Required]
	public string ModSourceDirectory { get; set; }
	/// <summary>
	/// 模组工程文件编译输出文件夹
	/// </summary>
	[Required]
	public string ProjectOutputDirectory { get; set; }
	/// <summary>
	/// tMod文件输出文件夹
	/// </summary>
	[Required]
	public string OutputDirectory { get; set; }
	/// <summary>
	/// 模组名，默认为文件夹名 <br/> 模组名应该与ILoadable的命名空间和程序集名称相同
	/// </summary>
	public string ModName { get; set; }
	public override bool Execute()
	{
		ModName ??= Path.GetDirectoryName(ModSourceDirectory);
	


		return true;
	}

}

