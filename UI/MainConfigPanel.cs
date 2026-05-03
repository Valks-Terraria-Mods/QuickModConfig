using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.Config;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

public class MainConfigPanel : UIState, IBlocksInput, IHasCloseButton
{
    public UIElement MainElement { get; set; } = null!;

    private const float PanelWidth = 600;
    private const float PanelHeight = 400;
    private const float PanelTransparency = 0.3f;
    private const float PanelMargin = 10;

    private readonly List<ModConfigDataGroup> _modData;
    private UIElement? _currentContent;
    private UIList _uiList = null!;
    private UIImageButton? _closeButton;
    private UIScrollbar? _uiScrollbar;
    private bool _scrollBarVisible;

    public MainConfigPanel()
    {
        _modData = ModConfigCollector.Collect();
    }

    public override void OnInitialize()
    {
        MainElement = new UIPanel()
        {
            Width = StyleDimension.FromPixels(PanelWidth),
            Height = StyleDimension.FromPixels(PanelHeight),
            BackgroundColor = ValkyrieAPI.UI.Colors.LightBackground * PanelTransparency,
            BorderColor = ValkyrieAPI.UI.Colors.Border,
            Left = StyleDimension.FromPixels(-PanelMargin),
            Top = StyleDimension.FromPixels(-PanelMargin),
            HAlign = 1f,
            VAlign = 1f
        };

        Append(MainElement);

        Build();
        NavigateToView(NavigationState.CurrentView);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Only show the scrollbar if it can actually scroll (there is enough content overflow to allow scrolling)
        if (_uiScrollbar != null)
        {
            bool canScroll = _uiScrollbar.CanScroll;

            if (canScroll != _scrollBarVisible)
            {
                if (canScroll)
                    MainElement.Append(_uiScrollbar);
                else
                    _uiScrollbar.Remove();

                _scrollBarVisible = canScroll;
            }
        }
    }

    public void Select()
    {
        NavigationState.CurrentView = NavigationState.View.Mods;
        Build();
    }

    public void SetContent(UIElement content, bool removeMainScrollbar = true)
    {
        if (removeMainScrollbar)
        {
            _uiScrollbar?.Remove();
            _uiScrollbar = null;
            _scrollBarVisible = false;
        }

        if (_currentContent is not null)
        {
            _scrollBarVisible = false;
            MainElement.RemoveChild(_currentContent);
        }

        _currentContent = content;
        MainElement.Append(content);

        if (_closeButton is not null)
        {
            _closeButton.Remove();
            MainElement.Append(_closeButton);
        }
    }

    private void Build()
    {
        _uiList = new UIList()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };

        _uiScrollbar = CreateScrollBar();
        _scrollBarVisible = true;

        _uiList.SetScrollbar(_uiScrollbar);

        var content = new UIElement()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };

        var vbox = new VBoxContainer()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };

        vbox.Append(new UITitle("Mods"));

        foreach (var mod in _modData)
        {
            var modBtn = new Button(mod.ModName);
            bool hasConfigs = mod.ModConfigs.Count > 0;

            if (hasConfigs)
            {
                var modConfigsPanel = new ModConfigsPanel(this, mod);
                modBtn.OnLeftClick += (_, _) => modConfigsPanel.Select();
            }
            else
            {
                // Gray out and disable interaction
                modBtn.IgnoresMouseInteraction = true;
                modBtn.TextColor = Color.DarkGray;
                modBtn.BackgroundColor = Color.DimGray;
            }

            _uiList.Add(modBtn);
        }

        vbox.Append(_uiList);

        var bottomRow = new HBoxContainer
        {
            VAlign = 1f,
            HAlign = 0f
        };

        var resetAllBtn = new ResetButton("Reset All Configs");
        resetAllBtn.OnLeftClick += (_, _) =>
        {
            foreach (var modGroup in _modData)
                ConfigResetter.ResetConfigs(modGroup.ModConfigs);
        };
        bottomRow.Append(resetAllBtn);
        vbox.Append(bottomRow);

        content.Append(vbox);
        content.Append(_uiScrollbar);

        SetContent(content, removeMainScrollbar: false);
    }

    private void NavigateToView(NavigationState.View view)
    {
        switch (view)
        {
            case NavigationState.View.ModConfigs:
                var modGroup = GetModGroup(NavigationState.CurrentModName);
                if (modGroup != null)
                    new ModConfigsPanel(this, modGroup).Select();
                break;

            case NavigationState.View.Config:
                modGroup = GetModGroup(NavigationState.CurrentModName);
                if (modGroup != null)
                {
                    var configData = modGroup.ModConfigs.Find(c => c.ModConfigName == NavigationState.CurrentConfigName);
                    if (configData != null)
                        new ModConfigPanel(this, new ModConfigsPanel(this, modGroup), configData, modGroup.ModName).Select();
                }
                break;
        }
    }

    private ModConfigDataGroup? GetModGroup(string? modName)
    {
        if (string.IsNullOrEmpty(modName)) return null;
        return _modData.Find(m => m.ModName == modName);
    }

    public void SetCloseButton(UIImageButton button)
    {
        _closeButton = button;
        MainElement.Append(_closeButton);
    }

    private static UIScrollbar CreateScrollBar()
    {
        const float ScrollBarTopInset = 27f;
        const float ScrollBarBottomInset = 3;
        const float ScrollBarLeftInset = 5;

        return new UIScrollbar()
        {
            Height = StyleDimension.FromPixelsAndPercent(-ScrollBarTopInset - ScrollBarBottomInset, 1f),
            Top = StyleDimension.FromPixels(ScrollBarTopInset),
            Left = StyleDimension.FromPixels(ScrollBarLeftInset),
            HAlign = 1f
        };
    }
}
