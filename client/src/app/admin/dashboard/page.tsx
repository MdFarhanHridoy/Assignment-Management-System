"use client";

import Link from "next/link";
import { useAuth } from "@/hooks/useAuth";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Card } from "@/components/ui/Card";
import { Spinner } from "@/components/ui/Spinner";
import { ROLE_LABELS, ROUTES } from "@/lib/constants";

export default function AdminDashboardPage() {
  const { user, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Spinner />
      </div>
    );
  }

  return (
    <RoleGuard allowedRoles={["Admin"]}>
      <RoleShell role="Admin">
        <div className="space-y-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">
              Welcome back, {user?.name}
            </h1>
            <p className="mt-1 text-sm text-gray-500">
              {ROLE_LABELS.Admin} dashboard overview
            </p>
          </div>

          <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
            <Card title="Users" description="Manage users, classes, subjects">
              <Link
                href={ROUTES.ADMIN_USERS}
                className="inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800"
              >
                Manage users →
              </Link>
            </Card>

            <Card title="Assignments" description="View all assignments">
              <Link
                href={ROUTES.ADMIN_ASSIGNMENTS}
                className="inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800"
              >
                View assignments →
              </Link>
            </Card>

            <Card title="Submissions" description="View all submissions">
              <Link
                href={ROUTES.ADMIN_SUBMISSIONS}
                className="inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800"
              >
                View submissions →
              </Link>
            </Card>
          </div>
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
