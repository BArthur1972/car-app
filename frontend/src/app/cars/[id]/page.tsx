import Link from "next/link";
import { getCar } from "@/lib/api";
import CarImage from "./CarImage";
import DeleteCarAction from "./DeleteCarAction";

export const dynamic = 'force-dynamic';

export default async function Page({ params }: { params: Promise<{ id: string }> }) {
    const { id } = await params;
    const car = await getCar(id);

    return (
        <div className="min-h-screen bg-gray-50">
            <header className="bg-white shadow-sm">
                <div className="container mx-auto px-6 py-4">
                    <Link href="/" className="text-blue-600 hover:text-blue-800">
                        Back
                    </Link>
                </div>
            </header>

            <main className="container mx-auto px-6 py-8">
                <div className="bg-white rounded-lg shadow-md p-6 flex gap-8">
                    <CarImage src={car.imageUrl} alt={`${car.make} ${car.model}`} />

                    <div className="flex flex-col justify-between flex-1">
                        <div>
                            <h1 className="text-2xl font-bold text-gray-900 mb-2">
                                {car.make} {car.model}
                            </h1>
                            <p className="text-gray-600">Year: {car.year}</p>
                        </div>

                        <div className="flex gap-3">
                            <Link
                                href={`/cars/${id}/edit`}
                                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
                                Edit
                            </Link>
                            <DeleteCarAction id={id} />
                        </div>
                    </div>
                </div>
            </main>
        </div>
    );
}
