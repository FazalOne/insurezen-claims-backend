namespace InsureZen.Domain.Models;

public enum ClaimStatus
{
    Pending,
    UnderMakerReview,
    PendingCheckerReview,
    UnderCheckerReview,
    Approved,
    Rejected
}

public enum Decision
{
    Approve,
    Reject
}
