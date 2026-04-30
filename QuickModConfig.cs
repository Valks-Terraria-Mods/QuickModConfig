using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using ValkyrieLib;

namespace QuickModConfig;

public class QuickModConfig : Mod
{
    private static readonly string _logPath = Path.Combine(Main.SavePath, "ModSources", nameof(QuickModConfig), "Logs.txt");

    public override void Load()
    {
        // Clear log file
        File.WriteAllText(_logPath, "");

        foreach (var modConfig in ModConfigCollector.Collect())
        {
            var modName = modConfig.Key;
            var modData = modConfig.Value;

            Log(modName);
            Log($"  {modData.ModConfigName} (Scope: {modData.ConfigScope})");

            foreach (var entry in modData.ModConfigEntries)
                Log($"    {entry.Name} ({entry.ValueType}) = {entry.DefaultValue}");
        }

        var modHandle = ValkyrieAPI.GetHandle(this);
        modHandle.RegisterUI("Quick Mod Config", "L", () => new QuickModConfigPanel());
    }

    public static void Log(object message)
    {
        File.AppendAllText(_logPath, message?.ToString() + Environment.NewLine);
    }
}
