using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.Config;
using Terraria.UI;
using ValkyrieLib;

namespace QuickModConfig;

public partial class ModConfigPanel(MainConfigPanel mainConfigPanel, ModConfigsPanel modConfigsPanel, ModConfigData data, string modName)
{
    public void Select()
    {
        VBoxContainer? content = Build();
        if (content != null)
        {
            NavigationState.CurrentView = NavigationState.View.Config;
            NavigationState.CurrentModName = modName;
            NavigationState.CurrentConfigName = data.ModConfigName;
            mainConfigPanel.SetContent(content);
        }
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

        if (ConfigReflection.GetConfigInstance(data.ConfigType) is not ModConfig config)
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
                ConfigEntryUIType.Slider => EntryBuilder.BuildSliderEntry(entryHBox, config, entry, resetBtn),
                ConfigEntryUIType.TextInput => EntryBuilder.BuildTextInputEntry(entryHBox, config, entry, resetBtn),
                ConfigEntryUIType.Boolean => EntryBuilder.BuildBooleanEntry(entryHBox, config, entry, resetBtn),
                ConfigEntryUIType.EnumDropdown => EntryBuilder.BuildEnumDropdownEntry(entryHBox, config, entry, resetBtn),
                ConfigEntryUIType.NotSupported => EntryBuilder.BuildUnsupportedEntry(entryHBox, entry, logUnexpected: false),

                _ => EntryBuilder.BuildUnsupportedEntry(entryHBox, entry, logUnexpected: true),
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

        var hboxNav = new HBoxContainer
        {
            VAlign = 1f,
            HAlign = 0f
        };

        var resetAllBtn = new ResetButton("Reset Config");

        resetAllBtn.OnLeftClick += (_, _) =>
        {
            ConfigResetter.ResetConfig(data);
            Select();
        };

        var modsBtn = new Button("Mods");
        modsBtn.OnLeftClick += (_, _) => mainConfigPanel.Select();
        hboxNav.Append(modsBtn);

        var configsBtn = new Button($"{modName}'s Configs");
        configsBtn.OnLeftClick += (_, _) => modConfigsPanel.Select();
        hboxNav.Append(configsBtn);
        hboxNav.Append(resetAllBtn);

        vboxMain.Append(hboxNav);

        return vboxMain;
    }

    private readonly struct ConfigRow
    {
        internal required UIText NameLabel { get; init; }
        internal UIText? FeedbackLabel { get; init; }
    }
}
