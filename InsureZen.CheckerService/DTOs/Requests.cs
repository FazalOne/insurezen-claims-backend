using InsureZen.Domain.Models;

namespace InsureZen.CheckerService.DTOs;

public class CheckerDecisionRequest
{
    public Decision Decision { get; set; }
    public string? Feedback { get; set; }
}
