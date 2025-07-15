import '@/assets/css/global.css'

import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import i18n from './locales/i18n'
import VueGtag from 'vue-gtag'
import LucideIcon from '@/components/global/LucideIcon.vue'

window.$ = window.jQuery = require('jquery')

import '@/assets/css/bootstrap-5.0.2/css/bootstrap.min.css'
import '@/assets/webfont/fontawesome-5.15.4/css/all.min.css'
import '@/assets/css/bootstrap-5.0.2/js/bootstrap.bundle.min'

const app = createApp(App)
app.use(router)
app.use(store)
app.use(i18n)
app.use(
    VueGtag,
    {
        pageTrackerTemplate(to) {
            return {
                page_title: to.name,
                page_path: to.fullPath,
            }
        },
        config: {
            id: ENV_CONFIG.VUE_APP_GTAG_ID,
        },
    },
    router
)

app.component('LucideIcon', LucideIcon)
app.config.globalProperties.$appName = ENV_CONFIG.VUE_APP_NAME
app.config.globalProperties.$clientIdAzure = ENV_CONFIG.VUE_APP_CLIENT_ID_AZURE

app.mount('#app')
