using InsureZen.Domain.Models;

namespace InsureZen.MakerService.DTOs;

public class CreateClaimRequest
{
    public string InsuranceCompany { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class MakerRecommendationRequest
{
    public Decision Recommendation { get; set; }
    public string? Feedback { get; set; }
}
