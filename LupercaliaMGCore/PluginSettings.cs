using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore {
    public class PluginSettings {

        private static PluginSettings? settingsInstance;

        private const string CONFIG_FOLDER = "csgo/cfg/lupercalia/";
        private const string CONFIG_FILE = "mgcore.cfg";

        public static PluginSettings getInstance {
            get {
                if(settingsInstance == null)
                    throw new InvalidOperationException("Settings instance is not initialized yet.");
                
                return settingsInstance;
            }
        }

        public FakeConVar<string> m_CVTeamColorCT {get;} = new("lp_mg_teamcolor_ct", "Should team is scrambled after round end", "0, 0, 255");
        public FakeConVar<string> m_CVTeamColorT {get;} = new("lp_mg_teamcolor_t", "Should team is scrambled after round end", "255, 0, 0");
        public FakeConVar<bool> m_CVIsScrambleEnabled {get;} = new("lp_mg_teamscramble_enabled", "Should team is scrambled after round end", true);
        public FakeConVar<int> m_CVMapConfigType {get;} = new ("lp_mg_mapcfg_type", "Map configuration type. 0: disabled, 1: Exact match, 2: Partial Match, 3: Both", 0);

        private LupercaliaMGCore m_CSSPlugin;

        public PluginSettings(LupercaliaMGCore plugin) {
            settingsInstance = this;
            m_CSSPlugin = plugin;
        }

        private void initialize() {
            m_CSSPlugin.RegisterFakeConVars(typeof(ConVar));
        }

        public bool initializeSettings() {
            string configFolder = Path.Combine(Server.GameDirectory, CONFIG_FOLDER);

            if(!Directory.Exists(configFolder)) {
                m_CSSPlugin.Logger.LogError($"{LupercaliaMGCore.PLUGIN_PREFIX} Failed to find the config folder. Trying to generate...");

                Directory.CreateDirectory(configFolder);

                if(!Directory.Exists(configFolder)) {
                    m_CSSPlugin.Logger.LogError($"{LupercaliaMGCore.PLUGIN_PREFIX} Failed to generate the Config folder!");
                    return false;
                }
            }

            string configLocation = Path.Combine(configFolder, CONFIG_FILE);

            if(!File.Exists(configLocation)) {
                m_CSSPlugin.Logger.LogInformation($"{LupercaliaMGCore.PLUGIN_PREFIX} Failed to find the config file. Trying to generate...");

                try {
                    generateCFG(configLocation);
                } catch(Exception e) {
                    m_CSSPlugin.Logger.LogError($"{LupercaliaMGCore.PLUGIN_PREFIX} Failed to generate config file!\n{e.StackTrace}");
                    return false;
                }
                
                m_CSSPlugin.Logger.LogInformation($"{LupercaliaMGCore.PLUGIN_PREFIX} Config file created.");
            }

            Server.ExecuteCommand($"exec {CONFIG_FOLDER}{CONFIG_FILE}");
            return true;
        }

        private void generateCFG(string configPath) {
            StreamWriter config = File.CreateText(configPath);

            config.WriteLine($"{m_CVIsScrambleEnabled.Name} \"{Convert.ToInt32(m_CVIsScrambleEnabled.Value)}\"");
            config.WriteLine($"{m_CVMapConfigType.Name} \"{m_CVMapConfigType.Value}\"");
            config.WriteLine($"{m_CVTeamColorCT.Name} \"{m_CVTeamColorCT.Value}\"");
            config.WriteLine($"{m_CVTeamColorT.Name} \"{m_CVTeamColorT.Value}\"");

            config.Close();
        }
    }
}