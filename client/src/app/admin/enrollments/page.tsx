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
  adminEnrollmentsApi,
  adminUsersApi,
  adminClassesApi,
} from "@/lib/api/endpoints";
import { ApiError } from "@/lib/types";
import { formatDate } from "@/lib/utils";

const selectClassName =
  "block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:ring-indigo-500";

function describeMutationError(err: unknown): string {
  const msg = (err as ApiError)?.message ?? "";
  if (/409|conflict|duplicate|already/i.test(msg)) {
    return "This student is already enrolled in that class.";
  }
  if (/404|not found/i.test(msg)) {
    return "One of the selected items no longer exists. Please refresh and try again.";
  }
  return msg || "Something went wrong.";
}

export default function AdminEnrollmentsPage() {
  const enrollments = useApi(() => adminEnrollmentsApi.list(), []);
  const users = useApi(() => adminUsersApi.list(), []);
  const classes = useApi(() => adminClassesApi.list(), []);

  const [classId, setClassId] = useState("");
  const [studentId, setStudentId] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  const loading =
    enrollments.loading || users.loading || classes.loading;
  const loadError = enrollments.error || users.error || classes.error;

  const allDataLoaded = Boolean(enrollments.data && users.data && classes.data);

  const students = (users.data ?? []).filter((u) => u.role === "Student");
  const userMap = new Map((users.data ?? []).map((u) => [u.id, u]));
  const classMap = new Map((classes.data ?? []).map((c) => [c.id, c]));

  const refetchAll = () => {
    enrollments.refetch();
    users.refetch();
    classes.refetch();
  };

  const resetForm = () => {
    setClassId("");
    setStudentId("");
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError(null);
    setActionError(null);
    if (!classId || !studentId) {
      setFormError("Please select a class and a student.");
      return;
    }
    setSubmitting(true);
    try {
      await adminEnrollmentsApi.create({ classId, studentId });
      resetForm();
      enrollments.refetch();
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
      await adminEnrollmentsApi.delete(id);
      enrollments.refetch();
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
    const rows = enrollments.data ?? [];
    if (rows.length === 0) {
      return (
        <EmptyState
          title="No enrollments yet"
          description="Enroll a student into a class using the form above."
        />
      );
    }
    return (
      <Table>
        <thead>
          <tr>
            <Th>Student</Th>
            <Th>Class</Th>
            <Th>Enrolled</Th>
            <Th>Actions</Th>
          </tr>
        </thead>
        <tbody>
          {rows.map((en) => {
            const student = userMap.get(en.studentId);
            const cls = classMap.get(en.classId);
            return (
              <Tr key={en.id}>
                <Td className="font-medium text-gray-900">
                  {student ? student.name : "Unknown student"}
                </Td>
                <Td>{cls ? cls.name : "Unknown class"}</Td>
                <Td>{formatDate(en.enrolledAt)}</Td>
                <Td>
                  <Button
                    variant="danger"
                    size="sm"
                    isLoading={deletingId === en.id}
                    onClick={() => handleDelete(en.id)}
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
            <h1 className="text-2xl font-bold text-gray-900">Enrollments</h1>
            <p className="mt-1 text-sm text-gray-500">
              Enroll students into classes.
            </p>
          </div>

          <Card title="Enroll student" description="Add a student to a class.">
            <form onSubmit={handleCreate} className="space-y-4">
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <div>
                  <label
                    htmlFor="en-class"
                    className="mb-1 block text-sm font-medium text-gray-700"
                  >
                    Class <span className="text-red-500">*</span>
                  </label>
                  <select
                    id="en-class"
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
                    htmlFor="en-student"
                    className="mb-1 block text-sm font-medium text-gray-700"
                  >
                    Student <span className="text-red-500">*</span>
                  </label>
                  <select
                    id="en-student"
                    value={studentId}
                    onChange={(e) => setStudentId(e.target.value)}
                    className={selectClassName}
                    disabled={submitting}
                  >
                    <option value="">Select a student</option>
                    {students.map((s) => (
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
                  Enroll student
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
