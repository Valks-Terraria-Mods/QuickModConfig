using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config;

namespace QuickModConfig;

public sealed class TestConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [Range(1, 10)]
    [Increment(0.1f)]
    [DefaultValue(3f)]
    public float MyFloat { get; set; } = 3f;

    [Range(1, 10)]
    [Increment(0.5f)]
    [DefaultValue(3.0)]
    public double MyDouble { get; set; } = 3.0;

    [Range(1, 10)]
    [Increment(1f)]
    [DefaultValue(5)]
    public int MyInt { get; set; } = 5;

    [Range(1, 100)]
    [Increment(2f)]
    [DefaultValue(50L)]
    public long MyLong { get; set; } = 50L;

    [Range(0, 100)]
    [Increment(1f)]
    [DefaultValue((short)42)]
    public short MyShort { get; set; } = 42;

    [Range(0, 255)]
    [Increment(1f)]
    [DefaultValue((byte)128)]
    public byte MyByte { get; set; } = 128;

    public List<string> MyStringList { get; set; } = new List<string>()
    {
        { "Apple" },
        { "Banana" },
        { "Cherry" }
    };

    public Color MyColor { get; set; } = Color.Red;

    [DefaultValue(true)]
    public bool MyBool { get; set; } = true;
}
