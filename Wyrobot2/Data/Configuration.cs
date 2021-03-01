using System.IO;
using Newtonsoft.Json.Linq;

namespace Wyrobot2.Data
{
    public static class Configuration
    {
        static Configuration()
        {
            if (!File.Exists("config.json"))
                return;

            var json = JObject.Parse(File.ReadAllText("appsettings.json"));

            DDevelopment = (string) json["DiscordTokens"]?["Development"];
            DProduction = (string) json["DiscordTokens"]?["Production"];
            DUseProduction = (bool) json["DiscordTokens"]?["UseProduction"];
        }

        public static string DDevelopment { get; }
        public static string DProduction { get; }
        public static bool DUseProduction { get; }
    }
}