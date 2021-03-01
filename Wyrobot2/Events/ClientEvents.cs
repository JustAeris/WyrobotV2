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
                DataManager<GuildData>.SaveData(new GuildData
                {
                    Id = args.Guild.Id
                });
                return Task.CompletedTask;
            };
        }
    }
}