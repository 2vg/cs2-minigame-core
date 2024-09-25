using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace CS2MinigameCore
{
    public class PlayerFreezeEvent : OmikujiEvent
    {
        public string eventName => "Player Freeze Event";

        public OmikujiType omikujiType => OmikujiType.EVENT_BAD;

        public OmikujiCanInvokeWhen omikujiCanInvokeWhen => OmikujiCanInvokeWhen.PLAYER_ALIVE;

        public void execute(CCSPlayerController client)
        {
            SimpleLogging.LogDebug("Player drew a omikuji and invoked Player freeze event");

            if (client.PlayerPawn.Value == null || client.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            {
                SimpleLogging.LogDebug("Player freeze event failed due to player is died. But this is should not be happened.");
                return;
            }

            CCSPlayerPawn? playerPawn = client.PlayerPawn.Value;

            if (playerPawn == null)
            {
                SimpleLogging.LogDebug("Player freeze event failed due to playerPawn is null.");
                return;
            }


            playerPawn.MoveType = MoveType_t.MOVETYPE_OBSOLETE;
            playerPawn.ActualMoveType = MoveType_t.MOVETYPE_OBSOLETE;
            SimpleLogging.LogDebug("Player freeze event: Move type changed to MOVETYPE_OBSOLETE");

            Server.PrintToChatAll($"{Omikuji.CHAT_PREFIX} {Omikuji.GetOmikujiLuckMessage(omikujiType, client)} {CS2MinigameCore.getInstance().Localizer["Omikuji.BadEvent.PlayerFreezeEvent.Notification.Froze", client.PlayerName]}");



            CS2MinigameCore.getInstance().AddTimer(PluginSettings.getInstance.m_CVOmikujiEventPlayerFreeze.Value, () =>
            {
                playerPawn.MoveType = MoveType_t.MOVETYPE_WALK;
                playerPawn.ActualMoveType = MoveType_t.MOVETYPE_WALK;
                SimpleLogging.LogDebug("Player freeze event: Move type changed to MOVETYPE_WALK");
                client.PrintToChat($"{Omikuji.CHAT_PREFIX} {CS2MinigameCore.getInstance().Localizer["Omikuji.BadEvent.PlayerFreezeEvent.Notification.UnFroze"]}");
            });
        }

        public void initialize() { }

        public double getOmikujiWeight()
        {
            return PluginSettings.getInstance.m_CVOmikujiEventPlayerFreezeSelectionWeight.Value;
        }
    }
}