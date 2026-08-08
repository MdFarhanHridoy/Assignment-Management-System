export function cn(...classes: (string | undefined | false | null)[]): string {
  return classes.filter(Boolean).join(' ');
}

export function formatUtcDate(iso: string): string {
  const date = new Date(iso);
  if (isNaN(date.getTime())) return iso;
  const formatter = new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
    timeZone: 'UTC',
  });
  return `${formatter.format(date)} UTC`;
}

export function formatDate(iso: string): string {
  const date = new Date(iso);
  if (isNaN(date.getTime())) return iso;
  return new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    timeZone: 'UTC',
  }).format(date);
}

export function isDeadlinePassed(deadlineUtc: string): boolean {
  const deadline = new Date(deadlineUtc);
  if (isNaN(deadline.getTime())) return false;
  return Date.now() > deadline.getTime();
}

export function getInitials(name: string): string {
  if (!name) return '';
  const parts = name
    .trim()
    .split(/\s+/)
    .filter((part) => part.length > 0);
  if (parts.length === 0) return '';
  return parts
    .slice(0, 2)
    .map((part) => part[0].toUpperCase())
    .join('');
}
