using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;

namespace CS2MinigameCore
{
    public class VoteRoundRestart
    {
        private CS2MinigameCore m_CSSPlugin;
        private List<CCSPlayerController> votedPlayers = new List<CCSPlayerController>();
        private int playersRequiredToRestart = 0;
        private bool isRoundRestarting = false;

        public VoteRoundRestart(CS2MinigameCore plugin)
        {
            m_CSSPlugin = plugin;

            m_CSSPlugin.AddCommand("css_vrr", "Vote round restart command.", CommandVoteRestartRound);
            m_CSSPlugin.RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
        }

        private HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo info)
        {
            isRoundRestarting = false;
            votedPlayers.Clear();
            return HookResult.Continue;
        }
        private void CommandVoteRestartRound(CCSPlayerController? client, CommandInfo info)
        {
            if (client == null)
                return;

            SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] trying to vote for restart round.");
            if (isRoundRestarting)
            {
                SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] Round is already restarting in progress.");
                client.PrintToChat(CS2MinigameCore.MessageWithPrefix(m_CSSPlugin.Localizer["VoteRoundRestart.Command.Notification.AlreadyRestarting"]));
                return;
            }

            if (votedPlayers.Contains(client))
            {
                SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] trying to vote for restart round.");
                client.PrintToChat(CS2MinigameCore.MessageWithPrefix(m_CSSPlugin.Localizer["VoteRoundRestart.Command.Notification.AlreadyVoted"]));
                return;
            }

            votedPlayers.Add(client);
            SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] voted to restart round.");
            playersRequiredToRestart = (int)Math.Ceiling(Utilities.GetPlayers().Count(player => !player.IsBot && !player.IsHLTV) * PluginSettings.getInstance.m_CVVoteRoundRestartThreshold.Value);

            SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] players count: {votedPlayers.Count}, Requires to restart: {playersRequiredToRestart}");
            Server.PrintToChatAll(CS2MinigameCore.MessageWithPrefix(m_CSSPlugin.Localizer["VoteRoundRestart.Notification.PlayerVote", client.PlayerName, votedPlayers.Count(), playersRequiredToRestart]));

            if (votedPlayers.Count < playersRequiredToRestart)
                return;

            InitiateRoundRestart();
        }

        private void InitiateRoundRestart()
        {
            SimpleLogging.LogDebug("[Vote Round Restart] Initiating round restart...");
            isRoundRestarting = true;

            Server.PrintToChatAll(CS2MinigameCore.MessageWithPrefix(m_CSSPlugin.Localizer["VoteRoundRestart.Notification.RoundRestart", PluginSettings.getInstance.m_CVVoteRoundRestartRestartTime.Value]));
            m_CSSPlugin.AddTimer(PluginSettings.getInstance.m_CVVoteRoundRestartRestartTime.Value, () =>
            {
                SimpleLogging.LogDebug("[Vote Round Restart] Restarting round.");
                Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules?.TerminateRound(0.0F, RoundEndReason.RoundDraw);
            }, TimerFlags.STOP_ON_MAPCHANGE);
        }
    }
}