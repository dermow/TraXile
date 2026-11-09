using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TraXile.Next.Services
{
    /// <summary>
    /// The one long-lived <see cref="HttpClient"/> of the app (metadata download, update check).
    /// HttpClient is meant to be shared; a client per request exhausts sockets.
    /// </summary>
    public static class Http
    {
        private static readonly HttpClient Client = CreateClient();

        private static HttpClient CreateClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"TraXile/{UpdateChecker.AppVersion}");
            return client;
        }

        /// <summary>Downloads <paramref name="url"/> as text; throws on network errors and non-success status codes.</summary>
        public static Task<string> GetStringAsync(string url) => Client.GetStringAsync(url);
    }
}
