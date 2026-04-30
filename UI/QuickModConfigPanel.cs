using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
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
        panel.BackgroundColor = Main.OurFavoriteColor;
        panel.HAlign = 1f;
        panel.VAlign = 1f;
        Append(panel);
        
        var closeButton = UiControlFactory.CloseBtn();
        closeButton.OnLeftClick += (_, _) => CloseRequested?.Invoke();
        panel.Append(closeButton);

        MainElement = panel;
    }
}
