using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Wyrobot2.Data.Models;

namespace Wyrobot2.Data
{
    internal static class DataContext
    {
        public static GuildData GetGuildData(ulong id)
        {
            var path = $"guilds/{id}.json";
            return !File.Exists(path) ? default : JsonConvert.DeserializeObject<GuildData>(File.ReadAllText(path));
        }

        public static UserData GetUserData(ulong guildId, ulong userId) =>
            GetGuildData(guildId).UsersList.FirstOrDefault(ud => ud.Id == userId);
        
        public static MusicData GetMusicData(ulong guildId) =>
            GetGuildData(guildId).MusicData;

        public static void SaveGuildData(this GuildData data)
        {
            var path = $"guilds/{data.Id}.json";

            if (!Directory.Exists("guilds")) Directory.CreateDirectory("guilds");

            var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);

            fs.SetLength(0);

            fs.Write(Encoding.UTF8.GetBytes(json));
            
            fs.Dispose();
        }

        public static void SaveUserData(this UserData data)
        {
            var gldData = GetGuildData(data.GuildId);

            var usrData = gldData.UsersList.FirstOrDefault(ud => ud.Id == data.Id);

            if (usrData == null)
            {
                gldData.UsersList.Add(data);
            }
            else
            {
                gldData.UsersList.Remove(usrData);
                gldData.UsersList.Add(data);
            }
            
            SaveGuildData(gldData);
        }
        
        public static void SaveMusicData(this MusicData data)
        {
            var gldData = GetGuildData(data.GuildId);

            gldData.MusicData = data;
            
            SaveGuildData(gldData);
        }

        public static void DeleteGuildData(ulong id) => File.Delete($"guilds/{id}.json");
    }
}