using System;
using System.Timers;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Wyrobot2.Data;
using Wyrobot2.Data.Models;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Wyrobot2.Events
{
    public static class RecurrentEvent
    {
        private static readonly Timer Timer;
        private static DiscordClient _client;

        static RecurrentEvent()
        {
            Timer = new Timer
            {
                Enabled = true,
                AutoReset = true,
                Interval = 60000
            };
            Timer.Elapsed += TimerOnElapsed;
        }

        public static void InitializeAndStart(DiscordClient client)
        {
            _client = client;
            Timer.Start();
        }

        private static async void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
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
                                await guild.UnbanMemberAsync(data.Id, $"'{data.Id}' has been automatically unbanned.");
                                _client.Logger.LogInformation(EventIds.Unban, $"'{data.Id}' has been automatically unbanned");
                                sanction.HasBeenHandled = true;
                                break;
                            
                            case Sanction.SanctionType.Mute:
                                var member = await guild.GetMemberAsync(data.Id);
                                var muteRole = guild.GetRole(guildData.Moderation.MuteRoleId);
                                await member.RevokeRoleAsync(muteRole, $"{member.Username}#{member.Discriminator} has been automatically un-muted.");
                                _client.Logger.LogInformation(EventIds.Unmute, $"{member.Username}#{member.Discriminator} has been automatically un-mute");
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
            
            _client.Logger.LogInformation(EventIds.Scheduled, "Scheduled tasks have been executed successfully");
        }
    }
}