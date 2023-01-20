using Setup;

var directory = AppDomain.CurrentDomain.BaseDirectory;
directory = directory[..(directory.IndexOf("ModTools") - 1)];

var modLoaderDirectory = Utils.FindModLoaderDirectory();
var modDirectory = Utils.FindModDirectory();
File.WriteAllText(Path.Combine(directory, "Config.props"),
	$"""
	<Project>
	    <PropertyGroup>
	        <tMLDirectory>{modLoaderDirectory}</tMLDirectory>
			<ModDirectory>{modDirectory}</ModDirectory>
	    </PropertyGroup>
	</Project>
	"""
);

File.WriteAllText(Path.Combine(directory, "ModTools", "Config.cs"),
	$$"""
	namespace ModTools;

	public static class Config
	{
		public const string ModLoaderDirectory = @"{{modLoaderDirectory}}";
		public const string ModDirectory = @"{{modDirectory}}";
		public const string BuildIdentifier = @"{{Utils.GetBuildIdentifier(Path.Combine(modLoaderDirectory, "tModLoader.dll"))}}";
	}
	"""
);