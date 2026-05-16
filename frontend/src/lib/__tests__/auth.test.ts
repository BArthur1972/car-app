import { describe, it, expect, beforeEach } from "vitest";
import { getToken, setToken, clearToken } from "@/lib/auth";

describe("auth", () => {
	beforeEach(() => {
		document.cookie = "token=; path=/; max-age=0";
	});

	it("returns undefined when no token is set", () => {
		expect(getToken()).toBeUndefined();
	});

	it("returns the token after setToken", () => {
		setToken("test-token");
		expect(getToken()).toBe("test-token");
	});

	it("returns undefined after clearToken", () => {
		setToken("test-token");
		clearToken();
		expect(getToken()).toBeUndefined();
	});
});
