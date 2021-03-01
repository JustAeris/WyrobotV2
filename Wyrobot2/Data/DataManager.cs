using System.IO;
using System.Text;
using Newtonsoft.Json;
using Wyrobot2.Data.Models;

namespace Wyrobot2.Data
{
    public static class DataManager<T> where T : IDataManager
    {
        public static T GetData(T t, string id, string guildId = null)
        {
            var path = t switch
            {
                GuildData => $"guilds/{id}/settings.json",
                UserData => $"guilds/{guildId}/users/{id}.json",
                _ => ""
            };

            if (!File.Exists(path))
                return default;

            var content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(content);
        }

        public static void SaveData(T t)
        {
            var path = $"{t.Folder}/{t.Identifier}.json";

            if (!Directory.Exists(t.Folder)) Directory.CreateDirectory(t.Folder);
            if (!File.Exists(path))
            {
                var fs = File.Create(path);
                fs.Dispose();
            }

            var json = JsonConvert.SerializeObject(t, Formatting.Indented);

            using var f =
                new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)
                    { Position = 0 };

            f.SetLength(0);

            f.Write(Encoding.UTF8.GetBytes(json));
        }
    }
}