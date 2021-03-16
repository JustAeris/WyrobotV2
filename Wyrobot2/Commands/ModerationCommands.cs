using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Wyrobot2.Data;
using Wyrobot2.Data.Models;
using Wyrobot2.Events;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Wyrobot2.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModerationCommands : BaseCommandModule
    {
        [Command("ban"), RequirePermissions(Permissions.BanMembers), Priority(2), Description("Bans a member.")]
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
                }
                    .Build());
                return;
            }
            
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (!bot.CanPunish(member))
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Permission denied",
                    Description =
                        $"{DiscordEmoji.FromName(ctx.Client, ":raised_hand:")} The bot needs a higher role to run this command.",
                    Color = new DiscordColor(0xFF0000)
                }
                    .Build());
                return;
            }

            var usrData = DataManager.GetData(member, ctx.Guild) ?? new UserData{ Id = member.Id, GuildId = ctx.Guild.Id };
            usrData.Sanctions ??= new List<Sanction>();
            
            usrData.Sanctions.Add(new Sanction
            {
                Type = Sanction.SanctionType.Ban,
                PunisherId = ctx.Member.Id,
                IssuedAt = DateTimeOffset.Now,
                ExpiresAt = expiresIn == TimeSpan.MaxValue ? DateTimeOffset.MaxValue : DateTimeOffset.Now + expiresIn,
                Reason = reason
            });

            try
            {
                await member.SendMessageAsync("You have been " +
                                              $"{(expiresIn == TimeSpan.MaxValue ? "permanently banned" : $"banned for {(expiresIn > TimeSpan.FromDays(1) ? $"{expiresIn.TotalDays} days" : $"{expiresIn.TotalHours} hours")}")} " +
                                              $"from **{ctx.Guild.Name}** for the following reason: ```{reason}```");
                await ctx.Guild.BanMemberAsync(member, 0, $"'{ctx.Member.Username}#{ctx.Member.Discriminator}' banned '{member.Username}#{member.Discriminator}'. " + reason);
            }
            catch (Exception e)
            {
                ctx.Client.Logger.LogError(EventIds.Error, e, "An error occured while trying to ban '{Username}#{Discriminator}'", member.Username, member.Discriminator);
                
                await ctx.RespondAsync(new DiscordEmbedBuilder()
                    .WithTitle("Oops! An error occured.")
                    .WithDescription(
                        $"An error occured while trying to ban '{member.Username}#{member.Discriminator}'. Be sure the bot has enough permissions and retry again.")
                    .WithColor(DiscordColor.DarkRed)
                    .WithFooter($"Issuer ID: {ctx.User.Id}")
                    .WithTimestamp(DateTime.UtcNow)
                    .Build());
            }
            
            DataManager.SaveData(usrData);
            ctx.Client.Logger.LogInformation(EventIds.Ban , "'{PunisherName}#{PunisherDiscriminator}' banned '{SanctionedName}#{SanctionedDiscriminator}' for the following reason: {Reason}", ctx.Member.Username, ctx.Member.Discriminator, member.Username, member.Discriminator, reason);
            
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":hammer: Success!")
                .WithDescription($"{member.Username}#{member.Discriminator} has been " +
                                 $"{(expiresIn == TimeSpan.MaxValue ? "permanently **banned**" : $"**banned** for {(expiresIn > TimeSpan.FromDays(1) ? $"{expiresIn.TotalDays} days" : $"{expiresIn.TotalHours} hours")}")}" +
                                 $"{(reason == null ? null : $" for the following reason: ```{reason}```")}")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithThumbnail(member.AvatarUrl)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }

        [Command("ban"), RequirePermissions(Permissions.BanMembers), Priority(1)]
        public async Task Ban(CommandContext ctx, [Description("Member to ban.")] DiscordMember member, [RemainingText, Description("Reason of the ban.")] string reason = "No reason provided.") =>
            await Ban(ctx, member, TimeSpan.MaxValue, reason);
        
        [Command("kick"), RequirePermissions(Permissions.KickMembers), Description("Kicks a member.")]
        public async Task Kick(CommandContext ctx, [Description("Member to kick.")] DiscordMember member, [RemainingText, Description("Reason of the kick.")] string reason = "No reason provided.")
        {
            if (!ctx.Member.CanPunish(member))
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Permission denied",
                    Description =
                        $"{DiscordEmoji.FromName(ctx.Client, ":raised_hand:")} You lack permissions necessary to run this command.",
                    Color = new DiscordColor(0xFF0000)
                }
                    .Build());
                return;
            }
            
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (!bot.CanPunish(member))
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Permission denied",
                    Description =
                        $"{DiscordEmoji.FromName(ctx.Client, ":raised_hand:")} The bot needs a higher role to run this command.",
                    Color = new DiscordColor(0xFF0000)
                }
                    .Build());
                return;
            }
            
            var usrData = DataManager.GetData(member, ctx.Guild) ?? new UserData{ Id = member.Id, GuildId = ctx.Guild.Id };
            usrData.Sanctions ??= new List<Sanction>();
            
            usrData.Sanctions.Add(new Sanction
            {
                Type = Sanction.SanctionType.Kick,
                PunisherId = ctx.Member.Id,
                IssuedAt = DateTimeOffset.Now,
                ExpiresAt = DateTimeOffset.Now,
                Reason = reason
            });

            try
            {
                await member.SendMessageAsync($"You have been kicked from **{ctx.Guild.Name}** for the following reason: ```{reason}```");
                await member.RemoveAsync($"'{ctx.Member.Username}#{ctx.Member.Discriminator}' kicked '{member.Username}#{member.Discriminator}'. " + reason);
            }
            catch (Exception e)
            {
                ctx.Client.Logger.LogError(EventIds.Error, e, "An error occured while trying to kick '{Username}#{Discriminator}'", member.Username, member.Discriminator);
                
                await ctx.RespondAsync(new DiscordEmbedBuilder()
                    .WithTitle("Oops! An error occured.")
                    .WithDescription(
                        $"An error occured while trying to ban '{member.Username}#{member.Discriminator}'. Be sure the bot has enough permissions and retry again.")
                    .WithColor(DiscordColor.DarkRed)
                    .WithFooter($"Issuer ID: {ctx.User.Id}")
                    .WithTimestamp(DateTime.UtcNow)
                    .Build());
            }
            
            DataManager.SaveData(usrData);
            ctx.Client.Logger.LogInformation(EventIds.Kick, "'{PunisherName}#{PunisherDiscriminator}' kicked '{SanctionedName}#{SanctionedDiscriminator}' for the following reason: {Reason}", ctx.Member.Username, ctx.Member.Discriminator, member.Username, member.Discriminator, reason);
            
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":hammer: Success!")
                .WithDescription($"{member.Username}#{member.Discriminator} has been **kicked**{(reason == null ? null : $" for the following reason: ```{reason}```")}")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithThumbnail(member.AvatarUrl)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }
        
        [Command("warn"), RequirePermissions(Permissions.ManageMessages), Description("Warns a member.")]
        public async Task Warn(CommandContext ctx, [Description("Member to warn.")] DiscordMember member, [RemainingText, Description("Reason of the warn.")] string reason = "No reason provided.")
        {
            if (!ctx.Member.CanPunish(member))
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Permission denied",
                    Description =
                        $"{DiscordEmoji.FromName(ctx.Client, ":raised_hand:")} You lack permissions necessary to run this command.",
                    Color = new DiscordColor(0xFF0000)
                }
                    .Build());
                return;
            }
            
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (!bot.CanPunish(member))
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Permission denied",
                    Description =
                        $"{DiscordEmoji.FromName(ctx.Client, ":raised_hand:")} The bot needs a higher role to run this command.",
                    Color = new DiscordColor(0xFF0000)
                }
                    .Build());
                return;
            }
            
            var usrData = DataManager.GetData(member, ctx.Guild) ?? new UserData{ Id = member.Id, GuildId = ctx.Guild.Id };
            usrData.Sanctions ??= new List<Sanction>();
            
            usrData.Sanctions.Add(new Sanction
            {
                Type = Sanction.SanctionType.Warn,
                PunisherId = ctx.Member.Id,
                IssuedAt = DateTimeOffset.Now,
                ExpiresAt = DateTimeOffset.Now + TimeSpan.FromDays(1),
                Reason = reason
            });

            try
            {
                await member.SendMessageAsync($"You have been warned from **{ctx.Guild.Name}** for the following reason: ```{reason}```");
            }
            catch
            {
                // ignored
            }
            
            DataManager.SaveData(usrData);
            ctx.Client.Logger.LogInformation(EventIds.Warn, "'{PunisherName}#{PunisherDiscriminator}' warned '{SanctionedName}#{SanctionedDiscriminator}' for the following reason: {Reason}", ctx.Member.Username, ctx.Member.Discriminator, member.Username, member.Discriminator, reason);
            
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":hammer: Success!")
                .WithDescription($"{member.Username}#{member.Discriminator} has been **warned**{(reason == null ? null : $" for the following reason: ```{reason}```")}")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithThumbnail(member.AvatarUrl)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }

        [Command("mute"), RequirePermissions(Permissions.ManageMessages), Priority(2), Description("Mutes a member.")]
        public async Task Mute(CommandContext ctx, [Description("Member to mute.")] DiscordMember member, [Description("Duration of the mute.")] TimeSpan expiresIn, [RemainingText, Description("Reason of the mute.")] string reason = "No reason provided.")
        {
            if (!ctx.Member.CanPunish(member))
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Permission denied",
                    Description =
                        $"{DiscordEmoji.FromName(ctx.Client, ":raised_hand:")} You lack permissions necessary to run this command.",
                    Color = new DiscordColor(0xFF0000)
                }
                    .Build());
                return;
            }

            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (!bot.CanPunish(member))
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Permission denied",
                    Description =
                        $"{DiscordEmoji.FromName(ctx.Client, ":raised_hand:")} The bot needs a higher role to run this command.",
                    Color = new DiscordColor(0xFF0000)
                }
                    .Build());
                return;
            }
            
            var usrData = DataManager.GetData(member, ctx.Guild) ?? new UserData{ Id = member.Id, GuildId = ctx.Guild.Id };
            usrData.Sanctions ??= new List<Sanction>();

            if (usrData.Sanctions.Any(s => s.Type == Sanction.SanctionType.Mute && !s.HasExpired))
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Oops!",
                    Description =
                        $"{DiscordEmoji.FromName(ctx.Client, ":raised_hand:")} User is already muted.",
                    Color = new DiscordColor(0xFF0000)
                }
                    .Build());
                return;
            }
            
            usrData.Sanctions.Add(new Sanction
            {
                Type = Sanction.SanctionType.Mute,
                PunisherId = ctx.Member.Id,
                IssuedAt = DateTimeOffset.Now,
                ExpiresAt = expiresIn == TimeSpan.MaxValue ? DateTimeOffset.MaxValue : DateTimeOffset.Now + expiresIn,
                Reason = reason
            });

            try
            {
                var gldData = DataManager.GetData(ctx.Guild);
                await member.GrantRoleAsync(ctx.Guild.GetRole(gldData.Moderation.MuteRoleId), $"'{ctx.Member.Username}#{ctx.Member.Discriminator}' muted '{member.Username}#{member.Discriminator}' for the following reason: {reason}.");
                await member.SendMessageAsync("You have been " +
                                              $"{(expiresIn == TimeSpan.MaxValue ? "permanently muted" : $"muted for {(expiresIn > TimeSpan.FromDays(1) ? $"{expiresIn.TotalDays} days" : $"{expiresIn.TotalHours} hours")}")} " +
                                              $"from **{ctx.Guild.Name}** for the following reason: ```{reason}```"); }
            catch (Exception e)
            {
                ctx.Client.Logger.LogError(EventIds.Error, e, "An error occured while trying to mute '{Username}#{Discriminator}'", member.Username, member.Discriminator);
                
                await ctx.RespondAsync(new DiscordEmbedBuilder()
                    .WithTitle("Oops! An error occured.")
                    .WithDescription(
                        $"An error occured while trying to mute '{member.Username}#{member.Discriminator}'. Be sure the bot has enough permissions and retry again.")
                    .WithColor(DiscordColor.DarkRed)
                    .WithFooter($"Issuer ID: {ctx.User.Id}")
                    .WithTimestamp(DateTime.UtcNow)
                    .Build());
            }
            
            DataManager.SaveData(usrData);
            ctx.Client.Logger.LogInformation(EventIds.Mute, "'{PunisherName}#{PunisherDiscriminator}' muted '{SanctionedName}#{SanctionedDiscriminator}' for the following reason: {Reason}", ctx.Member.Username, ctx.Member.Discriminator, member.Username, member.Discriminator, reason);
            
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":hammer: Success!")
                .WithDescription($"{member.Username}#{member.Discriminator} has been " +
                                 $"{(expiresIn == TimeSpan.MaxValue ? "permanently **muted**" : $"**muted** for {(expiresIn > TimeSpan.FromDays(1) ? $"{expiresIn.TotalDays} days" : $"{expiresIn.TotalHours} hours")}")}" +
                                 $"{(reason == null ? null : $" for the following reason: ```{reason}```")}")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithThumbnail(member.AvatarUrl)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }

        [Command("mute"), RequirePermissions(Permissions.BanMembers), Priority(1)]
        public async Task Mute(CommandContext ctx, [Description("Member to mute.")] DiscordMember member, [RemainingText, Description("Reason of the mute.")] string reason = "No reason provided.") =>
            await Mute(ctx, member, TimeSpan.MaxValue, reason);

        [Command("unmute")]
        public async Task Unmute(CommandContext ctx, DiscordMember member, string reason = "No reason provided.")
        {
            if (!ctx.Member.CanPunish(member))
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Permission denied",
                    Description =
                        $"{DiscordEmoji.FromName(ctx.Client, ":raised_hand:")} You lack permissions necessary to run this command.",
                    Color = new DiscordColor(0xFF0000)
                }
                    .Build());
                return;
            }

            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (!bot.CanPunish(member))
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Permission denied",
                    Description =
                        $"{DiscordEmoji.FromName(ctx.Client, ":raised_hand:")} The bot needs a higher role to run this command.",
                    Color = new DiscordColor(0xFF0000)
                }
                    .Build());
                return;
            }
            
            var usrData = DataManager.GetData(member, ctx.Guild) ?? new UserData{ Id = member.Id, GuildId = ctx.Guild.Id };
            usrData.Sanctions ??= new List<Sanction>();

            if (!usrData.Sanctions.Any(s => s.Type != Sanction.SanctionType.Mute && !s.HasExpired))
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Oops!",
                    Description =
                        $"{DiscordEmoji.FromName(ctx.Client, ":raised_hand:")} User is not muted.",
                    Color = new DiscordColor(0xFF0000)
                }
                    .Build());
                return;
            }

            try
            {
                var gldData = DataManager.GetData(ctx.Guild);
                await member.RevokeRoleAsync(ctx.Guild.GetRole(gldData.Moderation.MuteRoleId), $"'{ctx.Member.Username}#{ctx.Member.Discriminator}' muted '{member.Username}#{member.Discriminator}' for the following reason: {reason}.");
                await member.SendMessageAsync($"You have been un-muted from **{ctx.Guild.Name}** for the following reason: ```{reason}```"); }
            catch (Exception e)
            {
                ctx.Client.Logger.LogError(EventIds.Error, e, "An error occured while trying to un-mute '{Username}#{Discriminator}'", member.Username, member.Discriminator);
                
                await ctx.RespondAsync(new DiscordEmbedBuilder()
                    .WithTitle("Oops! An error occured.")
                    .WithDescription(
                        $"An error occured while trying to un-mute '{member.Username}#{member.Discriminator}'. Be sure the bot has enough permissions and retry again.")
                    .WithColor(DiscordColor.DarkRed)
                    .WithFooter($"Issuer ID: {ctx.User.Id}")
                    .WithTimestamp(DateTime.UtcNow)
                    .Build());
            }

            var lastMute = usrData.Sanctions.Last(s => s.Type == Sanction.SanctionType.Mute);
            lastMute.ExpiresAt = DateTimeOffset.Now;
            
            DataManager.SaveData(usrData);

            ctx.Client.Logger.LogInformation(EventIds.Unmute, "'{PunisherName}#{PunisherDiscriminator}' un-muted '{SanctionedName}#{SanctionedDiscriminator}' for the following reason: {Reason}", ctx.Member.Username, ctx.Member.Discriminator, member.Username, member.Discriminator, reason);
            
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":hammer: Success!")
                .WithDescription($"{member.Username}#{member.Discriminator} has been un-muted{(reason == null ? null : $" for the following reason: ```{reason}```")}")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithThumbnail(member.AvatarUrl)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }

        [Command("sanctions"), Description("Show sanctions for a given user.")] // TODO: Improve the result
        public async Task Sanctions(CommandContext ctx, [Description("User to show sanctions. If not given, show for self.")] DiscordMember mbr = null)
        {
            await ctx.TriggerTypingAsync();
            
            var data = DataManager.GetData(mbr ?? ctx.Member, ctx.Guild);

            var dataList = data.Sanctions.OrderBy(s => s.Type).ToList();
            
            if (!dataList.Any())
            {
                await ctx.RespondAsync(":white_check_mark: This user has not been punished in the past.");
                return;
            }
            
            var interactivity = ctx.Client.GetInteractivity();

            var sb = new StringBuilder();

            
            if (dataList.Any(sanction => sanction.Type == Sanction.SanctionType.Warn))
            {
                sb.AppendLine("**Warns**");
                foreach (var sanction in dataList.FindAll(s => s.Type == Sanction.SanctionType.Warn))
                {
                    DiscordMember punisher = null;
                    try
                    {
                        punisher = await ctx.Guild.GetMemberAsync(sanction.PunisherId);
                    }
                    catch (Exception e)
                    {
                        ctx.Client.Logger.LogWarning(EventIds.Warning, e,
                            "Could not get the punisher of ID '{PunisherId}'", sanction.PunisherId);
                    }

                    sb.AppendLine(
                        $"Issued at: `{sanction.IssuedAt}` **|** Reason: `{sanction.Reason}`{(punisher != null ? $" **|** Punisher: {punisher.Mention}" : null)}");
                }
                sb.AppendLine();
            }

            if (dataList.Any(sanction => sanction.Type == Sanction.SanctionType.Mute))
            {
                sb.AppendLine("**Mutes**");
                foreach (var sanction in dataList.FindAll(s => s.Type == Sanction.SanctionType.Mute))
                {
                    DiscordMember punisher = null;
                    try
                    {
                        punisher = await ctx.Guild.GetMemberAsync(sanction.PunisherId);
                    }
                    catch (Exception e)
                    {
                        ctx.Client.Logger.LogWarning(EventIds.Warning, e,
                            "Could not get the punisher of ID '{PunisherId}'", sanction.PunisherId);
                    }

                    sb.AppendLine(
                        $"Issued at: `{sanction.IssuedAt}` **|** Duration: `{(sanction.IsPermanent ? "Permanent" : sanction.HasExpired ? $"Expired on {sanction.ExpiresAt}" : $"Expires at {sanction.ExpiresAt}")}` **|** Reason: `{sanction.Reason}`{(punisher != null ? $" **|** Punisher: {punisher.Mention}" : null)}");
                }
                sb.AppendLine();
            }
            
            if (dataList.Any(sanction => sanction.Type == Sanction.SanctionType.Kick))
            {
                sb.AppendLine("**Kicks**");
                foreach (var sanction in dataList.FindAll(s => s.Type == Sanction.SanctionType.Kick))
                {
                    DiscordMember punisher = null;
                    try
                    {
                        punisher = await ctx.Guild.GetMemberAsync(sanction.PunisherId);
                    }
                    catch (Exception e)
                    {
                        ctx.Client.Logger.LogWarning(EventIds.Warning, e,
                            "Could not get the punisher of ID '{PunisherId}'", sanction.PunisherId);
                    }

                    sb.AppendLine(
                        $"Issued at: `{sanction.IssuedAt}` **|** Reason: `{sanction.Reason}`{(punisher != null ? $" **|** Punisher: {punisher.Mention}" : null)}");
                }
                sb.AppendLine();
            }
            
            if (dataList.Any(sanction => sanction.Type == Sanction.SanctionType.Ban))
            {
                sb.AppendLine("**Bans**");
                foreach (var sanction in dataList.FindAll(s => s.Type == Sanction.SanctionType.Ban))
                {
                    DiscordMember punisher = null;
                    try
                    {
                        punisher = await ctx.Guild.GetMemberAsync(sanction.PunisherId);
                    }
                    catch (Exception e)
                    {
                        ctx.Client.Logger.LogWarning(EventIds.Warning, e,
                            "Could not get the punisher of ID '{PunisherId}'", sanction.PunisherId);
                    }

                    sb.AppendLine(
                        $"Issued at: `{sanction.IssuedAt}` **|** Duration: `{(sanction.IsPermanent ? "Permanent" : sanction.HasExpired ? $"Expired on {sanction.ExpiresAt}" : $"Expires at {sanction.ExpiresAt}")}` **|** Reason: `{sanction.Reason}`{(punisher != null ? $" **|** Punisher: {punisher.Mention}" : null)}");
                }
                sb.AppendLine();
            }

            var pages = interactivity.GeneratePagesInContent(sb.ToString().TrimEnd('\r', '\n'), SplitType.Line);
            
            await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);
        }
    }
}