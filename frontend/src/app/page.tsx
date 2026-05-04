import { CarResponse } from "./types/car";
import { getCars } from "./lib/api"
import CarGrid from "./components/CarGrid";


export default async function Page() {
    const cars: CarResponse[] = await getCars();
    return (
        <div className="min-h-screen bg-gray-50">
            <header className="bg-white shadow-sm">
                <div className="container mx-auto px-6 py-4">
                    <h1 className="text-2xl font-bold text-gray-900">Cars</h1>
                </div>
            </header>
            <CarGrid cars={cars} />
        </div>
    );
}
