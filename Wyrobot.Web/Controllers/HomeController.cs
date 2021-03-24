using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Wyrobot.Web.Discord;
using Wyrobot.Web.Models;

namespace Wyrobot.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private DiscordRestClient _client = Program.Client;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            ViewData.Add("canEditGuild", true);
            ViewData.Add("userPfp", "https://aeris.dev/assets/nut.gif");
            ViewData.Add("manageableGuilds", new List<DiscordGuild>());
            
            if (_client == null)
            {
                if (Request.Cookies["DiscordAuth"] == null)
                {
                    ViewData["canEditGuild"] = false;
                    return View();
                }
                var cookie = JsonConvert.DeserializeObject<DiscordTokenResponse>(Request.Cookies["DiscordAuth"]!);
                if (cookie == null)
                {
                    ViewData["canEditGuild"] = false;
                    return View();
                }
                _client = new DiscordRestClient(new DiscordConfiguration
                {
                    Token = cookie.AccessToken,
                    TokenType = TokenType.Bearer
                });
                await _client.InitializeCacheAsync();
                await _client.InitializeAsync();
                
                _logger.LogInformation("Client initialized!");

                Program.Client = _client;
            }

            var manageableGuilds = new List<DiscordGuild>();
            foreach (var (unused, guild) in _client.Guilds)
            {
                if (guild.Permissions.HasValue && guild.Permissions.GetValueOrDefault().HasPermission(Permissions.ManageGuild))
                    manageableGuilds.Add(guild);
            }
            
            ViewData["manageableGuilds"] = manageableGuilds;
            ViewData["userPfp"] = _client.CurrentUser.AvatarUrl;
            
            return View(_client);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}