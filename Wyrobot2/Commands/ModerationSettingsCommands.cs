using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Wyrobot2.Data;
using Wyrobot2.Events;

// ReSharper disable UnusedMember.Global

namespace Wyrobot2.Commands
{
    [Group("moderationsettings")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModerationSettingsCommands : BaseCommandModule
    {
        [Command("automod"), Description("Enable / Disable the auto-moderator. This will not affect existing data."), RequireUserPermissions(Permissions.Administrator)]
        public async Task AutoMod(CommandContext ctx, [Description("Whether to enabled or disable leveling.")] bool value)
        {
            var data = DataContext.GetGuildData(ctx.Guild.Id);
            var oldValue = data.Moderation.AutoModerationEnabled;
            data.Moderation.AutoModerationEnabled = value;
            DataContext.SaveGuildData(data);
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":white_check_mark: Success!")
                .WithDescription($"The auto-moderation has been turned from **{oldValue}** to **{value}**")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }

        [Command("capspercentage"), Description("Changes the minium percentage of caps in a massge for the user to be warned."), RequireUserPermissions(Permissions.Administrator)]
        public async Task CapsPercentage(CommandContext ctx, [Description("This value accept deciamls.")] float value)
        {
            var data = DataContext.GetGuildData(ctx.Guild.Id);
            var oldValue = data.Moderation.CapsPercentage;
            data.Moderation.CapsPercentage = value;
            DataContext.SaveGuildData(data);
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":white_check_mark: Success!")
                .WithDescription($"The caps percentage has been changed from **{oldValue}** to **{value}**")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }
        
        [Command("bannedwords"), Description("Sets the list of banned words. Sending a message containing one will lead to a message deletion and a warn.")]
        public async Task BannedWords(CommandContext ctx, [RemainingText, Description("Use spaces as separators.")] string value)
        {
            var data = DataContext.GetGuildData(ctx.Guild.Id);
            var oldValue = data.Moderation.BannedWords;
            data.Moderation.BannedWords = value.Split(" ");
            DataContext.SaveGuildData(data);
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":white_check_mark: Success!")
                .WithDescription($"The banned words have changed from **||{string.Join(" ", oldValue)}||** to **||{value}||**")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }
        
        [Command("modroles"), Description("Sets a list of moderation roles, These roles will be immune against the auto-moderator.")]
        public async Task ModerationRoles(CommandContext ctx, [Description("Mention one or multiple roles.")] params DiscordRole[] value)
        {
            var data = DataContext.GetGuildData(ctx.Guild.Id);
            var oldValue = data.Moderation.ModerationRoles;
            data.Moderation.ModerationRoles = value.Select(r => r.Id);
            DataContext.SaveGuildData(data);

            var list = new List<DiscordRole>(data.Moderation.ModerationRoles.Count());
            foreach (var r in oldValue)
            {
                try
                {
                    list.Add(ctx.Guild.GetRole(r));
                }
                catch (Exception e)
                {
                    ctx.Client.Logger.LogWarning(EventIds.Warning, e, "Could not get moderation role of ID '{RId}' in guild '{GName}' ({GId})", r, ctx.Guild.Name, ctx.Guild.Id);
                }
            }
            
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":white_check_mark: Success!")
                .WithDescription($"The auto-moderation roles have changed from **{(list.Any() ? string.Join(" ", list.Select(r => r.Mention + " ")) : "None")}** to **{(value.Any() ? string.Join(" ", value.Select(r => r.Mention + " ")) : "None")}**")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }
    }
}