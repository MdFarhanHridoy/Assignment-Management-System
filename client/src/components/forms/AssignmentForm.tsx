"use client";

import React, { useState } from "react";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { useApi } from "@/hooks/useApi";
import { teacherAssignmentLinksApi } from "@/lib/api/endpoints";
import {
  CreateAssignmentRequest,
  UpdateAssignmentRequest,
} from "@/lib/types";

interface AssignmentFormProps {
  initialData?: Partial<CreateAssignmentRequest>;
  onSubmit: (
    data: CreateAssignmentRequest | UpdateAssignmentRequest
  ) => Promise<{ success: boolean; error?: string }>;
  submitLabel?: string;
}

const TEXTAREA_SELECT_CLASS =
  "block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:ring-indigo-500";

function isoToDatetimeLocal(iso?: string): string {
  if (!iso) return "";
  const date = new Date(iso);
  if (isNaN(date.getTime())) return "";
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(
    date.getDate()
  )}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function datetimeLocalToIso(value: string): string {
  return new Date(value).toISOString();
}

export function AssignmentForm({
  initialData,
  onSubmit,
  submitLabel = "Create Assignment",
}: AssignmentFormProps) {
  const [title, setTitle] = useState(initialData?.title ?? "");
  const [description, setDescription] = useState(initialData?.description ?? "");
  const [deadlineLocal, setDeadlineLocal] = useState(
    isoToDatetimeLocal(initialData?.deadlineUtc)
  );
  const [maxMarks, setMaxMarks] = useState(
    initialData?.maxMarks != null ? String(initialData.maxMarks) : ""
  );
  const [classId, setClassId] = useState(initialData?.classId ?? "");
  const [subjectId, setSubjectId] = useState(initialData?.subjectId ?? "");
  const [allowResubmission, setAllowResubmission] = useState(
    initialData?.allowResubmission ?? true
  );

  const { data: links, loading: linksLoading, error: linksError } = useApi(
    () => teacherAssignmentLinksApi.list(),
    []
  );

  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [apiError, setApiError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setApiError(null);

    const errors: Record<string, string> = {};
    if (!title.trim()) errors.title = "Title is required";
    if (!description.trim()) errors.description = "Description is required";
    if (!deadlineLocal) errors.deadlineUtc = "Deadline is required";
    const marksNum = Number(maxMarks);
    if (!maxMarks || isNaN(marksNum) || marksNum < 1)
      errors.maxMarks = "Max marks must be at least 1";
    if (!classId || !subjectId)
      errors.assignment = "Please select a class and subject.";

    setFieldErrors(errors);
    if (Object.keys(errors).length > 0) return;

    const payload: CreateAssignmentRequest = {
      title: title.trim(),
      description: description.trim(),
      deadlineUtc: datetimeLocalToIso(deadlineLocal),
      maxMarks: marksNum,
      classId: classId.trim(),
      subjectId: subjectId.trim(),
      allowResubmission,
    };

    setIsSubmitting(true);
    const result = await onSubmit(payload);
    setIsSubmitting(false);

    if (!result.success) {
      setApiError(result.error ?? "Something went wrong. Please try again.");
    }
  };

  const linksData = links ?? [];
  const noLinks = !linksLoading && !linksError && linksData.length === 0;
  const selectedKey = classId && subjectId ? `${classId}|${subjectId}` : "";
  const selectedAvailable = linksData.some(
    (l) => `${l.classId}|${l.subjectId}` === selectedKey
  );
  const showCurrentFallback =
    !noLinks && selectedKey !== "" && !selectedAvailable;

  const handleLinkChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const v = e.target.value;
    if (!v) {
      setClassId("");
      setSubjectId("");
      return;
    }
    const [cid, sid] = v.split("|");
    setClassId(cid);
    setSubjectId(sid);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-5" noValidate>
      {apiError && (
        <div
          role="alert"
          className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
        >
          {apiError}
        </div>
      )}

      <Input
        label="Title"
        name="title"
        placeholder="e.g. Midterm Essay"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        error={fieldErrors.title}
        required
      />

      <div className="w-full">
        <label
          htmlFor="description"
          className="mb-1 block text-sm font-medium text-gray-700"
        >
          Description<span className="ml-0.5 text-red-500">*</span>
        </label>
        <textarea
          id="description"
          name="description"
          rows={5}
          placeholder="Describe the assignment requirements..."
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          required
          className={TEXTAREA_SELECT_CLASS}
        />
        {fieldErrors.description && (
          <p className="mt-1 text-sm text-red-600">{fieldErrors.description}</p>
        )}
      </div>

      <Input
        label="Deadline (UTC)"
        name="deadlineUtc"
        type="datetime-local"
        value={deadlineLocal}
        onChange={(e) => setDeadlineLocal(e.target.value)}
        error={fieldErrors.deadlineUtc}
        hint="Stored as UTC. Select the date and time."
        required
      />

      <Input
        label="Max Marks"
        name="maxMarks"
        type="number"
        min={1}
        step={1}
        placeholder="e.g. 100"
        value={maxMarks}
        onChange={(e) => setMaxMarks(e.target.value)}
        error={fieldErrors.maxMarks}
        required
      />

      <div className="w-full">
        <label
          htmlFor="assignment-link"
          className="mb-1 block text-sm font-medium text-gray-700"
        >
          Class — Subject<span className="ml-0.5 text-red-500">*</span>
        </label>
        {linksLoading ? (
          <p className="text-sm text-gray-500">Loading your assignments…</p>
        ) : linksError ? (
          <p className="text-sm text-red-600">
            Could not load your class/subject assignments. {linksError}
          </p>
        ) : noLinks ? (
          <p className="text-sm text-red-600">
            You have no class/subject assignments yet. Ask an admin to assign
            you first.
          </p>
        ) : (
          <select
            id="assignment-link"
            value={selectedKey}
            onChange={handleLinkChange}
            className={TEXTAREA_SELECT_CLASS}
          >
            <option value="">Select a class/subject</option>
            {showCurrentFallback && (
              <option value={selectedKey}>
                {classId.slice(0, 8)}… — {subjectId.slice(0, 8)}… (current)
              </option>
            )}
            {linksData.map((l) => (
              <option key={l.id} value={`${l.classId}|${l.subjectId}`}>
                {l.className} — {l.subjectName}
              </option>
            ))}
          </select>
        )}
        {fieldErrors.assignment && (
          <p className="mt-1 text-sm text-red-600">{fieldErrors.assignment}</p>
        )}
      </div>

      <div className="flex items-center gap-2">
        <input
          id="allowResubmission"
          name="allowResubmission"
          type="checkbox"
          checked={allowResubmission}
          onChange={(e) => setAllowResubmission(e.target.checked)}
          className="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
        />
        <label
          htmlFor="allowResubmission"
          className="text-sm font-medium text-gray-700"
        >
          Allow students to resubmit before the deadline
        </label>
      </div>

      <div className="flex justify-end">
        <Button type="submit" isLoading={isSubmitting}>
          {submitLabel}
        </Button>
      </div>
    </form>
  );
}
