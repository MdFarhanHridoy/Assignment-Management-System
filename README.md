# Assignment & Submission Management System

A role-based full-stack web application for managing school/college assignments and submissions. Built with **ASP.NET Core 8** (backend) and **Next.js 14** (frontend), backed by **PostgreSQL**.

---

## Overview

The system supports three user roles — **Admin**, **Teacher**, and **Student** — each with distinct capabilities:

- **Admin** manages users, classes/courses, subjects, teacher assignments, and enrollments. Admin can view all assignments and submissions system-wide but does not create or grade.
- **Teacher** creates assignments for classes/subjects they are assigned to, publishes them, and reviews student submissions with marks and feedback.
- **Student** views published assignments for classes they are enrolled in, submits answers before the deadline, and views their submission status and review results.

---

## Main Features

### Authentication & Authorization
- JWT (HS256) login with BCrypt password hashing (work factor 11)
- Role-based access control enforced server-side on every endpoint
- 120-minute token expiry; no refresh tokens

### Admin
- Full CRUD for users, classes/courses, and subjects
- Assign teachers to class+subject combinations
- Enroll students into classes
- Read-only visibility of all assignments and submissions (any status, any owner)

### Teacher
- Create assignments (title, description, deadline, max marks, class, subject)
- Assignments start as **Draft**; teacher publishes when ready (**Draft → Published**)
- Update and delete own assignments (ownership enforced)
- View submissions for own assignments
- Review submissions: assign marks (0–MaxMarks), provide feedback, set status

### Student
- View published assignments for enrolled classes only (drafts are invisible)
- Submit answers before the deadline (UTC)
- Update submissions before deadline if `AllowResubmission` is enabled
- View own submissions and review results (marks/feedback)

---

## Technology Stack

| Layer | Technology |
|---|---|
| **Backend** | ASP.NET Core 8, C#, EF Core 8, Npgsql (PostgreSQL) |
| **Frontend** | Next.js 14 (App Router), React 18, TypeScript, Tailwind CSS |
| **Database** | PostgreSQL 16+ |
| **Auth** | JWT (HS256), BCrypt.Net-Next |
| **Validation** | FluentValidation |
| **Testing** | xUnit, FluentAssertions, EF Core In-Memory |
| **API Docs** | Swagger / OpenAPI |

---

## Project Structure

```
Assignment-Management-System/
├── docs/                          # Specification documents (Phase 0)
├── server/                        # Backend — ASP.NET Core 8
│   ├── AssignmentManagement.sln
│   ├── Directory.Packages.props   # Central Package Management
│   ├── src/
│   │   ├── AssignmentManagement.Api/            # Presentation layer
│   │   ├── AssignmentManagement.Application/    # DTOs, services, validators
│   │   ├── AssignmentManagement.Domain/         # Entities, enums, exceptions
│   │   └── AssignmentManagement.Infrastructure/ # EF Core, DbContext, seeding, JWT
│   └── tests/
│       └── AssignmentManagement.UnitTests/      # xUnit tests
├── client/                        # Frontend — Next.js 14
│   ├── package.json
│   └── src/
│       ├── app/                   # App Router pages
│       ├── components/            # UI, layout, forms, guards
│       ├── hooks/                 # useAuth, useApi, useCurrentUser
│       └── lib/                   # API client, types, auth, utils
├── .env.example                   # Environment variable template
├── .gitignore
└── README.md
```

### Backend Architecture (Layered)

```
Api ──► Application ──► Domain
  │
  └────► Infrastructure ──► Application ──► Domain
```

- **Domain** — pure entities, enums, exceptions (zero dependencies)
- **Application** — DTOs, services (business logic), validators, mapping
- **Infrastructure** — EF Core `AppDbContext`, entity configurations, JWT, BCrypt, seeding, migrations
- **Api** — controllers, middleware, Swagger, DI wiring

---

## Setup Instructions

### Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 8.0+ |
| Node.js | 18+ |
| PostgreSQL | 16+ |
| npm | 9+ |

### 1. Database Setup (PostgreSQL)

Ensure PostgreSQL is running on `localhost:5432` with a `postgres` superuser.

Create the database:

```sql
CREATE DATABASE assignment_management;
```

> The schema (tables, indexes, constraints) is created automatically by EF Core migrations on first run — **no manual table creation needed**.

### 2. Backend Setup

```powershell
cd server

# Restore dependencies
dotnet restore AssignmentManagement.sln

# Build
dotnet build AssignmentManagement.sln

# Apply EF Core migrations + seed demo data (run once)
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update -p src/AssignmentManagement.Infrastructure -s src/AssignmentManagement.Api

# Start the API (auto-migrates + seeds on startup in Development)
dotnet run --project src/AssignmentManagement.Api
```

The API runs on **http://localhost:5000**. Swagger UI is available at **http://localhost:5000/swagger**.

> **First run:** The `DbInitializationService` (a hosted service, Development-only) auto-applies pending migrations and seeds demo users + sample data. If the database doesn't exist yet, create it first (step 1), then the app handles the rest.

### 3. Frontend Setup

```powershell
cd client

# Install dependencies
npm install

# Create local env file (or copy from .env.example)
# NEXT_PUBLIC_API_URL=http://localhost:5000

# Start the dev server
npm run dev
```

The frontend runs on **http://localhost:3000**.

> Ensure the backend API is running on `http://localhost:5000` before starting the frontend. The frontend calls the API for all data operations.

### 4. Production Build (Frontend)

```powershell
cd client
npm run build
npm start
```

---

## Running Tests

### Backend Unit Tests (xUnit)

```powershell
cd server
dotnet test AssignmentManagement.sln
```

Tests cover 78+ scenarios across all business rules:

| Test Area | Scenarios |
|---|---|
| Auth | Valid/invalid login, disabled user, PasswordHash secrecy |
| User CRUD | Create, update, delete, duplicate email (409) |
| Class/Subject CRUD | Full lifecycle, bad FK (404), duplicate name (409) |
| Teacher Assignment | Assign teacher, duplicate (409), wrong role (404) |
| Enrollment | Enroll student, duplicate (409), wrong role (404) |
| Assignment Rules | Unassigned create (403), draft invisible, published+enrolled only, MaxMarks>0, ownership, UTC deadline, publish transition |
| Submission Rules | Submit before/after deadline, update, draft (404), cross-student (403), upsert, not-enrolled, resubmission=false |
| Review Rules | Own review, marks<0 (400), marks>max (400), boundary, cross-teacher (403), feedback optional, status transition |
| Admin Visibility | Sees all assignments (incl. Draft), all submissions, not limited by ownership |
| Cross-Cutting | UTC deadline storage, PasswordHash not in any DTO, login returns token for all roles |

---

## Demo Credentials

| Role | Email | Password |
|---|---|---|
| Admin | `admin@example.com` | `admin@123` |
| Teacher | `teacher@example.com` | `teacher@123` |
| Teacher (2nd) | `teacher2@example.com` | `teacher@123` |
| Student | `student@example.com` | `student@123` |

> These accounts are seeded automatically on first run (Development environment). Passwords are hashed with BCrypt (work factor 11) — the plaintext above is for login only.

---

## Environment Configuration

### Backend

Configure via environment variables or `appsettings.Development.json`:

| Variable | Default | Description |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Enables Swagger, auto-migration, seeding |
| `ConnectionStrings__DefaultConnection` | — | PostgreSQL connection string |
| `Jwt__Secret` | — | HS256 signing key (≥ 32 bytes) |
| `Jwt__Issuer` | `assignment-management-api` | JWT `iss` claim |
| `Jwt__Audience` | `assignment-management-client` | JWT `aud` claim |
| `Jwt__ExpiryMinutes` | `120` | Token lifetime |

### Frontend

| Variable | Default | Description |
|---|---|---|
| `NEXT_PUBLIC_API_URL` | `http://localhost:5000` | Backend API base URL |

See `.env.example` for the full template.

---

## API Overview

| Area | Base Route | Roles |
|---|---|---|
| Auth | `/api/auth/login`, `/api/auth/me` | Public + any authenticated |
| Admin | `/api/admin/*` | Admin only |
| Teacher | `/api/teacher/*` | Teacher only |
| Student | `/api/student/*` | Student only |

**Key business rules enforced:**
- Teachers can only create assignments for class+subject combinations they are assigned to
- Students see only Published assignments for classes they are enrolled in (drafts are invisible)
- Students cannot submit or update after the deadline (UTC)
- One submission per (assignment, student); resubmission allowed if `AllowResubmission = true`
- Teachers can only review submissions for assignments they own
- Marks must be in `[0, MaxMarks]`
- Admin sees all but cannot create assignments or grade submissions

---

## Assumptions

1. A student can belong to one or more classes/courses.
2. A teacher can be assigned to multiple class/course and subject combinations.
3. Assignments are text-based (no file upload).
4. Students can update submissions multiple times before the deadline (if `AllowResubmission` is enabled).
5. Deadlines are stored and compared in UTC.
6. Late submissions are not allowed after the deadline (no submit or update after `DeadlineUtc`).
7. Teachers can manage only assignments they created (ownership enforced).
8. Admin can view all assignments and submissions but does not submit or grade.
9. Email verification and password reset are out of scope.
10. File upload, notifications, and advanced reporting are not implemented.

---

## Known Limitations / Out of Scope

- **No real-time notifications** — users must refresh to see updates
- **No email verification** — accounts are created without email confirmation
- **No password reset flow** — passwords must be changed via admin
- **No file upload** — submissions are text-based only
- **No pagination** — all list endpoints return the full array (acceptable for this project's scale)
- **No refresh tokens** — expired JWTs require re-login (120-minute expiry)
- **No production deployment configuration** — HTTPS/HSTS are not configured (Development only)
- **No multi-tenancy** — single institution
- **No mobile application** — web only
- **No analytics dashboard** — basic CRUD views only

---

## Database Schema

Seven tables with referential integrity:

```
users ──────────────────┐
 │                      │
class ── subject        │
  │        │            │
  │        │            ├─ teacher_class_subject (teacher → class + subject)
  │        │            │
  ├─ enrollment ────────┤  (student → class)
  │                      │
  └─ assignment ── submission (student submits answer for assignment)
```

- **Unique constraints:** email, (classId, subjectName), (teacherId, classId, subjectId), (classId, studentId), (assignmentId, studentId)
- **CHECK constraints:** `MaxMarks > 0`, `Marks IS NULL OR Marks >= 0`
- **Soft disable:** `IsActive = false` prevents login; hard delete allowed with FK restrict

---

## License

This project is a recruitment assessment for **OnnoRokom Projukti Limited**.
