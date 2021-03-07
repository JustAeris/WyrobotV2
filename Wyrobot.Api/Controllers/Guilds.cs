using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wyrobot.Api.Data;
using Wyrobot.Api.Models;
using Wyrobot2.Data.Models;
using DataManager = Wyrobot.Api.Data.DataManager;

namespace Wyrobot.Api.Controllers
{
    public class Guilds : Controller
    {
        // GET
        [Route("api/[controller]/{userId}/{id}")]
        public async Task<object> GetGuildSettings(string apiKey, ulong id, ulong userId)
        {
            if (!Token.Authorize(apiKey, false))
                return new ApiResponse
                {
                    Status = 401,
                    Message = "Not authorized: Invalid API key."
                };

            try
            {
                var gld = await Client.DiscordClient.GetGuildAsync(id);
                var dummy = await gld.GetMemberAsync(userId);
            }
            catch
            {
                return new ApiResponse
                {
                    Status = 404,
                    Message = "Not found: Neither the guild or the user have been found."
                };
            }
            
            var data = new GuildSettings(id, userId);
            
            try
            {
                data.Settings = DataManager.GetData<GuildData>(id);
            }
            catch
            {
                return new ApiResponse
                {
                    Status = 404,
                    Message = "Not found: Configuration file has not been found."
                };
            }

            await data.GetRolesAndChannels();

            return data;
        }
    }
}