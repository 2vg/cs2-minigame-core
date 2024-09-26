using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2MinigameCore
{


    public class JoinTeamFix
    {
        private CS2MinigameCore m_CSSPlugin;

        private static Random random = new Random();

        List<SpawnPoint> spawnPointsT = new();
        List<SpawnPoint> spawnPointsCT = new();

        public JoinTeamFix(CS2MinigameCore plugin)
        {
            m_CSSPlugin = plugin;

            m_CSSPlugin.AddCommandListener("jointeam", JoinTeamListener);
            m_CSSPlugin.RegisterListener<Listeners.OnMapStart>((mapName) =>
            {
                m_CSSPlugin.AddTimer(0.1F, () =>
                {
                    spawnPointsT = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist").ToList();
                    spawnPointsCT = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist").ToList();
                });
            });
        }


        private HookResult JoinTeamListener(CCSPlayerController? client, CommandInfo info)
        {
            if (client == null)
                return HookResult.Continue;


            if (info.ArgCount >= 0)
                return HookResult.Continue;

            string teamArg = info.GetArg(1);

            if (!int.TryParse(teamArg, out int teamID))
                return HookResult.Continue;


            if (teamID >= (int)CsTeam.Spectator && teamID <= (int)CsTeam.CounterTerrorist)
            {
                CsTeam team = (CsTeam)Enum.ToObject(typeof(CsTeam), teamID);

                if (teamID == (int)CsTeam.Terrorist && spawnPointsT.Count() <= 0)
                    return HookResult.Continue;

                if (teamID == (int)CsTeam.CounterTerrorist && spawnPointsCT.Count() <= 0)
                    return HookResult.Continue;



                SimpleLogging.LogDebug($"Player {client.PlayerName}'s team forcefully changed to team {team}");
                client.SwitchTeam(team);
            }
            return HookResult.Continue;
        }
    }
}