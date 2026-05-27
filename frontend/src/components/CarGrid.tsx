import { CarResponse } from "@/types/car";
import CarCard from "@/components/CarCard";

interface CarGridProps {
    cars: CarResponse[];
}

export default function CarGrid({ cars }: CarGridProps) {
    return (
        <div className="container mx-auto px-6 py-8">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {cars.map((car, index) => (
                    <CarCard key={car.id} car={car} priority={index === 0} />
                ))}
            </div>
        </div>
    );
}
