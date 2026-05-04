using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Core;
using Microsoft.Xna.Framework;

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

                var instance = Activator.CreateInstance(type)!;
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
                    ConfigType = type,
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

        ConfigEntryUIType uiType;

        if (rangeAttribute != null && IsNumericType(valueType))
        {
            isSlider = true;
            min = Convert.ToSingle(rangeAttribute.Min);
            max = Convert.ToSingle(rangeAttribute.Max);
            uiType = ConfigEntryUIType.Slider;
        }
        else if (valueType == typeof(string))
        {
            uiType = ConfigEntryUIType.TextInput;
        }
        else if (valueType == typeof(bool))
        {
            uiType = ConfigEntryUIType.Boolean;
        }
        else if (valueType == typeof(Color))
        {
            uiType = ConfigEntryUIType.Color;
        }
        else if (valueType.IsEnum)
        {
            uiType = ConfigEntryUIType.EnumDropdown;
        }
        else
        {
            uiType = ConfigEntryUIType.NotSupported;
        }

        return new ModConfigEntry
        {
            Name = member.Name,
            ValueType = valueType,
            Member = member,
            UIType = uiType,
            DefaultValue = defaultValue,
            Min = min,
            Max = max,
            Increment = increment,
            IsSlider = isSlider
        };
    }

    private static readonly HashSet<TypeCode> NumericTypeCodes =
    [
        TypeCode.Byte, TypeCode.SByte,
        TypeCode.Int16, TypeCode.UInt16, // short, ushort
        TypeCode.Int32, TypeCode.UInt32, // int, uint
        TypeCode.Int64, TypeCode.UInt64, // long, ulong
        TypeCode.Single, TypeCode.Double, TypeCode.Decimal
    ];

    public static bool IsNumericType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return NumericTypeCodes.Contains(Type.GetTypeCode(type));
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
    public required Type ConfigType { get; init; }
    public required List<ModConfigEntry> ModConfigEntries { get; init; }
}

public sealed record ModConfigEntry
{
    public required string Name { get; init; }
    public required Type ValueType { get; init; }
    public required MemberInfo Member { get; init; }
    public required ConfigEntryUIType UIType { get; init; }
    public object? DefaultValue { get; init; }
    public float? Min { get; init; }
    public float? Max { get; init; }
    public float? Increment { get; init; }
    public bool IsSlider { get; init; }
}

public enum ConfigEntryUIType
{
    Slider,
    Color,
    TextInput,
    Boolean,
    EnumDropdown,
    NotSupported
}
