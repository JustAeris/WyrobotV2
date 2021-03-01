using System.Threading.Tasks;
using DSharpPlus;
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

            client.MessageCreated += async (_, args) =>
            {
                if (args.Author.IsBot) return;
                var gldData = DataManager.GetData(args.Guild);
                
                // LEVELING BEGIN
                if (gldData.Leveling.Enabled)
                {
                    var usrData = DataManager.GetData(args.Author, args.Guild) ?? new UserData
                    {
                        Id = args.Message.Author.Id,
                        GuildId = args.Guild.Id
                    };

                    // ReSharper disable once PossibleLossOfFraction
                    usrData.Xp = (int) (usrData.Xp + args.Message.Content.Length / 2 * gldData.Leveling.Multiplier);

                    if (usrData.Xp > usrData.XpToNextLevel)
                    {
                        usrData.Xp -= usrData.XpToNextLevel;
                        usrData.Level += 1;
                        await args.Channel.SendMessageAsync(gldData.Leveling.Message
                            .Replace("{user}", args.Message.Author.Mention)
                            .Replace("{level}", usrData.Level.ToString()));
                    }
                    
                    DataManager.SaveData(usrData);
                }
                // LEVELING END
            };
        }
    }
}