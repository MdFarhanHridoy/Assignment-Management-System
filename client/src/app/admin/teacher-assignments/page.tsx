"use client";

import { useState } from "react";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { Spinner } from "@/components/ui/Spinner";
import { ErrorState } from "@/components/ui/ErrorState";
import { EmptyState } from "@/components/ui/EmptyState";
import { Table, Th, Td, Tr } from "@/components/ui/Table";
import { useApi } from "@/hooks/useApi";
import {
  adminTeacherAssignmentsApi,
  adminUsersApi,
  adminClassesApi,
  adminSubjectsApi,
} from "@/lib/api/endpoints";
import { ApiError } from "@/lib/types";
import { formatDate } from "@/lib/utils";

const selectClassName =
  "block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:ring-indigo-500";

function describeMutationError(err: unknown): string {
  const msg = (err as ApiError)?.message ?? "";
  if (/409|conflict|duplicate|already/i.test(msg)) {
    return "This teacher is already assigned to that class and subject.";
  }
  if (/404|not found/i.test(msg)) {
    return "One of the selected items no longer exists. Please refresh and try again.";
  }
  return msg || "Something went wrong.";
}

export default function AdminTeacherAssignmentsPage() {
  const assignments = useApi(() => adminTeacherAssignmentsApi.list(), []);
  const users = useApi(() => adminUsersApi.list(), []);
  const classes = useApi(() => adminClassesApi.list(), []);
  const subjects = useApi(() => adminSubjectsApi.list(), []);

  const [teacherId, setTeacherId] = useState("");
  const [classId, setClassId] = useState("");
  const [subjectId, setSubjectId] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  const loading =
    assignments.loading || users.loading || classes.loading || subjects.loading;
  const loadError =
    assignments.error || users.error || classes.error || subjects.error;

  const allDataLoaded = Boolean(
    assignments.data && users.data && classes.data && subjects.data
  );

  const teachers = (users.data ?? []).filter((u) => u.role === "Teacher");
  const userMap = new Map((users.data ?? []).map((u) => [u.id, u]));
  const classMap = new Map((classes.data ?? []).map((c) => [c.id, c]));
  const subjectMap = new Map((subjects.data ?? []).map((s) => [s.id, s]));

  const refetchAll = () => {
    assignments.refetch();
    users.refetch();
    classes.refetch();
    subjects.refetch();
  };

  const resetForm = () => {
    setTeacherId("");
    setClassId("");
    setSubjectId("");
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError(null);
    setActionError(null);
    if (!teacherId || !classId || !subjectId) {
      setFormError("Please select a teacher, class, and subject.");
      return;
    }
    setSubmitting(true);
    try {
      await adminTeacherAssignmentsApi.create({ teacherId, classId, subjectId });
      resetForm();
      assignments.refetch();
    } catch (err) {
      setFormError(describeMutationError(err));
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    setActionError(null);
    setDeletingId(id);
    try {
      await adminTeacherAssignmentsApi.delete(id);
      assignments.refetch();
    } catch (err) {
      setActionError(describeMutationError(err));
    } finally {
      setDeletingId(null);
    }
  };

  const renderBody = () => {
    if (loading && !allDataLoaded) {
      return (
        <div className="flex items-center justify-center py-12">
          <Spinner />
        </div>
      );
    }
    if (loadError) {
      return <ErrorState message={loadError} onRetry={refetchAll} />;
    }
    const rows = assignments.data ?? [];
    if (rows.length === 0) {
      return (
        <EmptyState
          title="No teacher assignments yet"
          description="Assign a teacher to a class and subject using the form above."
        />
      );
    }
    return (
      <Table>
        <thead>
          <tr>
            <Th>Teacher</Th>
            <Th>Class</Th>
            <Th>Subject</Th>
            <Th>Created</Th>
            <Th>Actions</Th>
          </tr>
        </thead>
        <tbody>
          {rows.map((a) => {
            const teacher = userMap.get(a.teacherId);
            const cls = classMap.get(a.classId);
            const subject = subjectMap.get(a.subjectId);
            return (
              <Tr key={a.id}>
                <Td className="font-medium text-gray-900">
                  {teacher ? teacher.name : "Unknown teacher"}
                </Td>
                <Td>{cls ? cls.name : "Unknown class"}</Td>
                <Td>{subject ? subject.name : "Unknown subject"}</Td>
                <Td>{formatDate(a.createdAt)}</Td>
                <Td>
                  <Button
                    variant="danger"
                    size="sm"
                    isLoading={deletingId === a.id}
                    onClick={() => handleDelete(a.id)}
                  >
                    Delete
                  </Button>
                </Td>
              </Tr>
            );
          })}
        </tbody>
      </Table>
    );
  };

  return (
    <RoleGuard allowedRoles={["Admin"]}>
      <RoleShell role="Admin">
        <div className="space-y-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Teacher Assignments</h1>
            <p className="mt-1 text-sm text-gray-500">
              Assign teachers to teach a subject in a class.
            </p>
          </div>

          <Card title="Assign teacher" description="Link a teacher to a class and subject.">
            <form onSubmit={handleCreate} className="space-y-4">
              <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
                <div>
                  <label
                    htmlFor="ta-teacher"
                    className="mb-1 block text-sm font-medium text-gray-700"
                  >
                    Teacher <span className="text-red-500">*</span>
                  </label>
                  <select
                    id="ta-teacher"
                    value={teacherId}
                    onChange={(e) => setTeacherId(e.target.value)}
                    className={selectClassName}
                    disabled={submitting}
                  >
                    <option value="">Select a teacher</option>
                    {teachers.map((t) => (
                      <option key={t.id} value={t.id}>
                        {t.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label
                    htmlFor="ta-class"
                    className="mb-1 block text-sm font-medium text-gray-700"
                  >
                    Class <span className="text-red-500">*</span>
                  </label>
                  <select
                    id="ta-class"
                    value={classId}
                    onChange={(e) => setClassId(e.target.value)}
                    className={selectClassName}
                    disabled={submitting}
                  >
                    <option value="">Select a class</option>
                    {(classes.data ?? []).map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label
                    htmlFor="ta-subject"
                    className="mb-1 block text-sm font-medium text-gray-700"
                  >
                    Subject <span className="text-red-500">*</span>
                  </label>
                  <select
                    id="ta-subject"
                    value={subjectId}
                    onChange={(e) => setSubjectId(e.target.value)}
                    className={selectClassName}
                    disabled={submitting}
                  >
                    <option value="">Select a subject</option>
                    {(subjects.data ?? []).map((s) => (
                      <option key={s.id} value={s.id}>
                        {s.name}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              {formError && (
                <p className="text-sm text-red-600" role="alert">
                  {formError}
                </p>
              )}

              <div className="flex justify-end">
                <Button type="submit" isLoading={submitting}>
                  Assign teacher
                </Button>
              </div>
            </form>
          </Card>

          <Card>
            {actionError && (
              <div className="mb-4">
                <ErrorState message={actionError} />
              </div>
            )}
            {renderBody()}
          </Card>
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
