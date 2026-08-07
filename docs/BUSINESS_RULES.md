# Business Rules — Assignment & Submission Management System

> **Source of truth:** `docs/PRD.md` (Sections 3, 4, 13). This document is
> **self-contained**: every rule lists its **Enforcement Layer** and one or more
> **Test Scenario IDs** (resolved in §10) so an engineer can implement *and* test
> it without opening the PRD.
>
> Stack in force: **ASP.NET Core 8 + C#**, **EF Core 8 + Npgsql (PostgreSQL)**,
> **BCrypt**, **JWT HS256**, **xUnit**. Frontend: Next.js App Router + TS.

---

## 1. Purpose & Rule-ID Scheme

This document enumerates every enforceable business rule for the system and binds
each rule to (a) the layer that enforces it and (b) a concrete, unit-testable
scenario. It is the implementation contract for Phases 2–8.

### 1.1 Rule-ID families (reused verbatim from PRD)

| Family | Source | Range | Meaning |
|---|---|---|---|
| `AUTH-NNN` | PRD §3.1 | AUTH-001 … AUTH-007 | Authentication & authorization functional requirements |
| `USER-NNN` | PRD §3.2 | USER-001 … USER-006 | User-management functional requirements |
| `CLASS-NNN` | PRD §3.3 | CLASS-001 … CLASS-009 | Class/course & subject management functional requirements |
| `ASGN-NNN` | PRD §3.4 | ASGN-001 … ASGN-011 | Assignment-management functional requirements |
| `SUB-NNN` | PRD §3.5 | SUB-001 … SUB-012 | Submission-management functional requirements |
| `ADM-NNN` | PRD §3.6 | ADM-001 … ADM-003 | Admin visibility functional requirements |
| `BR-N` | PRD §4 | BR-1 … BR-13 | Cross-cutting business rules |

### 1.2 Test-Scenario-ID scheme

Test scenarios use **`TS-<AREA>-NN`**, where `<AREA>` ∈
{`AUTH`, `USER`, `CLASS`, `ASGN`, `SUB`, `REV`, `ADM`, `CROSS`} and `NN` is a
zero-padded ordinal unique within an area.

Examples: `TS-AUTH-01`, `TS-ASGN-03`, `TS-SUB-04`, `TS-REV-02`.

Every rule row in §2–§9 carries a **Test Scenario IDs** column pointing to one or
more `TS-…` entries; every `TS-…` in §10 lists the **Rule IDs** it verifies,
forming a bidirectional coverage map.

### 1.3 Enforcement-layer vocabulary (used throughout)

| Layer token | Where it lives in the codebase |
|---|---|
| **Endpoint** | ASP.NET Core controller action / minimal-API route (e.g. `POST /api/teacher/assignments`) |
| **Middleware** | JWT authentication middleware pipeline (`AddAuthentication().AddJwtBearer`) |
| **Policy** | Authorization policy + custom `AuthorizationHandler` / `[Authorize(Roles=...)]` |
| **Service** | Application/domain service layer containing business logic (e.g. `AssignmentService`) |
| **EF Core** | `DbContext` — unique indexes, FK constraints, migrations |
| **DTO Validation** | Request model validation (data annotations / FluentValidation) |
| **Seed** | Database seeding (`HasData` / seeder) |
| **Serializer** | Response serialization config that omits `PasswordHash` |

---

## 2. Authentication & Authorization Rules

Implements `AUTH-001…AUTH-007` and `BR-11` (role enforcement on the backend, not
only the UI).

| Rule ID | Description | Enforcement Layer | Test Scenario IDs |
|---|---|---|---|
| AUTH-001 | Users log in with email + password. | Endpoint (`POST /api/auth/login`) + Service (BCrypt verify) | TS-AUTH-01, TS-AUTH-02 |
| AUTH-002 | Authentication uses JWT (HS256). | Endpoint + Middleware (token issue + validate) | TS-AUTH-01, TS-CROSS-03 |
| AUTH-003 | JWT claim set includes the user's `UserRole`. | Service (claims factory) | TS-CROSS-03 |
| AUTH-004 | Every protected endpoint enforces role-based authorization server-side. | Policy (`[Authorize(Roles=...)]`) on each controller | TS-AUTH-03, TS-AUTH-04, TS-AUTH-05 |
| AUTH-005 | Unauthenticated → `401`; authenticated-but-wrong-role → `403`. | Middleware (401) + Policy (403) | TS-AUTH-02, TS-AUTH-04, TS-AUTH-05 |
| AUTH-006 | Passwords are stored hashed (BCrypt), never plaintext. | Service (hash on write) + EF Core (no plaintext column usage) | TS-CROSS-02 |
| AUTH-007 | Demo credentials exist for Admin, Teacher, Student on fresh DB. | Seed | TS-AUTH-01 |
| BR-11 | Role-based access is enforced by the backend API, independent of the frontend UI. | Policy on **every** role-scoped endpoint (`/api/admin/*`, `/api/teacher/*`, `/api/student/*`) | TS-AUTH-03, TS-AUTH-04, TS-AUTH-05 |

**Demo users (seeded):** `admin@example.com` / `admin@123` (Admin) ·
`teacher@example.com` / `teacher@123` (Teacher) ·
`teacher2@example.com` / `teacher@123` (Teacher) ·
`student@example.com` / `student@123` (Student).

---

## 3. User-Management Rules

Implements `USER-001…USER-006`. All write operations are Admin-only (enforced by
`BR-1`/`BR-11`; see §8).

| Rule ID | Description | Enforcement Layer | Test Scenario IDs |
|---|---|---|---|
| USER-001 | Admin can create a user. | Endpoint (`POST /api/admin/users`) + Policy (Admin) | TS-USER-01, TS-AUTH-03 |
| USER-002 | Admin can list users. | Endpoint (`GET /api/admin/users`) + Policy (Admin) | TS-AUTH-03 |
| USER-003 | Admin can update user information. | Endpoint (`PUT /api/admin/users/{id}`) + Service | TS-USER-01 |
| USER-004 | Admin can disable or delete a user. | Endpoint (`DELETE /api/admin/users/{id}`) + Service | TS-USER-01 |
| USER-005 | Each user has exactly one role in `UserRole{Admin,Teacher,Student}`. | DTO Validation (enum) + EF Core (non-nullable column) | TS-USER-02 |
| USER-006 | Email is unique per user. | EF Core (unique index on `Users.Email`) + Service (409 on conflict) | TS-USER-03 |

---

## 4. Class / Subject Management Rules

Implements `CLASS-001…CLASS-009`. Admin-only (BR-1).

| Rule ID | Description | Enforcement Layer | Test Scenario IDs |
|---|---|---|---|
| CLASS-001 | Admin can create classes/courses. | Endpoint (`POST /api/admin/classes`) + Policy (Admin) | TS-CLASS-01 |
| CLASS-002 | Admin can update classes/courses. | Endpoint (`PUT /api/admin/classes/{id}`) | TS-CLASS-01 |
| CLASS-003 | Admin can delete classes/courses. | Endpoint (`DELETE /api/admin/classes/{id}`) | TS-CLASS-01 |
| CLASS-004 | Admin can create subjects. | Endpoint (`POST /api/admin/subjects`) | TS-CLASS-01 |
| CLASS-005 | Admin can update subjects. | Endpoint (`PUT /api/admin/subjects/{id}`) | TS-CLASS-01 |
| CLASS-006 | Admin can delete subjects. | Endpoint (`DELETE /api/admin/subjects/{id}`) | TS-CLASS-01 |
| CLASS-007 | Subjects are associated with classes/courses (`Subjects.ClassId`). | EF Core (FK) + DTO Validation | TS-CLASS-01 |
| CLASS-008 | Admin assigns teachers to subject+class via `TeacherClassSubjects(TeacherId,ClassId,SubjectId)`. | Endpoint (`POST /api/admin/teacher-assignments`) + EF Core (UNIQUE composite) | TS-CLASS-02, TS-CLASS-03 |
| CLASS-009 | Admin enrolls students into classes via `Enrollments(ClassId,StudentId)`. | Endpoint (`POST /api/admin/enrollments`) + EF Core (UNIQUE composite) | TS-CLASS-04, TS-CLASS-05 |

---

## 5. Assignment Rules

Implements `ASGN-001…ASGN-011` and `BR-2`, `BR-3`, `BR-4`, `BR-5`, `BR-9`.

| Rule ID | Description | Enforcement Layer | Test Scenario IDs |
|---|---|---|---|
| ASGN-001 | Teacher can create an assignment. | Endpoint (`POST /api/teacher/assignments`) + Policy (Teacher) | TS-ASGN-01, TS-ASGN-04 |
| ASGN-002 | Assignment requires title, description, deadline, and maximum marks. | DTO Validation (required fields) | TS-ASGN-04 |
| ASGN-003 | Assignment is bound to exactly one class/course and one subject. | DTO Validation + EF Core (non-null FKs) | TS-ASGN-01 |
| ASGN-004 | Teacher can update an assignment **they created**. | Endpoint (`PUT /api/teacher/assignments/{id}`) + Service (ownership check) | TS-ASGN-05, TS-ASGN-06 |
| ASGN-005 | Teacher can delete an assignment **they created**. | Endpoint (`DELETE /api/teacher/assignments/{id}`) + Service (ownership check) | TS-ASGN-06 |
| ASGN-006 | Teacher can publish an assignment (`Draft → Published`). | Endpoint (`POST /api/teacher/assignments/{id}/publish`) + Service | TS-ASGN-03, TS-ASGN-09 |
| ASGN-007 | Teacher can keep an assignment as `Draft` on create. | Service (default `Status=Draft`) | TS-ASGN-02 |
| ASGN-008 | `Draft` assignments are invisible to students. | Service (student query filter `Status=Published`) | TS-ASGN-02, TS-SUB-05 |
| ASGN-009 | `Published` assignments are visible only to students enrolled in the assignment's class. | Service (join `Enrollments` by `StudentId`+`ClassId`) | TS-ASGN-03, TS-SUB-09 |
| ASGN-010 | Deadline is stored in UTC (`DeadlineUtc`). | DTO Validation (kind=UTC) + EF Core (column) | TS-ASGN-08 |
| ASGN-011 | `MaxMarks > 0`. | DTO Validation (`Range(1, int.MaxValue)`) | TS-ASGN-04 |
| BR-2 | Only Teachers create/manage assignments (not Admin, not Student). | Policy (`[Authorize(Roles="Teacher")]`) | TS-ASGN-01, TS-AUTH-05 |
| BR-3 | A teacher may create assignments **only** for `(ClassId, SubjectId)` rows that exist for them in `TeacherClassSubjects`. | Service (lookup `TeacherClassSubjects` by `TeacherId,ClassId,SubjectId`) | TS-ASGN-01 |
| BR-4 | Draft assignments are not visible to students. | Service (see ASGN-008) | TS-ASGN-02 |
| BR-5 | Published assignments are visible only to enrolled students. | Service (see ASGN-009) | TS-ASGN-03, TS-SUB-09 |
| BR-9 (assignment side) | A teacher manages/reviews only assignments they own. | Service (ownership check on update/delete/publish/list) | TS-ASGN-06, TS-REV-04 |

**Field contract:** `Assignments(TeacherId creator, ClassId, SubjectId, MaxMarks int>0, DeadlineUtc UTC, Status Draft|Published, AllowResubmission default true)`.

---

## 6. Submission Rules

Implements `SUB-001…SUB-012` and `BR-6`, `BR-7`, `BR-8`. Review-only entries
(`SUB-009..012`) are detailed in §7.

| Rule ID | Description | Enforcement Layer | Test Scenario IDs |
|---|---|---|---|
| SUB-001 | Student can submit an answer only for a `Published` assignment. | Service (status check) | TS-SUB-01, TS-SUB-05 |
| SUB-002 | Student can view assignment details before submitting. | Endpoint (`GET /api/student/assignments/{id}`) + Service (enrolled + published) | TS-SUB-09 |
| SUB-003 | Student can update a submission before the deadline **iff** `Assignment.AllowResubmission == true`. | Service (`AllowResubmission` gate + UTC compare) | TS-SUB-03, TS-SUB-10 |
| SUB-004 | Student cannot submit after the deadline. | Service (`DateTime.UtcNow > DeadlineUtc` → reject) | TS-SUB-02 |
| SUB-005 | Student can view their own submission status. | Endpoint (`GET /api/student/submissions/{id}`) + Service (owner filter) | TS-SUB-06 |
| SUB-006 | Student can view marks/feedback only after review (status ∈ {Reviewed, LateSubmitted-reviewed}). | Service (project `Marks`/`Feedback` only when reviewed) | TS-REV-05 |
| SUB-007 | Student can see **only their own** submissions. | Service (`WHERE StudentId = current user`) | TS-SUB-06 |
| SUB-008 | Teacher can view submissions only for **their own** assignments. | Endpoint (`GET /api/teacher/assignments/{assignmentId}/submissions`) + Service (ownership) | TS-REV-01, TS-REV-04 |
| SUB-009..012 | Marks/feedback/status — see §7. | — | — |
| BR-6 | Students submit **only before** the deadline (UTC). | Service (UTC deadline compare) | TS-SUB-02 |
| BR-7 | Students update **only before** the deadline **and only if** `AllowResubmission`. | Service (deadline + `AllowResubmission`) | TS-SUB-03, TS-SUB-04, TS-SUB-10 |
| BR-8 | A student cannot view another student's submission. | Service (owner filter + 404/403 on mismatch) | TS-SUB-06 |

**Derived constraints (service-enforced, model-implied):**
- One submission per `(AssignmentId, StudentId)` → enforced by EF Core
  `UNIQUE(AssignmentId, StudentId)` (see §9); a second create → `409`.
- Cannot submit to a `Draft` assignment (SUB-001).
- Student must be enrolled in the assignment's class to submit (SUB-002/ASGN-009).

---

## 7. Review Rules

Implements `SUB-009`, `SUB-010`, `SUB-011`, `SUB-012` and `BR-9`, `BR-10`.

| Rule ID | Description | Enforcement Layer | Test Scenario IDs |
|---|---|---|---|
| SUB-009 | Teacher can assign marks to a submission for **their own** assignment. | Endpoint (`PUT /api/teacher/submissions/{submissionId}/review`) + Service (ownership) | TS-REV-01, TS-REV-04 |
| SUB-010 | Teacher can provide feedback (optional). | DTO Validation (optional field) | TS-REV-05 |
| SUB-011 | Teacher can change submission status; transitions follow `Submitted → UnderReview → Reviewed`. | Service (state-machine) | TS-REV-06 |
| SUB-012 | `0 ≤ Marks ≤ Assignment.MaxMarks`. | DTO Validation (`Range(0, MaxMarks)`) + Service (re-check) | TS-REV-02, TS-REV-03 |
| BR-9 (review side) | A teacher reviews **only** submissions for assignments they own/responsible for. | Service (resolve assignment → `TeacherId == current user`) | TS-REV-04 |
| BR-10 | Marks are in `[0, MaxMarks]`. | Service + DTO Validation | TS-REV-02, TS-REV-03 |

**Status semantics (enum `SubmissionStatus`):**
`Submitted` (created before deadline) · `UnderReview` (teacher opened review) ·
`Reviewed` (marks finalized) · `LateSubmitted` (a submission that was created
late relative to an extended window — treated as reviewed-eligible but flagged).

**On write:** `Submissions.Marks` is `int?` in `[0, MaxMarks]`; `ReviewedByTeacherId`
and `ReviewedAtUtc` are stamped by the service at review time.

---

## 8. Admin Visibility Rules

Implements `ADM-001…ADM-003` and `BR-1`.

| Rule ID | Description | Enforcement Layer | Test Scenario IDs |
|---|---|---|---|
| ADM-001 | Admin can view **all** assignments (any status, any teacher). | Endpoint (`GET /api/admin/assignments`) + Policy (Admin) + Service (no owner/status filter) | TS-ADM-01 |
| ADM-002 | Admin can view **all** submissions (any student, any assignment). | Endpoint (`GET /api/admin/submissions`) + Policy (Admin) + Service (no owner filter) | TS-ADM-02 |
| ADM-003 | Admin visibility is **not** limited by teacher ownership rules or enrollment rules. | Service (bypass teacher/enrollment filters for Admin context) | TS-ADM-03 |
| BR-1 | Only Admin manages users, classes, subjects, and teacher assignments. | Policy (`[Authorize(Roles="Admin")]`) on `/api/admin/*` | TS-AUTH-03, TS-AUTH-04, TS-USER-01 |

**Admin does NOT create assignments or grade submissions** (no assignment-create
or review endpoint is exposed to Admin; Admin endpoints are read-only for
assignments/submissions). This is enforced by **Policy**: only
`/api/admin/assignments` (GET) and `/api/admin/submissions` (GET) exist for the
Admin role.

---

## 9. Cross-Cutting Rules

| Rule ID | Description | Enforcement Layer | Test Scenario IDs |
|---|---|---|---|
| BR-12 | Deadlines are compared using **UTC** time (`DateTime.UtcNow` vs `DeadlineUtc`); no local-time comparison anywhere. | Service (all deadline checks) | TS-CROSS-01 |
| BR-13 | `PasswordHash` is never serialized into any API response (login, user list, me, etc.). | Serializer (DTO mapping omits hash) + Service | TS-CROSS-02 |
| DI-1 | `TeacherClassSubjects(TeacherId, ClassId, SubjectId)` is UNIQUE — no duplicate assignment of the same teacher to the same class+subject. | EF Core (unique index) | TS-CLASS-03 |
| DI-2 | `Enrollments(ClassId, StudentId)` is UNIQUE — a student is enrolled in a class at most once. | EF Core (unique index) | TS-CLASS-05 |
| DI-3 | `Submissions(AssignmentId, StudentId)` is UNIQUE — one submission per (assignment, student). | EF Core (unique index) + Service (409) | TS-SUB-08 |
| DI-4 | `Users.Email` is UNIQUE. | EF Core (unique index) | TS-USER-03 |
| DI-5 | All FK relationships are enforced (`Assignments→Users/Classes/Subjects`, `Submissions→Assignments/Users`, `Enrollments→Classes/Users`, `TeacherClassSubjects→Users/Classes/Subjects`). | EF Core (FK constraints + migrations) | TS-CLASS-01 |
| LOG-1 | Failed login attempts and unhandled exceptions are logged; passwords and full JWTs are never logged. | Middleware/Service (logging filters) | TS-AUTH-02 |

---

## 10. Test Scenario Catalog (Given / When / Then)

Each scenario is concrete enough to implement as an xUnit test in Phase 8 (or
incrementally in Phases 2–5). Column **Verifies** maps back to the rule families
above and to PRD §13.1–§13.4.

### 10.1 Authentication & Authorization (`TS-AUTH-*`) — PRD §13.1

1. **TS-AUTH-01** — *Valid login returns a JWT.*
   - **Given** the seed DB with `teacher@example.com` / `teacher@123`.
   - **When** `POST /api/auth/login` with those credentials.
   - **Then** response is `200` with a non-empty `token` that decodes to a JWT
     whose `role` claim equals `Teacher`.
   - **Verifies:** AUTH-001, AUTH-002, AUTH-003, AUTH-007, BR-11. *(PRD §13.1)*

2. **TS-AUTH-02** — *Invalid login returns 401.*
   - **Given** the seed DB.
   - **When** `POST /api/auth/login` with `teacher@example.com` / `wrong-password`.
   - **Then** response is `401` (no token); a failed-login event is logged.
   - **Verifies:** AUTH-001, AUTH-005, LOG-1. *(PRD §13.1)*

3. **TS-AUTH-03** — *Admin can access admin endpoints.*
   - **Given** a logged-in Admin (`admin@example.com`).
   - **When** `GET /api/admin/users` with the Admin JWT.
   - **Then** response is `200` returning the user list.
   - **Verifies:** AUTH-004, AUTH-005, BR-1, BR-11, USER-002. *(PRD §13.1)*

4. **TS-AUTH-04** — *Teacher cannot access admin endpoints.*
   - **Given** a logged-in Teacher.
   - **When** `GET /api/admin/users` with the Teacher JWT.
   - **Then** response is `403`.
   - **Verifies:** AUTH-004, AUTH-005, BR-1, BR-11. *(PRD §13.1)*

5. **TS-AUTH-05** — *Student cannot access teacher endpoints.*
   - **Given** a logged-in Student.
   - **When** `POST /api/teacher/assignments` with the Student JWT.
   - **Then** response is `403`.
   - **Verifies:** AUTH-004, AUTH-005, BR-2, BR-11. *(PRD §13.1)*

### 10.2 User Management (`TS-USER-*`)

6. **TS-USER-01** — *Admin CRUD on users.*
   - **Given** a logged-in Admin.
   - **When** create (`POST /api/admin/users`), update (`PUT`), delete (`DELETE`)
     a user.
   - **Then** create → `201`; update → `200`; delete → `204`; subsequent get →
     `404`.
   - **Verifies:** USER-001, USER-002, USER-003, USER-004.

7. **TS-USER-02** — *Role must be a valid `UserRole`.*
   - **Given** a logged-in Admin.
   - **When** `POST /api/admin/users` with `role="SuperUser"`.
   - **Then** response is `400`.
   - **Verifies:** USER-005.

8. **TS-USER-03** — *Duplicate email returns 409.*
   - **Given** a user `a@x.com` already exists.
   - **When** Admin creates another user with email `a@x.com`.
   - **Then** response is `409`.
   - **Verifies:** USER-006, DI-4.

### 10.3 Class / Subject Management (`TS-CLASS-*`)

9. **TS-CLASS-01** — *Admin manages classes, subjects, and subject↔class link.*
   - **Given** a logged-in Admin.
   - **When** create/update/delete a class; create a subject bound to a class.
   - **Then** operations succeed (201/200/204) and the subject's `ClassId` FK is
     enforced (creating a subject with a non-existent `ClassId` → `400`/`404`).
   - **Verifies:** CLASS-001…CLASS-007, DI-5.

10. **TS-CLASS-02** — *Admin assigns a teacher to a class+subject.*
    - **Given** existing Teacher, Class, Subject.
    - **When** `POST /api/admin/teacher-assignments`
      `{teacherId,classId,subjectId}`.
    - **Then** response is `201`; a `TeacherClassSubjects` row exists.
    - **Verifies:** CLASS-008.

11. **TS-CLASS-03** — *Duplicate teacher assignment is rejected.*
    - **Given** a `TeacherClassSubjects(T1,C1,S1)` row exists.
    - **When** Admin inserts the same `(T1,C1,S1)` again.
    - **Then** response is `409` (unique constraint).
    - **Verifies:** CLASS-008, DI-1.

12. **TS-CLASS-04** — *Admin enrolls a student into a class.*
    - **Given** existing Student and Class.
    - **When** `POST /api/admin/enrollments` `{classId,studentId}`.
    - **Then** response is `201`.
    - **Verifies:** CLASS-009.

13. **TS-CLASS-05** — *Duplicate enrollment is rejected.*
    - **Given** `Enrollments(C1,Stu1)` exists.
    - **When** Admin enrolls `Stu1` into `C1` again.
    - **Then** response is `409`.
    - **Verifies:** CLASS-009, DI-2.

### 10.4 Assignment Rules (`TS-ASGN-*`) — PRD §13.2

14. **TS-ASGN-01** — *Teacher cannot create assignment for unassigned class/subject.*
    - **Given** Teacher `T1` has `TeacherClassSubjects` only for `(C1,S1)`.
    - **When** `T1` calls `POST /api/teacher/assignments` with `(C2,S2)` (not
      assigned).
    - **Then** response is `403` (or `400`); no assignment row is created. A
      parallel call with `(C1,S1)` succeeds with `201`.
    - **Verifies:** ASGN-001, ASGN-003, BR-3. *(PRD §13.2)*

15. **TS-ASGN-02** — *Draft assignment is invisible to students.*
    - **Given** a `Draft` assignment in class `C1`, student `Stu1` enrolled in
      `C1`.
    - **When** `Stu1` calls `GET /api/student/assignments`.
    - **Then** the draft is absent from the list; `GET /api/student/assignments/{id}`
      → `404`.
    - **Verifies:** ASGN-007, ASGN-008, BR-4. *(PRD §13.2)*

16. **TS-ASGN-03** — *Published assignment visible only to enrolled students.*
    - **Given** a `Published` assignment in class `C1`; `Stu1` enrolled in `C1`,
      `Stu2` not enrolled.
    - **When** `Stu1` and `Stu2` each call `GET /api/student/assignments`.
    - **Then** `Stu1` sees it; `Stu2` does not.
    - **Verifies:** ASGN-006, ASGN-009, BR-5. *(PRD §13.2)*

17. **TS-ASGN-04** — *MaxMarks must be greater than zero (and required fields).*
    - **Given** a logged-in Teacher with a valid `(ClassId,SubjectId)` assignment.
    - **When** `POST /api/teacher/assignments` with `MaxMarks = 0` (and again with
      `MaxMarks = -5`).
    - **Then** both return `400`; a call with `MaxMarks = 100` and all required
      fields returns `201`.
    - **Verifies:** ASGN-002, ASGN-011. *(PRD §13.2)*

18. **TS-ASGN-05** — *Teacher updates own assignment.*
    - **Given** assignment `A1` created by `T1`, still `Draft`.
    - **When** `T1` calls `PUT /api/teacher/assignments/{A1}`.
    - **Then** response is `200`; fields are updated.
    - **Verifies:** ASGN-004, BR-9.

19. **TS-ASGN-06** — *Teacher cannot update/delete another teacher's assignment.*
    - **Given** assignment `A1` created by `T1`.
    - **When** `T2` calls `PUT /api/teacher/assignments/{A1}` (and `DELETE`).
    - **Then** both return `403` (or `404`); `A1` is unchanged.
    - **Verifies:** ASGN-004, ASGN-005, BR-2, BR-9.

20. **TS-ASGN-08** — *Deadline stored and compared in UTC.*
    - **Given** a teacher creating an assignment.
    - **When** create with `DeadlineUtc = 2026-12-31T23:59:59Z`.
    - **Then** the persisted value equals the UTC instant; a read returns the same
      UTC instant.
    - **Verifies:** ASGN-010, BR-12.

21. **TS-ASGN-09** — *Publish transitions Draft → Published.*
    - **Given** a `Draft` assignment owned by `T1`.
    - **When** `T1` calls `POST /api/teacher/assignments/{id}/publish`.
    - **Then** status becomes `Published`; an enrolled student can now see it.
    - **Verifies:** ASGN-006.

### 10.5 Submission Rules (`TS-SUB-*`) — PRD §13.3

22. **TS-SUB-01** — *Student can submit before deadline.*
    - **Given** a `Published` assignment `A1` in `C1` with `DeadlineUtc` in the
      future; `Stu1` enrolled in `C1`; `AllowResubmission = true`.
    - **When** `Stu1` calls `POST /api/student/assignments/{A1}/submit` with
      `AnswerText`.
    - **Then** response is `201`; a `Submissions` row exists with
      `Status = Submitted`, `SubmittedAtUtc ≤ DeadlineUtc`.
    - **Verifies:** SUB-001, BR-6. *(PRD §13.3)*

23. **TS-SUB-02** — *Student cannot submit after deadline.*
    - **Given** a `Published` assignment with `DeadlineUtc` in the past.
    - **When** `Stu1` submits.
    - **Then** response is `400` (or `403`); no row created.
    - **Verifies:** SUB-004, BR-6, BR-12. *(PRD §13.3)*

24. **TS-SUB-03** — *Student can update submission before deadline if allowed.*
    - **Given** an existing submission for `Stu1`; deadline in the future;
      `AllowResubmission = true`.
    - **When** `Stu1` calls `PUT /api/student/submissions/{id}`.
    - **Then** response is `200`; `AnswerText` and `UpdatedAtUtc` change.
    - **Verifies:** SUB-003, BR-7. *(PRD §13.3)*

25. **TS-SUB-04** — *Student cannot update submission after deadline.*
    - **Given** an existing submission; deadline in the past.
    - **When** `Stu1` calls `PUT /api/student/submissions/{id}`.
    - **Then** response is `400` (or `403`); row unchanged.
    - **Verifies:** SUB-003, SUB-004, BR-7. *(PRD §13.3)*

26. **TS-SUB-05** — *Student cannot submit to a draft assignment.*
    - **Given** assignment `A1` is `Draft`; `Stu1` enrolled.
    - **When** `Stu1` submits to `A1`.
    - **Then** response is `404` (assignment not visible) or `400`; no row created.
    - **Verifies:** SUB-001, ASGN-008, BR-4. *(PRD §13.3)*

27. **TS-SUB-06** — *Student cannot view another student's submission.*
    - **Given** `Stu1` has a submission `S1`; `Stu2` is a different student.
    - **When** `Stu2` calls `GET /api/student/submissions/{S1}`.
    - **Then** response is `403` (or `404`); `Stu2` never sees `S1` in their list.
    - **Verifies:** SUB-005, SUB-007, BR-8. *(PRD §13.3)*

28. **TS-SUB-08** — *One submission per (assignment, student).*
    - **Given** `Stu1` already submitted to `A1`.
    - **When** `Stu1` submits to `A1` again (before deadline).
    - **Then** response is `409`; no duplicate row.
    - **Verifies:** DI-3.

29. **TS-SUB-09** — *Student must be enrolled to see/submit.*
    - **Given** `Published` assignment in `C1`; `Stu2` NOT enrolled in `C1`.
    - **When** `Stu2` calls `GET /api/student/assignments/{A1}` and submits.
    - **Then** get → `404`; submit → `403`/`400`.
    - **Verifies:** SUB-002, ASGN-009, BR-5.

30. **TS-SUB-10** — *Update blocked when AllowResubmission is false.*
    - **Given** an existing submission; `Assignment.AllowResubmission = false`;
      deadline in the future.
    - **When** `Stu1` calls `PUT /api/student/submissions/{id}`.
    - **Then** response is `403` (or `400`); row unchanged.
    - **Verifies:** SUB-003, BR-7.

### 10.6 Review Rules (`TS-REV-*`) — PRD §13.4

31. **TS-REV-01** — *Teacher can review submissions for their own assignment.*
    - **Given** submission `S1` on assignment `A1` owned by `T1`.
    - **When** `T1` calls `PUT /api/teacher/submissions/{S1}/review`
      `{marks: 80, feedback: "Good"}`.
    - **Then** response is `200`; `Marks=80`, `Feedback` set,
      `ReviewedByTeacherId=T1`, status transitions toward `Reviewed`.
    - **Verifies:** SUB-008, SUB-009, SUB-011, BR-9. *(PRD §13.4)*

32. **TS-REV-02** — *Marks cannot be negative.*
    - **Given** submission `S1` on `A1` (MaxMarks=100) owned by `T1`.
    - **When** `T1` reviews with `marks = -1`.
    - **Then** response is `400`; `Marks` remains null/unchanged.
    - **Verifies:** SUB-012, BR-10. *(PRD §13.4)*

33. **TS-REV-03** — *Marks cannot exceed MaxMarks.*
    - **Given** submission on `A1` with `MaxMarks = 100`.
    - **When** `T1` reviews with `marks = 101`.
    - **Then** response is `400`; `marks = 100` is accepted (boundary), `101`
      rejected.
    - **Verifies:** SUB-012, BR-10. *(PRD §13.4)*

34. **TS-REV-04** — *Teacher cannot review another teacher's submission.*
    - **Given** submission `S1` on `A1` owned by `T1`.
    - **When** `T2` calls `PUT /api/teacher/submissions/{S1}/review`.
    - **Then** response is `403` (or `404`); `S1` unchanged.
    - **Verifies:** SUB-008, SUB-009, BR-9.

35. **TS-REV-05** — *Feedback optional; student sees marks/feedback after review.*
    - **Given** `T1` reviews `S1` with `{marks: 70}` (no feedback).
    - **When** review succeeds, then `Stu1` reads `GET /api/student/submissions/{S1}`.
    - **Then** review → `200` with feedback null; student sees `Marks=70` (feedback
      null) only because status is `Reviewed`.
    - **Verifies:** SUB-006, SUB-010.

36. **TS-REV-06** — *Status transitions Submitted → UnderReview → Reviewed.*
    - **Given** submission `S1` with `Status = Submitted`.
    - **When** `T1` opens review (status → `UnderReview`), then finalizes marks
      (status → `Reviewed`).
    - **Then** each transition is persisted; an invalid jump
      (e.g. `Reviewed → Submitted`) is rejected by the service.
    - **Verifies:** SUB-011.

### 10.7 Admin Visibility (`TS-ADM-*`) — PRD §13.4 (admin views all)

37. **TS-ADM-01** — *Admin views all assignments.*
    - **Given** assignments across teachers and statuses (Draft + Published).
    - **When** Admin calls `GET /api/admin/assignments`.
    - **Then** all assignments are returned regardless of owner/status.
    - **Verifies:** ADM-001, BR-1. *(PRD §13.4)*

38. **TS-ADM-02** — *Admin views all submissions.*
    - **Given** submissions by multiple students on multiple assignments.
    - **When** Admin calls `GET /api/admin/submissions`.
    - **Then** all submissions are returned regardless of student/owner.
    - **Verifies:** ADM-002. *(PRD §13.4)*

39. **TS-ADM-03** — *Admin visibility not limited by teacher rules.*
    - **Given** assignment `A1` owned by `T1` and a submission on it by `Stu1`.
    - **When** Admin reads `A1` and the submission.
    - **Then** both are visible to Admin despite Admin being neither owner nor
      enrolled student.
    - **Verifies:** ADM-003.

40. **TS-ADM-04** — *Admin does not create/grade (no endpoints).*
    - **Given** a logged-in Admin.
    - **When** Admin calls `POST /api/teacher/assignments` or
      `PUT /api/teacher/submissions/{id}/review`.
    - **Then** response is `403` (role mismatch); Admin has only GET on
      assignments/submissions.
    - **Verifies:** BR-1, BR-2, ADM-001, ADM-002.

### 10.8 Cross-Cutting (`TS-CROSS-*`)

41. **TS-CROSS-01** — *UTC deadline comparison is timezone-stable.*
    - **Given** an assignment with `DeadlineUtc = 2026-12-31T23:59:59Z`.
    - **When** the service compares `DateTime.UtcNow` against `DeadlineUtc` under
      different server local timezones.
    - **Then** the before/after verdict is identical across timezones (no
      `DateTime.Now` used).
    - **Verifies:** ASGN-010, BR-12.

42. **TS-CROSS-02** — *Password hash never exposed.*
    - **Given** any authenticated user.
    - **When** calling `POST /api/auth/login`, `GET /api/auth/me`,
      `GET /api/admin/users`, `GET /api/admin/users/{id}`.
    - **Then** no response body contains a `passwordHash` / `PasswordHash` field.
    - **Verifies:** AUTH-006, BR-13.

43. **TS-CROSS-03** — *JWT carries the role claim.*
    - **Given** a token obtained by logging in as each demo role.
    - **When** decoding the JWT payload.
    - **Then** a `role` claim equals the user's `UserRole`
      (`Admin`/`Teacher`/`Student`).
    - **Verifies:** AUTH-002, AUTH-003.

---

### Coverage matrix summary (PRD §13.1–§13.4 → scenarios)

| PRD §13 requirement | Scenario(s) |
|---|---|
| §13.1 Valid login → JWT | TS-AUTH-01 |
| §13.1 Invalid login → 401 | TS-AUTH-02 |
| §13.1 Admin access | TS-AUTH-03 |
| §13.1 Teacher blocked from admin | TS-AUTH-04 |
| §13.1 Student blocked from teacher | TS-AUTH-05 |
| §13.2 Teacher cannot create for unassigned class/subject | TS-ASGN-01 |
| §13.2 Draft invisible to students | TS-ASGN-02 |
| §13.2 Published visible to enrolled | TS-ASGN-03 |
| §13.2 MaxMarks > 0 | TS-ASGN-04 |
| §13.3 Submit before deadline | TS-SUB-01 |
| §13.3 Submit blocked after deadline | TS-SUB-02 |
| §13.3 Update before deadline | TS-SUB-03 |
| §13.3 Update blocked after deadline | TS-SUB-04 |
| §13.3 Cannot submit to draft | TS-SUB-05 |
| §13.3 Cannot view other student's submission | TS-SUB-06 |
| §13.4 Teacher reviews own | TS-REV-01 |
| §13.4 Marks not negative | TS-REV-02 |
| §13.4 Marks ≤ max | TS-REV-03 |
| §13.4 Admin views all | TS-ADM-01, TS-ADM-02, TS-ADM-03 |
