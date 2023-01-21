using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using Penumbra;

namespace ModBattles
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

       
       
        public string GoatTextChange { get; set; } = "This will not be a goat.";

       // public List<CardPlayer> CardPlayers { get; set; } = new List<CardPlayer>() { };

        

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
