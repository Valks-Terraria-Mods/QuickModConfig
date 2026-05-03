using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

public class ModConfigsPanel(MainConfigPanel mainConfigPanel, ModConfigDataGroup modData)
{
    public void Select()
    {
        mainConfigPanel.SetContent(Build());
    }

    private VBoxContainer Build()
    {
        var content = new VBoxContainer();
        var title = new UITitle(modData.ModName);

        content.Append(title);

        var uiList = new UIList()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };

        var scrollbar = new UIScrollbar()
        {
            Height = StyleDimension.Fill
        };

        uiList.SetScrollbar(scrollbar);

        foreach (var modConfig in modData.ModConfigs)
        {
            var modConfigBtn = new Button(modConfig.ModConfigName);
            var modConfigPanel = new ModConfigPanel(mainConfigPanel, this, modConfig, modData.ModName);

            modConfigBtn.OnLeftClick += (_, _) => modConfigPanel.Select();

            uiList.Add(modConfigBtn);
        }

        content.Append(uiList);

        var modsBtn = new Button("Mods")
        {
            VAlign = 1f
        };
        modsBtn.OnLeftClick += (_, _) => mainConfigPanel.Select();
        content.Append(modsBtn);

        return content;
    }
}
