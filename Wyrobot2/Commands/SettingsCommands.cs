using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Wyrobot2.Data;
using Wyrobot2.Data.Models;
// ReSharper disable UnusedMember.Global

namespace Wyrobot2.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SettingsCommands : BaseCommandModule
    {
        [Command("changeprefix"), RequireUserPermissions(Permissions.Administrator)]
        public async Task ChangePrefix(CommandContext ctx, string prefix)
        {
            var data = DataManager<GuildData>.GetData(new GuildData(), ctx.Guild.Id.ToString());
            data.Prefix = prefix;
            DataManager<GuildData>.SaveData(data);
            await ctx.RespondAsync(":ok:");
        }
    }
}