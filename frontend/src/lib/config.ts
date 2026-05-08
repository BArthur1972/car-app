export const API_CONFIG = {
  // Server Components use API_URL (Docker internal network or localhost).
  // Client Components use NEXT_PUBLIC_API_URL (must be reachable from the browser).
  baseURL: typeof window === 'undefined'
    ? process.env.API_URL
    : process.env.NEXT_PUBLIC_API_URL,
  endpoints: {
    cars: '/cars/getCars',
    car: (id: string) => `/cars/getCar/${id}`,
    addCar: '/cars/addCar',
    updateCar: (id: string) => `/cars/updateCar/${id}`,
    deleteCar: (id: string) => `/cars/removeCar/${id}`,
  }
} as const;