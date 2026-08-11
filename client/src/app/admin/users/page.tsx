"use client";

import { useState } from "react";
import { useApi } from "@/hooks/useApi";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Spinner } from "@/components/ui/Spinner";
import { ErrorState } from "@/components/ui/ErrorState";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { Badge } from "@/components/ui/Badge";
import { EmptyState } from "@/components/ui/EmptyState";
import { Table, Th, Td, Tr } from "@/components/ui/Table";
import { adminUsersApi } from "@/lib/api/endpoints";
import { formatDate } from "@/lib/utils";
import type { UserDto, UserRole } from "@/lib/types";

const fieldClass =
  "block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:ring-indigo-500";

const roleBadgeVariant: Record<UserRole, "purple" | "blue" | "green"> = {
  Admin: "purple",
  Teacher: "blue",
  Student: "green",
};

const ROLE_OPTIONS: UserRole[] = ["Admin", "Teacher", "Student"];

function errorMessage(err: unknown, fallback: string): string {
  if (err && typeof err === "object" && "message" in err) {
    const msg = (err as { message?: unknown }).message;
    if (typeof msg === "string" && msg.trim()) return msg;
  }
  return fallback;
}

function isConflict(err: unknown): boolean {
  if (err && typeof err === "object" && "message" in err) {
    const msg = String((err as { message?: unknown }).message ?? "").toLowerCase();
    return msg.includes("409") || msg.includes("already exist");
  }
  return false;
}

interface CreateFormState {
  name: string;
  email: string;
  password: string;
  role: UserRole;
}

interface EditFormState {
  name: string;
  email: string;
  role: UserRole;
  isActive: boolean;
}

export default function AdminUsersPage() {
  const { data, loading, error, refetch } = useApi<UserDto[]>(
    () => adminUsersApi.list(),
    []
  );

  const users = data ?? [];

  const [showCreate, setShowCreate] = useState(false);
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);
  const [createForm, setCreateForm] = useState<CreateFormState>({
    name: "",
    email: "",
    password: "",
    role: "Student",
  });

  const [editingId, setEditingId] = useState<string | null>(null);
  const [updating, setUpdating] = useState(false);
  const [updateError, setUpdateError] = useState<string | null>(null);
  const [editForm, setEditForm] = useState<EditFormState>({
    name: "",
    email: "",
    role: "Student",
    isActive: true,
  });

  const [deletingId, setDeletingId] = useState<string | null>(null);

  function openCreate() {
    setEditingId(null);
    setUpdateError(null);
    setShowCreate((v) => !v);
  }

  function openEdit(u: UserDto) {
    setShowCreate(false);
    setCreateError(null);
    setUpdateError(null);
    setEditForm({
      name: u.name,
      email: u.email,
      role: u.role,
      isActive: u.isActive,
    });
    setEditingId(u.id);
  }

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    setCreating(true);
    setCreateError(null);
    try {
      await adminUsersApi.create({
        name: createForm.name.trim(),
        email: createForm.email.trim(),
        password: createForm.password,
        role: createForm.role,
      });
      setCreateForm({ name: "", email: "", password: "", role: "Student" });
      setShowCreate(false);
      refetch();
    } catch (err) {
      setCreateError(
        isConflict(err)
          ? "A user with this email already exists."
          : errorMessage(err, "Failed to create user.")
      );
    } finally {
      setCreating(false);
    }
  }

  async function handleUpdate(e: React.FormEvent) {
    e.preventDefault();
    if (!editingId) return;
    setUpdating(true);
    setUpdateError(null);
    try {
      await adminUsersApi.update(editingId, {
        name: editForm.name.trim(),
        email: editForm.email.trim(),
        role: editForm.role,
        isActive: editForm.isActive,
      });
      setEditingId(null);
      refetch();
    } catch (err) {
      setUpdateError(
        isConflict(err)
          ? "A user with this email already exists."
          : errorMessage(err, "Failed to update user.")
      );
    } finally {
      setUpdating(false);
    }
  }

  async function handleDelete(u: UserDto) {
    const ok = window.confirm(
      `Delete user "${u.name}"? This action cannot be undone.`
    );
    if (!ok) return;
    setDeletingId(u.id);
    try {
      await adminUsersApi.delete(u.id);
      if (editingId === u.id) setEditingId(null);
      refetch();
    } catch (err) {
      window.alert(errorMessage(err, "Failed to delete user."));
    } finally {
      setDeletingId(null);
    }
  }

  if (loading) {
    return (
      <RoleGuard allowedRoles={["Admin"]}>
        <RoleShell role="Admin">
          <div className="flex justify-center py-12">
            <Spinner />
          </div>
        </RoleShell>
      </RoleGuard>
    );
  }

  if (error) {
    return (
      <RoleGuard allowedRoles={["Admin"]}>
        <RoleShell role="Admin">
          <ErrorState message={error} onRetry={refetch} />
        </RoleShell>
      </RoleGuard>
    );
  }

  return (
    <RoleGuard allowedRoles={["Admin"]}>
      <RoleShell role="Admin">
        <div className="space-y-6">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Users</h1>
              <p className="mt-1 text-sm text-gray-500">
                Create and manage user accounts.
              </p>
            </div>
            <Button onClick={openCreate}>{showCreate ? "Close" : "New User"}</Button>
          </div>

          {showCreate && (
            <Card title="New User" description="Create a new account.">
              <form onSubmit={handleCreate} className="space-y-4">
                {createError && (
                  <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                    {createError}
                  </div>
                )}
                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                  <Input
                    label="Name"
                    name="name"
                    required
                    value={createForm.name}
                    onChange={(e) =>
                      setCreateForm((f) => ({ ...f, name: e.target.value }))
                    }
                    placeholder="Full name"
                  />
                  <Input
                    label="Email"
                    name="email"
                    type="email"
                    required
                    value={createForm.email}
                    onChange={(e) =>
                      setCreateForm((f) => ({ ...f, email: e.target.value }))
                    }
                    placeholder="user@example.com"
                  />
                  <Input
                    label="Password"
                    name="password"
                    type="password"
                    required
                    minLength={6}
                    value={createForm.password}
                    onChange={(e) =>
                      setCreateForm((f) => ({ ...f, password: e.target.value }))
                    }
                    placeholder="At least 6 characters"
                  />
                  <div className="w-full">
                    <label
                      htmlFor="create-role"
                      className="mb-1 block text-sm font-medium text-gray-700"
                    >
                      Role<span className="ml-0.5 text-red-500">*</span>
                    </label>
                    <select
                      id="create-role"
                      className={fieldClass}
                      value={createForm.role}
                      onChange={(e) =>
                        setCreateForm((f) => ({
                          ...f,
                          role: e.target.value as UserRole,
                        }))
                      }
                    >
                      {ROLE_OPTIONS.map((r) => (
                        <option key={r} value={r}>
                          {r}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
                <div className="flex justify-end gap-2">
                  <Button
                    variant="outline"
                    type="button"
                    onClick={() => setShowCreate(false)}
                  >
                    Cancel
                  </Button>
                  <Button type="submit" isLoading={creating}>
                    Create User
                  </Button>
                </div>
              </form>
            </Card>
          )}

          {editingId && (
            <Card title="Edit User" description="Update account details.">
              <form onSubmit={handleUpdate} className="space-y-4">
                {updateError && (
                  <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                    {updateError}
                  </div>
                )}
                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                  <Input
                    label="Name"
                    name="edit-name"
                    required
                    value={editForm.name}
                    onChange={(e) =>
                      setEditForm((f) => ({ ...f, name: e.target.value }))
                    }
                  />
                  <Input
                    label="Email"
                    name="edit-email"
                    type="email"
                    required
                    value={editForm.email}
                    onChange={(e) =>
                      setEditForm((f) => ({ ...f, email: e.target.value }))
                    }
                  />
                  <div className="w-full">
                    <label
                      htmlFor="edit-role"
                      className="mb-1 block text-sm font-medium text-gray-700"
                    >
                      Role<span className="ml-0.5 text-red-500">*</span>
                    </label>
                    <select
                      id="edit-role"
                      className={fieldClass}
                      value={editForm.role}
                      onChange={(e) =>
                        setEditForm((f) => ({
                          ...f,
                          role: e.target.value as UserRole,
                        }))
                      }
                    >
                      {ROLE_OPTIONS.map((r) => (
                        <option key={r} value={r}>
                          {r}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="flex items-end pb-2">
                    <label className="flex items-center gap-2 text-sm text-gray-700">
                      <input
                        type="checkbox"
                        checked={editForm.isActive}
                        onChange={(e) =>
                          setEditForm((f) => ({ ...f, isActive: e.target.checked }))
                        }
                        className="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                      />
                      Active account
                    </label>
                  </div>
                </div>
                <div className="flex justify-end gap-2">
                  <Button
                    variant="outline"
                    type="button"
                    onClick={() => setEditingId(null)}
                  >
                    Cancel
                  </Button>
                  <Button type="submit" isLoading={updating}>
                    Save Changes
                  </Button>
                </div>
              </form>
            </Card>
          )}

          <Card>
            {users.length === 0 ? (
              <EmptyState
                title="No users yet"
                description="Create your first user account to get started."
                action={
                  <Button onClick={() => setShowCreate(true)}>New User</Button>
                }
              />
            ) : (
              <Table>
                <thead>
                  <Tr>
                    <Th>Name</Th>
                    <Th>Email</Th>
                    <Th>Role</Th>
                    <Th>Status</Th>
                    <Th>Created</Th>
                    <Th>Actions</Th>
                  </Tr>
                </thead>
                <tbody>
                  {users.map((u) => (
                    <Tr
                      key={u.id}
                      className={editingId === u.id ? "bg-indigo-50" : undefined}
                    >
                      <Td className="font-medium text-gray-900">{u.name}</Td>
                      <Td>{u.email}</Td>
                      <Td>
                        <Badge variant={roleBadgeVariant[u.role]}>{u.role}</Badge>
                      </Td>
                      <Td>
                        {u.isActive ? (
                          <Badge variant="green">Active</Badge>
                        ) : (
                          <Badge variant="red">Disabled</Badge>
                        )}
                      </Td>
                      <Td>{formatDate(u.createdAt)}</Td>
                      <Td>
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => openEdit(u)}
                          >
                            Edit
                          </Button>
                          <Button
                            variant="danger"
                            size="sm"
                            isLoading={deletingId === u.id}
                            onClick={() => handleDelete(u)}
                          >
                            Delete
                          </Button>
                        </div>
                      </Td>
                    </Tr>
                  ))}
                </tbody>
              </Table>
            )}
          </Card>
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
