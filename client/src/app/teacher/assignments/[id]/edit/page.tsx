"use client";

import { useParams, useRouter } from "next/navigation";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Card } from "@/components/ui/Card";
import { Spinner } from "@/components/ui/Spinner";
import { ErrorState } from "@/components/ui/ErrorState";
import { AssignmentForm } from "@/components/forms/AssignmentForm";
import { useApi } from "@/hooks/useApi";
import { teacherAssignmentsApi } from "@/lib/api/endpoints";
import {
  ApiError,
  AssignmentDto,
  CreateAssignmentRequest,
  UpdateAssignmentRequest,
} from "@/lib/types";

function hasStatus(err: unknown, status: number): boolean {
  const message = (err as { message?: string })?.message ?? "";
  return message.includes(String(status));
}

export default function EditAssignmentPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const id = params.id;

  const { data: assignment, loading, error, refetch } = useApi(
    () => teacherAssignmentsApi.get(id),
    [id]
  );

  const handleSubmit = async (formData: CreateAssignmentRequest | UpdateAssignmentRequest) => {
    const data = formData as CreateAssignmentRequest;
    if (!assignment) {
      return { success: false, error: "Assignment not loaded yet." };
    }

    const changes: UpdateAssignmentRequest = {};
    if (data.title !== assignment.title) changes.title = data.title;
    if (data.description !== assignment.description)
      changes.description = data.description;
    if (
      new Date(data.deadlineUtc).getTime() !==
      new Date(assignment.deadlineUtc).getTime()
    ) {
      changes.deadlineUtc = data.deadlineUtc;
    }
    if (data.maxMarks !== assignment.maxMarks)
      changes.maxMarks = data.maxMarks;
    if (data.classId !== assignment.classId)
      changes.classId = data.classId;
    if (data.subjectId !== assignment.subjectId)
      changes.subjectId = data.subjectId;
    if (data.allowResubmission !== assignment.allowResubmission)
      changes.allowResubmission = data.allowResubmission;

    try {
      await teacherAssignmentsApi.update(id, changes);
      router.push("/teacher/assignments");
      return { success: true };
    } catch (err) {
      const message =
        (err as ApiError)?.message ?? "Failed to update assignment.";
      if (hasStatus(err, 404)) {
        return { success: false, error: "Assignment not found." };
      }
      if (
        hasStatus(err, 403) ||
        /forbidden|not allowed|not assigned/i.test(message)
      ) {
        return {
          success: false,
          error: "You are not allowed to edit this assignment.",
        };
      }
      return { success: false, error: message };
    }
  };

  return (
    <RoleGuard allowedRoles={["Teacher"]}>
      <RoleShell role="Teacher">
        <div className="mx-auto max-w-2xl space-y-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">
              Edit Assignment
            </h1>
            <p className="mt-1 text-sm text-gray-500">
              Update the details of your assignment.
            </p>
          </div>

          {loading ? (
            <div className="flex items-center justify-center py-12">
              <Spinner />
            </div>
          ) : error ? (
            <ErrorState message={error} onRetry={refetch} />
          ) : !assignment ? (
            <div className="flex items-center justify-center py-12">
              <Spinner />
            </div>
          ) : (
            <Card>
              <AssignmentForm
                initialData={assignmentToInitialData(assignment)}
                onSubmit={handleSubmit}
                submitLabel="Save Changes"
              />
            </Card>
          )}
        </div>
      </RoleShell>
    </RoleGuard>
  );
}

function assignmentToInitialData(
  assignment: AssignmentDto
): Partial<CreateAssignmentRequest> {
  return {
    title: assignment.title,
    description: assignment.description,
    deadlineUtc: assignment.deadlineUtc,
    maxMarks: assignment.maxMarks,
    classId: assignment.classId,
    subjectId: assignment.subjectId,
    allowResubmission: assignment.allowResubmission,
  };
}
