namespace ChordKTV.Services.Service;

using ChordKTV.Services.Api;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;

public class LRCService : ILRCService
{
    private readonly HttpClient _httpClient;

    public LRCService(HttpClient httpClient) => this._httpClient = httpClient;

    public async Task<ILRCService.LyricsDetails> GetLRCLIBLyrics(string title, string artist)
    {
        var query = $"track_name={Uri.EscapeDataString(title)}";
        query += $"&artist_name={Uri.EscapeDataString(artist)}";

        var response = await this._httpClient.GetAsync($"https://lrclib.net/api/get?{query}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ILRCService.LyricsDetails>(content);

            return result ?? throw new InvalidOperationException("Lyrics not found.");
        }

        throw new InvalidOperationException("Lyrics not found.");
    }
}
