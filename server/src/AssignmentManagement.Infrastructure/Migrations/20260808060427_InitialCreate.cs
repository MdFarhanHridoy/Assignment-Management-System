using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "class",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_class", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subject",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subject", x => x.id);
                    table.ForeignKey(
                        name: "fk_subject_class_class_id",
                        column: x => x.class_id,
                        principalTable: "class",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enrollment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrolled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_enrollment", x => x.id);
                    table.ForeignKey(
                        name: "fk_enrollment_class_class_id",
                        column: x => x.class_id,
                        principalTable: "class",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_enrollment_user_student_id",
                        column: x => x.student_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assignment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    deadline_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    max_marks = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    allow_resubmission = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignment", x => x.id);
                    table.CheckConstraint("ck_assignments_max_marks_positive", "\"max_marks\" > 0");
                    table.ForeignKey(
                        name: "fk_assignment_class_class_id",
                        column: x => x.class_id,
                        principalTable: "class",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_assignment_subject_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subject",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_assignment_user_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teacher_class_subject",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teacher_class_subject", x => x.id);
                    table.ForeignKey(
                        name: "fk_teacher_class_subject_class_class_id",
                        column: x => x.class_id,
                        principalTable: "class",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_teacher_class_subject_subject_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subject",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_teacher_class_subject_user_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "submission",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    answer_text = table.Column<string>(type: "text", nullable: false),
                    submitted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Submitted"),
                    marks = table.Column<int>(type: "integer", nullable: true),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    reviewed_by_teacher_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_submission", x => x.id);
                    table.CheckConstraint("ck_submissions_marks_nonneg", "\"marks\" IS NULL OR \"marks\" >= 0");
                    table.ForeignKey(
                        name: "fk_submission_assignment_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "assignment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_submission_user_reviewed_by_teacher_id",
                        column: x => x.reviewed_by_teacher_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_submission_user_student_id",
                        column: x => x.student_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ClassId",
                table: "assignment",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_Status",
                table: "assignment",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_SubjectId",
                table: "assignment",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_TeacherId",
                table: "assignment",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "ix_enrollment_student_id",
                table: "enrollment",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "UX_Enrollments_ClassId_StudentId",
                table: "enrollment",
                columns: new[] { "class_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_ClassId",
                table: "subject",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "UX_Subjects_ClassId_Name",
                table: "subject",
                columns: new[] { "class_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_submission_reviewed_by_teacher_id",
                table: "submission",
                column: "reviewed_by_teacher_id");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AssignmentId",
                table: "submission",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_Status",
                table: "submission",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_StudentId",
                table: "submission",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "UX_Submissions_AssignmentId_StudentId",
                table: "submission",
                columns: new[] { "assignment_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teacher_class_subject_class_id",
                table: "teacher_class_subject",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_class_subject_subject_id",
                table: "teacher_class_subject",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "UX_TeacherClassSubjects_TeacherId_ClassId_SubjectId",
                table: "teacher_class_subject",
                columns: new[] { "teacher_id", "class_id", "subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "user",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enrollment");

            migrationBuilder.DropTable(
                name: "submission");

            migrationBuilder.DropTable(
                name: "teacher_class_subject");

            migrationBuilder.DropTable(
                name: "assignment");

            migrationBuilder.DropTable(
                name: "subject");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "class");
        }
    }
}
