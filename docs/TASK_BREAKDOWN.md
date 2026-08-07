# TASK_BREAKDOWN — Assignment & Submission Management System

> **PHASE 0 deliverable.** Granular, orchestrator-ready task list for phases 1–9.
> **Authoritative inputs (read-only):** [`PRD.md`](./PRD.md), [`PROJECT_STRUCTURE.md`](./PROJECT_STRUCTURE.md),
> [`API_CONTRACT.md`](./API_CONTRACT.md), [`DATABASE_SCHEMA.md`](./DATABASE_SCHEMA.md),
> [`BUSINESS_RULES.md`](./BUSINESS_RULES.md), [`AUTH_MODEL.md`](./AUTH_MODEL.md).
> **Naming:** every file, type, enum, route, status code, and rule below is used **verbatim** from the
> canonical contract — do not rename. All paths are relative to the repo root
> `Assignment-Management-System/`.

---

## 1. How to use this doc

- **Phases map 1:1 to `docs/IMPLEMENTATION_PLAN.md`.** Each section (Phase 1–9) is a buildable unit; do
  not start Phase N+1 until every task in Phase N passes its verify command.
- **One task = one small, verifiable change.** A task touches a handful of files and has exactly one
  acceptance signal (build green, a command succeeds, a row exists, a status code matches). If a task
  feels large, split it.
- **Execute in order within a phase** (later tasks depend on earlier ones: entities → DbContext →
  migration → seed → services → controllers → tests).
- **Verify after every task**, not just after the phase. The `Verify` column is the gate.
- **After each phase completes**, update `docs/VERIFICATION_CHECKLIST.md`: tick the phase's items, paste
  the command outputs, and record any deviations. `VERIFICATION_CHECKLIST.md` is the audit trail; this
  file is the plan.
- **Rule traceability:** business-rule enforcement is tagged with `TS-*` scenario IDs from
  [`BUSINESS_RULES.md`](./BUSINESS_RULES.md) §10. Phase 8 closes the loop by implementing those scenarios
  as xUnit tests; every `TS-*` cited in phases 2–5 must have a green test by end of Phase 8.
- **Commands assume a working directory:** `server/` for `dotnet`/`dotnet ef`, `client/` for `npm`,
  repo root for `git`/global. PostgreSQL must be running on `5432` for any `database update` step.
- **EF CLI convention:** `-p src/AssignmentManagement.Infrastructure -s src/AssignmentManagement.Api`
  (startup = Api, migrations project = Infrastructure), run from `server/`.
- **No secrets in code.** Real `Jwt__Secret`, connection passwords, and `.env.local` never enter the
  repo — only `*.example.*` templates ship.

### Conventions in every task table

| Column | Meaning |
|---|---|
| Task ID | `P<phase>-T<nn>` (e.g. `P1-T03`). Stable; referenced by `VERIFICATION_CHECKLIST.md`. |
| Phase | The phase number this task belongs to (1–9). |
| Goal | One sentence — the verifiable outcome. |
| Files | Real paths to create/modify, drawn from [`PROJECT_STRUCTURE.md`](./PROJECT_STRUCTURE.md). |
| Acceptance criteria | The concrete, checkable condition(s) that mark the task done. |
| Verify | The exact command(s) to run. `✅` expected outcome noted inline. |

---

## 2. Phase 1 — Server scaffold, Domain, DbContext, migration, seed

> **Phase goal:** a compiling .NET 8 solution with the layered projects, the full EF Core model, an
> `InitialCreate` migration, and an idempotent seeder that provisions demo users + sample rows.
> **Starts from empty `server/`.** See [`PROJECT_STRUCTURE.md`](./PROJECT_STRUCTURE.md) §2 and
> [`DATABASE_SCHEMA.md`](./DATABASE_SCHEMA.md) §3–§7.

| Task ID | Phase | Goal | Files | Acceptance criteria | Verify |
|---|---|---|---|---|---|
| P1-T01 | 1 | Create the .NET 8 solution + 5 projects with strict layered references. | `server/AssignmentManagement.sln`; `server/Directory.Build.props` (net8.0, Nullable, ImplicitUsings, LangVersion=latest); `src/AssignmentManagement.Api/AssignmentManagement.Api.csproj`; `src/AssignmentManagement.Application/...csproj`; `src/AssignmentManagement.Domain/...csproj`; `src/AssignmentManagement.Infrastructure/...csproj`; `tests/AssignmentManagement.UnitTests/...csproj` | Solution loads with 5 projects. Refs: `Domain`→none; `Application`→`Domain`; `Infrastructure`→`Application`+`Domain`; `Api`→`Application`+`Infrastructure`+`Domain`; tests→`Application`+`Domain`(+`Infrastructure`). | From `server/`: `dotnet sln AssignmentManagement.sln list` lists all 5; `dotnet build` ✅ green. |
| P1-T02 | 1 | Enable central package management (CPM) and pin all NuGet versions. | `server/Directory.Packages.props` | CPM enabled (`ManagePackageVersionsCentrally=true`). Packages pinned: `Microsoft.EntityFrameworkCore` 8.x, `Npgsql.EntityFrameworkCore.PostgreSQL` 8.x, `Microsoft.EntityFrameworkCore.Design` 8.x, `BCrypt.Net-Next`, `Microsoft.AspNetCore.Authentication.JwtBearer` 8.x, `FluentValidation` (+ `DependencyInjection`), `Microsoft.IdentityModel.Tokens`/`System.IdentityModel.Tokens.Jwt`, `Swashbuckle.AspNetCore`; tests: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `FluentAssertions`, `Microsoft.AspNetCore.Mvc.Testing`, `Testcontainers.PostgreSql`. | `Directory.Packages.props` has one `PackageVersion` per package; no inline `<PackageVersion>` in `.csproj`. | `dotnet restore` ✅; `dotnet list package` shows resolved versions. |
| P1-T03 | 1 | Define the three string-backed enums. | `src/AssignmentManagement.Domain/Enums/UserRole.cs`; `Enums/AssignmentStatus.cs`; `Enums/SubmissionStatus.cs` | Values exactly: `UserRole{Admin,Teacher,Student}`; `AssignmentStatus{Draft,Published,Archived}`; `SubmissionStatus{Submitted,UnderReview,Reviewed,LateSubmitted}`. | `dotnet build` ✅. |
| P1-T04 | 1 | Add domain exceptions + rule constants. | `src/AssignmentManagement.Domain/Exceptions/DomainException.cs`; `NotFoundException.cs`; `ConflictException.cs`; `ForbiddenException.cs`; `Constants/DomainRules.cs` | Exception hierarchy maps to status codes: `DomainException`→400, `NotFoundException`→404, `ConflictException`→409, `ForbiddenException`→403. `DomainRules` holds `MaxMarks > 0` and `0 ≤ Marks ≤ MaxMarks` constants. | `dotnet build` ✅. |
| P1-T05 | 1 | Implement the 7 entities (Guid PKs, `*Utc` timestamps, audit cols). | `src/AssignmentManagement.Domain/Entities/{User,Class,Subject,TeacherClassSubject,Enrollment,Assignment,Submission}.cs` | Fields match [`DATABASE_SCHEMA.md`](./DATABASE_SCHEMA.md) §3 verbatim: e.g. `User{Id,Name,Email,PasswordHash,Role,IsActive,CreatedAt,UpdatedAt?}`; `Assignment{...DeadlineUtc,MaxMarks,Status,TeacherId,ClassId,SubjectId,AllowResubmission,CreatedAt,UpdatedAt?}`; `Submission{...AnswerText,SubmittedAtUtc,UpdatedAtUtc?,Status,Marks?,Feedback?,ReviewedByTeacherId?,ReviewedAtUtc?}`. `DateTime` properties use `DateTimeKind.Utc`. | `dotnet build` ✅. |
| P1-T06 | 1 | Create `AppDbContext` + abstraction with one `DbSet` per entity. | `src/AssignmentManagement.Infrastructure/Data/AppDbContext.cs`; `Data/IAppDbContext.cs` | `AppDbContext : DbContext, IAppDbContext`. DbSets for all 7 entities. `OnModelCreating` applies all configurations from the assembly + calls `UseSnakeCaseNamingConvention()` (Npgsql). `IAppDbContext` exposes sets + `SaveChangesAsync`/`IDbSet` accessors for tests. | `dotnet build` ✅. |
| P1-T07 | 1 | Add `IEntityTypeConfiguration<T>` for all 7 entities. | `src/AssignmentManagement.Infrastructure/Configurations/{User,Class,Subject,TeacherClassSubject,Enrollment,Assignment,Submission}Configuration.cs` | Per [`DATABASE_SCHEMA.md`](./DATABASE_SCHEMA.md) §6: tables→snake_case; enums `.HasConversion<string>()` with defaults (`Status`=`Draft`/`Submitted`, `IsActive`/`AllowResubmission` default true); **unique indexes** `UX_Users_Email`, `UX_Subjects_ClassId_Name`, `UX_TeacherClassSubjects_TeacherId_ClassId_SubjectId`, `UX_Enrollments_ClassId_StudentId`, `UX_Submissions_AssignmentId_StudentId`; FKs with cascade/restrict/set-null per §3; **CHECK constraints** `MaxMarks > 0` and `Marks IS NULL OR Marks >= 0` (via `HasCheckConstraint`); query indexes from §5; `Email` max 256/Name 200/etc.; `DateTime` → `timestamptz`. | `dotnet build` ✅. |
| P1-T08 | 1 | Build the Api host: `Program.cs` + DI wiring + Npgsql + config files. | `src/AssignmentManagement.Api/Program.cs`; `appsettings.json`; `appsettings.Development.json`; `appsettings.example.json`; `Api/Extensions/ServiceCollectionExtensions.cs` | `Program.cs` registers `AppDbContext` with `UseNpgsql(ConnectionStrings:DefaultConnection)` + snake_case, sets `ASPNETCORE_ENVIRONMENT`, configures JSON camelCase + enum-as-string, basic logging, Swagger. Startup project is Api. Ports: API on `5000`. `appsettings.example.json` has placeholders only (no real secret). | `dotnet build` ✅; `dotnet run --project src/AssignmentManagement.Api --no-build` starts and logs "Now listening on http://localhost:5000". |
| P1-T09 | 1 | Generate the `InitialCreate` migration. | `server/src/AssignmentManagement.Infrastructure/Migrations/<ts>_InitialCreate.cs` (+ `.Designer.cs`, `AppDbContextModelSnapshot.cs`) | Migration creates all 7 tables, FKs, unique indexes, CHECK constraints, and query indexes in dependency order. `ModelSnapshot` matches model. | From `server/`: `dotnet ef migrations add InitialCreate -p src/AssignmentManagement.Infrastructure -s src/AssignmentManagement.Api` ✅; `dotnet ef migrations script --idempotent -p src/AssignmentManagement.Infrastructure -s src/AssignmentManagement.Api` reviewed — contains CREATE TABLE for all 7 + constraints. |
| P1-T10 | 1 | Add `IPasswordHasher`/`PasswordHasher` (BCrypt wf 11) + `DbSeeder` for demo users + sample rows. | `src/AssignmentManagement.Application/Common/Interfaces/IPasswordHasher.cs`; `src/AssignmentManagement.Infrastructure/Identity/PasswordHasher.cs`; `Infrastructure/Seeding/DbSeeder.cs` | `PasswordHasher` uses `BCrypt.Net.BCrypt` work factor **11**. `DbSeeder` is **idempotent** (skips if rows by email/seed-Guid exist), stores **lowercased** emails, hashes the 4 demo passwords, and seeds: 2 Classes, 3 Subjects, `TeacherClassSubject` rows (incl. `teacher2@example.com`), `Enrollments` (student in the published-assignment class), 1 `Draft` + 1 `Published` assignment (future `DeadlineUtc`) by a seeded teacher, 1 `Reviewed` submission with marks/feedback. | `dotnet build` ✅; on a migrated DB, `DbSeeder.SeedAsync` runs twice without duplicating rows. |
| P1-T11 | 1 | Auto-migrate + seed on startup. | `src/AssignmentManagement.Api/Program.cs` (startup hook); `Api/Extensions/ServiceCollectionExtensions.cs` (`AddInfrastructure`) | On `Development` start: `context.Database.MigrateAsync()` then `DbSeeder.SeedAsync()`; failures are logged, not swallowed. Evaluator needs **no manual table creation**. | `dotnet ef database update -p src/AssignmentManagement.Infrastructure -s src/AssignmentManagement.Api` ✅ against local Postgres; then `dotnet run` seeds; `psql`/admin login confirms 4 demo users + sample rows exist. |
| P1-T12 | 1 | Phase-1 verification gate. | *(none — review only)* | Solution builds; migration is idempotent; DB applies from scratch; demo users + sample rows present. | From `server/`: `dotnet build` ✅; `dotnet ef migrations script --idempotent -p src/AssignmentManagement.Infrastructure -s src/AssignmentManagement.Api` ✅; `dotnet ef database update -p src/AssignmentManagement.Infrastructure -s src/AssignmentManagement.Api` ✅. Update `docs/VERIFICATION_CHECKLIST.md`. |

---

## 3. Phase 2 — Auth (login, JWT, role policies, error envelope)

> **Phase goal:** `POST /api/auth/login` returns an HS256 JWT; `GET /api/auth/me` returns the current
> user; JwtBearer is wired; the `{message,errors}` envelope works; Swagger offers a JWT box. Covers
> `AUTH-001…007`, `BR-11`, `BR-13`. See [`AUTH_MODEL.md`](./AUTH_MODEL.md) §2–§7.

| Task ID | Phase | Goal | Files | Acceptance criteria | Verify |
|---|---|---|---|---|---|
| P2-T01 | 2 | Auth + safe `UserDto` DTOs. | `src/AssignmentManagement.Application/DTOs/Auth/{LoginRequest,AuthResponse}.cs`; `DTOs/Users/UserDto.cs` | `LoginRequest{email,password}`; `AuthResponse{token,expiresAt,user:UserDto}`; `UserDto` has **no** `PasswordHash` (BR-13). | `dotnet build` ✅. |
| P2-T02 | 2 | `JwtTokenService` issuing HS256 tokens with the full claim set. | `src/AssignmentManagement.Application/Common/Interfaces/IJwtTokenService.cs`; `src/AssignmentManagement.Infrastructure/Identity/JwtTokenService.cs`; `Infrastructure/Identity/JwtOptions.cs` | Token claims: `sub,email,role,name,jti,iat,exp` (`AUTH_MODEL.md` §3); `exp = iat + Jwt:ExpiryMinutes(120)`; `iss=assignment-management-api`, `aud=assignment-management-client`; signed HS256 with `Jwt:Secret` (≥32 bytes). No refresh tokens. | `dotnet build` ✅; decode an issued token (jwt.io) → claims match; `role` claim present (TS-CROSS-03). |
| P2-T03 | 2 | `ICurrentUserService` reading claims. | `src/AssignmentManagement.Application/Common/Interfaces/ICurrentUserService.cs`; `src/AssignmentManagement.Api/.../CurrentUserService.cs` (or Infrastructure) | Exposes `UserId`, `Role`, `Email` from `sub`/`role`/`email` claims (`AUTH_MODEL.md` §9.3). | `dotnet build` ✅. |
| P2-T04 | 2 | `AuthService` (verify → issue). | `src/AssignmentManagement.Application/Services/IAuthService.cs`; `Services/AuthService.cs` | Lowercases email on lookup; `BCrypt.Verify` vs `PasswordHash`; `IsActive==false` → fail. Returns `AuthResponse` or throws `UnauthorizedAccessException` (→401). Failed attempts logged with email+IP only (LOG-1). | `dotnet build` ✅; service returns a token for `teacher@example.com`/`teacher@123`, fails for wrong password. |
| P2-T05 | 2 | `AuthController` (`/api/auth/login`, `/api/auth/me`). | `src/AssignmentManagement.Api/Controllers/AuthController.cs` | `POST /api/auth/login` `[AllowAnonymous]` → 200 `AuthResponse` / 401 / 400; `GET /api/auth/me` `[Authorize]` → 200 `UserDto` / 401. Responses never include `passwordHash`. | `dotnet build` ✅; Swagger shows both routes; manual login returns a token (TS-AUTH-01). |
| P2-T06 | 2 | JwtBearer wiring + role policies. | `src/AssignmentManagement.Api/Extensions/JwtExtensions.cs`; `Program.cs` | All four validations on (`ValidateIssuer/Audience/Lifetime/IssuerSigningKey`), `ClockSkew≈0`. Policies `AdminOnly`/`TeacherOnly`/`StudentOnly` (`AUTH_MODEL.md` §2.2, §9.2). 401 vs 403 per `AUTH_MODEL.md` §9.4. | `dotnet build` ✅; request with no token to `/api/auth/me` → 401; expired token → 401. |
| P2-T07 | 2 | Error envelope `{message,errors}` + `ExceptionMiddleware`. | `src/AssignmentManagement.Api/Middleware/ExceptionMiddleware.cs`; `Api/ProblemDetails/...` helpers; `Program.cs` | Maps: `DomainException`→400, `NotFoundException`→404, `ConflictException`→409, `ForbiddenException`→403, validation→400 with `errors` map, unhandled→500. Envelope shape per [`API_CONTRACT.md`](./API_CONTRACT.md) §2. | `dotnet build` ✅; bad JSON to `/login` → 400 with `{message,errors}`. |
| P2-T08 | 2 | FluentValidation pipeline + `LoginRequestValidator`. | `src/AssignmentManagement.Application/Validators/LoginRequestValidator.cs`; `Program.cs` (auto-register validators) | `email` required + valid format, `password` required; invalid → 400 with `errors.email`/`errors.password`. | `dotnet build` ✅; empty login body → 400 with field errors. |
| P2-T09 | 2 | Swagger with JWT security definition. | `src/AssignmentManagement.Api/Extensions/SwaggerExtensions.cs`; `Program.cs` | Swagger UI has "Authorize" box; OpenAPI security scheme = Bearer. | `dotnet build` ✅; `/swagger` shows Authorize button. |
| P2-T10 | 2 | Auth unit tests (TS-AUTH-01, TS-AUTH-02, TS-CROSS-03). | `tests/AssignmentManagement.UnitTests/Auth/AuthServiceTests.cs` | Valid seeded login → token with `role=Teacher`; invalid password → 401; token carries `role` claim. | `dotnet test --filter "FullyQualifiedName~AuthServiceTests"` ✅ green. |
| P2-T11 | 2 | Phase-2 verification gate. | *(review only)* | Build green; auth tests green; Swagger login returns a token for each demo role. | From `server/`: `dotnet build` ✅; `dotnet test` ✅; manual `/swagger` login as admin/teacher/student → token. Update `docs/VERIFICATION_CHECKLIST.md`. |

---

## 4. Phase 3 — Admin API (users/classes/subjects/teacher-assignments/enrollments + read-all)

> **Phase goal:** all `/api/admin/*` endpoints per [`API_CONTRACT.md`](./API_CONTRACT.md) §4, Admin-gated,
> with unique→409, FK-missing→404, soft-disable. Admin **does not** create/grade (only GET on
> assignments/submissions). Covers `USER-001…006`, `CLASS-001…009`, `BR-1`, `ADM-001/002`.

| Task ID | Phase | Goal | Files | Acceptance criteria | Verify |
|---|---|---|---|---|---|
| P3-T01 | 3 | User DTOs + validators + `UserService` CRUD. | `DTOs/Users/{CreateUserRequest,UpdateUserRequest}.cs`; `Validators/CreateUserRequestValidator.cs`; `Services/{IUserService,UserService}.cs` | Create: hash password (BCrypt wf 11), lowercase email, validate role enum (`USER-005`); duplicate email → `ConflictException` (409, `USER-006`/DI-4); update supports `isActive=false` (disable); delete is hard (204). `UserDto` omits `PasswordHash`. | `dotnet build` ✅. |
| P3-T02 | 3 | `AdminUsersController` (U-1..U-5). | `src/AssignmentManagement.Api/Controllers/AdminUsersController.cs` | `[Authorize(Policy="AdminOnly")]`, route `api/admin/users`. POST→201, GET→200[], GET/{id}→200/404, PUT→200/404/409, DELETE→204/404. | `dotnet build` ✅; Swagger shows 5 user routes. |
| P3-T03 | 3 | Classes DTOs + service + controller (C-1..C-4). | `DTOs/Classes/{CreateClassRequest,UpdateClassRequest,ClassDto}.cs`; `Services/{IClassService,ClassService}.cs`; `Controllers/AdminClassesController.cs` | CRUD on `Classes`; route `api/admin/classes`; DELETE→204. | `dotnet build` ✅; Swagger shows 4 class routes. |
| P3-T04 | 3 | Subjects DTOs + service + controller (S-1..S-4). | `DTOs/Subjects/{CreateSubjectRequest,UpdateSubjectRequest,SubjectDto}.cs`; `Services/{ISubjectService,SubjectService}.cs`; `Controllers/AdminSubjectsController.cs` | `UNIQUE(ClassId,Name)` duplicate → 409; non-existent `classId` → 404 (`CLASS-007`); route `api/admin/subjects`. | `dotnet build` ✅. |
| P3-T05 | 3 | Teacher-assignments DTOs + service + controller (T-1,T-2). | `DTOs/TeacherAssignments/{CreateTeacherAssignmentRequest,TeacherAssignmentDto}.cs`; `Services/{ITeacherAssignmentService,TeacherAssignmentService}.cs`; `Controllers/AdminTeacherAssignmentsController.cs` | `UNIQUE(TeacherId,ClassId,SubjectId)` → 409 (DI-1); non-existent teacher/class/subject, or teacher `Role!=Teacher` → 404 (`CLASS-008`). | `dotnet build` ✅. |
| P3-T06 | 3 | Enrollments DTOs + service + controller (E-1,E-2). | `DTOs/Enrollments/{CreateEnrollmentRequest,EnrollmentDto}.cs`; `Services/{IEnrollmentService,EnrollmentService}.cs`; `Controllers/EnrollmentsController.cs` | `UNIQUE(ClassId,StudentId)` → 409 (DI-2); non-existent class/student, or student `Role!=Student` → 404 (`CLASS-009`). | `dotnet build` ✅. |
| P3-T07 | 3 | Admin read-all assignments + submissions (A-1, SB-1). | `DTOs/Assignments/AssignmentSummaryDto.cs`; `DTOs/Submissions/SubmissionSummaryDto.cs`; `Controllers/{AdminAssignmentsController,AdminSubmissionsController}.cs` | `GET /api/admin/assignments` → all assignments (any status/owner) → `AssignmentSummaryDto[]` (ADM-001/003); `GET /api/admin/submissions` → all → `SubmissionSummaryDto[]` (ADM-002). No owner/status filters. | `dotnet build` ✅; as Admin, GET returns seeded Draft + Published assignments and the seeded submission. |
| P3-T08 | 3 | Admin authorization + user-management tests. | `tests/.../Auth/AuthorizationTests.cs` (TS-AUTH-03, TS-AUTH-04); `tests/.../Services/UserServiceTests.cs` (TS-USER-01, TS-USER-02, TS-USER-03) | Admin→200 on `/api/admin/users`; Teacher/Student→403; CRUD lifecycle 201/200/204/404; bad role→400; duplicate email→409. | `dotnet test --filter "FullyQualifiedName~UserServiceTests|AuthorizationTests"` ✅. |
| P3-T09 | 3 | Class/subject/teacher-assignment/enrollment tests. | `tests/.../Services/{ClassServiceTests,SubjectServiceTests,TeacherAssignmentServiceTests,EnrollmentServiceTests}.cs` (TS-CLASS-01..05) | CRUD + unique-conflict 409 + missing-FK 404 for each area. | `dotnet test --filter "FullyQualifiedName~ClassServiceTests|SubjectServiceTests|TeacherAssignmentServiceTests|EnrollmentServiceTests"` ✅. |
| P3-T10 | 3 | Phase-3 verification gate. | *(review only)* | Build green; all admin tests green; Swagger shows every `/api/admin/*` route. | From `server/`: `dotnet build` ✅; `dotnet test` ✅. Update `docs/VERIFICATION_CHECKLIST.md`. |

---

## 5. Phase 4 — Teacher API (assignments CRUD + publish + submissions review)

> **Phase goal:** `/api/teacher/*` per [`API_CONTRACT.md`](./API_CONTRACT.md) §5. Enforce
> `TeacherClassSubject` on create (BR-3), ownership on edit/delete/publish/review (BR-9),
> `0 ≤ Marks ≤ MaxMarks` (BR-10), UTC deadlines. Covers `ASGN-001…011`, `SUB-008…012`, `BR-2`.

| Task ID | Phase | Goal | Files | Acceptance criteria | Verify |
|---|---|---|---|---|---|
| P4-T01 | 4 | Assignment DTOs + validators. | `DTOs/Assignments/{CreateAssignmentRequest,UpdateAssignmentRequest,AssignmentDto}.cs`; `Validators/CreateAssignmentRequestValidator.cs` | `title`/`description` required; `deadlineUtc` required + **future**; `maxMarks > 0` (`ASGN-011`); `classId`/`subjectId` required. `Status` **not** settable on create (always `Draft`). `Update*` all optional. | `dotnet build` ✅. |
| P4-T02 | 4 | `AssignmentService` create + list (BR-3, ASGN-007). | `src/AssignmentManagement.Application/Services/{IAssignmentService,AssignmentService}.cs` | Create: `TeacherId = currentUser`; verify a `TeacherClassSubject` row exists for `(currentUser, classId, subjectId)` else `ForbiddenException` (BR-3); default `Status=Draft` (ASGN-007); default `AllowResubmission=true`. List returns only `TeacherId==currentUser` (TA-2). | `dotnet build` ✅. |
| P4-T03 | 4 | `AssignmentService` update/delete/publish (BR-9, ASGN-004/005/006). | *(extend `AssignmentService.cs`)* | Update/delete/publish load by id → 404 if missing; if `TeacherId != currentUser` → 403 (TS-ASGN-06); publish sets `Status=Published` (TS-ASGN-09). | `dotnet build` ✅. |
| P4-T04 | 4 | `TeacherAssignmentsController` (TA-1..TA-6). | `src/AssignmentManagement.Api/Controllers/TeacherAssignmentsController.cs` | `[Authorize(Policy="TeacherOnly")]`, route `api/teacher/assignments`. POST→201/403, GET→200[], GET/{id}→200/403/404, PUT→200/403/404, DELETE→204/403/404, POST/{id}/publish→200/403/404. | `dotnet build` ✅; Swagger shows 6 routes. |
| P4-T05 | 4 | Review DTO + validator. | `DTOs/Submissions/ReviewSubmissionRequest.cs`; `Validators/ReviewSubmissionRequestValidator.cs` | `marks` required (validator checks `>=0`; upper bound vs `MaxMarks` enforced in service); `feedback` optional; `status` optional (defaults `Reviewed`). | `dotnet build` ✅. |
| P4-T06 | 4 | `SubmissionService` review (BR-9, BR-10, SUB-009..012). | `src/AssignmentManagement.Application/Services/{ISubmissionService,SubmissionService}.cs` | Load `submission → assignment`; if `assignment.TeacherId != currentUser` → 403 (TS-REV-04); reject `marks < 0` or `> MaxMarks` → 400 (TS-REV-02/03); on success set `Marks`,`Feedback`,`ReviewedByTeacherId=currentUser`,`ReviewedAtUtc=nowUtc`,`Status` (default `Reviewed`, TS-REV-06). | `dotnet build` ✅. |
| P4-T07 | 4 | `TeacherSubmissionsController` (TS-1, TR-1). | `src/AssignmentManagement.Api/Controllers/TeacherSubmissionsController.cs` | `GET /api/teacher/assignments/{assignmentId}/submissions` → own-assignment submissions only (403/404 per ownership); `PUT /api/teacher/submissions/{id}/review` → 200/400/403/404. | `dotnet build` ✅; Swagger shows both routes. |
| P4-T08 | 4 | Teacher tests (assignment + review rules). | `tests/.../Rules/AssignmentRulesTests.cs` (TS-ASGN-01/04/06/09); `tests/.../Rules/ReviewRulesTests.cs` (TS-REV-01/02/03/04/06) | Unassigned create→403; MaxMarks≤0→400; cross-teacher edit/delete→403; publish Draft→Published; own review 200; marks<0/>max→400; non-owner review→403; status transitions Submitted→UnderReview→Reviewed. | `dotnet test --filter "FullyQualifiedName~AssignmentRulesTests|ReviewRulesTests"` ✅. |
| P4-T09 | 4 | Phase-4 verification gate. | *(review only)* | Build green; teacher tests green; Swagger shows `/api/teacher/*`. | From `server/`: `dotnet build` ✅; `dotnet test` ✅. Update `docs/VERIFICATION_CHECKLIST.md`. |

---

## 6. Phase 5 — Student API (view published + submit/update own submissions)

> **Phase goal:** `/api/student/*` per [`API_CONTRACT.md`](./API_CONTRACT.md) §6. Students see only
> `Published` assignments for **enrolled** classes (drafts hidden), submit before deadline (UTC),
> update only if `AllowResubmission`, and view **only their own** submissions. Covers
> `ASGN-008/009`, `SUB-001…007`, `BR-4..8`, `BR-12`.

| Task ID | Phase | Goal | Files | Acceptance criteria | Verify |
|---|---|---|---|---|---|
| P5-T01 | 5 | Student submission DTOs. | `DTOs/Submissions/{SubmitRequest,UpdateSubmissionRequest,SubmissionDto}.cs` | `SubmitRequest`/`UpdateSubmissionRequest` = `{answerText}` (required). `SubmissionDto` per [`API_CONTRACT.md`](./API_CONTRACT.md) §7.8 (marks/feedback null until reviewed). | `dotnet build` ✅. |
| P5-T02 | 5 | Student assignment visibility service (ASGN-008/009, BR-4/5). | *(extend `AssignmentService.cs` or add `StudentAssignmentQuery`)* | List/detail return only `Status=Published` AND classes the student is enrolled in (`Enrollments.ClassId` for `StudentId=currentUser`). Draft/non-enrolled → 404 on detail (TS-ASGN-02/03, TS-SUB-09). | `dotnet build` ✅. |
| P5-T03 | 5 | `StudentAssignmentsController` (SA-1, SA-2). | `src/AssignmentManagement.Api/Controllers/StudentAssignmentsController.cs` | `[Authorize(Policy="StudentOnly")]`, route `api/student/assignments`. GET→200[] (enrolled+published only); GET/{id}→200/404 (draft/invisible→404). | `dotnet build` ✅; as seeded student, list shows the Published assignment, not the Draft. |
| P5-T04 | 5 | `SubmissionService` submit (SUB-001/004, BR-6, DI-3). | *(extend `SubmissionService.cs`)* | Assignment must exist, be `Published`, student enrolled, and `nowUtc ≤ DeadlineUtc` (BR-6/BR-12) else 400 (after deadline)/403 (not enrolled). One submission per `(assignment,student)`: if exists + `AllowResubmission` → upsert (return updated); else → 409 (TS-SUB-08). `StudentId=currentUser`, `Status=Submitted`. | `dotnet build` ✅. |
| P5-T05 | 5 | `SubmissionService` update own submission (SUB-003, BR-7). | *(extend `SubmissionService.cs`)* | PUT updates `AnswerText` + `UpdatedAtUtc`; blocked if `nowUtc > DeadlineUtc` (400) or `AllowResubmission==false` (403) (TS-SUB-03/04/10); ownership `StudentId==currentUser` else 403/404 (TS-SUB-06, BR-8). | `dotnet build` ✅. |
| P5-T06 | 5 | `StudentSubmissionsController` (SA-3, SA-4, SA-5, SA-6). | `src/AssignmentManagement.Api/Controllers/StudentSubmissionsController.cs` | `POST /api/student/assignments/{assignmentId}/submit`→201/400/403/404/409; `PUT /api/student/submissions/{id}`→200/400/403/404; `GET .../submissions`→200[] (own only); `GET .../{id}`→200/403/404. | `dotnet build` ✅; Swagger shows the 4 routes. |
| P5-T07 | 5 | Student tests (submission + visibility rules). | `tests/.../Rules/SubmissionRulesTests.cs` (TS-SUB-01..06, TS-SUB-08, TS-SUB-09/10, TS-ASGN-02/03/08) | Submit before deadline 201; after deadline 400; update before deadline+allowed 200; update after deadline 400; cannot submit to draft 404; cross-student view 403; duplicate→409; not-enrolled→404/403; update blocked when `AllowResubmission=false`. | `dotnet test --filter "FullyQualifiedName~SubmissionRulesTests"` ✅. |
| P5-T08 | 5 | Phase-5 verification gate. | *(review only)* | Build green; student tests green; Swagger shows `/api/student/*`. | From `server/`: `dotnet build` ✅; `dotnet test` ✅. Update `docs/VERIFICATION_CHECKLIST.md`. |

---

## 7. Phase 6 — Frontend scaffold (Next.js, API client, auth, middleware, dashboard shells)

> **Phase goal:** a building Next.js 14 App Router + TS + Tailwind app with the API client, token
> handling, role-based middleware protection, login page, and role dashboard shells with
> loading/error/empty states. See [`PROJECT_STRUCTURE.md`](./PROJECT_STRUCTURE.md) §3.

| Task ID | Phase | Goal | Files | Acceptance criteria | Verify |
|---|---|---|---|---|---|
| P6-T01 | 6 | Scaffold Next.js 14 App Router + TS + deps. | `client/package.json`; `client/tsconfig.json` (`@/* → src/*`); `client/package-lock.json` | Deps: `next@14`, `react`, `react-dom`, `typescript`, `tailwindcss`, `axios` (or native fetch) + `zod`. Strict TS. | From `client/`: `npm install` ✅; `npx tsc --noEmit` ✅. |
| P6-T02 | 6 | Config files + global styles. | `client/next.config.mjs`; `tailwind.config.ts`; `postcss.config.js`; `client/.env.example` (`NEXT_PUBLIC_API_URL=http://localhost:5000`); `src/app/globals.css` | Tailwind directives present; `NEXT_PUBLIC_API_URL` wired; `client/.env.local` git-ignored (never committed). | `npm run build` ✅ (empty app builds). |
| P6-T03 | 6 | `lib` core: API client, types, constants, utils. | `src/lib/api/client.ts`; `src/lib/types.ts` (mirrors DTOs: `AuthResponse`,`UserDto`,`AssignmentDto`,`SubmissionDto`,…); `src/lib/constants.ts` (routes, role labels, status labels); `src/lib/utils.ts` (UTC date format, classNames) | Client sends `Authorization: Bearer <token>`; on 401 clears token + redirects to `/login`. JSON camelCase. | `npm run build` ✅. |
| P6-T04 | 6 | Typed API endpoints + token/session storage. | `src/lib/api/endpoints.ts` (auth/admin/teacher/student function groups); `src/lib/auth/token.ts` (get/set/clear); `src/lib/auth/session.ts` (decode user/role) | Every endpoint in [`API_CONTRACT.md`](./API_CONTRACT.md) §3–§6 has a typed caller. Token stored (recommended httpOnly cookie / acceptable `localStorage`). | `npm run build` ✅. |
| P6-T05 | 6 | React hooks (auth, current user, data fetch). | `src/hooks/{useAuth,useCurrentUser,useApi}.ts` | `useAuth` (login/logout, token state), `useCurrentUser` (GET `/api/auth/me`), `useApi` (loading/error/data state). | `npm run build` ✅. |
| P6-T06 | 6 | Edge middleware: route protection + role redirect. | `client/src/middleware.ts` | Unauthenticated → `/login`; authenticated user hitting a non-own-role URL (e.g. Student→`/admin/*`) → redirect to own dashboard; `/login` when already authed → role dashboard. (`AUTH_MODEL.md` §9.1 — backend is authority, this is first-line UX.) | `npm run build` ✅; manual: no-token `/admin/users` → `/login`. |
| P6-T07 | 6 | UI primitives + state components. | `src/components/ui/{Button,Input,Card,Table,Badge,Spinner,EmptyState,ErrorState}.tsx` | Reusable presentational components; `Badge` renders status chips (Draft/Published/Submitted/Reviewed). | `npm run build` ✅. |
| P6-T08 | 6 | App chrome + role shell + root layout/states. | `src/components/layout/{RoleShell,Sidebar,Topbar}.tsx`; `src/components/guards/RoleGuard.tsx`; `src/app/{layout,page,error,not-found,loading}.tsx` | Root entry redirects by role (or to `/login`); global error/loading/not-found present; Sidebar is role-aware. | `npm run build` ✅. |
| P6-T09 | 6 | Login page + form + role redirect. | `src/components/forms/LoginForm.tsx`; `src/app/(auth)/login/page.tsx` | Validates email/password (zod); shows field + API errors; on success stores token + redirects to role dashboard. | `npm run build` ✅; manual login (backend up) as admin→`/admin/dashboard`, teacher→`/teacher/dashboard`, student→`/student/dashboard`. |
| P6-T10 | 6 | Role dashboard shells. | `src/app/{admin,teacher,student}/dashboard/page.tsx` | Each dashboard renders the RoleShell + a placeholder summary; guards enforced. | `npm run build` ✅; manual: each role reaches its own dashboard. |
| P6-T11 | 6 | Phase-6 verification gate. | *(review only)* | App builds; login redirects per role; protected routes redirect to `/login`. | From `client/`: `npm install` ✅; `npm run build` ✅; manual login redirect per role. Update `docs/VERIFICATION_CHECKLIST.md`. |

---

## 8. Phase 7 — Frontend role pages (forms, validation, error display)

> **Phase goal:** implement every admin/teacher/student page from [`PROJECT_STRUCTURE.md`](./PROJECT_STRUCTURE.md) §3
> and [`PRD.md`](./PRD.md) §8, wired to the API client, with validation + loading/error/empty states.

| Task ID | Phase | Goal | Files | Acceptance criteria | Verify |
|---|---|---|---|---|---|
| P7-T01 | 7 | Admin users page (list + create/edit/disable/delete). | `src/app/admin/users/page.tsx` (+ inline forms/modal) | List from `GET /api/admin/users`; create form validates (name/email/role), shows 409 on duplicate email; disable via `isActive=false`; delete → 204; loading/error/empty states. | `npm run build` ✅; manual CRUD reflects in DB. |
| P7-T02 | 7 | Admin classes + subjects pages. | `src/app/admin/classes/page.tsx`; `src/app/admin/subjects/page.tsx` | CRUD wired (`/api/admin/classes`, `/api/admin/subjects`); subject create enforces class selection; 409 on duplicate `(classId,name)`. | `npm run build` ✅. |
| P7-T03 | 7 | Admin teacher-assignments + enrollments pages. | `src/app/admin/teacher-assignments/page.tsx`; `src/app/admin/enrollments/page.tsx` | Assign teacher→(class,subject) and enroll student→class via dropdowns; 409/404 surfaced. | `npm run build` ✅. |
| P7-T04 | 7 | Admin read-only assignments + submissions pages. | `src/app/admin/assignments/page.tsx`; `src/app/admin/submissions/page.tsx` | Lists all (`GET /api/admin/assignments|submissions`); read-only; status badges; empty states. | `npm run build` ✅. |
| P7-T05 | 7 | Teacher assignments list + new (AssignmentForm). | `src/components/forms/AssignmentForm.tsx`; `src/app/teacher/assignments/{page,new/page}.tsx` | New form validates (title/desc/future deadline/maxMarks>0/class/subject); class/subject limited to teacher's assignments; 403 surfaced; created as Draft. | `npm run build` ✅. |
| P7-T06 | 7 | Teacher assignment edit + publish actions. | `src/app/teacher/assignments/[id]/edit/page.tsx` (+ list actions) | Edit own assignment; publish button → `POST /{id}/publish`; 403/404 surfaced. | `npm run build` ✅. |
| P7-T07 | 7 | Teacher submissions list + review screen. | `src/app/teacher/assignments/[id]/submissions/page.tsx`; `src/app/teacher/submissions/[id]/page.tsx` | List submissions for own assignment; review form (`marks` ≤ MaxMarks, feedback, status) → `PUT /review`; 400/403 surfaced. | `npm run build` ✅; manual review updates marks/feedback/status. |
| P7-T08 | 7 | Student assignments list + detail/submit. | `src/app/student/assignments/{page,[id]/page}.tsx` | List shows enrolled Published only; detail shows deadline (UTC); submit form posts answer; after-deadline / draft errors surfaced. | `npm run build` ✅; manual submit creates a submission. |
| P7-T09 | 7 | Student submissions list + detail. | `src/app/student/submissions/{page,[id]/page}.tsx` | Lists only own submissions; detail shows status + (if Reviewed) marks/feedback; cannot view others' (403/404 handled). | `npm run build` ✅. |
| P7-T10 | 7 | Phase-7 verification gate. | *(review only)* | App builds; all role workflows work end-to-end against the backend. | From `client/`: `npm run build` ✅; manual admin/teacher/student workflows. Update `docs/VERIFICATION_CHECKLIST.md`. |

---

## 9. Phase 8 — xUnit tests consolidated (rules, authorization, deadline/marks, integration)

> **Phase goal:** a single green test suite that implements every `TS-*` scenario in
> [`BUSINESS_RULES.md`](./BUSINESS_RULES.md) §10, plus optional integration tests. Phase 8 consolidates
> the incremental tests added in phases 2–5 and fills gaps.

| Task ID | Phase | Goal | Files | Acceptance criteria | Verify |
|---|---|---|---|---|---|
| P8-T01 | 8 | Test helpers: DB fixture + data builders. | `tests/.../TestHelpers/TestDb/TestDbFixture.cs` (in-memory or Testcontainers Postgres); `tests/.../TestHelpers/TestData.cs` | Fixture spins a fresh DB (in-memory where provider-compatible, else Testcontainers), runs migrate+seed; `TestData` builds entities/DTOs with deterministic ids. | `dotnet build` ✅; fixture constructs without error. |
| P8-T02 | 8 | User + class/subject/enrollment/teacher-assignment service tests. | `tests/.../Services/{UserServiceTests,ClassServiceTests,SubjectServiceTests,TeacherAssignmentServiceTests,EnrollmentServiceTests}.cs` | Covers TS-USER-01/02/03, TS-CLASS-01..05, DI-1/2/4. | `dotnet test --filter "FullyQualifiedName~UserServiceTests|ClassServiceTests|SubjectServiceTests|TeacherAssignmentServiceTests|EnrollmentServiceTests"` ✅. |
| P8-T03 | 8 | Assignment rules tests. | `tests/.../Rules/AssignmentRulesTests.cs` | Covers TS-ASGN-01/02/03/04/05/06/08/09 (unassigned→403, draft hidden, published-to-enrolled, MaxMarks>0, own edit, cross-teacher 403, UTC deadline, publish transition). | `dotnet test --filter "FullyQualifiedName~AssignmentRulesTests"` ✅. |
| P8-T04 | 8 | Submission rules tests. | `tests/.../Rules/SubmissionRulesTests.cs` | Covers TS-SUB-01..06, TS-SUB-08/09/10 (before/after deadline, draft 404, cross-student 403, duplicate 409, not-enrolled 404/403, AllowResubmission=false update 403). | `dotnet test --filter "FullyQualifiedName~SubmissionRulesTests"` ✅. |
| P8-T05 | 8 | Review rules tests. | `tests/.../Rules/ReviewRulesTests.cs` | Covers TS-REV-01/02/03/04/05/06 (own review 200, marks<0→400, marks>max→400, non-owner 403, optional feedback, status transitions). | `dotnet test --filter "FullyQualifiedName~ReviewRulesTests"` ✅. |
| P8-T06 | 8 | Authorization + admin visibility tests. | `tests/.../Auth/AuthorizationTests.cs` (TS-AUTH-03/04/05, TS-ADM-04); `tests/.../Rules/AdminVisibilityTests.cs` (TS-ADM-01/02/03) | Role→route matrix enforced server-side; admin sees all assignments/submissions, does not create/grade (403). | `dotnet test --filter "FullyQualifiedName~AuthorizationTests|AdminVisibilityTests"` ✅. |
| P8-T07 | 8 | Cross-cutting tests (UTC + password-hash secrecy + JWT role). | `tests/.../Rules/CrossCuttingTests.cs` | TS-CROSS-01 (timezone-stable UTC deadline), TS-CROSS-02 (`passwordHash` absent from login/me/users responses), TS-CROSS-03 (JWT carries `role`). | `dotnet test --filter "FullyQualifiedName~CrossCuttingTests"` ✅. |
| P8-T08 | 8 | Optional integration tests (HTTP pipeline). | `tests/.../Integration/*Tests.cs` (using `WebApplicationFactory` + Testcontainers) | Login → call each protected route → assert 200/403/401 end-to-end through real middleware (not just services). | `dotnet test --filter "FullyQualifiedName~Integration"` ✅ (or skipped-with-reason if Testcontainers unavailable). |
| P8-T09 | 8 | Coverage review + `TS-*` traceability matrix. | `tests/.../TestHelpers/ScenarioCoverage.cs` (or doc) | Every `TS-*` in [`BUSINESS_RULES.md`](./BUSINESS_RULES.md) §10 maps to ≥1 passing test; no PRD §13 requirement uncovered. | `dotnet test` ✅ all green; matrix shows 100% of cited `TS-*`. |
| P8-T10 | 8 | Phase-8 verification gate. | *(review only)* | Whole solution test suite green; high rule coverage. | From `server/`: `dotnet build` ✅; `dotnet test` ✅ all green. Update `docs/VERIFICATION_CHECKLIST.md`. |

---

## 10. Phase 9 — README, .env.example, final verification, cleanup

> **Phase goal:** the repo is fresh-clone-runnable: README walks a new evaluator from clone to working
> app, `.env.example` templates are present, both apps run, migrations apply from scratch, demo logins
> work, and no secrets are committed. See [`PRD.md`](./PRD.md) §14–§16.

| Task ID | Phase | Goal | Files | Acceptance criteria | Verify |
|---|---|---|---|---|---|
| P9-T01 | 9 | Root `.gitignore` covering both stacks. | `.gitignore` (repo root) | Ignores `bin/`, `obj/`, `node_modules/`, `client/.next/`, `client/out/`, `*.user`, `*.suo`, `.env.local`, `appsettings.Local.json`, `*.log`, `.idea/`, `.vs/`. | `git status` shows no build artifacts after a full build. |
| P9-T02 | 9 | Env templates (root + server + client). | `.env.example` (root); `server/src/AssignmentManagement.Api/appsettings.example.json`; `client/.env.example` | Root `.env.example` mirrors [`AUTH_MODEL.md`](./AUTH_MODEL.md) §2.3 (ConnectionStrings, `Jwt__Secret/Issuer/Audience/ExpiryMinutes`, `NEXT_PUBLIC_API_URL`). Placeholders only — no real secrets. | Diff shows no real passwords/secrets. |
| P9-T03 | 9 | README overview + features + stack + structure. | `README.md` | Overview, main features, technology stack, and project structure sections (PRD §14.4). Structure matches [`PROJECT_STRUCTURE.md`](./PROJECT_STRUCTURE.md) §1. | README present and well-formed. |
| P9-T04 | 9 | README setup instructions (DB, backend, frontend, tests). | `README.md` | DB setup (Postgres + `dotnet ef database update`), backend run (`dotnet run`, port 5000), frontend run (`npm install && npm run dev`, port 3000), run tests (`dotnet test`). Evaluator can set up the DB without manual table creation (PRD §14.7). | Fresh-clone run-through (below) succeeds. |
| P9-T05 | 9 | README assumptions, known limitations, demo credentials. | `README.md` | Assumptions (PRD §16), known limitations/out-of-scope (PRD §17), and demo credentials table (admin/teacher/teacher2/student). | Credentials table matches contract exactly. |
| P9-T06 | 9 | Final end-to-end verification from a clean state. | *(run only)* | (1) drop/recreate DB; (2) `dotnet ef database update` recreates schema; (3) startup seeds demo users; (4) `dotnet test` green; (5) `npm run build` green; (6) login works for all three roles; (7) a full create→publish→submit→review loop works. | From `server/`: `dotnet test` ✅; `dotnet run` + `client`: `npm run build && npm start`; manual login + workflow per role. |
| P9-T07 | 9 | Final checklist + cleanup. | `README.md` (checklist); repo-wide scan | PRD §15 final checklist all ticked; no `Jwt__Secret`/real passwords/`.env.local` committed; `git log` clean of secrets. | `git grep -niE "admin@123|Jwt__Secret=change" -- ':!.env.example' ':!*.example.json'` returns nothing sensitive; demo passwords appear **only** in docs/README templates. Update `docs/VERIFICATION_CHECKLIST.md` — project Done. |

---

### Quick reference: phase → task counts

| Phase | Tasks | IDs |
|---|---|---|
| 1 | 12 | P1-T01 … P1-T12 |
| 2 | 11 | P2-T01 … P2-T11 |
| 3 | 10 | P3-T01 … P3-T10 |
| 4 | 9  | P4-T01 … P4-T09 |
| 5 | 8  | P5-T01 … P5-T08 |
| 6 | 11 | P6-T01 … P6-T11 |
| 7 | 10 | P7-T01 … P7-T10 |
| 8 | 10 | P8-T01 … P8-T10 |
| 9 | 7  | P9-T01 … P9-T07 |
| **Total** | **88** | |

> After the final task (P9-T07), update `docs/VERIFICATION_CHECKLIST.md` to mark the project Done per
> [`PRD.md`](./PRD.md) §18 (Definition of Done).
