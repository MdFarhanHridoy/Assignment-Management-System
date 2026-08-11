"use client";

import { useState } from "react";
import Link from "next/link";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Spinner } from "@/components/ui/Spinner";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { Table, Th, Td, Tr } from "@/components/ui/Table";
import { useApi } from "@/hooks/useApi";
import { teacherAssignmentsApi } from "@/lib/api/endpoints";
import { AssignmentDto, AssignmentStatus } from "@/lib/types";
import { formatUtcDate, formatDate } from "@/lib/utils";

const statusVariant: Record<
  AssignmentStatus,
  "gray" | "green" | "yellow"
> = {
  Draft: "gray",
  Published: "green",
  Archived: "yellow",
};

export default function TeacherAssignmentsPage() {
  const { data, loading, error, refetch } = useApi(
    () => teacherAssignmentsApi.list(),
    []
  );
  const [actionId, setActionId] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  const handlePublish = async (id: string) => {
    setActionError(null);
    setActionId(id);
    try {
      await teacherAssignmentsApi.publish(id);
      refetch();
    } catch (err) {
      setActionError(
        (err as { message?: string })?.message ??
          "Failed to publish assignment."
      );
    } finally {
      setActionId(null);
    }
  };

  const handleDelete = async (id: string) => {
    const confirmed = window.confirm(
      "Are you sure you want to delete this assignment? This action cannot be undone."
    );
    if (!confirmed) return;

    setActionError(null);
    setActionId(id);
    try {
      await teacherAssignmentsApi.delete(id);
      refetch();
    } catch (err) {
      setActionError(
        (err as { message?: string })?.message ??
          "Failed to delete assignment."
      );
    } finally {
      setActionId(null);
    }
  };

  return (
    <RoleGuard allowedRoles={["Teacher"]}>
      <RoleShell role="Teacher">
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">
                My Assignments
              </h1>
              <p className="mt-1 text-sm text-gray-500">
                Create, publish, and review your assignments.
              </p>
            </div>
            <Link href="/teacher/assignments/new">
              <Button>+ New Assignment</Button>
            </Link>
          </div>

          {actionError && (
            <div
              role="alert"
              className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
            >
              {actionError}
            </div>
          )}

          <Card>
            {loading ? (
              <div className="flex items-center justify-center py-12">
                <Spinner />
              </div>
            ) : error ? (
              <ErrorState message={error} onRetry={refetch} />
            ) : !data || data.length === 0 ? (
              <EmptyState
                title="You haven't created any assignments yet"
                description="Get started by creating a new assignment for your class."
              />
            ) : (
              <Table>
                <thead>
                  <tr>
                    <Th>Title</Th>
                    <Th>Status</Th>
                    <Th>Deadline</Th>
                    <Th>Max Marks</Th>
                    <Th>Created</Th>
                    <Th>Actions</Th>
                  </tr>
                </thead>
                <tbody>
                  {data.map((a: AssignmentDto) => (
                    <Tr key={a.id}>
                      <Td className="font-medium text-gray-900">{a.title}</Td>
                      <Td>
                        <Badge variant={statusVariant[a.status]}>
                          {a.status}
                        </Badge>
                      </Td>
                      <Td>{formatUtcDate(a.deadlineUtc)}</Td>
                      <Td>{a.maxMarks}</Td>
                      <Td>{formatDate(a.createdAt)}</Td>
                      <Td>
                        <div className="flex flex-wrap items-center gap-x-3 gap-y-1">
                          <Link
                            href={`/teacher/assignments/${a.id}/edit`}
                            className="text-sm font-medium text-indigo-600 hover:text-indigo-800"
                          >
                            Edit
                          </Link>
                          {a.status === "Draft" && (
                            <button
                              type="button"
                              onClick={() => handlePublish(a.id)}
                              disabled={actionId === a.id}
                              className="text-sm font-medium text-green-600 hover:text-green-800 disabled:opacity-50"
                            >
                              {actionId === a.id ? "Publishing..." : "Publish"}
                            </button>
                          )}
                          <Link
                            href={`/teacher/assignments/${a.id}/submissions`}
                            className="text-sm font-medium text-blue-600 hover:text-blue-800"
                          >
                            Submissions
                          </Link>
                          <button
                            type="button"
                            onClick={() => handleDelete(a.id)}
                            disabled={actionId === a.id}
                            className="text-sm font-medium text-red-600 hover:text-red-800 disabled:opacity-50"
                          >
                            Delete
                          </button>
                        </div>
                      </Td>
                    </Tr>
                  ))}
                </tbody>
              </Table>
            )}
          </Card>
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
