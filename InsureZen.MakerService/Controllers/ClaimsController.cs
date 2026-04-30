using InsureZen.Domain.Data;
using InsureZen.Domain.Models;
using InsureZen.MakerService.DTOs;
using InsureZen.MakerService.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InsureZen.MakerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClaimsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("generate-token")]
    public IActionResult GenerateToken(string userId = "maker_001", string role = "Maker")
    {
        var token = TokenGenerator.GenerateToken(userId, role);
        return Ok(new { Token = token });
    }

    [HttpPost]
    public async Task<IActionResult> IngestClaim([FromBody] CreateClaimRequest request)
    {
        var claim = new Domain.Models.Claim
        {
            InsuranceCompany = request.InsuranceCompany,
            PatientName = request.PatientName,
            Amount = request.Amount,
            Status = ClaimStatus.Pending
        };

        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHistory), new { id = claim.Id }, claim);
    }

    // A shared query endpoint hosted here for history
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] ClaimStatus? status,
        [FromQuery] string? insuranceCompany,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Claims.AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(insuranceCompany))
            query = query.Where(c => c.InsuranceCompany == insuranceCompany);

        var total = await query.CountAsync();
        var claims = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { Total = total, Page = page, PageSize = pageSize, Data = claims });
    }
}
