namespace QuickModConfig;

internal static class NavigationState
{
    public enum View { Mods, ModConfigs, Config }

    public static View CurrentView { get; set; } = View.Mods;
    public static string? CurrentModName { get; set; }
    public static string? CurrentConfigName { get; set; }
}
