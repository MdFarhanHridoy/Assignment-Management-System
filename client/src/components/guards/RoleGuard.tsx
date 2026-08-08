"use client";

import React, { useEffect } from "react";
import { useRouter } from "next/navigation";
import { Spinner } from "@/components/ui/Spinner";
import { useAuth } from "@/hooks/useAuth";
import { getUserRole } from "@/lib/auth/session";
import { ROLE_DASHBOARD, ROUTES } from "@/lib/constants";
import { UserRole } from "@/lib/types";

interface RoleGuardProps {
  allowedRoles: UserRole[];
  children: React.ReactNode;
}

export function RoleGuard({ allowedRoles, children }: RoleGuardProps) {
  const { user, isLoading } = useAuth();
  const router = useRouter();

  const role: UserRole | null = user?.role ?? getUserRole();

  useEffect(() => {
    if (isLoading) return;

    if (!role) {
      router.replace(ROUTES.LOGIN);
      return;
    }

    if (!allowedRoles.includes(role)) {
      router.replace(ROLE_DASHBOARD[role]);
    }
  }, [isLoading, role, allowedRoles, router]);

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Spinner />
      </div>
    );
  }

  if (!role || !allowedRoles.includes(role)) {
    return null;
  }

  return <>{children}</>;
}
