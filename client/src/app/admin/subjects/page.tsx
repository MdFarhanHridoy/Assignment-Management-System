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
import { adminSubjectsApi, adminClassesApi } from "@/lib/api/endpoints";
import { formatDate } from "@/lib/utils";
import type { SubjectDto, ClassDto } from "@/lib/types";

const fieldClass =
  "block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:ring-indigo-500";

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

export default function AdminSubjectsPage() {
  const subjectsQuery = useApi<SubjectDto[]>(() => adminSubjectsApi.list(), []);
  const classesQuery = useApi<ClassDto[]>(() => adminClassesApi.list(), []);

  const loading = subjectsQuery.loading || classesQuery.loading;
  const error = subjectsQuery.error ?? classesQuery.error;
  const refetch = () => {
    subjectsQuery.refetch();
    classesQuery.refetch();
  };

  const subjects = subjectsQuery.data ?? [];
  const classes = classesQuery.data ?? [];

  const classMap = new Map<string, string>(classes.map((c) => [c.id, c.name]));
  const noClasses = classes.length === 0;

  const [showCreate, setShowCreate] = useState(false);
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);
  const [createForm, setCreateForm] = useState({ name: "", classId: "" });

  const [editingId, setEditingId] = useState<string | null>(null);
  const [updating, setUpdating] = useState(false);
  const [updateError, setUpdateError] = useState<string | null>(null);
  const [editForm, setEditForm] = useState({ name: "", classId: "" });

  const [deletingId, setDeletingId] = useState<string | null>(null);

  function openCreate() {
    setEditingId(null);
    setUpdateError(null);
    setShowCreate((v) => !v);
  }

  function openEdit(s: SubjectDto) {
    setShowCreate(false);
    setCreateError(null);
    setUpdateError(null);
    setEditForm({ name: s.name, classId: s.classId });
    setEditingId(s.id);
  }

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    setCreating(true);
    setCreateError(null);
    try {
      await adminSubjectsApi.create({
        name: createForm.name.trim(),
        classId: createForm.classId,
      });
      setCreateForm({ name: "", classId: "" });
      setShowCreate(false);
      refetch();
    } catch (err) {
      setCreateError(
        isConflict(err)
          ? "A subject with this name already exists in this class."
          : errorMessage(err, "Failed to create subject.")
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
      await adminSubjectsApi.update(editingId, {
        name: editForm.name.trim(),
        classId: editForm.classId,
      });
      setEditingId(null);
      refetch();
    } catch (err) {
      setUpdateError(
        isConflict(err)
          ? "A subject with this name already exists in this class."
          : errorMessage(err, "Failed to update subject.")
      );
    } finally {
      setUpdating(false);
    }
  }

  async function handleDelete(s: SubjectDto) {
    const ok = window.confirm(
      `Delete subject "${s.name}"? This action cannot be undone.`
    );
    if (!ok) return;
    setDeletingId(s.id);
    try {
      await adminSubjectsApi.delete(s.id);
      if (editingId === s.id) setEditingId(null);
      refetch();
    } catch (err) {
      window.alert(errorMessage(err, "Failed to delete subject."));
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
              <h1 className="text-2xl font-bold text-gray-900">Subjects</h1>
              <p className="mt-1 text-sm text-gray-500">
                Create and manage subjects within classes.
              </p>
            </div>
            <Button onClick={openCreate} disabled={noClasses}>
              {showCreate ? "Close" : "New Subject"}
            </Button>
          </div>

          {noClasses && (
            <div className="rounded-md border border-yellow-200 bg-yellow-50 px-4 py-3 text-sm text-yellow-800">
              You need to create a class before adding subjects.
            </div>
          )}

          {showCreate && (
            <Card title="New Subject" description="Create a new subject.">
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
                    placeholder="e.g. Physics"
                  />
                  <div className="w-full">
                    <label
                      htmlFor="create-classId"
                      className="mb-1 block text-sm font-medium text-gray-700"
                    >
                      Class<span className="ml-0.5 text-red-500">*</span>
                    </label>
                    <select
                      id="create-classId"
                      className={fieldClass}
                      required
                      value={createForm.classId}
                      onChange={(e) =>
                        setCreateForm((f) => ({ ...f, classId: e.target.value }))
                      }
                    >
                      <option value="" disabled>
                        Select a class
                      </option>
                      {classes.map((c) => (
                        <option key={c.id} value={c.id}>
                          {c.name}
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
                    Create Subject
                  </Button>
                </div>
              </form>
            </Card>
          )}

          {editingId && (
            <Card title="Edit Subject" description="Update subject details.">
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
                  <div className="w-full">
                    <label
                      htmlFor="edit-classId"
                      className="mb-1 block text-sm font-medium text-gray-700"
                    >
                      Class<span className="ml-0.5 text-red-500">*</span>
                    </label>
                    <select
                      id="edit-classId"
                      className={fieldClass}
                      required
                      value={editForm.classId}
                      onChange={(e) =>
                        setEditForm((f) => ({ ...f, classId: e.target.value }))
                      }
                    >
                      <option value="" disabled>
                        Select a class
                      </option>
                      {classes.map((c) => (
                        <option key={c.id} value={c.id}>
                          {c.name}
                        </option>
                      ))}
                    </select>
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
            {subjects.length === 0 ? (
              <EmptyState
                title="No subjects yet"
                description={
                  noClasses
                    ? "Create a class first, then add subjects."
                    : "Create your first subject to get started."
                }
                action={
                  !noClasses ? (
                    <Button onClick={() => setShowCreate(true)}>New Subject</Button>
                  ) : undefined
                }
              />
            ) : (
              <Table>
                <thead>
                  <Tr>
                    <Th>Name</Th>
                    <Th>Class</Th>
                    <Th>Created</Th>
                    <Th>Actions</Th>
                  </Tr>
                </thead>
                <tbody>
                  {subjects.map((s) => (
                    <Tr
                      key={s.id}
                      className={editingId === s.id ? "bg-indigo-50" : undefined}
                    >
                      <Td className="font-medium text-gray-900">{s.name}</Td>
                      <Td>{classMap.get(s.classId) ?? <span className="text-gray-400">Unknown</span>}</Td>
                      <Td>{formatDate(s.createdAt)}</Td>
                      <Td>
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => openEdit(s)}
                          >
                            Edit
                          </Button>
                          <Button
                            variant="danger"
                            size="sm"
                            isLoading={deletingId === s.id}
                            onClick={() => handleDelete(s)}
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
