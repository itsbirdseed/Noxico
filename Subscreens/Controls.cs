﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

namespace Noxico
{
	public static class Controls
	{
		private static UIList controlList;
		private static UIButton saveButton;
		private static UIButton resetButton;
		private static bool waitingForKey;
		private static int numControls;

		private static void UpdateItems()
		{
			for (var i = 0; i < numControls; i++)
			{
				controlList.Items[i] = i18n.GetString("key_" + Enum.GetName(typeof(KeyBinding), i)).PadEffective(16) + Toolkit.TranslateKey((KeyBinding)i, true, true);
			}
		}

		public static void Handler()
		{
			if (Subscreens.FirstDraw)
			{
				Subscreens.FirstDraw = false;
				UIManager.Initialize();
				UIManager.Elements.Clear();

				NoxicoGame.Immediate = true;
				NoxicoGame.Me.CurrentBoard.Redraw();
				NoxicoGame.Me.CurrentBoard.Draw(true);

				var items = Enum.GetNames(typeof(KeyBinding));
				numControls = items.Length;
				var numShown = numControls > 17 ? 17 : numControls;
				controlList = new UIList(string.Empty, null, items)
				{
					Left = 6,
					Top = 3,
					Width = 40,
					Height = numShown
				};
				controlList.Enter = (s, e) =>
				{
					waitingForKey = true;
					controlList.Items[controlList.Index] = i18n.GetString("key_" + Enum.GetName(typeof(KeyBinding), controlList.Index)).PadEffective(16) + "........";
					controlList.DrawQuick();
				};

				saveButton = new UIButton(i18n.GetString("key_Save"), (s, e) =>
				{
					//TODO: set values
					IniFile.Save("noxico.ini");
					Options.Open();
				})
				{
					Left = 6,
					Top = 4 + numShown,
					Width = 12
				};

				resetButton = new UIButton(i18n.GetString("key_Reset"), (s, e) =>
				{
					NoxicoGame.ResetKeymap();
					UpdateItems();
					UIManager.Highlight = controlList;
					UIManager.Draw();
				})
				{
					Left = 20,
					Top = 4 + numShown,
					Width = 12
				};

				var window = new UIWindow(i18n.GetString("key_Title"))
				{
					Left = 4,
					Top = 1,
					Width = 44,
					Height = numShown + 6
				};

				UpdateItems();
				UIManager.Elements.Add(window);
				UIManager.Elements.Add(controlList);
				UIManager.Elements.Add(saveButton);
				UIManager.Elements.Add(resetButton);

				Subscreens.Redraw = true;
			}

			if (Subscreens.Redraw)
			{
				Subscreens.Redraw = false;
				UIManager.Draw();
			}

			if (!waitingForKey)
				UIManager.CheckKeys();
			else
			{
				var binding = (KeyBinding)controlList.Index;
				for (var i = 0; i < 255; i++)
				{
					if ((i >= 16 && i <= 18) || i == 91)
						continue; //skip modifiers
					if (NoxicoGame.KeyMap[(Keys)i])
					{
						var theKey = (Keys)i;
						NoxicoGame.KeyBindings[binding] = theKey;
						NoxicoGame.RawBindings[binding] = theKey.ToString().ToUpperInvariant();
						NoxicoGame.KeyBindingMods[binding] = NoxicoGame.Modifiers[0];
						UpdateItems();
						controlList.DrawQuick();

						waitingForKey = false;
						NoxicoGame.KeyMap[(Keys)i] = false;
						break;
					}
				}
			}
		}

		public static void Open()
		{
			NoxicoGame.Mode = UserMode.Subscreen;
			Subscreens.FirstDraw = true;
			NoxicoGame.Subscreen = Controls.Handler;
		}
	}
}
