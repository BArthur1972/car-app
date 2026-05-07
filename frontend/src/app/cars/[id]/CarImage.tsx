'use client';

const PLACEHOLDER = "/car_placeholder.jpg";

interface CarImageProps {
    src: string | null;
    alt: string;
}

export default function CarImage({ src, alt }: CarImageProps) {
    return (
        <img
            src={src || PLACEHOLDER}
            alt={alt}
            className="w-64 h-48 object-cover rounded-lg flex-shrink-0"
            onError={(e) => {
                if (e.currentTarget.src !== PLACEHOLDER) {
                    e.currentTarget.src = PLACEHOLDER;
                }
            }}
        />
    );
}
