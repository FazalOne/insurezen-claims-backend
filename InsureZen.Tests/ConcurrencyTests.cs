using System.Security.Claims;
using InsureZen.CheckerService.Controllers;
using InsureZen.CheckerService.DTOs;
using InsureZen.Domain.Data;
using InsureZen.Domain.Models;
using InsureZen.MakerService.Controllers;
using InsureZen.MakerService.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DomainClaim = InsureZen.Domain.Models.Claim;
using SecurityClaim = System.Security.Claims.Claim;

namespace InsureZen.Tests;

public class WorkflowTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static MakerClaimsController CreateMakerController(AppDbContext context, string userId)
    {
        var controller = new MakerClaimsController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new SecurityClaim(ClaimTypes.NameIdentifier, userId),
                        new SecurityClaim(ClaimTypes.Role, "Maker")
                    }, "Test"))
                }
            }
        };

        return controller;
    }

    private static CheckerClaimsController CreateCheckerController(AppDbContext context, string userId)
    {
        var controller = new CheckerClaimsController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new SecurityClaim(ClaimTypes.NameIdentifier, userId),
                        new SecurityClaim(ClaimTypes.Role, "Checker")
                    }, "Test"))
                }
            }
        };

        return controller;
    }

    [Fact]
    public async Task Maker_Assign_Then_Recommend_Updates_Claim()
    {
        using var context = CreateContext();

        var claim = new DomainClaim
        {
            InsuranceCompany = "Acme",
            PatientName = "John Doe",
            Amount = 100.0m,
            Status = ClaimStatus.Pending
        };

        context.Claims.Add(claim);
        await context.SaveChangesAsync();

        var controller = CreateMakerController(context, "maker_001");

        var assignResult = await controller.Assign(claim.Id);
        Assert.IsType<OkObjectResult>(assignResult);

        var assigned = await context.Claims.FindAsync(claim.Id);
        Assert.NotNull(assigned);
        Assert.Equal(ClaimStatus.UnderMakerReview, assigned!.Status);
        Assert.Equal("maker_001", assigned.MakerId);

        var recommendResult = await controller.SubmitRecommendation(claim.Id, new MakerRecommendationRequest
        {
            Recommendation = Decision.Approve,
            Feedback = "Looks good"
        });

        Assert.IsType<OkObjectResult>(recommendResult);

        var updated = await context.Claims.FindAsync(claim.Id);
        Assert.NotNull(updated);
        Assert.Equal(ClaimStatus.PendingCheckerReview, updated!.Status);
        Assert.Equal(Decision.Approve, updated.MakerRecommendation);
        Assert.Equal("Looks good", updated.MakerFeedback);
    }

    [Fact]
    public async Task Checker_Assign_Then_Decide_Updates_Claim()
    {
        using var context = CreateContext();

        var claim = new DomainClaim
        {
            InsuranceCompany = "Acme",
            PatientName = "Jane Doe",
            Amount = 200.0m,
            Status = ClaimStatus.PendingCheckerReview,
            MakerId = "maker_001",
            MakerRecommendation = Decision.Reject
        };

        context.Claims.Add(claim);
        await context.SaveChangesAsync();

        var controller = CreateCheckerController(context, "checker_001");

        var assignResult = await controller.Assign(claim.Id);
        Assert.IsType<OkObjectResult>(assignResult);

        var assigned = await context.Claims.FindAsync(claim.Id);
        Assert.NotNull(assigned);
        Assert.Equal(ClaimStatus.UnderCheckerReview, assigned!.Status);
        Assert.Equal("checker_001", assigned.CheckerId);

        var decisionResult = await controller.SubmitDecision(claim.Id, new CheckerDecisionRequest
        {
            Decision = Decision.Reject,
            Feedback = "Insufficient documents"
        });

        Assert.IsType<OkObjectResult>(decisionResult);

        var updated = await context.Claims.FindAsync(claim.Id);
        Assert.NotNull(updated);
        Assert.Equal(ClaimStatus.Rejected, updated!.Status);
        Assert.Equal(Decision.Reject, updated.CheckerDecision);
        Assert.Equal("Insufficient documents", updated.CheckerFeedback);
        Assert.NotNull(updated.DateForwarded);
    }
}
