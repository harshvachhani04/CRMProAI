export interface ApiResponse<T> {
  data: T;
  message: string;
  timestamp: string;
}

export interface ApiProblemDetails {
  type?: string;
  title?: string;
  status: number;
  detail?: string;
  instance?: string;
  code?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}
