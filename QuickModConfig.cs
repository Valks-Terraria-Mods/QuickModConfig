using Terraria;
using Terraria.ModLoader;
using ValkyrieLib;

namespace QuickModConfig;

public class QuickModConfig : Mod
{
    private static ModHandle _modHandle = null!;

    public override void Load()
    {
        _modHandle = ValkyrieAPI.GetHandle(this);
        _modHandle.RegisterUI("Quick Mod Config", "L", () => new MainConfigPanel());
    }

    public static void Log(object message)
    {
        Main.NewText(message);
        _modHandle.Log(message);
    }
}
