using CounterStrikeSharp.API.Modules.Utils;

namespace CS2MinigameCore
{
    public class TimedPosition
    {
        public readonly double time;
        public readonly Vector vector;

        public TimedPosition(double time, Vector vector)
        {
            this.time = time;
            this.vector = vector;
        }
    }
}