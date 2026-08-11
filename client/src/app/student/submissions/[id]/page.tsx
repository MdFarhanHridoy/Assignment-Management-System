"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { RoleGuard } from "@/components/guards/RoleGuard";
import { RoleShell } from "@/components/layout/RoleShell";
import { Badge } from "@/components/ui/Badge";
import { Card } from "@/components/ui/Card";
import { ErrorState } from "@/components/ui/ErrorState";
import { Spinner } from "@/components/ui/Spinner";
import { useApi } from "@/hooks/useApi";
import { studentSubmissionsApi } from "@/lib/api/endpoints";
import { SUBMISSION_STATUS_LABELS } from "@/lib/constants";
import { SubmissionDto, SubmissionStatus } from "@/lib/types";
import { formatUtcDate } from "@/lib/utils";

function statusVariant(
  status: SubmissionStatus
): "gray" | "yellow" | "green" | "red" {
  switch (status) {
    case "Reviewed":
      return "green";
    case "UnderReview":
      return "yellow";
    case "LateSubmitted":
      return "red";
    default:
      return "gray";
  }
}

export default function StudentSubmissionDetailPage() {
  const params = useParams();
  const idParam = params?.id;
  const id = Array.isArray(idParam) ? idParam[0] : idParam ?? "";

  const {
    data: submission,
    loading,
    error,
    refetch,
  } = useApi<SubmissionDto>(() => studentSubmissionsApi.get(id), [id]);

  return (
    <RoleGuard allowedRoles={["Student"]}>
      <RoleShell role="Student">
        <div className="space-y-6">
          <div>
            <Link
              href="/student/submissions"
              className="inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800"
            >
              ← Back to submissions
            </Link>
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Submission</h1>
          </div>

          {loading ? (
            <div className="flex items-center justify-center py-12">
              <Spinner />
            </div>
          ) : error ? (
            <ErrorState message={error} onRetry={refetch} />
          ) : !submission ? (
            <ErrorState message="Submission not found." />
          ) : (
            <div className="space-y-6">
              <Card>
                <div className="flex items-center justify-between gap-3">
                  <h2 className="text-lg font-semibold text-gray-900">
                    Submission Details
                  </h2>
                  <Badge variant={statusVariant(submission.status)}>
                    {SUBMISSION_STATUS_LABELS[submission.status]}
                  </Badge>
                </div>
                <dl className="mt-4 grid grid-cols-1 gap-3 text-sm sm:grid-cols-2">
                  <div>
                    <dt className="font-medium text-gray-500">Submitted</dt>
                    <dd className="mt-1 text-gray-900">
                      {formatUtcDate(submission.submittedAtUtc)}
                    </dd>
                  </div>
                  <div>
                    <dt className="font-medium text-gray-500">Last Updated</dt>
                    <dd className="mt-1 text-gray-900">
                      {formatUtcDate(submission.updatedAtUtc)}
                    </dd>
                  </div>
                </dl>
              </Card>

              <Card title="Your Answer">
                <p className="whitespace-pre-wrap text-sm text-gray-900">
                  {submission.answerText}
                </p>
              </Card>

              {submission.marks !== null && (
                <Card title="Review">
                  <dl className="space-y-3 text-sm">
                    <div className="flex items-center justify-between">
                      <dt className="font-medium text-gray-500">Marks</dt>
                      <dd className="text-lg font-semibold text-gray-900">
                        {submission.marks}
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
              )}
            </div>
          )}
        </div>
      </RoleShell>
    </RoleGuard>
  );
}
