#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Wyrobot2.Data;
using Wyrobot2.Data.Models;

namespace Wyrobot2.Events
{
    public static class ClientEvents
    {
        public static async Task OnGuildCreated(DiscordClient sender, GuildCreateEventArgs args)
        {
            var role = await args.Guild.CreateRoleAsync("Wyrobot Mute", Permissions.None, new DiscordColor(54, 57, 63), false, false,
                "Mute role creation on join");

            _ = Task.Run(async () =>
            {
                foreach (var (_, value) in args.Guild.Channels)
                {
                    try
                    {
                        await value.AddOverwriteAsync(role, Permissions.None,
                            Permissions.Speak | Permissions.SendMessages, "Auto permissions set");
                        await Task.Delay(2000);
                    }
                    catch (Exception e)
                    {
                        sender.Logger.LogError(EventIds.Error, e, "Could not set permission for channel '#{CName}' ({CId}) in guild '{GName}' ({GId})", value.Name, value.Id, args.Guild.Name, args.Guild.Id);
                    }
                }
                sender.Logger.LogDebug(EventIds.GuildRelated, "Channel permissions set for guild '{GName}' ({GId})", args.Guild.Name, args.Guild.Id);
            });
                
            var bot = await args.Guild.GetMemberAsync(sender.CurrentUser.Id);

            var first = bot.Roles.FirstOrDefault();

            await role.ModifyPositionAsync(first?.Position ?? args.Guild.Roles.Last().Value.Position);

            DataManager.SaveData(new GuildData
            {
                Id = args.Guild.Id,
                IntegrationRoleId = first?.Id ?? 0,
                Moderation = {MuteRoleId = role.Id}
            });
        }

        public static Task OnGuildDeleted(DiscordClient sender, GuildDeleteEventArgs args)
        {
            DataManager.DeleteData(args.Guild);
            return Task.CompletedTask;
        }

        public static async Task OnGuildBanAdded(DiscordClient sender, GuildBanAddEventArgs args)
        {
            var sanctions = await args.Guild.GetAuditLogsAsync(10, action_type: AuditLogActionType.Kick);
            var sanction = sanctions.OfType<DiscordAuditLogBanEntry>().FirstOrDefault(entry => entry.Target == args.Member);
            if (sanction == null) return;
            var responsible = sanction.UserResponsible;
            var reason = sanction.Reason ?? "No reason provided.";
                
            var usrData = DataManager.GetData(args.Member, args.Guild) ?? new UserData{ Id = args.Member.Id, GuildId = args.Guild.Id };
            usrData.Sanctions ??= new List<Sanction>();
                
            usrData.Sanctions.Add(new Sanction
            {
                Type = Sanction.SanctionType.Ban,
                PunisherId = responsible.Id,
                IssuedAt = DateTimeOffset.Now,
                ExpiresAt = DateTimeOffset.MaxValue,
                Reason = reason
            });
                
            DataManager.SaveData(usrData);
                
            sender.Logger.LogInformation(EventIds.Ban, "'{RUsername}#{RDiscriminator}' banned '{PUsername}#{PDiscriminator}' for the following reason: {Reason}", responsible.Username, responsible.Discriminator, args.Member.Username, args.Member.Discriminator, reason);
        }

        public static async Task OnGuildBanRemoved(DiscordClient sender, GuildBanRemoveEventArgs args)
        {
            var data = DataManager.GetData(args.Member, args.Guild);
            var lastBan = data.Sanctions.LastOrDefault(s => s.Type == Sanction.SanctionType.Ban);
            if (lastBan != null)
            {
                lastBan.Type = Sanction.SanctionType.Unban;
                lastBan.ExpiresAt = DateTimeOffset.Now;
                DataManager.SaveData(data);
            }
                
            var unbans = await args.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.Unban);
            var responsible = unbans[0].UserResponsible;
            sender.Logger.LogInformation(EventIds.Unban, "'{RUsername}#{RDiscriminator}' unbanned '{UMUsername}#{UMDiscriminator}'", responsible.Username, responsible.Discriminator, args.Member.Username, args.Member.Discriminator);
        }

        public static async Task OnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs args)
        {
            var gldData = DataManager.GetData(args.Guild);
            if (!gldData.Welcome.Enabled) return;
            var channel = args.Guild.GetChannel(gldData.Welcome.ChannelId);
            if (channel == null)
            {
                sender.Logger.LogWarning(EventIds.Warning, "Could not get welcome channel of ID '{ChannelID}'", gldData.Welcome.ChannelId);
                return;
            }

            await channel.SendMessageAsync(gldData.Welcome.Message.Replace("{user}", args.Member.Mention, StringComparison.InvariantCultureIgnoreCase));
        }
        
        public static async Task OnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs args)
        {
            var sanctions = await args.Guild.GetAuditLogsAsync(10, action_type: AuditLogActionType.Kick);
            var sanction = sanctions.OfType<DiscordAuditLogKickEntry>().FirstOrDefault(entry => entry.Target == args.Member);
            if (sanction == null) return;
            var responsible = sanction.UserResponsible;
            var reason = sanction.Reason ?? "No reason provided.";
                
            var usrData = DataManager.GetData(args.Member, args.Guild) ?? new UserData{ Id = args.Member.Id, GuildId = args.Guild.Id };
            usrData.Sanctions ??= new List<Sanction>();
                
            usrData.Sanctions.Add(new Sanction
            {
                Type = Sanction.SanctionType.Kick,
                PunisherId = responsible.Id,
                IssuedAt = DateTimeOffset.Now,
                ExpiresAt = DateTimeOffset.Now,
                Reason = reason
            });
                
            DataManager.SaveData(usrData);

            sender.Logger.LogInformation(EventIds.Kick ,"'{RUsername}#{RDiscriminator}' kicked '{PUsername}#{PDiscriminator}' for the following reason: {Reason}", responsible.Username, responsible.Discriminator, args.Member.Username, args.Member.Discriminator, reason);
        }

        public static async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
        {
            if (args.Author.IsBot) return;
            var gldData = DataManager.GetData(args.Guild);
            
            // ----- LEVELING BEGIN -----
            
            if (gldData.Leveling.Enabled)
            {
                var usrData = DataManager.GetData(args.Author, args.Guild) ?? new UserData
                {
                    Id = args.Author.Id,
                    GuildId = args.Guild.Id
                };

                // ReSharper disable once PossibleLossOfFraction
                usrData.Xp = (int) (usrData.Xp + args.Message.Content.Length / 2 * gldData.Leveling.Multiplier);

                if (usrData.Xp > usrData.XpToNextLevel)
                {
                    usrData.Xp -= usrData.XpToNextLevel;
                    usrData.Level += 1;
                    await args.Channel.SendMessageAsync(gldData.Leveling.Message
                        .Replace("{user}", args.Author.Mention)
                        .Replace("{level}", usrData.Level.ToString()));

                    foreach (var reward in gldData.Leveling.LevelRewards)
                    {
                        if (usrData.Level <= reward.RequiredLevel) continue;
                        var mbr = (DiscordMember) args.Author;
                        var role = args.Guild.GetRole(reward.RoleId);
                        
                        if (mbr.Roles.Contains(role))
                        {
                            sender.Logger.LogWarning(EventIds.Warning,
                                "{MUsername}#{MDiscriminator} has already the reward. Silently skipping the error", mbr.Username, mbr.Discriminator);
                            DataManager.SaveData(usrData);
                            continue;
                        }

                        try
                        {
                            await mbr.GrantRoleAsync(role);
                        }
                        catch (Exception e)
                        {
                            sender.Logger.LogError(EventIds.Error, e,
                                "Bot is lacking permissions to reward a user");
                            await args.Channel.SendMessageAsync(new DiscordEmbedBuilder
                            {
                                Color = DiscordColor.Red,
                                Title = "Error :raised_hand:",
                                Description =
                                    "I cannot reward a user, please consider moving the bot's role higher!"
                            });
                            return;
                        }
                        finally
                        {
                            DataManager.SaveData(usrData);
                        }
                    }
                }
                
                DataManager.SaveData(usrData);
            }
            
            // ----- LEVELING END -----
            
            
            // ----- AUTO-MODERATION START -----
            
            if (!gldData.Moderation.AutoModerationEnabled || gldData.Moderation.ModerationRoles.Intersect(args.Guild.Roles.Keys).Any())
            {
                if (gldData.Moderation.BannedWords.Any(bannedWord => args.Message.Content.Contains(bannedWord)))
                {
                    await args.Message.DeleteAsync();
                    await args.Channel.SendMessageAsync(
                        $":warning: {args.Author.Mention} Do not use banned words!");

                    var usrData = DataManager.GetData(args.Author, args.Guild) ?? new UserData
                        {Id = args.Author.Id, GuildId = args.Guild.Id};
                    usrData.Sanctions ??= new List<Sanction>();

                    usrData.Sanctions.Add(new Sanction
                    {
                        Type = Sanction.SanctionType.Warn,
                        PunisherId = sender.CurrentUser.Id,
                        IssuedAt = DateTimeOffset.Now,
                        ExpiresAt = DateTimeOffset.Now + TimeSpan.FromDays(1),
                        Reason = "Banned word usage"
                    });

                    sender.Logger.LogInformation(EventIds.Warn,
                        "{Username} has been warned for using banned words in the following message: '{MessageContent}'",
                        args.Author.Tag(), args.Message.Content);

                    return;
                }

                if (gldData.Moderation.CapsPercentage != 0 && args.Message.Content.Where(char.IsUpper).Count() >
                    gldData.Moderation.CapsPercentage)
                {
                    await args.Message.DeleteAsync();
                    await args.Channel.SendMessageAsync($":warning: {args.Author.Mention} Do not spam caps!");

                    var usrData = DataManager.GetData(args.Author, args.Guild) ?? new UserData
                        {Id = args.Author.Id, GuildId = args.Guild.Id};
                    usrData.Sanctions ??= new List<Sanction>();

                    usrData.Sanctions.Add(new Sanction
                    {
                        Type = Sanction.SanctionType.Warn,
                        PunisherId = sender.CurrentUser.Id,
                        IssuedAt = DateTimeOffset.Now,
                        ExpiresAt = DateTimeOffset.Now + TimeSpan.FromDays(1),
                        Reason = "Excessive caps usage"
                    });

                    sender.Logger.LogInformation(EventIds.Warn,
                        "{Username} has been warned for excessive caps usage in the following message: '{MessageContent}'",
                        args.Author.Tag(), args.Message.Content);

                    return;
                }
            }

            // ----- AUTO-MODERATION END -----
            
            sender.Logger.LogInformation(EventIds.Success, "MessageCreated event successfully handled!");
        }
    }
}