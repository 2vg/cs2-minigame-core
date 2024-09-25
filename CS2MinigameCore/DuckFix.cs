using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace CS2MinigameCore
{


    public class DuckFix
    {
        private CS2MinigameCore m_CSSPlugin;

        public DuckFix(CS2MinigameCore plugin, bool hotReload)
        {
            m_CSSPlugin = plugin;
            m_CSSPlugin.RegisterListener<Listeners.OnTick>(() =>
            {
                foreach (CCSPlayerController client in Utilities.GetPlayers())
                {
                    if (!client.IsValid || client.IsBot || client.IsHLTV)
                        continue;

                    if ((client.Buttons & PlayerButtons.Duck) == 0)
                        continue;

                    CCSPlayerPawn? playerPawn = client.PlayerPawn.Value;

                    if (playerPawn == null)
                        continue;

                    CPlayer_MovementServices? pmService = playerPawn.MovementServices;

                    if (pmService == null)
                        continue;

                    CCSPlayer_MovementServices movementServices = new CCSPlayer_MovementServices(pmService.Handle);
                    if (movementServices != null)
                    {
                        movementServices.LastDuckTime = 0.0f;
                        movementServices.DuckSpeed = 8.0f;
                    }

                }
            });
        }
    }
}