"use client";

import Link from "next/link";
import { useAuth } from "@/hooks/useAuth";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Card } from "@/components/ui/Card";
import { Spinner } from "@/components/ui/Spinner";
import { ROLE_LABELS, ROUTES } from "@/lib/constants";

export default function StudentDashboardPage() {
  const { user, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Spinner />
      </div>
    );
  }

  return (
    <RoleGuard allowedRoles={["Student"]}>
      <RoleShell role="Student">
        <div className="space-y-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">
              Welcome back, {user?.name}
            </h1>
            <p className="mt-1 text-sm text-gray-500">
              {ROLE_LABELS.Student} dashboard overview
            </p>
          </div>

          <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
            <Card
              title="My Assignments"
              description="Browse assignments assigned to you"
            >
              <Link
                href={ROUTES.STUDENT_ASSIGNMENTS}
                className="inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800"
              >
                View my assignments →
              </Link>
            </Card>

            <Card
              title="My Submissions"
              description="Track your submitted work and grades"
            >
              <Link
                href={ROUTES.STUDENT_SUBMISSIONS}
                className="inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800"
              >
                View my submissions →
              </Link>
            </Card>
          </div>
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
