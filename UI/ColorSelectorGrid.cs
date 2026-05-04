using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace QuickModConfig;

public sealed class ColorSelectorGrid : UIElement
{
    private const int TextureSize = 300;
    private const int MarkerOuterSize = 13;
    private const int MarkerInnerSize = 9;
    private const int MarkerOuterThickness = 2;
    private const int MarkerInnerThickness = 1;

    private static Texture2D? _gridTexture;
    private static GraphicsDevice? _gridTextureDevice;

    private readonly Action<Color> _onColorChanged;
    private Color _currentColor;
    private float _hue;
    private float _lightness;
    private bool _dragging;

    public ColorSelectorGrid(Color initialColor, Action<Color> onColorChanged)
    {
        _onColorChanged = onColorChanged;
        Width = StyleDimension.Fill;
        Height = StyleDimension.Fill;

        OnLeftMouseDown += (_, _) =>
        {
            _dragging = true;
            UpdateColorFromMouse();
        };
        OnLeftMouseUp += (_, _) => StopDragging();

        SetColor(initialColor, notify: false);
    }

    public void SetColor(Color color, bool notify)
    {
        Vector3 hsl = Main.rgbToHsl(color);
        _hue = ClampNormalized(hsl.X);
        _lightness = ClampNormalized(hsl.Z);

        if (_currentColor == color)
            return;

        _currentColor = color;

        if (notify)
            _onColorChanged(color);
    }

    public void StopDragging()
    {
        _dragging = false;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (!_dragging)
            return;

        if (!Main.mouseLeft)
        {
            StopDragging();
            return;
        }

        UpdateColorFromMouse();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        CalculatedStyle dimensions = GetDimensions();
        Rectangle bounds = dimensions.ToRectangle();

        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        EnsureGridTexture(spriteBatch.GraphicsDevice);
        spriteBatch.Draw(_gridTexture!, bounds, Color.White);
        DrawMarker(spriteBatch, bounds);

        if ((ContainsPoint(Main.MouseScreen) || _dragging) && Main.LocalPlayer is not null)
            Main.LocalPlayer.mouseInterface = true;
    }

    private void UpdateColorFromMouse()
    {
        CalculatedStyle dimensions = GetDimensions();

        if (dimensions.Width <= 0f || dimensions.Height <= 0f)
            return;

        float x = (Main.MouseScreen.X - dimensions.X) / dimensions.Width;
        float y = (Main.MouseScreen.Y - dimensions.Y) / dimensions.Height;

        _hue = MathHelper.Clamp(x, 0f, 1f);
        _lightness = 1f - MathHelper.Clamp(y, 0f, 1f);
        PublishSelectedColor();
    }

    private void PublishSelectedColor()
    {
        Color color = ColorFromGridPosition(_hue, _lightness);

        if (_currentColor == color)
            return;

        _currentColor = color;
        _onColorChanged(color);
    }

    private void DrawMarker(SpriteBatch spriteBatch, Rectangle bounds)
    {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        int halfOuter = MarkerOuterSize / 2;
        int markerX = bounds.X + (int)MathF.Round((bounds.Width - 1) * _hue);
        int markerY = bounds.Y + (int)MathF.Round((bounds.Height - 1) * (1f - _lightness));

        markerX = Math.Clamp(markerX, bounds.Left + halfOuter, bounds.Right - halfOuter - 1);
        markerY = Math.Clamp(markerY, bounds.Top + halfOuter, bounds.Bottom - halfOuter - 1);

        Rectangle outerRect = new(markerX - halfOuter, markerY - halfOuter, MarkerOuterSize, MarkerOuterSize);
        Rectangle innerRect = new(
            markerX - MarkerInnerSize / 2,
            markerY - MarkerInnerSize / 2,
            MarkerInnerSize,
            MarkerInnerSize);

        DrawRectangleOutline(spriteBatch, pixel, outerRect, Color.Black, MarkerOuterThickness);
        DrawRectangleOutline(spriteBatch, pixel, innerRect, Color.White, MarkerInnerThickness);
    }

    private static void DrawRectangleOutline(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rect, Color color, int thickness)
    {
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }

    private static void EnsureGridTexture(GraphicsDevice graphicsDevice)
    {
        if (_gridTexture is not null && _gridTextureDevice == graphicsDevice)
            return;

        _gridTexture?.Dispose();
        _gridTextureDevice = graphicsDevice;
        _gridTexture = new Texture2D(graphicsDevice, TextureSize, TextureSize);

        var pixels = new Color[TextureSize * TextureSize];

        for (int y = 0; y < TextureSize; y++)
        {
            float lightness = 1f - y / (TextureSize - 1f);

            for (int x = 0; x < TextureSize; x++)
            {
                float hue = x / (TextureSize - 1f);
                pixels[y * TextureSize + x] = ColorFromGridPosition(hue, lightness);
            }
        }

        _gridTexture.SetData(pixels);
    }

    private static Color ColorFromGridPosition(float hue, float lightness)
        => Main.hslToRgb(hue, 1f, lightness, byte.MaxValue);

    private static float ClampNormalized(float value)
    {
        return float.IsNaN(value) ? 0f : MathHelper.Clamp(value, 0f, 1f);
    }
}
