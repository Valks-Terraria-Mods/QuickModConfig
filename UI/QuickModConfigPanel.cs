using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

public class QuickModConfigPanel : UIState, IBlocksInput, IHasCloseButton, IHasScrollbar, IHasMainPanel
{
    public UIElement MainElement { get; set; } = null!;
    public UIElement ScrollViewElement { get; private set; } = null!;

    public event Action CloseRequested = null!;

    private readonly List<ModConfigDataGroup> _modData;

    private UIList _uiList = null!;

    public QuickModConfigPanel()
    {
        _modData = ModConfigCollector.Collect();
    }

    public override void OnInitialize()
    {
        MainElement.Width.Set(600, 0f);
        MainElement.Height.Set(300, 0f);
        MainElement.HAlign = 1f;
        MainElement.VAlign = 1f;
        MainElement.Left = StyleDimension.FromPixels(-10);
        MainElement.Top = StyleDimension.FromPixels(-10);

        Append(MainElement);

        _uiList = new UIList()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };

        foreach (var mod in _modData)
            _uiList.Add(new Button(mod.ModName));

        MainElement.Append(_uiList);

        ScrollViewElement = _uiList;
    }

    public void SetScrollbar(UIScrollbar scrollbar)
    {
        _uiList.SetScrollbar(scrollbar);
    }
}
