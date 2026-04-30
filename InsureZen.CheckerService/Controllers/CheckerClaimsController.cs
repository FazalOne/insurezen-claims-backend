using InsureZen.Domain.Data;
using InsureZen.Domain.Models;
using InsureZen.CheckerService.DTOs;
using InsureZen.CheckerService.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InsureZen.CheckerService.Controllers;

[ApiController]
[Route("api/checker")]
public class CheckerClaimsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CheckerClaimsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("generate-token")]
    public IActionResult GenerateToken(string userId = "checker_001", string role = "Checker")
    {
        var token = TokenGenerator.GenerateToken(userId, role);
        return Ok(new { Token = token });
    }

    [HttpGet("claims")]
    [Authorize(Roles = "Checker")]
    public async Task<IActionResult> GetPending(int page = 1, int pageSize = 10)
    {
        var claims = await _context.Claims
            .Where(c => c.Status == ClaimStatus.PendingCheckerReview)
            .OrderBy(c => c.SubmittedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(claims);
    }

    [HttpPost("claims/{id}/assign")]
    [Authorize(Roles = "Checker")]
    public async Task<IActionResult> Assign(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var claim = await _context.Claims.FindAsync(id);
        if (claim == null) return NotFound();

        if (claim.Status != ClaimStatus.PendingCheckerReview)
            return BadRequest(new { Message = "Claim is not pending checker review." });

        if (claim.MakerId == userId)
            return BadRequest(new { Message = "A Maker cannot be the Checker for the same claim." });

        if (!string.IsNullOrEmpty(claim.CheckerId))
            return Conflict(new { Message = "Claim is already assigned to a checker." });

        claim.Status = ClaimStatus.UnderCheckerReview;
        claim.CheckerId = userId;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Claim assigned successfully.", ClaimId = id });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { Message = "Claim was assigned by someone else concurrently." });
        }
    }

    [HttpPost("claims/{id}/decide")]
    [Authorize(Roles = "Checker")]
    public async Task<IActionResult> SubmitDecision(Guid id, [FromBody] CheckerDecisionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var claim = await _context.Claims.FindAsync(id);
        if (claim == null) return NotFound();

        if (claim.Status != ClaimStatus.UnderCheckerReview)
            return BadRequest(new { Message = "Claim is not under Checker review." });

        if (claim.CheckerId != userId)
            return Forbid(); // Checker cannot submit for a claim they did not assign

        claim.Status = request.Decision == Decision.Approve ? ClaimStatus.Approved : ClaimStatus.Rejected;
        claim.CheckerDecision = request.Decision;
        claim.CheckerFeedback = request.Feedback;
        claim.DateForwarded = DateTime.UtcNow; // Mocking the forwarding upstream action.

        // Log upstream forward
        Console.WriteLine($"[UPSTREAM FORWARD] Claim {claim.Id} was finalized as {claim.Status} and forwarded to {claim.InsuranceCompany}.");

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Decision finalized and claim forwarded successfully." });
    }
}
