using System.Timers;
using DSharpPlus;

namespace Wyrobot2.Events
{
    public static class RecurrentEvent
    {
        private static readonly Timer Timer;
        private static DiscordClient _client;

        static RecurrentEvent()
        {
            Timer = new Timer
            {
                Enabled = true,
                AutoReset = true,
                Interval = 60000
            };
            Timer.Elapsed += TimerOnElapsed;
        }

        public static void InitializeAndStart(DiscordClient client)
        {
            _client = client;
            Timer.Start();
        }

        private static void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}