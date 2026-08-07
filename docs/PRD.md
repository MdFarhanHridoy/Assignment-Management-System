# PRD - Assignment & Submission Management System

## Document Information

| Item | Details |
|---|---|
| Company | OnnoRokom Projukti Limited |
| Project Name | Assignment & Submission Management System |
| Project Type | Full-stack web application |
| Submission Deadline | 14 August, 2026 |
| Source Document | Assistant Software Engineer Recruitment Project.pdf |

Please read the requirements carefully and make reasonable assumptions where the requirements are not explicitly defined. Document those assumptions in the README.

---

## 1. Project Brief

Build a role-based Assignment & Submission Management System for a school or college.

The system should allow:

- Teachers to create assignments for specific classes or courses.
- Students to view and submit assignments.
- Teachers to review submissions and provide marks and feedback.

This project is intended to evaluate understanding of:

- Requirements analysis
- System design
- API development
- Frontend implementation
- Authentication and authorization
- Database design
- Testing

---

## 2. User Roles and Responsibilities

### 2.1 Admin

The Admin is responsible for managing users, academic structure, and system visibility.

Responsibilities:

- Manage users.
- Manage classes/courses and subjects.
- Assign teachers to subjects/classes.
- View all assignments and submissions.
- Manage application-level settings where necessary.

Expected Admin capabilities:

- Create, update, disable, or delete users.
- Create, update, and delete classes/courses.
- Create, update, and delete subjects.
- Assign a teacher to a subject/class.
- Enroll students into classes/courses.
- View all assignments across the system.
- View all submissions across the system.

### 2.2 Teacher

The Teacher is responsible for creating assignments and evaluating student submissions.

Responsibilities:

- Create, update, and delete assignments.
- Assign an assignment to a specific class/course and subject.
- Define the title, description, deadline, and maximum marks.
- Publish an assignment or keep it as a draft.
- View student submissions.
- Assign marks and provide feedback.
- Change the submission status when necessary.

Expected Teacher capabilities:

- Create assignments for classes/subjects assigned by Admin.
- Save assignments as draft or published.
- Edit or delete assignments they created.
- View submissions for their assignments.
- Review submissions.
- Provide marks and feedback.
- Update submission status.

### 2.3 Student

The Student is responsible for viewing assignments and submitting answers.

Responsibilities:

- View assignments assigned to their class/course.
- View assignment details and deadline.
- Submit an answer.
- Update a submission before the deadline, if allowed.
- View submission status, marks, and teacher feedback.

Expected Student capabilities:

- View only published assignments for enrolled classes/courses.
- View assignment details.
- Submit an answer before the deadline.
- Update submission before the deadline, if allowed.
- View their own submission status.
- View marks and feedback after teacher review.

Applicants may use a different but suitable design. Any important design decisions should be explained in the README.

---

## 3. Functional Requirements

### 3.1 Authentication and Authorization

| ID | Requirement |
|---|---|
| AUTH-001 | Users must log in using email and password. |
| AUTH-002 | Authentication must use JWT tokens. |
| AUTH-003 | JWT tokens must include the user role. |
| AUTH-004 | Backend API endpoints must enforce role-based authorization. |
| AUTH-005 | Unauthorized requests must return appropriate 401 or 403 responses. |
| AUTH-006 | Passwords must be stored securely using hashing. |
| AUTH-007 | Demo credentials must be provided for Admin, Teacher, and Student roles. |

### 3.2 User Management

| ID | Requirement |
|---|---|
| USER-001 | Admin can create users. |
| USER-002 | Admin can list users. |
| USER-003 | Admin can update user information. |
| USER-004 | Admin can disable or delete users. |
| USER-005 | Each user must have one role: Admin, Teacher, or Student. |
| USER-006 | Email should be unique for each user. |

### 3.3 Class/Course and Subject Management

| ID | Requirement |
|---|---|
| CLASS-001 | Admin can create classes/courses. |
| CLASS-002 | Admin can update classes/courses. |
| CLASS-003 | Admin can delete classes/courses. |
| CLASS-004 | Admin can create subjects. |
| CLASS-005 | Admin can update subjects. |
| CLASS-006 | Admin can delete subjects. |
| CLASS-007 | Subjects can be associated with classes/courses. |
| CLASS-008 | Admin can assign teachers to subjects/classes. |
| CLASS-009 | Admin can enroll students into classes/courses. |

### 3.4 Assignment Management

| ID | Requirement |
|---|---|
| ASGN-001 | Teacher can create an assignment. |
| ASGN-002 | Assignment must include title, description, deadline, and maximum marks. |
| ASGN-003 | Assignment must be assigned to a specific class/course and subject. |
| ASGN-004 | Teacher can update an assignment. |
| ASGN-005 | Teacher can delete an assignment. |
| ASGN-006 | Teacher can publish an assignment. |
| ASGN-007 | Teacher can keep an assignment as a draft. |
| ASGN-008 | Draft assignments must not be visible to students. |
| ASGN-009 | Published assignments must be visible to students in the assigned class/course. |
| ASGN-010 | Assignment deadline should be stored in UTC. |
| ASGN-011 | Maximum marks must be greater than zero. |

### 3.5 Submission Management

| ID | Requirement |
|---|---|
| SUB-001 | Student can submit an answer for a published assignment. |
| SUB-002 | Student can view assignment details before submitting. |
| SUB-003 | Student can update a submission before the deadline, if allowed. |
| SUB-004 | Student cannot submit after the deadline. |
| SUB-005 | Student can view submission status. |
| SUB-006 | Student can view marks and feedback after review. |
| SUB-007 | Student can view only their own submissions. |
| SUB-008 | Teacher can view submissions for their assignments. |
| SUB-009 | Teacher can assign marks. |
| SUB-010 | Teacher can provide feedback. |
| SUB-011 | Teacher can change submission status. |
| SUB-012 | Marks must not exceed the assignment maximum marks. |

### 3.6 Admin Visibility

| ID | Requirement |
|---|---|
| ADM-001 | Admin can view all assignments. |
| ADM-002 | Admin can view all submissions. |
| ADM-003 | Admin visibility should not be limited by teacher assignment rules. |

---

## 4. Business Rules

The following business rules should be implemented and tested:

1. Only Admin can manage users, classes/courses, subjects, and teacher assignments.
2. Only Teachers can create and manage assignments.
3. Teachers should create assignments only for classes/subjects assigned to them.
4. Draft assignments must not be visible to students.
5. Published assignments must be visible only to students enrolled in the related class/course.
6. Students can submit only before the assignment deadline.
7. Students may update their submission before the deadline, if allowed.
8. A student cannot view another student's submission.
9. A teacher can review submissions only for assignments they own or are responsible for.
10. Marks must be between zero and the assignment maximum marks.
11. Role-based access must be enforced by the backend API, not only by the frontend UI.
12. Deadlines should be compared using UTC time.
13. Sensitive data such as password hashes must not be exposed in API responses.

---

## 5. Technical Requirements

Use the following technologies, or equivalent technologies suitable for the project.

### 5.1 Frontend

Required technologies:

- Next.js
- React
- TypeScript
- Responsive UI
- Form validation
- API integration

Frontend expectations:

- Login page.
- Role-based dashboards for Admin, Teacher, and Student.
- Forms with validation.
- Loading, error, and empty states.
- Protected routes based on authentication and role.
- JWT token handling.
- API integration with backend REST endpoints.

### 5.2 Backend

Required technologies:

- ASP.NET Core Web API
- C#
- RESTful API
- Validation
- Error handling
- Logging
- Swagger/OpenAPI

Backend expectations:

- Clean project structure.
- JWT authentication.
- Role-based authorization.
- DTOs for request and response models.
- Consistent error responses.
- Input validation.
- Logging for errors and important operations.
- Swagger/OpenAPI documentation.

### 5.3 Database

Use one of the following databases:

- PostgreSQL
- MongoDB

Database expectations:

- Implement the required relationships.
- If using a relational database, include migration files.
- Include seed/sample data.
- Include a database script or backup file if applicable.
- The evaluator should be able to set up the database without manually creating tables or collections.
- If MongoDB is used, explain the chosen data model in the README.

Recommended choice: PostgreSQL, because the domain contains relational data such as users, classes, subjects, assignments, and submissions.

### 5.4 Authentication

Authentication requirements:

- Login endpoint.
- JWT-based authentication.
- Role-based authorization.
- Secure password hashing.
- Token should include user identity and role.

### 5.5 Testing

Testing requirements:

- Unit tests covering important business rules.
- Authorization tests.
- Submission workflow tests.
- Deadline validation tests.
- Marks validation tests.

Suggested backend testing framework:

- xUnit

---

## 6. Suggested Domain Model

The following is a suggested high-level data model. Applicants may use a different but suitable design. Important design decisions should be explained in the README.

### 6.1 User

| Field | Description |
|---|---|
| Id | Unique user identifier |
| Name | Full name |
| Email | Unique login email |
| PasswordHash | Hashed password |
| Role | Admin, Teacher, or Student |
| CreatedAt | Timestamp when user was created |

### 6.2 Class/Course

| Field | Description |
|---|---|
| Id | Unique class/course identifier |
| Name | Class/course name |
| Description | Optional description |
| CreatedAt | Timestamp |

### 6.3 Subject

| Field | Description |
|---|---|
| Id | Unique subject identifier |
| Name | Subject name |
| ClassId | Related class/course |
| CreatedAt | Timestamp |

### 6.4 Enrollment

| Field | Description |
|---|---|
| Id | Unique enrollment identifier |
| ClassId | Class/course identifier |
| StudentId | Student identifier |

### 6.5 TeacherClassSubject

| Field | Description |
|---|---|
| Id | Unique identifier |
| TeacherId | Teacher identifier |
| ClassId | Class/course identifier |
| SubjectId | Subject identifier |

### 6.6 Assignment

| Field | Description |
|---|---|
| Id | Unique assignment identifier |
| Title | Assignment title |
| Description | Assignment description |
| DeadlineUtc | Deadline in UTC |
| MaxMarks | Maximum possible marks |
| Status | Draft, Published, Archived |
| TeacherId | Teacher who created the assignment |
| ClassId | Related class/course |
| SubjectId | Related subject |
| CreatedAt | Timestamp |
| UpdatedAt | Timestamp |

### 6.7 Submission

| Field | Description |
|---|---|
| Id | Unique submission identifier |
| AssignmentId | Related assignment |
| StudentId | Student who submitted |
| AnswerText | Submitted answer |
| SubmittedAtUtc | Submission timestamp |
| UpdatedAtUtc | Last update timestamp |
| Status | Submitted, UnderReview, Reviewed, LateSubmitted |
| Marks | Marks assigned by teacher |
| Feedback | Teacher feedback |
| ReviewedByTeacherId | Teacher who reviewed the submission |
| ReviewedAtUtc | Review timestamp |

---

## 7. Suggested API Areas

The exact API design may vary, but the following areas should be covered.

### 7.1 Authentication

    POST /api/auth/login
    GET  /api/auth/me

### 7.2 Admin

    POST   /api/admin/users
    GET    /api/admin/users
    PUT    /api/admin/users/{id}
    DELETE /api/admin/users/{id}

    POST   /api/admin/classes
    GET    /api/admin/classes
    PUT    /api/admin/classes/{id}
    DELETE /api/admin/classes/{id}

    POST   /api/admin/subjects
    GET    /api/admin/subjects
    PUT    /api/admin/subjects/{id}
    DELETE /api/admin/subjects/{id}

    POST   /api/admin/teacher-assignments
    GET    /api/admin/teacher-assignments

    POST   /api/admin/enrollments
    GET    /api/admin/enrollments

    GET    /api/admin/assignments
    GET    /api/admin/submissions

### 7.3 Teacher

    POST   /api/teacher/assignments
    GET    /api/teacher/assignments
    GET    /api/teacher/assignments/{id}
    PUT    /api/teacher/assignments/{id}
    DELETE /api/teacher/assignments/{id}
    POST   /api/teacher/assignments/{id}/publish

    GET    /api/teacher/assignments/{assignmentId}/submissions
    PUT    /api/teacher/submissions/{submissionId}/review

### 7.4 Student

    GET  /api/student/assignments
    GET  /api/student/assignments/{id}

    POST /api/student/assignments/{assignmentId}/submit
    PUT  /api/student/submissions/{submissionId}

    GET  /api/student/submissions
    GET  /api/student/submissions/{submissionId}

---

## 8. Frontend Pages

The frontend should include role-specific pages.

### 8.1 Public Pages

    /login

### 8.2 Admin Pages

    /admin/dashboard
    /admin/users
    /admin/classes
    /admin/subjects
    /admin/teacher-assignments
    /admin/enrollments
    /admin/assignments
    /admin/submissions

### 8.3 Teacher Pages

    /teacher/dashboard
    /teacher/assignments
    /teacher/assignments/new
    /teacher/assignments/[id]
    /teacher/assignments/[id]/edit
    /teacher/assignments/[assignmentId]/submissions
    /teacher/submissions/[submissionId]

### 8.4 Student Pages

    /student/dashboard
    /student/assignments
    /student/assignments/[id]
    /student/submissions
    /student/submissions/[submissionId]

---

## 9. UI Requirements

The UI should:

- Be responsive.
- Show form validation errors.
- Show API error messages.
- Show loading states.
- Show empty states.
- Redirect users based on role after login.
- Protect routes based on authentication and role.
- Provide simple and usable dashboards for each role.

---

## 10. Validation Requirements

Examples of validation rules:

### 10.1 Login

- Email is required.
- Email must be valid.
- Password is required.

### 10.2 User Creation

- Name is required.
- Email is required.
- Email must be unique.
- Role must be valid.

### 10.3 Assignment Creation

- Title is required.
- Description is required.
- Deadline is required.
- Deadline must be a valid future date for new assignments.
- Maximum marks must be greater than zero.
- Class/course is required.
- Subject is required.

### 10.4 Submission

- Answer is required.
- Assignment must exist.
- Assignment must be published.
- Student must be enrolled in the assignment class/course.
- Submission must occur before the deadline.

### 10.5 Review

- Marks must be between zero and maximum marks.
- Feedback may be optional.
- Submission must exist.
- Teacher must be allowed to review the submission.

---

## 11. Error Handling Requirements

The API should return consistent and meaningful error responses.

Expected HTTP status codes:

| Status Code | Usage |
|---|---|
| 200 | Successful request |
| 201 | Resource created |
| 204 | Successful deletion with no content |
| 400 | Validation error or bad request |
| 401 | Not authenticated |
| 403 | Authenticated but not authorized |
| 404 | Resource not found |
| 409 | Conflict, such as duplicate email |
| 500 | Unexpected server error |

Example error response:

    {
      "message": "Validation failed.",
      "errors": {
        "title": [
          "Title is required."
        ]
      }
    }

---

## 12. Logging Requirements

The backend should include logging for:

- Application startup.
- Unhandled exceptions.
- Failed authentication attempts.
- Important business operations, if appropriate.
- API errors.

Do not log sensitive information such as passwords or full JWT tokens.

---

## 13. Testing Requirements

Unit tests should cover important business rules, including but not limited to:

### 13.1 Authentication and Authorization

- Valid login returns JWT.
- Invalid login returns 401.
- Admin can access admin endpoints.
- Teacher cannot access admin endpoints.
- Student cannot access teacher endpoints.

### 13.2 Assignment Rules

- Teacher cannot create assignment for unassigned class/subject.
- Draft assignment is not visible to students.
- Published assignment is visible to enrolled students.
- Maximum marks must be greater than zero.

### 13.3 Submission Rules

- Student can submit before deadline.
- Student cannot submit after deadline.
- Student can update submission before deadline.
- Student cannot update submission after deadline.
- Student cannot submit to draft assignment.
- Student cannot view other students' submissions.

### 13.4 Review Rules

- Teacher can review submissions for their assignments.
- Marks cannot be negative.
- Marks cannot exceed maximum marks.
- Admin can view all submissions.

---

## 14. Submission Guidelines

After completing the project, submit the following:

### 14.1 Git Repository Link

Submit a GitHub or GitLab repository link containing the complete source code.

### 14.2 Complete Project Code

Include:

- Frontend code
- Backend/API code
- Database files
- Unit tests

### 14.3 Database Files

Include:

- Migration files
- Seed/sample data
- Database script or backup file, if applicable

The evaluator should be able to set up the database without manually creating tables or collections.

### 14.4 README.md

Include:

- Short project overview
- Main features
- Technology stack
- Project structure
- Setup instructions
- Database setup instructions
- Instructions for running the frontend
- Instructions for running the backend
- Instructions for running the tests
- Assumptions
- Known limitations

### 14.5 Demo Credentials

Provide working login credentials for the Admin, Teacher, and Student roles.

| Role | Email | Password |
|---|---|---|
| Admin | admin@example.com | admin@123 |
| Teacher | teacher@example.com | teacher@123 |
| Student | student@example.com | student@123 |

Use demo credentials only. Do not use real personal passwords.

### 14.6 Environment Configuration

Do not upload real passwords, API keys, or other sensitive information.

Include an .env.example file showing the required environment variables.

Example:

    # Backend
    ASPNETCORE_ENVIRONMENT=Development
    ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=assignment_management;Username=postgres;Password=postgres
    Jwt__Secret=change-this-long-development-secret
    Jwt__Issuer=assignment-management-api
    Jwt__Audience=assignment-management-client
    Jwt__ExpiryMinutes=120

    # Frontend
    NEXT_PUBLIC_API_URL=http://localhost:5000

### 14.7 Easy Local Setup

Provide clear and complete setup instructions in the README so the project can be run locally.

---

## 15. Final Checklist

Before submitting, confirm the following:

- [ ] The repository link is accessible.
- [ ] Frontend and backend are both included.
- [ ] The database can be created using the provided files or instructions.
- [ ] Demo accounts for all three roles are available.
- [ ] The README explains how to run the project and its tests.
- [ ] Role-based access is enforced by the backend API.
- [ ] Important business rules are implemented and tested.
- [ ] No real secrets or credentials are committed to the repository.

---

## 16. Assumptions to Document in README

The following assumptions can be used if they match the implemented system:

1. A student can belong to one or more classes/courses.
2. A teacher can be assigned to multiple class/course and subject combinations.
3. Assignments are text-based unless file upload is implemented.
4. Students can update submissions multiple times before the deadline.
5. Deadlines are stored and compared in UTC.
6. Late submissions are not allowed after the deadline.
7. Teachers can manage only assignments they created.
8. Admin can view all assignments and submissions but does not submit assignments.
9. Email verification and password reset are out of scope.
10. File upload, notifications, and advanced reporting are optional.

---

## 17. Out of Scope

Unless explicitly implemented as optional features, the following are out of scope:

- Real-time notifications
- Email verification
- Password reset flow
- SMS integration
- Production deployment pipeline
- Multi-tenancy
- Mobile application
- Advanced analytics dashboard
- Internationalization

---

## 18. Definition of Done

The project can be considered complete when:

1. Admin, Teacher, and Student roles can log in.
2. Role-based access is enforced by the backend.
3. Admin can manage users, classes, subjects, teacher assignments, and enrollments.
4. Teacher can create, publish, update, and delete assignments.
5. Student can view published assignments and submit answers.
6. Student can update submission before the deadline, if allowed.
7. Teacher can review submissions and assign marks/feedback.
8. Database can be created locally without manual table creation.
9. Seed data provides working demo users.
10. Unit tests cover important business rules.
11. README contains full setup instructions.
12. No real secrets are committed.