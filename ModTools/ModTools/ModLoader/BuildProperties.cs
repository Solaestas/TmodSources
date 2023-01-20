using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ModTools.ModLoader;

public class BuildProperties
{
	public struct ModReference
	{
		public string Mod;
		public Version Target;

		public ModReference(string mod, Version target)
		{
			Mod = mod;
			Target = target;
		}

		public override string ToString() => Target == null ? Mod : Mod + '@' + Target;

		public static ModReference Parse(string spec)
		{
			var split = spec.Split('@');
			if (split.Length == 1)
			{
				return new ModReference(split[0], null);
			}

			if (split.Length > 2)
			{
				throw new Exception("Invalid mod reference: " + spec);
			}

			try
			{
				return new ModReference(split[0], new Version(split[1]));
			}
			catch
			{
				throw new Exception("Invalid mod reference: " + spec);
			}
		}
	}

	public string[] DllReferences = Array.Empty<string>();
	public ModReference[] ModReferences = Array.Empty<ModReference>();
	public ModReference[] WeakReferences = Array.Empty<ModReference>();

	//this mod will load after any mods in this list
	//sortAfter includes (mod|weak)References that are not in sortBefore
	public string[] SortAfter = Array.Empty<string>();

	//this mod will load before any mods in this list
	public string[] SortBefore = Array.Empty<string>();

	public string[] BuildIgnores = Array.Empty<string>();
	public string Author = "";
	public Version Version = new(1, 0);
	public string DisplayName = "";
	public bool NoCompile = false;
	public bool HideCode = false;
	public bool HideResources = false;
	public bool IncludeSource = false;
	public string EacPath = "";

	// This .tmod was built against a beta release, preventing publishing.
	public bool Beta = false;

	public Version BuildVersion = BuildInfo.tMLVersion;
	public string Homepage = "";
	public string Description = "";
	public ModSide Side;
	public bool PlayableOnPreview = true;

	public IEnumerable<ModReference> Refs(bool includeWeak) =>
		includeWeak ? ModReferences.Concat(WeakReferences) : ModReferences;

	public IEnumerable<string> RefNames(bool includeWeak) => Refs(includeWeak).Select(dep => dep.Mod);

	private static IEnumerable<string> ReadList(string value)
		=> value.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0);

	private static IEnumerable<string> ReadList(BinaryReader reader)
	{
		var list = new List<string>();
		for (string item = reader.ReadString(); item.Length > 0; item = reader.ReadString())
		{
			list.Add(item);
		}

		return list;
	}

	private static void WriteList<T>(IEnumerable<T> list, BinaryWriter writer)
	{
		foreach (var item in list)
		{
			writer.Write(item.ToString());
		}

		writer.Write("");
	}

	public static BuildProperties ReadBuildFile(string modDir)
	{
		string propertiesFile = Path.Combine(modDir, "build.txt");
		string descriptionfile = Path.Combine(modDir, "description.txt");
		var properties = new BuildProperties();
		if (!File.Exists(propertiesFile))
		{
			return properties;
		}
		if (File.Exists(descriptionfile))
		{
			properties.Description = File.ReadAllText(descriptionfile);
		}
		foreach (string line in File.ReadAllLines(propertiesFile))
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				continue;
			}
			int split = line.IndexOf('=');
			string property = line[..split].Trim();
			string value = line[(split + 1)..].Trim();
			if (value.Length == 0)
			{
				continue;
			}
			switch (property)
			{
				case "dllReferences":
					properties.DllReferences = ReadList(value).ToArray();
					break;

				case "modReferences":
					properties.ModReferences = ReadList(value).Select(ModReference.Parse).ToArray();
					break;

				case "weakReferences":
					properties.WeakReferences = ReadList(value).Select(ModReference.Parse).ToArray();
					break;

				case "sortBefore":
					properties.SortBefore = ReadList(value).ToArray();
					break;

				case "sortAfter":
					properties.SortAfter = ReadList(value).ToArray();
					break;

				case "author":
					properties.Author = value;
					break;

				case "version":
					properties.Version = new Version(value);
					break;

				case "displayName":
					properties.DisplayName = value;
					break;

				case "homepage":
					properties.Homepage = value;
					break;

				case "noCompile":
					properties.NoCompile = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
					break;

				case "playableOnPreview":
					properties.PlayableOnPreview = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
					break;

				case "hideCode":
					properties.HideCode = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
					break;

				case "hideResources":
					properties.HideResources = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
					break;

				case "includeSource":
					properties.IncludeSource = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
					break;

				case "buildIgnore":
					properties.BuildIgnores = value.Split(',').Select(s => s.Trim().Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar)).Where(s => s.Length > 0).ToArray();
					break;

				case "side":
					if (!Enum.TryParse(value, true, out properties.Side))
					{
						throw new Exception("side is not one of (Both, Client, Server, NoSync): " + value);
					}

					break;
			}
		}

		var refs = properties.RefNames(true).ToList();
		if (refs.Count != refs.Distinct().Count())
		{
			throw new Exception("Duplicate mod/weak reference");
		}

		//add (mod|weak)References that are not in sortBefore to sortAfter
		properties.SortAfter = properties.RefNames(true).Where(dep => !properties.SortBefore.Contains(dep))
			.Concat(properties.SortAfter).Distinct().ToArray();

		return properties;
	}

	public byte[] ToBytes()
	{
		byte[] data;
		using (var memoryStream = new MemoryStream())
		{
			using (var writer = new BinaryWriter(memoryStream))
			{
				if (DllReferences.Length > 0)
				{
					writer.Write("dllReferences");
					WriteList(DllReferences, writer);
				}
				if (ModReferences.Length > 0)
				{
					writer.Write("modReferences");
					WriteList(ModReferences, writer);
				}
				if (WeakReferences.Length > 0)
				{
					writer.Write("weakReferences");
					WriteList(WeakReferences, writer);
				}
				if (SortAfter.Length > 0)
				{
					writer.Write("sortAfter");
					WriteList(SortAfter, writer);
				}
				if (SortBefore.Length > 0)
				{
					writer.Write("sortBefore");
					WriteList(SortBefore, writer);
				}
				if (Author.Length > 0)
				{
					writer.Write("author");
					writer.Write(Author);
				}
				writer.Write("version");
				writer.Write(Version.ToString());
				if (DisplayName.Length > 0)
				{
					writer.Write("displayName");
					writer.Write(DisplayName);
				}
				if (Homepage.Length > 0)
				{
					writer.Write("homepage");
					writer.Write(Homepage);
				}
				if (Description.Length > 0)
				{
					writer.Write("description");
					writer.Write(Description);
				}
				if (NoCompile)
				{
					writer.Write("noCompile");
				}
				if (!PlayableOnPreview)
				{
					writer.Write("!playableOnPreview");
				}
				if (!HideCode)
				{
					writer.Write("!hideCode");
				}
				if (!HideResources)
				{
					writer.Write("!hideResources");
				}
				if (IncludeSource)
				{
					writer.Write("includeSource");
				}
				if (EacPath.Length > 0)
				{
					writer.Write("eacPath");
					writer.Write(EacPath);
				}
				if (Side != ModSide.Both)
				{
					writer.Write("side");
					writer.Write((byte)Side);
				}

				writer.Write("buildVersion");
				writer.Write(BuildVersion.ToString());

				writer.Write("");
			}
			data = memoryStream.ToArray();
		}
		return data;
	}

	public static BuildProperties ReadFromStream(Stream stream)
	{
		var properties = new BuildProperties
		{
			// While the intended defaults for these are false, Info will only have !hideCode and
			// !hideResources entries, so this is necessary.
			HideCode = true,
			HideResources = true
		};
		using (var reader = new BinaryReader(stream))
		{
			for (string tag = reader.ReadString(); tag.Length > 0; tag = reader.ReadString())
			{
				if (tag == "dllReferences")
				{
					properties.DllReferences = ReadList(reader).ToArray();
				}
				if (tag == "modReferences")
				{
					properties.ModReferences = ReadList(reader).Select(ModReference.Parse).ToArray();
				}
				if (tag == "weakReferences")
				{
					properties.WeakReferences = ReadList(reader).Select(ModReference.Parse).ToArray();
				}
				if (tag == "sortAfter")
				{
					properties.SortAfter = ReadList(reader).ToArray();
				}
				if (tag == "sortBefore")
				{
					properties.SortBefore = ReadList(reader).ToArray();
				}
				if (tag == "author")
				{
					properties.Author = reader.ReadString();
				}
				if (tag == "version")
				{
					properties.Version = new Version(reader.ReadString());
				}
				if (tag == "displayName")
				{
					properties.DisplayName = reader.ReadString();
				}
				if (tag == "homepage")
				{
					properties.Homepage = reader.ReadString();
				}
				if (tag == "description")
				{
					properties.Description = reader.ReadString();
				}
				if (tag == "noCompile")
				{
					properties.NoCompile = true;
				}
				if (tag == "!playableOnPreview")
				{
					properties.PlayableOnPreview = false;
				}
				if (tag == "!hideCode")
				{
					properties.HideCode = false;
				}
				if (tag == "!hideResources")
				{
					properties.HideResources = false;
				}
				if (tag == "includeSource")
				{
					properties.IncludeSource = true;
				}
				if (tag == "eacPath")
				{
					properties.EacPath = reader.ReadString();
				}
				if (tag == "side")
				{
					properties.Side = (ModSide)reader.ReadByte();
				}
				if (tag == "buildVersion")
				{
					properties.BuildVersion = new Version(reader.ReadString());
				}
			}
		}
		return properties;
	}

	public static void InfoToBuildTxt(Stream src, Stream dst)
	{
		BuildProperties properties = ReadFromStream(src);
		var sb = new StringBuilder();
		if (properties.DisplayName.Length > 0)
		{
			sb.AppendLine($"displayName = {properties.DisplayName}");
		}

		if (properties.Author.Length > 0)
		{
			sb.AppendLine($"author = {properties.Author}");
		}

		sb.AppendLine($"version = {properties.Version}");
		if (properties.Homepage.Length > 0)
		{
			sb.AppendLine($"homepage = {properties.Homepage}");
		}

		if (properties.DllReferences.Length > 0)
		{
			sb.AppendLine($"dllReferences = {string.Join(", ", properties.DllReferences)}");
		}

		if (properties.ModReferences.Length > 0)
		{
			sb.AppendLine($"modReferences = {string.Join(", ", properties.ModReferences)}");
		}

		if (properties.WeakReferences.Length > 0)
		{
			sb.AppendLine($"weakReferences = {string.Join(", ", properties.WeakReferences)}");
		}

		if (properties.NoCompile)
		{
			sb.AppendLine($"noCompile = true");
		}

		if (properties.HideCode)
		{
			sb.AppendLine($"hideCode = true");
		}

		if (properties.HideResources)
		{
			sb.AppendLine($"hideResources = true");
		}

		if (properties.IncludeSource)
		{
			sb.AppendLine($"includeSource = true");
		}

		if (!properties.PlayableOnPreview)
		{
			sb.AppendLine($"playableOnPreview = false");
		}
		// buildIgnores isn't preserved in Info, but it doesn't matter with extraction since the
		// ignored files won't be present anyway. if (properties.buildIgnores.Length > 0)
		// sb.AppendLine($"buildIgnores = {string.Join(", ", properties.buildIgnores)}");
		if (properties.Side != ModSide.Both)
		{
			sb.AppendLine($"side = {properties.Side}");
		}

		if (properties.SortAfter.Length > 0)
		{
			sb.AppendLine($"sortAfter = {string.Join(", ", properties.SortAfter)}");
		}

		if (properties.SortBefore.Length > 0)
		{
			sb.AppendLine($"sortBefore = {string.Join(", ", properties.SortBefore)}");
		}

		var bytes = Encoding.UTF8.GetBytes(sb.ToString());
		dst.Write(bytes, 0, bytes.Length);
	}

	public bool IgnoreFile(string resource) => BuildIgnores.Any(fileMask => FitsMask(resource, fileMask));

	private bool FitsMask(string fileName, string fileMask)
	{
		string pattern =
			'^' +
			Regex.Escape(fileMask.Replace(".", "__DOT__")
							 .Replace("*", "__STAR__")
							 .Replace("?", "__QM__"))
				 .Replace("__DOT__", "[.]")
				 .Replace("__STAR__", ".*")
				 .Replace("__QM__", ".")
			+ '$';
		return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(fileName);
	}
}