import { NextRequest, NextResponse } from "next/server";

async function handleRequest(
    request: NextRequest,
    params: Promise<{ path: string[] }>,
    method: string
) {
    const backendUrl = process.env.BACKEND_URL;

    if (!backendUrl) {
        throw new Error('BACKEND_URL is not defined');
    }

    const { path } = await params;
    const fullUrl = `${backendUrl}/${path.join('/')}`;
    const token = request.cookies.get('token')?.value;

    const headers: HeadersInit = {
        'Content-Type': 'application/json',
    };

    if (token) {
        headers.Authorization = `Bearer ${token}`;
    }

    const options: RequestInit = {
        method,
        headers,
    };

    if (method !== 'GET' && method !== 'DELETE') {
        options.body = await request.text();
    }

    const response = await fetch(fullUrl, options);

    return NextResponse.json(await response.json(), {
        status: response.status,
    });
}

export async function GET(req: NextRequest, { params }: { params: Promise<{ path: string[] }> }) {
    return handleRequest(req, params, 'GET');
}

export async function POST(req: NextRequest, { params }: { params: Promise<{ path: string[] }> }) {
    return handleRequest(req, params, 'POST');
}

export async function PATCH(req: NextRequest, { params }: { params: Promise<{ path: string[] }> }) {
    return handleRequest(req, params, 'PATCH');
}

export async function DELETE(req: NextRequest, { params }: { params: Promise<{ path: string[] }> }) {
    return handleRequest(req, params, 'DELETE');
}
