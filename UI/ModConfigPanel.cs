using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

public class ModConfigPanel(MainConfigPanel mainConfigPanel, ModConfigsPanel modConfigsPanel, ModConfigData data, string modName)
{
    public void Select()
    {
        VBoxContainer? content = Build();

        if (content != null)
            mainConfigPanel.SetContent(content);
    }

    private VBoxContainer? Build()
    {
        var vboxMain = new VBoxContainer();

        var titleHBox = new HBoxContainer()
        {
            HAlign = 0.5f
        };
        var title = new UITitle($"{modName} » {data.ModConfigName}");
        var scope = new UIText($"({data.ConfigScope})", textScale: 0.7f)
        {
            TextOriginY = 0.25f,
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

        var maxLabelNameWidth = 0f;
        var maxFeedbackNameWidth = 0f;

        if (ConfigReflectionHelpers.GetConfigInstance(data.ConfigType) is not ModConfig config)
            return null;

        var configRows = new List<ConfigRow>();

        foreach (var entry in data.ModConfigEntries)
        {
            const float SpaceAfterName = 5;

            var entryHBox = new HBoxContainer();
            var nameLabel = new UIText(entry.Name) { TextOriginX = 1f };
            entryHBox.Append(nameLabel);

            var nameSpacer = new UIElement()
            {
                Width = StyleDimension.FromPixels(SpaceAfterName)
            };

            entryHBox.Append(nameSpacer);

            var minNameWidth = nameLabel.MinWidth.Pixels;
            if (minNameWidth > maxLabelNameWidth) maxLabelNameWidth = minNameWidth;

            var resetBtn = ValkyrieAPI.UI.Assets.SearchCancelButton;

            UIText? feedbackLabel = entry.UIType switch
            {
                ConfigEntryUIType.Slider => BuildSliderEntry(entryHBox, config, entry, resetBtn),
                ConfigEntryUIType.TextInput => BuildTextInputEntry(entryHBox, config, entry, resetBtn),
                ConfigEntryUIType.Boolean => BuildBooleanEntry(entryHBox, config, entry, resetBtn),

                // TODO: Implement these types.
                ConfigEntryUIType.EnumDropdown or
                ConfigEntryUIType.NotSupported => BuildUnsupportedEntry(entryHBox, entry, logUnexpected: false),

                _ => BuildUnsupportedEntry(entryHBox, entry, logUnexpected: true),
            };

            if (feedbackLabel != null)
            {
                var minFeedbackWidth = feedbackLabel.MinWidth.Pixels;
                if (minFeedbackWidth > maxFeedbackNameWidth) maxFeedbackNameWidth = minFeedbackWidth;
            }

            configRows.Add(new ConfigRow()
            {
                NameLabel = nameLabel,
                FeedbackLabel = feedbackLabel
            });

            entryHBox.Append(resetBtn);
            entries.Add(entryHBox);
        }

        foreach (var configRow in configRows)
        {
            configRow.NameLabel.Width = StyleDimension.FromPixels(maxLabelNameWidth);

            if (configRow.FeedbackLabel is not null)
                configRow.FeedbackLabel.Width = StyleDimension.FromPixels(maxFeedbackNameWidth + 5);
        }

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

    private static UIText? BuildSliderEntry(HBoxContainer entryHBox, ModConfig config, ModConfigEntry entry, UIImageButton resetBtn)
    {
        var member = entry.Member;
        float min = entry.Min ?? 0f;
        float max = entry.Max ?? 10f;
        float defaultVal = ConfigReflectionHelpers.ConvertToFloat(entry.DefaultValue, 0f);
        float currentVal = ConfigReflectionHelpers.ConvertToFloat(
            ConfigReflectionHelpers.GetMemberValue(member, config), defaultVal);

        var feedbackLabel = new UIText(currentVal.ToString("0.##"));
        var slider = CreateSlider(config, member, currentVal, min, max, feedbackLabel);

        WireResetButton(resetBtn, () => slider.SetValue(defaultVal, notify: true));

        entryHBox.Append(slider);
        entryHBox.Append(feedbackLabel);

        return feedbackLabel;
    }

    private static UIText? BuildTextInputEntry(HBoxContainer entryHBox, ModConfig config, ModConfigEntry entry, UIImageButton resetBtn)
    {
        var member = entry.Member;
        string strValue = ConfigReflectionHelpers.GetMemberValue(member, config)?.ToString() ?? "";

        var inputField = new InputField("", strValue)
        {
            MaxLength = int.MaxValue
        };

        inputField.ValueChanged += newStr =>
        {
            ConfigReflectionHelpers.SetMemberValue(member, config, newStr);
            config.SaveChanges();
        };

        WireResetButton(resetBtn, () =>
        {
            ConfigReflectionHelpers.SetMemberValue(member, config, entry.DefaultValue ?? "");
            config.SaveChanges();
        });

        entryHBox.Append(inputField);
        return null;
    }

    private static UIText? BuildBooleanEntry(HBoxContainer entryHBox, ModConfig config, ModConfigEntry entry, UIImageButton resetBtn)
    {
        const float HorizontalBooleanPadding = 15;
        const string BooleanTrue = "On";
        const string BooleanFalse = "Off";

        var member = entry.Member;
        bool current = (bool)(ConfigReflectionHelpers.GetMemberValue(member, config) ?? false);

        var spacingElement = new UIElement()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };

        var boolBtn = new Button(current ? BooleanTrue : BooleanFalse)
        {
            PaddingLeft = HorizontalBooleanPadding,
            PaddingRight = HorizontalBooleanPadding,
        };

        boolBtn.OnLeftClick += (_, _) =>
        {
            bool current = (bool)(ConfigReflectionHelpers.GetMemberValue(member, config) ?? false);
            ConfigReflectionHelpers.SetMemberValue(member, config, !current);
            boolBtn.SetText(!current ? BooleanTrue : BooleanFalse);
            config.SaveChanges();
        };

        WireResetButton(resetBtn, () =>
        {
            ConfigReflectionHelpers.SetMemberValue(member, config, entry.DefaultValue ?? false);
            config.SaveChanges();
        });

        spacingElement.Append(boolBtn);
        entryHBox.Append(spacingElement);
        return null;
    }

    private static UIText? BuildUnsupportedEntry(HBoxContainer entryHBox, ModConfigEntry entry, bool logUnexpected)
    {
        if (logUnexpected)
        {
            QuickModConfig.Log($"Unexpected UIType: {entry.UIType}");
            return null;
        }

        entryHBox.Append(new UIText($"[{entry.Member.GetType().Name}]")
        {
            TextOriginX = 0,
            Width = StyleDimension.Fill,
        });

        return null;
    }

    private static void WireResetButton(UIImageButton resetBtn, Action resetAction)
    {
        resetBtn.OnLeftClick += (_, _) => resetAction();
    }

    private static Slider CreateSlider(ModConfig config, MemberInfo memberInfo, float value, float min, float max, UIText valueFeedback)
    {
        var slider = new Slider(value, min, max);

        slider.ValueChanged += (newValue) =>
        {
            Type? memberType = ConfigReflectionHelpers.GetMemberType(memberInfo);

            if (memberType == null)
                return;

            object finalValue = ConfigReflectionHelpers.ConvertToMemberType(newValue, memberType);
            float displayValue = ConfigReflectionHelpers.ConvertToFloat(finalValue, 0f);
            valueFeedback.SetText(displayValue.ToString("0.##"));
            ConfigReflectionHelpers.SetMemberValue(memberInfo, config, finalValue);
            config.SaveChanges();
        };

        return slider;
    }

    private static class ConfigReflectionHelpers
    {
        /// <summary>
        /// ModContent.GetInstance<T>() does not have a type parameter so that is why this method was created.
        /// </summary>
        internal static object? GetConfigInstance(Type configType)
        {
            var method = typeof(ModContent).GetMethod("GetInstance", Type.EmptyTypes);

            if (method == null)
            {
                QuickModConfig.Log("ModContent.GetInstance method not found - tModLoader API changed?");
                return null;
            }

            var generic = method.MakeGenericMethod(configType);

            var instance = generic.Invoke(null, null);

            if (instance == null)
            {
                QuickModConfig.Log($"ModContent.GetInstance<{configType.Name}>() returned null.");
                return null;
            }

            return instance;
        }

        internal static object? GetMemberValue(MemberInfo member, object target)
        {
            switch (member)
            {
                case PropertyInfo p: return p.GetValue(target);
                case FieldInfo f: return f.GetValue(target);
                default:
                    QuickModConfig.Log($"Unsupported member type: {member.MemberType}");
                    return null;
            }
        }

        internal static void SetMemberValue(MemberInfo member, object target, object value)
        {
            switch (member)
            {
                case PropertyInfo p: p.SetValue(target, value); break;
                case FieldInfo f: f.SetValue(target, value); break;
                default:
                    QuickModConfig.Log($"Unsupported member type: {member.MemberType}");
                    break;
            }
        }

        internal static Type? GetMemberType(MemberInfo member)
        {
            switch (member)
            {
                case PropertyInfo p:
                    return p.PropertyType;
                case FieldInfo f:
                    return f.FieldType;
                default:
                    QuickModConfig.Log($"Unsupported member type: {member.MemberType}");
                    return null;
            }
        }

        internal static float ConvertToFloat(object? value, float fallback)
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

        internal static object ConvertToMemberType(float sliderValue, Type targetType)
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

    private readonly struct ConfigRow
    {
        internal required UIText NameLabel { get; init; }
        internal UIText? FeedbackLabel { get; init; }
    }
}
