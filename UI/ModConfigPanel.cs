using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
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
                float value = entry.DefaultValue as float? ?? min;
                
                var slider = new Slider(value, min, max);

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
}
