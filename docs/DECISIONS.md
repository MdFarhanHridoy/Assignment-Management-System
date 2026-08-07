# Design Decisions — Assignment & Submission Management System

> **Phase 0 artifact.** Records the architectural and design decisions (ADR-lite) plus
> the explicit assumptions and known limitations the project ships with, so any agent
> or reviewer understands *why* the system is shaped the way it is.
>
> **Authoritative inputs:** the canonical project contract (stack, enums, entities,
> rules, JWT, demo users) and `docs/PRD.md`. Contract names below are used **verbatim**;
> do not rename. Companion documents: `docs/PRD.md` (read-only requirements),
> `docs/DATABASE_SCHEMA.md`, `docs/AUTH_MODEL.md`, `docs/API_CONTRACT.md`,
> `docs/BUSINESS_RULES.md`.

---

## 1. Purpose

This document captures the *why* behind the implementation, not the *what* (the *what*
lives in `PRD.md` and the sibling contract documents). For each decision it records the
**Context** (the problem or constraint), the **Decision** (the chosen direction), and the
**Consequences** (what we gain and what we give up or must handle later). It also
enumerates the **Assumptions** (mapping to PRD §16) and **Known Limitations** (mapping to
PRD §17) that bound v1.

The goal is twofold:

1. **Continuity** — any agent/p reviewer or future contributor can reconstruct the
   reasoning without re-deriving it from the code or the PRD.
2. **Auditability** — every contract-level choice (PostgreSQL over MongoDB, `int` marks,
   one submission per `(AssignmentId, StudentId)`, post-deadline blocking, admin
   read-only, etc.) is traceable to a single record here.

All entity and enum names below match the contract exactly: `User`, `Class`, `Subject`,
`TeacherClassSubject`, `Enrollment`, `Assignment`, `Submission`; enums `UserRole{Admin,
Teacher, Student}`, `AssignmentStatus{Draft, Published, Archived}`,
`SubmissionStatus{Submitted, UnderReview, Reviewed, LateSubmitted}`.

---

## 2. Decision Records (ADR-lite)

Each record has **Context**, **Decision**, and **Consequences**.

---

### D-01 — Monorepo (single repository: `client/` + `server/`)

- **Context.** The project is a full-stack web application (ASP.NET Core 8 backend +
  Next.js 14 frontend) that must be evaluated as one cohesive deliverable. PRD §14.2
  requires "frontend code, backend/API code, database files, unit tests" in a single
  submission, and PRD §14.1 requires one repository link.
- **Decision.** Use **one repository** containing two top-level apps: `client/`
  (Next.js) and `server/` (ASP.NET Core), alongside shared `docs/` and `plans/` folders.
  There is no polyrepo split and no separate frontend/backend git remotes.
- **Consequences.**
  - (+) Single clone, single history, simpler evaluation and local setup (PRD §14.7).
  - (+) Shared documentation (`docs/`) stays in sync across client and server.
  - (+) One PR/issue can touch both ends for a cross-cutting change.
  - (−) No independent per-app release cadence; acceptable for a recruitment project.
  - (−) CI/build tooling must distinguish the two stacks (`.NET` vs `node`).

---

### D-02 — Relational database (PostgreSQL) over MongoDB

- **Context.** PRD §5.3 offers PostgreSQL *or* MongoDB and explicitly *recommends*
  PostgreSQL "because the domain contains relational data such as users, classes,
  subjects, assignments, and submissions."
- **Decision.** Use **PostgreSQL** (14+) as the sole persistence store. The domain is
  intrinsically relational: `TeacherClassSubject` is a 3-way join (`TeacherId`,
  `ClassId`, `SubjectId`), `Enrollments` is a 2-way join, `Assignments` → `Submissions`
  is a strict parent/child aggregate, and uniqueness invariants
  (`UNIQUE(AssignmentId, StudentId)`, `UNIQUE(ClassId, StudentId)`,
  `UNIQUE(TeacherId, ClassId, SubjectId)`, `UNIQUE(Email)`) are first-class.
- **Consequences.**
  - (+) Foreign keys, unique constraints, and `CHECK` constraints express the integrity
    invariants directly (e.g. `MaxMarks > 0`, `Marks IS NULL OR Marks >= 0`).
  - (+) Transactional consistency across multi-table writes (create user + seed rows).
  - (−) No document flexibility; acceptable — the schema is stable and small.
  - (−) Requires migration tooling (see D-03) and a running Postgres instance.

---

### D-03 — EF Core code-first with migrations (no manual table creation)

- **Context.** PRD §5.3 mandates: "The evaluator should be able to set up the database
  without manually creating tables or collections," "include migration files," and
  "include seed/sample data."
- **Decision.** Model the schema **code-first** with **EF Core 8 + Npgsql**. Entity
  classes own the schema; a single **InitialCreate** migration is generated via
  `dotnet ef migrations add` and applied with `dotnet ef database update` (or
  `context.Database.MigrateAsync()` on startup). A dedicated, idempotent `DataSeeder`
  runs after migration to insert the demo users and sample rows.
- **Consequences.**
  - (+) Zero manual SQL for setup; first run auto-applies pending migrations.
  - (+) Schema lives in code, is diffable, and round-trips with the model snapshot.
  - (+) Seed data is deterministic and restart-safe (inserts only when absent).
  - (−) Migration authoring discipline required (never hand-edit applied migrations).
  - (−) `CHECK` constraints that cross tables (`Marks <= Assignment.MaxMarks`) cannot be
    expressed as a single-row Postgres CHECK, so that rule is enforced in the
    application/validation layer (see D-04).

---

### D-04 — Marks stored as `int` (not `decimal`/`float`)

- **Context.** PRD rules 10 and 11 require `0 ≤ Marks ≤ MaxMarks` and `MaxMarks > 0`.
  School marks are conventionally whole numbers. Floating-point comparison would risk
  off-by-epsilon boundary bugs (e.g. is `99.9999999` equal to `100`?).
- **Decision.** Model `Assignment.MaxMarks` as `int` with `CHECK MaxMarks > 0`, and
  `Submission.Marks` as `int?` with `0 ≤ Marks ≤ MaxMarks` enforced in the
  application/validation layer (cross-table rule, see D-03 consequence).
- **Consequences.**
  - (+) Exact integer comparison — no float-rounding or epsilon tolerance needed; the
    boundary cases `Marks == MaxMarks` (accepted) and `Marks == MaxMarks + 1`
    (rejected) are unambiguous and xUnit-testable.
  - (+) Simpler DTOs, validation (`Range`), and JSON serialization.
  - (−) No fractional marks (e.g. `87.5`). Acceptable for v1; would require a schema
    migration to `numeric` if ever needed.

---

### D-05 — One submission per `(AssignmentId, StudentId)` via UNIQUE constraint

- **Context.** A student should have exactly one answer per assignment; multiple
  "submissions" would complicate grading, status, and the marks/feedback model.
- **Decision.** Enforce **one row per `(AssignmentId, StudentId)`** with a database
  unique constraint `UX_Submissions_AssignmentId_StudentId`. A re-submit **updates the
  same row** rather than inserting a new one: `SubmittedAtUtc` is fixed at first submit,
  and `UpdatedAtUtc` advances on each permitted update (see D-06).
- **Consequences.**
  - (+) Single source of truth for a student's answer, marks, feedback, and status.
  - (+) The grade book is unambiguous (one row per student per assignment).
  - (+) Attempting to insert a second row yields `409 Conflict` (DI-3 / TS-SUB-08).
  - (−) `submit` on an existing row must behave as an upsert when resubmission is
    allowed (documented in the API contract), which is slightly non-RESTful.

---

### D-06 — Resubmission policy: `AllowResubmission` flag, gated by deadline

- **Context.** PRD SUB-003/BR-7: "Student can update a submission before the deadline,
  **if allowed**." The "if allowed" qualifier needs a concrete, per-assignment control.
- **Decision.** Add an `AllowResubmission` boolean to `Assignment` (default `true`).
  Updates to an existing submission are permitted **only** when **both** conditions hold:
  (a) the deadline has not passed (`DateTime.UtcNow < DeadlineUtc`, compared in UTC per
  D-07), **and** (b) `Assignment.AllowResubmission == true`. Otherwise the update is
  rejected (post-deadline → blocked per D-09; `AllowResubmission == false` → `409`/`403`).
- **Consequences.**
  - (+) Teachers control whether students can revise, per assignment.
  - (+) The two independent gates (deadline + flag) are each independently testable
    (TS-SUB-03, TS-SUB-04, TS-SUB-10).
  - (−) Two failure paths to document and message distinctly.

---

### D-07 — Deadlines stored and compared in UTC (`DateTimeKind.Utc`)

- **Context.** PRD ASGN-010/BR-12: "Deadline is stored in UTC" and "Deadlines should be
  compared using UTC time." Local-time comparisons would silently break when the server's
  timezone or DST changes.
- **Decision.** Store `Assignment.DeadlineUtc` and all `*Utc` submission columns as
  `DateTime` with `DateTimeKind.Utc`, mapped to PostgreSQL `timestamptz`. All deadline
  checks compare `DateTime.UtcNow` against `DeadlineUtc` — **never** `DateTime.Now`.
- **Consequences.**
  - (+) Timezone-stable verdicts; verified by TS-CROSS-01 (before/after identical across
    server timezones).
  - (+) The client is responsible for display formatting; the API exchanges ISO-8601 UTC
    strings (e.g. `2026-08-20T23:59:00Z`).
  - (−) Frontend must convert for display and must send UTC on create; documented as an
    integration concern, not a bug.

---

### D-08 — Draft vs Published visibility; Archived hides from students (future/optional)

- **Context.** PRD ASGN-006/007/008/009 and rules 4/5: a teacher may stage an assignment
  as `Draft`; only `Published` assignments are visible to **enrolled** students. `Archived`
  is in the enum but not required by any v1 scenario.
- **Decision.**
  - New assignments are always created with `Status = Draft` (the `Status` field is **not**
    settable on create; publish happens via `POST /api/teacher/assignments/{id}/publish`).
  - Students can see **only** `Status == Published` assignments for classes they are
    **enrolled** in (`Enrollments` join on `(ClassId, StudentId)`).
  - `Draft` is **never** visible to students (a request for a Draft id → `404`, hiding
    existence).
  - `Archived` is reserved: in v1 it behaves like "no longer published" — it is excluded
    from the student list. Full lifecycle semantics (re-publish, student-facing archive
    view) are future/optional.
- **Consequences.**
  - (+) Clear, testable visibility contract (TS-ASGN-02, TS-ASGN-03, TS-SUB-05).
  - (+) `Archived` is available in the schema/enum without forcing UI work now.
  - (−) `Archived` has minimal v1 behavior; future work item (see §5).

---

### D-09 — `LateSubmitted` status reserved; post-deadline submit/update is BLOCKED in v1

- **Context.** PRD rule 6 / SUB-004: "Students can submit only before the assignment
  deadline." There is no grace window in the requirements. The enum nonetheless includes
  `LateSubmitted`, which must be given a clear meaning to avoid ambiguity.
- **Decision.** In **v1, both** submitting **and** updating after the deadline are
  **BLOCKED** (the service rejects with `400`/`403`; no row is created/changed —
  TS-SUB-02, TS-SUB-04). The `LateSubmitted` status is therefore **reserved for a future
  optional "grace window"** and is **not assigned by any v1 code path**. If a grace window
  is ever introduced, a submission accepted inside the grace interval (deadline ≤ now <
  deadline + grace) would be stamped `LateSubmitted` and remain review-eligible.
- **Consequences.**
  - (+) No ambiguous "late" state in v1; the rule is binary (before = allowed,
    at/after = blocked).
  - (+) The enum value is forward-compatible — no schema change needed to add a grace
    window later.
  - (−) Reviewers may ask why the status exists but is unused; documented here so the
    intent is clear.

---

### D-10 — Auth: JWT HS256, 120-min expiry, NO refresh tokens; BCrypt work factor 11; secret from env (≥32 chars)

- **Context.** PRD §3.1/§5.4 and §17: JWT-based auth (AUTH-002), role in the token
  (AUTH-003), secure password hashing (AUTH-006), and explicit out-of-scope for password
  reset/advanced token flows. PRD §14.6 supplies the canonical env keys and
  `Jwt__ExpiryMinutes=120`.
- **Decision.**
  - **Algorithm:** HS256 (symmetric), validated with all four checks on
    (`ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`,
    `ValidateIssuerSigningKey`); minimal `ClockSkew`.
  - **Lifetime:** `Jwt__ExpiryMinutes = 120`. **No refresh tokens** — expiry yields `401`
    and the client re-logs in via `POST /api/auth/login`.
  - **Claims:** `sub` (userId), `email`, `role`, `name`, `jti`, `iat`, `exp`, `iss`,
    `aud` (per `AUTH_MODEL.md`).
  - **Password hashing:** BCrypt (work factor **11**) on create and verify; hash stored
    in `Users.PasswordHash`, never serialized/logged (BR-13).
  - **Secret:** `Jwt__Secret` injected from environment, **≥32 bytes**, never in code.
- **Consequences.**
  - (+) Stateless, horizontally-scalable auth; no session table.
  - (+) `jti` uniqueness enables future revocation/audit without changing the token shape.
  - (−) 120-min hard expiry means users re-authenticate a couple of times a day; acceptable
    for v1 (refresh tokens are open future work, §5).
  - (−) HS256 requires a shared secret per environment; must be rotated per environment and
    never committed.

---

### D-11 — Role authorization enforced on the BACKEND (`[Authorize(Roles)]` + ownership checks); frontend guards are UX only

- **Context.** PRD BR-11 / AUTH-004: "Role-based access must be enforced by the backend
  API, not only by the frontend UI." A malicious or buggy client cannot be trusted.
- **Decision.** Every role-scoped route group is gated server-side:
  - `/api/admin/*` → `[Authorize(Roles = "Admin")]`
  - `/api/teacher/*` → `[Authorize(Roles = "Teacher")]`
  - `/api/student/*` → `[Authorize(Roles = "Student")]`
  - `GET /api/auth/me` → `[Authorize]` (any authenticated role)
  - `POST /api/auth/login` → anonymous.

  Route-level role gates are **not sufficient**: services additionally enforce
  **resource-level ownership** — teacher must own the assignment (`TeacherId == current
  user`); teacher must own the assignment of any submission being reviewed; student must
  be enrolled for a published assignment and must own the submission they read/update
  (`StudentId == current user`). Frontend route guards exist only for UX and never as a
  security boundary.
- **Consequences.**
  - (+) The API is safe to call from any client (curl, Postman, a compromised browser).
  - (+) Two layers of control (role policy + ownership) with distinct `403` semantics
    (TS-AUTH-03/04/05, TS-ASGN-06, TS-REV-04, TS-SUB-06).
  - (−) Ownership logic is duplicated intent between controllers and services; kept in the
    service layer as the single source of truth.

---

### D-12 — Admin is read-only for assignments/submissions (cannot create or grade)

- **Context.** PRD §2.1/§3.6 and ADM-001/002/003: Admin "views all assignments and
  submissions" and visibility "should not be limited by teacher assignment rules." But
  creating assignments and grading are teacher responsibilities (§2.2). PRD assumption 8
  (§16): "Admin can view all assignments and submissions but does not submit
  assignments."
- **Decision.** Admin endpoints for assignments/submissions are **GET-only**
  (`GET /api/admin/assignments`, `GET /api/admin/submissions`). Admin has **no**
  assignment-create, publish, delete, submit, or review endpoints. Admin manages only
  users, classes/courses, subjects, teacher assignments, and enrollments.
- **Consequences.**
  - (+) Clear separation of concerns: Admin = structure + visibility; Teacher = content +
    grading; Student = submission.
  - (+) Verified by TS-ADM-04 (Admin calling `POST /api/teacher/assignments` or review →
    `403`).
  - (−) Admin cannot correct a grade; teachers must — acceptable per requirements.

---

### D-13 — Enums stored as strings in PostgreSQL

- **Context.** The enums (`UserRole`, `AssignmentStatus`, `SubmissionStatus`) are small,
  stable vocabularies. Native Postgres enums are painful to migrate (adding/removing a
  value requires `ALTER TYPE`), and integer storage is opaque in `psql`.
- **Decision.** Persist every enum as PostgreSQL **`text`** via an EF Core
  `HasConversion<string>()` value converter. Values are stored as their PascalCase member
  names (`"Admin"`, `"Published"`, `"Reviewed"`, etc.) and serialized identically in JSON.
- **Consequences.**
  - (+) Human-readable in `psql` and in JSON payloads (no enum/int translation needed).
  - (+) Trivial schema diffs — adding/removing an enum value is a data change, not a DDL
    migration.
  - (−) Slightly larger storage and no DB-level enum type safety; mitigated by application
    validation (and an optional defensive `CHECK`).

---

### D-14 — Soft-disable users via `IsActive` while also allowing hard delete

- **Context.** PRD USER-004: "Admin can disable **or** delete users." Disabling should
  retain the user record (and its FK history) without granting login; deletion removes the
  user outright.
- **Decision.** Implement **two** paths:
  - **Soft-disable:** set `Users.IsActive = false`. A disabled user cannot authenticate
    (login returns `401` like a bad credential, with no existence leak). No global query
    filter / `IsDeleted` column is used; disabled users remain queryable by Admin.
  - **Hard delete:** `DELETE /api/admin/users/{id}` removes the row, subject to FK
    `Restrict` constraints (except `ReviewedByTeacherId` → `Set Null`). Deletion only
    succeeds when no dependent rows remain.
- **Consequences.**
  - (+) Disable preserves referential history (e.g., who created which assignments) while
    immediately revoking access.
  - (+) Hard delete is available for full removal when dependencies are cleared.
  - (−) Two user-lifecycle states to reason about (active vs disabled vs gone); documented
    here to remove ambiguity.

---

### D-15 — Frontend: Next.js App Router + TailwindCSS; token storage recommendation

- **Context.** PRD §5.1 requires Next.js + React + TypeScript, responsive UI, form
  validation, JWT token handling, protected routes, and role-based dashboards. The API
  contract uses Bearer tokens over JSON.
- **Decision.** Build the client with **Next.js 14 App Router + React + TypeScript +
  TailwindCSS**. Role-based dashboards and protected routes are implemented as
  client/server components guarded by auth state; the **recommended** JWT storage is an
  **httpOnly, Secure, SameSite=Strict (or Lax)** cookie set on login, with the token
  attached as `Authorization: Bearer <token>` on outgoing requests. (Where a BFF/cookie
  path is not feasible, `localStorage` is an acceptable fallback but is XSS-exposed and
  must be paired with a strict CSP — `httpOnly` cookie remains the documented
  recommendation.)
- **Consequences.**
  - (+) httpOnly storage mitigates XSS-based token theft; server components can do initial
    auth-gated fetches.
  - (+) TailwindCSS gives a consistent, responsive, low-CSS design system for the three
    role dashboards.
  - (−) A cookie/BFF adds a small server surface vs a pure SPA; if `localStorage` is chosen
    instead, CSP hardening is mandatory. Whichever is implemented, the choice is recorded
    in the README per PRD §14.4.

---

### D-16 — Error envelope + status codes per PRD §11; ProblemDetails-friendly

- **Context.** PRD §11 specifies the exact HTTP status set (200/201/204/400/401/403/404/
  409/500) and an example `{ message, errors }` envelope with field-keyed arrays.
- **Decision.** Return a single consistent error envelope `{ "message": string,
  "errors": { "<field>": ["..."] } }` for all failures. `errors` is present for `400`
  validation failures (camelCase field names) and omitted/empty for auth/not-found/
  conflict/server-error cases where only `message` applies. Status codes follow PRD §11
  exactly (e.g. duplicate email → `409`; not-owner → `403`; draft-to-student → `404`).
  Responses are also expressed as RFC 7807-style `ProblemDetails` so they remain
  machine-readable and Swagger-friendly.
- **Consequences.**
  - (+) Predictable, documented contract for clients (and tests can assert on `message` +
    status + `errors[field]`).
  - (+) Aligns with ASP.NET Core's `ProblemDetails` infrastructure for free OpenAPI docs.
  - (−) Ownership failures sometimes choose between `403` (reveal existence) and `404`
    (hide existence); the per-endpoint choice is documented in `API_CONTRACT.md` and kept
    consistent.

---

### D-17 — Explicitly out of scope (v1 boundaries)

- **Context.** PRD §17 enumerates features that are out of scope unless explicitly
  implemented as optional, and §16 assumption 10 lists file upload/notifications/reporting
  as optional. PRD §17 also excludes password reset and email verification.
- **Decision.** v1 ships **without**: file upload (assignments are text-based —
  `AnswerText`/`Description` only), real-time notifications, email verification, password
  reset, multi-tenancy, mobile applications, and advanced analytics/reporting dashboards.
- **Consequences.**
  - (+) Focused, deliverable scope that satisfies all PRD Definition-of-Done items
    (§18).
  - (+) Simpler security surface (no email/SMS providers, no reset tokens).
  - (−) No file-based submissions and no async notifications; documented as a limitation
    and listed as future work (§5).

---

### D-18 — Tests: xUnit; consolidated in Phase 8 but authored incrementally; mapped to `TS-*` scenarios

- **Context.** PRD §5.5/§13 mandate unit tests for important business rules,
  authorization, submission workflow, deadline validation, and marks validation, and
  suggest **xUnit**. `BUSINESS_RULES.md` defines a bidirectional `TS-*` test-scenario
  catalog (Given/When/Then) covering PRD §13.1–§13.4.
- **Decision.** Use **xUnit** for backend unit/integration tests. Tests are authored
  **incrementally** alongside the features they cover (Phases 2–5) and **consolidated**
  in Phase 8, each test bound to one or more `TS-*` scenario IDs from
  `BUSINESS_RULES.md` (e.g. `TS-ASGN-04` for `MaxMarks > 0`, `TS-SUB-02` for
  post-deadline block, `TS-REV-03` for `Marks ≤ MaxMarks`, `TS-AUTH-04`/`TS-AUTH-05` for
  cross-role `403`s, `TS-CROSS-01` for UTC stability).
- **Consequences.**
  - (+) Coverage is explicit and traceable (rule ↔ scenario ↔ test), not ad hoc.
  - (+) Incremental authoring catches regressions as each phase lands.
  - (−) Requires discipline to keep the `TS-*` ↔ test mapping current as scenarios evolve.

---

## 3. Assumptions (map to PRD §16)

These are the standing assumptions v1 is built on. Each maps to the corresponding PRD §16
item. They are restated here in implementation terms (using the contract entity/enum names)
and must also appear in the README per PRD §14.4.

1. **A student can belong to one or more classes/courses.** Modeled by `Enrollments`
   (one row per `(ClassId, StudentId)`); a student may have many `Enrollments`. *(PRD §16.1)*
2. **A teacher can be assigned to multiple class/course and subject combinations.**
   Modeled by `TeacherClassSubject(TeacherId, ClassId, SubjectId)` with a 3-way unique
   constraint; one teacher can own many such rows. *(PRD §16.2)*
3. **Assignments are text-based unless file upload is implemented.** v1 stores only
   `Assignment.Description` and `Submission.AnswerText` as text; no file/blob fields. *(PRD §16.3)*
4. **Students can update submissions multiple times before the deadline.** Each permitted
   update mutates the single `Submission` row (advances `UpdatedAtUtc`, keeps
   `SubmittedAtUtc`), gated by `AllowResubmission` (D-06). *(PRD §16.4)*
5. **Deadlines are stored and compared in UTC.** `DeadlineUtc` is `DateTimeKind.Utc`
   (`timestamptz`); all comparisons use `DateTime.UtcNow`. *(PRD §16.5 / BR-12)*
6. **Late submissions are not allowed after the deadline.** Both submit and update are
   blocked at/after `DeadlineUtc` in v1; `LateSubmitted` is reserved for a future grace
   window (D-09). *(PRD §16.6 / BR-6)*
7. **Teachers can manage only assignments they created.** Enforced by ownership checks
   (`TeacherId == current user`) on create/edit/delete/publish and on review of the
   assignment's submissions. *(PRD §16.7 / BR-9)*
8. **Admin can view all assignments and submissions but does not submit assignments.**
   Admin endpoints for assignments/submissions are GET-only; Admin neither creates nor
   grades (D-12). *(PRD §16.8)*
9. **Email verification and password reset are out of scope.** No email provider, no
   verification tokens, no reset flow; expired JWTs simply require re-login (D-10). *(PRD §16.9)*
10. **File upload, notifications, and advanced reporting are optional.** None are
    implemented in v1; they are future work (§5). *(PRD §16.10)*

---

## 4. Known Limitations (map to PRD §17)

The following are intentionally **not** delivered in v1. Each maps to the corresponding
PRD §17 out-of-scope item and is documented in the README per PRD §14.4.

1. **No real-time notifications** (PRD §17) — no WebSockets/SSE; the UI polls or requires
   manual refresh. *(PRD §17)*
2. **No email verification** (PRD §17) — accounts are active on creation; no confirmation
   mail. *(PRD §17)*
3. **No password reset flow** (PRD §17) — Admin must reset/disable; no self-service reset. *(PRD §17)*
4. **No SMS integration** (PRD §17) — no 2FA or SMS notifications. *(PRD §17)*
5. **No production deployment pipeline** (PRD §17) — local-run + seed only; no CI/CD to a
   hosted environment is provided. *(PRD §17)*
6. **No multi-tenancy** (PRD §17) — single school/tenant; data is not partitioned per
   organization. *(PRD §17)*
7. **No mobile application** (PRD §17) — the web UI is responsive but there is no native
   mobile app. *(PRD §17)*
8. **No advanced analytics dashboard** (PRD §17) — only role dashboards with basic lists;
   no cohort/performance analytics. *(PRD §17)*
9. **No internationalization (i18n)** (PRD §17) — UI strings are hard-coded (single
   locale); error messages are English. *(PRD §17)*

Additional v1 limitations implied by the decisions above: **no pagination** on list
endpoints (full arrays returned — small dataset assumption), **no fractional marks**
(D-04), and **no refresh tokens** (D-10).

---

## 5. Open Questions / Future Work

These are deliberately deferred and are **not** blocking for v1 delivery. Each notes the
decision record it relates to.

- **Late-submission grace window** (relates D-09). If the school later wants to accept
  submissions in `[deadline, deadline + grace)`, stamp them `LateSubmitted` and keep them
  review-eligible. Requires a per-assignment (or global) `GracePeriod` setting and the
  associated tests; the schema/enum already accommodate it.
- **File uploads** (relates D-17). Support attachments on `Submission` (and possibly
  `Assignment`). Requires blob storage, size/type validation, and download authorization
  scoped to ownership/enrollment.
- **Pagination & filtering** (relates D-16 / API contract). Add `page`/`pageSize`/`total`
  (and filters by status/class/student) to the list endpoints once the dataset grows.
- **Refresh tokens / sliding sessions** (relates D-10). Add a refresh-token grant (and
  revocation via `jti`) to avoid re-login every 120 minutes. Keep HS256 unless rotating to
  asymmetric keys is desired.
- **Audit log** (relates D-11/D-14). Persist who-changed-what for sensitive actions
  (user disable/delete, grade changes, publish transitions). The existing `jti`/`sub`
  claims and audit columns (`CreatedAt`/`UpdatedAt`, `ReviewedByTeacherId`/`ReviewedAtUtc`)
  are a starting point for a fuller append-only audit table.
- **`Archived` lifecycle** (relates D-08). Define re-publish from `Archived` and whether
  archived assignments remain read-visible to already-enrolled students.

---

*End of Design Decisions. Companion documents: `docs/PRD.md` (authoritative requirements,
read-only), `docs/DATABASE_SCHEMA.md`, `docs/AUTH_MODEL.md`, `docs/API_CONTRACT.md`,
`docs/BUSINESS_RULES.md`.*
