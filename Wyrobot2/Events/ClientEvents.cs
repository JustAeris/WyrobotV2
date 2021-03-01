using System;
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

            client.MessageCreated += async (sender, args) =>
            {
                if (args.Author.IsBot) return;
                var gldData = DataManager.GetData(args.Guild);
                
                // LEVELING BEGIN
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
                            if (reward.RequiredLevel > usrData.Level) continue;
                            var mbr = (DiscordMember) args.Author;
                            var role = args.Guild.GetRole(reward.RoleId);
                            try
                            {
                                await mbr.GrantRoleAsync(role);
                            }
                            catch (Exception e)
                            {
                                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                                sender.Logger.LogError($"Bot is lacking permissions to reward a user. Exception: {e}");
                                await args.Channel.SendMessageAsync(new DiscordEmbedBuilder
                                {
                                    Color = DiscordColor.Red,
                                    Title = "Error :raised_hand:",
                                    Description = "I cannot reward a user, please consider moving the bot's role higher!"
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
                // LEVELING END
            };
        }
    }
}