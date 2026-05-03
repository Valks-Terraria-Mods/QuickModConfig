using Terraria;
using Terraria.ModLoader;
using ValkyrieLib;

namespace QuickModConfig;

public class QuickModConfig : Mod
{
    public static ModHandle ModHandle { get; private set; } = null!;

    public override void Load()
    {
        ModHandle = ValkyrieAPI.GetHandle(this);
        ModHandle.RegisterUI("Quick Mod Config", "L", () => new MainConfigPanel());
    }

    public static void Log(object message)
    {
        Main.NewText(message);
        ModHandle.Log(message);
    }
}
