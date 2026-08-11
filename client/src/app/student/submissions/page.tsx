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
import { Table, Td, Th, Tr } from "@/components/ui/Table";
import { useApi } from "@/hooks/useApi";
import { studentSubmissionsApi } from "@/lib/api/endpoints";
import { SUBMISSION_STATUS_LABELS } from "@/lib/constants";
import { SubmissionDto, SubmissionStatus } from "@/lib/types";
import { formatUtcDate } from "@/lib/utils";

function statusVariant(
  status: SubmissionStatus
): "gray" | "yellow" | "green" | "red" {
  switch (status) {
    case "Reviewed":
      return "green";
    case "UnderReview":
      return "yellow";
    case "LateSubmitted":
      return "red";
    default:
      return "gray";
  }
}

export default function StudentSubmissionsPage() {
  const {
    data: submissions,
    loading,
    error,
    refetch,
  } = useApi<SubmissionDto[]>(() => studentSubmissionsApi.list(), []);

  let content: React.ReactNode = null;

  if (loading) {
    content = (
      <div className="flex items-center justify-center py-12">
        <Spinner />
      </div>
    );
  } else if (error) {
    content = <ErrorState message={error} onRetry={refetch} />;
  } else if (!submissions || submissions.length === 0) {
    content = (
      <EmptyState title="You have not submitted any assignments yet" />
    );
  } else {
    content = (
      <Card>
        <Table>
          <thead>
            <tr>
              <Th>Assignment</Th>
              <Th>Status</Th>
              <Th>Marks</Th>
              <Th>Submitted</Th>
              <Th>Updated</Th>
            </tr>
          </thead>
          <tbody>
            {submissions.map((submission) => (
              <Tr key={submission.id} className="hover:bg-gray-50">
                <Td>
                  <Link
                    href={`/student/submissions/${submission.id}`}
                    className="font-medium text-blue-600 hover:text-blue-800"
                  >
                    {submission.assignmentId.slice(0, 8)}
                  </Link>
                </Td>
                <Td>
                  <Badge variant={statusVariant(submission.status)}>
                    {SUBMISSION_STATUS_LABELS[submission.status]}
                  </Badge>
                </Td>
                <Td>
                  {submission.marks !== null
                    ? submission.marks
                    : "—"}
                </Td>
                <Td>{formatUtcDate(submission.submittedAtUtc)}</Td>
                <Td>{formatUtcDate(submission.updatedAtUtc)}</Td>
              </Tr>
            ))}
          </tbody>
        </Table>
      </Card>
    );
  }

  return (
    <RoleGuard allowedRoles={["Student"]}>
      <RoleShell role="Student">
        <div className="space-y-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">
              My Submissions
            </h1>
            <p className="mt-1 text-sm text-gray-500">
              Track your submitted work and grades
            </p>
          </div>
          {content}
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
