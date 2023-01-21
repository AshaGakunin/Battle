using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Penumbra;
using Penumbra.Api;
using Dalamud.Plugin;
using Penumbra.Api.Enums;
using Penumbra.Api.Helpers;
//using Windows.ApplicationModel.Store.Preview.InstallControl;
using ModBattles;
//using Windows.Media.Playback;
using System.Threading;
//using Windows.Media.Playlists;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
namespace ModBattles.Windows;


public sealed class ConfigWindow : Window, IDisposable

{
    
    //public readonly IPenumbraApi Api;
    
    //private DalamudPluginInterface PluginInterface { get; init; }
    private Configuration Configuration;
   
    private GameObject Self = ModBattles.Obj[0];
    private static List<PlayerList> playerList = new List<PlayerList>();
    private static int item_current_idx = 0;
    private static int item_current_idxno = 0;
    private static int challenge_index = 0;
    private Vector4 Red = new Vector4(1.0f, 0.0f, 0.0f, .8f);
    private Vector4 Green= new Vector4(0.16470588235294117f, 0.7215686274509804f, 0.10980392156862745f, .8f);
    private Vector4 HealerGreen = new Vector4(0.16470588235294117f, 0.7215686274509804f, 0.10980392156862745f, .8f);
    //private Vector4 Blue = new Vector4(0.15294117647058825f, 0.16862745098039217f, 0.9607843137254902f, .8f);
    private Vector4 Blue = new Vector4(0.15294117647058825f, 0.16862745098039217f, 0.9607843137254902f, .8f);
    //private static 
    //private Pen





    public ConfigWindow(ModBattles plugin) : base(
        "Battle UI",
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.Configuration = plugin.Configuration;
        
    }

    private class PlayerList
    {
        public string Name { get; set; }
        public string HomeWorld { get; set; }
    }

    public void Dispose() { }

    public override unsafe void Draw()
    {
        if (ModBattles.ClientState.LocalPlayer.CurrentWorld.GameData.Name == "Gilgamesh")
        {
            if (ModBattles.ClientState.TerritoryType.ToString() == "650" || ModBattles.ClientState.TerritoryType.ToString() == "652" || ModBattles.ClientState.TerritoryType.ToString() == "608")
            {
                var lengthObjOut = ModBattles.Obj.Length;
                var logout = "";
                //playerList.Clear();
                for (int n = 1; n < lengthObjOut; n++)
                {
                    
                    try
                    {
                        if (ModBattles.Obj[n].ObjectKind.ToString() == "Player")
                        {
                            logout += ModBattles.Obj[n].Name.ToString() + " , ";

                            var check = true;
                            if (playerList.Count > 0)
                            {
                                //PluginLog.Log(playerList.Count + " > 0");
                                for (var g = 0; g < playerList.Count; g++)
                                {
                                    if (playerList[g].Name == ModBattles.Obj[n].Name.ToString())
                                    {
                                        check = false;
                                        //PluginLog.Log(playerList[g].Name + " = " + ModBattles.Obj[n].Name.ToString());

                                    }

                                }
                            }
                            if (check)
                            {
                                PlayerCharacter ThisPlayer = (PlayerCharacter)ModBattles.Obj[n];
                                var p = new PlayerList();
                                p.Name = ModBattles.Obj[n].Name.ToString();
                                p.HomeWorld = ThisPlayer.HomeWorld.GameData.Name.ToString();
                                playerList.Add(p);
                            }


                        }

                    }
                    catch (Exception e)
                    {

                    }
                }
                if (playerList.Count > 0)
                {
                    try
                    {
                        string combo_preview_value = playerList[item_current_idx].Name;
                        if (ImGui.BeginCombo("Player to Add", combo_preview_value))
                        {


                            var lengthObj = playerList.Count;
                            for (int n = 0; n < lengthObj; n++)
                            {


                                bool is_selected = item_current_idx == n;


                                if (ImGui.Selectable(playerList[n].Name, true))
                                    item_current_idx = n;


                                if (is_selected)

                                    ImGui.SetItemDefaultFocus();
                                // Set the initial focus when opening the combo (scrolling + keyboard navigation focus)

                            }
                            ImGui.EndCombo();

                        }
                    }
                    catch (Exception e)
                    {
                        PluginLog.Log(e.ToString());
                        if (item_current_idx != 0)
                        {
                            item_current_idx--;

                        }
                        else
                        {
                            item_current_idx = 0;
                        }


                    }



                    ImGui.SameLine();
                    if (ModBattles.battle.oppenent.Confirmed)
                    {
                        ImGui.BeginDisabled();
                    }
                    var green = new Vector4(0.16470588235294117f, 0.7215686274509804f, 0.10980392156862745f, .8f);
                    ImGui.PushStyleColor(ImGuiCol.Button, green);
                    if (ImGui.Button("Challenge Opponent"))
                    {

                        ModBattles.battle.oppenent.Name = playerList[item_current_idx].Name.ToString();
                        ModBattles.battle.oppenent.HomeWorld = playerList[item_current_idx].HomeWorld.ToString();
                        ModBattles.battle.oppenent.Tell = "/t " + playerList[item_current_idx].Name.ToString() + "@" + playerList[item_current_idx].HomeWorld.ToString();
                        //Challenge macro and execute
                        var run = RaptureShellModule.Instance;
                        var macroModule = RaptureMacroModule.Instance;
                        var macro = macroModule->GetMacro(0, 0);
                        string challenge = "/t " + playerList[item_current_idx].Name.ToString() + "@" + playerList[item_current_idx].HomeWorld.ToString() + " Will You Fight Me " + playerList[item_current_idx].Name.ToString() + "|" + ModBattles.battle.you.HomeWorld;

                        var newStr = Utf8String.FromString(challenge);
                        macroModule->ReplaceMacroLines(macro, newStr);
                        newStr->Dtor();
                        IMemorySpace.Free(newStr);
                        run->ExecuteMacro(macro);
                    }
                    try
                    {
                        ImGui.PopStyleColor();
                    }
                    catch (Exception e) { }

                    if (ModBattles.battle.oppenent.Confirmed)
                    {
                        ImGui.EndDisabled();
                    }
                }
                else
                {
                    //PluginLog.Log(logout);
                    List<string> quicklist = new List<string> { "no", "new", "opponents" };
                    string combo_preview_value = "No Opponents to Add";
                    if (ImGui.BeginCombo("Player to Add", combo_preview_value))
                    {

                        for (int n = 0; n < quicklist.Count; n++)
                        {
                            bool is_selected = item_current_idxno == n;

                            if (ImGui.Selectable(quicklist[n], true))
                                item_current_idxno = n;

                            if (is_selected)
                                ImGui.SetItemDefaultFocus();
                        }

                        // Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
                        //
                        ImGui.EndCombo();
                    }

                }

                ImGui.SameLine();
                if (ImGui.Button("Remove Hp Bar and Abilities"))
                {

                    ModBattles.PRemoveMod.Invoke("test", "Defalut", 0);
                    ModBattles.PRemoveMod.Invoke("HpBar", "Default", 0);
                    ModBattles.Predraw.Invoke(Self, RedrawType.Redraw);
                }
                ImGui.NewLine();
                ImGui.Text("Round: " + ModBattles.battle.round);
                {
                    //if (ImGui.Button("Turn On Hp Bard"))
                    //{
                    //    var tD = new Dictionary<string, string>();
                    //    tD.Add("chara/equipment/e6064/vfx/eff/ve0005.avfx", ModBattles.HpBarStart);
                    //    string manip = "";
                    //    PluginLog.Log(ModBattles.PTempMod.Invoke("HpBar", "Default", tD, ModBattles.HpBarManips, 0).ToString());
                    //    ModBattles.Predraw.Invoke(Self, RedrawType.Redraw);

                    //}
                    //ImGui.SameLine();
                    //if (ImGui.Button("Remove Hp Bar"))
                    //{
                    //    ModBattles.PRemoveMod.Invoke("HpBar", "Default", 0);
                    //    ModBattles.Predraw.Invoke(Self, RedrawType.Redraw);

                    //}
                    //ImGui.SameLine();
                    //if (ImGui.Button("A3"))
                    //{
                    //    PluginLog.Log("Current action "+ModBattles.battle.you.CurrentAction.Damage+" o action "+ ModBattles.battle.oppenent.CurrentAction.Damage);

                    //}

                    //ImGui.SameLine();
                    //if (ImGui.Button("Physik"))
                    //{
                    //    var tD = new Dictionary<string, string>();
                    //    tD.Add("chara/action/emote_sp/sp04_no_target.tmb", "chara/action/magic/swl_summon/swlcarel.tmb");
                    //    string manip = "";
                    //    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    //    ModBattles.Predraw.Invoke(Self, RedrawType.Redraw);


                    //}
                }
                if (ModBattles.ActionRecieved && ModBattles.PlayerReady)
                {

                    if (ImGui.Button("Ready for next Round"))
                    {
                        ModBattles.PlayerReady = false; 
                        ModBattles.ActionRecieved = false;

                    }
                }

                ImGui.NewLine();



                //Your Stats
                {
                    ImGui.BeginChild("ChildYou", new Vector2(ImGui.GetContentRegionAvail().X * 0.5f, ImGui.GetContentRegionAvail().Y * 0.9f));
                    if (ModBattles.ActionRecieved)
                    {
                        ImGui.Text("Ready yourself for the next round");
                    }
                    else
                    {
                        if (ModBattles.battle.you.FReady)
                        {
                            ImGui.Text("Action Selected Waiting For Opponent...");
                        }
                        else
                        {
                            if (!ModBattles.battle.oppenent.Confirmed)
                            {
                                ImGui.BeginDisabled();
                            }
                            ImGui.PushStyleColor(ImGuiCol.Button, HealerGreen);
                            if (ImGui.Button("Heal"))
                            {
                                var r = new Random();

                                ModBattles.battle.SetYourAction(1, r.Next(0, 1000), "no", ModBattles.battle.you, "Your action");

                            }
                            try
                            {
                                ImGui.PopStyleColor();
                            }
                            catch (Exception e) { }

                            ImGui.SameLine();
                            ImGui.PushStyleColor(ImGuiCol.Button, Blue);
                            if (ImGui.Button("Defend"))
                            {
                                var r = new Random();

                                ModBattles.battle.SetYourAction(2, r.Next(0, 1000), "no", ModBattles.battle.you, "Your action");
                            }
                            try
                            {
                                ImGui.PopStyleColor();
                            }
                            catch (Exception e) { }
                            ImGui.SameLine();
                            ImGui.PushStyleColor(ImGuiCol.Button, Red);
                            if (ImGui.Button("Attack"))
                            {
                                var r = new Random();

                                ModBattles.battle.SetYourAction(3, r.Next(0, 1000), "no", ModBattles.battle.you, "Your action");
                            }
                            try
                            {
                                ImGui.PopStyleColor();
                            }
                            catch (Exception e) { }

                            if (!ModBattles.battle.oppenent.Confirmed)
                            {
                                ImGui.EndDisabled();
                            }
                        }
                    }
                    ImGui.NewLine();

                    if (!ModBattles.battle.oppenent.Confirmed)
                    {
                        if (!ModBattles.battle.you.ChallengeRecieved)
                        {
                            ImGui.BeginDisabled();

                        }

                        if (ModBattles.battle.you.Challengers.Count > 0)
                        {
                            try
                            {
                                string combo_preview_value = ModBattles.battle.you.Challengers[challenge_index].Item1;
                                if (ImGui.BeginCombo("Player to Add", combo_preview_value))
                                {


                                    var lengthObj = playerList.Count;
                                    for (var i = 0; i < ModBattles.battle.you.Challengers.Count; i++)
                                    {


                                        bool is_selected = challenge_index == i;


                                        if (ImGui.Selectable(ModBattles.battle.you.Challengers[i].Item1, true))
                                            challenge_index = i;


                                        if (is_selected)

                                            ImGui.SetItemDefaultFocus();
                                        // Set the initial focus when opening the combo (scrolling + keyboard navigation focus)

                                    }
                                    ImGui.EndCombo();

                                }
                            }
                            catch (Exception e)
                            {
                                PluginLog.Log(e.ToString());
                                if (challenge_index != 0)
                                {
                                    challenge_index--;

                                }
                                else
                                {
                                    challenge_index = 0;
                                }


                            }

                            {

                            }
                        }
                        else
                        {
                            List<string> quicklist = new List<string> { "no", "new", "challengers" };
                            string combo_preview_value = "No New Challengers";
                            if (ImGui.BeginCombo("Player to Add", combo_preview_value))
                            {

                                for (int n = 0; n < quicklist.Count; n++)
                                {
                                    bool is_selected = item_current_idxno == n;

                                    if (ImGui.Selectable(quicklist[n], true))
                                        item_current_idxno = n;

                                    if (is_selected)
                                        ImGui.SetItemDefaultFocus();
                                }

                                // Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
                                //
                                ImGui.EndCombo();
                            }
                        }


                        if (ImGui.Button("Accept Challenge"))
                        {
                            //PluginLog.Log("test");

                            ModBattles.battle.oppenent.Confirmed = true;
                            ModBattles.battle.oppenent.FReady = false;
                            ModBattles.battle.oppenent.Name = ModBattles.battle.you.Challengers[challenge_index].Item1;
                            ModBattles.battle.oppenent.HomeWorld = ModBattles.battle.you.Challengers[challenge_index].Item2;
                            ModBattles.battle.oppenent.Tell= "/t " + ModBattles.battle.you.Challengers[challenge_index].Item1 + "@" + ModBattles.battle.you.Challengers[challenge_index].Item2;
                            ModBattles.battle.you.Emote = new Tuple<string, string>("chara/action/emote/goodbye.tmb", "/goodbye");
                            ////chara/action/emote/goodbye.tmb /goodbye
                        
                            var run = RaptureShellModule.Instance;
                            var macroModule = RaptureMacroModule.Instance;
                            var macro = macroModule->GetMacro(0, 0);
                            //"/t " + battle.you.Name + "@" + battle.you.HomeWorld;
                            string challenge = "/t " + ModBattles.battle.you.Challengers[challenge_index].Item1 + "@" + ModBattles.battle.you.Challengers[challenge_index].Item2 + " Yes I will fight you.|" + ModBattles.battle.you.HomeWorld;

                            var newStr = Utf8String.FromString(challenge);
                            macroModule->ReplaceMacroLines(macro, newStr);
                            newStr->Dtor();
                            IMemorySpace.Free(newStr);
                            run->ExecuteMacro(macro);

                            var tD = new Dictionary<string, string>();
                            tD.Add("chara/equipment/e6064/vfx/eff/ve0005.avfx", ModBattles.AllHpBars[100]);
                            tD.Add("vfx/common/texture/icon_tex01_t1.atex", ModBattles.HpBarOutLine);
                            tD.Add("vfx/common/texture/icon_tex02_t1.atex", ModBattles.HpBarBar);
                            tD.Add("vfx/common/texture/m0377priz102f_w.atex", ModBattles.Water);

                            PluginLog.Log(ModBattles.PTempMod.Invoke("HpBar", "Default", tD, ModBattles.HpBarManips, 0).ToString());
                            ModBattles.Predraw.Invoke(Self, RedrawType.Redraw);
                        }

                        if (!ModBattles.battle.you.ChallengeRecieved)
                        {
                            ImGui.EndDisabled();
                        }
                    }
                    else
                    {
                        ImGui.Text("Current Opponent: " + ModBattles.battle.oppenent.Name + " from " + ModBattles.battle.oppenent.HomeWorld);
                        ImGui.SameLine();
                        ImGui.PushStyleColor(ImGuiCol.Button, Red);
                        if (ImGui.Button("Clear"))
                        {


                            ModBattles.battle = new Battle();
                            //Start Fight When Ready
                            //ModBattles.battle.OnReadyChanged = (source, Ready) => ModBattles.battle.Fight();

                            //Setting Up Player as A Fighter
                            ModBattles.battle.you.Name = ModBattles.Obj[0].Name.ToString();
                            PlayerCharacter ThisPlayer = (PlayerCharacter)ModBattles.Obj[0];
                            ModBattles.battle.you.HomeWorld = ThisPlayer.HomeWorld.GameData.Name.ToString();
                            ModBattles.battle.you.Tell = "/t " + ModBattles.battle.you.Name + "@" + ModBattles.battle.you.HomeWorld;
                        }
                        ImGui.PopStyleColor();
                    }
                    ImGui.NewLine();
                    this.FighterStatus(ModBattles.battle.you);

                    ImGui.EndChild();
                }
                ImGui.SameLine();
                //Oppenent Stats
                {
                    ImGui.BeginChild("ChildOppenent", new Vector2(ImGui.GetContentRegionAvail().X * 0.5f, ImGui.GetContentRegionAvail().Y * 0.9f));
                    if (!ModBattles.battle.oppenent.Confirmed)
                    {
                        ImGui.Text("Waiting for Opponent...");
                    }
                    else
                    {
                        if (ModBattles.battle.oppenent.FReady)
                        {
                            ImGui.Text("Opponent Has Selected an Action and is Waiting for you...");
                        }
                        else
                        {
                            ImGui.Text("Oppent Is Still Deciding on and Action");
                        }
                        ImGui.NewLine();
                        this.FighterStatus(ModBattles.battle.oppenent);
                    }
                    ImGui.EndChild();
                }
            }
            else
            {
                ImGui.Text("You Need To Be In Dynasty for this Plugin To Work.");
                ImGui.SameLine();
                if (ImGui.Button("Remove Hp Bar and Abilities"))
                {

                    ModBattles.PRemoveMod.Invoke("test", "Defalut", 0);
                    ModBattles.PRemoveMod.Invoke("HpBar", "Default", 0);
                    ModBattles.Predraw.Invoke(Self, RedrawType.Redraw);
                }
            }
        }
        else
        {
            if (ImGui.Button("Remove Hp Bar and Abilities"))
            {

                ModBattles.PRemoveMod.Invoke("test", "Defalut", 0);
                ModBattles.PRemoveMod.Invoke("HpBar", "Default", 0);
                ModBattles.Predraw.Invoke(Self, RedrawType.Redraw);
            }
        }

    }
    public void FighterStatus(Battle.fighter Fighter)
    {

        ImGui.Text(Fighter.Name + ":");
        ImGui.NewLine();
        ImGui.Text("-----------------------------------------------------------");
        ImGui.NewLine();
        ImGui.Text("HP: " + Fighter.Health.ToString() + " / " + 100);
        ImGui.NewLine();
        if (Fighter.Defense.Item2 == 0)
        {
            ImGui.Text("Defense: 100 %%");
        }
        else
        {
            ImGui.Text("Defense: " + (100 + Fighter.Defense.Item1) + "%% for " + Fighter.Defense.Item2.ToString() + " Round(s)");
        }
        ImGui.NewLine();
        if (Fighter.Invuln.Item1 == false)
        {
            ImGui.Text("Invulnerable? : No");
        }
        else
        {
            ImGui.Text("Invulnerable? : Yes For " + Fighter.Invuln.Item2.ToString() + " Round(s)");
        }
        
        

    }
   

   
}
