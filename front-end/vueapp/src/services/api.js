import axios from "axios";
import qs from "qs";
import router from "@/router";
import store from "@/store";
import { pageview } from "vue-gtag";
import { jwtDecode } from "jwt-decode";
import LogService from "@/services/log/logService";
import { isMockMode } from "@/mock/mockConfig.js";
import { installMockApi } from "@/mock/installMockApi.js";

const api = axios.create();
installMockApi(api);

let _refreshTimerId = null;

export function cancelTokenRefresh() {
    if (_refreshTimerId) {
        clearTimeout(_refreshTimerId);
        _refreshTimerId = null;
    }
}

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
        const refreshBeforeSeconds = 60;
        const delayMs = Math.max(1000, (secondsUntilExpiry - refreshBeforeSeconds) * 1000);
        _refreshTimerId = setTimeout(async () => {
            _refreshTimerId = null;
            try {
                const rs = await api.post("/Account/refresh-token", null);
                if (rs?.data?.token) {
                    store.commit("updateUserProfile", {
                        amount: { ...store.state.userProfile, tokenApi: rs.data.token },
                    });
                    scheduleTokenRefresh();
                }
            } catch (_) {
                LogService.showMessage(
                    "Erro ao renovar o token"
                );
            }
        }, delayMs);
    } catch (_) {
        LogService.showMessage(
            "Erro rodar a tarefa de renovação de token"
        );
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

        if (isMockMode()) {
            return Promise.reject(error);
        }

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
