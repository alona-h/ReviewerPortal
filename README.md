# Reviewer Portal

## Project Overview

Reviewer Portal is a full-stack application for registering academic users and inviting users as peer reviewers. The backend is a .NET 8 Web API that resolves university data from an external API and applies eligibility criteria based on publication count and university score. The frontend is a Vue 3 single-page app that provides forms for both operations.

---

## Prerequisites

**Docker (recommended)**
- Docker Desktop

**Local development**
- .NET 8 SDK
- Node.js 20+ and npm

---

## Running with Docker

1. Set the external university API URL in [`.env`](.env):
   ```
   UNIVERSITY_API_BASE_URL=<university_api_base_url>
   ```

2. Build and start both services:
   ```bash
   docker-compose up --build
   ```

| Service  | URL                           |
|----------|-------------------------------|
| API      | http://localhost:5050         |
| Swagger  | http://localhost:5050/swagger |
| Frontend | http://localhost:5173         |

> The frontend is built with `VITE_API_BASE_URL=http://localhost:5050` baked in at image build time.

---

## Running Locally

### Backend

1. Set the university API URL in [`ReviewerPortal.API/appsettings.json`](ReviewerPortal.API/appsettings.json):
   ```json
   {
     "UniversityApiBaseUrl": "https://your-university-api.example.com"
   }
   ```

2. Run the API:
   ```bash
   dotnet run --project ReviewerPortal.API --launch-profile https
   ```

| Profile | URL                            |
|---------|--------------------------------|
| HTTPS   | https://localhost:7221         |
| HTTP    | http://localhost:5071          |
| Swagger | https://localhost:7221/swagger |

### Frontend

1. Install dependencies:
   ```bash
   cd ReviewerPortalWeb
   npm install
   ```

2. Start the dev server:
   ```bash
   npm run dev
   ```

The dev server starts at **http://localhost:5173** and proxies API calls to `https://localhost:7221` (configured in [`ReviewerPortalWeb/.env`](ReviewerPortalWeb/.env)).

### Tests

```bash
dotnet test
```

---

## API Endpoints

### `POST /api/users/register`

Registers a new user. Resolves the university name against the external API and stores the result.

**Request body**
```json
{
  "userName": "alice",
  "universityName": "Oxford",
  "numberOfPublications": 5
}
```

**201 Created**
```json
{
  "userId": 1,
  "userName": "alice",
  "numberOfPublications": 5,
  "university": {
    "id": 1,
    "universityName": "University of Oxford",
    "score": 110.01
  }
}
```

**400 Bad Request** — username already taken or university lookup failed
```json
{
  "error": "Username 'alice' is already taken."
}
```

---

### `POST /api/users/{userId}/invite`

Evaluates whether a registered user meets reviewer eligibility criteria:
- `numberOfPublications > 3`
- `university.score >= 60`

**200 OK** — eligible
```json
{
  "success": true,
  "message": "Invitation was successful"
}
```

**200 OK** — not eligible
```json
{
  "success": false,
  "message": "Invitation could not be sent"
}
```

**404 Not Found** — user does not exist
```json
{
  "error": "User with ID 99 was not found."
}
```

---

## Architecture

The backend follows Clean Layered Architecture — Controllers delegate to Services, which coordinate directly with the database and an external HTTP client.

- **Controllers** (`ReviewerPortal.API/Controllers/`) — HTTP routing and model binding; no business logic.
- **Services** (`Application/Services/`) — `UserService` handles registration and invitation eligibility; `UniversityService` resolves university names first from a local database cache, then from the external API. Both services query `AppDbContext` directly, backed by an in-memory database.
- **External API** — `UniversityService` calls `GET /v1/organizations/elasticSuggestions` via `IUniversityApiClient` (a named `HttpClient` registered as `"UniversityApi"`) to resolve university names and scores.
- **Frontend** (`ReviewerPortalWeb/`) — Vue 3 Composition API; all API calls are centralised in `src/services/api.js` using an Axios instance.

Cross-cutting: a global `IExceptionHandler` maps `BadRequestException` → 400 and `NotFoundException` → 404.
