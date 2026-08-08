import { apiClient } from './client';
import {
  AuthResponse,
  LoginRequest,
  UserDto,
  CreateUserRequest,
  UpdateUserRequest,
  ClassDto,
  CreateClassRequest,
  UpdateClassRequest,
  SubjectDto,
  CreateSubjectRequest,
  UpdateSubjectRequest,
  TeacherAssignmentDto,
  CreateTeacherAssignmentRequest,
  EnrollmentDto,
  CreateEnrollmentRequest,
  AssignmentDto,
  AssignmentSummaryDto,
  CreateAssignmentRequest,
  UpdateAssignmentRequest,
  SubmissionDto,
  SubmissionSummaryDto,
  SubmitRequest,
  UpdateSubmissionRequest,
  ReviewSubmissionRequest,
} from '../types';

// ===== Auth =====
export const authApi = {
  login: (data: LoginRequest) => apiClient.post<AuthResponse>('/api/auth/login', data),
  me: () => apiClient.get<UserDto>('/api/auth/me'),
};

// ===== Admin: Users =====
export const adminUsersApi = {
  list: () => apiClient.get<UserDto[]>('/api/admin/users'),
  get: (id: string) => apiClient.get<UserDto>(`/api/admin/users/${id}`),
  create: (data: CreateUserRequest) => apiClient.post<UserDto>('/api/admin/users', data),
  update: (id: string, data: UpdateUserRequest) =>
    apiClient.put<UserDto>(`/api/admin/users/${id}`, data),
  delete: (id: string) => apiClient.del(`/api/admin/users/${id}`),
};

// ===== Admin: Classes =====
export const adminClassesApi = {
  list: () => apiClient.get<ClassDto[]>('/api/admin/classes'),
  get: (id: string) => apiClient.get<ClassDto>(`/api/admin/classes/${id}`),
  create: (data: CreateClassRequest) => apiClient.post<ClassDto>('/api/admin/classes', data),
  update: (id: string, data: UpdateClassRequest) =>
    apiClient.put<ClassDto>(`/api/admin/classes/${id}`, data),
  delete: (id: string) => apiClient.del(`/api/admin/classes/${id}`),
};

// ===== Admin: Subjects =====
export const adminSubjectsApi = {
  list: () => apiClient.get<SubjectDto[]>('/api/admin/subjects'),
  get: (id: string) => apiClient.get<SubjectDto>(`/api/admin/subjects/${id}`),
  create: (data: CreateSubjectRequest) => apiClient.post<SubjectDto>('/api/admin/subjects', data),
  update: (id: string, data: UpdateSubjectRequest) =>
    apiClient.put<SubjectDto>(`/api/admin/subjects/${id}`, data),
  delete: (id: string) => apiClient.del(`/api/admin/subjects/${id}`),
};

// ===== Admin: Teacher Assignments =====
export const adminTeacherAssignmentsApi = {
  list: () => apiClient.get<TeacherAssignmentDto[]>('/api/admin/teacher-assignments'),
  get: (id: string) =>
    apiClient.get<TeacherAssignmentDto>(`/api/admin/teacher-assignments/${id}`),
  create: (data: CreateTeacherAssignmentRequest) =>
    apiClient.post<TeacherAssignmentDto>('/api/admin/teacher-assignments', data),
  delete: (id: string) => apiClient.del(`/api/admin/teacher-assignments/${id}`),
};

// ===== Admin: Enrollments =====
export const adminEnrollmentsApi = {
  list: () => apiClient.get<EnrollmentDto[]>('/api/admin/enrollments'),
  get: (id: string) => apiClient.get<EnrollmentDto>(`/api/admin/enrollments/${id}`),
  create: (data: CreateEnrollmentRequest) =>
    apiClient.post<EnrollmentDto>('/api/admin/enrollments', data),
  delete: (id: string) => apiClient.del(`/api/admin/enrollments/${id}`),
};

// ===== Admin: Assignments =====
export const adminAssignmentsApi = {
  list: () => apiClient.get<AssignmentSummaryDto[]>('/api/admin/assignments'),
};

// ===== Admin: Submissions =====
export const adminSubmissionsApi = {
  list: () => apiClient.get<SubmissionSummaryDto[]>('/api/admin/submissions'),
};

// ===== Teacher: Assignments =====
export const teacherAssignmentsApi = {
  list: () => apiClient.get<AssignmentDto[]>('/api/teacher/assignments'),
  get: (id: string) => apiClient.get<AssignmentDto>(`/api/teacher/assignments/${id}`),
  create: (data: CreateAssignmentRequest) =>
    apiClient.post<AssignmentDto>('/api/teacher/assignments', data),
  update: (id: string, data: UpdateAssignmentRequest) =>
    apiClient.put<AssignmentDto>(`/api/teacher/assignments/${id}`, data),
  delete: (id: string) => apiClient.del(`/api/teacher/assignments/${id}`),
  publish: (id: string) =>
    apiClient.post<AssignmentDto>(`/api/teacher/assignments/${id}/publish`),
};

// ===== Teacher: Submissions =====
export const teacherSubmissionsApi = {
  listByAssignment: (assignmentId: string) =>
    apiClient.get<SubmissionDto[]>(`/api/teacher/assignments/${assignmentId}/submissions`),
  review: (submissionId: string, data: ReviewSubmissionRequest) =>
    apiClient.put<SubmissionDto>(`/api/teacher/submissions/${submissionId}/review`, data),
};

// ===== Student: Assignments =====
export const studentAssignmentsApi = {
  list: () => apiClient.get<AssignmentDto[]>('/api/student/assignments'),
  get: (id: string) => apiClient.get<AssignmentDto>(`/api/student/assignments/${id}`),
};

// ===== Student: Submissions =====
export const studentSubmissionsApi = {
  submit: (assignmentId: string, data: SubmitRequest) =>
    apiClient.post<SubmissionDto>(`/api/student/assignments/${assignmentId}/submit`, data),
  list: () => apiClient.get<SubmissionDto[]>('/api/student/submissions'),
  get: (id: string) => apiClient.get<SubmissionDto>(`/api/student/submissions/${id}`),
  update: (id: string, data: UpdateSubmissionRequest) =>
    apiClient.put<SubmissionDto>(`/api/student/submissions/${id}`, data),
};
