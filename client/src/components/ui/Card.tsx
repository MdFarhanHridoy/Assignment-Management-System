import React from "react";

export interface CardProps {
  children: React.ReactNode;
  className?: string;
  title?: string;
  description?: string;
}

export function Card({ children, className, title, description }: CardProps) {
  const hasHeader = Boolean(title || description);

  return (
    <div
      className={[
        "rounded-lg border border-gray-200 bg-white p-6 shadow-sm",
        className ?? "",
      ]
        .filter(Boolean)
        .join(" ")}
    >
      {title && (
        <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
      )}
      {description && (
        <p className="mt-1 text-sm text-gray-500">{description}</p>
      )}
      {hasHeader ? (
        <div className="mt-4">{children}</div>
      ) : (
        children
      )}
    </div>
  );
}
