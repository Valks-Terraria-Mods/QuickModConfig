using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
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
    private const float DragHandleHeight = 30;

    private readonly List<ModConfigDataGroup> _modData;
    private ConfigPanelNavigator _navigator = null!;
    private UIElement? _currentContent;
    private UIElement? _dragHandle;
    private UIImageButton? _closeButton;
    private Vector2 _dragOffset;
    private bool _dragging;

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
        BringDragHandleToFront();

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

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (!_dragging)
            return;

        if (!Main.mouseLeft)
        {
            _dragging = false;
            return;
        }

        Vector2 panelPosition = Main.MouseScreen - _dragOffset;

        MainElement.Left = StyleDimension.FromPixels(panelPosition.X);
        MainElement.Top = StyleDimension.FromPixels(panelPosition.Y);
        MainElement.Recalculate();
    }

    private UIPanel CreateMainElement()
    {
        var panel = new UIPanel
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

        _dragHandle = new UIElement
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(DragHandleHeight)
        };

        _dragHandle.OnLeftMouseDown += (_, _) => StartDragging();
        _dragHandle.OnLeftMouseUp += (_, _) => _dragging = false;

        panel.Append(_dragHandle);

        return panel;
    }

    private void BringDragHandleToFront()
    {
        if (_dragHandle is null)
            return;

        _dragHandle.Remove();
        MainElement.Append(_dragHandle);
    }

    private void StartDragging()
    {
        CalculatedStyle dimensions = MainElement.GetDimensions();

        _dragging = true;
        _dragOffset = Main.MouseScreen - new Vector2(dimensions.X, dimensions.Y);

        MainElement.Left = StyleDimension.FromPixels(dimensions.X);
        MainElement.Top = StyleDimension.FromPixels(dimensions.Y);
        MainElement.HAlign = 0f;
        MainElement.VAlign = 0f;
        MainElement.Recalculate();
    }
}
