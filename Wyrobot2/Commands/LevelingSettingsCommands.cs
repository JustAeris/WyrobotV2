using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Wyrobot2.Data;
// ReSharper disable UnusedMember.Global

namespace Wyrobot2.Commands
{
    [Group("levelsettings")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LevelingSettingsCommands : BaseCommandModule
    {
        [Command("multiplier"), Description("Change the global XP multiplier."), RequireUserPermissions(Permissions.Administrator)]
        public async Task Multiplier(CommandContext ctx, [Description("Multiplier to apply.")] float value)
        {
            var data = DataContext.GetGuildData(ctx.Guild.Id);
            var oldValue = data.Leveling.Multiplier;
            data.Leveling.Multiplier = value;
            DataContext.SaveGuildData(data);
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":white_check_mark: Success!")
                .WithDescription($"The leveling multiplier has been updated from **{oldValue}** to **{value}**")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }

        [Command("enable"), Description("Enable / Disable leveling. This will not affect existing data."), RequireUserPermissions(Permissions.Administrator)]
        public async Task Enable(CommandContext ctx, [Description("Whether to enabled or disable leveling.")] bool value)
        {
            var data = DataContext.GetGuildData(ctx.Guild.Id);
            var oldValue = data.Leveling.Enabled;
            data.Leveling.Enabled = value;
            DataContext.SaveGuildData(data);
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":white_check_mark: Success!")
                .WithDescription($"The leveling has been turned from **{oldValue}** to **{value}**")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }
        
        [Command("message"), Description("Change the level-up message."), RequireUserPermissions(Permissions.Administrator)]
        public async Task Message(CommandContext ctx, [RemainingText, Description("Message to display when the user level up. Use `{user}` and `{level}` to represent these values.")] string value)
        {
            var data = DataContext.GetGuildData(ctx.Guild.Id);
            var oldValue = data.Leveling.Message;
            data.Leveling.Message = value;
            DataContext.SaveGuildData(data);
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":white_check_mark: Success!")
                .WithDescription($"The leveling message has been changed from **{oldValue}** to **{value}**")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }
    }
}