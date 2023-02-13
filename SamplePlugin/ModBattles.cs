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
using Lumina.Data.Parsing;
using static System.Net.WebRequestMethods;
using File = System.IO.File;


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
        public static FuncSubscriber<string, int, PenumbraApiEc> PRemoveMod { get; set; }
        public static Penumbra.Api.Helpers.FuncSubscriber<string, string, System.Collections.Generic.Dictionary<string, string>, string, int, PenumbraApiEc> PTempMod { get; set; }
        public static Penumbra.Api.Helpers.FuncSubscriber<string> PGetModDirectory { get; set; }
        public static FuncSubscriber<string, System.Collections.Generic.Dictionary<string, string>, string, int, PenumbraApiEc> PTempModAll { get; set; }
        public static FuncSubscriber<string, PenumbraApiEc> PAddDirectory { get; set; }
        public static string PenumbraDirectory { get; set; }

        public static bool ActionRecieved = false;

        public static bool PlayerReady = true;

        public static int ResourceVersion = 1;

        public static Battle battle = new Battle();
        public static Dictionary<int, string> AllHpBars = new Dictionary<int, string>();
        public static string HpBarManips = "";
        public static string HpBarOutLine = "";
        public static string HpBarBar = "";
        public static string Water = "";
        public static Dictionary<int, Dictionary<string, string>> InstaKills = new Dictionary<int,Dictionary<string,string>>();

        public static List<string> ValidWorlds=new List<string>();
        public static List<int> ValidZones = new List<int>();

        public static bool ActionDisabled=false;
        private void OnChatMessage(XivChatType type, uint id, ref SeString sender, ref SeString message, ref bool handled)
        {
            //PluginLog.Log(sender.ToString() + " " + sender.TextValue + " " + sender.Payloads.Count+" " + sender.Payloads[0].ToString());


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
                   // && type.ToString() == "TellIncoming"
                    if (B[0] == "A" )
                    {
                        if (B[2] == battle.oppenent.HomeWorld)
                        {
                            if (!ActionRecieved)
                            {
                                // PluginLog.Log(type.ToString()+" This should be TellIncomming") ;
                                PluginLog.Log(battle.you.FReady.ToString() + " READY STATUS");
                                ActionRecieved = true;
                                battle.SetOpponentAction(0, 0, B[1], battle.oppenent, "oppnent action");
                                //handled = true;
                            }
                            else
                            {
                                PluginLog.Log(ActionRecieved.ToString() + "  an action has already been recieved");
                            }


                            //PluginLog.Log("Opponent Action Registered");
                            //handled = true;
                        }
                    }
                    else 
                    //if(B[0] == "A" && type.ToString() == "TellOutGoing")
                    {
                        PluginLog.Log("Out Going Attack");
                        if (battle.oppenent.FReady && battle.you.FReady)
                        {

                            PluginLog.Log("Will Fire Once due to your action " + ActionRecieved);
                            battle.Fight(battle);
                        }
                        else
                        {
                            PluginLog.Log("is not firing your action" + ActionRecieved);
                        }

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

            


            //setting up valid worlds
            ValidWorlds.Add("Gilgamesh");
            //setting up valid zones
            ValidZones.Add(651);
            ValidZones.Add(284);
            ValidZones.Add(384);
            ValidZones.Add(608);

            if (clientState.IsLoggedIn)
            {
                StartBattle();
            }

            PluginLog.Log(Obj.Length.ToString() + " LENGTH ");
            PluginLog.Log(Obj.ToString());
            for(var i=0; i<Obj.Length; i++)
            {
                if(Obj[i] != null)
                {
                    PluginLog.Log(Obj[i].ObjectKind.ToString());

                }
                
            }
            
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
            PRemoveMod = Ipc.RemoveTemporaryModAll.Subscriber(PluginInterface);
            PAddDirectory = Ipc.AddMod.Subscriber(PluginInterface);
            PenumbraDirectory = PGetModDirectory.Invoke();
            PTempModAll = Ipc.AddTemporaryModAll.Subscriber(PluginInterface);

           

            // Building Redirect Paths

            //Setting up hp bar
            HpBarManips = "H4sIAAAAAAAACkWPPQ+CMBCG/S03d8AvBjaMJDIYTVAX41BoTapwxXKNEsJ/t0XU7e1z7z2XTs4dHNpaQgRpVQCDLUdV25KT0ghRBwmSaX2IiYzKLckYRaYtCoimwWzuN0gaxcvUEwZrWXxywOB0ffm0/JdiVNXgHhs/65Y3969x0A+FnsHeuA3T+mcYhAs3lYVGMSJ/hDsx0nB8l99kQeOHkodVdSXdiH1yVmpXgw1H0Ti20qId0RHvqJ/o4ImXynnJWNn3lzesBRT+HwEAAA==\r\n";
            HpBarOutLine = PenumbraDirectory + ("/MBR/hpoutline.atex").Replace("/", "\\");
            HpBarBar = PenumbraDirectory + ("/MBR/hpbar.atex").Replace("/", "\\");
            Water = PenumbraDirectory + ("/MBR/water.atex").Replace("/", "\\");
            //var hps = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Resources/hpbarstart.avfx");
            
            DirectoryCopy(PluginInterface.AssemblyLocation.Directory?.FullName!+"/Resources", @PenumbraDirectory.ToString()+"\\MBR", false);
            for (int i = 0; i < 101; i++)
            {
                AllHpBars.Add(i, PenumbraDirectory+("/MBR/hp" + i.ToString() + ".avfx").Replace("/", "\\"));
            }
            foreach(var items in AllHpBars)
            {
                PluginLog.Log(items.Value);
            }

            //Setting Up Instakill Paths
            //tD.Add(target.Emote.Item1, );

            //tD.Add("vfx/camera/eff/lbk_drk_lv3.avfx", "vfx\\emote_sp\\nage_kiss\\Oglb3\\lbk_drk_lv3(swirls changed).avfx");
            //tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c0s.avfx", "vfx\\emote_sp\\nage_kiss\\Oglb3\\lbk_2sw_lv3_c1s(shimery changed).avfx");
            //tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c2s.avfx", "\"vfx\\\\emote_sp\\\\nage_kiss\\\\Oglb3\\\\lbk_2sw_lv3_c2s(stupid sword).avfx");
            //tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c3s.avfx", "vfx\\emote_sp\\nage_kiss\\Oglb3\\lbk_2sw_lv3_c3s(better swipe).avfx");

            //tD.Add("vfx/limitbreak/lbk_2sw_lv3/texture/gr01as.atex", "vfx\\emote_sp\\nage_kiss\\texture\\oglogo.atex");
            //tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c6s.avfx", "vfx\\emote_sp\\nage_kiss\\Oglb3\\lbk_2sw_lv3_c6s(white floor).avfx");
            //tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c4s.avfx", "vfx\\emote_sp\\nage_kiss\\Oglb3\\bk_2sw_lv3_c4s(golddrip).avfx");
            //tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c5s.avfx", "vfx\\emote_sp\\nage_kiss\\Oglb3\\lbk_2sw_lv3_c5s(gold puddle).avfx");
            //tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c1s.avfx", "vfx\\emote_sp\\nage_kiss\\Oglb3\\lbk_2sw_lv3_c1s(bigog).avfx");
            //tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c8s.avfx", "vfx\\emote_sp\\nage_kiss\\Oglb3\\lbk_2sw_lv3_c8s(bigpattern).avfx");
            Dictionary<string,string> oglist = new Dictionary<string,string>();
            oglist.Add("filler", PenumbraDirectory + ("/MBR/instakill.tmb").Replace("/", "\\"));
            oglist.Add("vfx/camera/eff/lbk_drk_lv3.avfx", PenumbraDirectory + ("/MBR/lbk_drk_lv3(swirls changed).avfx").Replace("/", "\\"));
            oglist.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c0s.avfx", PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c1s(shimery changed).avfx").Replace("/", "\\"));
            oglist.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c2s.avfx", PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c2s(stupid sword).avfx").Replace("/", "\\"));
            oglist.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c3s.avfx", PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c3s(better swipe).avfx").Replace("/", "\\"));
            oglist.Add("vfx/limitbreak/lbk_2sw_lv3/texture/gr01as.atex", PenumbraDirectory + ("/MBR/oglogo.atex").Replace("/", "\\"));
            oglist.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c6s.avfx", PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c6s(white floor).avfx").Replace("/", "\\"));
            oglist.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c4s.avfx", PenumbraDirectory + ("/MBR/bk_2sw_lv3_c4s(golddrip).avfx").Replace("/", "\\"));
            oglist.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c5s.avfx", PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c5s(gold puddle).avfx").Replace("/", "\\"));
            oglist.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c1s.avfx", PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c1s(bigog).avfx").Replace("/", "\\"));
            oglist.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c8s.avfx", PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c8s(bigpattern).avfx").Replace("/", "\\"));


           

            InstaKills.Add(0, oglist);
            ///PluginLog.Log(InstaKills[0]["filler"].ToString());

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
                else
                {
                    DirectoryInfo dirtest = new DirectoryInfo(destDirName);
                    PluginLog.Log("startup file check");
                    FileInfo[] filestest = dirtest.GetFiles();
                    bool check = false;
                    string filetest = "rv" + ResourceVersion + ".txt";
                    foreach(FileInfo file in filestest)
                    {
                        if (file.Name == filetest)
                        {
                            check = true;
                            break;
                        }
                        
                    }
                    if (!check)
                    {
                        PluginLog.Log("no valid test file");
                        foreach (FileInfo file in filestest)
                        {
                            PluginLog.Log("deleting file " + file.Name);
                            file.Delete();

                        }
                       

                    }
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
        public void StartBattle(string source="")
        {
            battle = new Battle();
            if (source != "")
            {
                PluginLog.Log(source + " this is the source "+ClientState.LocalPlayer.CurrentWorld.GameData.Name.ToString());
            }

            //PluginLog.Log(ClientState.LocalPlayer.CurrentWorld.GameData.Name + " vs " + ValidWorlds.Contains(ClientState.LocalPlayer.CurrentWorld.GameData.Name.ToString()));
            if (ValidWorlds.Contains(ClientState.LocalPlayer.CurrentWorld.GameData.Name.ToString()))
            {
                if (source != "")
                {
                    PluginLog.Log(source + " World Valid");
                }
                if (ValidZones.Contains(ClientState.TerritoryType))
                {
                    if (source != "")
                    {
                        PluginLog.Log(source + " Zone Valid");
                    }
                    //PluginLog.Log(ClientState.TerritoryType);
                    battle.you.Name = Obj[0].Name.ToString();
                    PlayerCharacter ThisPlayer = (PlayerCharacter)ModBattles.Obj[0];
                    battle.you.HomeWorld = ThisPlayer.HomeWorld.GameData.Name.ToString();
                    battle.you.Tell = "/t " + battle.you.Name + "@" + battle.you.HomeWorld;
                    battle.you.FReady = false;
                    ActionRecieved = false;
                    //battle.you.Emote= new Tuple<string, string>("chara/action/emote/goodbye_st.tmb", "/wave");
                    //emote refrence
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
