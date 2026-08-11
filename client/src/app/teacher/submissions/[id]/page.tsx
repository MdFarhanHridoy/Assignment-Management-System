"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { Badge } from "@/components/ui/Badge";
import { Spinner } from "@/components/ui/Spinner";
import { ErrorState } from "@/components/ui/ErrorState";
import { useApi } from "@/hooks/useApi";
import { teacherAssignmentsApi, teacherSubmissionsApi } from "@/lib/api/endpoints";
import {
  ApiError,
  AssignmentDto,
  SubmissionDto,
  SubmissionStatus,
} from "@/lib/types";
import { formatUtcDate } from "@/lib/utils";

const SELECT_TEXTAREA_CLASS =
  "block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:ring-indigo-500";

const statusVariant: Record<
  SubmissionStatus,
  "blue" | "yellow" | "green" | "red"
> = {
  Submitted: "blue",
  UnderReview: "yellow",
  Reviewed: "green",
  LateSubmitted: "red",
};

function hasStatus(err: unknown, status: number): boolean {
  const message = (err as { message?: string })?.message ?? "";
  return message.includes(String(status));
}

interface SubmissionWithContext {
  submission: SubmissionDto;
  assignment: AssignmentDto;
}

async function fetchSubmissionWithContext(
  submissionId: string
): Promise<SubmissionWithContext> {
  const assignments = await teacherAssignmentsApi.list();
  const perAssignment = await Promise.all(
    assignments.map((assignment) =>
      teacherSubmissionsApi
        .listByAssignment(assignment.id)
        .then((submissions) => ({ assignment, submissions }))
        .catch(() => ({ assignment, submissions: [] as SubmissionDto[] }))
    )
  );

  for (const { assignment, submissions } of perAssignment) {
    const found = submissions.find((s) => s.id === submissionId);
    if (found) {
      return { submission: found, assignment };
    }
  }

  throw { message: "Submission not found." } as ApiError;
}

export default function ReviewSubmissionPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const submissionId = params.id;

  const { data, loading, error, refetch } = useApi(
    () => fetchSubmissionWithContext(submissionId),
    [submissionId]
  );

  const [marks, setMarks] = useState("");
  const [feedback, setFeedback] = useState("");
  const [status, setStatus] = useState<SubmissionStatus>("Reviewed");
  const [marksError, setMarksError] = useState<string | undefined>(undefined);
  const [apiError, setApiError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (data) {
      setMarks(
        data.submission.marks != null ? String(data.submission.marks) : ""
      );
      setFeedback(data.submission.feedback ?? "");
      setStatus(
        data.submission.status === "UnderReview"
          ? "UnderReview"
          : "Reviewed"
      );
    }
  }, [data]);

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setApiError(null);
    setMarksError(undefined);

    if (!data) return;

    const marksNum = Number(marks);
    const maxMarks = data.assignment.maxMarks;

    if (!marks || isNaN(marksNum)) {
      setMarksError("Marks is required");
      return;
    }
    if (marksNum < 0 || marksNum > maxMarks) {
      setMarksError(`Marks must be between 0 and ${maxMarks}`);
      return;
    }

    setIsSubmitting(true);
    try {
      await teacherSubmissionsApi.review(submissionId, {
        marks: marksNum,
        feedback: feedback.trim() ? feedback.trim() : undefined,
        status,
      });
      router.push(
        `/teacher/assignments/${data.submission.assignmentId}/submissions`
      );
    } catch (err) {
      const message =
        (err as ApiError)?.message ?? "Failed to submit review.";
      if (hasStatus(err, 400)) {
        setApiError(
          "Marks are out of range. Please check the maximum marks for this assignment."
        );
      } else if (
        hasStatus(err, 403) ||
        /forbidden|not allowed|not your|not owner/i.test(message)
      ) {
        setApiError("You are not allowed to review this submission.");
      } else {
        setApiError(message);
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <RoleGuard allowedRoles={["Teacher"]}>
      <RoleShell role="Teacher">
        <div className="mx-auto max-w-3xl space-y-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">
              Review Submission
            </h1>
            <p className="mt-1 text-sm text-gray-500">
              Grade and provide feedback for this student submission.
            </p>
          </div>

          {loading ? (
            <div className="flex items-center justify-center py-12">
              <Spinner />
            </div>
          ) : error ? (
            <ErrorState message={error} onRetry={refetch} />
          ) : !data ? (
            <div className="flex items-center justify-center py-12">
              <Spinner />
            </div>
          ) : (
            <>
              <Card title="Student Answer">
                <dl className="space-y-3 text-sm">
                  <div className="flex items-center gap-3">
                    <dt className="text-gray-500">Student:</dt>
                    <dd className="font-mono text-gray-900">
                      {data.submission.studentId.slice(0, 8)}
                    </dd>
                    <Badge variant={statusVariant[data.submission.status]}>
                      {data.submission.status}
                    </Badge>
                  </div>
                  <div className="flex items-center gap-3">
                    <dt className="text-gray-500">Submitted:</dt>
                    <dd className="text-gray-900">
                      {formatUtcDate(data.submission.submittedAtUtc)}
                    </dd>
                  </div>
                  <div className="flex items-center gap-3">
                    <dt className="text-gray-500">Max marks:</dt>
                    <dd className="text-gray-900">
                      {data.assignment.maxMarks}
                    </dd>
                  </div>
                </dl>
                <div className="mt-4 whitespace-pre-wrap rounded-md bg-gray-50 p-4 text-sm text-gray-800">
                  {data.submission.answerText || (
                    <span className="text-gray-400">
                      No answer text provided.
                    </span>
                  )}
                </div>
              </Card>

              <Card title="Review">
                {apiError && (
                  <div
                    role="alert"
                    className="mb-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
                  >
                    {apiError}
                  </div>
                )}

                <form onSubmit={handleSubmit} className="space-y-5" noValidate>
                  <Input
                    label={`Marks (out of ${data.assignment.maxMarks})`}
                    name="marks"
                    type="number"
                    min={0}
                    max={data.assignment.maxMarks}
                    step={0.5}
                    value={marks}
                    onChange={(e) => setMarks(e.target.value)}
                    error={marksError}
                    required
                  />

                  <div className="w-full">
                    <label
                      htmlFor="feedback"
                      className="mb-1 block text-sm font-medium text-gray-700"
                    >
                      Feedback
                    </label>
                    <textarea
                      id="feedback"
                      name="feedback"
                      rows={4}
                      placeholder="Provide feedback for the student (optional)..."
                      value={feedback}
                      onChange={(e) => setFeedback(e.target.value)}
                      className={SELECT_TEXTAREA_CLASS}
                    />
                  </div>

                  <div className="w-full">
                    <label
                      htmlFor="status"
                      className="mb-1 block text-sm font-medium text-gray-700"
                    >
                      Status
                    </label>
                    <select
                      id="status"
                      name="status"
                      value={status}
                      onChange={(e) =>
                        setStatus(e.target.value as SubmissionStatus)
                      }
                      className={SELECT_TEXTAREA_CLASS}
                    >
                      <option value="Reviewed">Reviewed</option>
                      <option value="UnderReview">Under Review</option>
                    </select>
                  </div>

                  <div className="flex justify-end">
                    <Button type="submit" isLoading={isSubmitting}>
                      Save Review
                    </Button>
                  </div>
                </form>
              </Card>
            </>
          )}
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
