"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { ROLE_LABELS, ROUTES } from "@/lib/constants";
import { UserRole } from "@/lib/types";
import { cn } from "@/lib/utils";

interface NavItem {
  label: string;
  href: string;
}

const NAV_ITEMS: Record<UserRole, NavItem[]> = {
  Admin: [
    { label: "Dashboard", href: ROUTES.ADMIN_DASHBOARD },
    { label: "Users", href: ROUTES.ADMIN_USERS },
    { label: "Classes", href: ROUTES.ADMIN_CLASSES },
    { label: "Subjects", href: ROUTES.ADMIN_SUBJECTS },
    { label: "Teacher Assignments", href: ROUTES.ADMIN_TEACHER_ASSIGNMENTS },
    { label: "Enrollments", href: ROUTES.ADMIN_ENROLLMENTS },
    { label: "Assignments", href: ROUTES.ADMIN_ASSIGNMENTS },
    { label: "Submissions", href: ROUTES.ADMIN_SUBMISSIONS },
  ],
  Teacher: [
    { label: "Dashboard", href: ROUTES.TEACHER_DASHBOARD },
    { label: "Assignments", href: ROUTES.TEACHER_ASSIGNMENTS },
    { label: "Submissions", href: ROUTES.TEACHER_SUBMISSIONS },
  ],
  Student: [
    { label: "Dashboard", href: ROUTES.STUDENT_DASHBOARD },
    { label: "Assignments", href: ROUTES.STUDENT_ASSIGNMENTS },
    { label: "Submissions", href: ROUTES.STUDENT_SUBMISSIONS },
  ],
};

interface SidebarProps {
  role: UserRole;
}

export function Sidebar({ role }: SidebarProps) {
  const pathname = usePathname();
  const items = NAV_ITEMS[role];

  const isActive = (href: string): boolean =>
    pathname === href || pathname.startsWith(`${href}/`);

  return (
    <aside className="fixed inset-y-0 left-0 flex min-h-screen w-64 flex-col bg-gray-900 text-gray-100">
      <div className="flex h-16 items-center border-b border-gray-800 px-6">
        <span className="text-lg font-semibold tracking-tight text-white">
          Assignment MS
        </span>
      </div>

      <nav className="flex-1 overflow-y-auto px-3 py-4">
        <ul className="space-y-1">
          {items.map((item) => {
            const active = isActive(item.href);
            return (
              <li key={item.href}>
                <Link
                  href={item.href}
                  aria-current={active ? "page" : undefined}
                  className={cn(
                    "block rounded-md px-3 py-2 text-sm font-medium transition-colors",
                    active
                      ? "bg-indigo-600 text-white"
                      : "text-gray-300 hover:bg-gray-800 hover:text-white"
                  )}
                >
                  {item.label}
                </Link>
              </li>
            );
          })}
        </ul>
      </nav>

      <div className="border-t border-gray-800 px-6 py-4">
        <p className="text-xs uppercase tracking-wide text-gray-500">Role</p>
        <p className="mt-1 text-sm font-medium text-gray-200">
          {ROLE_LABELS[role]}
        </p>
      </div>
    </aside>
  );
}
