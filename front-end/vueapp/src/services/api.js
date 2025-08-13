import axios from "axios";
import qs from "qs";
import router from "@/router";
import store from "@/store";
import { pageview } from "vue-gtag";

const api = axios.create();

// Show env config
console.log(ENV_CONFIG.VUE_APP_NAME);
console.log(ENV_CONFIG.VUE_APP_BASE_URL_API);

let baseUrlApi = ENV_CONFIG.VUE_APP_BASE_URL_API;
baseUrlApi = baseUrlApi.replace(/\/$/, "") + "/api";
api.defaults.baseURL = baseUrlApi; // App on development environment
//api.defaults.baseURL = "/api"; // App on test environment

api.defaults.paramsSerializer = {
    serialize: (params) => {
        return qs.stringify(params, { arrayFormat: "indices", allowDots: true });
    },
};

api.defaults.headers.post["Content-Type"] = "application/json;charset=utf-8";
api.defaults.headers.post["Access-Control-Allow-Origin"] = "*";
api.defaults.headers.get["Content-Type"] = "application/json;charset=utf-8";
api.defaults.headers.get["Access-Control-Allow-Origin"] = "*";

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
        //Google Analytics
        pageview(config.url);

        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

api.interceptors.response.use(
    (response) => {
        return response;
    },
    async (error) => {
        const originalRequest = error.config;
        if (originalRequest.url !== "/Account/Authenticate" && error.response) {
            if (error.response != null && error.response.status === 401 && !originalRequest._retry) {
                originalRequest._retry = true;
                try {
                    var formData = new FormData();
                    formData.append("login", store.state.userProfile.login);
                    const rs = await api
                        .post("/Account/Authenticate", formData, {
                            headers: { Authorization: `Bearer ${store.state.userProfile.tokenAzure}` },
                        })
                        .catch((err) => {
                            console.log(err);
                            router.push({ name: "Logout" });
                        });
                    if (rs && rs.data) {
                        var selectedTenant = store.state.userProfile.tenant;
                        var newDataUser = {
                            language: store.state.userProfile.language,
                            image: store.state.userProfile.image,
                            name: store.state.userProfile.name,
                            login: store.state.userProfile.login,
                            tokenAzure: store.state.userProfile.tokenAzure,
                            tokenApi: rs.data.token,
                            tenant: selectedTenant,
                            keyMongoAccess: store.state.userProfile.keyMongoAccess,
                        };
                        store.commit("updateUserProfile", { amount: newDataUser });
                        error.config.headers["Authorization"] = `Bearer ${rs.data.token}`;
                        return new Promise((resolve, reject) => {
                            api.request(originalRequest)
                                .then((response) => {
                                    resolve(response);
                                })
                                .catch((e) => {
                                    reject(e);
                                });
                        });
                    }
                } catch (_error) {
                    return Promise.reject(_error);
                }
            }
        }
        return Promise.reject(error);
    }
);

export default api;
