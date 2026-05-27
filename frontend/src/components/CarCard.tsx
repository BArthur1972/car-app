'use client';

import { useState } from "react";
import Link from "next/link";
import { CarResponse } from "@/types/car";
import Image, { StaticImageData } from "next/image";
import placeholder from "@/../public/car_placeholder.jpg";

interface CarCardProps {
    car: CarResponse;
    priority?: boolean;
}

export default function CarCard({ car, priority = false }: CarCardProps) {
    const [imgSrc, setImgSrc] = useState<string | StaticImageData>(car.imageUrl || placeholder);

    return (
        <Link href={`/cars/${car.id}`} className="block">
            <div className="bg-white rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow cursor-pointer">
                <div className="relative w-full h-48 mb-4 rounded-md overflow-hidden">
                    <Image
                        src={imgSrc}
                        alt={`${car.make} ${car.model}`}
                        fill
                        sizes="(max-width: 768px) 100vw, (max-width: 1024px) 50vw, 33vw"
                        priority={priority}
                        className="object-cover"
                        onError={() => setImgSrc(placeholder)}
                    />
                </div>
                <h2 className="text-lg font-semibold text-gray-900">{car.make} {car.model}</h2>
                <p className="text-gray-600">{car.year}</p>
            </div>
        </Link>
    );
}
