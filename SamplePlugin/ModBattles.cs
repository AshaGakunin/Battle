using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using ModBattles.Windows;
using System;
using Dalamud.Game.Text;
using Dalamud.Logging;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
//using XivCommon;
using Dalamud;
using Dalamud.Hooking;
using System.Threading.Tasks;
//using XivCommon.Functions;
using System.Collections.Generic;
using Dalamud.Game.Gui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Conditions;

using Lumina.Excel.GeneratedSheets;
using Condition = Dalamud.Game.ClientState.Conditions.Condition;
using Penumbra.Api.Enums;
//using Windows.Media.AppBroadcasting;
using Dalamud.Game.ClientState.Objects.SubKinds;
//using Windows.Devices.Power;
using static ModBattles.Battle;
using FFXIVClientStructs.FFXIV.Client.UI;
using Dalamud.Game.ClientState;
using Dalamud.Data;
using Penumbra.Api.Helpers;
using Penumbra.Api;
//using WinRT;
using Dalamud.Game;
using Lumina;
//using Windows.Media.Protection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;


//using ChatToAction.HubClient;

namespace ModBattles
{
    public sealed class ModBattles : IDalamudPlugin
    {
        public string Name => "Mod Battles";
        private const string CommandName = "/MB";

        //Penumbra.Api.IPenumbraApi

        //test



        private DalamudPluginInterface PluginInterface { get; init; }
        [PluginService] public static ChatGui Chat { get; set; }
        [PluginService] public static Framework Framework { get; set; }
        [PluginService] public static TargetManager Target { get; set; }
        [PluginService][RequiredVersion("1.0")] public static Condition Conditions { get; private set; } = null!;
        [PluginService] public static ClientState ClientState { get; set; } = null;
        public static Thread NewThread;
        //public static Lumina.Excel.GeneratedSheets.World CurrentWorld { get; set; }
        private bool FrameWorkSubscribed { get; set; } = false;
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("ModBattles");

        //[PluginService] public static GameGui GameGui { get; set; } = null;
        [PluginService] public static DataManager DataManager { get; set; } = null;


        [PluginService][RequiredVersion("1.0")] public static ObjectTable Obj { get; private set; } = null!;

        public static Penumbra.Api.Helpers.ActionSubscriber<GameObject, RedrawType> Predraw { get; set; }
        public static Penumbra.Api.Helpers.FuncSubscriber<IList<(string, string)>> PGetMods { get; set; }
        public static Penumbra.Api.Helpers.FuncSubscriber<string, string, string, string, string, PenumbraApiEc> PSetSettings { get; set; }
        public static Penumbra.Api.Helpers.FuncSubscriber<string, string, string, bool, PenumbraApiEc> PTurnon { get; set; }
        public static Penumbra.Api.Helpers.FuncSubscriber<string, string, IDictionary<string, (IList<string>, GroupType)>?> PGetSettings { get; set; }
        public static FuncSubscriber<string, string, int, PenumbraApiEc> PRemoveMod { get; set; }
        public static Penumbra.Api.Helpers.FuncSubscriber<string, string, System.Collections.Generic.Dictionary<string, string>, string, int, PenumbraApiEc> PTempMod { get; set; }
        public static Penumbra.Api.Helpers.FuncSubscriber<string> PGetModDirectory { get; set; }
        public static FuncSubscriber<string, PenumbraApiEc> PAddDirectory { get; set; }
        public static string PenumbraDirectory { get; set; }

        public static bool ActionRecieved = false;

        public static bool PlayerReady = true;

        public static Battle battle = new Battle();
        public static Dictionary<int, string> AllHpBars = new Dictionary<int, string>();
        public static string HpBarManips = "";
        public static string HpBarOutLine = "";
        public static string HpBarBar = "";
        public static string Water = "";
        private void OnChatMessage(XivChatType type, uint id, ref SeString sender, ref SeString message, ref bool handled)
        {
            //Oppnent not confirmed
            if (!battle.oppenent.Confirmed)
            {
                //PluginLog hallenge

                string[] A = message.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (A[0] == "Will You Fight Me " + battle.you.Name)
                {
                    if (sender.ToString() != battle.you.Name)
                    {
                        PluginLog.Log("Other player challening you.");
                        battle.you.ChallengeRecieved = true;
                        battle.you.Challengers.Add(new Tuple<string, string>(sender.ToString(), A[1]));

                    }
                    else
                    {
                        //PluginLog.Log("You challenging yourself");
                        ////should be empty not for testing puposes
                        //battle.you.ChallengeRecieved = true;
                        //battle.you.Challengers.Add(new Tuple<string, string>(sender.ToString(), A[1]));

                    }


                    //handled = true;
                }
                else
                {
                    if (battle.oppenent.Name != "")
                    {
                        if (sender.ToString() == battle.oppenent.Name)
                        {
                            if (A[1] == battle.oppenent.HomeWorld)
                            {
                                if (A[0] == "Yes I will fight you.")
                                {
                                    battle.oppenent.Confirmed = true;
                                    //handled = true;
                                    battle.you.Emote = new Tuple<string, string>("chara/action/emote/goodbye_st.tmb", "/wave");
                                    var tD = new Dictionary<string, string>();
                                    tD.Add("chara/equipment/e6064/vfx/eff/ve0005.avfx", ModBattles.AllHpBars[100]);
                                    string manip = "";
                                    PluginLog.Log(ModBattles.PTempMod.Invoke("HpBar", "Default", tD, ModBattles.HpBarManips, 0).ToString());
                                    ModBattles.Predraw.Invoke(Obj[0], RedrawType.Redraw);
                                }
                            }
                        }

                        //PluginLog.Log("not confrimed but irrelevent " + sender.ToString() + " text value " + sender.TextValue);

                    }

                }

            }
            else
            {
                if (sender.ToString() == battle.oppenent.Name)
                {
                    string[] B = message.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (B[0] == "A" && type.ToString() != "TellOutgoing")
                    {
                        if (B[2] == battle.oppenent.HomeWorld)
                        {
                            if (!ActionRecieved)
                            {
                                // PluginLog.Log(type.ToString()+" This should be TellIncomming") ;
                                ActionRecieved = true;
                                battle.SetOpponentAction(0, 0, B[1], battle.oppenent, "oppnent action");
                                handled = true;
                            }


                            //PluginLog.Log("Opponent Action Registered");
                            //handled = true;
                        }
                    }
                    else
                    {

                        //handled = true;
                    }
                }

                //PluginLog.Log("confirmed " + message.ToString() + " " + sender.ToString());
                //if (battle.oppenent.Name == sender.ToString())
                //{

                //}
            }



        }



        public ModBattles(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager, ClientState clientState, Framework framework)
        {
            
            PluginInterface = pluginInterface;
            CommandManager = commandManager;
            ClientState = clientState;
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);
            Chat.ChatMessage += OnChatMessage;
            Framework = framework;
            ClientState.Login += ClientState_Login;
            ClientState.TerritoryChanged += ClientState_TerritoryChanged;

            if (clientState.IsLoggedIn) {
                StartBattle();
            }






            //ClientState.TerritoryType

            //PluginLog.Log(PGetModDirectory.Invoke().ToString());
            /// <summary>
            /// Set a temporary mod with the given paths, manipulations and priority and the name tag to a specific collection.
            /// </summary>
            /// <param name="tag">Custom name for the temporary mod.</param>
            /// <param name="collectionName">Name of the collection the mod should apply to. Can be a temporary collection name.</param>
            /// <param name="paths">List of redirections (can be swaps or redirections).</param>
            /// <param name="manipString">Zipped Base64 string of meta manipulations.</param>
            /// <param name="priority">Desired priority.</param>
            /// <returns>CollectionMissing, InvalidGamePath, InvalidManipulation or Success.</returns>
            /// public PenumbraApiEc AddTemporaryMod( string tag, string collectionName, Dictionary< string, string > paths, string manipString,
            //int priority );
            
            //Setting Up Penubmra Api
            PTempMod = Penumbra.Api.Ipc.AddTemporaryMod.Subscriber(PluginInterface);
            Predraw = Penumbra.Api.Ipc.RedrawObject.Subscriber(PluginInterface);
            PGetMods = Penumbra.Api.Ipc.GetMods.Subscriber(PluginInterface);
            PSetSettings = Penumbra.Api.Ipc.TrySetModSetting.Subscriber(PluginInterface);
            PTurnon = Penumbra.Api.Ipc.TrySetMod.Subscriber(PluginInterface);
            PGetSettings = Penumbra.Api.Ipc.GetAvailableModSettings.Subscriber(PluginInterface);
            PGetModDirectory = Penumbra.Api.Ipc.GetModDirectory.Subscriber(PluginInterface);
            PRemoveMod = Ipc.RemoveTemporaryMod.Subscriber(PluginInterface);
            PAddDirectory = Ipc.AddMod.Subscriber(PluginInterface);
            PenumbraDirectory = PGetModDirectory.Invoke();
            //PluginLog.Log();


            //WriteResourceToFile()

            // Building Redirect Paths

            HpBarManips = "H4sIAAAAAAAACkWPPQ+CMBCG/S03d8AvBjaMJDIYTVAX41BoTapwxXKNEsJ/t0XU7e1z7z2XTs4dHNpaQgRpVQCDLUdV25KT0ghRBwmSaX2IiYzKLckYRaYtCoimwWzuN0gaxcvUEwZrWXxywOB0ffm0/JdiVNXgHhs/65Y3969x0A+FnsHeuA3T+mcYhAs3lYVGMSJ/hDsx0nB8l99kQeOHkodVdSXdiH1yVmpXgw1H0Ti20qId0RHvqJ/o4ImXynnJWNn3lzesBRT+HwEAAA==\r\n";
            var hps = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Resources/hpbarstart.avfx");
            //HpBar = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "hpbar.avfx").Replace("/","\\");
            //HpBarStart = hps.Replace("/", "\\");
            //HpBarOutLine = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Resources/hpoutline.atex").Replace("/", "\\\\");
            HpBarOutLine = PenumbraDirectory + ("/MBR/hpoutline.atex").Replace("/", "\\");
            //HpBarBar = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Resources/hpbar.atex").Replace("/", "\\\\");
            HpBarBar = PenumbraDirectory + ("/MBR/hpbar.atex").Replace("/", "\\");
            //Water = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Resources/water.atex").Replace("/", "\\\\");
            Water = PenumbraDirectory + ("/MBR/water.atex").Replace("/", "\\");
            DirectoryCopy(PluginInterface.AssemblyLocation.Directory?.FullName!+"/Resources", @PenumbraDirectory.ToString()+"\\MBR", false);
            //uginLog.Log(imagePath);

            //fuckyou(Ipc.GetPlayerMetaManipulations.Subscriber(PluginInterface).Invoke());
            for (int i = 0; i < 101; i++)
            {
                AllHpBars.Add(i, PenumbraDirectory+("/MBR/hp" + i.ToString() + ".avfx").Replace("/", "\\"));
            }
            foreach(var items in AllHpBars)
            {
                PluginLog.Log(items.Value);
            }
            //var goatImage = PluginInterface.UiBuilder.LoadImage(imagePath);
            //PluginLog.Log(battle.you.ActionDecode(test));
            WindowSystem.AddWindow(new ConfigWindow(this));



            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

       
            

        private static void DirectoryCopy(
            string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();
            //1.0.9.1
            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                //throw new DirectoryNotFoundException(
                //    "Source directory does not exist or could not be found: "
                //    + sourceDirName);
                PluginLog.Log("Source Directory of Resources does not exist.");
            }
            else
            {
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }


                // Get the file contents of the directory to copy.
                FileInfo[] files = dir.GetFiles();

                foreach (FileInfo file in files)
                {
                    // Create the path to the new copy of the file.
                    string temppath = Path.Combine(destDirName, file.Name);

                    // Copy the file.
                    if (!File.Exists(temppath))
                    {
                        file.CopyTo(temppath, false);
                    }
                    
                }

                // If copySubDirs is true, copy the subdirectories.
                if (copySubDirs)
                {

                    foreach (DirectoryInfo subdir in dirs)
                    {
                        // Create the subdirectory.
                        string temppath = Path.Combine(destDirName, subdir.Name);

                        // Copy the subdirectories.
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                    }
                }
            }

            // If the destination directory does not exist, create it.
            //"vfx/emote_sp/nage_kiss/eff/nk_emosp04_t0p.avfx": "vfx\\emote_sp\\nage_kiss\\eff\\nk_emosp04_t0p.avfx"
        }

        private void ClientState_Login(object? sender, EventArgs e)
        {
            try
            {
                StartBattle();
                //PluginLog.Log("New Battle Created On Actual Login");

            }
            catch (Exception a)
            {
                if (!FrameWorkSubscribed)
                {
                    Framework.Update += Framework_Update;
                    FrameWorkSubscribed = true;
                }
                
            }

            //throw new NotImplementedException();

        }
        public void StartBattle()
        {
            battle = new Battle();
            if (ClientState.LocalPlayer.CurrentWorld.GameData.Name == "Gilgamesh")
            {
                if (ClientState.TerritoryType.ToString() == "650" || ClientState.TerritoryType.ToString() == "652" || ClientState.TerritoryType.ToString() == "608")
                {
                    //PluginLog.Log(ClientState.TerritoryType);
                    battle.you.Name = Obj[0].Name.ToString();
                    PlayerCharacter ThisPlayer = (PlayerCharacter)ModBattles.Obj[0];
                    battle.you.HomeWorld = ThisPlayer.HomeWorld.GameData.Name.ToString();
                    battle.you.Tell = "/t " + battle.you.Name + "@" + battle.you.HomeWorld;
                    battle.you.Emote= new Tuple<string, string>("chara/action/emote/goodbye_st.tmb", "/wave");
                    //chara/action/emote/goodbye.tmb /goodbye
                }

            }
            
        }
        private void Framework_Update(Framework framework)
        {
            try
            {

                StartBattle();
                Framework.Update -= Framework_Update;
                FrameWorkSubscribed = false;

            }
            catch (Exception e) { }
           

            //throw new NotImplementedException();
        }

      

        private void ClientState_TerritoryChanged(object? sender, ushort e)
        {
            try
            {
                StartBattle();
               
                
            }
            catch (Exception a)
            {
                if (!FrameWorkSubscribed)
                {
                    Framework.Update += Framework_Update;
                    FrameWorkSubscribed = true;
                }
                
            }
            //PluginLog.Log(sender.ToString());
            //PluginLog.Log("I have changed territory");
            //battle = new Battle();
            //PluginLog.Log("New Battle Created: " + battle.you.Name);
            //battle.you.Name = "test";
            //PluginLog.Log(Obj[0].Name.ToString() + " this should be active");
            //battle.you.Name = Obj[0].Name.ToString();
            //PlayerCharacter ThisPlayer = (PlayerCharacter)ModBattles.Obj[0];
            //battle.you.HomeWorld = ThisPlayer.HomeWorld.GameData.Name.ToString();
            //battle.you.Tell = "/t " + battle.you.Name + "@" + battle.you.HomeWorld;
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            //Framework.Update? -= Framework_Update;
            if (FrameWorkSubscribed)
            {
                Framework.Update -= Framework_Update;
                FrameWorkSubscribed = false;
            }
            ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
            ClientState.Login -= ClientState_Login;
            Chat.ChatMessage -= OnChatMessage;
            WindowSystem.RemoveAllWindows();
            CommandManager.RemoveHandler(CommandName);
            battle.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            WindowSystem.GetWindow("Battle UI").IsOpen = true;

        }
        //public static void dfu(string arg)
        //{

        //    var path = @"c:\temp\fileName2.zip";
        //    byte[] zipBytes = Convert.FromBase64String(arg);
        //    using (FileStream fs = new FileStream(path, FileMode.Create))
        //    {
        //        fs.Write(zipBytes, 0, zipBytes.Length);
        //    }


        //}
        //public static string fuckyou(string arg)
        //{
        //    var path = @"c:\temp\import.zip";
        //    //var path = @arg.Replace("/", "\\");
        //    string base64 = "";

        //    using (FileStream zip = new FileStream(path, FileMode.Open))
        //    {
        //        var zipBytes = new byte[zip.Length];
        //        zip.Read(zipBytes, 0, (int)zip.Length);
        //        base64 = Convert.ToBase64String(zipBytes);
        //    }
        //    return base64; 
        //}
        //public void wtf(string m, string f)
        //{
        //    string docPath =@"c:\temp\";

        //    // Write the string array to a new file named "WriteLines.txt".
        //    using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, f)))
        //    {

        //        outputFile.WriteLine(m);
        //    }

        //}

        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            WindowSystem.GetWindow("Battle UI").IsOpen = true;
        }



    }

}
