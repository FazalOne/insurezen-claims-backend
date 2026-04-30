# InsureZen - API Design

## Maker Service API

### 1. Ingest Claim

- **Method**: `POST`
- **Path**: `/api/claims`
- **Requires**: Authentication not strictly required or any role, but prefer `Admin`/`System`.
- **Request Body**:

```json
{
	"insuranceCompany": "Acme Insurance",
	"patientName": "John Doe",
	"amount": 500.25
}
```

- **Responses**:
  - `201 Created` - Claim generated.

### 2. Get Pending Claims (Maker)

- **Method**: `GET`
- **Path**: `/api/maker/claims?page=1&pageSize=10`
- **Requires**: Role `Maker`
- **Responses**:
  - `200 OK` - Paginated list of pending claims.

### 3. Assign Claim For Maker

- **Method**: `POST`
- **Path**: `/api/maker/claims/{id}/assign`
- **Requires**: Role `Maker`
- **Responses**:
  - `200 OK` - Successfully claimed.
  - `409 Conflict` - Already claimed by someone else.

### 4. Submit Maker Recommendation

- **Method**: `POST`
- **Path**: `/api/maker/claims/{id}/recommend`
- **Requires**: Role `Maker`
- **Request Body**:

```json
{
	"recommendation": "Approve",
	"feedback": "All documents match."
}
```

- **Responses**:
  - `200 OK` - Recommendation saved.
  - `400 Bad Request` - Validation failures.

---

## Checker Service API

### 1. Get Claims Pending Checker Review

- **Method**: `GET`
- **Path**: `/api/checker/claims?page=1&pageSize=10`
- **Requires**: Role `Checker`
- **Responses**:
  - `200 OK` - Paginated list of claims with Maker recommendations.

### 2. Assign Claim For Checker

- **Method**: `POST`
- **Path**: `/api/checker/claims/{id}/assign`
- **Requires**: Role `Checker`
- **Responses**:
  - `200 OK` - Successfully claimed.
  - `409 Conflict` - Claimed by someone else.
  - `400 Bad Request` - Checker is the same as the Maker.

### 3. Submit Final Decision

- **Method**: `POST`
- **Path**: `/api/checker/claims/{id}/decide`
- **Requires**: Role `Checker`
- **Request Body**:

```json
{
	"decision": "Approve",
	"feedback": "Agreed with Maker."
}
```

- **Responses**:
  - `200 OK` - Decision saved, claim forwarded.
  - `400 Bad Request` - Validation failures.

---

## Shared / Query API (Can reside in either, let's put in Maker Service for simplicity or a new service. We will host it in MakerService prefixing with `/api/history`)

### 1. Claim History

- **Method**: `GET`
- **Path**: `/api/claims/history?status=Approved&insuranceCompany=Acme&page=1&pageSize=10`
- **Requires**: Any authenticated user
- **Responses**:
  - `200 OK` - Returns paginated list.
