# API Contract — Assignment & Submission Management System

> **PHASE 0 contract.** This document is the single, self-contained source of truth for backend
> controllers and DTOs. A developer must be able to implement every controller and DTO from this file
> alone — without reading `PRD.md`.
>
> **Authoritative inputs:** the canonical project contract (stack, enums, entities, rules, JWT,
> error envelope, endpoints) and `PRD.md` §11 (status codes) and §7 (endpoint areas). Names below
> are used **verbatim**; do not rename.
>
> **Out of scope for this file:** this is an HTTP contract only. Authorization semantics, claim
> structure details, and token validation rules live in `docs/AUTH_MODEL.md` (companion document).
> See §8 for the role→route matrix.

---

## Table of Contents

1. [Conventions](#1-conventions)
2. [Error Model](#2-error-model)
3. [Authentication](#3-authentication)
4. [Admin Endpoints](#4-admin-endpoints)
5. [Teacher Endpoints](#5-teacher-endpoints)
6. [Student Endpoints](#6-student-endpoints)
7. [Common DTO Reference](#7-common-dto-reference)
8. [Authorization Matrix](#8-authorization-matrix)
9. [Worked Examples](#9-worked-examples)

---

## 1. Conventions

| Item | Value |
|---|---|
| Base URL | `http://localhost:5000` |
| Auth header | `Authorization: Bearer {token}` (JWT, HS256) |
| Content-Type | `application/json` (UTF-8) for all request and response bodies |
| Date/Time format | ISO-8601 UTC, e.g. `2026-08-20T23:59:00Z`. **All deadlines and timestamps are UTC.** Deadlines are compared against current UTC time (PRD business rule #12). |
| Identifier format | UUID / .NET `Guid`, lowercased string in JSON, e.g. `"a1b2c3d4-1111-2222-3333-444455556666"` |
| API versioning | Version-neutral. No `/v1/` prefix. |
| Trailing slashes | Not used. |
| Empty-body responses | `204 No Content` returns no body. |
| Enums (stored as string) | `UserRole { Admin, Teacher, Student }`; `AssignmentStatus { Draft, Published, Archived }`; `SubmissionStatus { Submitted, UnderReview, Reviewed, LateSubmitted }`. Enum values are serialized as PascalCase strings in JSON (e.g. `"Published"`). |
| **Pagination** | **None for v1.** All list endpoints return the full array (no `page`, `pageSize`, `total`). **Assumption:** dataset is small for this recruitment project; pagination will be added in a later phase. |
| Casing | JSON property names are **camelCase** (ASP.NET Core default via `System.Text.Json`, `JsonNamingPolicy.CamelCase`). DTO field names in §7 are listed in PascalCase (the C# property name); the wire name is camelCase. |

### 1.1 Security notes (relevant to contract)

- `PasswordHash` is stored with BCrypt and **never** appears in any response DTO (see §7).
- JWT claims: `sub` (userId), `email`, `role`, `name`, `jti`, `iat`, `exp`.
- JWT config keys: `Jwt:Secret`, `Jwt:Issuer` = `assignment-management-api`,
  `Jwt:Audience` = `assignment-management-client`, `Jwt:ExpiryMinutes` = `120`.
- No refresh tokens.

### 1.2 Request/response rule summary (from canonical contract)

- `0 ≤ Marks ≤ MaxMarks`; `MaxMarks > 0`.
- One submission per `(assignment, student)` — `UNIQUE(AssignmentId, StudentId)`.
- Students see only **Published** assignments for **enrolled** classes; drafts are invisible to students.
- No submit/update after the deadline.
- Teachers manage only **their own** assignments and review only submissions **for their** assignments.
- Admin sees all assignments/submissions but does **not** create or grade.

---

## 2. Error Model

### 2.1 Error envelope

Every error response uses the same envelope:

```json
{
  "message": "string",
  "errors": {
    "field": [ "error message" ]
  }
}
```

- `message` (string, always present): human-readable summary of the failure.
- `errors` (object, present for `400` validation failures, absent/empty otherwise): map of field
  name → array of one or more error strings. Field names use camelCase. For non-field errors
  (e.g. auth, not-found, conflict), `errors` may be omitted and only `message` is returned.

### 2.2 HTTP status code table

Copied from **PRD §11**:

| Status Code | Usage |
|---|---|
| `200` | Successful request |
| `201` | Resource created |
| `204` | Successful deletion with no content |
| `400` | Validation error or bad request |
| `401` | Not authenticated |
| `403` | Authenticated but not authorized |
| `404` | Resource not found |
| `409` | Conflict, such as duplicate email |
| `500` | Unexpected server error |

**Endpoint-specific application of the table** (recurring patterns):

| Code | When it is returned |
|---|---|
| `400` | DTO validation failure (missing/invalid fields), `MaxMarks <= 0`, `Marks` out of `[0, MaxMarks]`, deadline not a valid future date on create, submit/update attempted after deadline. |
| `401` | Missing, malformed, expired, or otherwise invalid JWT; or bad login credentials. |
| `403` | Authenticated user whose role cannot call the route group (e.g. Student hitting `/api/teacher/*`), **or** authenticated user who is **not the owner** of the resource (teacher does not own the assignment; student not enrolled; teacher not owner of the assignment being reviewed; student accessing another student's submission). |
| `404` | Referenced resource id does not exist. For ownership-protected resources, a non-owner typically receives `403` (not `404`) — see per-endpoint notes. |
| `409` | Unique-constraint conflict: duplicate email on user create/update; duplicate `(ClassId, Name)` subject; duplicate `(TeacherId, ClassId, SubjectId)` teacher-assignment; duplicate `(ClassId, StudentId)` enrollment; duplicate `(AssignmentId, StudentId)` submission (resubmit when `AllowResubmission == false`). |
| `500` | Unhandled server exception. |

### 2.3 Example error response (validation failure)

`HTTP/1.1 400 Bad Request`

```json
{
  "message": "Validation failed.",
  "errors": {
    "title": [ "Title is required." ],
    "maxMarks": [ "Maximum marks must be greater than zero." ]
  }
}
```

### 2.4 Example error response (conflict)

`HTTP/1.1 409 Conflict`

```json
{
  "message": "A user with this email already exists.",
  "errors": {
    "email": [ "Email must be unique." ]
  }
}
```

### 2.5 Example error response (forbidden / not owner)

`HTTP/1.1 403 Forbidden`

```json
{
  "message": "You are not allowed to perform this action on this resource."
}
```

---

## 3. Authentication

All routes except `POST /api/auth/login` require a valid `Authorization: Bearer {token}` header.
A missing/invalid token yields `401` on any protected route.

### 3.1 POST /api/auth/login

Exchanges credentials for a JWT. Public — no auth required.

**Request DTO — `LoginRequest`**

| Field | Type | Required | Notes |
|---|---|---|---|
| `email` | string | yes | Valid email; compared case-insensitively (emails are stored lowercased). |
| `password` | string | yes | Plaintext; verified against BCrypt hash. |

Request example:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "teacher@example.com",
  "password": "teacher@123"
}
```

**Response — `200 OK` → `AuthResponse`**

| Field | Type | Notes |
|---|---|---|
| `token` | string | JWT (HS256). |
| `expiresAt` | string (ISO-8601 UTC) | Token expiry = `iat + 120` minutes. |
| `user` | `UserDto` | Current user. See §7. `role` is one of `Admin`, `Teacher`, `Student`. |

Response example:

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjMWQy...signature",
  "expiresAt": "2026-08-07T13:46:13Z",
  "user": {
    "id": "c1d2e3f4-0001-0001-0001-000000000001",
    "name": "Demo Teacher",
    "email": "teacher@example.com",
    "role": "Teacher",
    "isActive": true,
    "createdAt": "2026-08-01T09:00:00Z",
    "updatedAt": "2026-08-01T09:00:00Z"
  }
}
```

**Errors**

| Status | Condition |
|---|---|
| `400` | Malformed JSON, missing `email`/`password`, invalid email format. |
| `401` | No user with that email, wrong password, or user `IsActive == false`. |
| `500` | Unexpected server error. |

`401` example:

```json
{ "message": "Invalid email or password." }
```

### 3.2 GET /api/auth/me

Returns the currently authenticated user. Requires any valid authenticated role (Admin, Teacher, or Student).

**Request:** no body. Send `Authorization: Bearer {token}`.

**Response — `200 OK` → `UserDto`**

```json
{
  "id": "c1d2e3f4-0001-0001-0001-000000000002",
  "name": "Demo Student",
  "email": "student@example.com",
  "role": "Student",
  "isActive": true,
  "createdAt": "2026-08-01T09:00:00Z",
  "updatedAt": "2026-08-01T09:00:00Z"
}
```

**Errors**

| Status | Condition |
|---|---|
| `401` | Missing/invalid token. |

---

## 4. Admin Endpoints

> **Required role for every route in this section: `Admin`.**
>
| Route group | Required role |
|---|---|
| `/api/admin/*` | `Admin` only |

A non-Admin (Teacher/Student) calling any route below receives `403`. A missing/invalid token receives `401`.

### 4.1 Users — `/api/admin/users`

| # | Method | Path | Request DTO | Response DTO | Success | Error statuses |
|---|---|---|---|---|---|---|
| U-1 | `POST` | `/api/admin/users` | `CreateUserRequest` | `UserDto` | `201` | `400`, `401`, `403`, `409`, `500` |
| U-2 | `GET` | `/api/admin/users` | — | `UserDto[]` | `200` | `401`, `403`, `500` |
| U-3 | `GET` | `/api/admin/users/{id}` | — | `UserDto` | `200` | `401`, `403`, `404`, `500` |
| U-4 | `PUT` | `/api/admin/users/{id}` | `UpdateUserRequest` | `UserDto` | `200` | `400`, `401`, `403`, `404`, `409`, `500` |
| U-5 | `DELETE` | `/api/admin/users/{id}` | — | *(no body)* | `204` | `401`, `403`, `404`, `500` |

**Path param:** `{id}` = user `Guid`.

**Notes**
- `POST` returns `409` on duplicate email (case-insensitive, normalized to lower).
- `PUT` may change `isActive` (disable) and `role`; `409` on duplicate email if changed.
- `DELETE` is a hard delete returning `204`; (soft-delete via `PUT isActive=false` is the alternative disable path — both supported, `DELETE` returns `204`).
- `UserDto` never includes `PasswordHash` (see §7).

### 4.2 Classes/Courses — `/api/admin/classes`

| # | Method | Path | Request DTO | Response DTO | Success | Error statuses |
|---|---|---|---|---|---|---|
| C-1 | `POST` | `/api/admin/classes` | `CreateClassRequest` | `ClassDto` | `201` | `400`, `401`, `403`, `500` |
| C-2 | `GET` | `/api/admin/classes` | — | `ClassDto[]` | `200` | `401`, `403`, `500` |
| C-3 | `PUT` | `/api/admin/classes/{id}` | `UpdateClassRequest` | `ClassDto` | `200` | `400`, `401`, `403`, `404`, `500` |
| C-4 | `DELETE` | `/api/admin/classes/{id}` | — | *(no body)* | `204` | `401`, `403`, `404`, `500` |

**Path param:** `{id}` = class `Guid`.

**Notes**
- `UpdateClassRequest` mirrors `CreateClassRequest` with optional fields (`name?`, `description?`).
- `GET /api/admin/classes` returns the full array (no pagination — see §1).

### 4.3 Subjects — `/api/admin/subjects`

| # | Method | Path | Request DTO | Response DTO | Success | Error statuses |
|---|---|---|---|---|---|---|
| S-1 | `POST` | `/api/admin/subjects` | `CreateSubjectRequest` | `SubjectDto` | `201` | `400`, `401`, `403`, `404`, `409`, `500` |
| S-2 | `GET` | `/api/admin/subjects` | — | `SubjectDto[]` | `200` | `401`, `403`, `500` |
| S-3 | `PUT` | `/api/admin/subjects/{id}` | `UpdateSubjectRequest` | `SubjectDto` | `200` | `400`, `401`, `403`, `404`, `409`, `500` |
| S-4 | `DELETE` | `/api/admin/subjects/{id}` | — | *(no body)* | `204` | `401`, `403`, `404`, `500` |

**Notes**
- `UNIQUE(ClassId, Name)`: `POST`/`PUT` return `409` if the `(classId, name)` pair already exists.
- `POST`/`PUT` return `404` if the referenced `classId` does not exist.

### 4.4 Teacher Assignments — `/api/admin/teacher-assignments`

Assigns a teacher to a (class, subject) combination.

| # | Method | Path | Request DTO | Response DTO | Success | Error statuses |
|---|---|---|---|---|---|---|
| T-1 | `POST` | `/api/admin/teacher-assignments` | `CreateTeacherAssignmentRequest` | `TeacherAssignmentDto` | `201` | `400`, `401`, `403`, `404`, `409`, `500` |
| T-2 | `GET` | `/api/admin/teacher-assignments` | — | `TeacherAssignmentDto[]` | `200` | `401`, `403`, `500` |

**Notes**
- `UNIQUE(TeacherId, ClassId, SubjectId)`: `POST` returns `409` on duplicate.
- `POST` returns `404` if `teacherId` / `classId` / `subjectId` references a non-existent entity,
  or if the referenced user's role is not `Teacher`.
- This is what authorizes a teacher to create assignments for a given `(classId, subjectId)` — see §5.

### 4.5 Enrollments — `/api/admin/enrollments`

Enrolls a student into a class/course.

| # | Method | Path | Request DTO | Response DTO | Success | Error statuses |
|---|---|---|---|---|---|---|
| E-1 | `POST` | `/api/admin/enrollments` | `CreateEnrollmentRequest` | `EnrollmentDto` | `201` | `400`, `401`, `403`, `404`, `409`, `500` |
| E-2 | `GET` | `/api/admin/enrollments` | — | `EnrollmentDto[]` | `200` | `401`, `403`, `500` |

**Notes**
- `UNIQUE(ClassId, StudentId)`: `POST` returns `409` on duplicate.
- `POST` returns `404` if `classId`/`studentId` does not exist, or the referenced user's role is not `Student`.

### 4.6 Assignments (read-all) — ADM-001

| # | Method | Path | Request DTO | Response DTO | Success | Error statuses |
|---|---|---|---|---|---|---|
| A-1 | `GET` | `/api/admin/assignments` | — | `AssignmentSummaryDto[]` | `200` | `401`, `403`, `500` |

**Notes (ADM-001)**
- Returns **all** assignments across the system, regardless of teacher ownership, status (Draft/Published/Archived), or class. Admin visibility is **not** limited by teacher-assignment rules (ADM-003).
- Uses the summary DTO (no large text fields). For full detail, Admin has no dedicated detail route in this contract.

### 4.7 Submissions (read-all) — ADM-002

| # | Method | Path | Request DTO | Response DTO | Success | Error statuses |
|---|---|---|---|---|---|---|
| SB-1 | `GET` | `/api/admin/submissions` | — | `SubmissionSummaryDto[]` | `200` | `401`, `403`, `500` |

**Notes (ADM-002)**
- Returns **all** submissions across the system, across all teachers and students.
- Admin can **view** but does **not** create or grade submissions.

---

## 5. Teacher Endpoints

> **Required role for every route in this section: `Teacher`.**
>
| Route group | Required role |
|---|---|
| `/api/teacher/*` | `Teacher` only |

A non-Teacher calling any route below receives `403`; a missing/invalid token receives `401`.

### 5.1 Assignment CRUD — `/api/teacher/assignments`

| # | Method | Path | Request DTO | Response DTO | Success | Error statuses |
|---|---|---|---|---|---|---|
| TA-1 | `POST` | `/api/teacher/assignments` | `CreateAssignmentRequest` | `AssignmentDto` | `201` | `400`, `401`, `403`, `404`, `500` |
| TA-2 | `GET` | `/api/teacher/assignments` | — | `AssignmentDto[]` | `200` | `401`, `403`, `500` |
| TA-3 | `GET` | `/api/teacher/assignments/{id}` | — | `AssignmentDto` | `200` | `401`, `403`, `404`, `500` |
| TA-4 | `PUT` | `/api/teacher/assignments/{id}` | `UpdateAssignmentRequest` | `AssignmentDto` | `200` | `400`, `401`, `403`, `404`, `500` |
| TA-5 | `DELETE` | `/api/teacher/assignments/{id}` | — | *(no body)* | `204` | `401`, `403`, `404`, `500` |
| TA-6 | `POST` | `/api/teacher/assignments/{id}/publish` | — | `AssignmentDto` | `200` | `401`, `403`, `404`, `500` |

**Path param:** `{id}` = assignment `Guid`. The `TeacherId` is taken from the JWT (`sub` claim), not the request body.

**Ownership enforcement (canonical contract):**
- A teacher may only **create** an assignment for a `(classId, subjectId)` they are assigned to via a
  `TeacherClassSubjects` row (see §4.4). Otherwise → `403`.
- `GET` (list, TA-2) returns **only** assignments where `TeacherId == current user`.
- `GET/{id}`, `PUT/{id}`, `DELETE/{id}`, `POST/{id}/publish` — if the assignment's `TeacherId != current user` → `403` (even if the id exists). A non-existent id → `404`.
- `CreateAssignmentRequest.Status` is **not** settable on create; new assignments always start as `Draft` (default). To publish, call TA-6. `TA-6` sets `Status = Published` and returns the updated `AssignmentDto`.
- Validation: `title` required, `description` required, `deadlineUtc` required and must be a valid **future** date, `maxMarks > 0`, `classId` required, `subjectId` required (PRD §10.3).

### 5.2 Submissions for an assignment — `/api/teacher/assignments/{assignmentId}/submissions`

| # | Method | Path | Request DTO | Response DTO | Success | Error statuses |
|---|---|---|---|---|---|---|
| TS-1 | `GET` | `/api/teacher/assignments/{assignmentId}/submissions` | — | `SubmissionDto[]` | `200` | `401`, `403`, `404`, `500` |

**Path param:** `{assignmentId}` = assignment `Guid`.

**Ownership enforcement:** the teacher must **own** the assignment (`assignment.TeacherId == current user`).
If they do not own it → `403` (SUB-008: "Teacher can view submissions for their assignments"). If the assignment id does not exist → `404`.

### 5.3 Review a submission — `/api/teacher/submissions/{submissionId}/review`

| # | Method | Path | Request DTO | Response DTO | Success | Error statuses |
|---|---|---|---|---|---|---|
| TR-1 | `PUT` | `/api/teacher/submissions/{submissionId}/review` | `ReviewSubmissionRequest` | `SubmissionDto` | `200` | `400`, `401`, `403`, `404`, `500` |

**Path param:** `{submissionId}` = submission `Guid`.

**Ownership enforcement (canonical contract):** the teacher must own **the assignment that the submission belongs to**
(`submission → assignment → teacherId == current user`). If not the owner → `403`. A non-existent submission id → `404`.

**`ReviewSubmissionRequest` rules:**
- `marks`: required on review, integer, must satisfy `0 ≤ marks ≤ assignment.MaxMarks`. Out of range → `400`.
- `feedback`: optional string (PRD §10.5).
- `status`: optional `SubmissionStatus`. If omitted, the server sets `Reviewed`. If provided, must be a valid enum value.
- Side effects on success: sets `Marks`, `Feedback`, `ReviewedByTeacherId = current user`, `ReviewedAtUtc = now UTC`, and `Status` (default `Reviewed`).

### 5.4 Teacher DTOs (defined in §7)

`CreateAssignmentRequest`, `UpdateAssignmentRequest`, `AssignmentDto`, `ReviewSubmissionRequest`, `SubmissionDto`.

---

## 6. Student Endpoints

> **Required role for every route in this section: `Student`.**
>
| Route group | Required role |
|---|---|
| `/api/student/*` | `Student` only |

A non-Student calling any route below receives `403`; a missing/invalid token receives `401`.

### 6.1 Assignments — `/api/student/assignments`

| # | Method | Path | Request DTO | Response DTO | Success | Error statuses |
|---|---|---|---|---|---|---|
| SA-1 | `GET` | `/api/student/assignments` | — | `AssignmentDto[]` | `200` | `401`, `403`, `500` |
| SA-2 | `GET` | `/api/student/assignments/{id}` | — | `AssignmentDto` | `200` | `401`, `403`, `404`, `500` |

**Enforcement (canonical contract + PRD §3.4, business rules #4, #5):**
- Both routes return only **Published** assignments for classes the student is **enrolled** in.
- **Drafts are invisible to students** (ASGN-008). An enrolled student requesting a Draft assignment by id → `404` (treated as not visible / not found).
- A Published assignment in a class the student is **not** enrolled in → `404` on `GET/{id}` and excluded from the list.
- `{id}` not found → `404`.

### 6.2 Submit an answer — `/api/student/assignments/{assignmentId}/submit`

| # | Method | Path | Request DTO | Response DTO | Success | Error statuses |
|---|---|---|---|---|---|---|
| SA-3 | `POST` | `/api/student/assignments/{assignmentId}/submit` | `SubmitRequest` | `SubmissionDto` | `201` | `400`, `401`, `403`, `404`, `409`, `500` |

**Path param:** `{assignmentId}` = assignment `Guid`. The `StudentId` is taken from the JWT.

**Enforcement (canonical contract + PRD §3.5, §10.4):**
- Assignment must exist, be **Published**, the student must be **enrolled** in `assignment.ClassId`, and submission must occur **before** `DeadlineUtc` (compared in UTC).
- If the student is not enrolled, or the assignment is not Published → `403` (or `404` if the assignment is a draft / invisible). **Recommended:** not-enrolled → `403`; draft/missing → `404`. (Documented as an assumption; pick one and keep consistent.)
- After deadline → `400` (business rule #6: "Students can submit only before the assignment deadline").
- `answerText` required → `400` if missing/empty.
- **One submission per `(assignment, student)`:** if a submission already exists:
  - `AllowResubmission == true` → this endpoint **updates** the existing submission (sets `AnswerText`, `UpdatedAtUtc`, `Status`) and returns `201` (or `200` on update — see note).
  - `AllowResubmission == false` → `409` (duplicate `(AssignmentId, StudentId)`).
  - **Assumption:** when `AllowResubmission == true`, `submit` on an existing submission returns `200` with the updated `SubmissionDto`. Implementers should treat `submit` + existing as upsert when resubmission is allowed.

### 6.3 Submissions — `/api/student/submissions`

| # | Method | Path | Request DTO | Response DTO | Success | Error statuses |
|---|---|---|---|---|---|---|
| SA-4 | `PUT` | `/api/student/submissions/{submissionId}` | `UpdateSubmissionRequest` | `SubmissionDto` | `200` | `400`, `401`, `403`, `404`, `500` |
| SA-5 | `GET` | `/api/student/submissions` | — | `SubmissionDto[]` | `200` | `401`, `403`, `500` |
| SA-6 | `GET` | `/api/student/submissions/{submissionId}` | — | `SubmissionDto` | `200` | `401`, `403`, `404`, `500` |

**Path param:** `{submissionId}` = submission `Guid`.

**Enforcement (canonical contract + PRD §3.5, business rules #7, #8):**
- `PUT` updates only `AnswerText`. Must occur **before the deadline** (UTC) → `400` after deadline.
- **Ownership:** `submission.StudentId == current user`. A student **cannot view another student's submission** (SUB-007, rule #8). Non-owner → `403`. Non-existent → `404`.
- `GET` list returns only the current student's submissions.

### 6.4 Student DTOs (defined in §7)

`SubmitRequest`, `UpdateSubmissionRequest`, `AssignmentDto`, `SubmissionDto`.

---

## 7. Common DTO Reference

> **Naming:** C# PascalCase property names are listed; the JSON wire name is **camelCase** (see §1).
> **`?`** = nullable/optional on the wire.
> **`PasswordHash` is never included in any User DTO.** Every `UserDto` below omits it by design
> (PRD business rule #13).

### 7.1 Auth DTOs

**`LoginRequest`**

| Field | Type | Required |
|---|---|---|
| `email` | string | yes |
| `password` | string | yes |

**`AuthResponse`**

| Field | Type | Notes |
|---|---|---|
| `token` | string | JWT |
| `expiresAt` | string (ISO-8601 UTC) | |
| `user` | `UserDto` | Current user (no `PasswordHash`) |

### 7.2 User DTOs

**`UserDto`** — `PasswordHash` is **never** serialized.

| Field | Type | Notes |
|---|---|---|
| `id` | Guid (string) | |
| `name` | string | |
| `email` | string | Lowercased unique |
| `role` | `UserRole` | `Admin` \| `Teacher` \| `Student` |
| `isActive` | boolean | |
| `createdAt` | string (ISO-8601 UTC) | |
| `updatedAt` | string (ISO-8601 UTC) | |

**`CreateUserRequest`**

| Field | Type | Required | Notes |
|---|---|---|---|
| `name` | string | yes | |
| `email` | string | yes | Unique; normalized to lower |
| `password` | string | yes | Hashed with BCrypt; never returned |
| `role` | `UserRole` | yes | One of `Admin`, `Teacher`, `Student` |

**`UpdateUserRequest`** — all fields optional.

| Field | Type | Required | Notes |
|---|---|---|---|
| `name` | string? | no | |
| `email` | string? | no | `409` if it collides with another user |
| `role` | `UserRole?` | no | |
| `isActive` | boolean? | no | Use `false` to disable |

### 7.3 Class/Course DTOs

**`ClassDto`**

| Field | Type | Notes |
|---|---|---|
| `id` | Guid (string) | |
| `name` | string | |
| `description` | string? | |
| `createdAt` | string (ISO-8601 UTC) | |
| `updatedAt` | string (ISO-8601 UTC) | |

**`CreateClassRequest`**

| Field | Type | Required |
|---|---|---|
| `name` | string | yes |
| `description` | string? | no |

**`UpdateClassRequest`** — all optional.

| Field | Type | Required |
|---|---|---|
| `name` | string? | no |
| `description` | string? | no |

### 7.4 Subject DTOs

**`SubjectDto`**

| Field | Type | Notes |
|---|---|---|
| `id` | Guid (string) | |
| `name` | string | |
| `classId` | Guid (string) | FK → Class |
| `createdAt` | string (ISO-8601 UTC) | |
| `updatedAt` | string (ISO-8601 UTC) | |

**`CreateSubjectRequest`**

| Field | Type | Required | Notes |
|---|---|---|---|
| `name` | string | yes | `UNIQUE(ClassId, Name)` |
| `classId` | Guid (string) | yes | `404` if missing |

**`UpdateSubjectRequest`** — all optional.

| Field | Type | Required |
|---|---|---|
| `name` | string? | no |
| `classId` | Guid (string)? | no |

### 7.5 Teacher Assignment DTOs

**`TeacherAssignmentDto`**

| Field | Type | Notes |
|---|---|---|
| `id` | Guid (string) | |
| `teacherId` | Guid (string) | FK → User (role Teacher) |
| `classId` | Guid (string) | FK → Class |
| `subjectId` | Guid (string) | FK → Subject |
| `createdAt` | string (ISO-8601 UTC) | |

**`CreateTeacherAssignmentRequest`**

| Field | Type | Required | Notes |
|---|---|---|---|
| `teacherId` | Guid (string) | yes | `UNIQUE(TeacherId, ClassId, SubjectId)`; `404` if user not a Teacher |
| `classId` | Guid (string) | yes | `404` if missing |
| `subjectId` | Guid (string) | yes | `404` if missing |

### 7.6 Enrollment DTOs

**`EnrollmentDto`**

| Field | Type | Notes |
|---|---|---|
| `id` | Guid (string) | |
| `classId` | Guid (string) | FK → Class |
| `studentId` | Guid (string) | FK → User (role Student) |
| `enrolledAt` | string (ISO-8601 UTC) | |

**`CreateEnrollmentRequest`**

| Field | Type | Required | Notes |
|---|---|---|---|
| `classId` | Guid (string) | yes | `UNIQUE(ClassId, StudentId)`; `404` if missing |
| `studentId` | Guid (string) | yes | `404` if user not a Student |

### 7.7 Assignment DTOs

**`AssignmentDto`**

| Field | Type | Notes |
|---|---|---|
| `id` | Guid (string) | |
| `title` | string | |
| `description` | string | |
| `deadlineUtc` | string (ISO-8601 UTC) | |
| `maxMarks` | int | `> 0` |
| `status` | `AssignmentStatus` | `Draft` \| `Published` \| `Archived` |
| `teacherId` | Guid (string) | Creator (FK → User Teacher) |
| `classId` | Guid (string) | FK → Class |
| `subjectId` | Guid (string) | FK → Subject |
| `allowResubmission` | boolean | Default `true` |
| `createdAt` | string (ISO-8601 UTC) | |
| `updatedAt` | string (ISO-8601 UTC) | |

**`CreateAssignmentRequest`**

| Field | Type | Required | Notes |
|---|---|---|---|
| `title` | string | yes | |
| `description` | string | yes | |
| `deadlineUtc` | string (ISO-8601 UTC) | yes | Must be a valid future date |
| `maxMarks` | int | yes | `> 0`, else `400` |
| `classId` | Guid (string) | yes | Teacher must be assigned to `(classId, subjectId)`, else `403` |
| `subjectId` | Guid (string) | yes | |
| `allowResubmission` | boolean? | no | Defaults to `true` |

> `Status` is **not** accepted on create; new assignments are always created with `Status = Draft`.

**`UpdateAssignmentRequest`** — all optional (partial update).

| Field | Type | Required | Notes |
|---|---|---|---|
| `title` | string? | no | |
| `description` | string? | no | |
| `deadlineUtc` | string (ISO-8601 UTC)? | no | Must be valid if provided |
| `maxMarks` | int? | no | `> 0` if provided |
| `classId` | Guid (string)? | no | Must remain an assigned `(classId, subjectId)` |
| `subjectId` | Guid (string)? | no | |
| `allowResubmission` | boolean? | no | |

**`AssignmentSummaryDto`** — used by Admin read-all (ADM-001).

| Field | Type | Notes |
|---|---|---|
| `id` | Guid (string) | |
| `title` | string | |
| `status` | `AssignmentStatus` | |
| `teacherId` | Guid (string) | |
| `classId` | Guid (string) | |
| `subjectId` | Guid (string) | |
| `deadlineUtc` | string (ISO-8601 UTC) | |
| `maxMarks` | int | |
| `createdAt` | string (ISO-8601 UTC) | |

### 7.8 Submission DTOs

**`SubmissionDto`**

| Field | Type | Notes |
|---|---|---|
| `id` | Guid (string) | |
| `assignmentId` | Guid (string) | |
| `studentId` | Guid (string) | |
| `answerText` | string | |
| `submittedAtUtc` | string (ISO-8601 UTC) | |
| `updatedAtUtc` | string (ISO-8601 UTC) | |
| `status` | `SubmissionStatus` | `Submitted` \| `UnderReview` \| `Reviewed` \| `LateSubmitted` |
| `marks` | int? | `0 ≤ marks ≤ assignment.MaxMarks`; `null` until reviewed |
| `feedback` | string? | `null` until reviewed |
| `reviewedByTeacherId` | Guid (string)? | `null` until reviewed |
| `reviewedAtUtc` | string (ISO-8601 UTC)? | `null` until reviewed |

**`SubmitRequest`**

| Field | Type | Required |
|---|---|---|
| `answerText` | string | yes |

**`UpdateSubmissionRequest`**

| Field | Type | Required |
|---|---|---|
| `answerText` | string | yes |

**`ReviewSubmissionRequest`**

| Field | Type | Required | Notes |
|---|---|---|---|
| `marks` | int | yes | `0 ≤ marks ≤ assignment.MaxMarks`, else `400` |
| `feedback` | string? | no | |
| `status` | `SubmissionStatus?` | no | Defaults server-side to `Reviewed` if omitted |

**`SubmissionSummaryDto`** — used by Admin read-all (ADM-002).

| Field | Type | Notes |
|---|---|---|
| `id` | Guid (string) | |
| `assignmentId` | Guid (string) | |
| `studentId` | Guid (string) | |
| `status` | `SubmissionStatus` | |
| `marks` | int? | |
| `submittedAtUtc` | string (ISO-8601 UTC) | |
| `reviewedAtUtc` | string (ISO-8601 UTC)? | |

### 7.9 Enum reference

| Enum | Values |
|---|---|
| `UserRole` | `Admin`, `Teacher`, `Student` |
| `AssignmentStatus` | `Draft`, `Published`, `Archived` |
| `SubmissionStatus` | `Submitted`, `UnderReview`, `Reviewed`, `LateSubmitted` |

---

## 8. Authorization Matrix

> Detailed authorization semantics (claim structure, token validation, ownership checks,
> policy definitions) live in the companion document **`docs/AUTH_MODEL.md`**. This section is a
> quick cross-reference of role → route group.

| Route group | `Admin` | `Teacher` | `Student` | Anonymous |
|---|---|---|---|---|
| `POST /api/auth/login` | ✓ | ✓ | ✓ | ✓ |
| `GET /api/auth/me` | ✓ | ✓ | ✓ | ✗ (`401`) |
| `/api/admin/*` (users, classes, subjects, teacher-assignments, enrollments, assignments, submissions) | ✓ | ✗ (`403`) | ✗ (`403`) | ✗ (`401`) |
| `/api/teacher/*` (assignments, submissions review) | ✗ (`403`) | ✓ | ✗ (`403`) | ✗ (`401`) |
| `/api/student/*` (assignments, submissions) | ✗ (`403`) | ✗ (`403`) | ✓ | ✗ (`401`) |

**Status code conventions** (canonical contract):
- `401` — no token, malformed token, or invalid/expired token.
- `403` — authenticated but (a) the role is not permitted for the route group, **or** (b) the user is
  not the owner of the resource (teacher not owning the assignment/submission's assignment; student not
  owning the submission; student not enrolled for the class of a published assignment).

**Resource-level ownership (not just role):** even within an allowed route group, ownership checks apply:
- Teacher: assignment `TeacherId == current user`; submission's assignment `TeacherId == current user`.
- Student: submission `StudentId == current user`; assignment must be `Published` and in an enrolled class.

---

## 9. Worked Examples

All examples assume the conventions in §1. `Authorization` headers are abbreviated as `Bearer {token}`.
Replace `{token}` with the real JWT from `POST /api/auth/login`.

### 9.1 Login

**Request**

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "teacher@example.com",
  "password": "teacher@123"
}
```

**Response — `200 OK`**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjMWQy...sig",
  "expiresAt": "2026-08-07T13:46:13Z",
  "user": {
    "id": "c1d2e3f4-0001-0001-0001-000000000001",
    "name": "Demo Teacher",
    "email": "teacher@example.com",
    "role": "Teacher",
    "isActive": true,
    "createdAt": "2026-08-01T09:00:00Z",
    "updatedAt": "2026-08-01T09:00:00Z"
  }
}
```

**Error — `401 Unauthorized`** (bad credentials)

```json
{ "message": "Invalid email or password." }
```

**Error — `400 Bad Request`** (missing fields)

```json
{
  "message": "Validation failed.",
  "errors": {
    "email": [ "Email is required." ],
    "password": [ "Password is required." ]
  }
}
```

### 9.2 Create assignment (Teacher)

Precondition: the teacher is assigned to `(classId, subjectId)` via `/api/admin/teacher-assignments`.

**Request**

```http
POST /api/teacher/assignments
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Algebra — Linear Equations",
  "description": "Solve problems 1–10 from Chapter 3.",
  "deadlineUtc": "2026-08-20T23:59:00Z",
  "maxMarks": 100,
  "classId": "b0a0a0a0-0002-0002-0002-000000000002",
  "subjectId": "d0c0c0c0-0003-0003-0003-000000000003",
  "allowResubmission": true
}
```

**Response — `201 Created`**

```json
{
  "id": "e5f5f5f5-0010-0010-0010-000000000010",
  "title": "Algebra — Linear Equations",
  "description": "Solve problems 1–10 from Chapter 3.",
  "deadlineUtc": "2026-08-20T23:59:00Z",
  "maxMarks": 100,
  "status": "Draft",
  "teacherId": "c1d2e3f4-0001-0001-0001-000000000001",
  "classId": "b0a0a0a0-0002-0002-0002-000000000002",
  "subjectId": "d0c0c0c0-0003-0003-0003-000000000003",
  "allowResubmission": true,
  "createdAt": "2026-08-07T13:46:13Z",
  "updatedAt": "2026-08-07T13:46:13Z"
}
```

**Error — `400 Bad Request`** (`maxMarks <= 0`, past deadline, missing title)

```json
{
  "message": "Validation failed.",
  "errors": {
    "maxMarks": [ "Maximum marks must be greater than zero." ],
    "deadlineUtc": [ "Deadline must be a valid future date." ],
    "title": [ "Title is required." ]
  }
}
```

**Error — `403 Forbidden`** (teacher not assigned to this class/subject combination)

```json
{ "message": "You are not assigned to this class and subject." }
```

> Note: a Student/Admin token on this route also yields `403` (role not permitted); an invalid/missing
> token yields `401`.

### 9.3 Submit answer (Student)

Precondition: the assignment is `Published`, the student is enrolled in `assignment.classId`, and the
deadline has not passed.

**Request**

```http
POST /api/student/assignments/e5f5f5f5-0010-0010-0010-000000000010/submit
Authorization: Bearer {token}
Content-Type: application/json

{
  "answerText": "1) x = 3\n2) x = -2\n3) x = 0.5 ..."
}
```

**Response — `201 Created`**

```json
{
  "id": "a9a9a9a9-0020-0020-0020-000000000020",
  "assignmentId": "e5f5f5f5-0010-0010-0010-000000000010",
  "studentId": "c1d2e3f4-0001-0001-0001-000000000002",
  "answerText": "1) x = 3\n2) x = -2\n3) x = 0.5 ...",
  "submittedAtUtc": "2026-08-08T10:00:00Z",
  "updatedAtUtc": "2026-08-08T10:00:00Z",
  "status": "Submitted",
  "marks": null,
  "feedback": null,
  "reviewedByTeacherId": null,
  "reviewedAtUtc": null
}
```

**Error — `400 Bad Request`** (after deadline; business rule #6)

```json
{
  "message": "The assignment deadline has passed.",
  "errors": {
    "deadlineUtc": [ "Submission must occur before the deadline." ]
  }
}
```

**Error — `403 Forbidden`** (student not enrolled in the assignment's class)

```json
{ "message": "You are not enrolled in this class." }
```

**Error — `404 Not Found`** (assignment is a Draft — invisible to students — or id missing)

```json
{ "message": "Assignment not found." }
```

**Error — `409 Conflict`** (already submitted and `allowResubmission == false`)

```json
{
  "message": "You have already submitted an answer for this assignment.",
  "errors": {
    "assignmentId": [ "A submission for this assignment and student already exists." ]
  }
}
```

### 9.4 Review submission (Teacher)

Precondition: the teacher owns the assignment that the submission belongs to.

**Request**

```http
PUT /api/teacher/submissions/a9a9a9a9-0020-0020-0020-000000000020/review
Authorization: Bearer {token}
Content-Type: application/json

{
  "marks": 85,
  "feedback": "Good work. Recheck problem 7 — sign error.",
  "status": "Reviewed"
}
```

**Response — `200 OK`**

```json
{
  "id": "a9a9a9a9-0020-0020-0020-000000000020",
  "assignmentId": "e5f5f5f5-0010-0010-0010-000000000010",
  "studentId": "c1d2e3f4-0001-0001-0001-000000000002",
  "answerText": "1) x = 3\n2) x = -2\n3) x = 0.5 ...",
  "submittedAtUtc": "2026-08-08T10:00:00Z",
  "updatedAtUtc": "2026-08-08T10:00:00Z",
  "status": "Reviewed",
  "marks": 85,
  "feedback": "Good work. Recheck problem 7 — sign error.",
  "reviewedByTeacherId": "c1d2e3f4-0001-0001-0001-000000000001",
  "reviewedAtUtc": "2026-08-09T08:30:00Z"
}
```

**Error — `400 Bad Request`** (`marks > MaxMarks`; here `MaxMarks = 100`)

```json
{
  "message": "Validation failed.",
  "errors": {
    "marks": [ "Marks must be between 0 and 100." ]
  }
}
```

**Error — `400 Bad Request`** (negative marks)

```json
{
  "message": "Validation failed.",
  "errors": {
    "marks": [ "Marks must be between 0 and 100." ]
  }
}
```

**Error — `403 Forbidden`** (teacher does not own the assignment of this submission)

```json
{ "message": "You can only review submissions for your own assignments." }
```

**Error — `404 Not Found`** (submission id does not exist)

```json
{ "message": "Submission not found." }
```

---

*End of API Contract. Companion documents: `docs/PRD.md` (authoritative requirements, read-only),
`docs/AUTH_MODEL.md` (authorization detail).*
