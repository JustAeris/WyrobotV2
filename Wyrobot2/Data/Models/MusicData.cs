using System.Collections.Generic;

namespace Wyrobot2.Data.Models
{
    public class MusicData
    {
        public ulong GuildId { get; set; }

        public bool DjEnabled { get; set; }

        public ulong DjRoleId { get; set; }

        public bool IsLooping { get; set; }

        public Queue<MusicTrack> Tracks { get; set; }
    }
}