using InsureZen.Domain.Data;
using InsureZen.Domain.Models;
using InsureZen.MakerService.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InsureZen.MakerService.Controllers;

[ApiController]
[Route("api/maker/claims")]
[Authorize(Roles = "Maker")]
public class MakerClaimsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MakerClaimsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPending(int page = 1, int pageSize = 10)
    {
        var claims = await _context.Claims
            .Where(c => c.Status == ClaimStatus.Pending)
            .OrderBy(c => c.SubmittedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(claims);
    }

    [HttpPost("{id}/assign")]
    public async Task<IActionResult> Assign(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var claim = await _context.Claims.FindAsync(id);
        if (claim == null) return NotFound();

        if (claim.Status != ClaimStatus.Pending)
            return BadRequest(new { Message = "Claim is not pending." });

        if (!string.IsNullOrEmpty(claim.MakerId))
            return Conflict(new { Message = "Claim is already assigned." });

        claim.Status = ClaimStatus.UnderMakerReview;
        claim.MakerId = userId;

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

    [HttpPost("{id}/recommend")]
    public async Task<IActionResult> SubmitRecommendation(Guid id, [FromBody] MakerRecommendationRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var claim = await _context.Claims.FindAsync(id);
        if (claim == null) return NotFound();

        if (claim.Status != ClaimStatus.UnderMakerReview)
            return BadRequest(new { Message = "Claim is not under Maker review." });

        if (claim.MakerId != userId)
            return Forbid(); // Maker cannot submit for a claim they did not assign

        claim.Status = ClaimStatus.PendingCheckerReview;
        claim.MakerRecommendation = request.Recommendation;
        claim.MakerFeedback = request.Feedback;

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Recommendation submitted successfully." });
    }
}
