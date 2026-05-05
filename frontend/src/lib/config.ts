export const API_CONFIG = {
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5292',
  endpoints: {
    cars: '/cars/getCars',
    car: (id: string) => `/cars/getCar/${id}`,
    addCar: '/cars/addCar',
    updateCar: (id: string) => `/cars/updateCar/${id}`,
    deleteCar: (id: string) => `/cars/removeCar/${id}`,
  }
} as const;