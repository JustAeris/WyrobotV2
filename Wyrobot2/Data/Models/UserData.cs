using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wyrobot2.Data.Models
{
    public class UserData : IDataManager
    {
        public ulong Id { get; init; }

        public ulong GuildId { get; init; }
        
        public int Xp { get; set; }
        public int Level { get; set; }
        public int XpToNextLevel => Level * 100 + 75;

        public ICollection<Sanction> Sanctions { get; set; }
        
        [JsonIgnore]
        public string Folder => $"guilds/{GuildId}/users";
        [JsonIgnore]
        public string Identifier => Id.ToString();
    }
}