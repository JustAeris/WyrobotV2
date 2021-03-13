using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Wyrobot2.Commands;
using Wyrobot2.Data;
using Wyrobot2.Events;

namespace Wyrobot2
{
    internal static class Program
    {
        private static DiscordClient _client;

        private static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            _client = new DiscordClient(new DiscordConfiguration
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

            _client.UseInteractivity(new InteractivityConfiguration
            {
                PollBehaviour = PollBehaviour.DeleteEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });
            
            _client.RegisterEvents();

            var commands = _client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] {"wyrobot!"},
                IgnoreExtraArguments = true,
                EnableMentionPrefix = false,
                PrefixResolver = ResolvePrefixAsync
            });
            
            commands.RegisterEvents();
            commands.RegisterConverter(new BoolConverter());
            commands.SetHelpFormatter<CustomHelpFormatter>();

            commands.RegisterCommands<SettingsCommands>();
            commands.RegisterCommands<LevelingCommands>();
            commands.RegisterCommands<LevelingSettingsCommands>();
            commands.RegisterCommands<LevelRewardsSettingsCommands>();
            commands.RegisterCommands<ModerationCommands>();
            
            RecurrentEvent.InitializeAndStart(_client);
            
            await _client.ConnectAsync(new DiscordActivity
            {
                Name = "with code",
                ActivityType = ActivityType.Playing
            });

            await Task.Delay(-1);
        }
        
        private static Task<int> ResolvePrefixAsync(DiscordMessage msg)
        {
            var gld = msg.Channel.Guild;
            if (gld == null)
                return Task.FromResult(-1);

            var data = DataManager.GetData(gld);

            return msg.Content.StartsWith(data.Prefix, StringComparison.InvariantCultureIgnoreCase) ? Task.FromResult(data.Prefix.Length) : Task.FromResult(-1);
        }
    }
}