export const getToken = () =>
	document.cookie
		.split("; ")
		.find((row) => row.startsWith("token="))
		?.split("=")[1];

export const setToken = (token: string) => {
	document.cookie = `token=${token}; path=/; SameSite=Strict`;
};

export const clearToken = () => {
	document.cookie = "token=; path=/; max-age=0";
};
