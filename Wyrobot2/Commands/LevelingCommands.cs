using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Wyrobot2.Data;

// ReSharper disable UnusedMember.Global

namespace Wyrobot2.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LevelingCommands : BaseCommandModule
    {
        [Command("rank"), Aliases("level"), Description("Show your, or another user's, level stats.")]
        public async Task Rank(CommandContext ctx, [Description("Discord User to show the stats of.")] DiscordMember mbr = null)
        {
            if (!DataManager.GetData(ctx.Guild).Leveling.Enabled)
            {
                await ctx.RespondAsync(":x: Leveling is not enabled on this server");
            }
            
            var usrData = DataManager.GetData(mbr ?? ctx.Member, ctx.Guild);

            if (usrData == null)
            {
                await ctx.RespondAsync(":no_mouth: This user didn't talk yet");
                return;
            }

            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle($"{mbr?.Username ?? ctx.Member.Username}#{mbr?.Discriminator ?? ctx.Member.Discriminator}'s leveling stats:")
                .WithDescription($"Level: **{usrData.Level}**\nXP: **{usrData.Xp}**/{usrData.XpToNextLevel}")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithThumbnail(mbr?.AvatarUrl ?? ctx.Member.AvatarUrl)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow));
        }

        [Command("leaderboard"), Aliases("lb"), Description("Show the current leaderboard for this server.")]
        public async Task Leaderboard(CommandContext ctx)
        {
            if (!DataManager.GetData(ctx.Guild).Leveling.Enabled)
            {
                await ctx.RespondAsync(":x: Leveling is not enabled on this server");
            }
            
            var dataList = DataManager.GetAllData(ctx.Guild).OrderByDescending(d => d.Level).ThenByDescending(d => d.Xp).ToList();

            if (!dataList.Any())
            {
                await ctx.RespondAsync(":no_mouth: This server is very silent...");
                return;
            }
            
            var interactivity = ctx.Client.GetInteractivity();

            var sb = new StringBuilder();

            for (var i = 0; i < dataList.Count; i++)
            {
                if (dataList[i].Xp == 0) continue;
                var member = await ctx.Guild.GetMemberAsync(dataList[i].Id);
                sb.AppendLine($"{i + 1}. " + member.Username + "#" + member.Discriminator + $" with a level of **{dataList[i].Level}** " +
                              $"and **{dataList[i].Xp}**/{dataList[i].XpToNextLevel} XP. " +
                              $"{i switch { 0 => ":first_place:", 1 => ":second_place:", 2 => ":third_place:", _ => null }}");
            }

            await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User,
                interactivity.GeneratePagesInEmbed(sb.ToString(), SplitType.Line, new DiscordEmbedBuilder
                {
                    Title = "Level leaderboard for this server",
                    Color = DiscordColor.DarkButNotBlack
                }));
        }
    }
}