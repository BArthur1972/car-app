export const getToken = () => {
	const value = document.cookie
		.split("; ")
		.find((row) => row.startsWith("token="))
		?.split("=")[1];
	return value || undefined;
};

export const setToken = (token: string) => {
	document.cookie = `token=${token}; path=/; SameSite=Strict`;
};

export const clearToken = () => {
	document.cookie = "token=; path=/; max-age=0";
};
