using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace CS2MinigameCore
{
    public partial class CS2MinigameCore : BasePlugin
    {
        public void SetupEntityOutputHook()
        {
            HookEntityOutput("*", "*", AllEntityOutputHook, HookMode.Post);
        }

        public HookResult AllEntityOutputHook(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
        {
            if (caller?.DesignerName == null || !PluginSettings.getInstance.m_CVAllEntityOutputHook.Value) return HookResult.Continue;

            // it seems that these entities are mainly used when sending commands from the map.
            // if there is an edge case, increase the entities to check
            var validEntities = new[] { "logic_auto", "point_servercommand", "func_button" };
            if (!validEntities.Contains(caller.DesignerName)) return HookResult.Continue;

            var cvarList = new Dictionary<(ConVar, string), float>();
            var ent = output.Connections;

            while (ent != null)
            {
                if (ent.TargetInput == "Command")
                {
                    var raw = ent.ValueOverride.Replace("\"", "");
                    var mapCommands = raw.Split(' ', 2); // command should be like a "say bluh" so

                    if (mapCommands.Length > 1)
                    {
                        var cvar = ConVar.Find(mapCommands[0]);

                        if (cvar != null && cvar.Flags.HasFlag(ConVarFlags.FCVAR_CHEAT))
                        {
                            cvarList[(cvar, mapCommands[1])] = delay;
                        }
                    }
                }
                ent = ent.Next;
            }

            if (cvarList.Count > 0)
            {
                SetCvar(cvarList);
            }

            return HookResult.Continue;
        }

        public void SetCvar(Dictionary<(ConVar, string), float> cvarList)
        {
            foreach (((ConVar, string) pair, float delay) in cvarList)
            {
                Action setCvarAction = () =>
                {
                    Console.WriteLine($"[EntityOutputHook] sv_cheats ON");
                    var cheats = ConVar.Find("sv_cheats");
                    cheats?.SetValue(true);

                    ParseAndSet(pair);

                    cheats?.SetValue(false);
                    Console.WriteLine($"[EntityOutputHook] sv_cheats OFF");
                };

                if (delay == 0.0f)
                {
                    Server.NextFrame(setCvarAction);
                }
                else
                {
                    new Timer(delay, setCvarAction);
                }
            }
        }

        public void ParseAndSet((ConVar, string) pair)
        {
            try
            {
                switch (pair.Item1.Type)
                {
                    case ConVarType.Bool:
                        // Parse to bool
                        if (TryParseBool(pair.Item2, out bool boolValue))
                        {
                            pair.Item1.SetValue(boolValue);
                            Console.WriteLine($"[EntityOutputHook] {pair.Item1.Name} = {boolValue}");
                            return;
                        }
                        break;
                    case ConVarType.Float32:
                    case ConVarType.Float64:
                        // Parse to float
                        if (TryParseFloat(pair.Item2, out float floatValue))
                        {
                            pair.Item1.SetValue(floatValue);
                            Console.WriteLine($"[EntityOutputHook] {pair.Item1.Name} = {floatValue}");
                            return;
                        }
                        break;
                    case ConVarType.Int16:
                    case ConVarType.Int32:
                    case ConVarType.Int64:
                        // Parse to int
                        if (TryParseInt(pair.Item2, out int intValue))
                        {
                            pair.Item1.SetValue(intValue);
                            Console.WriteLine($"[EntityOutputHook] {pair.Item1.Name} = {intValue}");
                            return;
                        }
                        break;
                    case ConVarType.UInt16:
                    case ConVarType.UInt32:
                    case ConVarType.UInt64:
                        // Parse to uint
                        if (TryParseUint(pair.Item2, out uint uintValue))
                        {
                            pair.Item1.SetValue(uintValue);
                            Console.WriteLine($"[EntityOutputHook] {pair.Item1.Name} = {uintValue}");
                            return;
                        }
                        break;
                    default:
                        // Default to string if all parsing fails
                        pair.Item1.StringValue = pair.Item2;
                        Console.WriteLine($"[EntityOutputHook] {pair.Item1.Name} = {pair.Item2}");
                        break;
                }
            }
            catch (Exception ex) { Console.WriteLine($"[EntityOutputHook] parse error. {pair.Item1.Name} = {pair.Item2}\n[EntityOutputHook] Raw error: {ex}"); }
        }

        // Utility methods for parsing
        public bool TryParseBool(string value, out bool result)
        {
            if (value == "0" || value == "1")
            {
                result = (value == "1");
                return true;
            }
            result = false;
            return false;
        }

        public bool TryParseInt(string value, out int result)
        {
            return int.TryParse(value, out result);
        }

        public bool TryParseUint(string value, out uint result)
        {
            return uint.TryParse(value, out result);
        }

        public bool TryParseFloat(string value, out float result)
        {
            return float.TryParse(value, out result);
        }
    }
}
