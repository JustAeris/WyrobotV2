#nullable enable
using System;
using Newtonsoft.Json;

namespace Wyrobot2.Data.Models
{
    public class Sanction
    {
        public SanctionType Type { get; set; }

        public DateTimeOffset IssuedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }

        [JsonIgnore]
        public bool HasExpired
        {
            get
            {
                if (IsPermanent) return false;
                return ExpiresAt < DateTimeOffset.Now;
            }
        }

        public bool IsPermanent { get; set; }

        public string? Reason { get; set; }
        
        public enum SanctionType
        {
            Warn,
            Mute,
            Kick,
            Ban
        }
    }

}