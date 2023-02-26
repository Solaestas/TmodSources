using System.Diagnostics;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.OS;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace UwUMod;

internal class CustomSourceItem : UIModSourceItem
{
	private string _modFolder;
	private string _csproj;
	private new string _modName;
	private int _contextButtonsLeft = -26;

	public CustomSourceItem(string modFolder, string modName, string csproj, LocalMod builtMod)
		: base(modFolder, builtMod)
	{
		Elements.Clear();
		_modFolder = modFolder;
		_modName = modName;
		_csproj = csproj;

		BackgroundColor = Color.MediumPurple * 0.7f;
		Height.Pixels = 90;
		Width.Percent = 1f;
		SetPadding(6f);

		var _modNameUI = new UIText(modName)
		{
			Left = { Pixels = 10 },
			Top = { Pixels = 5 },
		};
		Append(_modNameUI);

		var buildButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuild"))
		{
			Width = { Pixels = 100 },
			Height = { Pixels = 36 },
			Left = { Pixels = 10 },
			Top = { Pixels = 40 },
		}.WithFadedMouseOver();
		buildButton.PaddingTop -= 2f;
		buildButton.PaddingBottom -= 2f;
		buildButton.OnClick += BuildMod;
		Append(buildButton);

		var buildReloadButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuildReload"));
		buildReloadButton.CopyStyle(buildButton);
		buildReloadButton.Width.Pixels = 200;
		buildReloadButton.Left.Pixels = 150;
		buildReloadButton.WithFadedMouseOver();
		buildReloadButton.OnClick += BuildAndReload;
		Append(buildReloadButton);

		if (builtMod != null)
		{
			var publishButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSPublish"));
			publishButton.CopyStyle(buildReloadButton);
			publishButton.Width.Pixels = 100;
			publishButton.Left.Pixels = 390;
			publishButton.WithFadedMouseOver();

			if (builtMod.properties.side == ModSide.Server)
			{
				publishButton.OnClick += PublishServerSideMod;
				Append(publishButton);
			}
			else if (builtMod.Enabled)
			{
				publishButton.OnClick += PublishMod;
				Append(publishButton);
			}
		}

		OnDoubleClick += BuildAndReload;

		var openCSProjButton = new UIHoverImage(UICommon.CopyCodeButtonTexture, "Open .csproj")
		{
			Left = { Pixels = _contextButtonsLeft, Percent = 1f },
			Top = { Pixels = 4 },
		};
		openCSProjButton.OnClick += (a, b) =>
		{
			Process.Start(
				new ProcessStartInfo(_csproj)
				{
					UseShellExecute = true,
				});
		};
		Append(openCSProjButton);

		_contextButtonsLeft -= 26;
	}

	public override void DrawSelf(SpriteBatch spriteBatch)
	{
		var temp = _upgradePotentialChecked;
		_upgradePotentialChecked = true;
		base.DrawSelf(spriteBatch);
		_upgradePotentialChecked = temp;

		// This code here rather than ctor since the delay for dozens of mod source folders is noticable.
		if (_upgradePotentialChecked)
		{
			return;
		}

		_upgradePotentialChecked = true;

		// Display Run tModPorter for Windows
		if (Platform.IsWindows)
		{
			var pIcon = UICommon.ButtonExclamationTexture;
			var portModButton = new UIHoverImage(pIcon, Language.GetTextValue("tModLoader.MSPortToLatest"))
			{
				Left = { Pixels = _contextButtonsLeft, Percent = 1f },
				Top = { Pixels = 4 },
			};

			portModButton.OnClick += (s, e) =>
			{
				string modFolderName = Path.GetFileName(_modFolder);
				string csprojFile = Path.Combine(_modFolder, $"{modFolderName}.csproj");

				string args = $"\"{csprojFile}\"";
				var tMLPath = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
				var porterPath = Path.Combine(Path.GetDirectoryName(tMLPath)!, "tModPorter", "tModPorter.bat");

				var porterInfo = new ProcessStartInfo()
				{
					FileName = porterPath,
					Arguments = args,
					UseShellExecute = true,
				};

				var porter = Process.Start(porterInfo);
			};

			Append(portModButton);

			_contextButtonsLeft -= 26;
		}
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
		BackgroundColor = Color.MediumPurple;
		BorderColor = new Color(89, 116, 213);
	}

	public override void MouseOut(UIMouseEvent evt)
	{
		base.MouseOut(evt);
		BackgroundColor = Color.MediumPurple * 0.7f;
		BorderColor = new Color(89, 116, 213) * 0.7f;
	}

	public override int CompareTo(object obj)
	{
		if (obj is not CustomSourceItem uIModSourceItem)
		{
			return -1;
		}
		if (uIModSourceItem._builtMod == null && _builtMod == null)
		{
			return _modName.CompareTo(uIModSourceItem._modName);
		}

		if (uIModSourceItem._builtMod == null)
		{
			return -1;
		}

		if (_builtMod == null)
		{
			return 1;
		}

		return uIModSourceItem._builtMod.lastModified.CompareTo(_builtMod.lastModified);
	}
}