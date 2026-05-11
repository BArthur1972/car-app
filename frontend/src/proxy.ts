import { NextRequest, NextResponse } from "next/server";

export function proxy(request: NextRequest) {
	// Id the user is not authenticated and the request requires authentication,
	// we redirect the user to the login page.
	const token = request.cookies.get("token");
	const isAuthRequired =
		!request.nextUrl.pathname.startsWith("/login") &&
		!request.nextUrl.pathname.startsWith("/register");

	if (!token && isAuthRequired) {
		return NextResponse.redirect(new URL("/login", request.url));
	}
}

export const config = {
	matcher: ["/((?!_next|favicon.ico).*)"],
};
