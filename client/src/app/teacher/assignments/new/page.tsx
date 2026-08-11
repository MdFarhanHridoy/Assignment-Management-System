"use client";

import { useRouter } from "next/navigation";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Card } from "@/components/ui/Card";
import { AssignmentForm } from "@/components/forms/AssignmentForm";
import { teacherAssignmentsApi } from "@/lib/api/endpoints";
import { ApiError, CreateAssignmentRequest, UpdateAssignmentRequest } from "@/lib/types";

function hasStatus(err: unknown, status: number): boolean {
  const message = (err as { message?: string })?.message ?? "";
  return message.includes(String(status));
}

export default function NewAssignmentPage() {
  const router = useRouter();

  const handleSubmit = async (formData: CreateAssignmentRequest | UpdateAssignmentRequest) => {
    const data = formData as CreateAssignmentRequest;
    try {
      await teacherAssignmentsApi.create(data);
      router.push("/teacher/assignments");
      return { success: true };
    } catch (err) {
      const message =
        (err as ApiError)?.message ?? "Failed to create assignment.";
      if (
        hasStatus(err, 403) ||
        /forbidden|not assigned/i.test(message)
      ) {
        return {
          success: false,
          error:
            "You are not assigned to this class and subject combination.",
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
              Create New Assignment
            </h1>
            <p className="mt-1 text-sm text-gray-500">
              Fill in the details below to create a new assignment.
            </p>
          </div>
          <Card>
            <AssignmentForm
              onSubmit={handleSubmit}
              submitLabel="Create Assignment"
            />
          </Card>
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
