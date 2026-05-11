export const API_CONFIG = {
	// Server Components use BACKEND_URL (Docker internal network or localhost).
	// Client Components use NEXT_PUBLIC_BACKEND_URL (baked into bundle at build time).
	baseURL:
		typeof window === "undefined"
			? process.env.BACKEND_URL
			: process.env.NEXT_PUBLIC_BACKEND_URL,
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
