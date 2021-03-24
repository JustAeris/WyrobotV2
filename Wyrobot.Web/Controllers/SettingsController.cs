using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Wyrobot.Api.Data;
using Wyrobot.Api.Models;
using Wyrobot.Web.Discord;
using Wyrobot2.Data.Models;

namespace Wyrobot.Web.Controllers
{
    public class SettingsController : Controller
    {
        private DiscordRestClient _client = Program.Client;
        private static readonly HttpClient HttpClient = new()
        {
            BaseAddress = new Uri("https://localhost:44311/")
        };

        public SettingsController()
        {
            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        // GET
        public async Task<IActionResult> Index(ulong userId ,ulong guildId)
        {
            if (_client == null)
            {
                if (Request.Cookies["DiscordAuth"] == null) return View();
                var cookie = JsonConvert.DeserializeObject<DiscordTokenResponse>(Request.Cookies["DiscordAuth"]!);
                if (cookie == null) return View();
                _client = new DiscordRestClient(new DiscordConfiguration
                {
                    Token = cookie.AccessToken,
                    TokenType = TokenType.Bearer
                });
                await _client.InitializeCacheAsync();
                await _client.InitializeAsync();

                Program.Client = _client;
            }

            if (_client.CurrentUser.Id != userId) return Unauthorized();

            var (_, value) = _client.Guilds.FirstOrDefault(pair => pair.Key == guildId);
            if (value == null) return NotFound();

            if (!value.Permissions.GetValueOrDefault().HasPermission(Permissions.ManageGuild))
                return Unauthorized();

            GuildSettings settings;
            using (var apiClient = new Api.Get())
            {
                settings = await apiClient.GetT<GuildSettings>($"v1/guilds/{userId}/{guildId}?key={Token.Generate(Configuration.ApiConfig.Guid)}");
            }
            ViewData.Add("settings", settings);
            
            return View();
        }

        public string ShowValues(ulong guildId, [Bind("Leveling")] GuildData settings)
        {
            return settings.Leveling.Message;
        }
    }
}