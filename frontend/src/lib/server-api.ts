// Server-only API module for use in Server Components.
// Creates a fresh HttpClient per request with the JWT from the incoming cookie,
// forwarded to the backend via Authorization header.
// Do not import in Client Components — use lib/api.ts instead.
import { HttpClient } from "@/lib/http-client";
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

export const getCars = async (): Promise<CarResponse[]> => {
	const client = await createServerClient();
	return client.get<CarResponse[]>(API_CONFIG.endpoints.cars);
};

export const getCar = async (id: string): Promise<CarResponse> => {
	const client = await createServerClient();
	return client.get<CarResponse>(API_CONFIG.endpoints.car(id));
};
