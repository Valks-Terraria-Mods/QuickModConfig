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
        
        foreach (var modConfig in ModConfigCollector.Collect())
        {
            var modName = modConfig.Key;
            var modData = modConfig.Value;

            Log(modName);
            Log($"  {modData.ModConfigName} (Scope: {modData.ConfigScope})");

            foreach (var entry in modData.ModConfigEntries)
                Log($"    {entry.Name} ({entry.ValueType}) = {entry.DefaultValue}");
        }
    }

    public static void Log(object message)
    {
        _modHandle.Log(message);
    }
}
