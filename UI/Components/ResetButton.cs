using Microsoft.Xna.Framework;
using ValkyrieLib;

namespace QuickModConfig;

public class ResetButton : Button
{
    private static Color _backgroundColor = new(60, 60, 60, 255); // dark gray
    private static Color _hoverColor = new(200, 50, 50, 255); // red

    public ResetButton(string text) : base(text)
    {
        BackgroundColor = _backgroundColor;
        HoverPanelColor = _hoverColor;
        HoverBorderColor = Color.Black;
    }
}
