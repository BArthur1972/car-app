'use client';

import Link from "next/link";
import { CarResponse } from "../types/car";

interface CarCardProps {
    car: CarResponse;
}

const PLACEHOLDER = "/car_placeholder.jpg";

export default function CarCard({ car }: CarCardProps) {
    return (
        <Link href={`/cars/${car.id}`} className="block">
            <div className="bg-white rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow cursor-pointer">
                <img
                    src={car.imageUrl || PLACEHOLDER}
                    alt={`${car.make} ${car.model}`}
                    className="w-full h-48 object-cover rounded-md mb-4"
                    onError={(e) => {
                        if (e.currentTarget.src !== PLACEHOLDER) {
                            e.currentTarget.src = PLACEHOLDER;
                        }
                    }}
                />
                <h2 className="text-lg font-semibold text-gray-900">{car.make} {car.model}</h2>
                <p className="text-gray-600">{car.year}</p>
            </div>
        </Link>
    );
}
