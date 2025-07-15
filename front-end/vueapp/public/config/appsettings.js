const ENV_CONFIG = (() => {
    return {
        VUE_APP_NAME: 'Woopi AI Hub',
        VUE_APP_BASE_URL_API: 'https://localhost:7045',
        VUE_APP_WAITING_TIME_MSG_UPLD: '5000',
        VUE_APP_TIMER_REQ: '1',
        VUE_APP_GTAG_ID: 'G-XXXXXXXXXX',
    }
})()

document.title = ENV_CONFIG.VUE_APP_NAME
