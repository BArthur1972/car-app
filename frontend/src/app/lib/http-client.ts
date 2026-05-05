class HttpClient {
  private _baseURL: string;
  private _headers: Record<string, string>;

  constructor(options: { baseURL?: string; headers?: Record<string, string> } = {}) {
    this._baseURL = options.baseURL || '';
    this._headers = options.headers || {};
  }

  setHeader(key: string, value: string) {
    this._headers[key] = value;
    return this; // Enable method chaining
  }

  private async _fetchJSON<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const url = this._baseURL + endpoint;
    const response = await fetch(url, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...this._headers,
        ...options.headers,
      },
    });

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    // Handle no-content responses
    if (response.status === 204 || options.method === 'DELETE') {
      return undefined as any;
    }

    return response.json();
  }

  get<T>(endpoint: string, options?: RequestInit): Promise<T> {
    return this._fetchJSON<T>(endpoint, { ...options, method: 'GET' });
  }

  post<T>(endpoint: string, body?: any, options?: RequestInit): Promise<T> {
    return this._fetchJSON<T>(endpoint, {
      ...options,
      method: 'POST',
      body: body ? JSON.stringify(body) : undefined,
    });
  }

  put<T>(endpoint: string, body?: any, options?: RequestInit): Promise<T> {
    return this._fetchJSON<T>(endpoint, {
      ...options,
      method: 'PUT',
      body: body ? JSON.stringify(body) : undefined,
    });
  }

  patch<T>(endpoint: string, body?: any, options?: RequestInit): Promise<T> {
    return this._fetchJSON<T>(endpoint, {
      ...options,
      method: 'PATCH',
      body: body ? JSON.stringify(body) : undefined,
    });
  }

  delete<T>(endpoint: string, options?: RequestInit): Promise<T> {
    return this._fetchJSON<T>(endpoint, {
      ...options,
      method: 'DELETE',
    });
  }
}

export { HttpClient };