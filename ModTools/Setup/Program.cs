using System.Xml;
using Setup;

var directory = AppDomain.CurrentDomain.BaseDirectory;
directory = directory[..(directory.IndexOf("ModTools") - 1)];
string modLoaderDirectory;
try
{
	modLoaderDirectory = Utils.FindModLoaderDirectory();
}
catch (Exception ex)
{
	Console.WriteLine(ex.Message);
	Console.WriteLine("Please enter tmodloader directory : ");
	modLoaderDirectory = Utils.EnsureDirectory(Console.ReadLine()!);
	if (!Directory.Exists(modLoaderDirectory) || !File.Exists(Path.Combine(modLoaderDirectory, "tMLMod.targets")))
	{
		throw new ArgumentException("Invalid Directory");
	}
}
var modDirectory = Utils.FindModDirectory();

var constants = "$(DefineConstants)";
try
{
	Console.WriteLine("Try read Constants");
	XmlDocument doc = new XmlDocument();
	using (var reader = XmlReader.Create(Path.Combine(modLoaderDirectory, "tMLMod.targets")))
	{
		doc.Load(reader);
		XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
		nsmgr.AddNamespace("cs", doc.FirstChild!.NamespaceURI);
		var node = doc.SelectSingleNode("//cs:DefineConstants", nsmgr) ?? throw new Exception("tML怎么改格式了");
		constants = node.InnerText;
	}
	Console.WriteLine("Reading Constants succeed!");
}
catch
{
	Console.WriteLine("Reading Constansts Failed, ignore and continue");
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
		public const string ThisProjectDirectory = @"{{directory}}";
		public const string ModLoaderDirectory = @"{{modLoaderDirectory}}";
		public const string ModDirectory = @"{{modDirectory}}";
		public const string BuildIdentifier = @"{{Utils.GetBuildIdentifier(Path.Combine(modLoaderDirectory, "tModLoader.dll"))}}";
	}
	"""
);