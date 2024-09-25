using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

namespace CS2MinigameCore
{
    public class GravityChangeEvent : OmikujiEvent
    {

        public string eventName => "Gravity Change Event";

        public OmikujiType omikujiType => OmikujiType.EVENT_BAD;

        public OmikujiCanInvokeWhen omikujiCanInvokeWhen => OmikujiCanInvokeWhen.ANYTIME;

        private static bool isInGravityChangeEvent = false;

        private static Random random = OmikujiEvents.random;

        public void execute(CCSPlayerController client)
        {
            SimpleLogging.LogDebug("Player drew a omikuji and invoked Gravity change event");

            int randomGravity = random.Next(
                PluginSettings.getInstance.m_CVOmikujiEventGravityMin.Value,
                PluginSettings.getInstance.m_CVOmikujiEventGravityMax.Value
            );

            string msg;

            if (isInGravityChangeEvent)
            {
                msg = $"{Omikuji.CHAT_PREFIX} {Omikuji.GetOmikujiLuckMessage(omikujiType, client)} {CS2MinigameCore.getInstance().Localizer["Omikuji.BadEvent.GravityChangeEvent.Notification.AnotherEventOnGoing"]}";
            }
            else
            {
                msg = $"{Omikuji.CHAT_PREFIX} {Omikuji.GetOmikujiLuckMessage(omikujiType, client)} {CS2MinigameCore.getInstance().Localizer["Omikuji.BadEvent.GravityChangeEvent.Notification.GravityChanged", randomGravity]}";
            }

            foreach (CCSPlayerController cl in Utilities.GetPlayers())
            {
                if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                    continue;

                cl.PrintToChat(msg);
            }

            if (isInGravityChangeEvent)
                return;

            isInGravityChangeEvent = true;
            ConVar? sv_gravity = ConVar.Find("sv_gravity");

            float oldGravity = sv_gravity!.GetPrimitiveValue<float>();

            sv_gravity.SetValue((float)randomGravity);

            float TIMER_INTERVAL_PLACE_HOLDER = PluginSettings.getInstance.m_CVOmikujiEventGravityRestoreTime.Value;

            CS2MinigameCore.getInstance().AddTimer(TIMER_INTERVAL_PLACE_HOLDER, () =>
            {
                sv_gravity.SetValue(oldGravity);
                foreach (CCSPlayerController cl in Utilities.GetPlayers())
                {
                    if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                        continue;

                    cl.PrintToChat($"{Omikuji.CHAT_PREFIX} {Omikuji.GetOmikujiLuckMessage(omikujiType, client)} {CS2MinigameCore.getInstance().Localizer["Omikuji.BadEvent.GravityChangeEvent.Notification.GravityRestored", oldGravity]}");
                    isInGravityChangeEvent = false;
                }
            });
        }

        public void initialize() { }

        public double getOmikujiWeight()
        {
            return PluginSettings.getInstance.m_CVOmikujiEventGravitySelectionWeight.Value;
        }
    }
}