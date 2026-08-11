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
import { adminAssignmentsApi } from "@/lib/api/endpoints";
import { AssignmentStatus } from "@/lib/types";
import { formatDate, formatUtcDate } from "@/lib/utils";

const statusVariant: Record<
  AssignmentStatus,
  "gray" | "green" | "yellow"
> = {
  Draft: "gray",
  Published: "green",
  Archived: "yellow",
};

export default function AdminAssignmentsPage() {
  const { data, loading, error, refetch } = useApi(
    () => adminAssignmentsApi.list(),
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
          title="No assignments found"
          description="There are no assignments in the system yet."
        />
      );
    }
    return (
      <Table>
        <thead>
          <tr>
            <Th>Title</Th>
            <Th>Status</Th>
            <Th>Deadline</Th>
            <Th>Max Marks</Th>
            <Th>Created</Th>
          </tr>
        </thead>
        <tbody>
          {data.map((a) => (
            <Tr key={a.id}>
              <Td className="font-medium text-gray-900">{a.title}</Td>
              <Td>
                <Badge variant={statusVariant[a.status]}>{a.status}</Badge>
              </Td>
              <Td>{formatUtcDate(a.deadlineUtc)}</Td>
              <Td>{a.maxMarks}</Td>
              <Td>{formatDate(a.createdAt)}</Td>
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
            <h1 className="text-2xl font-bold text-gray-900">Assignments</h1>
            <p className="mt-1 text-sm text-gray-500">
              Read-only overview of all assignments across the system.
            </p>
          </div>

          <Card>{renderBody()}</Card>
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
