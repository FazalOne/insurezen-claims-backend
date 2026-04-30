using InsureZen.Domain.Data;
using InsureZen.Domain.Models;
using InsureZen.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace InsureZen.Tests;

public class ConcurrencyIntegrationTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public ConcurrencyIntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Two_Makers_Cannot_Assign_Same_Claim_Concurrently()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_fixture.ConnectionString, b => b.MigrationsAssembly("InsureZen.MakerService"))
            .Options;

        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.MigrateAsync();
            setupContext.Claims.Add(new Claim
            {
                InsuranceCompany = "Acme",
                PatientName = "Concurrency Test",
                Amount = 150.0m,
                Status = ClaimStatus.Pending
            });
            await setupContext.SaveChangesAsync();
        }

        await using var context1 = new AppDbContext(options);
        await using var context2 = new AppDbContext(options);

        var claim1 = await context1.Claims.SingleAsync();
        var claim2 = await context2.Claims.SingleAsync();

        claim1.Status = ClaimStatus.UnderMakerReview;
        claim1.MakerId = "maker_001";

        claim2.Status = ClaimStatus.UnderMakerReview;
        claim2.MakerId = "maker_002";

        await context1.SaveChangesAsync();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context2.SaveChangesAsync());
    }
}
