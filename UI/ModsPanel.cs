using System.Collections.Generic;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

internal sealed class ModsPanel(ConfigPanelNavigator navigator, IReadOnlyList<ModConfigDataGroup> modData)
{
    public UIElement Build()
    {
        var content = new VBoxContainer
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };

        content.Append(new UITitle("Mods"));
        content.Append(BuildModList());
        content.Append(PanelControls.CreateFooter(
            PanelControls.CreateResetButton("Reset All Configs", ResetAllConfigs)));

        return content;
    }

    private ScrollableList BuildModList()
    {
        var list = new ScrollableList();

        foreach (var modGroup in modData)
            list.Add(BuildModButton(modGroup));

        return list;
    }

    private UIElement BuildModButton(ModConfigDataGroup modGroup)
    {
        if (modGroup.ModConfigs.Count == 0)
            return PanelControls.CreateDisabledButton(modGroup.ModName);

        return PanelControls.CreateButton(modGroup.ModName, () => navigator.ShowModConfigs(modGroup));
    }

    private void ResetAllConfigs()
    {
        foreach (var modGroup in modData)
            ConfigResetter.ResetConfigs(modGroup.ModConfigs);
    }
}
