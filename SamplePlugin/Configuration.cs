using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using System;

namespace ChatToAction
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
        public bool Say { get; set; } = true;
        public string GoatTextChange { get; set; } = "This will not be a goat.";


// the below exist just to make saving less cumbersome
[NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
