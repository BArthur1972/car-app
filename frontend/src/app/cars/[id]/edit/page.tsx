import Link from "next/link";
import { getCar } from "@/lib/server-api";
import EditCarForm from "./EditCarForm";

export const dynamic = 'force-dynamic';

export default async function Page({ params }: { params: Promise<{ id: string }> }) {
    const { id } = await params;
    const car = await getCar(id);

    return (
        <div className="min-h-screen bg-gray-50">
            <header className="bg-white shadow-sm">
                <div className="container mx-auto px-6 py-4">
                    <Link href={`/cars/${id}`} className="text-blue-600 hover:text-blue-800">
                        Back
                    </Link>
                </div>
            </header>

            <main className="container mx-auto px-6 py-8 max-w-lg">
                <h1 className="text-2xl font-bold text-gray-900 mb-6">Edit Car</h1>
                <EditCarForm id={id} car={car} />
            </main>
        </div>
    );
}
