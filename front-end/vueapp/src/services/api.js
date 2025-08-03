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
        //Google Analytics
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
    const originalRequest = error.config;
    console.log(originalRequest);
    if (originalRequest.url !== "/Account/Login-sso" &&  originalRequest.url != "/Account/Login" && originalRequest.url != "/Account/refresh-token" && error.response) {
      if (error.response.status === 401 && !originalRequest._retry) {
        originalRequest._retry = true;

        try {
          // Chama o endpoint de refresh token, sem corpo, com cookie HttpOnly
          const rs = await api.post("/Account/refresh-token", null);

          if (rs && rs.data && rs.data.token) {
            // Atualiza o token no Vuex store
            store.commit("updateUserProfile", {
              amount: {
                ...store.state.userProfile,
                tokenApi: rs.data.token
              }
            });

            // Atualiza o header Authorization da requisição original
            originalRequest.headers["Authorization"] = `Bearer ${rs.data.token}`;

            // Reenvia a requisição original com token renovado
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
