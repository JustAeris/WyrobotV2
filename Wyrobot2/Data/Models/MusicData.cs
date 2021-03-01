using System.Collections.Generic;

namespace Wyrobot2.Data.Models
{
    public class MusicData : IDataManager
    {
        public ulong GuildId { get; set; }

        public LoopMode RepeatMode { get; set; }

        public Queue<MusicTrack> Tracks { get; set; }
        
        public string Folder => $"guilds/{GuildId}";
        public string Identifier => "music";
        
        public enum LoopMode
        {
            None,
            Repeat,
            SingleRepeat,
            Shuffle
        }
    }
}