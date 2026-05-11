export interface CarResponse {
	id: string;
	make: string;
	model: string;
	year: number;
	imageUrl: string | null;
}

export interface CarRequest {
	make: string;
	model: string;
	year: number;
	imageUrl?: string;
}

export interface CarUpdate {
	make?: string;
	model?: string;
	year?: number;
	imageUrl?: string;
}
