import "./versionCheck.js";
import "@/assets/css/global.css";
import { isMockMode } from "@/mock/mockConfig.js";
import { bootstrapMockSession } from "@/mock/mockBootstrap.js";
import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import store from "./store";
import i18n from "./locales/i18n";
import VueGtag from "vue-gtag";
import tooltip from "@/directives/bootstrapTooltip";
import popover from "@/directives/bootstrapPopover";
import outsideClick from "@/directives/outsideClick.js";
import FloatingVue from "floating-vue";
import "@/validators/validationRules";
import "floating-vue/dist/style.css";

import LucideIcon from "@/components/global/LucideIcon.vue";
import NotificationComponent from "@/components/global/NotificationComponent.vue";
import { notify } from "@/utils/notification";

import "@/assets/css/bootstrap-5.0.2/css/bootstrap.min.css";
import "@/assets/webfont/fontawesome-5.15.4/css/all.min.css";
import "@vueform/multiselect/themes/default.css";

const savedTheme = localStorage.getItem("theme");
const themeClass = savedTheme === "css-theme-dark" ? "css-theme-dark" : "css-theme-light";
document.documentElement.className = themeClass;

if (isMockMode()) {
    document.documentElement.dataset.mockMode = "true";
    bootstrapMockSession();
}

const app = createApp(App);
app.use(FloatingVue);
app.use(router);
app.use(store);
store.commit("setTheme", themeClass);
app.use(i18n);
app.use(
    VueGtag,
    {
        pageTrackerTemplate(to) {
            return {
                page_title: to.name,
                page_path: to.fullPath,
            };
        },
        config: {
            id: ENV_CONFIG.VUE_APP_GTAG_ID,
        },
    },
    router
);

app.directive("outsideClick", outsideClick);
app.directive("tooltip", tooltip);
app.directive("popover", popover);
app.component("NotificationComponent", NotificationComponent);
app.component("LucideIcon", LucideIcon);

app.config.globalProperties.$notify = notify;
app.config.globalProperties.$appName = ENV_CONFIG.VUE_APP_NAME;
app.config.globalProperties.$clientIdAzure = ENV_CONFIG.VUE_APP_CLIENT_ID_AZURE;

app.config.globalProperties.$tBracketsToBraces = function (key) {
    return this.$t(key).replaceAll("[", "{{").replaceAll("]", "}}");
};

app.mount("#app");
