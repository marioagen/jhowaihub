import axios from "axios";
import qs from "qs";
import router from "@/router";
import store from "@/store";
import { pageview } from "vue-gtag";
import { jwtDecode } from "jwt-decode";

const api = axios.create();

/** Timer for proactive token refresh (refresh before expiry to avoid 401). */
let _refreshTimerId = null;

/**
 * Cancels any scheduled proactive token refresh (e.g. on logout).
 */
export function cancelTokenRefresh() {
    if (_refreshTimerId) {
        clearTimeout(_refreshTimerId);
        _refreshTimerId = null;
    }
}

/**
 * Schedules a single proactive refresh of the access token before it expires.
 * Call after login and after each successful refresh so the user stays logged in.
 */
export function scheduleTokenRefresh() {
    cancelTokenRefresh();
    const token = store.state?.userProfile?.tokenApi;
    if (!token) return;
    try {
        const decoded = jwtDecode(token);
        const exp = decoded.exp;
        if (!exp) return;
        const nowSeconds = Math.floor(Date.now() / 1000);
        const secondsUntilExpiry = exp - nowSeconds;
        const refreshBeforeSeconds = 90; // Increased from 60s to 90s for better safety margin
        const delayMs = Math.max(1000, (secondsUntilExpiry - refreshBeforeSeconds) * 1000);
        
        console.log(`[Token Refresh] Agendado para ${Math.floor(delayMs / 1000)}s (${Math.floor(delayMs / 60000)} minutos)`);
        
        _refreshTimerId = setTimeout(async () => {
            _refreshTimerId = null;
            console.log("[Token Refresh] Executando refresh proativo...");
            try {
                const rs = await api.post("/Account/refresh-token", null);
                if (rs?.data?.token) {
                    console.log("[Token Refresh] Token renovado com sucesso");
                    store.commit("updateUserProfile", {
                        amount: { ...store.state.userProfile, tokenApi: rs.data.token },
                    });
                    scheduleTokenRefresh();
                } else {
                    console.warn("[Token Refresh] Resposta sem token válido");
                }
            } catch (error) {
                console.warn("[Token Refresh] Falha no refresh proativo:", error?.response?.status ?? error.message);
                // Proactive refresh failed; next API call will trigger 401 and interceptor will handle it
            }
        }, delayMs);
    } catch (error) {
        console.warn("[Token Refresh] Token inválido, não foi possível agendar refresh:", error.message);
    }
}
let baseUrlApi = ENV_CONFIG.VUE_APP_BASE_URL_API;
baseUrlApi = baseUrlApi.replace(/\/$/, "") + "/api";
api.defaults.baseURL = baseUrlApi;

api.defaults.paramsSerializer = {
    serialize: (params) => {
        return qs.stringify(params, { arrayFormat: "indices", allowDots: true });
    },
};

api.defaults.withCredentials = true;
api.defaults.headers.post["Content-Type"] = "application/json;charset=utf-8";
api.defaults.headers.get["Content-Type"] = "application/json;charset=utf-8";
api.defaults.headers.common["X-Time-Zone"] = window.Intl.DateTimeFormat().resolvedOptions().timeZone;

api.interceptors.request.use(
    (config) => {
        config.headers["X-Email"] = store.state.userProfile.login;
        config.headers["X-Tenant"] = store.state.userProfile.tenant;
        config.headers["X-Key-Mongo-Access"] = store.state.userProfile.keyMongoAccess;
        config.headers["X-Language"] = store.state.userProfile.language;
        if (config.headers.Authorization === undefined) {
            config.headers["Authorization"] = `Bearer ${store.state.userProfile.tokenApi}`;
        }
        pageview(config.url);
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

api.interceptors.response.use(
    response => response,
    async (error) => {
        const maxRefreshAttempts = 3;
        const originalRequest = error.config;
        const isAuthEndpoint =
            originalRequest.url === "/Account/Login-sso" ||
            originalRequest.url === "/Account/Login" ||
            originalRequest.url === "/Account/refresh-token" ||
            originalRequest.url === "/Account/logout";

        if (!isAuthEndpoint && error.response?.status === 401) {
            originalRequest._refreshCount = (originalRequest._refreshCount || 0) + 1;

            if (originalRequest._refreshCount > maxRefreshAttempts) {
                console.warn("Número máximo de tentativas de atualização do token atingido.");
                router.push({ name: "Logout" });
                return Promise.reject(error);
            }

            try {
                const rs = await api.post("/Account/refresh-token", null);
                if (rs?.data?.token) {
                    store.commit("updateUserProfile", {
                        amount: { ...store.state.userProfile, tokenApi: rs.data.token },
                    });
                    scheduleTokenRefresh();
                    originalRequest.headers["Authorization"] = `Bearer ${rs.data.token}`;
                    return api.request(originalRequest);
                }
                router.push({ name: "Logout" });
                return Promise.reject(error);
            } catch (refreshError) {
                console.warn("Falha ao renovar token:", refreshError?.response?.status ?? refreshError);
                router.push({ name: "Logout" });
                return Promise.reject(refreshError);
            }
        }

        return Promise.reject(error);
    }
);

export default api;
