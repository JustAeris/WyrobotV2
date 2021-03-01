using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace Wyrobot2.Events
{
    public static class CommandsEvents
    {
        public static void RegisterEvents(this CommandsNextExtension commands)
        {
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
                    await e.Context.RespondAsync(embed.Build());
            };
        }
    }
}