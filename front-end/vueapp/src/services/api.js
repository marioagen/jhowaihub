import axios from "axios";
import qs from "qs";
import router from "@/router";
import store from "@/store";
import { pageview } from "vue-gtag";

const api = axios.create();
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
        const maxRequests = 2;
        const originalRequest = error.config;
        if (originalRequest.url !== "/Account/Login-sso" &&  originalRequest.url != "/Account/Login" && originalRequest.url != "/Account/refresh-token" && originalRequest.url != "/Account/logout" && error.response) {
            if (error.response.status === 401 && !originalRequest._retry) {
                originalRequest._retry = true;
                if (!originalRequest._retry) {
                    originalRequest._retry = true;
                    originalRequest._retryCount = 1;
                } else {
                    originalRequest._retryCount += 1;
                }
            
                if (originalRequest._retryCount > maxRequests) {
                    console.log("Número máximo de tentativas de atualização do token atingido.");
                    router.push({ name: "Logout" });
                    return Promise.reject(error);
                }

                try {
                    const rs = await api.post("/Account/refresh-token", null);
                    if (rs && rs.data && rs.data.token) {
                        store.commit("updateUserProfile", {
                            amount: {
                                ...store.state.userProfile,
                                tokenApi: rs.data.token
                            }
                        });

                        originalRequest.headers["Authorization"] = `Bearer ${rs.data.token}`;
                        return api.request(originalRequest);
                    } else {
                        router.push({ name: "Logout" });
                        return Promise.reject(error);
                    }
                } catch (refreshError) {
                    console.log(refreshError);
                    router.push({ name: "Logout" });
                    return Promise.reject(refreshError);
                }
            }
        }

        return Promise.reject(error);
    }
);

export default api;
