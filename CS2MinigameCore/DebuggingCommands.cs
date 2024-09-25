using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace CS2MinigameCore
{
    public class Debugging
    {
        private CS2MinigameCore m_CSSPlugin;

        private Dictionary<CCSPlayerController, Vector> savedPlayerPos = new();


        public Debugging(CS2MinigameCore plugin)
        {
            m_CSSPlugin = plugin;
            m_CSSPlugin.AddCommand("css_dbg_savepos", "Save current location for teleport", CommandSavePos);
            m_CSSPlugin.AddCommand("css_dbg_restorepos", "Use saved location to teleport", CommandRestorePos);
        }

        private void CommandSavePos(CCSPlayerController? client, CommandInfo info)
        {
            if (client == null)
                return;

            if (!PluginSettings.getInstance.m_CVDebuggingEnabled.Value)
            {
                client.PrintToChat(CS2MinigameCore.MessageWithPrefix("Debugging feature is disabled."));
                return;
            }

            if (client.PlayerPawn.Value == null || client.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            {
                client.PrintToChat(CS2MinigameCore.MessageWithPrefix("You must be alive to use this command."));
                return;
            }

            var playerPawn = client.PlayerPawn.Value;

            if (playerPawn == null)
                return;

            var clientAbsPos = playerPawn.AbsOrigin;

            if (clientAbsPos == null)
            {
                client.PrintToChat(CS2MinigameCore.MessageWithPrefix("Failed to retrieve your position!"));
                return;
            }

            var vector = new Vector(clientAbsPos.X, clientAbsPos.Y, clientAbsPos.Z);

            savedPlayerPos[client] = vector;

            client.PrintToChat(CS2MinigameCore.MessageWithPrefix($"Location saved! {vector.X}, {vector.Y}, {vector.Z}"));
        }

        private void CommandRestorePos(CCSPlayerController? client, CommandInfo info)
        {
            if (client == null)
                return;

            if (!PluginSettings.getInstance.m_CVDebuggingEnabled.Value)
            {
                client.PrintToChat(CS2MinigameCore.MessageWithPrefix("Debugging feature is disabled."));
                return;
            }

            if (client.PlayerPawn.Value == null || client.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            {
                client.PrintToChat(CS2MinigameCore.MessageWithPrefix("You must be alive to use this command."));
                return;
            }

            var playerPawn = client.PlayerPawn.Value;

            if (playerPawn == null)
                return;

            Vector? vector = null;

            if (!savedPlayerPos.TryGetValue(client, out vector) || vector == null)
            {
                client.PrintToChat(CS2MinigameCore.MessageWithPrefix("There is no saved location! save location first!"));
                return;
            }

            client.PrintToChat(CS2MinigameCore.MessageWithPrefix($"Teleported to {vector.X}, {vector.Y}, {vector.Z}"));
            playerPawn.Teleport(vector, playerPawn.AbsRotation);
        }
    }
}