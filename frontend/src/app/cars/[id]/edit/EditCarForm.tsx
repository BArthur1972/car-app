'use client';

import { useRouter } from "next/navigation";
import CarForm from "@/components/CarForm";
import { updateCar } from "@/lib/api";
import { CarResponse, CarRequest, CarUpdate } from "@/types/car";

interface EditCarFormProps {
    id: string;
    car: CarResponse;
}

export default function EditCarForm({ id, car }: EditCarFormProps) {
    const router = useRouter();

    const handleSubmit = async (data: CarRequest | CarUpdate) => {
        await updateCar(id, data as CarUpdate);
        router.push(`/cars/${id}`);
    };

    return (
        <CarForm
            initialCar={car}
            onSubmit={handleSubmit}
            onCancel={() => router.push(`/cars/${id}`)} />
    );
}
