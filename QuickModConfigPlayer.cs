using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace QuickModConfig;

// Navigation state is saved per player
public class QuickModConfigPlayer : ModPlayer
{
    private const string ViewKey = "view";
    private const string ModNameKey = "modName";
    private const string ConfigNameKey = "configName";

    public override void SaveData(TagCompound tag)
    {
        tag[ViewKey] = (int)NavigationState.CurrentView;
        
        if (NavigationState.CurrentModName != null)
            tag[ModNameKey] = NavigationState.CurrentModName;

        if (NavigationState.CurrentConfigName != null)
            tag[ConfigNameKey] = NavigationState.CurrentConfigName;
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.ContainsKey(ViewKey))
            NavigationState.CurrentView = (NavigationState.View)tag.GetInt(ViewKey);
        
        NavigationState.CurrentModName = tag.ContainsKey(ModNameKey) ? tag.GetString(ModNameKey) : null;
        NavigationState.CurrentConfigName = tag.ContainsKey(ConfigNameKey) ? tag.GetString(ConfigNameKey) : null;
    }
}
