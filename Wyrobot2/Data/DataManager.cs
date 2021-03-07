using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Wyrobot2.Data.Models;

namespace Wyrobot2.Data
{
    public static class DataManager
    {
        private static T GetData<T>(ulong id, ulong guildId = 123456789) where T : IDataManager
        {
            var path = guildId != 123456789 ? $"guilds/{guildId}/users/{id}.json" : $"guilds/{id}/settings.json";

            if (!File.Exists(path))
                return default;

            var content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(content);
        }
        public static GuildData GetData(DiscordGuild gld) => GetData<GuildData>(gld.Id);
        public static UserData GetData(DiscordUser user, DiscordGuild gld) => GetData<UserData>(user.Id, gld.Id);
        
        public static void SaveData<T>(T obj) where T : IDataManager
        {
            var path = $"{obj.Folder}/{obj.Identifier}.json";

            if (!Directory.Exists(obj.Folder)) Directory.CreateDirectory(obj.Folder);
            if (!File.Exists(path))
            {
                var fs = File.Create(path);
                fs.Dispose();
            }

            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);

            using var f =
                new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)
                    { Position = 0 };

            f.SetLength(0);

            f.Write(Encoding.UTF8.GetBytes(json));
        }
        
        private static void DeleteData(ulong id, ulong guildId = 123456789)
        {
            var path = guildId != 123456789 ? $"guilds/{guildId}/users/{id}.json" : $"guilds/{id}/";

            if (guildId != 123456789) File.Delete(path);
            else Directory.Delete(path, true);
        }
        public static void DeleteData(DiscordGuild gld) => DeleteData(gld.Id);
        public static void DeleteData(DiscordUser user, DiscordGuild gld) => DeleteData(user.Id, gld.Id);
        
        private static IEnumerable<T> GetAllData<T>(ulong id)
        {
            return from file in Directory.GetFiles($"guilds/{id}/users/") where File.Exists(file) select JsonConvert.DeserializeObject<T>(File.ReadAllText(file));
        }
        public static IEnumerable<UserData> GetAllData(DiscordGuild gld) => GetAllData<UserData>(gld.Id);
    }
}