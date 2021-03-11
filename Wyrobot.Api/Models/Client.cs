using DSharpPlus;
using Microsoft.Extensions.Logging;

namespace Wyrobot.Api.Models
{
    public static class Client
    {
        static Client()
        {
            DiscordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = Configuration.DUseProduction ? Configuration.DProduction : Configuration.DDevelopment,
                TokenType = TokenType.Bot,
#if DEBUG
                MinimumLogLevel = LogLevel.Debug,
#else
                MinimumLogLevel = LogLevel.Information,
#endif
                Intents = DiscordIntents.All
            });

            DiscordClient.InitializeAsync();
        }
        
        public static DiscordClient DiscordClient { get; }
    }
}