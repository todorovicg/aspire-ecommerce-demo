import { HttpErrorResponse } from '@angular/common/http';

export interface ApiError {
  code: string;
  message: string;
  details: unknown;
  traceId: string | null;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  error: ApiError | null;
}

export class ApiFailure extends Error {
  constructor(
    readonly code: string,
    message: string,
    readonly traceId: string | null = null,
    readonly details: unknown = null,
  ) {
    super(message);
    this.name = 'ApiFailure';
  }
}

export function unwrap<T>(response: ApiResponse<T>): T {
  if (!response.success) {
    throw fromApiError(response.error);
  }
  return response.data as T;
}

export function toApiFailure(error: unknown): ApiFailure {
  if (error instanceof ApiFailure) {
    return error;
  }
  if (error instanceof HttpErrorResponse) {
    const body = error.error as Partial<ApiResponse<unknown>> | null;
    if (body && typeof body === 'object' && body.error) {
      return fromApiError(body.error);
    }
    if (error.status === 0) {
      return new ApiFailure('client.network_error', 'The server could not be reached');
    }
    return new ApiFailure(`common.http_${error.status}`, error.statusText || 'Request failed');
  }
  return new ApiFailure('client.unknown_error', 'Something went wrong');
}

function fromApiError(error: ApiError | null): ApiFailure {
  return new ApiFailure(
    error?.code ?? 'client.unknown_error',
    error?.message ?? 'Request failed',
    error?.traceId ?? null,
    error?.details ?? null,
  );
}
