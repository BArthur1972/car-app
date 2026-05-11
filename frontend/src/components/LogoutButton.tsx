'use client';

import { useRouter } from "next/navigation";
import { clearToken } from "@/lib/auth";

export default function LogoutButton() {
	const router = useRouter();

	const handleLogout = () => {
		clearToken();
		router.push("/login");
	};

	return (
		<button
			onClick={handleLogout}
			className="px-4 py-2 text-sm text-gray-600 hover:text-gray-900">
			Logout
		</button>
	);
}
