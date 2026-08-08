"use client";

import React from "react";
import { Sidebar } from "./Sidebar";
import { Topbar } from "./Topbar";
import { UserRole } from "@/lib/types";

interface RoleShellProps {
  role: UserRole;
  children: React.ReactNode;
}

export function RoleShell({ role, children }: RoleShellProps) {
  return (
    <div className="min-h-screen bg-gray-50">
      <Sidebar role={role} />
      <div className="ml-64 flex min-h-screen flex-col">
        <Topbar />
        <main className="flex-1 p-6">{children}</main>
      </div>
    </div>
  );
}
