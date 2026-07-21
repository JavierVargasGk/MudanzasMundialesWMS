
import { AuthErrorResponse } from '@/types/auth';

export class ApiError extends Error {
  public status: number;
  public data: AuthErrorResponse;

  constructor(status: number, data: AuthErrorResponse) {
    super(data.message || 'Ha ocurrido un error en la solicitud');
    this.name = 'ApiError';
    this.status = status;
    this.data = data;
  }
}

interface RequestOptions extends RequestInit {
  params?: Record<string, string | number | boolean | undefined>;
}

export async function httpClient<T>(
  endpoint: string,
  options: RequestOptions = {}
): Promise<T> {
  const { params, headers, ...customConfig } = options;
  const baseUrl = process.env.NEXT_PUBLIC_API_URL || '';

  let url = `${baseUrl}${endpoint}`;
  if (params) {
    const searchParams = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined) {
        searchParams.append(key, String(value));
      }
    });
    const queryString = searchParams.toString();
    if (queryString) {
      url += `?${queryString}`;
    }
  }

  const defaultHeaders: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  const config: RequestInit = {
    method: customConfig.body ? 'POST' : 'GET',
    credentials: 'include', // Ensures HttpOnly cookies are attached and accepted
    ...customConfig,
    headers: {
      ...defaultHeaders,
      ...headers,
    },
  };

  const response = await fetch(url, config);

  if (!response.ok) {
    let errorData: AuthErrorResponse;
    try {
      errorData = await response.json();
    } catch {
      errorData = {
        message: `Error HTTP ${response.status}: ${response.statusText}`,
      };
    }
    throw new ApiError(response.status, errorData);
  }

  if (response.status === 204) {
    return {} as T;
  }

  return response.json();
}