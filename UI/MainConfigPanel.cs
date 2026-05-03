using System.Collections.Generic;
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
    }

    public void Select()
    {
        Build();
    }

    public void SetContent(UIElement content)
    {
        if (_currentContent is not null)
            MainElement.RemoveChild(_currentContent);

        _currentContent = content;
        MainElement.Append(content);
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
            var modConfigsPanel = new ModConfigsPanel(this, mod);

            modBtn.OnLeftClick += (_, _) => modConfigsPanel.Select();

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
}
