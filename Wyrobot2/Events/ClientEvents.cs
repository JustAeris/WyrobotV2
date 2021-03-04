using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Wyrobot2.Data;
using Wyrobot2.Data.Models;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Wyrobot2.Events
{
    public static class ClientEvents
    {
        public static void RegisterEvents(this DiscordClient client)
        {
            client.GuildCreated += (_, args) =>
            {
                DataManager.SaveData(new GuildData
                {
                    Id = args.Guild.Id
                });
                return Task.CompletedTask;
            };

            client.GuildDeleted += (_, args) =>
            {
                DataManager.DeleteData(args.Guild);
                return Task.CompletedTask;
            };

            client.GuildBanAdded += async (sender, args) =>
            {
                var usrData = DataManager.GetData(args.Member, args.Guild) ?? new UserData{ Id = args.Member.Id, GuildId = args.Guild.Id };
                usrData.Sanctions ??= new List<Sanction>();
                
                var bans = await args.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.Ban);
                var banner = bans[0].UserResponsible;
                var reason = bans[0].Reason ?? "No reason provided.";
                
                usrData.Sanctions.Add(new Sanction
                {
                    Type = Sanction.SanctionType.Ban,
                    BannerId = banner.Id,
                    IssuedAt = DateTimeOffset.Now,
                    ExpiresAt = DateTimeOffset.MaxValue,
                    Reason = reason
                });
                
                DataManager.SaveData(usrData);
                
                sender.Logger.LogInformation($"'{banner.Username}#{banner.Discriminator}' banned '{args.Member.Username}#{args.Member.Discriminator}' for the following reason: {reason}.");
            };

            client.GuildBanRemoved += async (sender, args) =>
            {
                var data = DataManager.GetData(args.Member, args.Guild);
                var lastBan = data.Sanctions.LastOrDefault(s => s.Type == Sanction.SanctionType.Ban);
                if (lastBan != null)
                {
                    lastBan.HasBeenUnbanned = true;
                    lastBan.ExpiresAt = DateTimeOffset.Now;
                    DataManager.SaveData(data);
                }
                
                var unbans = await args.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.Unban);
                var responsible = unbans[0].UserResponsible;
                sender.Logger.LogInformation($"'{responsible.Username}#{responsible.Discriminator}' unbanned '{args.Member.Username}#{args.Member.Discriminator}'.");
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
                                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                                sender.Logger.LogError(
                                    $"{mbr.Username} has already the reward. Silently skipping the error.");
                                DataManager.SaveData(usrData);
                                continue;
                            }

                            try
                            {
                                await mbr.GrantRoleAsync(role);
                            }
                            catch (Exception e)
                            {
                                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                                sender.Logger.LogError(
                                    $"Bot is lacking permissions to reward a user. Exception: {e}");
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
            };
        }
    }
}