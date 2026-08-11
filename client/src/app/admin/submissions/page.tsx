"use client";

import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Card } from "@/components/ui/Card";
import { Spinner } from "@/components/ui/Spinner";
import { ErrorState } from "@/components/ui/ErrorState";
import { EmptyState } from "@/components/ui/EmptyState";
import { Badge } from "@/components/ui/Badge";
import { Table, Th, Td, Tr } from "@/components/ui/Table";
import { useApi } from "@/hooks/useApi";
import { adminSubmissionsApi } from "@/lib/api/endpoints";
import { SubmissionStatus } from "@/lib/types";
import { formatUtcDate } from "@/lib/utils";

const statusVariant: Record<
  SubmissionStatus,
  "gray" | "yellow" | "green" | "red"
> = {
  Submitted: "gray",
  UnderReview: "yellow",
  Reviewed: "green",
  LateSubmitted: "red",
};

function shortId(id: string): string {
  return id && id.length > 8 ? `${id.slice(0, 8)}...` : id;
}

export default function AdminSubmissionsPage() {
  const { data, loading, error, refetch } = useApi(
    () => adminSubmissionsApi.list(),
    []
  );

  const renderBody = () => {
    if (loading && !data) {
      return (
        <div className="flex items-center justify-center py-12">
          <Spinner />
        </div>
      );
    }
    if (error) {
      return <ErrorState message={error} onRetry={refetch} />;
    }
    if (!data || data.length === 0) {
      return (
        <EmptyState
          title="No submissions found"
          description="There are no submissions in the system yet."
        />
      );
    }
    return (
      <Table>
        <thead>
          <tr>
            <Th>Assignment</Th>
            <Th>Student</Th>
            <Th>Status</Th>
            <Th>Marks</Th>
            <Th>Submitted</Th>
            <Th>Reviewed</Th>
          </tr>
        </thead>
        <tbody>
          {data.map((s) => (
            <Tr key={s.id}>
              <Td className="font-mono text-gray-900">{shortId(s.assignmentId)}</Td>
              <Td className="font-mono text-gray-900">{shortId(s.studentId)}</Td>
              <Td>
                <Badge variant={statusVariant[s.status]}>{s.status}</Badge>
              </Td>
              <Td>{s.marks === null ? "—" : s.marks}</Td>
              <Td>{formatUtcDate(s.submittedAtUtc)}</Td>
              <Td>
                {s.reviewedAtUtc ? formatUtcDate(s.reviewedAtUtc) : "—"}
              </Td>
            </Tr>
          ))}
        </tbody>
      </Table>
    );
  };

  return (
    <RoleGuard allowedRoles={["Admin"]}>
      <RoleShell role="Admin">
        <div className="space-y-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Submissions</h1>
            <p className="mt-1 text-sm text-gray-500">
              Read-only overview of all submissions across the system.
            </p>
          </div>

          <Card>{renderBody()}</Card>
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
