# PROJECT_STRUCTURE - Assignment & Submission Management System

> **Phase 0 documentation.** This file is the authoritative scaffold blueprint for the monorepo.
> All names below use the **CANONICAL CONTRACT** verbatim (entities, enums, roles, ports, phases).
> The engineer should be able to scaffold the entire repository from this single document.
> Companion doc: [`PRD.md`](./PRD.md) (source of truth for requirements — do not modify).

---

## 1. Monorepo Overview

A single Git repository (`Assignment-Management-System`) containing two applications — a
**.NET 8 backend** and a **Next.js 14 frontend** — plus documentation and scratch plans. The
two apps communicate only over the REST API (HTTP/JWT); they share no code.

```
Assignment-Management-System/        # repo root (single git repo, two apps)
├── .gitignore                        # ignores: bin/, obj/, node_modules/, .env.local,
│                                     #          client/.next/, server app secrets, etc.
├── README.md                         # (Phase 9) overview, setup, demo creds, assumptions
├── .env.example                      # root-level copy of required env vars (see Section 6)
│
├── docs/                             # ← SOURCE OF TRUTH for design (these docs)
│   ├── PRD.md                        # authoritative requirements — DO NOT MODIFY
│   └── PROJECT_STRUCTURE.md          # ← this file (scaffold blueprint)
│
├── plans/                            # scratch / planning notes (not shipped, git-ignored or local)
│   └── (scratch notes, TODO dumps, phase task lists)
│
├── server/                           # .NET solution (ASP.NET Core 8 + EF Core 8 + Npgsql)
│   └── AssignmentManagement.sln      # see Section 2
│
└── client/                           # Next.js 14 App Router (React + TS + Tailwind)
    └── package.json                  # see Section 3
```

**Key points**

- **One repo, two apps.** `server/` and `client/` are independent build units; they are versioned
  together for ease of review and local setup.
- **`docs/` is the source of truth.** Any architecture decision lives here before code is written.
- **`plans/`** is a scratch area for working notes, not part of the deliverable; it may be added to
  `.gitignore` or kept as lightweight markdown.
- **`.gitignore` essentials:** `bin/`, `obj/`, `node_modules/`, `client/.next/`,
  `client/out/`, `*.user`, `.env.local`, `*.log`, `appsettings.Local.json`,
  IDE folders (`.idea/`, `.vs/`).
- **No secrets committed.** Real passwords, JWT secrets, and connection strings with real passwords
  never enter the repo; only `.env.example` / `appsettings.example.json` templates ship.

---

## 2. Backend Solution Tree (`.NET 8 / C#`)

Layered architecture. Dependency direction is strict:

```
Api ──► Application ──► Domain
  │
  └────► Infrastructure ──► Application ──► Domain
```

- `Domain` has **zero** project references (pure entities, enums, exceptions).
- `Application` references **only** `Domain`.
- `Infrastructure` references `Application` and `Domain`.
- `Api` references `Application`, `Domain`, and `Infrastructure` (only to wire DI in `Program.cs`).

```
server/
├── AssignmentManagement.sln                    # solution file (binds all projects below)
├── Directory.Build.props                       # shared MSBuild props: TargetFramework=net8.0,
│                                               #   Nullable=enable, ImplicitUsings=enable, LangVersion=latest
├── Directory.Packages.props                    # central package management (CPM) — all NuGet versions pinned here
│
├── src/
│   │
│   ├── AssignmentManagement.Api/               # PRESENTATION LAYER — ASP.NET Core host
│   │   │                                       # Responsibility: HTTP entry point. Controllers, middleware,
│   │   │                                       #   filters, ProblemDetails shaping, Swagger, DI wiring.
│   │   │                                       #   Depends on: Application, Infrastructure (DI only), Domain.
│   │   ├── AssignmentManagement.Api.csproj
│   │   ├── Program.cs                          # host build: services, JWT, Swagger, DI registration, pipeline
│   │   ├── appsettings.json                    # base config (logging, JWT issuer/audience/expiry placeholders)
│   │   ├── appsettings.Development.json        # dev overrides (connection string, JWT secret)
│   │   ├── appsettings.example.json            # template committed; real secrets stay local
│   │   │
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs               # POST /api/auth/login, GET /api/auth/me
│   │   │   ├── AdminUsersController.cs         # CRUD /api/admin/users
│   │   │   ├── AdminClassesController.cs       # CRUD /api/admin/classes
│   │   │   ├── AdminSubjectsController.cs      # CRUD /api/admin/subjects
│   │   │   ├── AdminTeacherAssignmentsController.cs # /api/admin/teacher-assignments
│   │   │   ├── EnrollmentsController.cs        # /api/admin/enrollments
│   │   │   ├── AdminAssignmentsController.cs   # GET /api/admin/assignments (read-only, all)
│   │   │   ├── AdminSubmissionsController.cs   # GET /api/admin/submissions (read-only, all)
│   │   │   ├── TeacherAssignmentsController.cs # /api/teacher/assignments (+ publish)
│   │   │   ├── TeacherSubmissionsController.cs # /api/teacher/assignments/{id}/submissions,
│   │   │   │                                   #   PUT /api/teacher/submissions/{id}/review
│   │   │   ├── StudentAssignmentsController.cs # GET /api/student/assignments, /{id}, POST .../submit
│   │   │   └── StudentSubmissionsController.cs # /api/student/submissions (+ PUT update)
│   │   │
│   │   ├── Middleware/
│   │   │   └── ExceptionMiddleware.cs          # global try/catch → ProblemDetails + logging
│   │   ├── Filters/                            # (e.g., ValidationFilter, result-wrapping filters)
│   │   ├── Extensions/                         # IServiceCollection / WebApplication extension methods
│   │   │   ├── ServiceCollectionExtensions.cs  # AddApplication, AddInfrastructure
│   │   │   ├── JwtExtensions.cs                # AddJwtBearer config
│   │   │   └── SwaggerExtensions.cs            # Swagger + JWT security definition
│   │   └── ProblemDetails/                     # ProblemDetailsFactory, error response helpers
│   │
│   ├── AssignmentManagement.Application/       # APPLICATION LAYER — use-cases / business orchestration
│   │   │                                       # Responsibility: DTOs, services, validation, mapping,
│   │   │                                       #   abstractions. No HTTP, no EF. Depends on: Domain only.
│   │   ├── AssignmentManagement.Application.csproj
│   │   │
│   │   ├── DTOs/                               # grouped by feature; suffix convention: …Request / …Response / …Dto
│   │   │   ├── Auth/
│   │   │   │   ├── LoginRequest.cs
│   │   │   │   ├── LoginResponse.cs            # { token, user: UserSummaryDto }
│   │   │   │   └── MeResponse.cs
│   │   │   ├── Users/
│   │   │   │   ├── CreateUserRequest.cs
│   │   │   │   ├── UpdateUserRequest.cs
│   │   │   │   ├── UserDto.cs                  # UserSummaryDto (no PasswordHash)
│   │   │   │   └── UserListDto.cs
│   │   │   ├── Classes/
│   │   │   │   ├── CreateClassRequest.cs
│   │   │   │   ├── UpdateClassRequest.cs
│   │   │   │   └── ClassDto.cs
│   │   │   ├── Subjects/
│   │   │   │   ├── CreateSubjectRequest.cs
│   │   │   │   ├── UpdateSubjectRequest.cs
│   │   │   │   └── SubjectDto.cs
│   │   │   ├── TeacherAssignments/
│   │   │   │   ├── AssignTeacherRequest.cs    # TeacherId, ClassId, SubjectId
│   │   │   │   └── TeacherAssignmentDto.cs
│   │   │   ├── Enrollments/
│   │   │   │   ├── CreateEnrollmentRequest.cs  # ClassId, StudentId
│   │   │   │   └── EnrollmentDto.cs
│   │   │   ├── Assignments/
│   │   │   │   ├── CreateAssignmentRequest.cs
│   │   │   │   ├── UpdateAssignmentRequest.cs
│   │   │   │   ├── PublishAssignmentRequest.cs
│   │   │   │   └── AssignmentDto.cs
│   │   │   └── Submissions/
│   │   │       ├── CreateSubmissionRequest.cs
│   │   │       ├── UpdateSubmissionRequest.cs
│   │   │       ├── ReviewSubmissionRequest.cs  # { marks, feedback }
│   │   │       └── SubmissionDto.cs
│   │   │
│   │   ├── Services/
│   │   │   ├── IAuthService.cs        + AuthService.cs       # login, token issuance
│   │   │   ├── IUserService.cs        + UserService.cs       # admin user CRUD
│   │   │   ├── IClassService.cs       + ClassService.cs
│   │   │   ├── ISubjectService.cs     + SubjectService.cs
│   │   │   ├── ITeacherAssignmentService.cs + TeacherAssignmentService.cs
│   │   │   ├── IEnrollmentService.cs  + EnrollmentService.cs
│   │   │   ├── IAssignmentService.cs  + AssignmentService.cs # create/update/publish/delete + rules
│   │   │   └── ISubmissionService.cs  + SubmissionService.cs # submit/update/review + rules
│   │   │
│   │   ├── Validators/                         # FluentValidation validators per request
│   │   │   ├── LoginRequestValidator.cs
│   │   │   ├── CreateUserRequestValidator.cs
│   │   │   ├── CreateAssignmentRequestValidator.cs
│   │   │   ├── CreateSubmissionRequestValidator.cs
│   │   │   └── ReviewSubmissionRequestValidator.cs
│   │   │
│   │   ├── Mapping/                            # entity ↔ DTO mapping helpers / AutoMapper profiles
│   │   │   └── MappingProfile.cs
│   │   │
│   │   └── Common/
│   │       ├── Interfaces/
│   │       │   └── ICurrentUserService.cs      # exposes UserId, Role, Email from claims
│   │       └── Result.cs                       # optional result/error wrapper
│   │
│   ├── AssignmentManagement.Domain/            # DOMAIN LAYER — pure model, no dependencies
│   │   │                                       # Responsibility: entities, enums, domain exceptions,
│   │   │                                       #   constants. No infra, no frameworks. Referenced by all.
│   │   ├── AssignmentManagement.Domain.csproj
│   │   │
│   │   ├── Entities/
│   │   │   ├── User.cs                         # Id, Name, Email, PasswordHash, Role, CreatedAt
│   │   │   ├── Class.cs                        # Id, Name, Description, CreatedAt
│   │   │   ├── Subject.cs                      # Id, Name, ClassId, CreatedAt
│   │   │   ├── TeacherClassSubject.cs          # Id, TeacherId, ClassId, SubjectId
│   │   │   ├── Enrollment.cs                   # Id, ClassId, StudentId
│   │   │   ├── Assignment.cs                   # Id, Title, Description, DeadlineUtc, MaxMarks,
│   │   │   │                                   #   Status, TeacherId, ClassId, SubjectId, CreatedAt, UpdatedAt
│   │   │   └── Submission.cs                   # Id, AssignmentId, StudentId, AnswerText, SubmittedAtUtc,
│   │   │                                       #   UpdatedAtUtc, Status, Marks, Feedback,
│   │   │                                       #   ReviewedByTeacherId, ReviewedAtUtc
│   │   │
│   │   ├── Enums/
│   │   │   ├── UserRole.cs                     # Admin, Teacher, Student
│   │   │   ├── AssignmentStatus.cs             # Draft, Published, Archived
│   │   │   └── SubmissionStatus.cs             # Submitted, UnderReview, Reviewed, LateSubmitted
│   │   │
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs              # base domain exception → 400
│   │   │   ├── NotFoundException.cs            # → 404
│   │   │   ├── ConflictException.cs            # → 409 (e.g., duplicate email)
│   │   │   └── ForbiddenException.cs           # → 403
│   │   │
│   │   └── Constants/
│   │       └── DomainRules.cs                  # e.g., MinMaxMarks, deadline semantics
│   │
│   └── AssignmentManagement.Infrastructure/    # INFRASTRUCTURE LAYER — persistence + identity
│       │                                       # Responsibility: EF Core DbContext, configurations,
│       │                                       #   repositories, JWT + BCrypt, seeding, migrations.
│       │                                       #   Depends on: Application, Domain.
│       ├── AssignmentManagement.Infrastructure.csproj
│       │
│       ├── Data/
│       │   ├── AppDbContext.cs                 # DbSet<T> per entity; model config via OnModelCreating
│       │   └── IAppDbContext.cs                # abstraction for tests (save/sets)
│       │
│       ├── Configurations/                     # IEntityTypeConfiguration<T> per entity
│       │   ├── UserConfiguration.cs
│       │   ├── ClassConfiguration.cs
│       │   ├── SubjectConfiguration.cs
│       │   ├── TeacherClassSubjectConfiguration.cs
│       │   ├── EnrollmentConfiguration.cs
│       │   ├── AssignmentConfiguration.cs
│       │   └── SubmissionConfiguration.cs
│       │
│       ├── Repositories/                       # (optional) repository implementations / generic repo
│       │   └── (Repository classes if used)
│       │
│       ├── Identity/
│       │   ├── JwtTokenService.cs              # IJwtTokenService impl: token includes role claim
│       │   ├── IPasswordHasher.cs              # abstraction
│       │   └── PasswordHasher.cs               # BCrypt implementation
│       │
│       ├── Seeding/
│       │   └── DbSeeder.cs                     # seeds demo users (see DEMO USERS) + sample data
│       │
│       └── Migrations/                         # EF Core migration files (Phase 1)
│           ├── <timestamp>_InitialCreate.cs
│           └── <timestamp>_InitialCreate.Designer.cs
│
└── tests/
    └── AssignmentManagement.UnitTests/         # xUnit test project
        ├── AssignmentManagement.UnitTests.csproj
        ├── Services/                           # service-layer unit tests
        │   ├── UserServiceTests.cs
        │   ├── AssignmentServiceTests.cs
        │   └── SubmissionServiceTests.cs
        ├── Auth/                               # authentication + authorization tests
        │   ├── AuthServiceTests.cs             # valid login → JWT; invalid login → 401
        │   └── AuthorizationTests.cs           # Admin/Teacher/Student endpoint access matrix
        ├── Rules/                              # business-rule tests (PRD §13)
        │   ├── AssignmentRulesTests.cs         # unassigned class/subject, draft hidden, published visible, MaxMarks>0
        │   ├── SubmissionRulesTests.cs         # submit before/after deadline, update, draft, cross-student
        │   └── ReviewRulesTests.cs             # own-assignment review, marks in [0,MaxMarks], admin sees all
        └── TestHelpers/
            ├── TestDb/                         # In-Memory or Testcontainers PostgreSQL fixture
            │   └── TestDbFixture.cs
            └── TestData.cs                     # builder helpers for entities/DTOs
```

### Project responsibilities (recap)

| Project | Responsibility | References |
|---|---|---|
| `AssignmentManagement.Api` | HTTP host, controllers, middleware, Swagger, DI wiring | Application, Infrastructure, Domain |
| `AssignmentManagement.Application` | DTOs, services, validators, mapping, abstractions | Domain |
| `AssignmentManagement.Domain` | Entities, enums, exceptions, constants (pure) | (none) |
| `AssignmentManagement.Infrastructure` | EF Core `AppDbContext`, configurations, identity (JWT/BCrypt), seeding, migrations | Application, Domain |
| `AssignmentManagement.UnitTests` | xUnit tests for services, auth, business rules | Application, Domain (+ infra for fixtures) |

---

## 3. Frontend Tree (`Next.js 14 App Router`)

```
client/
├── package.json                              # deps: next@14, react, react-dom, typescript,
│                                             #   tailwindcss, axios/zod, etc.
├── next.config.mjs                           # rewrites/proxy to API if needed
├── tsconfig.json                             # strict TS, path alias @/* → src/*
├── tailwind.config.ts                        # theme, role color tokens
├── postcss.config.js                         # tailwind + autoprefixer
├── .env.example                              # NEXT_PUBLIC_API_URL=http://localhost:5000
├── .env.local                                # LOCAL secrets — git-ignored (never committed)
│
└── src/
    │
    ├── app/                                  # App Router — routing is folder-based, kebab-case URLs
    │   ├── layout.tsx                        # root layout: providers, fonts, globals
    │   ├── page.tsx                          # entry: redirect by role (admin/teacher/student) or to /login
    │   ├── globals.css                       # Tailwind directives + base styles
    │   ├── error.tsx                         # global error boundary
    │   ├── not-found.tsx                     # 404 page
    │   ├── loading.tsx                       # route-level loading skeleton
    │   │
    │   ├── (auth)/
    │   │   └── login/
    │   │       └── page.tsx                  # /login — public login page (route group `(auth)`)
    │   │
    │   ├── admin/                            # Admin role pages (guarded: role=Admin)
    │   │   ├── dashboard/page.tsx            # /admin/dashboard
    │   │   ├── users/page.tsx                # /admin/users
    │   │   ├── classes/page.tsx              # /admin/classes
    │   │   ├── subjects/page.tsx             # /admin/subjects
    │   │   ├── teacher-assignments/page.tsx  # /admin/teacher-assignments
    │   │   ├── enrollments/page.tsx          # /admin/enrollments
    │   │   ├── assignments/page.tsx          # /admin/assignments (read-only, all)
    │   │   └── submissions/page.tsx          # /admin/submissions (read-only, all)
    │   │
    │   ├── teacher/                          # Teacher role pages (guarded: role=Teacher)
    │   │   ├── dashboard/page.tsx            # /teacher/dashboard
    │   │   ├── assignments/
    │   │   │   ├── page.tsx                  # /teacher/assignments
    │   │   │   ├── new/page.tsx              # /teacher/assignments/new
    │   │   │   └── [id]/
    │   │   │       ├── edit/page.tsx         # /teacher/assignments/[id]/edit
    │   │   │       └── submissions/page.tsx  # /teacher/assignments/[id]/submissions
    │   │   └── submissions/
    │   │       └── [id]/page.tsx             # /teacher/submissions/[id]  (review screen)
    │   │
    │   └── student/                          # Student role pages (guarded: role=Student)
    │       ├── dashboard/page.tsx            # /student/dashboard
    │       ├── assignments/
    │       │   ├── page.tsx                  # /student/assignments
    │       │   └── [id]/page.tsx             # /student/assignments/[id] (view + submit)
    │       └── submissions/
    │           ├── page.tsx                  # /student/submissions
    │           └── [id]/page.tsx             # /student/submissions/[id] (status/marks/feedback)
    │
    ├── components/
    │   ├── layout/
    │   │   ├── Sidebar.tsx                   # role-aware nav links
    │   │   ├── Topbar.tsx                    # current user, logout
    │   │   └── RoleShell.tsx                 # wraps a role section (Sidebar+Topbar+content)
    │   ├── ui/                               # presentational primitives
    │   │   ├── Button.tsx
    │   │   ├── Input.tsx
    │   │   ├── Card.tsx
    │   │   ├── Table.tsx
    │   │   ├── Badge.tsx                     # status chips (Draft/Published/Submitted/Reviewed…)
    │   │   ├── Spinner.tsx                   # loading state
    │   │   ├── EmptyState.tsx                # empty data state
    │   │   └── ErrorState.tsx                # API error display
    │   ├── forms/
    │   │   ├── LoginForm.tsx
    │   │   ├── AssignmentForm.tsx            # create/edit assignment (title, desc, deadline, marks, class, subject)
    │   │   └── SubmissionForm.tsx            # submit/update answer
    │   └── guards/
    │       └── RoleGuard.tsx                 # client-side role gate (defense in depth; backend is authority)
    │
    ├── lib/
    │   ├── api/
    │   │   ├── client.ts                     # configured fetch/axios instance (baseURL, JWT header, 401 handling)
    │   │   └── endpoints.ts                  # typed functions per API area (auth, admin, teacher, student)
    │   ├── auth/
    │   │   ├── token.ts                      # token storage/get/set/clear (httpOnly cookie or memory)
    │   │   └── session.ts                    # current user session decode/management
    │   ├── constants.ts                      # API base URL, role names, routes, status labels
    │   ├── types.ts                          # DTO mirrors (LoginResponse, UserDto, AssignmentDto, etc.)
    │   └── utils.ts                          # date/UTC formatting, classNames, formatters
    │
    ├── hooks/
    │   ├── useAuth.ts                        # login/logout, token state
    │   ├── useCurrentUser.ts                 # GET /api/auth/me wrapper
    │   └── useApi.ts                         # generic data-fetch hook (loading/error/data)
    │
    └── middleware.ts                         # Next.js Edge middleware: route protection by role,
                                              #   redirect unauthenticated to /login, block cross-role URLs
```

### Frontend folder responsibilities

| Folder | Responsibility |
|---|---|
| `app/` | App Router pages/layout per route. Route groups `(auth)` keep login outside role shells. Dynamic segments `[id]` map to entity ids. |
| `components/layout/` | App chrome (sidebar/topbar/shell) reused across role sections. |
| `components/ui/` | Stateless, reusable primitives (button, table, badge, states). |
| `components/forms/` | Validated form components wired to API endpoints. |
| `components/guards/` | Client-side role gating (UI convenience only; backend enforces auth). |
| `lib/` | Cross-cutting utilities: API client, endpoints, auth token/session, constants, TS DTO mirrors. |
| `hooks/` | React hooks encapsulating auth, current user, and data fetching. |
| `middleware.ts` | Edge middleware for first-line route protection + role-based redirects. |

---

## 4. Naming Conventions

| Scope | Convention | Examples |
|---|---|---|
| **C# types** (classes, records, enums, interfaces) | PascalCase | `UserService`, `IAuthService`, `UserRole`, `AssignmentStatus` |
| **C# interfaces** | `I` prefix + PascalCase | `IUserService`, `IAppDbContext`, `ICurrentUserService` |
| **C# methods, properties, fields** | PascalCase (public), camelCase (private locals) | `CreateAsync`, `DeadlineUtc`, `userId` |
| **C# files** | PascalCase, one type per file, filename matches type | `AssignmentController.cs`, `User.cs` |
| **TypeScript types/interfaces** | PascalCase | `LoginResponse`, `AssignmentDto` |
| **TS variables, functions, hooks** | camelCase; hooks prefixed `use` | `fetchAssignments`, `useAuth` |
| **TS/React components** | PascalCase, filename matches component | `RoleShell.tsx`, `AssignmentForm.tsx` |
| **URLs / routes** | kebab-case (default), dynamic in `[brackets]` | `/admin/teacher-assignments`, `/student/assignments/[id]` |
| **DTO suffix convention** | `…Request` (input), `…Response` / `…Dto` (output) | `CreateAssignmentRequest`, `AssignmentDto`, `LoginResponse` |
| **Controller route attributes** | lowercase, plural resource, role-prefixed areas | `[Route("api/admin/users")]`, `[Route("api/teacher/assignments")]`, `[Route("api/student/submissions")]` |
| **DB tables / EF entities** | PascalCase entity → snake_case table (via configuration) | `User` → table `users`; `TeacherClassSubject` → `teacher_class_subjects` |
| **Migrations** | `<timestamp>_<Name>.cs` via EF CLI | `20260807_InitialCreate.cs` |

---

## 5. Build & Run Entry Points

### Backend (run from `server/`)

| Action | Command |
|---|---|
| Restore + build solution | `dotnet build` |
| Run API (default profile) | `dotnet run --project src/AssignmentManagement.Api` |
| Add initial migration | `dotnet ef migrations add InitialCreate -p src/AssignmentManagement.Infrastructure -s src/AssignmentManagement.Api` |
| Apply migrations to DB | `dotnet ef database update -p src/AssignmentManagement.Infrastructure -s src/AssignmentManagement.Api` |
| Run unit tests | `dotnet test` |

- **API port:** `5000` (e.g., `http://localhost:5000`, Swagger at `/swagger`).
- PostgreSQL must be running locally on `5432`; connection string in `appsettings.Development.json`
  (overridable via env var `ConnectionStrings__DefaultConnection`).

### Frontend (run from `client/`)

| Action | Command |
|---|---|
| Install dependencies | `npm install` |
| Dev server | `npm run dev` |
| Production build | `npm run build` |
| Start production server | `npm start` |

- **Client port:** `3000` (`http://localhost:3000`).
- Frontend calls backend at `NEXT_PUBLIC_API_URL` (default `http://localhost:5000`).

### Tests

| Scope | Command (from `server/`) |
|---|---|
| All unit tests | `dotnet test` |
| Specific test class | `dotnet test --filter "FullyQualifiedName~SubmissionRulesTests"` |

### Port summary

| Service | Port |
|---|---|
| Backend API (`AssignmentManagement.Api`) | `5000` |
| Frontend (`client`) | `3000` |
| PostgreSQL (local) | `5432` |

---

## 6. Config Files List

### Backend (`server/`)

- `appsettings.json` — base logging + JWT placeholders (issuer, audience, expiry minutes).
- `appsettings.Development.json` — dev overrides: connection string, JWT secret, Swagger enabled.
- `appsettings.example.json` — committed template (no real secrets).
- Env overrides (PRD §14.6):
  - `ASPNETCORE_ENVIRONMENT=Development`
  - `ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=assignment_management;Username=postgres;Password=postgres`
  - `Jwt__Secret=change-this-long-development-secret`
  - `Jwt__Issuer=assignment-management-api`
  - `Jwt__Audience=assignment-management-client`
  - `Jwt__ExpiryMinutes=120`

### Frontend (`client/`)

- `.env.example` — `NEXT_PUBLIC_API_URL=http://localhost:5000`.
- `.env.local` — local secrets; **git-ignored, never committed.**

### Root

- `.gitignore` — must ignore:
  - `bin/`, `obj/` (.NET build output)
  - `node_modules/`, `client/.next/`, `client/out/`
  - `.env.local`, `*.user`, `*.suo`, IDE folders (`.idea/`, `.vs/`)
  - `appsettings.Local.json`, `*.log`

### `.env.example` (root template — Phase 9 deliverable)

```env
# Backend
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=assignment_management;Username=postgres;Password=postgres
Jwt__Secret=change-this-long-development-secret
Jwt__Issuer=assignment-management-api
Jwt__Audience=assignment-management-client
Jwt__ExpiryMinutes=120

# Frontend
NEXT_PUBLIC_API_URL=http://localhost:5000
```

---

### Reference: Canonical Contract (verbatim)

| Item | Value |
|---|---|
| **PROJECT** | Assignment & Submission Management System. Repo: `client/`, `server/`, `docs/`, `plans/`. |
| **STACK** | Backend ASP.NET Core 8 + C#; EF Core 8 + Npgsql; PostgreSQL; BCrypt; JwtBearer; xUnit. Layered: Api→Application→Domain; Infrastructure→Application/Domain; Api references Infrastructure for DI. Frontend Next.js 14 App Router + React + TypeScript + TailwindCSS. |
| **ENUMS** | `UserRole{Admin,Teacher,Student}`; `AssignmentStatus{Draft,Published,Archived}`; `SubmissionStatus{Submitted,UnderReview,Reviewed,LateSubmitted}`. |
| **ENTITIES** | `User`, `Class`, `Subject`, `TeacherClassSubject`, `Enrollment`, `Assignment`, `Submission`. |
| **DEMO USERS** | `admin@example.com` / `admin@123`; `teacher@example.com` / `teacher@123`; `teacher2@example.com` / `teacher@123`; `student@example.com` / `student@123`. |
| **PORTS** | API `5000`, client `3000`. |
| **PHASES** | 0 Docs | 1 Server scaffold+entities+DbContext+migrations+seed | 2 Auth | 3 Admin API | 4 Teacher API | 5 Student API | 6 FE scaffold+auth+dashboards | 7 FE role pages | 8 xUnit tests | 9 README/.env.example/final. |
