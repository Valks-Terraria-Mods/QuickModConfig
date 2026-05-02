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

                    entries.Add(CreateConfigEntry(prop, prop.PropertyType));
                }

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (field.DeclaringType == typeof(ModConfig))
                        continue;

                    entries.Add(CreateConfigEntry(field, field.FieldType));
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

    private static ModConfigEntry CreateConfigEntry(MemberInfo member, Type valueType)
    {
        var defaultValueAttribute = member.GetCustomAttribute<DefaultValueAttribute>();
        var rangeAttribute = member.GetCustomAttribute<RangeAttribute>();
        var incrementAttribute = member.GetCustomAttribute<IncrementAttribute>();

        var defaultValue = defaultValueAttribute?.Value;
        var isSlider = false;
        float? increment = incrementAttribute != null ? Convert.ToSingle(incrementAttribute.Increment) : null;

        var min = 0f;
        var max = 0f;

        if (rangeAttribute != null)
        {
            min = Convert.ToSingle(rangeAttribute.Min);
            max = Convert.ToSingle(rangeAttribute.Max);
            isSlider = true;
        }

        return new ModConfigEntry
        {
            Name = member.Name,
            ValueType = valueType,
            DefaultValue = defaultValue,
            Min = min,
            Max = max,
            Increment = increment,
            IsSlider = isSlider
        };
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
