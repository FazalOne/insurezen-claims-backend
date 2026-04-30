using System.ComponentModel.DataAnnotations;

namespace InsureZen.Domain.Models;

public class Claim
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string InsuranceCompany { get; set; } = string.Empty;

    [Required]
    public string PatientName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

    public string? MakerId { get; set; }
    public Decision? MakerRecommendation { get; set; }
    public string? MakerFeedback { get; set; }

    public string? CheckerId { get; set; }
    public Decision? CheckerDecision { get; set; }
    public string? CheckerFeedback { get; set; }

    public DateTime? DateForwarded { get; set; }

    [Timestamp]
    public uint Version { get; set; }
}
