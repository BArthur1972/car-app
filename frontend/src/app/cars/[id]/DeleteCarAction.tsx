'use client';

import { useState } from "react";
import { useRouter } from "next/navigation";
import { deleteCar } from "@/lib/api";

interface DeleteCarActionProps {
    id: string;
}

export default function DeleteCarAction({ id }: DeleteCarActionProps) {
    const router = useRouter();
    const [error, setError] = useState<string | null>(null);
    const [deleting, setDeleting] = useState(false);

    const handleDelete = async () => {
        if (!confirm("Are you sure you want to delete this car?")) return;
        try {
            setDeleting(true);
            setError(null);
            await deleteCar(id);
            router.push("/");
        } catch {
            setError("Failed to delete. Please try again.");
        } finally {
            setDeleting(false);
        }
    };

    return (
        <div>
            {error && (
                <p className="text-sm text-red-600 mb-2">{error}</p>
            )}
            <button
                onClick={handleDelete}
                disabled={deleting}
                className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed">
                {deleting ? "Deleting..." : "Delete"}
            </button>
        </div>
    );
}
