#nullable enable
using System;
using Newtonsoft.Json;

namespace Wyrobot2.Data.Models
{
    public class Sanction
    {
        public SanctionType Type { get; set; }
        public ulong PunisherId { get; set; }

        public DateTimeOffset IssuedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }

        [JsonIgnore]
        public bool HasExpired
        {
            get
            {
                if (IsPermanent) return false;
                return ExpiresAt.UtcDateTime < DateTimeOffset.Now.UtcDateTime;
            }
        }

        public bool HasBeenHandled { get; set; }

        public bool IsPermanent => ExpiresAt == DateTimeOffset.MaxValue;

        public string? Reason { get; set; }
        
        public enum SanctionType
        {
            Warn,
            Mute,
            Kick,
            Ban,
            Unban
        }
    }

}