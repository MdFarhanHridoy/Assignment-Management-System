"use client";

import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Spinner } from "@/components/ui/Spinner";
import { useAuth } from "@/hooks/useAuth";
import { ROLE_LABELS } from "@/lib/constants";
import { UserRole } from "@/lib/types";
import { getInitials } from "@/lib/utils";

const ROLE_BADGE_VARIANT: Record<UserRole, "purple" | "blue" | "green"> = {
  Admin: "purple",
  Teacher: "blue",
  Student: "green",
};

export function Topbar() {
  const { user, isLoading, logout } = useAuth();

  return (
    <header className="sticky top-0 z-30 flex h-16 items-center justify-between border-b border-gray-200 bg-white px-6">
      {isLoading ? (
        <Spinner />
      ) : (
        <>
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-indigo-600 text-sm font-semibold text-white">
              {user && user.name ? getInitials(user.name) || "?" : "?"}
            </div>
            <div className="leading-tight">
              <p className="text-sm font-semibold text-gray-900">
                {user?.name ?? "Unknown user"}
              </p>
              <p className="text-xs text-gray-500">{user?.email ?? ""}</p>
            </div>
            {user && (
              <Badge variant={ROLE_BADGE_VARIANT[user.role]} className="ml-2">
                {ROLE_LABELS[user.role]}
              </Badge>
            )}
          </div>

          <Button variant="outline" size="sm" onClick={logout}>
            Logout
          </Button>
        </>
      )}
    </header>
  );
}
