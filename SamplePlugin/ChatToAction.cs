using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using ChatToAction.Windows;
using System;
using Dalamud.Game.Text;
using Dalamud.Logging;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using XivCommon;
using Dalamud;
using Dalamud.Hooking;
using System.Threading.Tasks;
using XivCommon.Functions;
using System.Collections.Generic;
using Dalamud.Game.Gui;

namespace ChatToAction
{
    public sealed class ChatToAction : IDalamudPlugin
    {
        public string Name => "Chat To Action";
        private const string CommandName = "/CTA";
       

        private DalamudPluginInterface PluginInterface { get; init; }
        [PluginService] public ChatGui Chat { get; set; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("ChatToAction");


        //private XivCommonBase xivCommon = null!;
        //private Localization localization = null!;


        private void OnChatMessage(XivChatType type, uint id, ref SeString sender, ref SeString message, ref bool handled)
        {
            this.Configuration.GoatTextChange = message.ToString();

        }

        public ChatToAction(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            Chat.ChatMessage += OnChatMessage;


          
           // you might normally want to embed resources and load them from the manifest stream
           var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

            WindowSystem.AddWindow(new ConfigWindow(this));
            WindowSystem.AddWindow(new MainWindow(this, goatImage));
           

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            WindowSystem.GetWindow("My Amazing Window").IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            WindowSystem.GetWindow("Settings").IsOpen = true;
        }


        
    }

}
