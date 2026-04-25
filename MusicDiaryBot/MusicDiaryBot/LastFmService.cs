using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MusicDiaryBot
{
    public class LastFmService
    {
        private readonly string apiKey = "5a3c1b919b32d1f15e86c71079c3a877";
        private readonly HttpClient httpClient = new HttpClient();
        public async Task<List<string>> GetSimilarArtistsAsync(string artist)
        {
            string url = $"https://ws.audioscrobbler.com/2.0/" +
                         $"?method=artist.getsimilar" +
                         $"&artist={Uri.EscapeDataString(artist)}" +
                         $"&api_key={apiKey}" +
                         $"&format=json" +
                         $"&limit=5";

            string response = await httpClient.GetStringAsync(url);

            JObject json = JObject.Parse(response);

            var similarArtists = json["similarartists"]?["artist"];
            if (similarArtists == null) return new List<string>();

            return similarArtists.Select(a => a["name"]?.ToString() ?? "").Where(name => name != "").ToList();
        }

    }
}
