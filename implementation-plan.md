# Implementation Plan — Reviewer Portal

## Overview

The application is a full-stack reviewer portal with a .NET 8 Web API backend and a Vue 3 frontend,
implemented in a clean layered architecture (Controllers → Services → Infrastructure).
`AppDbContext` is injected directly into services — no repository abstraction layer.

Implementation proceeds layer by layer, bottom-up: domain first, then infrastructure, then services,
then controllers, then tests, then frontend, then Docker.

---

## Project Structure

```
ReviewerPortal/
├── ReviewerPortal.sln
├── ReviewerPortal.API/
│   ├── Controllers/
│   │   ├── Models/
│   │   │   └── RegisterUserRequest.cs
│   │   └── UsersController.cs
│   ├── Domain/
│   │   └── Entities/
│   │       ├── User.cs
│   │       └── University.cs
│   ├── Application/
│   │   ├── Constants/
│   │   │   └── EligibilityRules.cs
│   │   ├── DTOs/
│   │   │   ├── RegisterUserResponseDto.cs
│   │   │   └── UniversityDto.cs
│   │   ├── Exceptions/
│   │   │   ├── BadRequestException.cs
│   │   │   └── NotFoundException.cs
│   │   ├── Interfaces/
│   │   │   ├── IUniversityApiClient.cs
│   │   │   ├── IUniversityService.cs
│   │   │   └── IUserService.cs
│   │   ├── Models/
│   │   │   ├── InvitationResult.cs
│   │   │   └── UniversityApiResult.cs
│   │   └── Services/
│   │       ├── UniversityService.cs
│   │       └── UserService.cs
│   ├── Infrastructure/
│   │   ├── ExternalApi/
│   │   │   ├── Models/
│   │   │   │   └── UniversitySuggestion.cs
│   │   │   └── UniversityApiClient.cs
│   │   └── Persistence/
│   │       └── AppDbContext.cs
│   ├── Middleware/
│   │   └── GlobalExceptionHandler.cs
│   ├── appsettings.json
│   ├── Program.cs
│   └── Dockerfile
├── ReviewerPortal.Tests/
│   ├── UserServiceTests.cs
│   └── UniversityServiceTests.cs
├── ReviewerPortalWeb/
│   ├── src/
│   │   ├── components/
│   │   │   ├── RegisterUser.vue
│   │   │   └── InviteReviewer.vue
│   │   ├── services/
│   │   │   └── api.js
│   │   ├── App.vue
│   │   └── main.js
│   ├── .env
│   └── Dockerfile
├── docker-compose.yml
└── .env
```

---

## Phase 1 — Solution & Project Setup

**Goal:** Runnable skeleton with no logic yet.

1. Create solution, API project (`webapi`), and test project (`xunit`). Add both to the solution with a project reference from Tests → API.
2. Install NuGet packages:
   - API: `Microsoft.EntityFrameworkCore.InMemory`, `Swashbuckle.AspNetCore`
   - Tests: `Moq`, `FluentAssertions`, `Microsoft.EntityFrameworkCore.InMemory`
3. Configure `Program.cs`: EF Core InMemory, Swagger, CORS (`AllowAnyOrigin`/`AllowAnyMethod`/`AllowAnyHeader`).
4. Add `"UniversityApiBaseUrl"` to `appsettings.json`.

**Deliverable:** `dotnet build` and `dotnet run` succeed; Swagger UI is reachable.

---

## Phase 2 — Domain Layer

**Goal:** Define the core entities with no dependencies.

- `User`: `UserId` (PK), `UserName`, `UniversityId` (FK), `NumberOfPublications`, `University` (navigation property).
- `University`: `UniversityId` (PK), `UniversityName`, `Score` (`decimal`).

No logic in entities.

**Deliverable:** Both entity classes compile.

---

## Phase 3 — Infrastructure Layer

**Goal:** Database context and external API client.

### 3a — AppDbContext

`DbSet<User> Users` and `DbSet<University> Universities`. Configure in `OnModelCreating`:
- Unique index on `University.UniversityName`.
- Unique index on `User.UserName`.
- Foreign key `User.UniversityId` → `University.UniversityId`.

### 3b — IUniversityApiClient (`Application/Interfaces/`)

Interface lives in the Application layer so services depend on the abstraction, not on HTTP infrastructure:

```
Task<UniversityApiResult?> FindAsync(string query)
```

### 3c — UniversityApiResult (`Application/Models/`)

Record type: `(string Name, decimal Score)`.

### 3d — UniversityApiClient (`Infrastructure/ExternalApi/`)

Implements `IUniversityApiClient`. Depends on `IHttpClientFactory` (named client `"UniversityApi"`).

Algorithm:
1. GET `/v1/organizations/elasticSuggestions?query={query}&maxcount=1` using the named client.
2. If `HttpRequestException` is thrown, rethrow as `BadRequestException`.
3. If response status is not success, throw `BadRequestException` with the status code.
4. Deserialize to `List<UniversitySuggestion>`. Return `null` if list is empty.
5. Return `new UniversityApiResult(first.OrganizationName, first.Score)`.

Dispose `HttpResponseMessage` with a `using` block.

### 3e — Registration in `Program.cs`

Read `UniversityApiBaseUrl` from configuration at startup — throw `InvalidOperationException` immediately if missing (fail-fast). Register named `HttpClient` and `IUniversityApiClient`.

**Deliverable:** `AppDbContext` and `UniversityApiClient` compile and are registered.

---

## Phase 4 — Application Layer (Services & DTOs)

**Goal:** All business logic implemented and testable in isolation.

### 4a — Custom Exceptions (`Application/Exceptions/`)

`BadRequestException` and `NotFoundException`, both extending `Exception`.

### 4b — DTOs (`Application/DTOs/`)

- `RegisterUserResponseDto`: `UserId`, `UserName`, `NumberOfPublications`, `University` (type `UniversityDto`).
- `UniversityDto`: `UniversityId`, `UniversityName`, `Score`.

### 4c — InvitationResult (`Application/Models/`)

Record type: `(bool Success, string Message)`. Replaces a tuple so the service contract uses a named type.

### 4d — EligibilityRules (`Application/Constants/`)

`internal static class` with four constants: `MinimumPublications = 3`, `MinimumUniversityScore = 60m`, `InvitationSuccessMessage`, `InvitationFailureMessage`. Marked `internal` — tests assert against literal strings, not these constants.

### 4e — UniversityService (`Application/Services/`)

Implements `IUniversityService`. Depends on `AppDbContext` and `IUniversityApiClient`.

Algorithm for `GetOrCreateUniversityAsync`:
1. Query by `universityName` → return if found.
2. Call `universityApiClient.FindAsync`. If `null`, throw `BadRequestException`.
3. Query by `apiResult.Name` → return if found (avoids duplicate on canonical name).
4. Insert new `University`. On `DbUpdateException` (concurrent insert race): detach entity, re-query and return the winner's record.

### 4f — UserService (`Application/Services/`)

Implements `IUserService`. Depends on `AppDbContext` and `IUniversityService`.

`RegisterUserAsync`: check for duplicate username → get/create university → insert user → return mapped DTO.

`InviteReviewerAsync`: load user with `.Include(u => u.University)` → throw `NotFoundException` if absent → evaluate `NumberOfPublications > MinimumPublications && Score >= MinimumUniversityScore` → return `InvitationResult`.

**Deliverable:** Service classes compile with no controller dependencies.

---

## Phase 5 — API Layer (Controllers)

**Goal:** Expose the two endpoints; no business logic in the controller.

### RegisterUserRequest (`Controllers/Models/`)

DataAnnotations in the presentation layer, not on Application DTOs. `NumberOfPublications` is `int?` so `[Required]` correctly rejects a missing field.

### UsersController

- `POST /api/users` — calls `RegisterUserAsync`, returns 201.
- `POST /api/users/{userId:int}/invitations` — calls `InviteReviewerAsync`, returns 200.

### GlobalExceptionHandler (`Middleware/`)

Implements `IExceptionHandler`. Injected with `ILogger<GlobalExceptionHandler>`.
- `BadRequestException` → 400
- `NotFoundException` → 404
- All other exceptions → 500 + log via `logger.LogError`

Response body: `{ "error": "<message>" }` for all error cases.

**Deliverable:** Both endpoints are reachable via Swagger and return expected shapes.

---

## Phase 6 — Unit Tests

**Goal:** Service layer covered; external dependencies mocked.

Each test class creates an isolated `AppDbContext` with a unique in-memory database name (`Guid.NewGuid().ToString()`). All tests follow Arrange-Act-Assert with explicit `// Arrange`, `// Act`, `// Assert` comments.

### `UniversityServiceTests.cs`

Mocks `IUniversityApiClient`.

| Test | Scenario |
|------|----------|
| Returns cached university when `UniversityName` found in DB | Happy path — first DB hit |
| Calls API when not cached, creates new university | Happy path — API hit, new record |
| Returns cached university when `organizationName` already in DB | Happy path — second DB check |
| Throws `BadRequestException` when API returns empty result | Rejection — empty response |
| Throws `BadRequestException` when API client throws | Rejection — network failure |
| Throws `BadRequestException` when API returns non-success status | Rejection — bad status |
| Never inserts when first cache hits | Interaction — short-circuit on input name |
| Never inserts when second cache hits | Interaction — short-circuit on API name |
| Persists university with `OrganizationName` and `Score` from API result | Interaction — correct fields |

### `UserServiceTests.cs`

Mocks `IUniversityService`.

| Test | Scenario |
|------|----------|
| Returns `RegisterUserResponseDto` with all fields correctly mapped | Happy path |
| Throws `BadRequestException` when `UserName` already exists | Rejection |
| Propagates `BadRequestException` from university service | Rejection |
| Returns `success: true` when publications = 4 and score = 60 | Invitation — minimum passing values |
| Returns `success: true` when both exceed thresholds | Invitation — above threshold |
| Returns `success: false` when publications = 3 (at threshold, not above) | Invitation boundary |
| Returns `success: false` when score = 59 | Invitation boundary |
| Throws `NotFoundException` when `userId` does not exist | Rejection |
| Never calls university service when username already exists | Interaction — short-circuit |
| Calls `GetOrCreateUniversityAsync` with exact `universityName` | Interaction — correct arg |
| Persists user with `UniversityId` matching university returned by service | Interaction — FK linkage |

**Deliverable:** All tests pass with `dotnet test`.

---

## Phase 7 — Frontend (`ReviewerPortalWeb/`)

**Goal:** Two functional components wired to the backend.

- Vue 3 + Composition API. Install Axios.
- `.env`: `VITE_API_BASE_URL=https://localhost:7221`.
- `api.js`: Axios instance with `baseURL` and `timeout: 10000`. Functions: `registerUser` → `POST /api/users`; `inviteReviewer` → `POST /api/users/{userId}/invitations`. Export `extractErrorMessage` helper.
- `RegisterUser.vue`: form with `userName`, `universityName`, `numberOfPublications`; client-side validation; loading state; success/error display.
- `InviteReviewer.vue`: single `userId` input; client-side validation; loading state; display `message` from response or extracted error.
- `App.vue`: renders both components. All shared styles (`.field`, `.error`, `.success`, button, inputs) defined globally here — individual components have no `<style>` blocks.

**Deliverable:** Both components work end-to-end against the running backend.

---

## Phase 8 — Docker

**Goal:** `docker-compose up` brings up both services.

- API `Dockerfile`: SDK 8.0 build stage → ASP.NET 8.0 runtime.
- Frontend `Dockerfile`: Node 20 Alpine build with `ARG VITE_API_BASE_URL` baked at build time → nginx:alpine.
- `docker-compose.yml`: API on `5050:8080`; frontend on `5173:80` built with `VITE_API_BASE_URL: http://localhost:5050`; frontend `depends_on` api; API reads `UniversityApiBaseUrl` from environment.
- Root `.env`: `UNIVERSITY_API_BASE_URL=<university_api_base_url>

**Deliverable:** `docker-compose up --build` starts both containers; frontend reaches backend.

---

## Open Questions (Spec Ambiguities)

| # | Area | Ambiguity | Resolution |
|---|------|-----------|------------|
| 1 | `University.Score` type | Spec domain model says `int`, external API returns a float. | `decimal` used throughout. |
| 2 | New university name | Store user-input name or `organizationName` from API? | `organizationName` (canonical API value) — future lookups hit the DB cache. |
| 3 | External API — empty response | Undefined behavior when array is empty. | Throw `BadRequestException` → HTTP 400. |
| 4 | User navigation to score | `User` entity has no `UniversityScore` field. | EF Core navigation property `User.University` loaded via `.Include()`. |
| 5 | Concurrent university inserts | Two simultaneous registrations can both miss the DB check. | Unique index on `University.UniversityName` + catch `DbUpdateException`, detach and re-query. |
| 6 | CORS allowed origins | Spec says "CORS Enabled" without specifying origins. | `AllowAnyOrigin` for development. |
| 7 | Frontend API base URL in Docker | Vue bakes `VITE_API_BASE_URL` at build time. | Build-time `ARG` passed via `docker-compose.yml` `args`. |
| 8 | Backend HTTP port in Docker | .NET 8 default internal port is `8080`. | Mapped as `5050:8080`. |
| 9 | Transaction scope | If university saves but user save fails, university record is orphaned. | Single `AppDbContext` per request via DI. Spec does not require rollback of university on user failure. |
| 10 | HTTP response body for errors | Spec says "error message" without defining the shape. | `{ "error": "<message>" }` — consistent across 400, 404, and 500. |
| 11 | Eligibility — publications threshold | Spec: `NumberOfPublications > 3`. | Strictly greater than 3; exactly 3 is not eligible. |
