# Reviewer Portal — Specification

## Tech Stack

| Layer    | Technologies                                                              |
|----------|---------------------------------------------------------------------------|
| Backend  | .NET 8 Web API, EF Core (InMemory), HttpClient (IHttpClientFactory), Swagger/OpenAPI, CORS, xUnit |
| Frontend | Vue 3, Composition API, Axios                                             |
| Infra    | Docker, Docker Compose                                                    |

---

## Backend — Web API

### POST /api/users/register — RegisterUser

**Request:** `RegisterUserRequestDto`

**Business Logic:**

1. Validate input (see [Validation Rules](#validation-rules)).
2. Get university score via `UniversityService`:
   - Check DB for `UniversityName`. If found, return the `University` object.
   - Otherwise, call the external API:
     - `GET <baseUrl>/v1/organizations/elasticSuggestions?query={UniversityName}&maxcount=1`
   - If the external API call fails, return **HTTP 400** + error message.
   - From the response, extract `organizationName` and `score` from the first element.
   - Check DB for `organizationName`. If found, return that `University` (using `organizationName` as `UniversityName`).
3. Save user and university. When creating a new University record, store organizationName (from the external API response) as UniversityName, not the user-supplied value.
   - Success: **HTTP 201** + `RegisterUserResponseDto`
   - Failure: **HTTP 400** + error message

---

### POST /api/users/{userId}/invite — InviteReviewer

**Request:** `userId` (route parameter)

**Business Logic:**

1. If user does not exist, return **HTTP 404** + error message.
2. If `user.NumberOfPublications > 3` AND user's `university.UniversityScore >= 60`, return:
   ```json
   { "success": true, "message": "Invitation was successful" }
   ```
3. Otherwise, return:
   ```json
   { "success": false, "message": "Invitation could not be sent" }
   ```
   Both cases return **HTTP 200**.

---

## Data Contracts

### DTOs

#### RegisterUserRequestDto
| Field                | Type   |
|----------------------|--------|
| UserName             | string |
| UniversityName       | string |
| NumberOfPublications | int    |

#### RegisterUserResponseDto
| Field                | Type         |
|----------------------|--------------|
| UserId               | int          |
| UserName             | string       |
| NumberOfPublications | int          |
| University           | UniversityDto |

#### UniversityDto
| Field          | Type    |
|----------------|---------|
| Id             | int     |
| UniversityName | string  |
| Score          | decimal |

---

### Domain Models

#### User
| Field                | Type   |
|----------------------|--------|
| UserId               | int    |
| UserName             | string |
| UniversityId         | int    |
| NumberOfPublications | int    |

#### University
| Field          | Type    |
|----------------|---------|
| UniversityId   | int     |
| UniversityName | string  |
| Score          | decimal     |

---

### External API Model

#### UniversitySuggestion
| Field            | Type    |
|------------------|---------|
| OrganizationName | string  |
| Score            | decimal |

**Sample external API response:**
```json
[
  {
    "id": 123,
    "organizationName": "University of Oxford",
    "matchedName": null,
    "country": "United Kingdom",
    "countryIsoCode": "GBR",
    "city": "Oxford",
    "street": "Wellington Square",
    "zipCode": "OX1 2JD",
    "state": "England",
    "webDomain": "https://www.ox.ac.uk/",
    "type": null,
    "score": 110.01135
  }
]
```

---

## Validation Rules

Applied to `RegisterUserRequestDto`:

| Field                | Rule                              |
|----------------------|-----------------------------------|
| All fields           | Required (non-null, non-empty)    |
| UserName             | Unique; min length = 3            |
| UniversityName       | Min length = 3                    |
| NumberOfPublications | >= 0                              |

---

## Architecture

- **Layered Architecture:** Controllers → Services → Infrastructure
- No business logic in controllers
- SOLID principles throughout
- Dependency Injection for all services and repositories
- `async`/`await` pattern for all I/O operations
- `IHttpClientFactory` for external HTTP calls
- External API base URL configured via:
  - `appsettings.json` key: `"UniversityApiBaseUrl"`
  - `.env` file for Docker environments
- Service layer must be independently unit-testable

---

## Unit Tests (xUnit)

- Test the service layer
- Mock the external API call
- Follow Arrange-Act-Assert principle
- Cover:
  - Happy path (successful registration, successful invitation)
  - Rejection paths (validation failure, API failure, user not eligible)
  - Verify all code flows

---

## Frontend — `ReviewerPortalWeb/` directory

### Register User Component

- Form fields: `UserName`, `UniversityName`, `NumberOfPublications`
- Client-side validation with friendly messages
- Loading indicator during API call
- Display success or error message after submission

### Invite Reviewer Component

- Input field for `UserId`
- Submit invitation request
- Display invitation result

### Service Layer (Axios)

- Centralized API client with base URL configuration
- Request abstraction per endpoint
- Consistent error handling

### UI/UX Requirements

- Responsive layout
- Simple, clean design
- Loading indicators during API calls
- User-friendly validation and error messages
- No inline-styling

---

## Docker

### Dockerfiles

- `Dockerfile` for the .NET API
- `Dockerfile` for the Vue application

### Docker Compose

- Runs frontend and backend together
- Exposes ports for both services
- Supports environment variable injection
