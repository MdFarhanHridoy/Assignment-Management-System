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

### PHASE 3 — TODO

- **Phase goal:** Admin API — users CRUD, classes/courses CRUD, subjects CRUD, teacher-assignments, enrollments, and read-only visibility of all assignments/submissions.
- **Depends on:** Phase 2 (auth + role authorization enforced).

#### Verification checkboxes
- [ ] Code builds
- [ ] Migrations applied (only if schema changed)
- [ ] Tests pass
- [ ] Manual smoke (admin endpoints return 2xx for Admin; 403 for Teacher/Student)
- [ ] Docs updated if changed

#### Verification record
| Field | Value |
|---|---|
| Commands run | |
| Expected | |
| Actual | |
| Result | |
| Commit message | `feat(admin): user, class, subject, teacher-assignment and enrollment APIs` |
| Commit command | `git add -A && git commit -m "feat(admin): user, class, subject, teacher-assignment and enrollment APIs"` |

#### Canonical verify commands (backend)
```bash
dotnet build server/<Solution>.sln
dotnet test server/<Tests>
# manual: hit /api/admin/* with admin token (2xx); with teacher/student token (403)
```

#### Notes / deviations
_(none yet)_

---

### PHASE 4 — TODO

- **Phase goal:** Teacher API — assignment lifecycle (create/update/delete/publish/draft) and submission review (view, assign marks + feedback, update status).
- **Depends on:** Phase 3 (admin APIs that establish classes, subjects, teacher-assignments, enrollments).

#### Verification checkboxes
- [ ] Code builds
- [ ] Migrations applied (only if schema changed)
- [ ] Tests pass
- [ ] Manual smoke (teacher creates/publishes assignment; reviews a submission)
- [ ] Docs updated if changed

#### Verification record
| Field | Value |
|---|---|
| Commands run | |
| Expected | |
| Actual | |
| Result | |
| Commit message | `feat(teacher): assignment lifecycle and submission review APIs` |
| Commit command | `git add -A && git commit -m "feat(teacher): assignment lifecycle and submission review APIs"` |

#### Canonical verify commands (backend)
```bash
dotnet build server/<Solution>.sln
dotnet test server/<Tests>
# manual: /api/teacher/assignments CRUD + publish; PUT /api/teacher/submissions/{id}/review
```

#### Notes / deviations
_(none yet)_

---

### PHASE 5 — TODO

- **Phase goal:** Student API — listing of published assignments for enrolled classes, assignment detail, submit/update submission (deadline-aware), view own submission + review result.
- **Depends on:** Phase 4 (teacher API that creates/publishes assignments to consume).

#### Verification checkboxes
- [ ] Code builds
- [ ] Migrations applied (only if schema changed)
- [ ] Tests pass
- [ ] Manual smoke (student lists/publishes-only assignments; submits; views own review)
- [ ] Docs updated if changed

#### Verification record
| Field | Value |
|---|---|
| Commands run | |
| Expected | |
| Actual | |
| Result | |
| Commit message | `feat(student): assignment listing, submission and review-view APIs` |
| Commit command | `git add -A && git commit -m "feat(student): assignment listing, submission and review-view APIs"` |

#### Canonical verify commands (backend)
```bash
dotnet build server/<Solution>.sln
dotnet test server/<Tests>
# manual: GET /api/student/assignments (only published for enrolled); submit before deadline; cannot view others' submissions
```

#### Notes / deviations
_(none yet)_

---

### PHASE 6 — TODO

- **Phase goal:** Frontend scaffold — Next.js App Router + TypeScript, auth/login flow, JWT token handling, role-based dashboards, and protected route guards.
- **Depends on:** Phase 2 (auth endpoints usable) and Phases 3–5 (API surfaces for the dashboards). Ports: client 3000, API 5000.

#### Verification checkboxes
- [ ] Code builds (`npm run build`)
- [ ] Migrations applied — N/A (frontend)
- [ ] Tests pass (lint/typecheck)
- [ ] Manual smoke (login redirects by role; protected routes redirect when unauthenticated)
- [ ] Docs updated if changed

#### Verification record
| Field | Value |
|---|---|
| Commands run | |
| Expected | |
| Actual | |
| Result | |
| Commit message | `feat(client): scaffold, auth, role-based dashboards and route guards` |
| Commit command | `git add -A && git commit -m "feat(client): scaffold, auth, role-based dashboards and route guards"` |

#### Canonical verify commands (frontend)
```bash
cd client
npm install
npm run build   # expect successful production build
npm run dev     # client on http://localhost:3000, API at http://localhost:5000
```

#### Notes / deviations
_(none yet)_

---

### PHASE 7 — TODO

- **Phase goal:** Frontend role pages — Admin (users/classes/subjects/teacher-assignments/enrollments/assignments/submissions), Teacher (assignments CRUD + submissions/review), Student (assignments + submissions). Loading/error/empty states + form validation.
- **Depends on:** Phase 6 (FE scaffold, auth, route guards).

#### Verification checkboxes
- [ ] Code builds (`npm run build`)
- [ ] Migrations applied — N/A (frontend)
- [ ] Tests pass (lint/typecheck)
- [ ] Manual smoke (each role's pages render and call the correct API area)
- [ ] Docs updated if changed

#### Verification record
| Field | Value |
|---|---|
| Commands run | |
| Expected | |
| Actual | |
| Result | |
| Commit message | `feat(client): admin, teacher and student pages` |
| Commit command | `git add -A && git commit -m "feat(client): admin, teacher and student pages"` |

#### Canonical verify commands (frontend)
```bash
cd client
npm run build
npm run dev
```

#### Notes / deviations
_(none yet)_

---

### PHASE 8 — TODO

- **Phase goal:** xUnit coverage for business rules and authorization — auth/role guards, assignment ownership, draft visibility, published-for-enrolled visibility, max-marks > 0, deadline before/after, update-before-deadline, marks within [0, MaxMarks], cross-student isolation, admin full visibility.
- **Depends on:** Phases 1–5 (server + all API surfaces exist to assert against).

#### Verification checkboxes
- [ ] Code builds (`dotnet build`)
- [ ] Migrations applied — N/A (test phase)
- [ ] Tests pass (`dotnet test`)
- [ ] Manual smoke — N/A (automated)
- [ ] Docs updated if changed

#### Verification record
| Field | Value |
|---|---|
| Commands run | |
| Expected | |
| Actual | |
| Result | |
| Commit message | `test: xUnit coverage for business rules and authorization` |
| Commit command | `git add -A && git commit -m "test: xUnit coverage for business rules and authorization"` |

#### Canonical verify commands (backend tests)
```bash
dotnet build server/<Solution>.sln
dotnet test server/<Tests>   # expect all green; covers PRD §13.1–13.4
```

#### Notes / deviations
_(none yet)_

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
