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
    [Group("welcomesettings")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WelcomeSettingsCommands : BaseCommandModule
    {
        [Command("enabled"), Description("Enable / Disable leveling. This will not affect existing data."), RequireUserPermissions(Permissions.Administrator)]
        public async Task Enabled(CommandContext ctx, bool value)
        {
            var gldData = await DataManager.GetData(ctx.Guild);
            var oldValue = gldData.Welcome.Enabled;
            gldData.Welcome.Enabled = value;
            DataManager.SaveData(gldData);
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":white_check_mark: Success!")
                .WithDescription($"The welcoming system has been turned from **{oldValue}** to **{value}**")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }
        
        [Command("message"), Description("Change the welcome message."), RequireUserPermissions(Permissions.Administrator)]
        public async Task Message(CommandContext ctx, [RemainingText, Description("Message to display when a user joins the server. Use `{user}` to mention the new member.")] string value)
        {
            var data = await DataManager.GetData(ctx.Guild);
            var oldValue = data.Welcome.Message;
            data.Welcome.Message = value;
            DataManager.SaveData(data);
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":white_check_mark: Success!")
                .WithDescription($"The welcome message has been changed from **{oldValue}** to **{value}**")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }
        
        [Command("channel"), Description("Change the welcome channel."), RequireUserPermissions(Permissions.Administrator)]
        public async Task Channel(CommandContext ctx, [RemainingText, Description("Channel to send the welcome message to. Mention a channel.")] DiscordChannel value)
        {
            var data = await DataManager.GetData(ctx.Guild);
            var oldValue = data.Welcome.ChannelId;
            data.Welcome.ChannelId = value.Id;
            DataManager.SaveData(data);
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":white_check_mark: Success!")
                .WithDescription($"The welcome message has been changed from **{(oldValue == 0 ? "None" : ctx.Guild.GetChannel(oldValue) == null ? "None" : ctx.Guild.GetChannel(oldValue).Mention)}** to **{value}**")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }
    }
}