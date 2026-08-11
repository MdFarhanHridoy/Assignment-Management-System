"use client";

import React, { useState } from "react";
import { Button } from "@/components/ui/Button";

interface SubmissionFormProps {
  initialValue?: string;
  onSubmit: (answerText: string) => Promise<{ success: boolean; error?: string }>;
  submitLabel?: string;
  disabled?: boolean;
}

export function SubmissionForm({
  initialValue = "",
  onSubmit,
  submitLabel = "Submit Answer",
  disabled = false,
}: SubmissionFormProps) {
  const [answerText, setAnswerText] = useState<string>(initialValue);
  const [fieldError, setFieldError] = useState<string | null>(null);
  const [apiError, setApiError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setAnswerText(e.target.value);
    if (fieldError) setFieldError(null);
    if (apiError) setApiError(null);
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setApiError(null);

    const trimmed = answerText.trim();
    if (trimmed.length < 1) {
      setFieldError("Answer is required.");
      return;
    }

    setFieldError(null);
    setIsSubmitting(true);

    const result = await onSubmit(trimmed);

    setIsSubmitting(false);

    if (!result.success) {
      setApiError(result.error ?? "Something went wrong. Please try again.");
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4" noValidate>
      {apiError && (
        <div
          role="alert"
          className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
        >
          {apiError}
        </div>
      )}

      <div className="w-full">
        <label
          htmlFor="answerText"
          className="mb-1 block text-sm font-medium text-gray-700"
        >
          Your Answer
          <span className="ml-0.5 text-red-500">*</span>
        </label>
        <textarea
          id="answerText"
          name="answerText"
          value={answerText}
          onChange={handleChange}
          aria-invalid={fieldError ? true : undefined}
          disabled={disabled || isSubmitting}
          required
          rows={8}
          placeholder="Type your answer here..."
          className="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:ring-indigo-500 disabled:cursor-not-allowed disabled:bg-gray-100"
        />
        {fieldError && <p className="mt-1 text-sm text-red-600">{fieldError}</p>}
      </div>

      <Button type="submit" isLoading={isSubmitting} disabled={disabled}>
        {submitLabel}
      </Button>
    </form>
  );
}
