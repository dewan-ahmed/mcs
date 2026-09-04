namespace Notify.IntegrationTests;

public class CoreApiSmokeTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Health_ReturnsSuccess_WhenCoreApiUrlIsSet()
    {
        var baseUrl = Environment.GetEnvironmentVariable("CORE_API_URL");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return;
        }

        using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        using var response = await client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }
}
