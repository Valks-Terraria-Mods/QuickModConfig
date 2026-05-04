using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

internal sealed class ModConfigsPanel(ConfigPanelNavigator navigator, ModConfigDataGroup modData)
{
    public UIElement Build()
    {
        var content = new VBoxContainer
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };

        content.Append(new UITitle(modData.ModName));
        content.Append(BuildConfigList());
        content.Append(PanelControls.CreateFooter(
            PanelControls.CreateButton("Mods", navigator.ShowMods),
            PanelControls.CreateResetButton("Reset Configs", ResetConfigs)));

        return content;
    }

    private ScrollableList BuildConfigList()
    {
        var list = new ScrollableList();

        foreach (var config in modData.ModConfigs)
            list.Add(BuildConfigButton(config));

        return list;
    }

    private UIElement BuildConfigButton(ModConfigData config)
    {
        if (config.ModConfigEntries.Count == 0)
            return PanelControls.CreateDisabledButton(config.ModConfigName);

        return PanelControls.CreateButton(config.ModConfigName, () => navigator.ShowConfig(modData, config));
    }

    private void ResetConfigs()
    {
        ConfigResetter.ResetConfigs(modData.ModConfigs);

        navigator.ShowModConfigs(modData);
    }
}
