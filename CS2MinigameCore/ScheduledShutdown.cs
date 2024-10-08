using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;

namespace CS2MinigameCore
{
    public class ScheduledShutdown
    {
        private CS2MinigameCore m_CSSPlugin;
        private CounterStrikeSharp.API.Modules.Timers.Timer shutdownTimer;
        private CounterStrikeSharp.API.Modules.Timers.Timer? warningTimer;
        private bool shutdownAfterRoundEnd = false;

        public ScheduledShutdown(CS2MinigameCore plugin)
        {
            m_CSSPlugin = plugin;

            m_CSSPlugin.AddCommand("css_cancelshutdown", "Cancel the initiated shutdown.", CommandCancelShutdown);
            m_CSSPlugin.AddCommand("css_startshutdown", "Initiate the shutdown.", CommandStartShutdown);

            m_CSSPlugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
            shutdownTimer = m_CSSPlugin.AddTimer(60.0f, () =>
            {
                if (DateTime.Now.ToString("HHmm").Equals(PluginSettings.getInstance.m_CVScheduledShutdownTime.Value))
                {
                    initiateShutdown();
                }
            }, TimerFlags.REPEAT);
        }


        private void initiateShutdown()
        {
            shutdownTimer.Kill();

            if (PluginSettings.getInstance.m_CVScheduledShutdownRoundEnd.Value)
            {
                shutdownAfterRoundEnd = true;
                Server.PrintToChatAll(CS2MinigameCore.MessageWithPrefix(m_CSSPlugin.Localizer["ScheduledShutdown.Notification.AfterRoundEnd"]));
            }
            else
            {
                int time = PluginSettings.getInstance.m_CVScheduledShutdownWarningTime.Value;
                warningTimer = m_CSSPlugin.AddTimer(1.0F, () =>
                {
                    if (time < 1)
                    {
                        m_CSSPlugin.Logger.LogInformation(CS2MinigameCore.MessageWithPrefix("Server is shutting down..."));
                        Server.ExecuteCommand("quit");
                        return;
                    }

                    Server.PrintToChatAll(CS2MinigameCore.MessageWithPrefix(m_CSSPlugin.Localizer["ScheduledShutdown.Notification.Countdown", time]));
                    time--;
                }, TimerFlags.REPEAT);
            }
            SimpleLogging.LogDebug($"[Scheduled Shutdown] Shutdown initiated.");
        }

        private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            if (!shutdownAfterRoundEnd)
                return HookResult.Continue;

            m_CSSPlugin.Logger.LogInformation(CS2MinigameCore.MessageWithPrefix("Server is shutting down..."));
            Server.ExecuteCommand("quit");


            return HookResult.Continue;
        }

        private void cancelShutdown()
        {
            shutdownAfterRoundEnd = false;
            shutdownTimer.Kill();
            warningTimer?.Kill();

            shutdownTimer = m_CSSPlugin.AddTimer(60.0f, () =>
            {
                if (DateTime.Now.ToString("HHmm").Equals(PluginSettings.getInstance.m_CVScheduledShutdownTime))
                {
                    initiateShutdown();
                }
            }, TimerFlags.REPEAT);
            SimpleLogging.LogDebug($"[Scheduled Shutdown] Cancelled shutdown.");
        }

        [RequiresPermissions(@"css/root")]
        private void CommandCancelShutdown(CCSPlayerController? client, CommandInfo info)
        {
            if (client == null)
                return;

            cancelShutdown();
            Server.PrintToChatAll(CS2MinigameCore.MessageWithPrefix(m_CSSPlugin.Localizer["ScheduledShutdown.Notification.CancelShutdown", client.PlayerName]));
        }

        [RequiresPermissions(@"css/root")]
        private void CommandStartShutdown(CCSPlayerController? client, CommandInfo info)
        {
            if (client == null)
                return;

            initiateShutdown();
            Server.PrintToChatAll(CS2MinigameCore.MessageWithPrefix(m_CSSPlugin.Localizer["ScheduledShutdown.Notification.InitiateShutdown", client.PlayerName]));
        }
    }
}