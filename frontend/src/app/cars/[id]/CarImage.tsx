'use client';

import Image, { StaticImageData } from "next/image";
import { useState } from "react";
import placeholder from "@/../public/car_placeholder.jpg";

interface CarImageProps {
    src: string | null;
    alt: string;
}

export default function CarImage({ src, alt }: CarImageProps) {
    const [imgSrc, setImgSrc] = useState<string | StaticImageData>(src || placeholder);

    return (
        <Image
            src={imgSrc}
            alt={alt}
            width={256}
            height={192}
            priority
            className="w-64 h-48 object-cover rounded-lg flex-shrink-0"
            onError={() => setImgSrc(placeholder)}
        />
    );
}
