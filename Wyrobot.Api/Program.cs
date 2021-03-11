using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Wyrobot.Api.Data;

namespace Wyrobot.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args[0] == "getkey") Console.WriteLine(Token.Generate(false));
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}