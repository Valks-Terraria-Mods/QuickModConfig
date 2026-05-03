using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.Config;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

public class MainConfigPanel : UIState, IBlocksInput, IHasCloseButton, IHasScrollbar, IHasMainPanel
{
    public UIElement MainElement { get; set; } = null!;
    public UIElement ScrollViewElement { get; private set; } = null!;

    private readonly List<ModConfigDataGroup> _modData;
    private UIElement? _currentContent;
    private UIList _uiList = null!;
    private UIImageButton? _closeButton;

    public MainConfigPanel()
    {
        _modData = ModConfigCollector.Collect();
    }

    public override void OnInitialize()
    {
        MainElement.Width = StyleDimension.FromPixels(600);
        MainElement.Height = StyleDimension.FromPixels(400);
        MainElement.Left = StyleDimension.FromPixels(-10);
        MainElement.Top = StyleDimension.FromPixels(-10);
        MainElement.HAlign = 1f;
        MainElement.VAlign = 1f;

        Append(MainElement);

        Build();
        NavigateToView(NavigationState.CurrentView);
    }

    public void Select()
    {
        NavigationState.CurrentView = NavigationState.View.Mods;
        Build();
    }

    public void SetContent(UIElement content)
    {
        if (_currentContent is not null)
            MainElement.RemoveChild(_currentContent);

        _currentContent = content;
        MainElement.Append(content);
        EnsureCloseButtonOnTop();
    }

    public void SetScrollbar(UIScrollbar scrollbar)
    {
        _uiList.SetScrollbar(scrollbar);
    }

    private void Build()
    {
        _uiList = new UIList()
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

        ScrollViewElement = _uiList;
        SetContent(vbox);
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
        EnsureCloseButtonOnTop();
    }

    private void EnsureCloseButtonOnTop()
    {
        if (_closeButton != null)
            MainElement.Append(_closeButton);
    }
}
