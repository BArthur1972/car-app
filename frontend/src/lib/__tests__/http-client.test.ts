import { describe, it, expect } from "vitest";
import { ApiError } from "@/lib/http-client";

describe("ApiError", () => {
	it("sets the status code", () => {
		const error = new ApiError(404, "Not Found");
		expect(error.status).toBe(404);
	});

	it("sets the message", () => {
		const error = new ApiError(401, "Unauthorized");
		expect(error.message).toBe("Unauthorized");
	});

	it("is an instance of Error", () => {
		const error = new ApiError(500, "Internal Server Error");
		expect(error).toBeInstanceOf(Error);
	});

	it("has the correct name", () => {
		const error = new ApiError(400, "Bad Request");
		expect(error.name).toBe("ApiError");
	});
});
