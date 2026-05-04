using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using System;

namespace QuickModConfig;

[JITWhenModsEnabled("DragonLens")]
[ExtendsFromMod("DragonLens")]
public class QuickModConfigTool : Tool
{
    public override string IconKey => "QuickModConfig";
    public override string DisplayName => "Quick config";
    public override string Description => "Quickly edit configs.";

    // Gear icon by MSavioti from https://opengameart.org/content/gear-pixel-art-32x32 licensed CC0
    private static readonly Texture2D _gearIcon = ModContent.Request<Texture2D>("QuickModConfig/Assets/Textures/Gear", AssetRequestMode.ImmediateLoad).Value;

    public override void OnActivate()
    {
        QuickModConfig.ModHandle.ToggleUI("Quick Mod Config");
    }

    public override void DrawIcon(SpriteBatch spriteBatch, Rectangle target)
    {
        var scale = Math.Min((float)target.Width / _gearIcon.Width, (float)target.Height / _gearIcon.Height);

        var position = new Vector2(
            target.X + ((target.Width - (_gearIcon.Width * scale)) / 2f),
            target.Y + ((target.Height - (_gearIcon.Height * scale)) / 2f)
        );

        spriteBatch.Draw(_gearIcon, position, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}

// IMPORTANT: Commented out because Krafs.Publicizer stopped working for some reason!

// [JITWhenModsEnabled("DragonLens")]
// [ExtendsFromMod("DragonLens")]
// public class DragonLensIconIntegration : ModSystem
// {
//     public override void PostSetupContent()
//     {
//         if (!ModLoader.TryGetMod("DragonLens", out _))
//             return;

//         // Gear icon by MSavioti from https://opengameart.org/content/gear-pixel-art-32x32 licensed CC0
//         var tex = ModContent.Request<Texture2D>($"{Mod.Name}/Assets/Textures/Gear", AssetRequestMode.ImmediateLoad).Value;

//         foreach (var provider in ThemeHandler.allIconProviders.Values)
//             provider.icons["QuickModConfig"] = tex;

//         ModContent.GetInstance<ToolbarHandler>().OnModLoad();
//     }
// }
