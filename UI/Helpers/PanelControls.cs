using System;
using Microsoft.Xna.Framework;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

internal static class PanelControls
{
    public static Button CreateButton(string text, Action onClick)
    {
        var button = new Button(text);
        button.OnLeftClick += (_, _) => onClick();
        return button;
    }

    public static ResetButton CreateResetButton(string text, Action onClick)
    {
        var button = new ResetButton(text);
        button.OnLeftClick += (_, _) => onClick();
        return button;
    }

    public static Button CreateDisabledButton(string text)
    {
        return new Button(text)
        {
            IgnoresMouseInteraction = true,
            TextColor = Color.DarkGray,
            BackgroundColor = Color.DimGray
        };
    }

    public static HBoxContainer CreateFooter(params UIElement[] children)
    {
        var footer = new HBoxContainer
        {
            VAlign = 1f,
            HAlign = 0f
        };

        foreach (var child in children)
            footer.Append(child);

        return footer;
    }
}
