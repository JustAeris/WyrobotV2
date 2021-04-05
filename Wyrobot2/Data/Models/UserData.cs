using System.Collections.Generic;

namespace Wyrobot2.Data.Models
{
    public class UserData
    {
        public UserData()
        {
            Sanctions = new List<Sanction>();
        }
        
        public ulong Id { get; init; }

        public ulong GuildId { get; init; }
        
        public int Xp { get; set; }
        public int Level { get; set; }
        public int XpToNextLevel => Level * 100 + 75;

        public ICollection<Sanction> Sanctions { get; set; }
    }
}