// Server-only API module for use in Server Components.
// Creates a fresh HttpClient per request with the JWT from the incoming cookie,
// forwarded to the backend via Authorization header.
// Do not import in Client Components — use lib/api.ts instead.
import { redirect } from "next/navigation";
import { HttpClient, ApiError } from "@/lib/http-client";
import { API_CONFIG } from "@/lib/config";
import { getServerSession } from "@/lib/session";
import type { CarResponse } from "@/types/car";

async function createServerClient() {
	const session = await getServerSession();
	return new HttpClient({
		baseURL: API_CONFIG.baseURL,
		headers: {
			Accept: "application/json",
			...(session ? { Authorization: `Bearer ${session.token}` } : {}),
		},
	});
}

// proxy.ts only checks that the cookie exists, not that the token is still valid.
// An expired token passes through and the backend returns 401 — catch it here
// and redirect to login rather than letting the Server Component crash with a 500.
function handleError(err: unknown): never {
	if (err instanceof ApiError && err.status === 401) {
		redirect("/login");
	}
	throw err;
}

export const getCars = async (): Promise<CarResponse[]> => {
	const client = await createServerClient();
	return client.get<CarResponse[]>(API_CONFIG.endpoints.cars).catch(handleError);
};

export const getCar = async (id: string): Promise<CarResponse> => {
	const client = await createServerClient();
	return client.get<CarResponse>(API_CONFIG.endpoints.car(id)).catch(handleError);
};
