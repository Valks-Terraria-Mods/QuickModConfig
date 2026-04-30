using Terraria.ModLoader.UI;
using Terraria.UI;

namespace QuickModConfig;

public static class UiControlFactory
{
    /// <summary>
    /// Creates a close button docked to the top right.
    /// </summary>
    public static UIButton<string> CloseBtn()
    {
        const int Size = 32;

        return new UIButton<string>("X")
        {
            Width = StyleDimension.FromPixels(Size),
            Height = StyleDimension.FromPixels(Size),
            HAlign = 1f,
        };
    }
}
