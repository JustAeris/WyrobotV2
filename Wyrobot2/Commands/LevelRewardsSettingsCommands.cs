using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Wyrobot2.Data;
using Wyrobot2.Data.Models;
// ReSharper disable UnusedMember.Global

namespace Wyrobot2.Commands
{
    [Group("levelreward")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LevelRewardsSettingsCommands : BaseCommandModule
    {
        [Command("add"), Description("Adds a level reward to this guild.")]
        public async Task Add(CommandContext ctx, [Description("Required level to give the reward.")] int requiredLevel, [Description("Role to award.")] DiscordRole role)
        {
            var data = await DataManager.GetData(ctx.Guild);
            data.Leveling.LevelRewards ??= new List<LevelReward>();
            
            data.Leveling.LevelRewards.Add(new LevelReward(requiredLevel, role.Id));
            
            DataManager.SaveData(data);
            
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":white_check_mark: Success!")
                .WithDescription($"{role.Mention} will be awarded on level **{requiredLevel}**")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow));
        }
        
        [Command("remove"), Aliases("del"), Description("Removes a level reward from this guild.")]
        public async Task Remove(CommandContext ctx, [Description("Required level of the reward to remove.")] int requiredLevel, [Description("Role of the reward to remove.")] DiscordRole role)
        {
            var data = await DataManager.GetData(ctx.Guild);
            data.Leveling.LevelRewards ??= new List<LevelReward>();
            
            data.Leveling.LevelRewards.Add(new LevelReward(requiredLevel, role.Id));
            
            DataManager.SaveData(data);
            
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle(":white_check_mark: Success!")
                .WithDescription($"{role.Mention} will no longer be awarded on level **{requiredLevel}**")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithFooter($"Issuer ID: {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Build());
        }
        
        [Command("list"), Description("Lists all available level rewards for this guild.")]
        public async Task List(CommandContext ctx)
        {
            var dataList = (await DataManager.GetData(ctx.Guild)).Leveling.LevelRewards.OrderBy(lr => lr.RequiredLevel).ToList();
            
            if (!dataList.Any())
            {
                await ctx.RespondAsync(":no_mouth: This server is very silent...");
                return;
            }
            
            var interactivity = ctx.Client.GetInteractivity();

            var sb = new StringBuilder();

            foreach (var reward in dataList)
                sb.AppendLine($"Role {ctx.Guild.GetRole(reward.RoleId).Mention} will be awarded on level **{reward.RequiredLevel}**");

            await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User,
                interactivity.GeneratePagesInEmbed(sb.ToString(), SplitType.Line, new DiscordEmbedBuilder
                {
                    Title = "Level rewards list for this server",
                    Color = DiscordColor.DarkButNotBlack
                }));
        }
    }
}