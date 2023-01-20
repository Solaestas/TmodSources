using System;

namespace ModTools.ModLoader;

public static class BuildInfo
{
	public enum BuildPurpose
	{
		Dev, // Personal Builds
		Preview, // Monthly preview builds from CI that modders develop against for compatibility
		Stable // The 'stable' builds from CI that players are expected to play on.
	}

	public static readonly string BuildIdentifier = Config.BuildIdentifier;

#pragma warning disable IDE1006 // 命名样式
	public static readonly Version tMLVersion;
#pragma warning restore IDE1006 // 命名样式
	public static readonly Version StableVersion;
	public static readonly BuildPurpose Purpose;
	public static readonly string BranchName;
	public static readonly string CommitSHA;

	/// <summary>
	/// local time, for display purposes
	/// </summary>
	public static readonly DateTime BuildDate;

	public static bool IsStable => Purpose == BuildPurpose.Stable;
	public static bool IsPreview => Purpose == BuildPurpose.Preview;
	public static bool IsDev => Purpose == BuildPurpose.Dev;

	public static readonly string VersionedName;

	public static readonly string VersionTag;
	public static readonly string VersionedNameDevFriendly;

	static BuildInfo()
	{
		//Current Version
		var parts = BuildIdentifier[(BuildIdentifier.IndexOf('+') + 1)..].Split('|');
		tMLVersion = new Version(parts[0]);
		if (parts.Length >= 2)
		{
			BranchName = parts[1];
		}
		else
		{
			BranchName = "unknown";
		}
		if (parts.Length >= 3)
		{
			Enum.TryParse(parts[2], true, out Purpose);
		}
		if (parts.Length >= 4)
		{
			CommitSHA = parts[3];
		}
		else
		{
			CommitSHA = "unknown";
		}
		if (parts.Length >= 5)
		{
			BuildDate = DateTime.FromBinary(long.Parse(parts[4])).ToLocalTime();
		}

		// Version name for players
		VersionedName = $"tModLoader v{tMLVersion}";

		if (!string.IsNullOrEmpty(BranchName) && BranchName != "unknown"
			&& BranchName != "1.4-stable" && BranchName != "1.4-preview" && BranchName != "1.4")
			VersionedName += $" {BranchName}";

		if (Purpose != BuildPurpose.Stable)
			VersionedName += $" {Purpose}";

		// Version Tag for ???
		VersionTag = VersionedName["tModLoader ".Length..].Replace(' ', '-').ToLower();

		// Version name for modders
		VersionedNameDevFriendly = VersionedName;

		if (CommitSHA != "unknown")
			VersionedNameDevFriendly += $" {CommitSHA.Substring(0, 8)}";

		VersionedNameDevFriendly += $", built {BuildDate:g}";
		#region Newest Version
		//var parts = BuildIdentifier[(BuildIdentifier.IndexOf('+') + 1)..].Split('|');
		//int i = 0;

		//TMLVersion = new Version(parts[i++]);
		//StableVersion = new Version(parts[i++]);
		//BranchName = parts[i++];
		//Enum.TryParse(parts[i++], true, out Purpose);
		//CommitSHA = parts[i++];
		//BuildDate = DateTime.FromBinary(long.Parse(parts[i++])).ToLocalTime();

		//// Version name for players
		//VersionedName = $"tModLoader v{TMLVersion}";

		//if (!string.IsNullOrEmpty(BranchName) && BranchName != "unknown"
		//	&& BranchName != "1.4-stable" && BranchName != "1.4-preview" && BranchName != "1.4")
		//	VersionedName += $" {BranchName}";

		//if (Purpose != BuildPurpose.Stable)
		//	VersionedName += $" {Purpose}";

		//// Version Tag for ???
		//VersionTag = VersionedName["tModLoader ".Length..].Replace(' ', '-').ToLower();

		//// Version name for modders
		//VersionedNameDevFriendly = VersionedName;

		//if (CommitSHA != "unknown")
		//	VersionedNameDevFriendly += $" {CommitSHA.Substring(0, 8)}";

		//VersionedNameDevFriendly += $", built {BuildDate:g}"; 
		#endregion
	}
}