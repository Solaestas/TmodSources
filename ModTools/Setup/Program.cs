using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using Setup;

var directory = AppDomain.CurrentDomain.BaseDirectory;
directory = directory[..(directory.IndexOf("ModTools") - 1)];

var modLoaderDirectory = Utils.FindModLoaderDirectory();
var modDirectory = Utils.FindModDirectory();
#if DEV
modLoaderDirectory = Path.GetFullPath(Path.Combine(modLoaderDirectory, "..", "tModLoaderDev")) + Path.DirectorySeparatorChar;
modDirectory = Path.GetFullPath(Path.Combine(modDirectory, "..", "..", "tModLoader-dev", "Mods")) + Path.DirectorySeparatorChar;
#endif

XmlDocument doc = new XmlDocument();
var constants = "$(DefineConstants)";
using (var reader = XmlReader.Create(Path.Combine(modLoaderDirectory, "tMLMod.targets")))
{
	doc.Load(reader);
	XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
	nsmgr.AddNamespace("cs", doc.FirstChild!.NamespaceURI);
	var node = doc.SelectSingleNode("//cs:DefineConstants", nsmgr) ?? throw new Exception("tML怎么改格式了");
	constants = node.InnerText;
}

File.WriteAllText(Path.Combine(directory, "Config.props"),
	$"""
	<Project>
	    <PropertyGroup>
	        <tMLDirectory>{modLoaderDirectory}</tMLDirectory>
			<ModDirectory>{modDirectory}</ModDirectory>
			<DefineConstants>{constants}</DefineConstants>
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