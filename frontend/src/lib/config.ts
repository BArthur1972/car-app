export const API_CONFIG = {
	// Server Components use BACKEND_URL to directly communicate with the backend.
	// Client Components route requests to /api/* (proxied to the backend).
	baseURL:
		typeof window === "undefined" ? process.env.BACKEND_URL : "/api",
	endpoints: {
		cars: "/cars/getCars",
		car: (id: string) => `/cars/getCar/${id}`,
		addCar: "/cars/addCar",
		updateCar: (id: string) => `/cars/updateCar/${id}`,
		deleteCar: (id: string) => `/cars/removeCar/${id}`,
		login: "/auth/login",
		register: "/auth/register",
	},
} as const;
