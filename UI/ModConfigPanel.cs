using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

public class ModConfigPanel(MainConfigPanel mainConfigPanel, ModConfigsPanel modConfigsPanel, ModConfigData data)
{
    private readonly List<UIText> _entryLabels = [];

    public void Select()
    {
        mainConfigPanel.MainElement.RemoveAllChildren();
        mainConfigPanel.MainElement.Append(Build());
    }

    private VBoxContainer Build()
    {
        var vboxMain = new VBoxContainer();

        var titleHBox = new HBoxContainer()
        {
            HAlign = 0.5f
        };
        var title = new UITitle(data.ModConfigName);
        var scope = new UIText($"({data.ConfigScope})", textScale: 0.7f)
        {
            TextOriginY = 0.5f,
            Height = StyleDimension.Fill
        };

        titleHBox.Append(title);
        titleHBox.Append(scope);
        vboxMain.Append(titleHBox);

        var hboxEntries = new HBoxContainer()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
        };

        var entries = new UIList()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
            ListPadding = 15
        };
        var entriesScrollbar = new UIScrollbar()
        {
            Height = StyleDimension.Fill
        };

        entries.SetScrollbar(entriesScrollbar);

        var maxLabelWidth = 0f;

        foreach (var entry in data.ModConfigEntries)
        {
            var entryHBox = new HBoxContainer();
            var label = new UIText(entry.Name)
            {
                TextOriginX = 1f
            };

            _entryLabels.Add(label);

            entryHBox.Append(label);

            var minWidth = label.MinWidth.Pixels;

            if (minWidth > maxLabelWidth)
                maxLabelWidth = minWidth;

            if (entry.IsSlider)
            {
                float min = entry.Min ?? 0f;
                float max = entry.Max ?? 10f;

                var currentEntry = entry;
                var configInstance = GetConfigInstance(data.ConfigType);
                var prop = configInstance.GetType().GetProperty(currentEntry.Name);

                object liveValue = prop?.GetValue(configInstance)!;
                float value = ConvertToFloat(liveValue, min);

                var slider = new Slider(value, min, max);

                slider.ValueChanged += (newValue) =>
                {
                    if (prop == null)
                        return;

                    object finalValue = ConvertToPropertyType(newValue, prop.PropertyType);
                    prop.SetValue(configInstance, finalValue);

                    // if (configInstance is ModConfig modConfig)
                    //     modConfig.SaveChanges();
                };

                entryHBox.Append(slider);
            }

            entries.Add(entryHBox);
        }

        foreach (var label in _entryLabels)
            label.Width = StyleDimension.FromPixels(maxLabelWidth);

        hboxEntries.Append(entries);
        hboxEntries.Append(entriesScrollbar);
        vboxMain.Append(hboxEntries);

        var goBackBtn = new Button("Back")
        {
            VAlign = 1f
        };

        goBackBtn.OnLeftClick += (_, _) => modConfigsPanel.Select();

        vboxMain.Append(goBackBtn);

        return vboxMain;
    }

    private static object GetConfigInstance(Type configType)
    {
        var method = typeof(ModContent).GetMethod("GetInstance", Type.EmptyTypes);
        var generic = method!.MakeGenericMethod(configType);
        return generic.Invoke(null, null)!;
    }

    private static float ConvertToFloat(object value, float fallback)
    {
        if (value == null)
            return fallback;

        // Unwrap Nullable<T>
        Type type = value.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var hasValue = (bool)type.GetProperty("HasValue")!.GetValue(value)!;
            if (!hasValue) return fallback;
            value = type.GetProperty("Value")!.GetValue(value)!;
        }

        return value switch
        {
            float f => f,
            double d => (float)d,
            int i => i,
            long l => l,
            short s => s,
            byte b => b,
            _ => Convert.ToSingle(value)
        };
    }

    private static object ConvertToPropertyType(float sliderValue, Type targetType)
    {
        Type underlying = Nullable.GetUnderlyingType(targetType)!;
        if (underlying != null)
            targetType = underlying;

        if (targetType == typeof(float)) return sliderValue;
        if (targetType == typeof(double)) return (double)sliderValue;
        if (targetType == typeof(int)) return (int)Math.Round(sliderValue);
        if (targetType == typeof(long)) return (long)Math.Round(sliderValue);
        if (targetType == typeof(short)) return (short)Math.Round(sliderValue);
        if (targetType == typeof(byte)) return (byte)Math.Clamp(Math.Round(sliderValue), 0, 255);
        if (targetType == typeof(bool)) return sliderValue >= 0.5f;

        return Convert.ChangeType(sliderValue, targetType);
    }
}
