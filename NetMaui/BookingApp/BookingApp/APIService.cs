using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BookingApp.Services;

public class APIService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "http://localhost:8080/api/"; // For Android emulator http://10.0.2.2:8080/api/
                                                                     // Use "" for Windows app testing

    public APIService()
    {
        _httpClient = new HttpClient();
        Console.WriteLine($"Using base URL: {_baseUrl}");
    }

    // Generic GET method
    public async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    // Generic POST method
    public async Task<T> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            Console.WriteLine($"Sending to {_baseUrl}{endpoint}: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);

            // Log and get the response
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Received response: {responseContent}");

            response.EnsureSuccessStatusCode();

            // Configure JSON serializer options to be case-insensitive
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(responseContent, options);
        }
        catch (HttpRequestException ex)
        {
            // Try to get response content if available
            if (ex.Data.Contains("ResponseContent"))
            {
                Console.WriteLine($"Error response: {ex.Data["ResponseContent"]}");
            }
            throw;
        }
    }

    // Generic PUT method
    public async Task<T> PutAsync<T>(string endpoint, object data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{_baseUrl}{endpoint}", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    // DELETE method
    public async Task DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}{endpoint}");
        response.EnsureSuccessStatusCode();
    }
}
