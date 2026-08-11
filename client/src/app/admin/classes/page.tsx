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
import { EmptyState } from "@/components/ui/EmptyState";
import { Table, Th, Td, Tr } from "@/components/ui/Table";
import { adminClassesApi } from "@/lib/api/endpoints";
import { formatDate } from "@/lib/utils";
import type { ClassDto } from "@/lib/types";

const fieldClass =
  "block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:ring-indigo-500";

function errorMessage(err: unknown, fallback: string): string {
  if (err && typeof err === "object" && "message" in err) {
    const msg = (err as { message?: unknown }).message;
    if (typeof msg === "string" && msg.trim()) return msg;
  }
  return fallback;
}

export default function AdminClassesPage() {
  const { data, loading, error, refetch } = useApi<ClassDto[]>(
    () => adminClassesApi.list(),
    []
  );

  const classes = data ?? [];

  const [showCreate, setShowCreate] = useState(false);
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);
  const [createForm, setCreateForm] = useState({ name: "", description: "" });

  const [editingId, setEditingId] = useState<string | null>(null);
  const [updating, setUpdating] = useState(false);
  const [updateError, setUpdateError] = useState<string | null>(null);
  const [editForm, setEditForm] = useState({ name: "", description: "" });

  const [deletingId, setDeletingId] = useState<string | null>(null);

  function openCreate() {
    setEditingId(null);
    setUpdateError(null);
    setShowCreate((v) => !v);
  }

  function openEdit(c: ClassDto) {
    setShowCreate(false);
    setCreateError(null);
    setUpdateError(null);
    setEditForm({
      name: c.name,
      description: c.description ?? "",
    });
    setEditingId(c.id);
  }

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    setCreating(true);
    setCreateError(null);
    try {
      const description = createForm.description.trim();
      await adminClassesApi.create({
        name: createForm.name.trim(),
        description: description ? description : undefined,
      });
      setCreateForm({ name: "", description: "" });
      setShowCreate(false);
      refetch();
    } catch (err) {
      setCreateError(errorMessage(err, "Failed to create class."));
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
      const description = editForm.description.trim();
      await adminClassesApi.update(editingId, {
        name: editForm.name.trim(),
        description: description ? description : undefined,
      });
      setEditingId(null);
      refetch();
    } catch (err) {
      setUpdateError(errorMessage(err, "Failed to update class."));
    } finally {
      setUpdating(false);
    }
  }

  async function handleDelete(c: ClassDto) {
    const ok = window.confirm(
      `Delete class "${c.name}"? This action cannot be undone.`
    );
    if (!ok) return;
    setDeletingId(c.id);
    try {
      await adminClassesApi.delete(c.id);
      if (editingId === c.id) setEditingId(null);
      refetch();
    } catch (err) {
      window.alert(errorMessage(err, "Failed to delete class."));
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
              <h1 className="text-2xl font-bold text-gray-900">Classes</h1>
              <p className="mt-1 text-sm text-gray-500">
                Create and manage classes.
              </p>
            </div>
            <Button onClick={openCreate}>{showCreate ? "Close" : "New Class"}</Button>
          </div>

          {showCreate && (
            <Card title="New Class" description="Create a new class.">
              <form onSubmit={handleCreate} className="space-y-4">
                {createError && (
                  <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                    {createError}
                  </div>
                )}
                <Input
                  label="Name"
                  name="name"
                  required
                  value={createForm.name}
                  onChange={(e) =>
                    setCreateForm((f) => ({ ...f, name: e.target.value }))
                  }
                  placeholder="e.g. Class 9 - Science"
                />
                <div className="w-full">
                  <label
                    htmlFor="create-description"
                    className="mb-1 block text-sm font-medium text-gray-700"
                  >
                    Description
                  </label>
                  <textarea
                    id="create-description"
                    className={fieldClass}
                    rows={3}
                    value={createForm.description}
                    onChange={(e) =>
                      setCreateForm((f) => ({ ...f, description: e.target.value }))
                    }
                    placeholder="Optional description"
                  />
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
                    Create Class
                  </Button>
                </div>
              </form>
            </Card>
          )}

          {editingId && (
            <Card title="Edit Class" description="Update class details.">
              <form onSubmit={handleUpdate} className="space-y-4">
                {updateError && (
                  <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                    {updateError}
                  </div>
                )}
                <Input
                  label="Name"
                  name="edit-name"
                  required
                  value={editForm.name}
                  onChange={(e) =>
                    setEditForm((f) => ({ ...f, name: e.target.value }))
                  }
                />
                <div className="w-full">
                  <label
                    htmlFor="edit-description"
                    className="mb-1 block text-sm font-medium text-gray-700"
                  >
                    Description
                  </label>
                  <textarea
                    id="edit-description"
                    className={fieldClass}
                    rows={3}
                    value={editForm.description}
                    onChange={(e) =>
                      setEditForm((f) => ({ ...f, description: e.target.value }))
                    }
                  />
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
            {classes.length === 0 ? (
              <EmptyState
                title="No classes yet"
                description="Create your first class to get started."
                action={
                  <Button onClick={() => setShowCreate(true)}>New Class</Button>
                }
              />
            ) : (
              <Table>
                <thead>
                  <Tr>
                    <Th>Name</Th>
                    <Th>Description</Th>
                    <Th>Created</Th>
                    <Th>Actions</Th>
                  </Tr>
                </thead>
                <tbody>
                  {classes.map((c) => (
                    <Tr
                      key={c.id}
                      className={editingId === c.id ? "bg-indigo-50" : undefined}
                    >
                      <Td className="font-medium text-gray-900">{c.name}</Td>
                      <Td>
                        {c.description ? (
                          c.description
                        ) : (
                          <span className="text-gray-400">&mdash;</span>
                        )}
                      </Td>
                      <Td>{formatDate(c.createdAt)}</Td>
                      <Td>
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => openEdit(c)}
                          >
                            Edit
                          </Button>
                          <Button
                            variant="danger"
                            size="sm"
                            isLoading={deletingId === c.id}
                            onClick={() => handleDelete(c)}
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
