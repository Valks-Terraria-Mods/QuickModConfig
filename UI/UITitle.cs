using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace QuickModConfig;

public class UITitle : UIText
{
    public UITitle(string name) : base(name, textScale: 0.5f, large: true)
    {
        Width = StyleDimension.Fill;
        HAlign = 0.5f;
    }
}
