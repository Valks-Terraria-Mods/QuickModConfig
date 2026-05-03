using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolbarSystem;
using Terraria;
using DragonLens.Core.Systems.ToolSystem;
using System;
using Microsoft.Xna.Framework;

namespace QuickModConfig;

[JITWhenModsEnabled("DragonLens")]
[ExtendsFromMod("DragonLens")]
public class QuickModConfigTool : Tool
{
    public override string IconKey => "QuickModConfig";
    public override string DisplayName => "Quick config";
    public override string Description => "Quickly edit configs.";

    public override void OnActivate()
    {
        QuickModConfig.ModHandle.ToggleUI("Quick Mod Config");
    }
}

[JITWhenModsEnabled("DragonLens")]
[ExtendsFromMod("DragonLens")]
public class DragonLensIconIntegration : ModSystem
{
    public override void PostSetupContent()
    {
        if (!ModLoader.TryGetMod("DragonLens", out _))
            return;

        // Gear icon by MSavioti from https://opengameart.org/content/gear-pixel-art-32x32 licensed CC0
        var tex = ModContent.Request<Texture2D>($"{Mod.Name}/Assets/Textures/Gear", AssetRequestMode.ImmediateLoad).Value;

        foreach (var provider in ThemeHandler.allIconProviders.Values)
            provider.icons["QuickModConfig"] = tex;

        ModContent.GetInstance<ToolbarHandler>().OnModLoad();
    }
}
