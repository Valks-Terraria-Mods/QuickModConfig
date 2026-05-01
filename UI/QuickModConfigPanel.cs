using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

public class QuickModConfigPanel : UIState, IBlocksInput, IHasCloseButton
{
    public UIElement MainElement { get; private set; } = null!;

    public event Action CloseRequested = null!;

    private readonly List<ModConfigDataGroup> _modData;

    private UIPanel _panel = null!;
    private UIScrollbar _uiScrollbar = null!;
    private bool _scrollBarVisible;

    public QuickModConfigPanel()
    {
        _modData = ModConfigCollector.Collect();
    }

    public override void OnInitialize()
    {
        _panel = new UIPanel();
        _panel.Width.Set(600, 0f);
        _panel.Height.Set(300, 0f);
        _panel.BackgroundColor = ValkyrieAPI.UI.Colors.LightBackground;
        _panel.BorderColor = ValkyrieAPI.UI.Colors.Border;
        _panel.HAlign = 1f;
        _panel.VAlign = 1f;
        _panel.Left = StyleDimension.FromPixels(-10);
        _panel.Top = StyleDimension.FromPixels(-10);
        Append(_panel);

        var uiList = new UIList()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };

        const float ScrollBarTopInset = 27;
        const float ScrollBarBottomInset = 3;
        const float ScrollBarLeftInset = 5;

        _uiScrollbar = new UIScrollbar()
        {
            Height = StyleDimension.FromPixelsAndPercent(-ScrollBarTopInset - ScrollBarBottomInset, 1f),
            Top = StyleDimension.FromPixels(ScrollBarTopInset),
            Left = StyleDimension.FromPixels(ScrollBarLeftInset),
            HAlign = 1f
        };
        _scrollBarVisible = true;
        uiList.SetScrollbar(_uiScrollbar);

        foreach (var mod in _modData)
        {
            const float VerticalPadding = 5;
            const float HorizontalPadding = 20;

            var btn = new UIButton<string>(mod.ModName)
            {
                ScalePanel = true,
                PaddingTop = VerticalPadding,
                PaddingBottom = VerticalPadding,
                PaddingLeft = HorizontalPadding,
                PaddingRight = HorizontalPadding,
            };
            
            uiList.Add(btn);
        }

        _panel.Append(uiList);
        _panel.Append(_uiScrollbar);

        MainElement = _panel;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        bool canScroll = _uiScrollbar.CanScroll;

        if (canScroll != _scrollBarVisible)
        {
            if (canScroll)
                _panel.Append(_uiScrollbar);
            else
                _uiScrollbar.Remove();

            _scrollBarVisible = canScroll;
        }
    }
}
