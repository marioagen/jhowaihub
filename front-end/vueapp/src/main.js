import "@/assets/css/global.css";
import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import store from "./store";
import i18n from "./locales/i18n";
import VueGtag from "vue-gtag";
import tooltip from "@/directives/bootstrapTooltip";

import LucideIcon from "@/components/global/LucideIcon.vue";
import NotificationComponent from "@/components/global/NotificationComponent.vue";
import { notify } from "@/utils/notification";

window.$ = window.jQuery = require("jquery");
import "@/assets/css/bootstrap-5.0.2/css/bootstrap.min.css";
import "@/assets/webfont/fontawesome-5.15.4/css/all.min.css";

const app = createApp(App);
app.use(router);
app.use(store);
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

app.directive("tooltip", tooltip);
app.component("NotificationComponent", NotificationComponent);
app.component("LucideIcon", LucideIcon);

app.config.globalProperties.$notify = notify;
app.config.globalProperties.$appName = ENV_CONFIG.VUE_APP_NAME;
app.config.globalProperties.$clientIdAzure = ENV_CONFIG.VUE_APP_CLIENT_ID_AZURE;
app.mount("#app");
