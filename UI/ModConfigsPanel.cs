using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

public class ModConfigsPanel(MainConfigPanel mainConfigPanel, ModConfigDataGroup modData)
{
    private VBoxContainer _content = null!;

    public void Select()
    {
        mainConfigPanel.MainElement.RemoveAllChildren();
        mainConfigPanel.MainElement.Append(Build());
    }

    private VBoxContainer Build()
    {
        _content = new VBoxContainer()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };

        var title = new UITitle(modData.ModName);

        _content.Append(title);

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
            var modConfigPanel = new ModConfigPanel(mainConfigPanel, this, modConfig);

            modConfigBtn.OnLeftClick += (_, _) => modConfigPanel.Select();

            uiList.Add(modConfigBtn);
        }

        _content.Append(uiList);

        var goBackBtn = new Button("Back")
        {
            VAlign = 1f
        };

        goBackBtn.OnLeftClick += (_, _) => mainConfigPanel.Select();

        _content.Append(goBackBtn);

        return _content;
    }
}
