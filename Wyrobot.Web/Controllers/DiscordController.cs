using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Wyrobot.Web.Discord;

namespace Wyrobot.Web.Controllers
{
    public class DiscordController : Controller
    {
        private readonly DiscordAuthClient _authClient;
        private DiscordRestClient _client = Program.Client;

        public DiscordController()
        {
            _authClient = new DiscordAuthClient
            {
                ClientId = Configuration.DiscordAuthClientConfig.ClientId,
                ClientSecret = Configuration.DiscordAuthClientConfig.ClientSecret,
                RedirectUrl = Configuration.DiscordAuthClientConfig.RedirectUrl,
                Scopes = new List<string>
                {
                    "identify", "guilds"
                }
            };
        }

        public IActionResult Authorize()
        {
            return new RedirectResult(_authClient.BuildAuthorizeUrl(), false);
        }

        public async Task<IActionResult> Redirect()
        {
            if (!Request.Query.ContainsKey("code")) return new RedirectResult("/", false);

            var response = await _authClient.RequestAccessToken(Request.Query["code"]);

            Response.Cookies.Append("DiscordAuth", JsonConvert.SerializeObject(response), new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                Expires = DateTimeOffset.FromUnixTimeSeconds(int.MaxValue),
                IsEssential = true
            });
            
            _client = new DiscordRestClient(new DiscordConfiguration
            {
                Token = response.AccessToken,
                TokenType = TokenType.Bearer
            });
            await _client.InitializeCacheAsync();
            await _client.InitializeAsync();

            Program.Client = _client;

            return Redirect("/");
        }
    }
}