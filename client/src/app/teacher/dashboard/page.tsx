"use client";

import Link from "next/link";
import { useAuth } from "@/hooks/useAuth";
import { useApi } from "@/hooks/useApi";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Card } from "@/components/ui/Card";
import { Spinner } from "@/components/ui/Spinner";
import { ErrorState } from "@/components/ui/ErrorState";
import { EmptyState } from "@/components/ui/EmptyState";
import { Table, Th, Td, Tr } from "@/components/ui/Table";
import { teacherAssignmentLinksApi } from "@/lib/api/endpoints";
import { ROLE_LABELS, ROUTES } from "@/lib/constants";

export default function TeacherDashboardPage() {
  const { user, isLoading } = useAuth();
  const { data: links, loading, error, refetch } = useApi(
    () => teacherAssignmentLinksApi.list(),
    []
  );

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Spinner />
      </div>
    );
  }

  const renderAssignments = () => {
    if (loading && !links) {
      return (
        <div className="flex items-center justify-center py-12">
          <Spinner />
        </div>
      );
    }
    if (error) {
      return <ErrorState message={error} onRetry={refetch} />;
    }
    if (!links || links.length === 0) {
      return (
        <EmptyState
          title="No class/subject assignments yet"
          description="Ask an admin to assign you to a class and subject before creating assignments."
        />
      );
    }
    return (
      <Table>
        <thead>
          <tr>
            <Th>Class</Th>
            <Th>Subject</Th>
            <Th>Class ID</Th>
            <Th>Subject ID</Th>
          </tr>
        </thead>
        <tbody>
          {links.map((l) => (
            <Tr key={l.id}>
              <Td className="font-medium text-gray-900">{l.className}</Td>
              <Td>{l.subjectName}</Td>
              <Td className="break-all font-mono text-xs text-gray-500">
                {l.classId}
              </Td>
              <Td className="break-all font-mono text-xs text-gray-500">
                {l.subjectId}
              </Td>
            </Tr>
          ))}
        </tbody>
      </Table>
    );
  };

  return (
    <RoleGuard allowedRoles={["Teacher"]}>
      <RoleShell role="Teacher">
        <div className="space-y-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">
              Welcome back, {user?.name}
            </h1>
            <p className="mt-1 text-sm text-gray-500">
              {ROLE_LABELS.Teacher} dashboard overview
            </p>
          </div>

          <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
            <Card
              title="My Assignments"
              description="Create and manage your assignments"
            >
              <Link
                href={ROUTES.TEACHER_ASSIGNMENTS}
                className="inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800"
              >
                View my assignments →
              </Link>
            </Card>

            <Card
              title="Create Assignment"
              description="Publish a new assignment for your class"
            >
              <Link
                href={ROUTES.TEACHER_ASSIGNMENT_NEW}
                className="inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800"
              >
                Create assignment →
              </Link>
            </Card>

            <Card
              title="Submissions to Review"
              description="Grade and review student submissions"
            >
              <Link
                href={ROUTES.TEACHER_SUBMISSIONS}
                className="inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800"
              >
                Review submissions →
              </Link>
            </Card>
          </div>

          <Card
            title="My Class / Subject Assignments"
            description="The classes and subjects you are assigned to teach."
          >
            {renderAssignments()}
          </Card>
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
