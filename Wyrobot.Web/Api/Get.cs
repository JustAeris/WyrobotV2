using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Wyrobot.Web.Api
{
    public class Get : IDisposable
    {
        public void Dispose()
        {
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }

        private readonly HttpClient _client;
        
        public Get()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(Configuration.ApiConfig.BaseUrl)
            };
        }
        
        public async Task<T> GetT<T>(string url)
        {
            // Add an Accept header for JSON format.
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            var response = await _client.GetAsync(url);  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                return await response.Content.ReadAsAsync<T>();  //Make sure to add a reference to System.Net.Http.Formatting.dll
            }

            Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            return default;
        }
    }
}