export function isMockMode() {
    if (import.meta.env.VITE_MOCK_MODE === "true") {
        return true;
    }
    if (typeof ENV_CONFIG !== "undefined") {
        const flag = ENV_CONFIG.VITE_MOCK_MODE;
        return flag === true || flag === "true";
    }
    return false;
}

export const MOCK_TENANT = "prototype";
export const MOCK_USER_EMAIL = "demo@prototype.local";
export const MOCK_USER_NAME = "Demo Prototype";
export const MOCK_KEY_MONGO_ACCESS = "mock-local-prototype";
