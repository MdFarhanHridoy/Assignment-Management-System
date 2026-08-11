"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Spinner } from "@/components/ui/Spinner";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { Table, Th, Td, Tr } from "@/components/ui/Table";
import { useApi } from "@/hooks/useApi";
import { teacherSubmissionsApi } from "@/lib/api/endpoints";
import { SubmissionDto, SubmissionStatus } from "@/lib/types";
import { formatUtcDate } from "@/lib/utils";

const statusVariant: Record<
  SubmissionStatus,
  "blue" | "yellow" | "green" | "red"
> = {
  Submitted: "blue",
  UnderReview: "yellow",
  Reviewed: "green",
  LateSubmitted: "red",
};

export default function AssignmentSubmissionsPage() {
  const params = useParams<{ id: string }>();
  const assignmentId = params.id;

  const { data, loading, error, refetch } = useApi(
    () => teacherSubmissionsApi.listByAssignment(assignmentId),
    [assignmentId]
  );

  return (
    <RoleGuard allowedRoles={["Teacher"]}>
      <RoleShell role="Teacher">
        <div className="space-y-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Submissions</h1>
            <p className="mt-1 text-sm text-gray-500">
              Review submissions for this assignment.
            </p>
          </div>

          <Card>
            {loading ? (
              <div className="flex items-center justify-center py-12">
                <Spinner />
              </div>
            ) : error ? (
              <ErrorState message={error} onRetry={refetch} />
            ) : !data || data.length === 0 ? (
              <EmptyState title="No submissions for this assignment yet" />
            ) : (
              <Table>
                <thead>
                  <tr>
                    <Th>Student</Th>
                    <Th>Status</Th>
                    <Th>Marks</Th>
                    <Th>Submitted</Th>
                    <Th>Actions</Th>
                  </tr>
                </thead>
                <tbody>
                  {data.map((s: SubmissionDto) => (
                    <Tr key={s.id}>
                      <Td className="font-mono text-gray-900">
                        {s.studentId.slice(0, 8)}
                      </Td>
                      <Td>
                        <Badge variant={statusVariant[s.status]}>
                          {s.status}
                        </Badge>
                      </Td>
                      <Td>
                        {s.marks != null ? (
                          <span className="font-medium text-gray-900">
                            {s.marks}
                          </span>
                        ) : (
                          <span className="text-gray-400">Not reviewed</span>
                        )}
                      </Td>
                      <Td>{formatUtcDate(s.submittedAtUtc)}</Td>
                      <Td>
                        <Link
                          href={`/teacher/submissions/${s.id}`}
                          className="text-sm font-medium text-indigo-600 hover:text-indigo-800"
                        >
                          Review
                        </Link>
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
