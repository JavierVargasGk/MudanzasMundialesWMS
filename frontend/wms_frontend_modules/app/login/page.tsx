'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { AlertOctagon, Lock, Mail, ShieldAlert, UserX, Loader2 } from 'lucide-react';
import { loginSchema, LoginFormData } from '@/lib/zod/schema';
import { httpClient, ApiError } from '@/lib/http-client';
import { LoginResponse, UserRole } from '@/types/auth';

const ROLE_REDIRECTS: Record<UserRole, string> = {
  Administrador: '/dashboard',
  Operario: '/inventory',
  Analista: '/dashboard',
};

export default function LoginPage() {
  const router = useRouter();
  const [serverError, setServerError] = useState<string | null>(null);
  const [lockoutDetails, setLockoutDetails] = useState<{
    isLocked: boolean;
    lockoutEndUtc?: string;
    remainingAttempts?: number;
    isInactive?: boolean;
  }>({ isLocked: false });

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      Correo: '',
      Contrasena: '',
    },
  });

const onSubmit = async (data: LoginFormData) => {
  setServerError(null);
  setLockoutDetails({ isLocked: false });

  try {
    const response = await httpClient<LoginResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
    });

    if (response.accessToken) {
      localStorage.setItem('accessToken', response.accessToken);
    }

    const redirectPath = ROLE_REDIRECTS[response.role as UserRole] || '/dashboard';
    router.push(redirectPath);
    router.refresh();
  } catch (err) {
    console.error('Login error details:', err); 

    if (err instanceof ApiError) {
      const errorData = err.data;

      if (errorData?.isLockedOut) {
        setLockoutDetails({
          isLocked: true,
          lockoutEndUtc: errorData.lockoutEndUtc,
        });
        setServerError('Cuenta bloqueada temporalmente por demasiados intentos fallidos.');
      } else if (errorData?.isInactive) {
        setLockoutDetails({ isLocked: false, isInactive: true });
        setServerError('La cuenta de cliente se encuentra inactiva. Contacte al administrador.');
      } else if (errorData?.remainingAttempts !== undefined) {
        setLockoutDetails({
          isLocked: false,
          remainingAttempts: errorData.remainingAttempts,
        });
        setServerError(
          `Credenciales inválidas. Intentos restantes antes del bloqueo: ${errorData.remainingAttempts}`
        );
      } else {
        setServerError(errorData?.message || 'Error de autenticación');
      }
    } else {
      setServerError('Ocurrió un error inesperado de conexión con el servidor.');
    }
  }
};

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-900 text-gray-100 px-4">
      <div className="max-w-md w-full space-y-8 bg-gray-800 p-8 rounded-xl shadow-2xl border border-gray-700">
        <div>
          <h2 className="mt-2 text-center text-3xl font-extrabold text-white">
            Mudanzas Mundiales
          </h2>
          <p className="mt-2 text-center text-sm text-gray-400">
            Sistema Control Logístico Multicliente WMS
          </p>
        </div>

        {serverError && (
            <div className="bg-red-900/50 border border-red-500 text-red-200 p-4 rounded-lg flex items-start space-x-3 text-sm">
                {lockoutDetails.isLocked ? (
                <Lock className="w-5 h-5 text-red-400 shrink-0 mt-0.5" />
                ) : lockoutDetails.isInactive ? (
                <UserX className="w-5 h-5 text-red-400 shrink-0 mt-0.5" />
                ) : (
                <AlertOctagon className="w-5 h-5 text-red-400 shrink-0 mt-0.5" />
                )}
                <div className="flex-1">
                <p className="font-semibold">{serverError}</p>
                {lockoutDetails.lockoutEndUtc && (
                    <p className="mt-1 text-xs text-red-300">
                    Desbloqueo programado (UTC): <strong>{lockoutDetails.lockoutEndUtc}</strong>
                    </p>
                )}
                </div>
            </div>
            )}

        <form className="mt-8 space-y-6" onSubmit={handleSubmit(onSubmit)}>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-300 mb-1">
                Correo Electrónico
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <Mail className="h-5 w-5 text-gray-400" />
                </div>
                <input
                  {...register('Correo')}
                  type="Correo"
                  autoComplete="Correo"
                  className="block w-full pl-10 pr-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
                  placeholder="usuario@empresa.com"
                />
              </div>
              {errors.Correo && (
                <p className="mt-1 text-xs text-red-400">{errors.Correo.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-300 mb-1">
                Contraseña
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <Lock className="h-5 w-5 text-gray-400" />
                </div>
                <input
                  {...register('Contrasena')}
                  type="Contrasena"
                  autoComplete="current-Contrasena"
                  className="block w-full pl-10 pr-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
                  placeholder="••••••••"
                />
              </div>
              {errors.Contrasena && (
                <p className="mt-1 text-xs text-red-400">{errors.Contrasena.message}</p>
              )}
            </div>
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full flex justify-center items-center py-2.5 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {isSubmitting ? (
              <>
                <Loader2 className="animate-spin -ml-1 mr-2 h-4 w-4" />
                Autenticando...
              </>
            ) : (
              'Iniciar Sesión'
            )}
          </button>
        </form>
      </div>
    </div>
  );
}