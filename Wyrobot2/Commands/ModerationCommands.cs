using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Wyrobot2.Data;
using Wyrobot2.Data.Models;
// ReSharper disable UnusedMember.Global
// ReSharper disable TemplateIsNotCompileTimeConstantProblem
// ReSharper disable MemberCanBePrivate.Global

namespace Wyrobot2.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModerationCommands : BaseCommandModule
    {
        [Command("ban"), RequirePermissions(Permissions.BanMembers), Priority(2), Description("Permanently bans a member.")]
        public async Task Ban(CommandContext ctx, [Description("Member to ban.")] DiscordMember member, [Description("Duration of the ban.")] TimeSpan expiresIn, [RemainingText, Description("Reason of the ban.")] string reason = "No reason provided.")
        {
            if (!ctx.Member.CanPunish(member))
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Permission denied",
                    Description =
                        $"{DiscordEmoji.FromName(ctx.Client, ":raised_hand:")} You lack permissions necessary to run this command.",
                    Color = new DiscordColor(0xFF0000)
                });
            }
            
            var usrData = DataManager.GetData(member, ctx.Guild) ?? new UserData{ Id = member.Id, GuildId = ctx.Guild.Id };
            usrData.Sanctions ??= new List<Sanction>();
            
            usrData.Sanctions.Add(new Sanction
            {
                Type = Sanction.SanctionType.Ban,
                BannerId = ctx.Member.Id,
                IssuedAt = DateTimeOffset.Now,
                ExpiresAt = expiresIn == TimeSpan.MaxValue ? DateTimeOffset.MaxValue : DateTimeOffset.Now + expiresIn,
                Reason = reason,
                HasBeenUnbanned = false
            });

            try
            {
                await member.SendMessageAsync("You have benn " +
                                              $"{(expiresIn == TimeSpan.MaxValue ? "permanently banned" : $"banned for {(expiresIn > TimeSpan.FromDays(1) ? $"{expiresIn.TotalDays} days" : $"{expiresIn.TotalHours} hours")}")} " +
                                              $"from **{ctx.Guild.Name}** for the following reason: ```{reason}```");
                await ctx.Guild.BanMemberAsync(member, 0, $"'{ctx.Member.Username}#{ctx.Member.Discriminator}' banned '{member.Username}#{member.Discriminator}'. " + reason);
            }
            catch (Exception e)
            {
                ctx.Client.Logger.LogError($"An error occured while trying to ban '{member.Username}#{member.Discriminator}'. Exception: {e}");
                
                await ctx.RespondAsync(new DiscordEmbedBuilder()
                    .WithTitle("Oops! An error occured.")
                    .WithDescription(
                        $"An error occured while trying to ban '{member.Username}#{member.Discriminator}'. Be sure the bot has enough permissions and retry again.")
                    .WithColor(DiscordColor.DarkRed)
                    .WithFooter($"Issuer ID: {ctx.User.Id}")
                    .WithTimestamp(DateTime.UtcNow));
            }
            
            DataManager.SaveData(usrData);
            ctx.Client.Logger.LogInformation($"'{ctx.Member.Username}#{ctx.Member.Discriminator}' banned '{member.Username}#{member.Discriminator}' for the following reason: {reason}.");
            
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":hammer: Success!")
                .WithDescription($"{member.Username}#{member.Discriminator} has been " +
                                 $"{(expiresIn == TimeSpan.MaxValue ? "permanently banned" : $"banned for {(expiresIn > TimeSpan.FromDays(1) ? $"{expiresIn.TotalDays} days" : $"{expiresIn.TotalHours} hours")}")}" +
                                 $"{(reason == null ? null : $" for the following reason: ```{reason}```")}")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithThumbnail(member.AvatarUrl)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow));
        }

        [Command("ban"), RequirePermissions(Permissions.BanMembers), Priority(1)]
        public async Task Ban(CommandContext ctx, [Description("Member to ban.")] DiscordMember member, [RemainingText, Description("Reason of the ban.")] string reason = "No reason provided.") =>
            await Ban(ctx, member, TimeSpan.MaxValue, reason);
    }
}