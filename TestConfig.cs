using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace QuickModConfig;

public sealed class TestConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide; // Terraria.ModLoader.Config.ConfigScope

    [Range(1, 10)] // Terraria.ModLoader.Config.RangeAttribute
    [Increment(0.1f)] // Terraria.ModLoader.Config.IncrementAttribute
    [DefaultValue(3)] // System.ComponentModel.DefaultValueAttribute
    public float Speed { get; set; } = 3;
}
