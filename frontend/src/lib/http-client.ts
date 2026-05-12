import { getToken } from "@/lib/auth";

class ApiError extends Error {
	constructor(public readonly status: number, message: string) {
		super(message);
		this.name = "ApiError";
	}
}

class HttpClient {
	private baseURL: string;
	private headers: Record<string, string>;

	constructor(
		options: { baseURL?: string; headers?: Record<string, string> } = {}
	) {
		this.baseURL = options.baseURL || "";
		this.headers = options.headers || {};
	}

	setHeader(key: string, value: string) {
		this.headers[key] = value;
		return this;
	}

	private async fetchJSON<T>(
		endpoint: string,
		options: RequestInit = {}
	): Promise<T> {
		const url = this.baseURL + endpoint;
		const response = await fetch(url, {
			...options,
			headers: {
				"Content-Type": "application/json",
				...this.getHeaders(),
				...options.headers,
			},
		});

		if (!response.ok) {
			throw new ApiError(response.status, response.statusText);
		}

		if (response.status === 204 || options.method === "DELETE") {
			return undefined as unknown as T;
		}

		const text = await response.text();
		return (text ? JSON.parse(text) : undefined) as T;
	}

	// document.cookie is not available server-side, so skip
	// token addition outside the browser context.
	private getHeaders() {
		const token = typeof window !== "undefined" ? getToken() : undefined;
		return token
			? { ...this.headers, Authorization: `Bearer ${token}` }
			: this.headers;
	}

	get<T>(endpoint: string, options?: RequestInit): Promise<T> {
		return this.fetchJSON<T>(endpoint, { ...options, method: "GET" });
	}

	post<T, B = unknown>(
		endpoint: string,
		body?: B,
		options?: RequestInit
	): Promise<T> {
		return this.fetchJSON<T>(endpoint, {
			...options,
			method: "POST",
			body: body ? JSON.stringify(body) : undefined,
		});
	}

	put<T, B = unknown>(
		endpoint: string,
		body?: B,
		options?: RequestInit
	): Promise<T> {
		return this.fetchJSON<T>(endpoint, {
			...options,
			method: "PUT",
			body: body ? JSON.stringify(body) : undefined,
		});
	}

	patch<T, B = unknown>(
		endpoint: string,
		body?: B,
		options?: RequestInit
	): Promise<T> {
		return this.fetchJSON<T>(endpoint, {
			...options,
			method: "PATCH",
			body: body ? JSON.stringify(body) : undefined,
		});
	}

	delete(endpoint: string, options?: RequestInit): Promise<void> {
		return this.fetchJSON<void>(endpoint, {
			...options,
			method: "DELETE",
		});
	}
}

export { HttpClient, ApiError };
