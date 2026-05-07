'use client';

import { CarResponse, CarRequest, CarUpdate } from "@/types/car";
import { useState } from "react";

interface CarFormProps {
    initialCar?: CarResponse; // undefined = adding, provided = editing
    onSubmit: (data: CarRequest | CarUpdate) => Promise<void>;
    onCancel: () => void;
}

export default function CarForm({ initialCar, onSubmit, onCancel }: CarFormProps) {
    const [make, setMake] = useState(initialCar?.make || "");
    const [model, setModel] = useState(initialCar?.model || "");
    const [year, setYear] = useState<number | undefined>(initialCar?.year || undefined);
    const [imageUrl, setImageUrl] = useState(initialCar?.imageUrl || "");
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    const handleSubmit: React.SubmitEventHandler<HTMLFormElement> = async (e) => {
        e.preventDefault();
        if (!make || !model || !year) {
            setError("Make, Model and Year are required.");
            return;
        }
        try {
            setError(null);
            setSubmitting(true);
            await onSubmit({ make, model, year, imageUrl: imageUrl || undefined });
        } catch {
            setError("Something went wrong. Please try again.");
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <form className="bg-white rounded-lg shadow-md p-6 flex flex-col gap-4"
            onSubmit={handleSubmit}>
            {error && (
                <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-md px-3 py-2">
                    {error}
                </p>
            )}
            <div>
                <label className="block text-gray-700">Make</label>
                <input
                    type="text"
                    value={make}
                    className="mt-1 block w-full border-gray-300 rounded-md shadow-sm text-gray-900 placeholder:text-gray-400"
                    onChange={(e) => setMake(e.target.value)}
                />
            </div>
            <div>
                <label className="block text-gray-700">Model</label>
                <input
                    type="text"
                    value={model}
                    className="mt-1 block w-full border-gray-300 rounded-md shadow-sm text-gray-900 placeholder:text-gray-400"
                    onChange={(e) => setModel(e.target.value)}
                />
            </div>
            <div>
                <label className="block text-gray-700">Year</label>
                <input
                    type="number"
                    value={year === undefined ? "" : year}
                    className="mt-1 block w-full border-gray-300 rounded-md shadow-sm text-gray-900 placeholder:text-gray-400"
                    onChange={(e) => setYear(e.target.value === "" ? undefined : Number(e.target.value))}
                />
            </div>
            <div>
                <label className="block text-gray-700">Image URL</label>
                <input
                    type="text"
                    value={imageUrl}
                    className="mt-1 block w-full border-gray-300 rounded-md shadow-sm text-gray-900 placeholder:text-gray-400"
                    onChange={(e) => setImageUrl(e.target.value)}
                />
            </div>
            <button type="button" className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700"
                onClick={onCancel}>
                Cancel
            </button>
            <button
                type="submit"
                disabled={submitting}
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed">
                {submitting ? "Saving..." : "Save"}
            </button>
        </form>
    );
}
