using System.Collections.Generic;
using Terraria.ModLoader.Config;

namespace QuickModConfig;

internal static class ConfigResetter
{
    public static void ResetConfig(ModConfigData data)
    {
        if (ConfigReflection.GetConfigInstance(data.ConfigType) is not ModConfig config) return;

        foreach (var entry in data.ModConfigEntries)
            ConfigReflection.SetMemberValue(entry.Member, config, entry.DefaultValue!);

        config.SaveChanges();
    }

    public static void ResetConfigs(IEnumerable<ModConfigData> configs)
    {
        foreach (var data in configs)
            ResetConfig(data);
    }
}
