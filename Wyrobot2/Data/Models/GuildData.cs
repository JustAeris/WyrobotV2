using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wyrobot2.Data.Models
{
    public class GuildData : IDataManager
    {
        public ulong Id { get; init; }

        public string Prefix { get; set; }

        public ulong? IntegrationRoleId { get; init; }
        
        public GuildData()
        {
            Prefix = "w!";

            Moderation = new ModerationSettings
            {
                CapsPercentage = 15F,
                AutoModerationEnabled = true
            };
            Welcome = new WelcomeSettings();
            Logging = new LoggingSettings();
            Leveling = new LevelingSettings
            {
                Enabled = true,
                Message = "Woo! {user} leveled up to {level}! :tada:",
                Multiplier = 1,
                LevelRewards = new List<LevelReward>()
            };
            Other = new OtherSettings();
            Moderation.ModerationRoles = new List<ulong>();
            Moderation.BannedWords = new List<string>();
        }

        public LoggingSettings Logging { get; }
        public LevelingSettings Leveling { get; }
        public ModerationSettings Moderation { get; }
        public WelcomeSettings Welcome { get; }
        public OtherSettings Other { get; }
        
        public class ModerationSettings
        {
            public ulong MuteRoleId { get; set; }
            public IEnumerable<ulong> ModerationRoles { get; set; }
            public IEnumerable<string> BannedWords { get; set; }
            public float CapsPercentage { get; set; }
            public bool AutoModerationEnabled { get; set; }
        }
        
        public class WelcomeSettings
        {
            public bool Enabled { get; set; }
            public ulong ChannelId { get; set; }
            public string Message { get; set; }
        }

        public class LoggingSettings
        {
            public bool Enabled { get; set; }
            public ulong ChannelId { get; set; }
            public bool LogMessages { get; set; }
            public bool LogPunishments { get; set; }
            public bool LogInvites { get; set; }
            public bool LogVoiceState { get; set; }
            public bool LogChannels { get; set; }
            public bool LogUsers { get; set; }
            public bool LogRoles { get; set; }
            public bool LogServer { get; set; }
        }
        
        public class LevelingSettings
        {
            public bool Enabled { get; set; }
            public float Multiplier { get; set; }
            public string Message { get; set; }
            public ICollection<LevelReward> LevelRewards { get; set; }
        }

        public class OtherSettings
        {
            public bool LoungesEnabled { get; set; }
            public bool CatCmdEnabled { get; set; }
            public bool DogCmdEnabled { get; set; }
        }
        
        [JsonIgnore]
        public string Folder => $"guilds/{Id.ToString()}";
        [JsonIgnore]
        public string Identifier => "settings";
    }
}