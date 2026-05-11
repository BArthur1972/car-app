import Link from "next/link";
import { getCars } from "@/lib/api";
import CarGrid from "@/components/CarGrid";
import LogoutButton from "@/components/LogoutButton";

export const dynamic = 'force-dynamic';

export default async function Page() {
    const cars = await getCars();
    return (
        <div className="min-h-screen bg-gray-50">
            <header className="bg-white shadow-sm">
                <div className="container mx-auto px-6 py-4">
                    <div className="flex items-center justify-between">
                        <h1 className="text-2xl font-bold text-gray-900">Cars</h1>
                        <div className="flex items-center gap-3">
                            <Link
                                href="/cars/new"
                                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
                                + Add Car
                            </Link>
                            <LogoutButton />
                        </div>
                    </div>
                </div>
            </header>
            <CarGrid cars={cars} />
        </div>
    );
}
