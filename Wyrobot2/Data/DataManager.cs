using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Wyrobot2.Data.Models;

namespace Wyrobot2.Data
{
    internal static class DataManager
    {
        private static async Task<T> GetData<T>(ulong id, ulong? guildId = null) where T : IDataManager
        {
            var path = "";

            if (typeof(T) == typeof(GuildData))
                path = $"guilds/{id}/";
            else if (typeof(T) == typeof(UserData))
                path = $"guilds/{guildId}/users/{id}.json";
            else if (typeof(T) == typeof(MusicData))
                path = $"guilds/{id}/music.json";

            return !File.Exists(path) ? default : JsonConvert.DeserializeObject<T>(await File.ReadAllTextAsync(path));
        }
        public static async Task<GuildData> GetData(DiscordGuild gld) => await GetData<GuildData>(gld.Id);
        public static async Task<UserData> GetData(DiscordUser user, DiscordGuild gld) => await GetData<UserData>(user.Id, gld.Id);
        
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
        
        public static void DeleteData<T>(ulong id, ulong? guildId = null) where T : IDataManager
        {
            var path = "";

            if (typeof(T) == typeof(GuildData))
                path = $"guilds/{id}/";
            else if (typeof(T) == typeof(UserData))
                path = $"guilds/{guildId}/users/{id}.json";
            else if (typeof(T) == typeof(MusicData))
                path = $"guilds/{id}/music.json";

            if (typeof(T) != typeof(GuildData)) File.Delete(path);
            else Directory.Delete(path, true);
        }
        
        private static IEnumerable<T> GetAllData<T>(ulong id = 0)
        {
            if (typeof(T) != typeof(GuildData))
                return from file in Directory.GetFiles($"guilds/{id}/users/")
                    where File.Exists(file)
                    select JsonConvert.DeserializeObject<T>(File.ReadAllText(file));
            var directories = Directory.GetDirectories("guilds/");
            return directories.Select(directory => JsonConvert.DeserializeObject<T>(File.ReadAllText(Path.Combine(directory, "settings.json")))).ToList();

        }
        public static IEnumerable<UserData> GetAllData(DiscordGuild gld) => GetAllData<UserData>(gld.Id);
    }
}