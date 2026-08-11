"use client";

import React, { useMemo } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { SubmissionForm } from "@/components/forms/SubmissionForm";
import { Badge } from "@/components/ui/Badge";
import { Card } from "@/components/ui/Card";
import { ErrorState } from "@/components/ui/ErrorState";
import { Spinner } from "@/components/ui/Spinner";
import { useApi } from "@/hooks/useApi";
import {
  studentAssignmentsApi,
  studentSubmissionsApi,
} from "@/lib/api/endpoints";
import { AssignmentDto, SubmissionDto } from "@/lib/types";
import { formatUtcDate, isDeadlinePassed } from "@/lib/utils";

function extractErrorMessage(err: unknown, fallback: string): string {
  if (err && typeof err === "object" && "message" in err) {
    const message = (err as { message?: unknown }).message;
    if (typeof message === "string" && message.trim()) return message;
  }
  return fallback;
}

export default function StudentAssignmentDetailPage() {
  const router = useRouter();
  const params = useParams();
  const idParam = params?.id;
  const id = Array.isArray(idParam) ? idParam[0] : idParam ?? "";

  const {
    data: assignment,
    loading: assignmentLoading,
    error: assignmentError,
    refetch: refetchAssignment,
  } = useApi<AssignmentDto>(() => studentAssignmentsApi.get(id), [id]);

  const {
    data: submissions,
    loading: submissionsLoading,
    error: submissionsError,
    refetch: refetchSubmissions,
  } = useApi<SubmissionDto[]>(() => studentSubmissionsApi.list(), []);

  const existingSubmission = useMemo(() => {
    if (!submissions) return null;
    return submissions.find((s) => s.assignmentId === id) ?? null;
  }, [submissions, id]);

  const loading = assignmentLoading || submissionsLoading;

  const handleSubmit = async (
    answerText: string
  ): Promise<{ success: boolean; error?: string }> => {
    try {
      await studentSubmissionsApi.submit(id, { answerText });
      router.push("/student/submissions");
      return { success: true };
    } catch (err) {
      return {
        success: false,
        error: extractErrorMessage(err, "Failed to submit. Please try again."),
      };
    }
  };

  const handleUpdate = async (
    answerText: string
  ): Promise<{ success: boolean; error?: string }> => {
    const submission = existingSubmission;
    if (!submission) {
      return { success: false, error: "Submission not found." };
    }
    try {
      await studentSubmissionsApi.update(submission.id, { answerText });
      router.push("/student/submissions");
      return { success: true };
    } catch (err) {
      return {
        success: false,
        error: extractErrorMessage(err, "Failed to update. Please try again."),
      };
    }
  };

  let body: React.ReactNode;

  if (loading) {
    body = (
      <div className="flex items-center justify-center py-12">
        <Spinner />
      </div>
    );
  } else if (assignmentError) {
    body = <ErrorState message={assignmentError} onRetry={refetchAssignment} />;
  } else if (submissionsError) {
    body = <ErrorState message={submissionsError} onRetry={refetchSubmissions} />;
  } else if (!assignment) {
    body = <ErrorState message="Assignment not found." />;
  } else {
    const deadlinePassed = isDeadlinePassed(assignment.deadlineUtc);
    const submission = existingSubmission;
    const isReviewed = submission?.status === "Reviewed";
    const canResubmit =
      !!submission && assignment.allowResubmission && !deadlinePassed;

    let actionArea: React.ReactNode = null;

    if (!submission) {
      if (deadlinePassed) {
        actionArea = (
          <ErrorState message="The deadline for this assignment has passed." />
        );
      } else {
        actionArea = (
          <Card title="Submit Your Answer">
            <SubmissionForm onSubmit={handleSubmit} />
          </Card>
        );
      }
    } else if (isReviewed) {
      actionArea = (
        <Card title="Your Results">
          <dl className="space-y-3 text-sm">
            <div className="flex items-center justify-between">
              <dt className="font-medium text-gray-500">Marks</dt>
              <dd className="text-lg font-semibold text-gray-900">
                {submission.marks} / {assignment.maxMarks}
              </dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">Feedback</dt>
              <dd className="mt-1 whitespace-pre-wrap text-gray-900">
                {submission.feedback ?? "No feedback provided."}
              </dd>
            </div>
            {submission.reviewedAtUtc && (
              <div className="flex items-center justify-between">
                <dt className="font-medium text-gray-500">Reviewed</dt>
                <dd className="text-gray-900">
                  {formatUtcDate(submission.reviewedAtUtc)}
                </dd>
              </div>
            )}
          </dl>
        </Card>
      );
    } else if (canResubmit) {
      actionArea = (
        <Card title="Update Your Answer">
          <SubmissionForm
            initialValue={submission.answerText}
            submitLabel="Update Answer"
            onSubmit={handleUpdate}
          />
        </Card>
      );
    } else {
      const note = !assignment.allowResubmission
        ? "Resubmission not allowed"
        : "The deadline has passed. Your answer can no longer be updated.";
      actionArea = (
        <Card title="Your Submitted Answer">
          <p className="mb-3 text-sm font-medium text-gray-500">{note}</p>
          <p className="whitespace-pre-wrap text-sm text-gray-900">
            {submission.answerText}
          </p>
        </Card>
      );
    }

    body = (
      <div className="space-y-6">
        <div>
          <Link
            href="/student/assignments"
            className="inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800"
          >
            ← Back to assignments
          </Link>
        </div>

        <Card>
          <div className="flex items-start justify-between gap-3">
            <h1 className="text-2xl font-bold text-gray-900">
              {assignment.title}
            </h1>
            <Badge variant="blue">{assignment.maxMarks} marks</Badge>
          </div>
          <div className="mt-3 flex flex-wrap gap-2">
            {deadlinePassed ? (
              <Badge variant="red">Deadline passed</Badge>
            ) : (
              <Badge variant="yellow">Open</Badge>
            )}
            {assignment.allowResubmission ? (
              <Badge variant="green">Resubmission allowed</Badge>
            ) : (
              <Badge variant="gray">Resubmission not allowed</Badge>
            )}
          </div>
          <div className="mt-4">
            <h2 className="text-sm font-medium text-gray-500">Description</h2>
            <p className="mt-1 whitespace-pre-wrap text-sm text-gray-900">
              {assignment.description}
            </p>
          </div>
          <div className="mt-4 text-sm text-gray-500">
            Deadline:{" "}
            <span className="font-medium text-gray-900">
              {formatUtcDate(assignment.deadlineUtc)}
            </span>
          </div>
        </Card>

        {actionArea}
      </div>
    );
  }

  return (
    <RoleGuard allowedRoles={["Student"]}>
      <RoleShell role="Student">{body}</RoleShell>
    </RoleGuard>
  );
}
