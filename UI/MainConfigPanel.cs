using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
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
    private ConfigPanelNavigator _navigator = null!;
    private UIElement? _currentContent;
    private UIImageButton? _closeButton;

    public MainConfigPanel()
    {
        _modData = ModConfigCollector.Collect();
    }

    public override void OnInitialize()
    {
        MainElement = CreateMainElement();
        _navigator = new ConfigPanelNavigator(this, _modData);

        Append(MainElement);
        _navigator.ShowInitialView();
    }

    public void Select()
    {
        _navigator.ShowMods();
    }

    public void SetContent(UIElement content)
    {
        if (_currentContent is not null)
            MainElement.RemoveChild(_currentContent);

        _currentContent = content;
        MainElement.Append(content);

        if (_closeButton is not null)
        {
            _closeButton.Remove();
            MainElement.Append(_closeButton);
        }
    }

    public void SetCloseButton(UIImageButton button)
    {
        _closeButton = button;
        MainElement.Append(_closeButton);
    }

    private static UIPanel CreateMainElement()
    {
        return new UIPanel
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
    }
}
