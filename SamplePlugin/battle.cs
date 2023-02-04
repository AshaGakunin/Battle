using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Lumina.Excel.GeneratedSheets;
using Penumbra.Api.Enums;

namespace ModBattles
{
    public class background
    {
        public async Task test(int wait)
        {
            await test1(wait); 
            ModBattles.battle.NextRound(ModBattles.battle.you);
            ModBattles.battle.NextRound(ModBattles.battle.oppenent);
            ModBattles.battle.sethp(ModBattles.battle.you.Health);
            ModBattles.ActionRecieved = false;
            ModBattles.PlayerReady = true;
            ModBattles.ActionDisabled = false;

        }

        public async Task wrd(int wait, string tell)
        {

        }
        public async Task<bool> test1(int wait)
        {
            Thread.Sleep(wait);
            return true;
        }
        
    }

    public unsafe class Battle : IDisposable
    {


        private bool ready;
        //public Action<Battle, bool> On    ReadyChanged;
        public bool Ready { get; set; }
        public int round { get; set; }
        public fighter you { get; set; }

        public fighter oppenent { get; set; }

        private BackgroundWorker bw;

       

        public void ResetOppoenent()
        {
            oppenent = new fighter();
        }
        
        public Battle(){
            you = new fighter();
            oppenent = new fighter();
            you.Confirmed = true;
            you.FReady = false;
            round = 1;
            you.ChallengeRecieved = false;
            you.Health = 100;
            oppenent.Health = 100;
            ready = false;
            //PluginLog.Log("Oppenent: " + oppenent.Name + " Confirmed: " + oppenent.Confirmed.ToString());
            //Set on main window
            //you.Name = ModBattles.Obj[0].Name.ToString();
            

           
            //ResetOppoenent();

        }

        public void Fight(Battle target)
        {
            //Fight calculation calculating damage
            target.round++;
            target.ready = false;
            var you = ModBattles.battle.you;
            var opponent = ModBattles.battle.oppenent;
            you.FReady = false;
            oppenent.FReady = false;

            var st = "/target <2>";
            
            var waitamount = "<wait.2>";
            var ywaittime = 7000;
            if (you.CurrentAction.Type == 1)
            {
                ywaittime = 8000;
                waitamount = "<wait.3>";
            }
            if (you.CurrentAction.Type == 2 || you.CurrentAction.Type == 3)
            {
                ywaittime = 9000;
                waitamount = "<wait.4>";
            }
            if (you.CurrentAction.Type == 4)
            {   
                ywaittime = 10000;
                waitamount = "<wait.5>";
            }
            var owaittime = 7000;
            var owaitamount = "<wait.3>";
            if (oppenent.CurrentAction.Type == 1)
            {
                owaittime = 8000;
                owaitamount = "<wait.4>";
            }
            if (oppenent.CurrentAction.Type == 2 || oppenent.CurrentAction.Type == 3)
            {
                owaittime = 9000;
                owaitamount = "<wait.5>";
            }
            if (oppenent.CurrentAction.Type == 4)
            {
                owaittime = 10000;
                owaitamount = "<wait.6>";
            }

            if (ywaittime > owaittime)
            {
                owaittime = ywaittime;
            }
            if (you.CurrentAction.Type == oppenent.CurrentAction.Type)
            {
                string actionmessage = "";
                actionmessage=DoAction(you, opponent);
                DoAction(opponent, you);

                if (you.Health > 0)
                {
                    AnnounceAction(st, waitamount, actionmessage,false, you);
                    

                    if (oppenent.Health > 0)
                    {

                        oppenent.CurrentAction = new Action();
                        you.CurrentAction = new Action();
                        //ModBattles.ActionRecieved = false;
                    }

                }
                else
                {
                    AnnounceAction(st, owaitamount, "I lose.", true, you);
                    //code to reset fight


                }

            }
            else if (you.CurrentAction.Type < oppenent.CurrentAction.Type)
            {
                string actionmessage = "";
                actionmessage = DoAction(you, opponent);

                AnnounceAction(st, waitamount, actionmessage, false, you);


                if (oppenent.Health > 0)
                {
                    DoAction(opponent, you);

                    oppenent.CurrentAction = new Action();
                    you.CurrentAction = new Action();
                    //ModBattles.ActionRecieved = false;
                }
                else
                {
                    //code to reset the fight
                }

            }
            else if (you.CurrentAction.Type > oppenent.CurrentAction.Type)
            {
                DoAction(opponent, you);

                if (you.Health > 0)
                {
                    string actionmessage = "";
                    actionmessage = DoAction(you, opponent);


                    AnnounceAction(st, waitamount, actionmessage, false,you);

                    oppenent.CurrentAction = new Action();
                    you.CurrentAction = new Action();
                    //ModBattles.ActionRecieved = false;
                }
                else
                {
                    AnnounceAction(st, owaitamount, "I lose.", true, you);
                    //ModBattles.ActionRecieved = false;
                    //code to reset the fight
                }

            }
            else
            {
                PluginLog.Log("Fuckery is happening");
            }
            background b = new background();
            bw = new BackgroundWorker();
            bw.DoWork += (Battle,a) => b.test(owaittime);
            bw.RunWorkerAsync();
            
            
            //b.test(owaittime).
            //test(owaittime);
            //PluginLog.Log("Test this actually changed");


        }

        public unsafe void AnnounceAction(string target, string wait, string message,bool lose, fighter fighter)
        {
            var run = RaptureShellModule.Instance;
            var macroModule = RaptureMacroModule.Instance;
            var macro = macroModule->GetMacro(0, 0);
            //"/t " + battle.you.Name + "@" + battle.you.HomeWorld;
            string challenge = target + "\n" + wait + "\n"+fighter.Emote.Item2+"<wait.2>\n/sh " + message;
            if(lose)
            {
                challenge = target + "\n" + wait + "\n/sh " + message;
            }
           

            var newStr = Utf8String.FromString(challenge);
            macroModule->ReplaceMacroLines(macro, newStr);
            newStr->Dtor();
            IMemorySpace.Free(newStr);
            run->ExecuteMacro(macro);
        }
        public string DoAction(fighter you, fighter opponent)
        {
            string returnstring= "";
            var t = you.CurrentAction.Type;
            if (t == 0)
            {
                returnstring = you.Name + " Instantly Kills " + oppenent.Name;
                InstaKill(you.CurrentAction.Instakill, oppenent);

            }else if(t == 1)
            {
                Heal(you.CurrentAction.Heal, you);
                returnstring = you.Name + " Heals themself for  " + you.CurrentAction.Heal+" health.<wait.2>\n/sh Reducing their defense for 3 turns";
            }
            else if (t == 2)
            {
                Bolide(new Tuple<bool, int>(you.CurrentAction.Invuln, you.CurrentAction.InvulnTurns), you);
                returnstring = you.Name + " Uses Superboilide making them invunerable for 3 turns.";
            }
            else if(t == 3)
            {
                Defend(new Tuple<int, int>(you.CurrentAction.Defense, you.CurrentAction.DefenseTurns), you);
                returnstring = you.Name + " Defends themself increasing their defense by "+you.CurrentAction.Defense+" %";
            }
            else if(t == 4)
            {
                GetHit(oppenent.CurrentAction.Damage, you, "inside easy");
                returnstring = you.Name + " Attacks " + oppenent.Name + " doing " + you.CurrentAction.Damage + " before defense";
            }
            return returnstring;
        }

        public class fighter
        {
            //private bool fready;
            //public Action<fighter, bool> OnFReadyChanged;
            public bool FReady { get; set; } = false;
            
            public bool ChallengeRecieved { get; set; } = false;

            public List<Tuple<string,string>> Challengers = new List<Tuple<string,string>>();
            public string Name { get; set; }
            public string HomeWorld { get; set; }
            public bool Confirmed { get; set; }=false;
            public Tuple<string, string> Emote { get; set; }
            public string Tell { get; set; }
            public int Health { get; set; } = 100;

            public Action CurrentAction = new Action();
            //int 1 is percentage of defense, int 2 is turns left
            public Tuple<int, int> Defense { get; set; } = new Tuple<int, int>(0, 0);
            public Tuple<bool, int> Invuln { get; set; } = new Tuple<bool, int>(false, 0);
            
           
           

            
           
            public void TargetSelf()
            {
                ModBattles.Target.SetSoftTarget(ModBattles.Obj[0]);
                
            }

            //public void CurrentOppenentAction(string)




        }
        public void Heal(int value, fighter target)
        {
            if(value > 0){
                if (target.Health + value > 100)
                {
                    target.Health = 100;
                }
                else
                {
                    target.Health = target.Health + value;
                }
                Defend(new Tuple<int, int>(-25, 3), target);
            }
            

        }
        public void NextRound(fighter target)
        {
            if (target.Invuln.Item2 > 0)
            {
                if (target.Invuln.Item2 == 1)
                {
                    target.Invuln = new Tuple<bool, int>(false, 0);
                }
                else
                {
                    target.Invuln = new Tuple<bool, int>(true, (target.Invuln.Item2-1));
                }
               
            }
            if ( target.Defense.Item2 > 0)
            {
                if(target.Defense.Item2 == 1)
                {
                    target.Defense = new Tuple<int, int>(0, (target.Defense.Item2 - 1));
                }
                else
                {
                    target.Defense = new Tuple<int, int>(target.Defense.Item1, (target.Defense.Item2 - 1));
                }
                
                
            }
        }
        public void Defend(Tuple<int, int> ability, fighter target)
        {
            if(ability.Item1 != 0)
            {
                PluginLog.Log("old defense "+target.Defense.Item1.ToString() + " " + target.Defense.Item2.ToString());
                int cd = target.Defense.Item1 + ability.Item1;
                if (cd > 60)
                {
                    cd = 60;
                }
                if (cd < -60)
                {
                    cd = -60;
                }
                int cdt = target.Defense.Item2 + ability.Item2;
                if (cdt > 3)
                {
                    cdt = 3;
                }
               
                Tuple<int, int> calculateddefense = new Tuple<int, int>(cd, cdt);
                PluginLog.Log("new defense " + calculateddefense.Item1.ToString()+" "+calculateddefense.Item2.ToString());
                target.Defense = calculateddefense;
            }
           
            

        }
        public void Bolide(Tuple<bool, int> ability, fighter target)
        {
            if (ability.Item1)
            {
                target.Invuln = ability;
                target.Health = 1;
            }
            
        }
        public void GetHit(int value, fighter target, string location)
        {
            if (!target.Invuln.Item1)
            {
                if(target.Health - (value * (1 - (target.Defense.Item1 / 100))) > 0){
                    target.Health = target.Health - (value * (1 - (target.Defense.Item1 / 100)));
                }
                else
                {
                    target.Health = 0;
                }
                //PluginLog.Log(ModBattles.battle.you.Health.ToString() + " your health " + ModBattles.battle.oppenent.Health.ToString() + " o helath " + location);
                //PluginLog.Log(target.Health.ToString() + " - " + (value * (1 - (target.Defense.Item1 / 100))).ToString() + " called from :" + location);
                
            }
        }
        public void InstaKill(bool value, fighter target)
        {
            if (value)
            {
                target.Health = 0;
            }
        }
        public string ActionEncode(string plainText)
        {
            
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public string ActionDecode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            //try
            //{
               
            //}
            //catch
            //{
            //    PluginLog.Log("Error with this string => "+base64EncodedData);
            //    return "false";
            //}
            
            
        }
        public void SetYourAction(int type, int roll, string OppnentAction, fighter target, string location)
        {


            PluginLog.Log("Setting your action " + ModBattles.battle.you.Health + location);

            if (type == 1)
            {

                int heal = 0;
                //assize 10-20 health
                if (roll <= 300)
                {
                    var r = new Random();
                    heal = r.Next(10, 20);
                    var tD = new Dictionary<string, string>();
                    tD.Add(target.Emote.Item1, "chara/action/ability/cnj_white/abl010.tmb");
                    //tD.Add("chara/action/emote_sp/sp04.tmb", "chara/action/ability/cnj_white/abl010.tmb");
                    string manip = "";
                    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);
                    //target.TargetSelf();
                }
                //celestial opposition 20-30 health
                else if (roll <= 700 && roll > 300)
                {
                    var r = new Random();
                    heal = r.Next(20, 30);
                    var tD = new Dictionary<string, string>();
                    //tD.Add("chara/action/emote_sp/sp04.tmb", "chara/action/ability/2gl_astro/abl014.tmb");
                    tD.Add(target.Emote.Item1, "chara/action/ability/2gl_astro/abl014.tmb");
                    string manip = "";
                    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);
                    // target.TargetSelf();
                }
                //panHaima 30-40
                else if (roll <= 701 && roll > 700)
                {
                    var r = new Random();
                    heal = r.Next(30, 40);
                    var tD = new Dictionary<string, string>();
                    //tD.Add("chara/action/emote_sp/sp04.tmb", "chara/action/ability/2ff_sage/abl017.tmb");
                    tD.Add(target.Emote.Item1, "chara/action/ability/2ff_sage/abl017.tmb");
                    string manip = "";
                    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);
                    // target.TargetSelf();
                }
                //neutral sect
                else
                {
                    heal = 100;
                    var tD = new Dictionary<string, string>();
                   // tD.Add("chara/action/emote_sp/sp04.tmb", "chara/action/limitbreak/lbk_astro_lv3.tmb");
                    tD.Add(target.Emote.Item1, "chara/action/limitbreak/lbk_astro_lv3.tmb");
                    string manip = "";
                    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);
                    //target.TargetSelf();

                }

                target.CurrentAction.Type = 1;
                target.CurrentAction.Heal = heal;
                target.CurrentAction.Defense = -25;
                target.CurrentAction.DefenseTurns = 1;
                string sendactonraw = target.CurrentAction.Type.ToString() + "|" + target.CurrentAction.Damage.ToString() + "|" + target.CurrentAction.Defense.ToString() + "|" + target.CurrentAction.DefenseTurns.ToString() + "|" + target.CurrentAction.Heal.ToString() + "|" + target.CurrentAction.Invuln.ToString() + "|" + target.CurrentAction.InvulnTurns.ToString() + "|" + target.CurrentAction.Instakill.ToString();
                string sendactione = ActionEncode(sendactonraw);
                var run = RaptureShellModule.Instance;
                var macroModule = RaptureMacroModule.Instance;
                var macro = macroModule->GetMacro(0, 1);
                string challenge = ModBattles.battle.oppenent.Tell + " A||" + sendactione + "||" + ModBattles.battle.you.HomeWorld;

                var newStr = Utf8String.FromString(challenge);
                macroModule->ReplaceMacroLines(macro, newStr);
                newStr->Dtor();
                IMemorySpace.Free(newStr);
                run->ExecuteMacro(macro);
                target.FReady = true;

            }
            else if (type == 2)
            {
                int defense = 0;
                int defenseturns = 0;
                bool invul = false;
                int invult = 0;
                //Shadow wall 20-30 defense 1-2 turns
                if (roll <= 250)
                {
                    var r = new Random();
                    defense = r.Next(20, 30);
                    defenseturns = r.Next(1,2);
                    target.CurrentAction.Type = 3;
                    var tD = new Dictionary<string, string>();
                    tD.Add(target.Emote.Item1, "chara/action/ability/2sw_dark/abl004.tmb");
                    string manip = "";
                    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);

                    //target.TargetSelf();

                }
                //Vengance 30-40 defense 1-2 turns
                else if (roll <= 600 && roll > 250)
                {
                    var r = new Random();
                    defense = r.Next(30, 40);
                    defenseturns = r.Next(2, 3);
                    target.CurrentAction.Type = 3;
                    var tD = new Dictionary<string, string>();
                    tD.Add(target.Emote.Item1, "chara/action/ability/2ax_warrior/abl010.tmb");
                    string manip = "";
                    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);
                    target.CurrentAction.SelfTarget = false;

                }
                //Shelltron 30-35 2 turns
                else if (roll <= 850 && roll > 600)
                {
                    var r = new Random();
                    defense = r.Next(35, 40);
                    defenseturns = r.Next(2,4);
                    target.CurrentAction.Type = 3;
                    var tD = new Dictionary<string, string>();
                    tD.Add(target.Emote.Item1, "chara/action/ability/swd_knight/abl017.tmb");
                    string manip = "";
                    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);
                    target.CurrentAction.SelfTarget = false;

                }
                else
                {
                    invul = true;
                    invult = 3;
                    target.CurrentAction.Type = 2;

                    var tD = new Dictionary<string, string>();
                    tD.Add(target.Emote.Item1, "chara/action/ability/2gb_bgb/abl007.tmb");
                    string manip = "";
                    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);
                    target.CurrentAction.SelfTarget = false;

                }
                target.CurrentAction.Defense = defense;
                target.CurrentAction.DefenseTurns = defenseturns;
                target.CurrentAction.Invuln = invul;
                target.CurrentAction.InvulnTurns = invult;
                string sendactonraw = target.CurrentAction.Type.ToString() + "|" + target.CurrentAction.Damage.ToString() + "|" + target.CurrentAction.Defense.ToString() + "|" + target.CurrentAction.DefenseTurns.ToString() + "|" + target.CurrentAction.Heal.ToString() + "|" + target.CurrentAction.Invuln.ToString() + "|" + target.CurrentAction.InvulnTurns.ToString() + "|" + target.CurrentAction.Instakill.ToString();
                string sendactione = ActionEncode(sendactonraw);
                var run = RaptureShellModule.Instance;
                var macroModule = RaptureMacroModule.Instance;
                var macro = macroModule->GetMacro(0, 1);
                string challenge = ModBattles.battle.oppenent.Tell + " A||" + sendactione + "||" + ModBattles.battle.you.HomeWorld;

                var newStr = Utf8String.FromString(challenge);
                macroModule->ReplaceMacroLines(macro, newStr);
                newStr->Dtor();
                IMemorySpace.Free(newStr);
                run->ExecuteMacro(macro);
                target.FReady = true;

            }
            else if (type == 3)
            {

                int damage = 0;
                bool instakill = false;
                //refulgant arrow 20-30 damage
                if (roll <= 300)
                {
                    var r = new Random();
                    damage = r.Next(10, 20);
                    target.CurrentAction.Type = 4;
                    var tD = new Dictionary<string, string>();
                    tD.Add(target.Emote.Item1, "chara/action/ws/bt_2bw_emp/ws_s13.tmb");
                    string manip = "";
                    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);
                    target.CurrentAction.SelfTarget = false;
                }
                //Paradox 30-40 defense 1-2 turns
                else if (roll <= 700 && roll > 300)
                {
                    var r = new Random();
                    damage = r.Next(20, 30);
                    target.CurrentAction.Type = 4;
                    var tD = new Dictionary<string, string>();
                    tD.Add(target.Emote.Item1, "chara/action/magic/thm_black/mgc010.tmb");
                    string manip = "";
                    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);
                    target.CurrentAction.SelfTarget = false;
                }
                //Communio 40-50 2 turns
                else if (roll <= 989 && roll > 700)
                {
                    
                    var r = new Random();
                    damage = r.Next(30, 40);
                    target.CurrentAction.Type = 4;
                    var tD = new Dictionary<string, string>();
                    tD.Add(target.Emote.Item1, "chara/action/magic/2km_riaper/mgc002.tmb");
                    string manip = "";
                    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);
                    target.CurrentAction.SelfTarget = false;
                }
                //Samurai lb3 100 damage
                else if (roll <= 999 && roll > 989)
                {
                    damage = 70;
                    target.CurrentAction.Type = 4;
                    var tD = new Dictionary<string, string>();
                    tD.Add(target.Emote.Item1, "chara/action/limitbreak/lbk_samurai_lv3.tmb");
                    string manip = "";
                    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);
                    target.CurrentAction.SelfTarget = false;

                }
                //OG Dote
                else
                {
                    instakill = true;
                    target.CurrentAction.Type = 0;
                    target.CurrentAction.SelfTarget = false;
                    string manip = "";
                    var tD = new Dictionary<string, string>();
                    //tmb
                    //PluginLog.Log(ModBattles.InstaKills[0]["filler"]);
                   
                    tD.Add(target.Emote.Item1, ModBattles.PenumbraDirectory + ("/MBR/instakill.tmb").Replace("/", "\\"));
                    tD.Add("vfx/camera/eff/lbk_drk_lv3.avfx", ModBattles.PenumbraDirectory + ("/MBR/lbk_drk_lv3(swirls changed).avfx").Replace("/", "\\"));
                    tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c0s.avfx", ModBattles.PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c1s(shimery changed).avfx").Replace("/", "\\"));
                    tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c2s.avfx", ModBattles.PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c2s(stupid sword).avfx").Replace("/", "\\"));
                    tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c3s.avfx", ModBattles.PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c3s(better swipe).avfx").Replace("/", "\\"));
                    tD.Add("vfx/limitbreak/lbk_2sw_lv3/texture/gr01as.atex", ModBattles.PenumbraDirectory + ("/MBR/oglogo.atex").Replace("/", "\\"));
                    tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c6s.avfx", ModBattles.PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c6s(white floor).avfx").Replace("/", "\\"));
                    tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c4s.avfx", ModBattles.PenumbraDirectory + ("/MBR/bk_2sw_lv3_c4s(golddrip).avfx").Replace("/", "\\"));
                    tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c5s.avfx", ModBattles.PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c5s(gold puddle).avfx").Replace("/", "\\"));
                    tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c1s.avfx", ModBattles.PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c1s(bigog).avfx").Replace("/", "\\"));
                    tD.Add("vfx/limitbreak/lbk_2sw_lv3/eff/lbk_2sw_lv3_c8s.avfx", ModBattles.PenumbraDirectory + ("/MBR/lbk_2sw_lv3_c8s(bigpattern).avfx").Replace("/", "\\"));
                    //ModBattles.InstaKills[0].Add(target.Emote.Item1, ModBattles.InstaKills[0]["filler"]);

                    PluginLog.Log(ModBattles.PTempMod.Invoke("test", "Default", tD, manip, 0).ToString());
                    ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);

                }
                target.CurrentAction.Damage = damage;
                target.CurrentAction.Instakill = instakill;
                string sendactonraw = target.CurrentAction.Type.ToString() + "|" + target.CurrentAction.Damage.ToString() + "|" + target.CurrentAction.Defense.ToString() + "|" + target.CurrentAction.DefenseTurns.ToString() + "|" + target.CurrentAction.Heal.ToString() + "|" + target.CurrentAction.Invuln.ToString() + "|" + target.CurrentAction.InvulnTurns.ToString() + "|" + target.CurrentAction.Instakill.ToString();
                string sendactione = ActionEncode(sendactonraw);
                
                string challenge = ModBattles.battle.oppenent.Tell + " A||" + sendactione + "||" + ModBattles.battle.you.HomeWorld;
                var run = RaptureShellModule.Instance;
                var macroModule = RaptureMacroModule.Instance;
                var macro = macroModule->GetMacro(0, 1);
                var newStr = Utf8String.FromString(challenge);
                macroModule->ReplaceMacroLines(macro, newStr);
                newStr->Dtor();
                IMemorySpace.Free(newStr);
                run->ExecuteMacro(macro);
               
                target.FReady = true;
            }
            else
            {
                PluginLog.Log("Something fucked up");
            }
            




        }
        public void SetOpponentAction(int type, int roll, string OppnentAction, fighter target, string location)
        {
            PluginLog.Log("Setting opponent action");
            //if (ModBattles.ActionRecieved == true)
           // {
                PluginLog.Log("Setting opponent action" + ModBattles.battle.you.Health + location + ModBattles.ActionRecieved);
                ModBattles.ActionRecieved = true;
                string rawaction = ActionDecode(OppnentAction);
                string[] A = rawaction.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                ModBattles.battle.oppenent.CurrentAction.Type = int.Parse(A[0]);
                ModBattles.battle.oppenent.CurrentAction.Damage = int.Parse(A[1]);
                ModBattles.battle.oppenent.CurrentAction.Defense = int.Parse(A[2], NumberStyles.AllowLeadingSign);
                ModBattles.battle.oppenent.CurrentAction.DefenseTurns = int.Parse(A[3]);
                ModBattles.battle.oppenent.CurrentAction.Heal = int.Parse(A[4]);
                ModBattles.battle.oppenent.CurrentAction.Invuln = bool.Parse(A[5]);
                ModBattles.battle.oppenent.CurrentAction.InvulnTurns = int.Parse(A[6]);
                ModBattles.battle.oppenent.CurrentAction.Instakill = bool.Parse(A[7]);
                ModBattles.battle.oppenent.FReady = true;

                if (ModBattles.battle.oppenent.FReady && ModBattles.battle.you.FReady)
                {
                    PluginLog.Log("Will Fire Once due to opponent " + ModBattles.ActionRecieved);
                    ModBattles.battle.Fight(ModBattles.battle);
                }
                else
                {
                    PluginLog.Log("is not firing opponent action " + ModBattles.ActionRecieved);
                }
           // }
            
        }
        public class Action
        {
            public int Type { get; set; }
            public int Damage { get; set; } = 0;
            public int Defense { get; set; } = 0;
            public int DefenseTurns { get; set; } = 0;
            public int Heal { get; set; } = 0;
            public bool Invuln { get; set; }=false;
            public int InvulnTurns { get; set; } = 0;
            public bool Instakill { get; set; } = false;

            public bool SelfTarget { get; set; } = true;
        }
        
        public void sethp(int hpvalue)
        {
            var tD = new Dictionary<string, string>();
            tD.Add("chara/equipment/e6064/vfx/eff/ve0005.avfx", ModBattles.AllHpBars[hpvalue]);
            tD.Add("vfx/common/texture/icon_tex01_t1.atex", ModBattles.HpBarOutLine);
            tD.Add("vfx/common/texture/icon_tex02_t1.atex", ModBattles.HpBarBar);
            tD.Add("vfx/common/texture/m0377priz102f_w.atex", ModBattles.Water);

            PluginLog.Log(ModBattles.PTempMod.Invoke("HpBar", "Default", tD, ModBattles.HpBarManips, 0).ToString());
            ModBattles.Predraw.Invoke(ModBattles.Obj[0], RedrawType.Redraw);
        }
        public void Dispose() { }

    }
   
   
}
