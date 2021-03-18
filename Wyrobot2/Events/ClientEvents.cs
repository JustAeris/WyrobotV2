#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Wyrobot2.Data;
using Wyrobot2.Data.Models;

namespace Wyrobot2.Events
{
    public static class ClientEvents
    {
        public static void RegisterEvents(this DiscordClient client)
        {
            client.GuildCreated += async (sender, args) =>
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
            };

            client.GuildDeleted += (_, args) =>
            {
                DataManager.DeleteData(args.Guild);
                return Task.CompletedTask;
            };

            client.GuildBanAdded += async (sender, args) =>
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
            };

            client.GuildBanRemoved += async (sender, args) =>
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
            };

            client.GuildMemberRemoved += async (sender, args) =>
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
            };

            client.MessageCreated += async (sender, args) =>
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
                
                if (!gldData.Moderation.AutoModerationEnabled) return;

                if (gldData.Moderation.BannedWords.Any(bannedWord => args.Message.Content.Contains(bannedWord)))
                {
                    // TODO: punish here
                    
                    return;
                }
                
                var capsCount = 0F;
                foreach (var unused in args.Message.Content.Where(char.IsUpper))
                    capsCount++;
                if (capsCount > gldData.Moderation.CapsPercentage)
                {
                    // TODO: Punish here
                    
                    return;
                }

                // ----- AUTO-MODERATION END -----
            };
        }
    }
}