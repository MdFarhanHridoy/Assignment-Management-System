"use client";

import React from "react";
import Link from "next/link";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Badge } from "@/components/ui/Badge";
import { Card } from "@/components/ui/Card";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { Spinner } from "@/components/ui/Spinner";
import { useApi } from "@/hooks/useApi";
import { studentAssignmentsApi } from "@/lib/api/endpoints";
import { AssignmentDto } from "@/lib/types";
import { formatUtcDate, isDeadlinePassed } from "@/lib/utils";

export default function StudentAssignmentsPage() {
  const {
    data: assignments,
    loading,
    error,
    refetch,
  } = useApi<AssignmentDto[]>(() => studentAssignmentsApi.list(), []);

  let content: React.ReactNode = null;

  if (loading) {
    content = (
      <div className="flex items-center justify-center py-12">
        <Spinner />
      </div>
    );
  } else if (error) {
    content = <ErrorState message={error} onRetry={refetch} />;
  } else if (!assignments || assignments.length === 0) {
    content = <EmptyState title="No assignments available" />;
  } else {
    content = (
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        {assignments.map((assignment) => {
          const passed = isDeadlinePassed(assignment.deadlineUtc);
          return (
            <Card key={assignment.id}>
              <div className="flex items-start justify-between gap-3">
                <h3 className="text-lg font-semibold text-gray-900">
                  {assignment.title}
                </h3>
                <Badge variant="blue">{assignment.maxMarks} marks</Badge>
              </div>
              <p className="mt-2 line-clamp-3 text-sm text-gray-600">
                {assignment.description}
              </p>
              <div className="mt-4 text-sm">
                <p className="text-gray-500">
                  Deadline: {formatUtcDate(assignment.deadlineUtc)}
                </p>
                {passed && (
                  <p className="mt-1 font-medium text-red-600">
                    Deadline passed
                  </p>
                )}
              </div>
              <div className="mt-4">
                <Link
                  href={`/student/assignments/${assignment.id}`}
                  className="inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800"
                >
                  View &amp; Submit →
                </Link>
              </div>
            </Card>
          );
        })}
      </div>
    );
  }

  return (
    <RoleGuard allowedRoles={["Student"]}>
      <RoleShell role="Student">
        <div className="space-y-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Assignments</h1>
            <p className="mt-1 text-sm text-gray-500">
              Assignments from your enrolled classes
            </p>
          </div>
          {content}
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
