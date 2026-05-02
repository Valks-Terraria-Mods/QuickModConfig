using System;
using System.ComponentModel;
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

    [Range(0, 100)]
    [Increment(1f)]
    [DefaultValue(null)]
    public int? MyNullableInt { get; set; } = 50;

    [DefaultValue(true)]
    public bool MyBool { get; set; } = true;
}
