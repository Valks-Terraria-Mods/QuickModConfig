using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using ValkyrieLib;

namespace QuickModConfig;

public class QuickModConfig : Mod
{
    private static ModHandle _modHandle = null!;

    public override void Load()
    {
        _modHandle = ValkyrieAPI.GetHandle(this);
        _modHandle.RegisterUI("Quick Mod Config", "L", () => new QuickModConfigPanel());

        foreach (var modDataGroup in ModConfigCollector.Collect())
        {
            Log(modDataGroup.ModName);

            foreach (var modConfig in modDataGroup.ModConfigs)
            {
                Log($"  {modConfig.ModConfigName} (Scope: {modConfig.ConfigScope})");

                foreach (var entry in modConfig.ModConfigEntries)
                    Log($"    {entry.Name} ({entry.ValueType}) = {entry.DefaultValue}");
            }
        }
    }

    public static void Log(object message)
    {
        _modHandle.Log(message);
    }
}
