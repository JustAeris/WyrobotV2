using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Wyrobot2.Commands;
using Wyrobot2.Data;
using Wyrobot2.Data.Models;

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

            _client.GuildCreated += (_, args) =>
            {
                DataManager<GuildData>.SaveData(new GuildData
                {
                    Id = args.Guild.Id
                });
                return Task.CompletedTask;
            };
            
            var commands = _client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] {"wyrobot!"},
                IgnoreExtraArguments = true,
                EnableMentionPrefix = false,
                PrefixResolver = ResolvePrefixAsync
            });

            commands.CommandExecuted += (_, e) =>
            {
                e.Context.Client.Logger.LogInformation(
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    $"User '{e.Context.User.Username}#{e.Context.User.Discriminator}' ({e.Context.User.Id}) executed '{e.Command.QualifiedName}' in #{e.Context.Channel.Name} ({e.Context.Channel.Id})",
                    DateTime.Now);
                return Task.CompletedTask;
            };

            commands.CommandErrored += async (_, e) =>
            {
                e.Context.Client.Logger.LogError(
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    $"User '{e.Context.User.Username}#{e.Context.User.Discriminator}' ({e.Context.User.Id}) tried to execute '{e.Command?.QualifiedName ?? "<unknown command>"}' "
                    + $"in #{e.Context.Channel.Name} ({e.Context.Channel.Id}) and failed with {e.Exception.GetType()}: {e.Exception.Message}",
                    DateTime.Now);
                DiscordEmbedBuilder embed = null;

                var ex = e.Exception;
                while (ex is AggregateException)
                    ex = ex.InnerException;

                switch (ex)
                {
                    case CommandNotFoundException:
                        break; // ignore
                    case ChecksFailedException cfe:
                    {
                        if (!cfe.FailedChecks.Any(x => x is RequirePrefixesAttribute))
                            embed = new DiscordEmbedBuilder
                            {
                                Title = "Permission denied",
                                Description =
                                    $"{DiscordEmoji.FromName(e.Context.Client, ":raised_hand:")} You lack permissions necessary to run this command.",
                                Color = new DiscordColor(0xFF0000)
                            };
                        break;
                    }
                    default:
                        embed = new DiscordEmbedBuilder
                        {
                            Title = "A problem occured while executing the command",
                            Description =
                                $"{Formatter.InlineCode(e.Command?.QualifiedName)} threw an exception: `{ex?.GetType()}: {ex?.Message}`",
                            Color = new DiscordColor(0xFF0000)
                        };
                        break;
                }

                if (embed != null)
                    await e.Context.RespondAsync("", embed: embed.Build());
            };
            
            commands.RegisterCommands<SettingsCommands>();

            _client.UseInteractivity(new InteractivityConfiguration
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });
            
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

            var data = DataManager<GuildData>.GetData(new GuildData(), gld.Id.ToString());

            return msg.Content.StartsWith(data.Prefix, StringComparison.InvariantCultureIgnoreCase) ? Task.FromResult(data.Prefix.Length) : Task.FromResult(-1);
        }
    }
}