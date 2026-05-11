'use client';

import Link from "next/link";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { register } from "@/lib/api";
import { ApiError } from "@/lib/http-client";

export default function Page() {
	const router = useRouter();
	const [username, setUsername] = useState("");
	const [email, setEmail] = useState("");
	const [password, setPassword] = useState("");
	const [error, setError] = useState<string | null>(null);
	const [submitting, setSubmitting] = useState(false);

	const handleSubmit: React.SubmitEventHandler<HTMLFormElement> = async (e) => {
		e.preventDefault();

		setSubmitting(true);
		setError(null);
		try {
			await register({ username, email, password });
			router.push("/login");
		} catch (err: unknown) {
			if (err instanceof ApiError && err.status === 409) {
				setError("Email is already registered");
			} else {
				setError("Something went wrong. Please try again.");
			}
		} finally {
			setSubmitting(false);
		}
	};

	return (
		<div className="min-h-screen bg-gray-50 flex items-center justify-center">
			<div className="bg-white rounded-lg shadow-md p-8 w-full max-w-sm">

				<h1 className="text-2xl font-bold text-gray-900 mb-6">Create account</h1>

				<form onSubmit={handleSubmit} className="flex flex-col gap-4">
					{error && (
						<p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-md px-3 py-2">
							{error}
						</p>
					)}

					<div>
						<label className="block text-gray-700">Username</label>
						<input
							type="text"
							value={username}
							onChange={(e) => setUsername(e.target.value)}
							className="mt-1 block w-full border-gray-300 rounded-md shadow-sm text-gray-900"
						/>
					</div>

					<div>
						<label className="block text-gray-700">Email</label>
						<input
							type="email"
							value={email}
							onChange={(e) => setEmail(e.target.value)}
							className="mt-1 block w-full border-gray-300 rounded-md shadow-sm text-gray-900"
						/>
					</div>

					<div>
						<label className="block text-gray-700">Password</label>
						<input
							type="password"
							value={password}
							onChange={(e) => setPassword(e.target.value)}
							className="mt-1 block w-full border-gray-300 rounded-md shadow-sm text-gray-900"
						/>
					</div>

					<button
						type="submit"
						disabled={submitting}
						className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed">
						{submitting ? "Creating account..." : "Create account"}
					</button>
				</form>

				<p className="mt-4 text-sm text-gray-600 text-center">
					Already have an account?{" "}
					<Link href="/login" className="text-blue-600 hover:text-blue-800">
						Sign in
					</Link>
				</p>

			</div>
		</div>
	);
}
