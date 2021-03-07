using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Wyrobot.Api.Data
{
    
    public static class DataManager
    {
        private static readonly string ParentDirectory = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)
            ?.Parent?.Parent?.ToString();
        public static T GetData<T>(ulong id, ulong guildId = 123456789)
        {
            var path = guildId != 123456789 ? $"{ParentDirectory}\\guilds\\{guildId}\\users\\{id}.json" : $"{ParentDirectory}\\guilds\\{id}\\settings.json";

            if (!File.Exists(path))
                throw new FileNotFoundException();

            var content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(content);
        }
        
        public static void SaveData<T>(T obj)
        {
            /*var path = $"{obj.Folder}/{obj.Identifier}.json";

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

            f.Write(Encoding.UTF8.GetBytes(json));*/
        }
        
        public static void DeleteData(ulong id, ulong guildId = 123456789)
        {
            var path = guildId != 123456789 ? $"../../guilds/{guildId}/users/{id}.json" : $"../../guilds/{id}/";

            if (guildId != 123456789) File.Delete(path);
            else Directory.Delete(path, true);
        }
        
        public static IEnumerable<T> GetAllData<T>(ulong id)
        {
            return from file in Directory.GetFiles($"../../guilds/{id}/users/") where File.Exists(file) select JsonConvert.DeserializeObject<T>(File.ReadAllText(file));
        }
    }
}