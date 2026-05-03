using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

internal sealed class ModConfigPanel(ConfigPanelNavigator navigator, ModConfigDataGroup modGroup, ModConfigData data)
{
    public UIElement? Build()
    {
        UIElement? entries = ConfigEntryListBuilder.Build(data);

        if (entries is null)
            return null;

        var content = new VBoxContainer
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };

        content.Append(BuildTitle());
        content.Append(entries);
        content.Append(BuildFooter());

        return content;
    }

    private HBoxContainer BuildTitle()
    {
        var titleRow = new HBoxContainer
        {
            HAlign = 0.5f
        };

        titleRow.Append(new UITitle($"{modGroup.ModName} » {data.ModConfigName}"));
        titleRow.Append(new UIText($"({data.ConfigScope})", textScale: 0.7f)
        {
            TextOriginY = 0.25f,
            Height = StyleDimension.Fill
        });

        return titleRow;
    }

    private HBoxContainer BuildFooter()
    {
        return PanelControls.CreateFooter(
            PanelControls.CreateButton($"{modGroup.ModName}'s Configs", () => navigator.ShowModConfigs(modGroup)),
            PanelControls.CreateButton("Mods", navigator.ShowMods),
            PanelControls.CreateResetButton("Reset Config", ResetConfig));
    }

    private void ResetConfig()
    {
        ConfigResetter.ResetConfig(data);

        navigator.ShowConfig(modGroup, data);
    }
}
