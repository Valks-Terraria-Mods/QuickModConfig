using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

public sealed class ColorPickerButton : Button
{
    private const float ButtonWidthPixels = 60f;
    private const float ButtonHeightPixels = 30f;
    private const float PopupSizePixels = 300f;
    private const float PopupGapPixels = 4f;
    private const float ScreenMarginPixels = 8f;
    private const float HiddenCoord = -10000f;

    private static readonly Color ButtonBorderColor = ValkyrieAPI.UI.Colors.Border;
    private static readonly Color ButtonHoverBorderColor = Color.White;
    private static readonly Color PopupBackgroundColor = new(33, 43, 79, 255);

    private readonly Action<Color> _onColorChanged;
    private Color _currentColor;
    private UIPanel? _popupPanel;
    private ColorSelectorGrid? _selectorGrid;
    private bool _popupVisible;
    private bool _wasLeftMouseDown;
    private bool _wasRightMouseDown;

    public ColorPickerButton(Color initialColor, Action<Color> onColorChanged) : base("")
    {
        _currentColor = initialColor;
        _onColorChanged = onColorChanged;

        Width.Set(ButtonWidthPixels, 0f);
        Height.Set(ButtonHeightPixels, 0f);
        ScalePanel = false;
        TextColor = Color.Transparent;
        UseAltColors = () => true;
        SetPadding(0f);

        OnLeftClick += (_, _) => TogglePopup();
        ApplyButtonColor();
    }

    public void SetColor(Color color, bool notify)
    {
        if (_currentColor == color)
            return;

        _currentColor = color;
        ApplyButtonColor();
        _selectorGrid?.SetColor(color, notify: false);

        if (notify)
            _onColorChanged(color);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        bool leftDown = Main.mouseLeft;
        bool rightDown = Main.mouseRight;
        bool leftClicked = leftDown && !_wasLeftMouseDown;
        bool rightClicked = rightDown && !_wasRightMouseDown;

        _wasLeftMouseDown = leftDown;
        _wasRightMouseDown = rightDown;

        if (!_popupVisible)
            return;

        if (Main.keyState.IsKeyDown(Keys.Escape) && !Main.oldKeyState.IsKeyDown(Keys.Escape))
        {
            ClosePopup();
            return;
        }

        if (!leftClicked && !rightClicked)
            return;

        bool overButton = ContainsPoint(Main.MouseScreen);
        bool overPopup = _popupPanel?.ContainsPoint(Main.MouseScreen) ?? false;

        if (!overButton && !overPopup)
            ClosePopup();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (ContainsPoint(Main.MouseScreen) && Main.LocalPlayer is not null)
            Main.LocalPlayer.mouseInterface = true;
    }

    private void TogglePopup()
    {
        if (_popupVisible)
            ClosePopup();
        else
            OpenPopup();
    }

    private void OpenPopup()
    {
        UIElement root = GetRootElement();

        EnsurePopup(root);
        PositionPopup();
        _selectorGrid!.SetColor(_currentColor, notify: false);
        _popupPanel!.IgnoresMouseInteraction = false;
        _popupPanel.Recalculate();
        _popupVisible = true;
    }

    private void ClosePopup()
    {
        if (_popupPanel is null)
        {
            _popupVisible = false;
            return;
        }

        _selectorGrid?.StopDragging();
        _popupPanel.Left.Set(HiddenCoord, 0f);
        _popupPanel.Top.Set(HiddenCoord, 0f);
        _popupPanel.Width.Set(0f, 0f);
        _popupPanel.Height.Set(0f, 0f);
        _popupPanel.IgnoresMouseInteraction = true;
        _popupVisible = false;
    }

    private void EnsurePopup(UIElement root)
    {
        if (_popupPanel is not null)
        {
            if (_popupPanel.Parent is null)
                root.Append(_popupPanel);

            return;
        }

        _popupPanel = new UIPanel
        {
            Left = StyleDimension.FromPixels(HiddenCoord),
            Top = StyleDimension.FromPixels(HiddenCoord),
            Width = StyleDimension.FromPixels(0f),
            Height = StyleDimension.FromPixels(0f),
            BackgroundColor = PopupBackgroundColor,
            BorderColor = ButtonBorderColor,
            IgnoresMouseInteraction = true,
        };
        _popupPanel.SetPadding(0f);

        _selectorGrid = new ColorSelectorGrid(_currentColor, HandleGridColorChanged)
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
        };

        _popupPanel.Append(_selectorGrid);
        root.Append(_popupPanel);
    }

    private void PositionPopup()
    {
        CalculatedStyle buttonDims = GetDimensions();
        float maxLeft = Math.Max(ScreenMarginPixels, Main.screenWidth - PopupSizePixels - ScreenMarginPixels);
        float maxTop = Math.Max(ScreenMarginPixels, Main.screenHeight - PopupSizePixels - ScreenMarginPixels);

        float left = MathHelper.Clamp(buttonDims.X, ScreenMarginPixels, maxLeft);
        float belowTop = buttonDims.Y + buttonDims.Height + PopupGapPixels;
        float aboveTop = buttonDims.Y - PopupSizePixels - PopupGapPixels;
        bool placeAbove = belowTop + PopupSizePixels > Main.screenHeight - ScreenMarginPixels
            && aboveTop >= ScreenMarginPixels;

        float top = MathHelper.Clamp(placeAbove ? aboveTop : belowTop, ScreenMarginPixels, maxTop);

        _popupPanel!.Left.Set(left, 0f);
        _popupPanel.Top.Set(top, 0f);
        _popupPanel.Width.Set(PopupSizePixels, 0f);
        _popupPanel.Height.Set(PopupSizePixels, 0f);
    }

    private void HandleGridColorChanged(Color color)
    {
        _currentColor = color;
        ApplyButtonColor();
        _onColorChanged(color);
    }

    private void ApplyButtonColor()
    {
        BackgroundColor = _currentColor;
        HoverPanelColor = _currentColor;
        HoverBorderColor = ButtonHoverBorderColor;
        BorderColor = ButtonBorderColor;
        AltPanelColor = _currentColor;
        AltHoverPanelColor = _currentColor;
        AltBorderColor = ButtonBorderColor;
        AltHoverBorderColor = ButtonHoverBorderColor;
    }

    private UIElement GetRootElement()
    {
        UIElement element = this;

        while (element.Parent is not null)
            element = element.Parent;

        return element;
    }
}
