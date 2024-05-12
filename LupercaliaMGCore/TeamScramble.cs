using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace LupercaliaMGCore {
    

    public class TeamScramble
    {
        private LupercaliaMGCore m_CSSPlugin;
        private FakeConVar<bool> m_CVIsScrambleEnabled = new("lp_mg_teamscramble_enabled", "Should team is scrambled after round end", true);

        public TeamScramble(LupercaliaMGCore plugin) {
            m_CSSPlugin = plugin;
        }
        
        public void initialize() {
            m_CSSPlugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
            m_CSSPlugin.RegisterFakeConVars(m_CVIsScrambleEnabled);
        }


        private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
            if(m_CVIsScrambleEnabled.Value) {
                List<CCSPlayerController> players = Utilities.GetPlayers();
                int playerCount = players.Count;
                int playerCountHalf = playerCount/2;
                List<uint> pickedPlayer = new List<uint>();


                var unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                Random random = new Random(unixTimestamp);

                for(int i = playerCountHalf; i > 0; i--) {
                    if(pickedPlayer.Contains(players[i].Index)) {
                        i++;
                        continue;
                    }

                    int index = random.Next(playerCount);
                    pickedPlayer.Add(players[index].Index);
                    players[index].SwitchTeam(CsTeam.CounterTerrorist);
                }

                for(int i = playerCount-1; i >= 0; i--) {
                    if(pickedPlayer.Contains(players[i].Index)) 
                        continue;
                    
                    players[i].SwitchTeam(CsTeam.Terrorist);
                }
            }
            return HookResult.Continue;
        }
    }
}