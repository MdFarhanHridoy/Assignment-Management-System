'use client';

import { LoginForm } from '@/components/forms/LoginForm';

export default function LoginPage() {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50 px-4">
      <h1 className="text-2xl font-bold text-gray-900 mb-2">
        Assignment Management System
      </h1>
      <p className="text-gray-500 mb-8">Sign in to your account</p>
      <LoginForm />
    </div>
  );
}
