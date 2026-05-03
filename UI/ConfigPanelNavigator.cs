using System;
using System.Collections.Generic;
using Terraria.UI;

namespace QuickModConfig;

internal sealed class ConfigPanelNavigator(MainConfigPanel host, IReadOnlyList<ModConfigDataGroup> modData)
{
    public void ShowInitialView()
    {
        switch (NavigationState.CurrentView)
        {
            case NavigationState.View.ModConfigs:
                if (TryShowModConfigs(NavigationState.CurrentModName))
                    return;
                break;

            case NavigationState.View.Config:
                if (TryShowConfig(NavigationState.CurrentModName, NavigationState.CurrentConfigName))
                    return;
                break;
        }

        ShowMods();
    }

    public void ShowMods()
    {
        NavigationState.CurrentView = NavigationState.View.Mods;
        NavigationState.CurrentModName = null;
        NavigationState.CurrentConfigName = null;

        host.SetContent(new ModsPanel(this, modData).Build());
    }

    public void ShowModConfigs(ModConfigDataGroup modGroup)
    {
        NavigationState.CurrentView = NavigationState.View.ModConfigs;
        NavigationState.CurrentModName = modGroup.ModName;
        NavigationState.CurrentConfigName = null;

        host.SetContent(new ModConfigsPanel(this, modGroup).Build());
    }

    public bool ShowConfig(ModConfigDataGroup modGroup, ModConfigData configData)
    {
        UIElement? content = new ModConfigPanel(this, modGroup, configData).Build();

        if (content is null)
            return false;

        NavigationState.CurrentView = NavigationState.View.Config;
        NavigationState.CurrentModName = modGroup.ModName;
        NavigationState.CurrentConfigName = configData.ModConfigName;

        host.SetContent(content);
        return true;
    }

    private bool TryShowModConfigs(string? modName)
    {
        ModConfigDataGroup? modGroup = FindModGroup(modName);

        if (modGroup is null)
            return false;

        ShowModConfigs(modGroup);
        return true;
    }

    private bool TryShowConfig(string? modName, string? configName)
    {
        ModConfigDataGroup? modGroup = FindModGroup(modName);
        ModConfigData? configData = FindConfig(modGroup, configName);

        if (modGroup is null || configData is null)
            return false;

        return ShowConfig(modGroup, configData);
    }

    private ModConfigDataGroup? FindModGroup(string? modName)
    {
        if (string.IsNullOrEmpty(modName))
            return null;

        foreach (var group in modData)
        {
            if (string.Equals(group.ModName, modName, StringComparison.Ordinal))
                return group;
        }

        return null;
    }

    private static ModConfigData? FindConfig(ModConfigDataGroup? modGroup, string? configName)
    {
        if (modGroup is null || string.IsNullOrEmpty(configName))
            return null;

        foreach (var config in modGroup.ModConfigs)
        {
            if (string.Equals(config.ModConfigName, configName, StringComparison.Ordinal))
                return config;
        }

        return null;
    }
}
