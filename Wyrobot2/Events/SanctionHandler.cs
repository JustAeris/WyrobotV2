using System;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Wyrobot2.Data;
using Wyrobot2.Data.Models;

namespace Wyrobot2.Events
{
    public static class SanctionHandler
    {
        private static readonly Timer Timer;
        private static DiscordClient _client;

        static SanctionHandler()
        {
            Timer = new Timer
            {
                Enabled = true,
                AutoReset = true,
                Interval = 60000
            };
            Timer.Elapsed += async (_, _) => await HandleSanctions();
        }

        public static void InitializeAndStart(DiscordClient client)
        {
            _client = client;
            Timer.Start();
        }

        private static async Task HandleSanctions()
        {
            var anyErrors = false;
            
            foreach (var guild in _client.Guilds.Values)
            {
                var userDataList = DataManager.GetAllData(guild);
                var guildData = DataManager.GetData(guild);

                foreach (var data in userDataList)
                {
                    foreach (var sanction in data.Sanctions)
                    {
                        if (!sanction.HasExpired) continue;
                        
                        if (sanction.HasExpired && sanction.HasBeenHandled) continue;
                        
                        switch (sanction.Type)
                        {
                            case Sanction.SanctionType.Ban:
                                try
                                {
                                    await guild.UnbanMemberAsync(data.Id, $"'{data.Id}' has been automatically unbanned.");
                                }
                                catch (Exception exception)
                                {
                                    _client.Logger.LogError(EventIds.ScheduledError, exception, "Could not unban '{Id}'", data.Id);
                                    anyErrors = true;
                                    break;
                                }
                                _client.Logger.LogInformation(EventIds.Unban, "'{Id}' has been automatically unbanned", data.Id);
                                sanction.HasBeenHandled = true;
                                break;
                            
                            case Sanction.SanctionType.Mute:
                                DiscordMember member;
                                try
                                {
                                    member = await guild.GetMemberAsync(data.Id);
                                }
                                catch (Exception exception)
                                {
                                    _client.Logger.LogError(EventIds.ScheduledError, exception, "Could not get member of ID '{Id}'", data.Id);
                                    anyErrors = true;
                                    break;
                                }

                                DiscordRole muteRole;
                                try
                                {
                                    muteRole = guild.GetRole(guildData.Moderation.MuteRoleId);
                                    
                                }
                                catch (Exception exception)
                                {
                                    _client.Logger.LogError(EventIds.ScheduledError, exception, "Could not get role of ID '{RoleId}'", guildData.Moderation.MuteRoleId);
                                    anyErrors = true;
                                    break;
                                }

                                try
                                {
                                    await member.RevokeRoleAsync(muteRole, $"{member.Username}#{member.Discriminator} has been automatically un-muted.");
                                }
                                catch (Exception exception)
                                {
                                    _client.Logger.LogError(EventIds.ScheduledError, exception, "Could not un-mute '{Username}'", member.Username);
                                    anyErrors = true;
                                    break;
                                }
                                
                                _client.Logger.LogInformation(EventIds.Unmute, "{Username}#{Discriminator} has been automatically un-mute", member.Username, member.Discriminator);
                                sanction.HasBeenHandled = true;
                                break;
                            
                            case Sanction.SanctionType.Warn:
                            case Sanction.SanctionType.Kick:
                            case Sanction.SanctionType.Unban:
                                continue;
                            
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    DataManager.SaveData(data);
                }
            }
            
            if (!anyErrors) _client.Logger.LogInformation(EventIds.Scheduled, "Scheduled tasks have been executed successfully");
            else _client.Logger.LogWarning(EventIds.ScheduledError, "Scheduled tasks have been executed with errors");
        }
    }
}