using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Wyrobot.Web
{
    public static class Configuration
    {
        
        public static class DiscordAuthClientConfig
        {
            static DiscordAuthClientConfig()
            {
                var json = JObject.Parse(File.ReadAllText("appsettings.json"));
    
                ClientId = (string) json["DiscordAuthClient"]?["ClientId"];
                ClientSecret = (string) json["DiscordAuthClient"]?["ClientSecret"];
                RedirectUrl = (string) json["DiscordAuthClient"]?["RedirectUrl"];
            }
            public static string ClientId { get; }
            public static string ClientSecret { get; }
            public static string RedirectUrl { get; }
        }

        public static class ApiConfig
        {
            static ApiConfig()
            {
                var json = JObject.Parse(File.ReadAllText("appsettings.json"));

                Guid = new Guid(((string) json["Api"]?["Guid"])!);
                BaseUrl = (string) json["Api"]?["BaseUrl"];
            }
            public static Guid Guid { get; }
            public static string BaseUrl { get; }
        }
    }
}