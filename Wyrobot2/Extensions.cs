﻿using DSharpPlus.Entities;

namespace Wyrobot2
{
    public static class Extensions
    {
        public static bool CanPunish(this DiscordMember mbr1, DiscordMember mbr2) => mbr1.Hierarchy > mbr2.Hierarchy;

        public static string Tag(this DiscordMember member) => member.Username + "#" + member.Discriminator;
        public static string Tag(this DiscordUser user) => Tag((DiscordMember) user);
    }
}