'use client';

import Link from "next/link";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { login } from "@/lib/api";
import { setToken } from "@/lib/auth";

export default function Page() {
    const router = useRouter();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    const handleSubmit: React.SubmitEventHandler<HTMLFormElement> = async (e) => {
        e.preventDefault();
        
        setSubmitting(true);
        setError(null);
        try {
            const response = await login({ email, password });
            setToken(response.token);
            router.push("/");
        } catch (err) {
            console.log("Login failed with error: ", err);
            setError("Invalid email or password");
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
            <div className="bg-white rounded-lg shadow-md p-8 w-full max-w-sm">

                <h1 className="text-2xl font-bold text-gray-900 mb-6">Sign in</h1>

                <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                    {error && (
                        <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-md px-3 py-2">
                            {error}
                        </p>
                    )}

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
                        {submitting ? "Signing in..." : "Sign in"}
                    </button>
                </form>

                <p className="mt-4 text-sm text-gray-600 text-center">
                    Don't have an account?{" "}
                    <Link href="/register" className="text-blue-600 hover:text-blue-800">
                        Register
                    </Link>
                </p>

            </div>
        </div>
    );
}
