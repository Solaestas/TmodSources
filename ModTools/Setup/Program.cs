using Setup;

var directory = args[0];            //TmodSources文件夹位置

var modLoaderPath = Utils.FindModLoader();
File.WriteAllText(Path.Combine(directory, "Config.props"),
	$"""
	<Project>
	    <PropertyGroup>
	        <tMLPath>{modLoaderPath}</tMLPath>
	    </PropertyGroup>
	</Project>
	"""
);

File.WriteAllText(Path.Combine(directory, "ModTools", "Config.cs"),
	$$"""
	namespace ModTools;

	public static class Config
	{
		public const string ModLoaderPath = "{{modLoaderPath}}"
	}
	"""
);