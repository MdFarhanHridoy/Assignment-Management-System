import { UserRole, AssignmentStatus, SubmissionStatus } from './types';

export const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';
export const TOKEN_KEY = 'am_token';

export const ROUTES = {
  LOGIN: '/login',
  ADMIN_DASHBOARD: '/admin/dashboard',
  ADMIN_USERS: '/admin/users',
  ADMIN_CLASSES: '/admin/classes',
  ADMIN_SUBJECTS: '/admin/subjects',
  ADMIN_TEACHER_ASSIGNMENTS: '/admin/teacher-assignments',
  ADMIN_ENROLLMENTS: '/admin/enrollments',
  ADMIN_ASSIGNMENTS: '/admin/assignments',
  ADMIN_SUBMISSIONS: '/admin/submissions',
  TEACHER_DASHBOARD: '/teacher/dashboard',
  TEACHER_ASSIGNMENTS: '/teacher/assignments',
  TEACHER_ASSIGNMENT_NEW: '/teacher/assignments/new',
  TEACHER_SUBMISSIONS: '/teacher/submissions',
  STUDENT_DASHBOARD: '/student/dashboard',
  STUDENT_ASSIGNMENTS: '/student/assignments',
  STUDENT_SUBMISSIONS: '/student/submissions',
} as const;

export const ROLE_DASHBOARD: Record<UserRole, string> = {
  Admin: '/admin/dashboard',
  Teacher: '/teacher/dashboard',
  Student: '/student/dashboard',
};

export const ROLE_LABELS: Record<UserRole, string> = {
  Admin: 'Administrator',
  Teacher: 'Teacher',
  Student: 'Student',
};

export const ASSIGNMENT_STATUS_LABELS: Record<AssignmentStatus, string> = {
  Draft: 'Draft',
  Published: 'Published',
  Archived: 'Archived',
};

export const SUBMISSION_STATUS_LABELS: Record<SubmissionStatus, string> = {
  Submitted: 'Submitted',
  UnderReview: 'Under Review',
  Reviewed: 'Reviewed',
  LateSubmitted: 'Late Submitted',
};
