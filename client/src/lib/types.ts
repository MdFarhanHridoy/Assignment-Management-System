// Enums as string unions
export type UserRole = 'Admin' | 'Teacher' | 'Student';
export type AssignmentStatus = 'Draft' | 'Published' | 'Archived';
export type SubmissionStatus = 'Submitted' | 'UnderReview' | 'Reviewed' | 'LateSubmitted';

// Auth
export interface LoginRequest {
  email: string;
  password: string;
}
export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: UserDto;
}

// User
export interface UserDto {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}
export interface CreateUserRequest {
  name: string;
  email: string;
  password: string;
  role: UserRole;
}
export interface UpdateUserRequest {
  name?: string;
  email?: string;
  role?: UserRole;
  isActive?: boolean;
}

// Class
export interface ClassDto {
  id: string;
  name: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
}
export interface CreateClassRequest {
  name: string;
  description?: string;
}
export interface UpdateClassRequest {
  name?: string;
  description?: string;
}

// Subject
export interface SubjectDto {
  id: string;
  name: string;
  classId: string;
  createdAt: string;
  updatedAt: string;
}
export interface CreateSubjectRequest {
  name: string;
  classId: string;
}
export interface UpdateSubjectRequest {
  name?: string;
  classId?: string;
}

// TeacherAssignment
export interface TeacherAssignmentDto {
  id: string;
  teacherId: string;
  classId: string;
  subjectId: string;
  createdAt: string;
}
export interface CreateTeacherAssignmentRequest {
  teacherId: string;
  classId: string;
  subjectId: string;
}
export interface TeacherAssignmentViewDto {
  id: string;
  classId: string;
  className: string;
  subjectId: string;
  subjectName: string;
}

// Enrollment
export interface EnrollmentDto {
  id: string;
  classId: string;
  studentId: string;
  enrolledAt: string;
}
export interface CreateEnrollmentRequest {
  classId: string;
  studentId: string;
}

// Assignment
export interface AssignmentDto {
  id: string;
  title: string;
  description: string;
  deadlineUtc: string;
  maxMarks: number;
  status: AssignmentStatus;
  teacherId: string;
  classId: string;
  subjectId: string;
  allowResubmission: boolean;
  createdAt: string;
  updatedAt: string;
  subjectName?: string;
}
export interface CreateAssignmentRequest {
  title: string;
  description: string;
  deadlineUtc: string;
  maxMarks: number;
  classId: string;
  subjectId: string;
  allowResubmission?: boolean;
}
export interface UpdateAssignmentRequest {
  title?: string;
  description?: string;
  deadlineUtc?: string;
  maxMarks?: number;
  classId?: string;
  subjectId?: string;
  allowResubmission?: boolean;
}
export interface AssignmentSummaryDto {
  id: string;
  title: string;
  status: AssignmentStatus;
  teacherId: string;
  classId: string;
  subjectId: string;
  deadlineUtc: string;
  maxMarks: number;
  createdAt: string;
}

// Submission
export interface SubmissionDto {
  id: string;
  assignmentId: string;
  studentId: string;
  answerText: string;
  submittedAtUtc: string;
  updatedAtUtc: string;
  status: SubmissionStatus;
  marks: number | null;
  feedback: string | null;
  reviewedByTeacherId: string | null;
  reviewedAtUtc: string | null;
}
export interface SubmitRequest {
  answerText: string;
}
export interface UpdateSubmissionRequest {
  answerText: string;
}
export interface ReviewSubmissionRequest {
  marks: number;
  feedback?: string;
  status?: SubmissionStatus;
}
export interface SubmissionSummaryDto {
  id: string;
  assignmentId: string;
  studentId: string;
  status: SubmissionStatus;
  marks: number | null;
  submittedAtUtc: string;
  reviewedAtUtc: string | null;
}

// Error envelope
export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
}
