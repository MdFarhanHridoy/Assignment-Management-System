import React from "react";

export function Table({ children }: { children: React.ReactNode }) {
  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-gray-200">{children}</table>
    </div>
  );
}

export function Th({
  children,
  className,
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <th
      className={[
        "px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500",
        className ?? "",
      ]
        .filter(Boolean)
        .join(" ")}
    >
      {children}
    </th>
  );
}

export function Td({
  children,
  className,
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <td
      className={[
        "px-6 py-4 text-sm text-gray-700",
        className ?? "",
      ]
        .filter(Boolean)
        .join(" ")}
    >
      {children}
    </td>
  );
}

export function Tr({
  children,
  className,
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <tr
      className={[
        "border-t border-gray-100",
        className ?? "",
      ]
        .filter(Boolean)
        .join(" ")}
    >
      {children}
    </tr>
  );
}
