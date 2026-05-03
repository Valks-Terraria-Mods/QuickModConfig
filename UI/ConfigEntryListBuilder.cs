using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.Config;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

internal static class ConfigEntryListBuilder
{
    public static UIElement? Build(ModConfigData data)
    {
        if (ConfigReflection.GetConfigInstance(data.ConfigType) is not ModConfig config)
            return null;

        var list = new ScrollableList(listPadding: 15f);
        var rows = new List<ConfigEntryRow>();

        foreach (var entry in data.ModConfigEntries)
        {
            ConfigEntryRow row = ConfigEntryRowBuilder.Build(config, entry);
            rows.Add(row);
            list.Add(row.Element);
        }

        AlignRows(rows);

        return list;
    }

    private static void AlignRows(List<ConfigEntryRow> rows)
    {
        float maxLabelWidth = 0f;
        float maxFeedbackWidth = 0f;

        foreach (var row in rows)
        {
            maxLabelWidth = Math.Max(maxLabelWidth, row.NameLabel.MinWidth.Pixels);

            if (row.FeedbackLabel is not null)
                maxFeedbackWidth = Math.Max(maxFeedbackWidth, row.FeedbackLabel.MinWidth.Pixels);
        }

        foreach (var row in rows)
        {
            row.NameLabel.Width = StyleDimension.FromPixels(maxLabelWidth);

            if (row.FeedbackLabel is not null)
                row.FeedbackLabel.Width = StyleDimension.FromPixels(maxFeedbackWidth + 5f);
        }
    }

    private readonly struct ConfigEntryRow
    {
        public required HBoxContainer Element { get; init; }
        public required UIText NameLabel { get; init; }
        public UIText? FeedbackLabel { get; init; }
    }

    private static class ConfigEntryRowBuilder
    {
        private const float SpaceAfterName = 5f;

        public static ConfigEntryRow Build(ModConfig config, ModConfigEntry entry)
        {
            var row = new HBoxContainer();
            var nameLabel = new UIText(entry.Name) { TextOriginX = 1f };
            var resetButton = ValkyrieAPI.UI.Assets.SearchCancelButton;

            row.Append(nameLabel);
            row.Append(new UIElement { Width = StyleDimension.FromPixels(SpaceAfterName) });

            UIText? feedbackLabel = ConfigEntryControlBuilder.Build(row, config, entry, resetButton);

            row.Append(resetButton);

            return new ConfigEntryRow
            {
                Element = row,
                NameLabel = nameLabel,
                FeedbackLabel = feedbackLabel
            };
        }
    }

    private static class ConfigEntryControlBuilder
    {
        public static UIText? Build(HBoxContainer row, ModConfig config, ModConfigEntry entry, UIImageButton resetButton)
        {
            return entry.UIType switch
            {
                ConfigEntryUIType.Slider => BuildSliderEntry(row, config, entry, resetButton),
                ConfigEntryUIType.TextInput => BuildTextInputEntry(row, config, entry, resetButton),
                ConfigEntryUIType.Boolean => BuildBooleanEntry(row, config, entry, resetButton),
                ConfigEntryUIType.EnumDropdown => BuildEnumDropdownEntry(row, config, entry, resetButton),
                ConfigEntryUIType.NotSupported => BuildUnsupportedEntry(row, entry, logUnexpected: false),
                _ => BuildUnsupportedEntry(row, entry, logUnexpected: true),
            };
        }

        private static UIText? BuildEnumDropdownEntry(HBoxContainer row, ModConfig config, ModConfigEntry entry, UIImageButton resetButton)
        {
            MemberInfo member = entry.Member;
            Type? valueType = ConfigReflection.GetMemberType(member);

            if (valueType is null)
                return null;

            Type? underlying = Nullable.GetUnderlyingType(valueType);
            Type enumType = underlying ?? valueType;
            Type dropdownValueType = underlying is not null ? typeof(Nullable<>).MakeGenericType(enumType) : enumType;
            Type dropdownType = typeof(Dropdown<>).MakeGenericType(dropdownValueType);

            object? currentValue = ConfigReflection.GetMemberValue(member, config);
            object initialDropdownValue = CreateDropdownValue(currentValue, enumType, dropdownValueType, underlying is not null);

            Action<object> callback = selected =>
            {
                object? converted = ConvertDropdownValue(selected, dropdownValueType, underlying is not null);
                SetMemberValueAndSave(member, config, converted);
            };

            object dropdown = Activator.CreateInstance(dropdownType, initialDropdownValue, callback)!;
            row.Append((UIElement)dropdown);

            resetButton.OnLeftClick += (_, _) =>
            {
                object defaultDropdownValue = CreateDropdownValue(entry.DefaultValue, enumType, dropdownValueType, underlying is not null);
                MethodInfo? setValueMethod = dropdownType.GetMethod("SetValue", [dropdownValueType, typeof(bool)]);

                setValueMethod?.Invoke(dropdown, [defaultDropdownValue, true]);
            };

            return null;
        }

        private static UIText BuildSliderEntry(HBoxContainer row, ModConfig config, ModConfigEntry entry, UIImageButton resetButton)
        {
            MemberInfo member = entry.Member;
            float min = entry.Min ?? 0f;
            float max = entry.Max ?? 10f;
            float defaultValue = ConfigReflection.ConvertToFloat(entry.DefaultValue, 0f);
            float currentValue = ConfigReflection.ConvertToFloat(
                ConfigReflection.GetMemberValue(member, config),
                defaultValue);

            var feedbackLabel = new UIText(currentValue.ToString("0.##"));
            Slider slider = CreateSlider(config, member, currentValue, min, max, feedbackLabel);

            WireResetButton(resetButton, () => slider.SetValue(defaultValue, notify: true));

            row.Append(slider);
            row.Append(new UIElement { Width = StyleDimension.FromPixels(5f) });
            row.Append(feedbackLabel);

            return feedbackLabel;
        }

        private static UIText? BuildTextInputEntry(HBoxContainer row, ModConfig config, ModConfigEntry entry, UIImageButton resetButton)
        {
            MemberInfo member = entry.Member;
            string currentValue = ConfigReflection.GetMemberValue(member, config)?.ToString() ?? "";

            var inputField = new InputField(currentValue)
            {
                MaxLength = int.MaxValue
            };

            inputField.ValueChanged += newValue => SetMemberValueAndSave(member, config, newValue);

            WireResetButton(resetButton, () =>
            {
                string defaultValue = entry.DefaultValue?.ToString() ?? "";
                inputField.SetValue(defaultValue, notify: true);
            });

            row.Append(inputField);
            return null;
        }

        private static UIText? BuildBooleanEntry(HBoxContainer row, ModConfig config, ModConfigEntry entry, UIImageButton resetButton)
        {
            const float HorizontalBooleanPadding = 15f;
            const string BooleanTrue = "On";
            const string BooleanFalse = "Off";

            MemberInfo member = entry.Member;
            bool current = (bool)(ConfigReflection.GetMemberValue(member, config) ?? false);

            var button = new Button(current ? BooleanTrue : BooleanFalse)
            {
                PaddingLeft = HorizontalBooleanPadding,
                PaddingRight = HorizontalBooleanPadding,
            };

            button.OnLeftClick += (_, _) =>
            {
                bool current = (bool)(ConfigReflection.GetMemberValue(member, config) ?? false);
                bool next = !current;

                SetMemberValueAndSave(member, config, next);
                button.SetText(next ? BooleanTrue : BooleanFalse);
            };

            WireResetButton(resetButton, () =>
            {
                bool defaultValue = (bool)(entry.DefaultValue ?? false);

                SetMemberValueAndSave(member, config, defaultValue);
                button.SetText(defaultValue ? BooleanTrue : BooleanFalse);
            });

            row.Append(button);
            return null;
        }

        private static UIText? BuildUnsupportedEntry(HBoxContainer row, ModConfigEntry entry, bool logUnexpected)
        {
            if (logUnexpected)
            {
                QuickModConfig.Log($"Unexpected UIType: {entry.UIType}");
                return null;
            }

            row.Append(new UIText($"[{entry.ValueType.Name}]")
            {
                TextOriginX = 0f
            });

            return null;
        }

        private static Slider CreateSlider(ModConfig config, MemberInfo member, float value, float min, float max, UIText valueFeedback)
        {
            var slider = new Slider(value, min, max);

            slider.ValueChanged += newValue =>
            {
                Type? memberType = ConfigReflection.GetMemberType(member);

                if (memberType is null)
                    return;

                object finalValue = ConfigReflection.ConvertToMemberType(newValue, memberType);
                float displayValue = ConfigReflection.ConvertToFloat(finalValue, 0f);

                valueFeedback.SetText(displayValue.ToString("0.##"));
                SetMemberValueAndSave(member, config, finalValue);
            };

            return slider;
        }

        private static object CreateDropdownValue(object? value, Type enumType, Type dropdownValueType, bool isNullable)
        {
            if (isNullable)
                return value is null ? Activator.CreateInstance(dropdownValueType)! : Activator.CreateInstance(dropdownValueType, value)!;

            return value ?? Activator.CreateInstance(enumType)!;
        }

        private static object? ConvertDropdownValue(object selected, Type dropdownValueType, bool isNullable)
        {
            if (!isNullable)
                return selected;

            bool hasValue = (bool)dropdownValueType.GetProperty("HasValue")!.GetValue(selected)!;

            return hasValue
                ? dropdownValueType.GetProperty("Value")!.GetValue(selected)
                : null;
        }

        private static void SetMemberValueAndSave(MemberInfo member, ModConfig config, object? value)
        {
            ConfigReflection.SetMemberValue(member, config, value!);
            config.SaveChanges();
        }

        private static void WireResetButton(UIImageButton resetButton, Action resetAction)
        {
            resetButton.OnLeftClick += (_, _) => resetAction();
        }
    }
}
