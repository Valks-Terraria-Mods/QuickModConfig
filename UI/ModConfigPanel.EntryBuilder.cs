using System;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.Config;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

public partial class ModConfigPanel
{
    private static class EntryBuilder
    {
        internal static UIText? BuildEnumDropdownEntry(HBoxContainer entryHBox, ModConfig config, ModConfigEntry entry, UIImageButton resetBtn)
        {
            var member = entry.Member;
            Type valueType = Reflection.GetMemberType(member)!;

            if (valueType is null)
                return null;

            Type underlying = Nullable.GetUnderlyingType(valueType)!;
            Type enumType = underlying ?? valueType;
            Type structType = underlying is not null ? typeof(Nullable<>).MakeGenericType(enumType) : enumType;
            Type dropdownType = typeof(Dropdown<>).MakeGenericType(structType);

            object currentVal = Reflection.GetMemberValue(member, config)!;

            object initialDropdownValue;

            if (underlying is not null)
            {
                initialDropdownValue = currentVal is null ? Activator.CreateInstance(structType)! : Activator.CreateInstance(structType, currentVal)!;
            }
            else
            {
                initialDropdownValue = currentVal ?? Activator.CreateInstance(enumType)!;
            }

            Action<object> callback = selected =>
            {
                object converted;
                if (underlying is not null)
                {
                    bool hasValue = (bool)structType.GetProperty("HasValue")!.GetValue(selected)!;
                    converted = hasValue
                        ? structType.GetProperty("Value")!.GetValue(selected)!
                        : null!;
                }
                else
                {
                    converted = selected;
                }

                Reflection.SetMemberValue(member, config, converted);
                config.SaveChanges();
            };

            object dropdownObj = Activator.CreateInstance(dropdownType, initialDropdownValue, callback)!;
            entryHBox.Append((UIElement)dropdownObj);

            resetBtn.OnLeftClick += (_, _) =>
            {
                object? defaultVal = entry.DefaultValue;
                object defaultForDropdown;

                if (underlying is not null)
                {
                    defaultForDropdown = defaultVal is null ? Activator.CreateInstance(structType)! : Activator.CreateInstance(structType, defaultVal)!;
                }
                else
                {
                    defaultForDropdown = defaultVal ?? Activator.CreateInstance(enumType)!;
                }

                var setValueMethod = dropdownType.GetMethod("SetValue", [structType, typeof(bool)]);
                
                setValueMethod!.Invoke(dropdownObj, [defaultForDropdown, true]);
            };

            return null;
        }

        internal static UIText? BuildSliderEntry(HBoxContainer entryHBox, ModConfig config, ModConfigEntry entry, UIImageButton resetBtn)
        {
            var member = entry.Member;
            float min = entry.Min ?? 0f;
            float max = entry.Max ?? 10f;
            float defaultVal = Reflection.ConvertToFloat(entry.DefaultValue, 0f);
            float currentVal = Reflection.ConvertToFloat(
                Reflection.GetMemberValue(member, config), defaultVal);

            var feedbackLabel = new UIText(currentVal.ToString("0.##"));
            var slider = CreateSlider(config, member, currentVal, min, max, feedbackLabel);

            WireResetButton(resetBtn, () => slider.SetValue(defaultVal, notify: true));

            entryHBox.Append(slider);
            entryHBox.Append(feedbackLabel);

            return feedbackLabel;
        }

        internal static UIText? BuildTextInputEntry(HBoxContainer entryHBox, ModConfig config, ModConfigEntry entry, UIImageButton resetBtn)
        {
            var member = entry.Member;
            string strValue = Reflection.GetMemberValue(member, config)?.ToString() ?? "";

            var inputField = new InputField(strValue)
            {
                MaxLength = int.MaxValue
            };

            inputField.ValueChanged += newStr =>
            {
                Reflection.SetMemberValue(member, config, newStr);
                config.SaveChanges();
            };

            WireResetButton(resetBtn, () =>
            {
                Reflection.SetMemberValue(member, config, entry.DefaultValue ?? "");
                config.SaveChanges();
            });

            entryHBox.Append(inputField);
            return null;
        }

        internal static UIText? BuildBooleanEntry(HBoxContainer entryHBox, ModConfig config, ModConfigEntry entry, UIImageButton resetBtn)
        {
            const float HorizontalBooleanPadding = 15;
            const string BooleanTrue = "On";
            const string BooleanFalse = "Off";

            var member = entry.Member;
            bool current = (bool)(Reflection.GetMemberValue(member, config) ?? false);

            var boolBtn = new Button(current ? BooleanTrue : BooleanFalse)
            {
                PaddingLeft = HorizontalBooleanPadding,
                PaddingRight = HorizontalBooleanPadding,
            };

            boolBtn.OnLeftClick += (_, _) =>
            {
                bool current = (bool)(Reflection.GetMemberValue(member, config) ?? false);
                Reflection.SetMemberValue(member, config, !current);
                boolBtn.SetText(!current ? BooleanTrue : BooleanFalse);
                config.SaveChanges();
            };

            WireResetButton(resetBtn, () =>
            {
                Reflection.SetMemberValue(member, config, entry.DefaultValue ?? false);
                config.SaveChanges();
            });

            entryHBox.Append(boolBtn);
            return null;
        }

        internal static UIText? BuildUnsupportedEntry(HBoxContainer entryHBox, ModConfigEntry entry, bool logUnexpected)
        {
            if (logUnexpected)
            {
                QuickModConfig.Log($"Unexpected UIType: {entry.UIType}");
                return null;
            }

            entryHBox.Append(new UIText($"[{entry.ValueType.Name}]")
            {
                TextOriginX = 0
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
                Type? memberType = Reflection.GetMemberType(memberInfo);

                if (memberType == null)
                    return;

                object finalValue = Reflection.ConvertToMemberType(newValue, memberType);
                float displayValue = Reflection.ConvertToFloat(finalValue, 0f);
                valueFeedback.SetText(displayValue.ToString("0.##"));
                Reflection.SetMemberValue(memberInfo, config, finalValue);
                config.SaveChanges();
            };

            return slider;
        }
    }
}
