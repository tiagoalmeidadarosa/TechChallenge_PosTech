using System.Text;
using System.Text.Json;

namespace TechChallenge_Fase01.Tests.Helpers;

public static class HttpContentExtensions
{
    public static async Task<T?> ReadAs<T>(this HttpContent content)
    {
        var data = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(data, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public static HttpContent From<T>(T content)
    {
        StringContent stringContent = new(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
        return stringContent;
    }
}
