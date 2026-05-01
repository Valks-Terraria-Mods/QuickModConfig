using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Core;

namespace QuickModConfig;

public sealed class ModConfigCollector
{
    public static List<ModConfigDataGroup> Collect()
    {
        var modDataList = new List<ModConfigDataGroup>();
        
        foreach (var mod in ModLoader.Mods)
        {
            // Check if code is null because why not
            // Skipping ModLoader as this appears to be the core built-in tModLoader mod
            if (mod.Code is null || mod.Name.Equals("ModLoader", StringComparison.Ordinal))
                continue;

            var modData = new ModConfigDataGroup()
            {
                ModName = mod.Name,
                ModConfigs = []
            };

            foreach (var type in AssemblyManager.GetLoadableTypes(mod.Code))
            {
                if (type.IsAbstract || !typeof(ModConfig).IsAssignableFrom(type))
                    continue;

                var instance = Activator.CreateInstance(type);
                var config = (ModConfig)instance!;

                var entries = new List<ModConfigEntry>();

                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!prop.CanRead || !prop.CanWrite || prop.DeclaringType == typeof(ModConfig))
                        continue;

                    var defaultValue = prop.GetCustomAttribute<DefaultValueAttribute>()?.Value;

                    entries.Add(new ModConfigEntry
                    {
                        Name = prop.Name,
                        ValueType = prop.PropertyType,
                        DefaultValue = defaultValue
                    });
                }

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (field.DeclaringType == typeof(ModConfig))
                        continue;

                    var defaultValue = field.GetCustomAttribute<DefaultValueAttribute>()?.Value;

                    entries.Add(new ModConfigEntry
                    {
                        Name = field.Name,
                        ValueType = field.FieldType,
                        DefaultValue = defaultValue
                    });
                }

                modData.ModConfigs.Add(new ModConfigData()
                {
                    ModConfigName = type.Name,
                    ConfigScope = config.Mode,
                    ModConfigEntries = entries
                });
            }

            modDataList.Add(modData);
        }

        return modDataList;
    }
}

public sealed record ModConfigDataGroup
{
    public required string ModName { get; init; }
    public required List<ModConfigData> ModConfigs { get; init; }
}

public sealed record ModConfigData
{
    public required string ModConfigName { get; init; }
    public required ConfigScope ConfigScope { get; init; }
    public required List<ModConfigEntry> ModConfigEntries { get; init; }
}

public sealed record ModConfigEntry
{
    public required string Name { get; init; }
    public required Type ValueType { get; init; }
    public object? DefaultValue { get; init; }
    public float? Min { get; init; }
    public float? Max { get; init; }
    public float? Increment { get; init; }
    public bool IsSlider { get; init; }
}

public sealed class TestConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide; // Terraria.ModLoader.Config.ConfigScope

    [Range(1, 10)] // Terraria.ModLoader.Config.RangeAttribute
    [Increment(0.1f)] // Terraria.ModLoader.Config.IncrementAttribute
    [DefaultValue(3)] // System.ComponentModel.DefaultValueAttribute
    public float Speed { get; set; } = 3;
}
