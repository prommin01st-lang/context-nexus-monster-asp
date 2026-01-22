using System.Net.Http.Headers;
using System.Text.Json;
using DevContextNexus.API.Configuration;
using Microsoft.Extensions.Options;

namespace DevContextNexus.API.Services
{
    public interface IGitHubService
    {
        Task<string> FetchFileContentAsync(string filePath);
        Task<string?> GetFileShaAsync(string filePath);
        Task<string> UpsertFileAsync(string filePath, string content, string? sha = null);
        Task DeleteFileAsync(string filePath, string sha);
    }

    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly GitHubSettings _settings;

        public GitHubService(HttpClient httpClient, IOptions<GitHubSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;

            _httpClient.BaseAddress = new Uri("https://api.github.com/");
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DevContextNexus", "1.0"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.PersonalAccessToken);
        }

        public async Task<string> FetchFileContentAsync(string filePath)
        {
            // GET /repos/{owner}/{repo}/contents/{path}
            // Note: This returns JSON with base64 encoded content. 
            // For raw content, use explicit Accept header or download_url. 
            // Simpler approach: Use raw.githubusercontent.com or Accept: application/vnd.github.raw+json

            var request = new HttpRequestMessage(HttpMethod.Get, $"repos/{_settings.RepositoryOwner}/{_settings.RepositoryName}/contents/{filePath}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.raw+json"));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string?> GetFileShaAsync(string filePath)
        {
             var request = new HttpRequestMessage(HttpMethod.Get, $"repos/{_settings.RepositoryOwner}/{_settings.RepositoryName}/contents/{filePath}");
             // Default Accept header returns JSON metadata including SHA
             
             var response = await _httpClient.SendAsync(request);
             if (!response.IsSuccessStatusCode) return null;

             var json = await response.Content.ReadAsStringAsync();
             using var doc = JsonDocument.Parse(json);
             return doc.RootElement.GetProperty("sha").GetString();
        }
        public async Task<string> UpsertFileAsync(string filePath, string content, string? sha = null)
        {
            // PUT /repos/{owner}/{repo}/contents/{path}
            var request = new HttpRequestMessage(HttpMethod.Put, $"repos/{_settings.RepositoryOwner}/{_settings.RepositoryName}/contents/{filePath}");
            
            var payload = new
            {
                message = $"Update context: {filePath}",
                content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content)),
                sha = sha // Optional: used for updating existing files
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("content").GetProperty("sha").GetString()!;
        }

        public async Task DeleteFileAsync(string filePath, string sha)
        {
            // DELETE /repos/{owner}/{repo}/contents/{path}
            var request = new HttpRequestMessage(HttpMethod.Delete, $"repos/{_settings.RepositoryOwner}/{_settings.RepositoryName}/contents/{filePath}");

            var payload = new
            {
                message = $"Delete context: {filePath}",
                sha = sha
            };

            var json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
