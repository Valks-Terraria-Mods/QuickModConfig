using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

public class ModConfigPanel(MainConfigPanel mainConfigPanel, ModConfigsPanel modConfigsPanel, ModConfigData data)
{
    public void Select()
    {
        mainConfigPanel.MainElement.RemoveAllChildren();
        mainConfigPanel.MainElement.Append(Build());
    }

    private VBoxContainer Build()
    {
        var vboxMain = new VBoxContainer();

        var titleHBox = new HBoxContainer()
        {
            HAlign = 0.5f
        };
        var title = new UITitle(data.ModConfigName);
        var scope = new UIText($"({data.ConfigScope})", textScale: 0.7f)
        {
            TextOriginY = 0.5f,
            Height = StyleDimension.Fill
        };

        titleHBox.Append(title);
        titleHBox.Append(scope);
        vboxMain.Append(titleHBox);

        var hboxEntries = new HBoxContainer()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
        };

        var entries = new UIList()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
            ListPadding = 15
        };
        var entriesScrollbar = new UIScrollbar()
        {
            Height = StyleDimension.Fill
        };

        entries.SetScrollbar(entriesScrollbar);

        foreach (var entry in data.ModConfigEntries)
        {
            var entryHBox = new HBoxContainer();

            entryHBox.Append(new UIText(entry.Name));

            entries.Add(entryHBox);
        }

        hboxEntries.Append(entries);
        hboxEntries.Append(entriesScrollbar);
        vboxMain.Append(hboxEntries);

        var goBackBtn = new Button("Back")
        {
            VAlign = 1f
        };

        goBackBtn.OnLeftClick += (_, _) => modConfigsPanel.Select();

        vboxMain.Append(goBackBtn);

        return vboxMain;
    }
}
