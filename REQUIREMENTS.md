# InsureZen - Requirements Analysis

## Entities and Data Points

1. **Claim**: Represents a medical insurance claim.
   - `Id` (GUID / UUID): Unique identifier.
   - `InsuranceCompany` (String): The insurance company this claim belongs to.
   - `PatientName` (String): The name of the patient.
   - `Amount` (Decimal): The amount claimed.
   - `SubmittedDate` (DateTime): Date the claim was originally submitted.
   - `Status` (Enum/String): `Pending`, `UnderMakerReview`, `PendingCheckerReview`, `UnderCheckerReview`, `Approved`, `Rejected`.
   - `MakerId` (String, nullable): ID of the Maker currently reviewing or who reviewed it.
   - `CheckerId` (String, nullable): ID of the Checker currently reviewing or who reviewed it.
   - `MakerRecommendation` (String, nullable): `Approve` or `Reject`.
   - `MakerFeedback` (String, nullable): Notes from the Maker.
   - `CheckerDecision` (String, nullable): Final decision, `Approve` or `Reject`.
   - `CheckerFeedback` (String, nullable): Notes from the Checker.
   - `DateForwarded` (DateTime, nullable): The date the final decision was forwarded upstream.
   - `Version` (byte[] / uint): Concurrency token for optimistic concurrency.

2. **User** (handled via claims in JWT or minimal representation since we are mocking auth):
   - `UserId` (String): Unique identifier.
   - `Role` (String): `Maker`, `Checker`.

## Actors

- **Maker**: Reviews newly ingested claims, annotates with feedback, and provides a recommendation (Approve/Reject).
- **Checker**: Reviews claims that have a Maker recommendation. Agrees or disagrees and issues the final decision. Forwarding to upstream occurs automatically here.
- **Upstream Service** (System): Submits standardized claim payload to the API.

## Functional Requirements

- **Claim Ingestion**: API endpoint to receive structured claim data.
- **Claim Retrieval for Maker**: Endpoint to fetch claims in `Pending` status.
- **Maker Review Assignment**: A Maker claims a record for review (changes status). Prevent concurrent reviews.
- **Maker Submission**: Maker submits recommendation and feedback. Claim moves to `PendingCheckerReview`.
- **Claim Retrieval for Checker**: Endpoint to fetch claims in `PendingCheckerReview` status.
- **Checker Review Assignment**: A Checker claims a record for review.
- **Checker Submission**: Checker submits final decision. Claim moves to `Approved` or `Rejected` and forwarding is simulated/logged.
- **Query / History**: Paginated, filterable endpoint for claims history.

## Non-Functional Requirements

- **Concurrency**: Claim state transitions must use optimistic concurrency control (OCC) to prevent two makers or two checkers from assigning the same claim.
- **Security**: JWT-based Authentication with Role-Based Access Control (RBAC).
- **Microservices Boundary**: Maker operations decoupled from Checker operations. Shared Db context/Domain for simplicity in this assessment, but separate API hosts.

## Edge Cases and Assumptions

- _Assumption_: Maker and Checker cannot be the same user. Checked at Checker assignment phase.
- _Assumption_: A Maker can only review a claim once. No re-evaluations.
- _Assumption_: Forwarding to the insurance company is synchronous or "fire and forget". We will mock it with a simple log.
- _Edge Case_: Concurrency - If User A and User B request to assign the same `Pending` claim at the same exact time, only one should succeed. Addressed via database row versioning / OCC.
