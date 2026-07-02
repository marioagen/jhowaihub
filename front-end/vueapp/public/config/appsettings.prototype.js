const ENV_CONFIG = (() => {
    return {
        VITE_APP_VERSION: "prototype",
        VUE_APP_NAME: "WOOPI AI",
        VUE_APP_BASE_URL_API: "https://prototype.local",
        VUE_APP_WAITING_TIME_MSG_UPLD: "5000",
        VUE_APP_TIMER_REQ: "1",
        VUE_APP_GTAG_ID: "G-XXXXXXXXXX",
        VUE_APP_ENV_TYPE: "Prototype",
        VUE_APP_HOME_VIDEO_URL: "",
        VUE_APP_HOME_GUIDE_URL: "",
        VUE_APP_HOME_DOCS_URL: "",
        VUE_APP_WHATSAPP_LINK:
            "https://api.whatsapp.com/send/?phone=%2B5511918020002&text&type=phone_number&app_absent=0",
        VUE_APP_SHOW_WHATSAPP_LINK: false,
        VUE_APP_MCP_IS_ACTIVE: true,
        VUE_APP_SUBSCRIBE_PLAN_URL: "https://site.qa.woopi.ai/planos.html",
        VITE_MOCK_MODE: true,
    };
})();

document.title = ENV_CONFIG.VUE_APP_NAME;
