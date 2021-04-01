using System.IO;
using Newtonsoft.Json.Linq;

namespace Wyrobot2.Data
{
    public static class Configuration
    {
        static Configuration()
        {
            if (!File.Exists("appsettings.json"))
                throw new FileNotFoundException("Settings file has not been found", "appsettings.json");

            var json = JObject.Parse(File.ReadAllText("appsettings.json"));

            Discord = new DiscordConfiguration(
                (string) json["DiscordTokens"]?["Development"],
                (string) json["DiscordTokens"]?["Production"],
                (bool) json["DiscordTokens"]?["UseProduction"]);
            
            Lavalink = new LavalinkConfiguration(
                (string) json[nameof(Lavalink)]?["Host"],
                (int) json[nameof(Lavalink)]?["Port"],
                (string) json[nameof(Lavalink)]?["Password"]);
        }

        public static DiscordConfiguration Discord { get; }
        public static LavalinkConfiguration Lavalink { get; set; }

        public class DiscordConfiguration
        {
            public DiscordConfiguration(string development, string production, bool useProduction)
            {
                Development = development;
                Production = production;
                UseProduction = useProduction;
            }
            public string Development { get; }
            public string Production { get; }
            public bool UseProduction { get; }
        }

        public class LavalinkConfiguration
        {
            public LavalinkConfiguration(string host, int port, string password)
            {
                Host = host;
                Port = port;
                Password = password;
            }

            public string Host { get; set; }
            public int Port { get; set; }
            public string Password { get; set; }
        }
    }
}