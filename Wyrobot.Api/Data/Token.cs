using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Wyrobot.Api.Data
{
    public static class Token
    {
        internal static Guid ApiGuid { get; } =
            new(((string) JObject.Parse(File.ReadAllText("appsettings.json"))[nameof(ApiGuid)])!);
        
        public static string Generate(Guid guid, bool htmlEncode = true)
        {
            IEnumerable<byte> bytes = Encoding.UTF8.GetBytes(DateTime.Today.ToLongDateString());

            IEnumerable<byte> bytes2 = guid.ToByteArray();

            var finalBytes = bytes.Concat(bytes2).ToArray();

            var sha = System.Security.Cryptography.SHA384.Create();

            var keyBytes = sha.ComputeHash(finalBytes);

            var key = Convert.ToBase64String(keyBytes);

            return htmlEncode ? HttpUtility.UrlEncode(key) : key;
        }

        public static bool Authorize(string key, bool isEncoded = true)
        {
            var finalKey = Generate(ApiGuid, isEncoded);

            return finalKey == key;
        }
    }
}