'use client';

import { useRouter } from "next/navigation";
import Link from "next/link";
import CarForm from "@/components/CarForm";
import { addCar } from "@/lib/api";
import { CarRequest, CarUpdate } from "@/types/car";

export default function Page() {
    const router = useRouter();

    const handleAddCar = async (data: CarRequest | CarUpdate) => {
        await addCar(data as CarRequest);
        router.push("/");
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <header className="bg-white shadow-sm">
                <div className="container mx-auto px-6 py-4">
                    <Link href="/" className="text-blue-600 hover:text-blue-800">
                        Back
                    </Link>
                </div>
            </header>

            <main className="container mx-auto px-6 py-8 max-w-lg">
                <h1 className="text-2xl font-bold text-gray-900 mb-6">Add Car</h1>
                <CarForm
                    onSubmit={handleAddCar}
                    onCancel={() => router.push("/")}
                />
            </main>
        </div>
    );
}
