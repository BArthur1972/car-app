import { CarResponse } from "../types/car";

export async function getCars(): Promise<CarResponse[]> {
    const url = "http://localhost:5292/cars/getCars";
    const response = await fetch(url);
    if (!response.ok) {
        console.log(`Failed to fetch ${url}: ${response.statusText}`);
        throw new Error(`Failed to fetch ${url}: ${response.statusText}`);
    }
    return response.json() as Promise<CarResponse[]>;
}
