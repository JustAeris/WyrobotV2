using Microsoft.Extensions.Logging;

namespace Wyrobot2.Events
{
    public struct EventIds
    {
        public static readonly EventId Error = new EventId(0, "Error");
        public static readonly EventId Warning = new EventId(1, "Warning");
        
        public static readonly EventId Ban = new EventId(2, "Ban");
        public static readonly EventId Unban = new EventId(2, "Unban");
        public static readonly EventId Kick = new EventId(3, "Kick");
        public static readonly EventId Warn = new EventId(4, "Warn");
        public static readonly EventId Mute = new EventId(5, "Mute");
        public static readonly EventId Unmute = new EventId(5, "Unmute");

        public static readonly EventId Scheduled = new EventId(6, "Scheduled");
        public static readonly EventId ScheduledError = new EventId(6, "Error");

        public static readonly EventId CommandExecution = new EventId(7, "CmdExe");

        public static readonly EventId GuildRelated = new EventId(8, "Guild");
    }
}