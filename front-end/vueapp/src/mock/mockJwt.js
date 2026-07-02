function encodeBase64Url(value) {
    return btoa(value).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

export function createMockJwt() {
    const header = encodeBase64Url(JSON.stringify({ alg: "HS256", typ: "JWT" }));
    const payload = encodeBase64Url(
        JSON.stringify({
            permissions: "[]",
            isAdmin: "true",
            exp: Math.floor(Date.now() / 1000) + 86400 * 365,
        }),
    );
    return `${header}.${payload}.mock-signature`;
}
