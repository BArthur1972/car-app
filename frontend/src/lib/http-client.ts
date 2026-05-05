class HttpClient {
  private baseURL: string;
  private headers: Record<string, string>;

  constructor(options: { baseURL?: string; headers?: Record<string, string> } = {}) {
    this.baseURL = options.baseURL || '';
    this.headers = options.headers || {};
  }

  setHeader(key: string, value: string) {
    this.headers[key] = value;
    return this;
  }

  private async fetchJSON<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const url = this.baseURL + endpoint;
    const response = await fetch(url, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...this.headers,
        ...options.headers,
      },
    });

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    if (response.status === 204 || options.method === 'DELETE') {
      return undefined as unknown as T;
    }

    return response.json();
  }

  get<T>(endpoint: string, options?: RequestInit): Promise<T> {
    return this.fetchJSON<T>(endpoint, { ...options, method: 'GET' });
  }

  post<T, B = unknown>(endpoint: string, body?: B, options?: RequestInit): Promise<T> {
    return this.fetchJSON<T>(endpoint, {
      ...options,
      method: 'POST',
      body: body ? JSON.stringify(body) : undefined,
    });
  }

  put<T, B = unknown>(endpoint: string, body?: B, options?: RequestInit): Promise<T> {
    return this.fetchJSON<T>(endpoint, {
      ...options,
      method: 'PUT',
      body: body ? JSON.stringify(body) : undefined,
    });
  }

  patch<T, B = unknown>(endpoint: string, body?: B, options?: RequestInit): Promise<T> {
    return this.fetchJSON<T>(endpoint, {
      ...options,
      method: 'PATCH',
      body: body ? JSON.stringify(body) : undefined,
    });
  }

  delete<T>(endpoint: string, options?: RequestInit): Promise<void> {
    return this.fetchJSON<void>(endpoint, {
      ...options,
      method: 'DELETE',
    });
  }
}

export { HttpClient };