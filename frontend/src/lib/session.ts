import { cookies } from "next/headers";

export interface ServerSession {
	token: string;
}

export async function getServerSession(): Promise<ServerSession | null> {
	const token = (await cookies()).get("token")?.value;
	return token ? { token } : null;
}
