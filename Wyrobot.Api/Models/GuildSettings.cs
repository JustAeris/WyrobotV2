using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Wyrobot2.Data.Models;

namespace Wyrobot.Api.Models
{
    public class GuildSettings
    {
        public GuildSettings(ulong id, ulong userId)
        {
            Id = id;
            UserId = userId;
        }
        
        public ulong Id { get; }

        private ulong UserId { get; }
        public GuildData Settings { get; set; }
        
        public IEnumerable<Role> Roles { get; private set; }

        public IEnumerable<Channel> Channels { get; private set; }

        public async Task GetRolesAndChannels()
        {
            var guild = await Client.DiscordClient.GetGuildAsync(Id);
            var member = await guild.GetMemberAsync(UserId);

            Roles = guild.Roles.Values
                .Where(role => member.Hierarchy > role.Position)
                .Select(role => new Role { Id = role.Id, Name = role.Name, Color = role.Color }).ToList();
            
            Channels = guild.Channels.Values
                .Where(c => !c.IsCategory && c.Users.Contains(member) && c.Type == ChannelType.Text)
                .Select(channel => new Channel {Id = channel.Id, Name = channel.Name}).ToList();
        }
    }

    public class Role
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public DiscordColor Color { get; set; }
    }

    public class Channel
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public bool IsCategory { get; set; }
    }
}