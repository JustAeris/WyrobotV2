﻿using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Wyrobot2.Data;
// ReSharper disable UnusedMember.Global

namespace Wyrobot2.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SettingsCommands : BaseCommandModule
    {
        [Command("changeprefix"), RequireUserPermissions(Permissions.Administrator)]
        public async Task ChangePrefix(CommandContext ctx, string prefix)
        {
            var data = DataContext.GetGuildData(ctx.Guild.Id);
            data.Prefix = prefix;
            DataContext.SaveGuildData(data);
            await ctx.RespondAsync(":ok:");
        }
    }
}