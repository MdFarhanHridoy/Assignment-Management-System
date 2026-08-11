"use client";

import Link from "next/link";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Spinner } from "@/components/ui/Spinner";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { Table, Th, Td, Tr } from "@/components/ui/Table";
import { useApi } from "@/hooks/useApi";
import { teacherAssignmentsApi, teacherSubmissionsApi } from "@/lib/api/endpoints";
import { AssignmentDto, SubmissionDto, SubmissionStatus } from "@/lib/types";
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

interface SubmissionRow {
  submission: SubmissionDto;
  assignmentTitle: string;
}

function shortId(id: string): string {
  return id && id.length > 8 ? `${id.slice(0, 8)}...` : id;
}

async function fetchTeacherSubmissions(): Promise<SubmissionRow[]> {
  const assignments: AssignmentDto[] = await teacherAssignmentsApi.list();
  const titleByAssignmentId = new Map(
    assignments.map((a) => [a.id, a.title])
  );

  const perAssignment = await Promise.all(
    assignments.map((assignment) =>
      teacherSubmissionsApi
        .listByAssignment(assignment.id)
        .then((submissions) => submissions)
        .catch(() => [] as SubmissionDto[])
    )
  );

  return perAssignment
    .flat()
    .map((submission) => ({
      submission,
      assignmentTitle: titleByAssignmentId.get(submission.assignmentId) ?? "",
    }))
    .sort((a, b) =>
      b.submission.submittedAtUtc.localeCompare(a.submission.submittedAtUtc)
    );
}

export default function TeacherSubmissionsPage() {
  const { data, loading, error, refetch } = useApi(
    () => fetchTeacherSubmissions(),
    []
  );

  return (
    <RoleGuard allowedRoles={["Teacher"]}>
      <RoleShell role="Teacher">
        <div className="space-y-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Submissions</h1>
            <p className="mt-1 text-sm text-gray-500">
              Review submissions across all your assignments.
            </p>
          </div>

          <Card>
            {loading && !data ? (
              <div className="flex items-center justify-center py-12">
                <Spinner />
              </div>
            ) : error ? (
              <ErrorState message={error} onRetry={refetch} />
            ) : !data || data.length === 0 ? (
              <EmptyState
                title="No submissions yet"
                description="Students' submissions to your assignments will appear here."
              />
            ) : (
              <Table>
                <thead>
                  <tr>
                    <Th>Assignment</Th>
                    <Th>Student</Th>
                    <Th>Status</Th>
                    <Th>Marks</Th>
                    <Th>Submitted</Th>
                    <Th>Actions</Th>
                  </tr>
                </thead>
                <tbody>
                  {data.map((row) => {
                    const s = row.submission;
                    const title =
                      row.assignmentTitle || shortId(s.assignmentId);
                    return (
                      <Tr key={s.id}>
                        <Td className="font-medium text-gray-900">{title}</Td>
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
                    );
                  })}
                </tbody>
              </Table>
            )}
          </Card>
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
