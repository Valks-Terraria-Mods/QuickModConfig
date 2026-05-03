using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace QuickModConfig;

internal sealed class ScrollableList : UIElement
{
    private const float ScrollbarWidth = 20f;
    private const float ScrollbarVerticalInset = 5f;

    private readonly UIScrollbar _scrollbar;
    private readonly bool _autoHideScrollbar;
    private bool _scrollbarVisible = true;

    public UIList List { get; }

    public ScrollableList(float listPadding = 0f, bool autoHideScrollbar = true)
    {
        _autoHideScrollbar = autoHideScrollbar;

        Width = StyleDimension.Fill;
        Height = StyleDimension.Fill;

        List = new UIList
        {
            Width = StyleDimension.FromPixelsAndPercent(-ScrollbarWidth, 1f),
            Height = StyleDimension.Fill,
            ListPadding = listPadding
        };

        _scrollbar = new UIScrollbar
        {
            Width = StyleDimension.FromPixels(ScrollbarWidth),
            Top = StyleDimension.FromPixels(ScrollbarVerticalInset),
            Height = StyleDimension.FromPixelsAndPercent(-ScrollbarVerticalInset * 2f, 1f),
            HAlign = 1f
        };

        List.SetScrollbar(_scrollbar);

        Append(List);
        Append(_scrollbar);
    }

    public void Add(UIElement item)
    {
        List.Add(item);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_autoHideScrollbar)
            SetScrollbarVisible(_scrollbar.CanScroll);
    }

    private void SetScrollbarVisible(bool visible)
    {
        if (visible == _scrollbarVisible)
            return;

        if (visible)
        {
            List.Width = StyleDimension.FromPixelsAndPercent(-ScrollbarWidth, 1f);
            Append(_scrollbar);
        }
        else
        {
            List.Width = StyleDimension.Fill;
            _scrollbar.Remove();
        }

        _scrollbarVisible = visible;
        Recalculate();
    }
}
