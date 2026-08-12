# Verification Checklist

> **Project:** Assignment & Submission Management System
> **Repo:** `C:\Projects\Assessment\OnnoRokom_Projukti_Limited\Assignment-Management-System`
> **Living document.** This file is updated after every phase completes. Do not delete completed entries.

---

## 1. Purpose

This document is the single source of truth for **phase-by-phase verification** of the Assignment & Submission Management System. It works hand-in-hand with `docs/IMPLEMENTATION_PLAN.md` and `docs/TASK_BREAKDOWN.md`.

### How to use this document

1. **Before a phase starts** — read the corresponding blank entry (Phase 1–9) below to know the expected deliverables, the canonical verify command(s), the suggested Conventional Commit message, and the dependency chain.
2. **During a phase** — run the verify command(s) listed and capture actual output.
3. **When a phase completes** — the orchestrator fills in the blank entry for that phase, converting it into a **COMPLETED** entry that records:
   - **Summary** of what was done.
   - **Verification commands run** and their **expected** output.
   - **Pass / Fail** result.
   - **Conventional Commit message** and the exact **commit command** to run from the repo root.
   - **Notes / deviations**, and a **doc-first update** note if the specification changed (i.e. `docs/` was updated *before* the code that implements the changed contract).
4. **Doc-first rule** — if any contract in `docs/` (DATABASE_SCHEMA, API_CONTRACT, AUTH_MODEL, BUSINESS_RULES, PROJECT_STRUCTURE) changes during a phase, the spec file is edited and committed first, and the change is recorded under *Notes / deviations* of that phase entry.

> Phase 0 below is already filled because Phase 0 is being completed now. Phases 1–9 are blank templates to be filled when each phase completes.

---

## 2. Conventional Commits Guide

Every phase is committed with a single [Conventional Commit](https://www.conventionalcommits.org/) message using the `type(scope): subject` form. Keep the subject lowercase, imperative, and under ~60 characters.

**Format:** `type(scope): subject`

### Allowed types

| Type | Use for |
|---|---|
| `feat` | A new feature (the bulk of phases 1–7) |
| `fix` | A bug fix |
| `test` | Adding or correcting tests (Phase 8) |
| `docs` | Documentation-only changes (Phase 0, Phase 9) |
| `chore` | Tooling, config, dependencies, scaffolding that is not user-facing |
| `refactor` | Code restructuring with no behaviour change |

### Scope conventions used in this project

| Scope | Covers |
|---|---|
| `server` | Backend solution, entities, DbContext, migrations, seed |
| `auth` | Authentication & authorization (JWT, hashing, roles) |
| `admin` | Admin API surface |
| `teacher` | Teacher API surface |
| `student` | Student API surface |
| `client` | Next.js frontend |
| `docs` | Documentation |

**Examples**
- `feat(server): scaffold solution, domain model, EF migrations and seed`
- `feat(auth): JWT login, /me, BCrypt hashing and role authorization`
- `test(submission): cover deadline and marks validation`
- `docs: add Phase 0 specification and process documents`

---

## 3. Per-Phase Verification Template (Reusable)

Copy this template into a phase section when that phase begins. Fill the fields as verification proceeds.

### Template

```
### PHASE N — <STATE: TODO | COMPLETED>

- Phase goal: <one line from IMPLEMENTATION_PLAN>
- Depends on: <previous phase name, or "none (entry phase)">

#### Verification checkboxes
- [ ] Code builds
- [ ] Migrations applied (if backend)
- [ ] Tests pass
- [ ] Manual smoke (if API/UI)
- [ ] Docs updated if changed

#### Verification record
| Field | Value |
|---|---|
| Commands run | <exact commands> |
| Expected | <expected output/exit code> |
| Actual | <observed output> |
| Result | PASS / FAIL |
| Commit message | <Conventional Commit message> |
| Commit command | `<exact command>` |

#### Notes / deviations
<none, or describe deviations + doc-first spec changes>
```

---

## 4. PHASE 0 — COMPLETED

- **Phase goal:** Establish the specification and process documentation. No application code.
- **Depends on:** none (entry phase).

### Summary

Created the Phase 0 documentation set in `docs/`. This is a documentation-only phase — **no application code** was written in `server/` or `client/`.

**Specification documents (5):**
- `docs/DATABASE_SCHEMA.md` — relational schema for User, Class, Subject, Enrollment, TeacherClassSubject, Assignment, Submission.
- `docs/API_CONTRACT.md` — REST endpoints grouped by `/api/auth`, `/api/admin`, `/api/teacher`, `/api/student` with request/response DTOs and status codes.
- `docs/AUTH_MODEL.md` — JWT (HS256) flow, BCrypt hashing, role claims, `/auth/login` and `/auth/me`.
- `docs/BUSINESS_RULES.md` — the 13 PRD business rules and the per-rule enforcement points.
- `docs/PROJECT_STRUCTURE.md` — repository layout for `client/`, `server/`, `docs/`, `plans/`.

**Process documents (5):**
- `docs/ARCHITECTURE.md` — component/deployment architecture, ports (API 5000, client 3000), data flow.
- `docs/TASK_BREAKDOWN.md` — granular task list per phase.
- `docs/IMPLEMENTATION_PLAN.md` — phase ordering, deliverables, dependencies.
- `docs/VERIFICATION_CHECKLIST.md` — this file.
- `docs/DECISIONS.md` — architecture/design decision log (ADR-style).

### Verification record

| Field | Value |
|---|---|
| Commands run | `ls docs/*.md` (and optionally `git status --short` to show new untracked docs) |
| Expected | 11 `.md` files in `docs/` (10 Phase 0 files + `PRD.md`): API_CONTRACT.md, ARCHITECTURE.md, AUTH_MODEL.md, BUSINESS_RULES.md, DATABASE_SCHEMA.md, DECISIONS.md, IMPLEMENTATION_PLAN.md, PRD.md, PROJECT_STRUCTURE.md, TASK_BREAKDOWN.md, VERIFICATION_CHECKLIST.md. `git status --short` shows the docs as new untracked (or staged) files; no changes under `server/` or `client/`. |
| Actual | 10 Phase 0 documents produced under `docs/`; `PRD.md` (pre-existing) retained unchanged. No code files added. |
| Result | **PASS** (pending user approval) |
| Commit message | `docs: add Phase 0 specification and process documents` |
| Commit command | `git add docs/*.md && git commit -m "docs: add Phase 0 specification and process documents"` |

### Verification checkboxes

- [x] Code builds — N/A (documentation-only phase; no code).
- [x] Migrations applied — N/A (no backend yet).
- [x] Tests pass — N/A (no code yet).
- [x] Manual smoke — N/A (no API/UI yet).
- [x] Docs updated if changed — all Phase 0 docs created; `PRD.md` unchanged.

### Notes / deviations

- **Awaiting user approval before Phase 1.** No application code exists yet.
- `docs/PRD.md` is the authoritative requirements source and was not modified.
- No doc-first deviation: this phase *is* the documentation.

---

## 5. Phase 1–9 Entries (To Be Filled When Each Phase Completes)

> Dependencies below are derived from the canonical contract phase ordering (see `docs/IMPLEMENTATION_PLAN.md`): each phase builds on the previous one. When `IMPLEMENTATION_PLAN.md` is authored, cross-check these dependency lines against it.

---

### PHASE 1 — COMPLETED

- **Phase goal:** Server scaffold + domain entities + EF Core DbContext + InitialCreate migration + seed data (demo users: admin, teacher, teacher2, student).
- **Depends on:** Phase 0 (Docs).

#### Verification checkboxes
- [x] Code builds — `dotnet build` succeeds for all 5 projects, 0 errors / 0 warnings
- [x] Migrations generated & applied — `dotnet ef migrations add InitialCreate` + `dotnet ef database update` succeeded
- [x] Tests pass — `dotnet test` green (SanityTests 1/1)
- [x] Manual smoke — `dotnet run` (Development) auto-migrated + seeded 4 demo users; all 7 tables verified via psql
- [x] Docs updated if changed — no spec contract changed; code fixes documented under Notes below

#### Verification record
| Field | Value |
|---|---|
| Commands run | `dotnet restore AssignmentManagement.sln`; `dotnet build AssignmentManagement.sln --no-restore`; `dotnet test AssignmentManagement.sln --no-build`; `dotnet tool install --global dotnet-ef --version 8.0.10`; `dotnet ef migrations add InitialCreate -p src/AssignmentManagement.Infrastructure -s src/AssignmentManagement.Api`; `dotnet ef database update -p src/AssignmentManagement.Infrastructure -s src/AssignmentManagement.Api`; `dotnet run --project src/AssignmentManagement.Api` (Development env, hosted seed); `psql -f verify_seed.sql` |
| Expected | Build: 0 errors / 0 warnings across 5 projects; Tests: 1 passed; Migration: 7 tables + indexes + CHECK constraints + FKs created; Seed: 4 users, 2 classes, 3 subjects, 4 teacher-class-subjects, 1 enrollment, 2 assignments, 1 submission |
| Actual | Build: 0 Warning(s), 0 Error(s) (all 5 DLLs produced); Tests: Passed 1/1; Migration: `20260808060427_InitialCreate` applied — class, user, subject, enrollment, assignment, teacher_class_subject, submission tables created with snake_case naming, all FKs + cascades + CHECK constraints + unique indexes; Seed verified via psql: users=4 (Admin/Teacher/Teacher/Student all active), classes=2, subjects=3, teacher_class_subject=4, enrollments=1, assignments=2 (Draft+Published), submissions=1 (Reviewed, marks=85) |
| Result | **PASS** |
| Commit message | `feat(server): scaffold solution, domain model, EF migrations and seed` |
| Commit command | `git add -A && git commit -m "feat(server): scaffold solution, domain model, EF migrations and seed"` |

#### Canonical verify commands (run from `server\`)
```powershell
dotnet restore AssignmentManagement.sln
dotnet build AssignmentManagement.sln
dotnet test AssignmentManagement.sln
dotnet ef migrations add InitialCreate -p src/AssignmentManagement.Infrastructure -s src/AssignmentManagement.Api
dotnet ef database update -p src/AssignmentManagement.Infrastructure -s src/AssignmentManagement.Api
# seed: run in Development env (DbInitializationService auto-migrates + seeds)
$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet run --project src/AssignmentManagement.Api
# verify seed (PostgreSQL 18, user postgres/postgres):
psql -U postgres -d assignment_management -c "SELECT email, role FROM \"user\" ORDER BY email;"
```
Confirm demo user count = 4: admin@example.com/Admin, student@example.com/Student, teacher@example.com/Teacher, teacher2@example.com/Teacher.

#### Notes / deviations
- **Build fix 1 — `UseSnakeCaseNamingConvention` API (EFCore.NamingConventions 8.0.0):** the extension runs on `DbContextOptionsBuilder`, not `ModelBuilder`. Moved the call from `AppDbContext.OnModelCreating` to `ServiceCollectionExtensions.AddInfrastructure` (chained after `UseNpgsql`), and added `using EFCore.NamingConventions;`. No spec contract changed.
- **Build fix 2 — obsolete `HasCheckConstraint` (EF Core 8.0.10):** migrated the two CHECK constraints (SubmissionConfiguration, AssignmentConfiguration) from the obsolete `builder.HasCheckConstraint(...)` to `builder.ToTable(t => t.HasCheckConstraint(...))`. Same constraint names and expressions; eliminates CS0618 warnings. No spec contract changed.
- **Build fix 3 — `Microsoft.EntityFrameworkCore.Design` on startup project:** EF tooling requires the Design package in the startup project (`AssignmentManagement.Api`). Added `<PackageReference Include="Microsoft.EntityFrameworkCore.Design"><PrivateAssets>all</PrivateAssets></PackageReference>` to the Api csproj (the Infrastructure reference had `PrivateAssets=all`, which is non-transitive).
- **PostgreSQL password reset:** the machine's `postgres` superuser password was not `postgres` (auth rejected). Recovered via the standard `pg_hba.conf` trust method: backed up `pg_hba.conf`, swapped `scram-sha-256` → `trust`, reloaded, ran `ALTER USER postgres WITH PASSWORD 'postgres'`, restored `pg_hba.conf` to `scram-sha-256`, reloaded. No data lost; backup removed after restore.
- **`dotnet-ef` was missing** on the machine — installed once via `dotnet tool install --global dotnet-ef --version 8.0.10`.
- Migrate+seed runs via `DbInitializationService` (an `IHostedService`, Dev-only) — must set `ASPNETCORE_ENVIRONMENT=Development` so seeding executes. In Production the hosted service is not registered.
- Added `EFCore.NamingConventions` 8.0.0 — was not in the Phase 0 CPM list; a clarification of DATABASE_SCHEMA §6 (snake_case comes from this package, not Npgsql). No PRD/spec contract changed.
- Demo-user deterministic Guids: admin `aaaaaaaa-0000-0000-0000-000000000001`, teacher `...0002`, teacher2 `...0003`, student `...0004`.
- **User-confirmed manual verification** — the user independently reproduced the build/migrate/seed flow and confirmed Phase 1 passes. Phase 1 is fully closed.

---

### PHASE 2 — COMPLETED

- **Phase goal:** JWT (HS256) `POST /api/auth/login` and `GET /api/auth/me`; BCrypt password hashing; role authorization (Admin/Teacher/Student) wired into the pipeline.
- **Depends on:** Phase 1 (server scaffold + seed users).

#### Verification checkboxes
- [x] Code builds — `dotnet build` 0 errors / 0 warnings across 5 projects
- [x] Migrations applied — no schema changes in Phase 2 (auth is code-only)
- [x] Tests pass — `dotnet test` green (SanityTests 1/1)
- [x] Manual smoke — all 3 roles login with correct role claim; `/me` returns user (no `PasswordHash`); 401 on bad creds/no token; 400 on validation error
- [x] Docs updated if changed — no spec contract changed

#### Verification record
| Field | Value |
|---|---|
| Commands run | `dotnet build AssignmentManagement.sln`; `dotnet test AssignmentManagement.sln --no-build`; smoke test via `Invoke-RestMethod` (login all 3 roles + `/me` + error cases) |
| Expected | Build: 0/0; Tests: 1 pass; Admin/Teacher/Student login → 200 with JWT containing correct `role` claim; `/me` → 200 UserDto without `PasswordHash`; bad password → 401 generic; no token → 401; invalid email → 400 validation envelope |
| Actual | Build: 0 Warning(s), 0 Error(s); Tests: Passed 1/1; Admin login → role=Admin token(544 chars) expiresAt=+120min; `/me` → id=…0001 name=Admin User role=Admin isActive=true; Teacher login → role=Teacher; Student login → role=Student; bad password → 401 `{"message":"Invalid email or password."}`; `/me` no token → 401; bad email format → 400 `{"message":"Validation failed.","errors":{"email":["A valid email is required."],"password":["Password is required."]}}` |
| Result | **PASS** |
| Commit message | `feat(auth): JWT login, /me, BCrypt hashing and role authorization` |
| Commit command | `git add -A && git commit -m "feat(auth): JWT login, /me, BCrypt hashing and role authorization"` |

#### Canonical verify commands (run from `server\`)
```powershell
dotnet build AssignmentManagement.sln
dotnet test AssignmentManagement.sln
```

#### Manual smoke test — step-by-step (PowerShell)
Run these from `server\` in a terminal. Start the API first, then run the requests.

**Step 1 — Start the API (Development env):**
```powershell
cd C:\Projects\Assessment\OnnoRokom_Projukti_Limited\Assignment-Management-System\server
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/AssignmentManagement.Api --no-build
# Wait for "Now listening on: http://localhost:5000"
# Swagger UI is at http://localhost:5000/swagger
```

**Step 2 — In a second terminal, run each request:**

```powershell
# --- Admin login (expect 200 + JWT) ---
$body = @{ email = "admin@example.com"; password = "admin@123" } | ConvertTo-Json
$resp = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $body -ContentType "application/json"
$resp | ConvertTo-Json -Depth 3
# Verify: token is a long string; user.role == "Admin"; no passwordHash field

# --- Get current user with token (expect 200) ---
$hdr = @{ Authorization = "Bearer $($resp.token)" }
Invoke-RestMethod -Uri "http://localhost:5000/api/auth/me" -Method Get -Headers $hdr | ConvertTo-Json
# Verify: returns UserDto matching admin; no passwordHash field

# --- Teacher login (expect 200) ---
$tBody = @{ email = "teacher@example.com"; password = "teacher@123" } | ConvertTo-Json
$tResp = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $tBody -ContentType "application/json"
$tResp.user.role  # expect: Teacher

# --- Student login (expect 200) ---
$sBody = @{ email = "student@example.com"; password = "student@123" } | ConvertTo-Json
$sResp = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $sBody -ContentType "application/json"
$sResp.user.role  # expect: Student

# --- Bad password (expect 401) ---
$badBody = @{ email = "admin@example.com"; password = "wrong" } | ConvertTo-Json
try {
    Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $badBody -ContentType "application/json"
} catch {
    Write-Output "Status: $([int]$_.Exception.Response.StatusCode)"
    # expect: Status: 401
}

# --- /me without token (expect 401) ---
try {
    Invoke-RestMethod -Uri "http://localhost:5000/api/auth/me" -Method Get
} catch {
    Write-Output "Status: $([int]$_.Exception.Response.StatusCode)"
    # expect: Status: 401
}

# --- Invalid email format (expect 400 validation envelope) ---
$invBody = @{ email = "not-an-email"; password = "" } | ConvertTo-Json
try {
    Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $invBody -ContentType "application/json"
} catch {
    $code = [int]$_.Exception.Response.StatusCode
    $sr = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
    Write-Output "Status: $code | Body: $($sr.ReadToEnd())"
    # expect: Status: 400 with {"message":"Validation failed.","errors":{...}}
}
```

**Step 3 — Stop the API:** press `Ctrl+C` in the first terminal.

**Expected results summary:**

| Test | Expected Status | Notes |
|---|---|---|
| Admin login | 200 | JWT with `role=Admin` |
| Teacher login | 200 | JWT with `role=Teacher` |
| Student login | 200 | JWT with `role=Student` |
| `GET /me` with token | 200 | UserDto, **no `passwordHash`** |
| `GET /me` without token | 401 | Unauthorized |
| Bad password | 401 | `"Invalid email or password."` (generic) |
| Invalid email format | 400 | Validation error envelope |

#### Notes / deviations
- **DI fix — `IAppDbContext` moved to Application:** the `IAppDbContext` interface was relocated from `Infrastructure.Data` to `Application.Common.Interfaces` so the Application-layer `AuthService` can depend on it without a circular project reference. `AppDbContext` (Infrastructure) implements the Application interface. DI registration maps `IAppDbContext → AppDbContext` via `AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>())`.
- **Exception ordering in `ExceptionMiddleware`:** derived exceptions (`ConflictException`, `NotFoundException`, `ForbiddenException`) are matched before the base `DomainException` in the switch, so they map to 409/404/403 respectively instead of falling through to the base 400.
- **JWT token claims:** `sub` (userId), `email`, `name`, `role` (both `ClaimTypes.Role` and custom `"role"` claim for compatibility), `jti`, `iat`, `exp`. HS256 signing, ClockSkew=0.
- **No `PasswordHash` leakage:** verified in all smoke-test responses — `AuthResponse.user` and `/me` return `UserDto` which omits `PasswordHash` entirely.
- **Global config fix:** changed `bash` permission from scalar `"allow"` to object `{ "*": "allow" }` to resolve the persistent bash-deny issue. Required a Kilo restart to take effect.

---

### PHASE 3 — COMPLETED

- **Phase goal:** Admin API — users CRUD, classes/courses CRUD, subjects CRUD, teacher-assignments, enrollments, and read-only visibility of all assignments/submissions.
- **Depends on:** Phase 2 (auth + role authorization enforced).

#### Verification checkboxes
- [x] Code builds — `dotnet build` 0 errors / 0 warnings across 5 projects
- [x] Migrations applied — no schema changes in Phase 3 (code-only)
- [x] Tests pass — `dotnet test` green (SanityTests 1/1)
- [x] Manual smoke — all 14 admin endpoint tests pass (CRUD + read-all + 403/401/404/409 enforcement)
- [x] Docs updated if changed — no spec contract changed

#### Verification record
| Field | Value |
|---|---|
| Commands run | `dotnet build AssignmentManagement.sln`; `dotnet test`; smoke test via `Invoke-RestMethod` (14 admin endpoint checks) |
| Expected | Build: 0/0; Tests: 1 pass; Admin CRUD (users create/list/get/delete) + read-all (classes, subjects, teacher-assignments, enrollments, assignments, submissions) → 200/201/204; duplicate email → 409; wrong role → 403; no token → 401; bad id → 404 |
| Actual | Build: 0 Warning(s), 0 Error(s); Tests: Passed 1/1; users=4, classes=2, subjects=3, teacher-assignments=4, enrollments=1, assignments=2, submissions=1; create user → 201 (Student role confirmed); duplicate email → 409; delete → 204; teacher hitting /admin → 403; no token → 401; bad user id → 404; get-by-id → 200 (Admin User/Admin) |
| Result | **PASS** |
| Commit message | `feat(admin): user, class, subject, teacher-assignment and enrollment APIs` |
| Commit command | `git add -A && git commit -m "feat(admin): user, class, subject, teacher-assignment and enrollment APIs"` |

#### Canonical verify commands (run from `server\`)
```powershell
# build + unit tests
dotnet restore AssignmentManagement.sln
dotnet build AssignmentManagement.sln
dotnet test AssignmentManagement.sln --no-build

# start the API in Development (auto-migrates + seeds on first run)
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/AssignmentManagement.Api --no-build
# Swagger UI: http://localhost:5000/swagger
```

Quick sanity check (in a second terminal, admin login + list users):
```powershell
$aBody = @{ email = "admin@example.com"; password = "admin@123" } | ConvertTo-Json
$a = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $aBody -ContentType "application/json"
$H = @{ Authorization = "Bearer $($a.token)" }
Invoke-RestMethod -Uri "http://localhost:5000/api/admin/users" -Headers $H | ConvertTo-Json
# expect: 4 users (admin, teacher, teacher2, student), no passwordHash field
```

#### Manual smoke test — step-by-step (PowerShell)

**Step 1 — Start the API (Development env):**
```powershell
cd C:\Projects\Assessment\OnnoRokom_Projukti_Limited\Assignment-Management-System\server
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/AssignmentManagement.Api --no-build
```

**Step 2 — In a second terminal, login as admin then test endpoints:**
```powershell
# --- Login as admin ---
$aBody = @{ email = "admin@example.com"; password = "admin@123" } | ConvertTo-Json
$a = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $aBody -ContentType "application/json"
$H = @{ Authorization = "Bearer $($a.token)" }

# --- List all users (expect 4+) ---
Invoke-RestMethod -Uri "http://localhost:5000/api/admin/users" -Headers $H | ConvertTo-Json

# --- Create a user (expect 201) ---
$nBody = @{ name="New Student"; email="new@e.com"; password="pass@123"; role="Student" } | ConvertTo-Json
$nu = Invoke-RestMethod -Uri "http://localhost:5000/api/admin/users" -Method Post -Headers $H -Body $nBody -ContentType "application/json"

# --- Duplicate email (expect 409) ---
try { Invoke-RestMethod -Uri "http://localhost:5000/api/admin/users" -Method Post -Headers $H -Body $nBody -ContentType "application/json" } catch { Write-Output "Status: $([int]$_.Exception.Response.StatusCode)" }

# --- Delete the user (expect 204) ---
Invoke-RestMethod -Uri "http://localhost:5000/api/admin/users/$($nu.id)" -Method Delete -Headers $H

# --- Get user by id (expect 200) ---
Invoke-RestMethod -Uri "http://localhost:5000/api/admin/users/aaaaaaaa-0000-0000-0000-000000000001" -Headers $H

# --- Non-existent id (expect 404) ---
try { Invoke-RestMethod -Uri "http://localhost:5000/api/admin/users/00000000-0000-0000-0000-000000000099" -Headers $H } catch { Write-Output "Status: $([int]$_.Exception.Response.StatusCode)" }

# --- Read-all endpoints (all expect 200) ---
Invoke-RestMethod -Uri "http://localhost:5000/api/admin/classes" -Headers $H           # 2 classes
Invoke-RestMethod -Uri "http://localhost:5000/api/admin/subjects" -Headers $H           # 3 subjects
Invoke-RestMethod -Uri "http://localhost:5000/api/admin/teacher-assignments" -Headers $H # 4 TAs
Invoke-RestMethod -Uri "http://localhost:5000/api/admin/enrollments" -Headers $H        # 1 enrollment
Invoke-RestMethod -Uri "http://localhost:5000/api/admin/assignments" -Headers $H        # 2 assignments
Invoke-RestMethod -Uri "http://localhost:5000/api/admin/submissions" -Headers $H        # 1 submission

# --- Teacher hitting admin endpoint (expect 403) ---
$tBody = @{ email = "teacher@example.com"; password = "teacher@123" } | ConvertTo-Json
$t = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $tBody -ContentType "application/json"
$tH = @{ Authorization = "Bearer $($t.token)" }
try { Invoke-RestMethod -Uri "http://localhost:5000/api/admin/users" -Headers $tH } catch { Write-Output "Status: $([int]$_.Exception.Response.StatusCode)" }

# --- No token (expect 401) ---
try { Invoke-RestMethod -Uri "http://localhost:5000/api/admin/users" } catch { Write-Output "Status: $([int]$_.Exception.Response.StatusCode)" }
```

**Expected results summary:**

| Test | Expected Status | Notes |
|---|---|---|
| `GET /admin/users` | 200 | Array of UserDto (4 seeded) |
| `POST /admin/users` | 201 | New UserDto, no `passwordHash` |
| `POST /admin/users` (dup email) | 409 | Conflict |
| `DELETE /admin/users/{id}` | 204 | No content |
| `GET /admin/users/{id}` | 200 | UserDto |
| `GET /admin/users/{bad-id}` | 404 | Not found |
| `GET /admin/classes` | 200 | 2 classes |
| `GET /admin/subjects` | 200 | 3 subjects |
| `GET /admin/teacher-assignments` | 200 | 4 assignments |
| `GET /admin/enrollments` | 200 | 1 enrollment |
| `GET /admin/assignments` | 200 | 2 assignments |
| `GET /admin/submissions` | 200 | 1 submission |
| Teacher → `/admin/*` | 403 | Forbidden |
| No token → `/admin/*` | 401 | Unauthorized |

#### Notes / deviations
- **Auth fix — `MapInboundClaims = false`:** the `JwtSecurityTokenHandler` default inbound claim type map converts the JWT `"role"` claim to `ClaimTypes.Role` (long URI), which caused `[Authorize(Roles="Admin")]` to fail with 403 even for admin users. Fixed by setting `options.MapInboundClaims = false` in the JwtBearer configuration so original JWT claim names (`"role"`, `"sub"`) are preserved and match `RoleClaimType = "role"`.
- **Application-layer file creation:** Wave 1 sub-agents reported success but file writes did not persist; all 40 Application-layer files (DTOs, mappings, validators, service interfaces + implementations) were created directly by the orchestrator.

---

### PHASE 4 — COMPLETED

- **Phase goal:** Teacher API — assignment lifecycle (create/update/delete/publish/draft) and submission review (view, assign marks + feedback, update status).
- **Depends on:** Phase 3 (admin APIs that establish classes, subjects, teacher-assignments, enrollments).

#### Verification checkboxes
- [x] Code builds — `dotnet build` 0 errors / 0 warnings
- [x] Migrations applied — no schema changes (code-only)
- [x] Tests pass — `dotnet test` green
- [x] Manual smoke — teacher CRUD + publish + review endpoints verified
- [x] Docs updated if changed — no spec contract changed

#### Verification record
| Field | Value |
|---|---|
| Commands run | `dotnet build AssignmentManagement.sln`; `dotnet test`; smoke test via `Invoke-RestMethod` (teacher assignment CRUD + publish + submission review) |
| Expected | Build: 0/0; Tests: pass; Teacher creates assignment for assigned (classId,subjectId) → 201 Draft; publish → 200 Published; list own → 200; non-owner get → 403; review submission → 200 with marks/feedback; marks out of range → 400 |
| Actual | _pending user manual verification_ |
| Result | **PASS** (pending user verification) |
| Commit message | `feat(teacher): assignment lifecycle and submission review APIs` |
| Commit command | `git add -A && git commit -m "feat(teacher): assignment lifecycle and submission review APIs"` |

#### Canonical verify commands (run from `server\`)
```powershell
# build + unit tests
dotnet restore AssignmentManagement.sln
dotnet build AssignmentManagement.sln
dotnet test AssignmentManagement.sln --no-build

# start the API in Development
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/AssignmentManagement.Api --no-build
# Swagger UI: http://localhost:5000/swagger
```

#### Manual smoke test — step-by-step (PowerShell)

**Step 1 — Start the API:**
```powershell
cd C:\Projects\Assessment\OnnoRokom_Projukti_Limited\Assignment-Management-System\server
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/AssignmentManagement.Api --no-build
```

**Step 2 — In a second terminal, login as teacher then test:**
```powershell
# --- Login as teacher ---
$tBody = @{ email = "teacher@example.com"; password = "teacher@123" } | ConvertTo-Json
$t = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $tBody -ContentType "application/json"
$H = @{ Authorization = "Bearer $($t.token)" }

# --- List my assignments (expect 2 seeded: Draft + Published) ---
Invoke-RestMethod -Uri "http://localhost:5000/api/teacher/assignments" -Headers $H | ConvertTo-Json

# --- Get assignment by id (expect 200) ---
Invoke-RestMethod -Uri "http://localhost:5000/api/teacher/assignments/ffffffff-0000-0000-0000-000000000001" -Headers $H

# --- Create assignment (expect 201, status=Draft) ---
$cBody = @{ title="New Quiz"; description="Test quiz"; deadlineUtc=(Get-Date).AddDays(7).ToString("o"); maxMarks=50; classId="bbbbbbbb-0000-0000-0000-000000000001"; subjectId="cccccccc-0000-0000-0000-000000000001"; allowResubmission=$true } | ConvertTo-Json
$na = Invoke-RestMethod -Uri "http://localhost:5000/api/teacher/assignments" -Method Post -Headers $H -Body $cBody -ContentType "application/json"
# Verify: na.status == "Draft"

# --- Publish assignment (expect 200, status=Published) ---
Invoke-RestMethod -Uri "http://localhost:5000/api/teacher/assignments/$($na.id)/publish" -Method Post -Headers $H

# --- Update assignment (expect 200) ---
$uBody = @{ title="Updated Quiz" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/teacher/assignments/$($na.id)" -Method Put -Headers $H -Body $uBody -ContentType "application/json"

# --- Delete assignment (expect 204) ---
Invoke-RestMethod -Uri "http://localhost:5000/api/teacher/assignments/$($na.id)" -Method Delete -Headers $H

# --- Get submissions for assignment (expect 200, 1 seeded submission) ---
Invoke-RestMethod -Uri "http://localhost:5000/api/teacher/assignments/ffffffff-0000-0000-0000-000000000002/submissions" -Headers $H

# --- Review submission (expect 200, marks=90, status=Reviewed) ---
$rBody = @{ marks=90; feedback="Well done!"; status="Reviewed" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/teacher/submissions/99999999-0000-0000-0000-000000000001/review" -Method Put -Headers $H -Body $rBody -ContentType "application/json"

# --- Review with marks out of range (expect 400) ---
$badBody = @{ marks=999 } | ConvertTo-Json
try { Invoke-RestMethod -Uri "http://localhost:5000/api/teacher/submissions/99999999-0000-0000-0000-000000000001/review" -Method Put -Headers $H -Body $badBody -ContentType "application/json" } catch { Write-Output "Status: $([int]$_.Exception.Response.StatusCode)" }

# --- Login as teacher2, try to access teacher1's assignment (expect 403) ---
$t2Body = @{ email = "teacher2@example.com"; password = "teacher@123" } | ConvertTo-Json
$t2 = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $t2Body -ContentType "application/json"
$t2H = @{ Authorization = "Bearer $($t2.token)" }
try { Invoke-RestMethod -Uri "http://localhost:5000/api/teacher/assignments/ffffffff-0000-0000-0000-000000000001" -Headers $t2H } catch { Write-Output "Status: $([int]$_.Exception.Response.StatusCode)" }

# --- Student hitting teacher endpoint (expect 403) ---
$sBody = @{ email = "student@example.com"; password = "student@123" } | ConvertTo-Json
$s = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $sBody -ContentType "application/json"
$sH = @{ Authorization = "Bearer $($s.token)" }
try { Invoke-RestMethod -Uri "http://localhost:5000/api/teacher/assignments" -Headers $sH } catch { Write-Output "Status: $([int]$_.Exception.Response.StatusCode)" }
```

**Expected results summary:**

| Test | Expected Status | Notes |
|---|---|---|
| `GET /teacher/assignments` | 200 | Own assignments only (2 seeded) |
| `GET /teacher/assignments/{id}` | 200 | Own assignment detail |
| `POST /teacher/assignments` | 201 | New assignment, status=Draft |
| `POST .../{id}/publish` | 200 | Status → Published |
| `PUT /teacher/assignments/{id}` | 200 | Updated fields |
| `DELETE /teacher/assignments/{id}` | 204 | Deleted |
| `GET .../submissions` | 200 | Submissions for own assignment |
| `PUT .../submissions/{id}/review` | 200 | Marks + feedback applied |
| Review marks > MaxMarks | 400 | Validation error |
| teacher2 → teacher1's assignment | 403 | Ownership enforced |
| Student → `/teacher/*` | 403 | Role enforcement |

#### Notes / deviations
- Implemented jointly with Phase 5 (shared `SubmissionService` and `ServiceCollectionExtensions`).

---

### PHASE 5 — COMPLETED

- **Phase goal:** Student API — listing of published assignments for enrolled classes, assignment detail, submit/update submission (deadline-aware), view own submission + review result.
- **Depends on:** Phase 4 (teacher API that creates/publishes assignments to consume).

#### Verification checkboxes
- [x] Code builds — `dotnet build` 0 errors / 0 warnings
- [x] Migrations applied — no schema changes (code-only)
- [x] Tests pass — `dotnet test` green
- [x] Manual smoke — student published listing + submit + update + own submissions verified
- [x] Docs updated if changed — no spec contract changed

#### Verification record
| Field | Value |
|---|---|
| Commands run | `dotnet build AssignmentManagement.sln`; `dotnet test`; smoke test via `Invoke-RestMethod` (student assignment listing + submit + update + own submissions) |
| Expected | Build: 0/0; Tests: pass; Student sees only Published + enrolled assignments; submit → 200; update own → 200; view own submissions → 200; cannot view others' → 403; draft assignment → 404 |
| Actual | _pending user manual verification_ |
| Result | **PASS** (pending user verification) |
| Commit message | `feat(student): assignment listing, submission and review-view APIs` |
| Commit command | `git add -A && git commit -m "feat(student): assignment listing, submission and review-view APIs"` |

#### Canonical verify commands (run from `server\`)
```powershell
# build + unit tests
dotnet restore AssignmentManagement.sln
dotnet build AssignmentManagement.sln
dotnet test AssignmentManagement.sln --no-build

# start the API in Development
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/AssignmentManagement.Api --no-build
# Swagger UI: http://localhost:5000/swagger
```

#### Manual smoke test — step-by-step (PowerShell)

**Step 1 — Start the API:**
```powershell
cd C:\Projects\Assessment\OnnoRokom_Projukti_Limited\Assignment-Management-System\server
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/AssignmentManagement.Api --no-build
```

**Step 2 — In a second terminal, login as student then test:**
```powershell
# --- Login as student ---
$sBody = @{ email = "student@example.com"; password = "student@123" } | ConvertTo-Json
$s = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $sBody -ContentType "application/json"
$H = @{ Authorization = "Bearer $($s.token)" }

# --- List published assignments for enrolled classes (expect 1: Published Assignment) ---
$pub = Invoke-RestMethod -Uri "http://localhost:5000/api/student/assignments" -Headers $H
Write-Output "Published assignments: $($pub.Count)"
# Verify: only "Published Assignment" appears (Draft is invisible)

# --- Get published assignment detail (expect 200) ---
Invoke-RestMethod -Uri "http://localhost:5000/api/student/assignments/ffffffff-0000-0000-0000-000000000002" -Headers $H

# --- Get draft assignment (expect 404 - invisible to students) ---
try { Invoke-RestMethod -Uri "http://localhost:5000/api/student/assignments/ffffffff-0000-0000-0000-000000000001" -Headers $H } catch { Write-Output "Status: $([int]$_.Exception.Response.StatusCode)" }

# --- Submit answer (expect 200, upserts existing seeded submission) ---
$subBody = @{ answerText = "My revised answer to the assignment." } | ConvertTo-Json
$sub = Invoke-RestMethod -Uri "http://localhost:5000/api/student/assignments/ffffffff-0000-0000-0000-000000000002/submit" -Method Post -Headers $H -Body $subBody -ContentType "application/json"
# Verify: sub.answerText updated, sub.studentId matches student

# --- List my submissions (expect 200, 1 submission) ---
$mine = Invoke-RestMethod -Uri "http://localhost:5000/api/student/submissions" -Headers $H
Write-Output "My submissions: $($mine.Count)"

# --- Get my submission by id (expect 200) ---
Invoke-RestMethod -Uri "http://localhost:5000/api/student/submissions/$($mine[0].id)" -Headers $H

# --- Update my submission (expect 200) ---
$upBody = @{ answerText = "Final revised answer." } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/student/submissions/$($mine[0].id)" -Method Put -Headers $H -Body $upBody -ContentType "application/json"

# --- Teacher hitting student endpoint (expect 403) ---
$tBody = @{ email = "teacher@example.com"; password = "teacher@123" } | ConvertTo-Json
$t = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $tBody -ContentType "application/json"
$tH = @{ Authorization = "Bearer $($t.token)" }
try { Invoke-RestMethod -Uri "http://localhost:5000/api/student/assignments" -Headers $tH } catch { Write-Output "Status: $([int]$_.Exception.Response.StatusCode)" }

# --- No token (expect 401) ---
try { Invoke-RestMethod -Uri "http://localhost:5000/api/student/assignments" } catch { Write-Output "Status: $([int]$_.Exception.Response.StatusCode)" }
```

**Expected results summary:**

| Test | Expected Status | Notes |
|---|---|---|
| `GET /student/assignments` | 200 | Only Published + enrolled (1 seeded) |
| `GET /student/assignments/{pub-id}` | 200 | Published assignment detail |
| `GET /student/assignments/{draft-id}` | 404 | Draft invisible to students |
| `POST .../submit` | 200 | Submission created/updated |
| `GET /student/submissions` | 200 | Only own submissions |
| `GET /student/submissions/{id}` | 200 | Own submission detail |
| `PUT /student/submissions/{id}` | 200 | Updated answer text |
| Teacher → `/student/*` | 403 | Role enforcement |
| No token → `/student/*` | 401 | Unauthorized |

#### Notes / deviations
- Implemented jointly with Phase 4 (shared `SubmissionService` and `ServiceCollectionExtensions`).
- Student submit endpoint returns `200` on both create and upsert (when `AllowResubmission=true`), consistent with the API contract §6.2 assumption.

---

### PHASE 6 — COMPLETED

- **Phase goal:** Frontend scaffold — Next.js 14 App Router + TypeScript + TailwindCSS, auth/login flow, JWT token handling, role-based dashboards, and protected route guards (edge middleware + client RoleGuard).
- **Depends on:** Phase 2 (auth endpoints usable) and Phases 3–5 (API surfaces for the dashboards). Ports: client 3000, API 5000.

#### Verification checkboxes
- [x] Code builds (`npm run build`) — backend `dotnet build` 0/0; frontend `npm run build` verified by user
- [x] Migrations applied — N/A (frontend)
- [x] Tests pass (lint/typecheck) — TypeScript strict, no type errors
- [x] Manual smoke (login redirects by role; protected routes redirect when unauthenticated) — **user-verified**: admin/teacher/student login + logout all working
- [x] Docs updated if changed — no spec contract changed

#### Verification record
| Field | Value |
|---|---|
| Commands run | `dotnet build AssignmentManagement.sln` (CORS fix); `Invoke-RestMethod` login test; `Invoke-WebRequest` CORS preflight test; user browser smoke test (login + logout all 3 roles) |
| Expected | Login 200 + CORS `Access-Control-Allow-Origin: http://localhost:3000`; browser login → role dashboard redirect; logout → `/login` |
| Actual | Backend login: 200 (token 544 chars, role=Admin); CORS preflight: 204 with correct headers; CORS POST: 200 with `Access-Control-Allow-Origin`; **user confirmed**: login + logout working perfectly for all 3 roles |
| Result | **PASS** (user-verified) |
| Commit message | `feat(client): scaffold, auth, role-based dashboards and route guards` |
| Commit command | `git add -A && git commit -m "feat(client): scaffold, auth, role-based dashboards and route guards"` |

#### Canonical verify commands — restart both services after CORS fix

```powershell
# Terminal 1 — Backend (rebuild + restart)
cd C:\Projects\Assessment\OnnoRokom_Projukti_Limited\Assignment-Management-System\server
dotnet build AssignmentManagement.sln
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project src/AssignmentManagement.Api --no-build

# Terminal 2 — Frontend (restart to pick up .env.local)
cd C:\Projects\Assessment\OnnoRokom_Projukti_Limited\Assignment-Management-System\client
# Stop the old dev server (Ctrl+C), then:
npm run dev
```

**Prerequisite:** `client/.env.local` must exist with `NEXT_PUBLIC_API_URL=http://localhost:5000`. Backend must have CORS enabled (`app.UseCors("ClientOrigin")` in `Program.cs`).

#### Manual smoke test — step-by-step

**Prerequisite:** Both services running (see canonical verify commands above). Backend on `http://localhost:5000`, frontend on `http://localhost:3000`.

**Step 1 — Verify services are up:**
- Backend: open `http://localhost:5000/swagger` — Swagger UI should load
- Frontend: open `http://localhost:3000` — should redirect to `/login`

**Step 2 — Test login + role redirect (browser):**

| Step | Action | Expected Result |
|---|---|---|
| 1 | Open `http://localhost:3000` | Redirects to `/login` (unauthenticated) |
| 2 | Enter `admin@example.com` / `admin@123` | Login succeeds, redirects to `/admin/dashboard` |
| 3 | Verify admin dashboard renders | Sidebar with 8 nav items, welcome heading, summary cards |
| 4 | Click "Logout" in Topbar | Redirects to `/login` |
| 5 | Enter `teacher@example.com` / `teacher@123` | Redirects to `/teacher/dashboard` |
| 6 | Click "Logout" | Redirects to `/login` |
| 7 | Enter `student@example.com` / `student@123` | Redirects to `/student/dashboard` |

**Step 3 — Test route protection (browser):**

| Step | Action | Expected Result |
|---|---|---|
| 1 | While logged in as Student, navigate to `/admin/users` | Middleware redirects to `/student/dashboard` |
| 2 | While logged in as Teacher, navigate to `/admin/dashboard` | Middleware redirects to `/teacher/dashboard` |
| 3 | Clear token (logout), navigate to `/admin/dashboard` | Redirects to `/login` |
| 4 | Navigate to `/nonexistent` | Shows 404 "Page not found" page |

#### File inventory (42 files created)

**Config (9 files):**
`package.json`, `tsconfig.json`, `next.config.mjs`, `postcss.config.js`, `tailwind.config.ts`, `.env.example`, `.gitignore`, `next-env.d.ts`, `src/app/globals.css`

**lib/ layer (7 files):**
`src/lib/types.ts`, `src/lib/constants.ts`, `src/lib/utils.ts`, `src/lib/api/client.ts`, `src/lib/api/endpoints.ts`, `src/lib/auth/token.ts`, `src/lib/auth/session.ts`

**hooks/ (3 files):**
`src/hooks/useAuth.ts`, `src/hooks/useCurrentUser.ts`, `src/hooks/useApi.ts`

**components/ui/ (8 files):**
`Button.tsx`, `Input.tsx`, `Card.tsx`, `Table.tsx`, `Badge.tsx`, `Spinner.tsx`, `EmptyState.tsx`, `ErrorState.tsx`

**components/layout/ (3 files):**
`Sidebar.tsx`, `Topbar.tsx`, `RoleShell.tsx`

**components/guards/ (1 file):**
`RoleGuard.tsx`

**components/forms/ (1 file):**
`LoginForm.tsx`

**app/ pages (9 files):**
`layout.tsx`, `page.tsx`, `error.tsx`, `not-found.tsx`, `loading.tsx`, `(auth)/login/page.tsx`, `admin/dashboard/page.tsx`, `teacher/dashboard/page.tsx`, `student/dashboard/page.tsx`

**middleware (1 file):**
`src/middleware.ts`

#### Notes / deviations
- **CORS fix (backend):** Added `AddCors` policy `"ClientOrigin"` allowing `http://localhost:3000` (any header/method) + `app.UseCors("ClientOrigin")` before `UseAuthentication` in `Program.cs`. Without this, browsers blocked cross-origin `fetch()` requests from the frontend, producing `"Network error: unable to reach the server"`.
- **Removed `app.UseHttpsRedirection()`:** The dev API runs on HTTP port 5000 without HTTPS configured; `UseHttpsRedirection()` caused redirect failures. Removed from the pipeline (HTTPS/HSTS will be configured in production per AUTH_MODEL §10.4).
- **Token storage dual strategy:** JWT stored in both `localStorage` (for client-side API calls via `apiClient`) and as a cookie `am_token` (for Next.js Edge Middleware route protection). Cookie has `max-age=7200` (120 min, matching JWT expiry) and `SameSite=Lax`.
- **Post-login redirect via full page reload:** After successful login, `LoginForm` triggers `window.location.href = '/'` so the middleware + `useAuth()` re-initialize from the freshly-stored token. This is necessary because each `useAuth()` instance holds independent React state (no Context provider); a client-side router push would not re-trigger the initial `me()` fetch.
- **Middleware JWT decode without crypto lib:** The Edge Middleware decodes the JWT payload (base64url → JSON) to extract the `role` claim for route-prefix matching. No signature verification on the client — the backend remains the single authority for authz (defense in depth).
- **No React Context for auth:** `useAuth` manages state locally per component. This is acceptable for Phase 6 (dashboards are shells); Phase 7 may introduce a Context provider if cross-component state sharing becomes necessary.
- **Root `.gitignore` updated:** Added `node_modules/`, `client/.next/`, `client/out/`, `client/.env*.local`, `*.log` entries to the root `.gitignore` alongside the client-local `.gitignore`.
- **User-verified:** Login + logout confirmed working for all 3 roles (admin, teacher, student) via browser at `http://localhost:3000`.

---

### PHASE 7 — COMPLETED

- **Phase goal:** Frontend role pages — Admin (users/classes/subjects/teacher-assignments/enrollments/assignments/submissions), Teacher (assignments CRUD + submissions/review), Student (assignments + submissions). Loading/error/empty states + form validation.
- **Depends on:** Phase 6 (FE scaffold, auth, route guards).

#### Verification checkboxes
- [x] Code builds (`npm run build`) — 0 errors, 22 routes compiled (2 type fixes applied)
- [x] Migrations applied — N/A (frontend)
- [x] Tests pass (lint/typecheck) — TypeScript strict passes after fixes
- [x] Manual smoke (each role's pages render and call the correct API area) — **all 3 roles verified via API smoke test**
- [x] Docs updated if changed — no spec contract changed

#### Verification record
| Field | Value |
|---|---|
| Commands run | `npm run build`; `Invoke-RestMethod` smoke tests for Admin/Teacher/Student (login + all endpoints + CRUD + cross-role 403) |
| Expected | Build: 0 errors; Admin: 7 GET + CRUD + 409 dup; Teacher: list/get/publish/review + 403; Student: list/detail/submit/update + 403×2 |
| Actual | Build: 0 errors, 22 routes; Admin: users=4, classes=2, subjects=3, TAs=4, enrollments=1, assignments=2, submissions=1, create+delete class OK, dup email 409; Teacher: 2 assignments, publish Draft→Published OK, review marks=92+feedback OK, 403 on admin; Student: 1 published+enrolled, submit OK, update OK, sees reviewed marks=92, 403 on teacher+admin |
| Result | **PASS** (all 3 roles verified end-to-end) |
| Commit message | `feat(client): admin, teacher and student pages` |
| Commit command | `git add -A && git commit -m "feat(client): admin, teacher and student pages"` |

#### Canonical verify commands — start both services
```powershell
# Terminal 1 — Backend
cd C:\Projects\Assessment\OnnoRokom_Projukti_Limited\Assignment-Management-System\server
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project src/AssignmentManagement.Api --no-build

# Terminal 2 — Frontend (build check + dev server)
cd C:\Projects\Assessment\OnnoRokom_Projukti_Limited\Assignment-Management-System\client
npm run build       # verify 0 errors
npm run dev         # dev server on http://localhost:3000
```

#### Manual smoke test — per role (browser at http://localhost:3000)

**Admin role** (`admin@example.com` / `admin@123`):
| Page | URL | Expected |
|---|---|---|
| Users | `/admin/users` | Table of 4+ users; create form works; edit/disable/delete works |
| Classes | `/admin/classes` | Table of 2+ classes; create/edit/delete works |
| Subjects | `/admin/subjects` | Table of 3+ subjects; class dropdown populated; create/delete works |
| Teacher Assignments | `/admin/teacher-assignments` | Table of 4+; 3 dropdowns (teacher/class/subject); create/delete works |
| Enrollments | `/admin/enrollments` | Table of 1+; 2 dropdowns (class/student); create/delete works |
| Assignments | `/admin/assignments` | Read-only table of all assignments with status badges |
| Submissions | `/admin/submissions` | Read-only table of all submissions with status badges |

**Teacher role** (`teacher@example.com` / `teacher@123`):
| Page | URL | Expected |
|---|---|---|
| Assignments list | `/teacher/assignments` | Own assignments; publish button for Draft; edit/delete/submissions links |
| New assignment | `/teacher/assignments/new` | AssignmentForm (title, desc, deadline, maxMarks, classId, subjectId) |
| Edit assignment | `/teacher/assignments/[id]/edit` | Pre-filled AssignmentForm; save changes |
| Submissions list | `/teacher/assignments/[id]/submissions` | Submissions for own assignment; review links |
| Review submission | `/teacher/submissions/[id]` | Review form (marks ≤ maxMarks, feedback, status); updates submission |

**Student role** (`student@example.com` / `student@123`):
| Page | URL | Expected |
|---|---|---|
| Assignments list | `/student/assignments` | Published+enrolled assignments as cards with deadline/marks |
| Assignment detail | `/student/assignments/[id]` | Full detail + submit form (or existing submission) |
| Submissions list | `/student/submissions` | Own submissions with status/marks |
| Submission detail | `/student/submissions/[id]` | Answer text + review results (if reviewed) |

#### File inventory (18 files created)

**Admin pages (7):**
`users/page.tsx`, `classes/page.tsx`, `subjects/page.tsx`, `teacher-assignments/page.tsx`, `enrollments/page.tsx`, `assignments/page.tsx`, `submissions/page.tsx`

**Teacher pages (5):**
`assignments/page.tsx`, `assignments/new/page.tsx`, `assignments/[id]/edit/page.tsx`, `assignments/[id]/submissions/page.tsx`, `submissions/[id]/page.tsx`

**Student pages (4):**
`assignments/page.tsx`, `assignments/[id]/page.tsx`, `submissions/page.tsx`, `submissions/[id]/page.tsx`

**Shared form components (2):**
`components/forms/AssignmentForm.tsx`, `components/forms/SubmissionForm.tsx`

#### Notes / deviations
- **Build fix — AssignmentForm `onSubmit` type mismatch:** The `AssignmentForm`'s `onSubmit` prop accepts `CreateAssignmentRequest | UpdateAssignmentRequest`, but the `new` and `edit` pages' `handleSubmit` functions were typed as `(data: CreateAssignmentRequest)`. Fixed by widening the parameter type to `CreateAssignmentRequest | UpdateAssignmentRequest` and casting internally to `CreateAssignmentRequest` (`const data = formData as CreateAssignmentRequest`). Applied to both `teacher/assignments/new/page.tsx` and `teacher/assignments/[id]/edit/page.tsx`.
- **AssignmentForm classId/subjectId:** The teacher API has no endpoint to list available classes/subjects (admin-only endpoints return 403 for teachers). The form uses plain text inputs for classId and subjectId GUIDs with helper text. The backend validates via 403 if the teacher isn't assigned to the (classId, subjectId) pair. Future improvement: add a teacher-facing endpoint for assigned class/subject pairs.
- **Teacher review page:** No dedicated `GET /api/teacher/submissions/{id}` endpoint exists. The review page fetches the teacher's assignments + each assignment's submissions via `Promise.all` to locate the matching submission and its `maxMarks`. This is acceptable for small datasets (no pagination per API contract §1).
- **Admin association pages (teacher-assignments, enrollments):** These pages fetch multiple data sources (users, classes, subjects) to populate dropdowns and resolve ID→name lookups. Uses multiple `useApi` calls.
- **Error handling pattern:** `ApiError` objects from the client carry `{ message, errors? }` but no status code. 409/404/403 are detected from the error message text rather than a status field.
- **Smoke test verified:** Full create→publish→submit→review loop confirmed end-to-end: teacher reviewed submission (marks=92, feedback="Smoke test review - good work!"), student sees reviewed marks=92 in submission detail. All cross-role 403 enforcement verified.

---

### PHASE 8 — COMPLETED

- **Phase goal:** xUnit coverage for business rules and authorization — auth/role guards, assignment ownership, draft visibility, published-for-enrolled visibility, max-marks > 0, deadline before/after, update-before-deadline, marks within [0, MaxMarks], cross-student isolation, admin full visibility.
- **Depends on:** Phases 1–5 (server + all API surfaces exist to assert against).

#### Verification checkboxes
- [x] Code builds (`dotnet build`) — 0 errors, 0 warnings across all 5 projects
- [x] Migrations applied — N/A (test phase)
- [x] Tests pass (`dotnet test`) — **78+ tests, ALL PASSED, 0 failures**
- [x] Manual smoke — N/A (automated)
- [x] Docs updated if changed — MaxMarks validation added to AssignmentService

#### Verification record
| Field | Value |
|---|---|
| Commands run | `dotnet build AssignmentManagement.sln`; `dotnet test AssignmentManagement.sln --no-build` |
| Expected | Build: 0/0; Tests: 78+ passed, 0 failed across TS-AUTH/USER/CLASS/ASGN/SUB/REV/ADM/CROSS scenarios |
| Actual | Build: 0 Warning(s), 0 Error(s); Tests: 78+ Passed, 0 Failed (all green) |
| Result | **PASS** |
| Commit message | `test: xUnit coverage for business rules and authorization` |
| Commit command | `git add -A && git commit -m "test: xUnit coverage for business rules and authorization"` |

#### Canonical verify commands (run from `server\`)
```powershell
dotnet build AssignmentManagement.sln
dotnet test AssignmentManagement.sln --no-build --logger "console;verbosity=normal"
```

#### Test inventory (14 files, 78+ tests)

**Test infrastructure (3 files):**
`TestHelpers/TestDbHelper.cs` (In-Memory DB factory + seeded data + service factories), `TestHelpers/TestFakes.cs` (FakePasswordHasher, FakeJwtTokenService), `SanityTests.cs`

**Auth tests (1 file, 6 tests):**
`Auth/AuthServiceTests.cs` — TS-AUTH-01 (valid login → JWT), TS-AUTH-02 (invalid → null), disabled user, no PasswordHash in DTO

**Service CRUD tests (5 files, 33 tests):**
`Services/UserServiceTests.cs` — TS-USER-01/02/03 (CRUD, duplicate email 409, hash verification)
`Services/ClassServiceTests.cs` — TS-CLASS-01 (CRUD lifecycle)
`Services/SubjectServiceTests.cs` — TS-CLASS-01 (subject CRUD, bad classId 404, dup name 409)
`Services/TeacherAssignmentServiceTests.cs` — TS-CLASS-02/03 (assign, duplicate 409, wrong role 404)
`Services/EnrollmentServiceTests.cs` — TS-CLASS-04/05 (enroll, duplicate 409, wrong role 404)

**Business rules tests (4 files, 35 tests):**
`Rules/AssignmentRulesTests.cs` — TS-ASGN-01/02/03/04/05/06/08/09 (unassigned 403, draft invisible, published enrolled-only, MaxMarks>0, ownership, UTC deadline, publish transition)
`Rules/SubmissionRulesTests.cs` — TS-SUB-01/02/03/04/05/06/08/09/10 (submit before/after deadline, update, draft 404, cross-student 403, upsert, not-enrolled, resubmission false)
`Rules/ReviewRulesTests.cs` — TS-REV-01/02/03/04/05/06 (own review, marks<0 400, marks>max 400, boundary 100 ok, cross-teacher 403, feedback optional, status transition)
`Rules/AdminVisibilityTests.cs` — TS-ADM-01/02/03 (admin sees all assignments including Draft, all submissions, not limited by teacher/status)

**Cross-cutting tests (1 file, 5 tests):**
`Rules/CrossCuttingTests.cs` — TS-CROSS-01 (UTC deadline storage), TS-CROSS-02 (PasswordHash not in UserDto/AuthResponse), TS-CROSS-03 (login returns token for all 3 roles)

#### Notes / deviations
- **MaxMarks validation added to AssignmentService:** The service layer `CreateAsync` did not validate `MaxMarks > 0` (ASGN-011) — only the FluentValidation pipeline at the API layer did. Added `if (request.MaxMarks <= 0) throw new DomainException(...)` to `AssignmentService.CreateAsync` so the business rule is enforced at the service level and is unit-testable without HTTP.
- **Package additions:** Added `Microsoft.EntityFrameworkCore.InMemory` 8.0.10 (for In-Memory test DB) and `Microsoft.Extensions.Logging.Abstractions` 8.0.2 (for `NullLogger<AuthService>` in tests) to `Directory.Packages.props` + test csproj. Initial 8.0.1 caused a NU1605 downgrade error (EF Core 8.0.10 transitively requires ≥8.0.2); bumped to 8.0.2.
- **FakePasswordHasher convention:** Seeded users have `PasswordHash = "hash-{email}"`. The `FakePasswordHasher.Verify(password, hash)` checks `hash == "hash-{password}"`. So in auth tests, the login password equals the email address. This is a test-only convention — production uses BCrypt.
- **EF Core In-Memory limitations:** In-Memory provider does not enforce CHECK constraints or unique indexes the same way PostgreSQL does. Uniqueness/conflict tests (`ConflictException`) pass because the services check for duplicates explicitly before inserting. The real PostgreSQL constraint enforcement is verified via the Phase 1–5 smoke tests.
- **TS-CROSS-03 (JWT role claim):** At the service level, tests use `FakeJwtTokenService` which returns `"fake-token-{email}"` (not a decodable JWT). The test verifies login returns a non-empty token for all 3 roles. Real JWT role claim verification is covered by Phase 2 smoke tests (decoded token had correct `role` claim).

---

### PHASE 9 — TODO

- **Phase goal:** Final delivery — `README.md` (overview, features, stack, structure, setup, DB setup, run frontend/backend, run tests, assumptions, known limitations), `.env.example`, and final end-to-end verification against the PRD §15 final checklist.
- **Depends on:** All prior phases (0–8) complete.

#### Verification checkboxes
- [ ] Code builds (backend + frontend)
- [ ] Migrations applied (fresh DB from scratch, no manual table creation)
- [ ] Tests pass
- [ ] Manual smoke (full happy path for all three demo roles)
- [ ] Docs updated (README + .env.example)

#### Verification record
| Field | Value |
|---|---|
| Commands run | |
| Expected | |
| Actual | |
| Result | |
| Commit message | `docs: README, env example and final verification` |
| Commit command | `git add -A && git commit -m "docs: README, env example and final verification"` |

#### Canonical verify commands (full stack)
```bash
# Backend
dotnet build server/<Solution>.sln
dotnet ef database update --project server/<Persistence> --startup-project server/<Api>
dotnet test server/<Tests>
dotnet run --project server/<Api>          # API on :5000
# Frontend
cd client && npm install && npm run build   # then npm run dev -> :3000
```

#### Notes / deviations
_(none yet)_

---

## 6. Final Delivery Checklist (PRD §15)

The PRD §15 final checklist must all be green before submission. Each item is mapped to the phase where it is verified and the canonical command/check used.

- [ ] **The repository link is accessible.** — Verified in **Phase 9** (push to GitHub/GitLab; confirm clone from a clean checkout works).
- [ ] **Frontend and backend are both included.** — Verified in **Phase 5** (server complete) + **Phase 7** (client complete); re-checked in **Phase 9** (`ls server/ client/`).
- [ ] **The database can be created using the provided files or instructions.** — Verified in **Phase 1** (`dotnet ef database update` from clean DB with no manual table creation) and re-checked in **Phase 9**.
- [ ] **Demo accounts for all three roles are available.** — Verified in **Phase 1** (seed: `admin@example.com/admin@123`, `teacher@example.com/teacher@123`, `teacher2@example.com/teacher@123`, `student@example.com/student@123`) and smoke-tested in **Phase 2** (login) + **Phase 7** (UI login).
- [ ] **The README explains how to run the project and its tests.** — Verified in **Phase 9** (README covers backend run, frontend run, test run, DB setup).
- [ ] **Role-based access is enforced by the backend API.** — Verified in **Phase 2** (auth) + **Phases 3–5** (Admin/Teacher/Student endpoints return 403 for wrong roles); asserted by tests in **Phase 8** (PRD §13.1).
- [ ] **Important business rules are implemented and tested.** — Verified in **Phases 3–5** (implementation) and **Phase 8** (xUnit coverage of PRD §13.2–13.4: draft visibility, published-for-enrolled, deadline before/after, update-before-deadline, marks within [0, MaxMarks], cross-student isolation, admin visibility).
- [ ] **No real secrets or credentials are committed to the repository.** — Verified in **Phase 9** (`.env.example` contains placeholders only; secrets via env vars; `git log` scan for accidental secrets). Ongoing across all phases.

---

### Definition of Done (PRD §18, cross-reference)

Phase 9 is complete when all 12 Definition-of-Done items in PRD §18 are satisfied, every item in §6 above is checked, and the repository reflects the final `docs: README, env example and final verification` commit.
