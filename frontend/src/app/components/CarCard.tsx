'use client';

import { CarResponse } from "../types/car";

interface CarCardProps {
    car: CarResponse;
}

export default function CarCard({ car }: CarCardProps) {
    return (
        <div className="bg-white rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow">
            <img 
                src={car.imageUrl || "/car_placeholder.jpg"}
                alt={`${car.make} ${car.model}`}
                className="w-full h-48 object-cover rounded-md mb-4"
                onError={(e) => {
                    e.currentTarget.src = "/car_placeholder.jpg";
                }}
            />
            <h2 className="text-lg font-semibold text-gray-900">{car.make} {car.model}</h2>
            <p className="text-gray-600">{car.year}</p>
        </div>
    );
}
