using CounterStrikeSharp.API;

namespace CS2MinigameCore
{
    public static class SimpleLogging
    {
        public static void LogDebug(string information)
        {
            if (PluginSettings.getInstance.m_CVPluginDebugLevel.Value < 1)
            {
                return;
            }
            if (PluginSettings.getInstance.m_CVPluginDebugShowClientConsole.Value)
            {
                foreach (var client in Utilities.GetPlayers())
                {
                    if (!client.IsValid || client.IsBot || client.IsHLTV)
                        continue;

                    client.PrintToConsole("[CS2MGCORE DEBUG] " + information);
                }
            }
            Server.PrintToConsole("[CS2MGCORE DEBUG] " + information);
        }

        public static void LogTrace(string information)
        {
            if (PluginSettings.getInstance.m_CVPluginDebugLevel.Value < 2)
            {
                return;
            }
            if (PluginSettings.getInstance.m_CVPluginDebugShowClientConsole.Value)
            {
                foreach (var client in Utilities.GetPlayers())
                {
                    if (!client.IsValid || client.IsBot || client.IsHLTV)
                        continue;

                    client.PrintToConsole("[CS2MGCORE TRACE] " + information);
                }
            }
            Server.PrintToConsole("[CS2MGCORE TRACE] " + information);
        }
    }
}