import { createI18n } from 'vue-i18n'
import store from '@/store'

import portuguese from './translations/pt'
import english from './translations/en'
import spanish from './translations/es'

const i18n = createI18n({
    locale: store.state.userProfile.language, // set locale
    fallbackLocale: 'pt', // set fallback locale
    messages: {
        // set locale resources
        pt: portuguese,
        en: english,
        es: spanish,
    },
})

export default i18n
