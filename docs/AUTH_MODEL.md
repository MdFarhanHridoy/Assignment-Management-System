# Authentication & Authorization Model

> Authoritative contract for the server/ authentication layer (PHASE 0).
> Source of truth: `docs/PRD.md`. Stack: **ASP.NET Core 8 + C#**, **EF Core 8 + Npgsql**, **PostgreSQL**, **BCrypt.Net-Next**, **Microsoft.AspNetCore.Authentication.JwtBearer**.
> This document is self-contained: a developer can implement JWT auth, password hashing, login, `/me`, and role-based authorization from it alone.

---

## 1. Overview

The Assignment & Submission Management System uses **JWT Bearer authentication** with the **HS256** (HMAC-SHA256) symmetric signing algorithm. Authentication is **stateless**: the server holds no server-side session table; every authenticated request is validated purely from the signed token.

- **Scheme:** Bearer (`Authorization: Bearer <token>`).
- **Algorithm:** HS256 (symmetric — a single shared `Jwt__Secret` signs and verifies).
- **Transport:** Token is sent in the `Authorization` header of every protected request.
- **Role propagation:** The user role (`Admin` / `Teacher` / `Student`) is embedded in the token (AUTH-003) and is the basis for role-based authorization (AUTH-004).
- **Out of scope:** email verification, password reset, and refresh tokens (see Section 4 and PRD §17).

Roles are modeled by the `UserRole` enum (stored as string in DB): `UserRole { Admin, Teacher, Student }`. Each `Users` row has exactly one `Role` (USER-005).

---

## 2. Configuration

Authentication parameters are supplied via environment variables using the ASP.NET Core `__` (double-underscore) hierarchical key convention.

### 2.1 Environment keys

| Key | Value (canonical) | Purpose |
|---|---|---|
| `Jwt__Secret` | long, random, high-entropy string | HS256 symmetric signing key. **Must be ≥ 32 bytes** (256 bits) for HS256. Loaded from env, never from code. |
| `Jwt__Issuer` | `assignment-management-api` | Token `iss` claim; validated by `ValidIssuer`. |
| `Jwt__Audience` | `assignment-management-client` | Token `aud` claim; validated by `ValidAudience`. |
| `Jwt__ExpiryMinutes` | `120` | Token lifetime (`exp = iat + 120 min`). |

### 2.2 Validation parameters (server)

`Microsoft.AspNetCore.Authentication.JwtBearer` must be configured so that **all four** validations are enabled:

```
ValidateIssuer           = true      // iss must equal "assignment-management-api"
ValidateAudience         = true      // aud must equal "assignment-management-client"
ValidateLifetime         = true      // exp/nbf checked; expired tokens are rejected -> 401
ValidateIssuerSigningKey = true      // signature verified against Jwt__Secret
ValidIssuer              = "assignment-management-api"
ValidAudience            = "assignment-management-client"
IssuerSigningKey         = SymmetricSecurityKey(UTF8.GetBytes(Jwt__Secret))
ClockSkew                = 0 (or minimal) // do not silently accept far-expired tokens
```

If any validation fails, the middleware short-circuits the request with **401 Unauthorized** (AUTH-005).

### 2.3 `.env.example` snippet

```
# Backend
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=assignment_management;Username=postgres;Password=postgres
Jwt__Secret=change-this-long-development-secret-must-be-at-least-32-bytes
Jwt__Issuer=assignment-management-api
Jwt__Audience=assignment-management-client
Jwt__ExpiryMinutes=120

# Frontend
NEXT_PUBLIC_API_URL=http://localhost:5000
```

---

## 3. Token Structure

The JWT is signed with HS256 and carries the following claims. `JwtTokenService` issues them at login; `JwtBearer` validation consumes `sub`, `exp`, `iss`, `aud`, and the signature. Authorization consumes `role`.

| Claim | JWT std | Type | Meaning |
|---|---|---|---|
| `sub` | yes | string (UUID) | Subject = `Users.Id` (userId). Stable user identifier; used to load the user on `/me` and for ownership checks. |
| `email` | (private) | string | `Users.Email` (unique, lowercased). |
| `role` | (private) | string | `Users.Role` — one of `Admin`, `Teacher`, `Student`. **Included per AUTH-003.** Drives `[Authorize(Roles=...)]` and policies. |
| `name` | (private) | string | `Users.Name` (display name). |
| `jti` | yes | string (UUID) | Unique JWT ID. Guarantees token uniqueness; supports future revocation/audit (see Section 10). |
| `iat` | yes | numeric (Unix) | Issued-at timestamp (seconds). |
| `exp` | yes | numeric (Unix) | Expiry = `iat + Jwt__ExpiryMinutes (120)` (seconds). |
| `iss` | yes | string | `assignment-management-api`. |
| `aud` | yes | string | `assignment-management-client`. |

> Note on role as string: `UserRole` is an enum but is serialized as its string member name (`"Admin"`, `"Teacher"`, `"Student"`) both in the DB and in the `role` claim, matching `[Authorize(Roles="Admin")]` expectations.

---

## 4. Token Lifecycle

1. **Issuance (login):** On valid credentials, `JwtTokenService.Issue(...)` builds the claims above, sets `exp = iat + 120`, signs with HS256, and returns the compact serialized token inside `AuthResponse`.
2. **Lifetime:** `Jwt__ExpiryMinutes = 120` (2 hours). `ValidateLifetime = true` enforces it.
3. **No refresh tokens (decision):** This system intentionally does **not** implement refresh tokens (see PRD §17 — password reset / advanced token flows are out of scope). When a token expires, the client receives **401** and must re-authenticate via `POST /api/auth/login`.
4. **Client storage — recommended `httpOnly` cookie:** Store the JWT in an `httpOnly`, `Secure`, `SameSite=Strict` (or `Lax`) cookie set by the server/bff on login. This mitigates XSS-based token theft. An acceptable fallback for a pure SPA without a BFF is `localStorage`, but that is XSS-exposed and must be paired with a strict CSP; **`httpOnly` cookie is the recommended choice**. Whichever storage is used, the token is attached to outgoing requests as `Authorization: Bearer <token>`.
5. **Logout:** Stateless by design — logout is a client-side action (delete the token / clear the cookie). `jti` uniqueness enables future server-side revocation lists if added later.

---

## 5. Password Hashing

- **Library:** `BCrypt.Net-Next`.
- **Work factor:** `11` (BCrypt cost factor). Used for both hashing on user creation and verification on login.
- **Storage:** `Users.PasswordHash` holds the BCrypt hash (never the plaintext). Plaintext passwords are never persisted or logged.
- **Never serialized:** `PasswordHash` is **never** included in any API response (PRD rule 13). Every response uses DTOs that omit it (see Section 10 / DTO mapping).
- **Verify on login:** `UserService.VerifyPassword` runs `BCrypt.Net.BCrypt.Verify(plaintext, storedHash)`.
- **Failed authentication:** returns **401 Unauthorized** (AUTH-005) with a generic message (e.g., `"Invalid email or password."`). Do **not** reveal whether the email exists.
- **Logging:** Failed authentication attempts are logged (PRD §12) including, at most, the attempted `email` and client IP — **never** the password or hash. Example log: `"Failed login attempt for email=<x> from ip=<y>"`.
- **Hash creation:** When Admin creates a user (`POST /api/admin/users`), the supplied plaintext is hashed with work factor 11 before persistence.

---

## 6. Login Flow

Endpoint: `POST /api/auth/login` (public). Request body: `{ "email": string, "password": string }`. Response (200): `AuthResponse { token, expiresAt, user }` where `user` excludes `PasswordHash`.

```mermaid
sequenceDiagram
    participant C as Client
    participant AC as AuthController
    participant US as UserService
    participant TS as JwtTokenService
    participant DB as Users (PostgreSQL)

    C->>AC: POST /api/auth/login {email, password}
    AC->>US: VerifyPassword(email, password)
    US->>DB: Find user by Email (lowercased)
    DB-->>US: User row (incl. PasswordHash)
    alt user not found OR IsActive==false
        US-->>AC: false
        AC-->>C: 401 Unauthorized ("Invalid email or password.")
    else
        US->>US: BCrypt.Verify(password, user.PasswordHash)
        alt hash mismatch
            US-->>AC: false (log attempt; no password)
            AC-->>C: 401 Unauthorized
        else verified
            US-->>AC: true (user)
            AC->>TS: Issue(user)  // claims: sub,email,role,name,jti,iat,exp; HS256
            TS-->>AC: token + expiresAt (now+120m)
            AC-->>C: 200 AuthResponse { token, expiresAt, user (no PasswordHash) }
        end
    end
```

Key points:
- Email comparison is case-insensitive (store lowercased).
- `IsActive=false` users cannot log in (treated like failed auth → 401).
- `AuthResponse.user` is a DTO with **no** `PasswordHash` field (Section 5, rule 13).

---

## 7. Authorized Request Flow

All protected endpoints sit under `/api/admin/*`, `/api/teacher/*`, `/api/student/*` (and `GET /api/auth/me`). The flow validates the token, then enforces role policy, then runs the controller (which may do resource-level ownership checks).

```mermaid
sequenceDiagram
    participant C as Client
    participant MW as JwtBearer Middleware
    participant PZ as Authorization (Policy/Role)
    participant CTL as Controller
    participant SVC as Service (ownership check)
    participant DB as PostgreSQL

    C->>MW: Request with Authorization: Bearer <token>
    alt no token OR signature/exp/iss/aud invalid
        MW-->>C: 401 Unauthorized
    else token valid
        MW->>PZ: Authenticated principal (claims: sub, role, ...)
        alt role not allowed by [Authorize(Roles=...)] / policy
            PZ-->>C: 403 Forbidden
        else role allowed
            PZ->>CTL: invoke action
            CTL->>SVC: load resource + ownership check
            alt ownership fails (e.g., TeacherId != currentUser; StudentId != currentUser; not enrolled)
                SVC-->>CTL: Forbid/throw
                CTL-->>C: 403 Forbidden (or 404 to avoid leaking existence)
            else allowed
                SVC->>DB: read/write
                DB-->>SVC: result
                SVC-->>CTL: data (DTO, no PasswordHash)
                CTL-->>C: 200/201 result
            end
        end
    end
```

**401 vs 403 rules (AUTH-005):**

| Condition | Status | Reason |
|---|---|---|
| No token provided | **401** | Not authenticated. |
| Token expired | **401** | `ValidateLifetime` rejected it. |
| Invalid signature / wrong issuer / wrong audience | **401** | Token validation failed. |
| Authenticated but role not permitted on endpoint | **403** | Authenticated-but-not-allowed. |
| Authenticated, correct role, but **not the owner** of the resource (e.g., teacher editing another teacher's assignment; student reading another student's submission) | **403** | Not-owner. (404 also acceptable where resource existence must be hidden.) |

---

## 8. Role-Permission Matrix

`✓` = allowed; `✗` = denied. Cells carry scope notes from the canonical rules (teachers manage only their own assignments; review only submissions for their own assignments; students see only Published assignments for classes they're enrolled in; students view only their own submissions; admin sees all but does not create/grade).

| Capability | Admin | Teacher | Student |
|---|:---:|:---:|:---:|
| Manage users (create/list/update/disable/delete) | ✓ | ✗ | ✗ |
| Manage classes/courses & subjects (create/update/delete) | ✓ | ✗ | ✗ |
| Assign teachers to subjects/classes (`TeacherClassSubject`) | ✓ | ✗ | ✗ |
| Enroll students into classes/courses (`Enrollments`) | ✓ | ✗ | ✗ |
| View all assignments (incl. Draft) | ✓ | ✓ (only own assignments¹) | ✗ (Draft invisible²) |
| View all submissions | ✓ (all) | ✓ (only submissions for own assignments³) | ✗ |
| Create / update / delete assignments | ✗ (admin does not create/grade⁴) | ✓ (only own assignments¹) | ✗ |
| Publish / archive assignments | ✗ | ✓ (only own assignments¹) | ✗ |
| Review submissions (marks + feedback, status change) | ✗ (admin does not grade⁴) | ✓ (only submissions for own assignments³) | ✗ |
| View Published assignments for enrolled class | ✓ (all) | ✗ | ✓ (only enrolled classes²) |
| Submit / update own submission | ✗ | ✗ | ✓ (only own; before deadline⁵) |
| View own submissions + marks/feedback | ✓ (all) | ✓ (for own assignments) | ✓ (only own submissions⁶) |
| Login (`/api/auth/*`) + `/me` | ✓ | ✓ | ✓ |

**Notes:**
1. *Own assignments:* a Teacher may create/edit/delete/publish only `Assignments` where `TeacherId == current user Id`.
2. *Enrolled classes:* a Student sees only `Status == Published` `Assignments` whose `ClassId` matches a `Classes` the student is enrolled in via `Enrollments`; `Draft` assignments are never visible to students (ASGN-008).
3. *Own assignments' submissions:* a Teacher reviews only `Submissions` whose `Assignment.TeacherId == current user Id` (SUB-008, business rule 9).
4. *Admin visibility without mutation:* Admin may **view** all assignments/submissions (ADM-001/002, ADM-003) but does **not** create assignments or assign marks/feedback.
5. *Deadline:* a Student may submit/update only before the assignment deadline (UTC); updates are blocked after the deadline (SUB-003/004, rule 6/7).
6. *Own submissions:* a Student may view only `Submissions` where `StudentId == current user Id` (SUB-007, rule 8).

---

## 9. Authorization Implementation Guidance

### 9.1 Route-level role gates

Apply role-based authorization at the controller/route group level using the contract's role→routes mapping:

| Role | Allowed route prefixes |
|---|---|
| Admin | `/api/auth/*`, `/api/admin/*` |
| Teacher | `/api/auth/*`, `/api/teacher/*` |
| Student | `/api/auth/*`, `/api/student/*` |

Examples:

```csharp
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : ControllerBase { /* users, classes, subjects, teacher-assignments, enrollments, read-only assignments/submissions */ }

[ApiController]
[Authorize(Roles = "Teacher")]
[Route("api/teacher")]
public class TeacherController : ControllerBase { /* assignments CRUD/publish, submission review */ }

[ApiController]
[Authorize(Roles = "Student")]
[Route("api/student")]
public class StudentController : ControllerBase { /* view published assignments, submit/update, view own submissions */ }

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase { /* POST /login (anonymous), GET /me ([Authorize]) */ }
```

### 9.2 Policies (optional)

Define ASP.NET Core authorization policies for clarity and reuse:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",   p => p.RequireRole("Admin"));
    options.AddPolicy("TeacherOnly", p => p.RequireRole("Teacher"));
    options.AddPolicy("StudentOnly", p => p.RequireRole("Student"));
});
```

Use via `[Authorize(Policy = "TeacherOnly")]`. Policies can later be extended with requirements (e.g., resource ownership handlers) without changing controllers.

### 9.3 Resource-level (ownership) checks — in services

Route-level role gates are **not enough**. Services must enforce ownership because multiple Teachers/Students share the same role:

- **Teacher editing an assignment:** load `Assignment`; if `Assignment.TeacherId != currentUser.Id` → **403** (or 404). Applies to update, delete, publish, and viewing/reviewing its submissions.
- **Teacher reviewing a submission:** load `Submission → Assignment`; if `Assignment.TeacherId != currentUser.Id` → **403**. On review, set `Submission.ReviewedByTeacherId = currentUser.Id` and `ReviewedAtUtc`.
- **Student viewing an assignment:** only `Status == Published` AND enrolled (`Enrollments` contains `(ClassId, StudentId==currentUser.Id)`); else **404** (hide existence) — never expose Draft.
- **Student submit/update:** verify assignment Published + enrolled + before deadline (UTC); the created/updated `Submission.StudentId` must equal `currentUser.Id`.
- **Student viewing submissions:** filter `Submissions` by `StudentId == currentUser.Id`; requesting another's submission → **404/403**.

Obtain the current user id from claims: `User.FindFirstValue(JwtRegisteredClaimNames.Sub)` (or `ClaimTypes.NameIdentifier`), and role from the `role` claim.

### 9.4 401 vs 403 summary

- **401** = not authenticated (missing/invalid/expired token) — emitted by `JwtBearer`.
- **403** = authenticated but not allowed (wrong role at the policy layer **or** failed ownership check in a service).
- Prefer **404** over 403 where revealing resource existence would itself leak information (e.g., a Draft assignment to a Student).

---

## 10. Security Rules

1. **Never log passwords or password hashes.** Log at most the `email` and client IP on failed login (Section 5).
2. **Never log full JWT tokens.** If a token must be referenced in logs, use its `jti` only.
3. **`PasswordHash` is excluded from all responses.** All controllers return DTOs (request/response models) that map `Users` → a safe shape omitting `PasswordHash`. Do not serialize the entity directly.
4. **HTTPS in production.** Serve the API over TLS in any non-Development environment; set `ASPNETCORE_HTTPS_PORT`/HSTS accordingly.
5. **Secret from environment, not code.** `Jwt__Secret` is injected via env/`appsettings`/secret store; never committed to source. A separate long random value per environment.
6. **`jti` uniqueness.** Every issued token gets a fresh `jti` (GUID). This guarantees no two tokens collide and enables future server-side revocation/audit lists.
7. **Symmetric key length.** `Jwt__Secret` must be ≥ 32 bytes (256 bits) so HS256 is cryptographically sound.
8. **No password reset / no refresh tokens.** Stale/expired tokens simply yield 401; the client re-logs in (Section 4).
9. **`IsActive` gating.** Disabled users (`IsActive=false`) cannot authenticate (401), even if their password hash is valid.
10. **Generic failure messages.** Failed auth returns a non-revealing `"Invalid email or password."` (no account-existence leakage).

---

## 11. Demo Credentials

Seed data must provision these working accounts (PRD §14.5, AUTH-007). Passwords are hashed with BCrypt (work factor 11) at seed time; the plaintext below is for login only and must not be stored.

| Role | Email | Password | Notes |
|---|---|---|---|
| Admin | `admin@example.com` | `admin@123` | Full admin visibility; no assignment/grading mutation. |
| Teacher | `teacher@example.com` | `teacher@123` | Owns sample assignments/submissions to review. |
| Teacher | `teacher2@example.com` | `teacher@123` | Second teacher for ownership-boundary testing (cannot access teacher1's resources). |
| Student | `student@example.com` | `student@123` | Enrolled in at least one class with Published assignments. |

> Use demo credentials only. Do not use real personal passwords.
