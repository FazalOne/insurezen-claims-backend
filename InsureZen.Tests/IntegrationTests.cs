using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using InsureZen.Domain.Data;
using InsureZen.Domain.Models;
using InsureZen.MakerService.DTOs;
using InsureZen.CheckerService.DTOs;
using InsureZen.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InsureZen.Tests;

public class IntegrationTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public IntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    private static WebApplicationFactory<TEntryPoint> CreateFactory<TEntryPoint>(string connectionString)
        where TEntryPoint : class
    {
        return new WebApplicationFactory<TEntryPoint>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<AppDbContext>>();
                    services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
                });
            });
    }

    [Fact]
    public async Task Maker_And_Checker_Flow_Works_End_To_End()
    {
        await using var makerFactory = CreateFactory<InsureZen.MakerService.MakerServiceEntryPoint>(_fixture.ConnectionString);
        await using var checkerFactory = CreateFactory<InsureZen.CheckerService.CheckerServiceEntryPoint>(_fixture.ConnectionString);

        var makerClient = makerFactory.CreateClient();
        var checkerClient = checkerFactory.CreateClient();

        var makerToken = await GetTokenAsync(makerClient, "/api/Claims/generate-token?userId=maker_001&role=Maker");
        makerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", makerToken);

        var createResponse = await makerClient.PostAsJsonAsync("/api/Claims", new CreateClaimRequest
        {
            InsuranceCompany = "Acme",
            PatientName = "John Doe",
            Amount = 120.50m
        });
        createResponse.EnsureSuccessStatusCode();

        var created = await ReadJsonAsync(createResponse);
        var claimId = created.RootElement.GetProperty("id").GetGuid();

        var assignResponse = await makerClient.PostAsync($"/api/maker/claims/{claimId}/assign", null);
        assignResponse.EnsureSuccessStatusCode();

        var recommendResponse = await makerClient.PostAsJsonAsync($"/api/maker/claims/{claimId}/recommend", new MakerRecommendationRequest
        {
            Recommendation = Decision.Approve,
            Feedback = "Looks good"
        });
        recommendResponse.EnsureSuccessStatusCode();

        var checkerToken = await GetTokenAsync(checkerClient, "/api/checker/generate-token?userId=checker_001&role=Checker");
        checkerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", checkerToken);

        var checkerList = await checkerClient.GetAsync("/api/checker/claims");
        checkerList.EnsureSuccessStatusCode();

        var checkerAssign = await checkerClient.PostAsync($"/api/checker/claims/{claimId}/assign", null);
        checkerAssign.EnsureSuccessStatusCode();

        var decideResponse = await checkerClient.PostAsJsonAsync($"/api/checker/claims/{claimId}/decide", new CheckerDecisionRequest
        {
            Decision = Decision.Approve,
            Feedback = "Approved"
        });
        decideResponse.EnsureSuccessStatusCode();
    }

    private static async Task<string> GetTokenAsync(HttpClient client, string url)
    {
        var response = await client.PostAsync(url, null);
        response.EnsureSuccessStatusCode();

        var json = await ReadJsonAsync(response);
        return json.RootElement.GetProperty("token").GetString() ?? string.Empty;
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }
}
