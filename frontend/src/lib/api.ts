import { HttpClient } from "@/lib/http-client";
import { API_CONFIG } from "@/lib/config";
import { CarResponse, CarRequest, CarUpdate } from "@/types/car";
import { AuthResponse, LoginRequest, RegisterRequest } from "@/types/auth";

class ApiClient extends HttpClient {
	constructor() {
		super({
			baseURL: API_CONFIG.baseURL,
			headers: {
				Accept: "application/json",
			},
		});
	}

	readonly cars = {
		get: (): Promise<CarResponse[]> =>
			this.get<CarResponse[]>(API_CONFIG.endpoints.cars),

		getById: (id: string): Promise<CarResponse> =>
			this.get<CarResponse>(API_CONFIG.endpoints.car(id)),

		create: (car: CarRequest): Promise<CarResponse> =>
			this.post<CarResponse>(API_CONFIG.endpoints.addCar, car),

		update: (id: string, car: CarUpdate): Promise<CarResponse> =>
			this.patch<CarResponse>(API_CONFIG.endpoints.updateCar(id), car),

		delete: (id: string): Promise<void> =>
			this.delete(API_CONFIG.endpoints.deleteCar(id)),
	};

	readonly auth = {
		login: (req: LoginRequest): Promise<AuthResponse> =>
			this.post<AuthResponse>(API_CONFIG.endpoints.login, req),

		register: (req: RegisterRequest): Promise<void> =>
			this.post<void>(API_CONFIG.endpoints.register, req),
	};
}

export const api = new ApiClient();

export const getCars = () => api.cars.get();
export const getCar = (id: string) => api.cars.getById(id);
export const addCar = (car: CarRequest) => api.cars.create(car);
export const updateCar = (id: string, car: CarUpdate) =>
	api.cars.update(id, car);
export const deleteCar = (id: string) => api.cars.delete(id);
export const login = (req: LoginRequest) => api.auth.login(req);
export const register = (req: RegisterRequest) => api.auth.register(req);
