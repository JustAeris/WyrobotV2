using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;

namespace Wyrobot2.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            var channel = ctx.Member.VoiceState.Channel;
            
            if (channel?.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not in a voice channel.");
                return;
            }

            await node.ConnectAsync(channel);
            await ctx.RespondAsync($"Joined **{channel.Name}**!");
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            var channel = ctx.Member.VoiceState.Channel;

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not in a voice channel.");
                return;
            }

            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("The bot is not connected.");
                return;
            }

            await conn.DisconnectAsync();
            await ctx.RespondAsync($"Left {channel.Name}!");
        }
        
        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await Join(ctx);
            }

            LavalinkLoadResult loadResult;
            if (Regex.IsMatch(search ,@"https://.+"))
            {
                loadResult = await node.Rest.GetTracksAsync(new Uri(search));
                if (conn != null) await conn.PlayAsync(loadResult.Tracks.First());
                return;
            }
            
            loadResult = await node.Rest.GetTracksAsync(search,
                search.StartsWith("soundcloud:") ? LavalinkSearchType.SoundCloud : LavalinkSearchType.Youtube);
            

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed 
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for `{search}`.");
                return;
            }

            var sb = new StringBuilder();
            for (var i = 0; i < loadResult.Tracks.Take(loadResult.Tracks.Count() > 5 ? 5 : loadResult.Tracks.Count()).Count(); i++)
            {
                var track = loadResult.Tracks.ToList()[i];
                sb.AppendLine($"{(i+1 == 1 ? ":one:" : i+1 == 2 ? ":two:" : i+1 == 3 ? ":three:" : i+1 == 4 ? ":four:" : i+1 == 5 ? ":five:" : null)} {track.Title} by {track.Author}");
            }

            var message = await ctx.RespondAsync(sb.ToString());

            var interactivity = ctx.Client.GetInteractivity();
            var result = await interactivity.WaitForMessageAsync(m => m.Author == ctx.Member);

            if (result.TimedOut)
            {
                await message.ModifyAsync("Timed out!");
                return;
            }

            if (int.TryParse(result.Result.Content, out var choice))
            {
                choice = Math.Abs(choice);
                if (choice > 5)
                {
                    await message.ModifyAsync(":x: Choice cannot be above 5.");
                    return;
                }

                await conn.PlayAsync(loadResult.Tracks.ToList()[choice-1]);
            }
        }
    }
}