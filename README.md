# InsureZen

An InsureZen Backend Architecture implementing Maker-Checker workflows for medical insurance claim processing.

## Technologies Used

- **Language**: C# (.NET 9.0)
- **Architecture**: Separated Maker and Checker REST APIs sharing a domain database.
- **Database**: PostgreSQL
- **ORMs/Data Access**: Entity Framework Core
- **Authentication**: Simple JWT generation / bearer tokens and standard Policy roles.

## Design & Assumptions

- **Design Process**: Read through [REQUIREMENTS.md](REQUIREMENTS.md) and [API_DESIGN.md](API_DESIGN.md).
- **Concurrency Strategy**: Handled using EntityFramework Core's `[Timestamp]` property (RowVersion equivalent) utilizing Optimistic Concurrency Control (OCC). If two Makers try to claim the same record, one will fail context saving with a `DbUpdateConcurrencyException`.
- **Database**: Separate microservices conventionally hold their own context/db. To simplify for this assessment but still fulfill the multiple App concept, I used a shared single Database via EF Code First Migrations hosted from `MakerService`.
- **Forwarding to Insurance App**: As instructed, simulating upstream transmission by outputting logs in the `CheckerService`.

## Setup & Running

**Prerequisites:** Docker, Docker Compose, and `.NET 9.0 SDK` (if wanting to build/run via host rather than compose).

### Method 1: Running with Docker Compose (Recommended)

From the root repository block run:

```bash
docker compose up --build
```

- **Maker Service URL**: `http://localhost:5100/swagger`
- **Checker Service URL**: `http://localhost:5101/swagger`

_Note: Since automatic EF migrations run out of the CLI, you will need to apply migrations manually or configure automatic applying migrations in `Program.cs` before deploying via docker image. The included codebase runs `context.Database.MigrateAsync()` in background logic on startup, meaning starting docker compose handles DB schema application directly._

### Testing Flow

1. Open the [Maker Service Swagger](http://localhost:5100/swagger) and locate `/api/Claims/generate-token?userId=123&role=Maker`. Call and copy the Token.
2. In Swagger find "Authorize" button, paste `Bearer <token>`
3. Call `/api/Claims` to generate a new Claim.
4. Call `/api/maker/claims` to fetch your claims, notice Status is 0 (Pending).
5. Call `/api/maker/claims/{id}/assign` to assign it to yourself (Status -> 1).
6. Call `/api/maker/claims/{id}/recommend` to approve it (Status -> 2).
7. Open [Checker Service Swagger](http://localhost:5101/swagger) and generate checker token (`role=Checker`). Authorize context.
8. Call `/api/checker/claims` to list it.
9. Assign and decide using similar workflow. Ensure you see the "[UPSTREAM FORWARD]" console log.

### Method 2: Local without Docker (Requires a running Postgres DB locally)

1. Run `dotnet ef database update --project InsureZen.MakerService`
2. Run `dotnet run --project InsureZen.MakerService`
3. Run `dotnet run --project InsureZen.CheckerService`
