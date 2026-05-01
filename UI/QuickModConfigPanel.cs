using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

public class QuickModConfigPanel : UIState, IInputConsumer, IClosable
{
    public UIElement MainElement { get; private set; } = null!;

    public event Action CloseRequested = null!;

    public override void OnInitialize()
    {
        var panel = new UIPanel();
        panel.Width.Set(300, 0f);
        panel.Height.Set(150, 0f);
        panel.BackgroundColor = ValkyrieAPI.UI.Colors.LightBackground;
        panel.BorderColor = ValkyrieAPI.UI.Colors.Border;
        panel.HAlign = 1f;
        panel.VAlign = 1f;
        panel.Left = StyleDimension.FromPixels(-10);
        panel.Top = StyleDimension.FromPixels(-10);
        Append(panel);
        
        var closeButton = ValkyrieAPI.UI.CreateCloseButton();
        closeButton.OnLeftClick += (_, _) => CloseRequested?.Invoke();
        panel.Append(closeButton);

        MainElement = panel;
    }
}
