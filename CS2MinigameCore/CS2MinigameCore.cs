using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace CS2MinigameCore {
    public class CS2MinigameCore : BasePlugin
    {
        public static readonly string PLUGIN_PREFIX = $" {ChatColors.DarkRed}[{ChatColors.Blue}Minigame{ChatColors.DarkRed}]{ChatColors.Default}";

        public static string MessageWithPrefix(string message)
        {
            return $"{PLUGIN_PREFIX} {message}";
        }

        private static CS2MinigameCore? instance;

        public static CS2MinigameCore getInstance()
        {
            return instance!;
        }

        public override string ModuleName => "CS2 Minigame Core";

        public override string ModuleVersion => "1.0.0";

        public override string ModuleAuthor => "faketuna";

        public override string ModuleDescription => "Provides core MG feature in CS2 with CounterStrikeSharp";


        public override void Load(bool hotReload)
        {
            instance = this;
            new PluginSettings(this);
            Logger.LogInformation("Plugin settings initialized");

            new TeamBasedBodyColor(this);
            Logger.LogInformation("TBBC initialized");

            new DuckFix(this, hotReload);
            Logger.LogInformation("DFix initialized");

            new TeamScramble(this);
            Logger.LogInformation("TeamScramble initialized");

            new VoteMapRestart(this);
            Logger.LogInformation("VoteMapRestart initialized");

            new VoteRoundRestart(this);
            Logger.LogInformation("VoteRoundRestart initialized");

            new RoundEndDamageImmunity(this);
            Logger.LogInformation("RoundEndDamageImmunity initialized");

            new RoundEndWeaponStrip(this);
            Logger.LogInformation("RoundEndWeaponStrip initialized");

            new RoundEndDeathMatch(this);
            Logger.LogInformation("RoundEndDeathMatch initialized");

            new ScheduledShutdown(this);
            Logger.LogInformation("ScheduledShutdown initialized");

            new Respawn(this);
            Logger.LogInformation("Respawn initialized");

            new MapConfig(this);
            Logger.LogInformation("MapConfig initialized");

            new AntiCamp(this, hotReload);
            Logger.LogInformation("Anti Camp initialized");

            new Omikuji(this);
            Logger.LogInformation("Omikuji initialized");

            new Debugging(this);
            Logger.LogInformation("Debugging feature is initialized");

            new MiscCommands(this);
            Logger.LogInformation("misc commands initialized");

            new JoinTeamFix(this);
            Logger.LogInformation("Join team fix initialized");

            new Rocket(this);
            Logger.LogInformation("Rocket initialized");

            SetupPrecache();
        }

        private void SetupPrecache()
        {
            // Precache resources
            RegisterListener<Listeners.OnServerPrecacheResources>((manifest) =>
            {
                manifest.AddResource("C4.ExplodeWarning");
                manifest.AddResource("c4.explode");
                manifest.AddResource("particles/explosions_fx/explosion_basic.vpcf");
            });
        }

        /*
         * Dirty hack for sv_cheats cvar
         */
        [EntityOutputHook("*", "*")]
        public HookResult OnAllEntityEvent(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
        {
            if (caller == null || caller.DesignerName == null) return HookResult.Continue;

            var found = false;
            var cvarList = new Dictionary<(ConVar, String), float>();
            if (caller.DesignerName == "logic_auto" || caller.DesignerName == "point_servercommand" || caller.DesignerName == "func_button")
            {
                var ent = output.Connections;
                while (ent != null)
                {
                    if (ent.TargetInput == "Command")
                    {
                        // Console.WriteLine($"[EntityOutputHook] Called OnPressed (Caller: {caller.DesignerName}, Desc: {ent.TargetDesc}, Value: {ent.ValueOverride}, Type: {ent.TargetType}, Input: {ent.TargetInput})");
                        var raw = ent.ValueOverride.Replace("\"", "");
                        var mapCommands = raw.Split(" ");
                        // command should be like a "say bluh" so..
                        if (mapCommands.Length > 1)
                        {
                            var cvar = ConVar.Find(mapCommands.First());
                            if (cvar != null)
                            {
                                if (cvar.Flags.HasFlag(ConVarFlags.FCVAR_CHEAT))
                                {
                                    found = true;
                                    var tuple = (cvar, string.Join("", mapCommands.Skip(1)));
                                    cvarList[tuple] = delay;
                                }
                            };
                        }
                    }
                    ent = ent.Next;
                }
            }

            if (found)
            {
                SetCvar(cvarList);
            }

            return HookResult.Continue;
        }

        private void SetCvar(Dictionary<(ConVar, String), float> cvarList)
        {
            foreach (((ConVar, String) pair, float delay) in cvarList)
            {
                if (delay == 0.0) {
                    Server.NextFrame(() => {
                        Console.WriteLine($"[EntityOutputHook] sv_cheats ON");
                        var cheats = ConVar.Find("sv_cheats");
                        cheats?.SetValue(true);
                        ParseAndSet(pair);
                        cheats?.SetValue(false);
                        Console.WriteLine($"[EntityOutputHook] sv_cheats OFF");
                    });
                } else
                {
                    new Timer(delay, () =>
                    {
                        Console.WriteLine($"[EntityOutputHook] sv_cheats ON");
                        var cheats = ConVar.Find("sv_cheats");
                        cheats?.SetValue(true);
                        ParseAndSet(pair);
                        cheats?.SetValue(false);
                        Console.WriteLine($"[EntityOutputHook] sv_cheats OFF");
                    });
                }
            }
        }

        private void ParseAndSet((ConVar, String) pair)
        {
            if (pair.Item2 == "0" || pair.Item2 == "1")
            {
                try
                {
                    var FLAG = pair.Item2 == "1";
                    pair.Item1.SetValue(FLAG);
                    Console.WriteLine($"[EntityOutputHook] {pair.Item1.Name} = {FLAG}");
                }
                catch
                {
                    try
                    {
                        var parsed = int.Parse(pair.Item2);
                        pair.Item1.SetValue(parsed);
                        Console.WriteLine($"[EntityOutputHook] {pair.Item1.Name} = {parsed}");
                    }
                    catch
                    {
                        pair.Item1.StringValue = pair.Item2;
                        Console.WriteLine($"[EntityOutputHook] {pair.Item1.Name} = {pair.Item2}");
                    }
                }
            }
            else
            {
                try
                {
                    var parsed = float.Parse(pair.Item2);
                    pair.Item1.SetValue(parsed);
                    Console.WriteLine($"[EntityOutputHook] {pair.Item1.Name} = {parsed}");
                }
                catch
                {
                    try
                    {
                        var parsed = uint.Parse(pair.Item2);
                        pair.Item1.SetValue(parsed);
                        Console.WriteLine($"[EntityOutputHook] {pair.Item1.Name} = {parsed}");
                    }
                    catch
                    {
                        try
                        {
                            var parsed = int.Parse(pair.Item2);
                            pair.Item1.SetValue(parsed);
                            Console.WriteLine($"[EntityOutputHook] {pair.Item1.Name} = {parsed}");
                        }
                        catch
                        {
                            pair.Item1.StringValue = pair.Item2;
                            Console.WriteLine($"[EntityOutputHook] {pair.Item1.Name} = {pair.Item2}");
                        }
                    }
                }
            }
        }
    }
}
