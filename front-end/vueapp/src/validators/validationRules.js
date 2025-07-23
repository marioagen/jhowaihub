import { defineRule } from 'vee-validate'
import { required } from '@vee-validate/rules'
import i18n from '@/locales/i18n';

defineRule('required', value => {
    if (required(value)) return true
    return i18n.global.t('validation.required') || 'Campo obrigatório.'
})

defineRule('custom_email', value => {
    console.log(value)
    if (!value || value.trim() === '') {
        return i18n.global.t('validation.required')
    }

    const hasAt = value.includes('@')
    const endsWithCom = value.toLowerCase().endsWith('.com')

    if (!hasAt || !endsWithCom) {
        return i18n.global.t('validation.email_simple')
    }

    return true
})

defineRule('custom_password', value => {
    console.log(value)
    if (!value || value.trim() === '') {
        return i18n.global.t('validation.required') || 'A senha é obrigatória.'
    }
    if (value.length < 6) {
        return i18n.global.t('validation.password_min') || 'A senha deve ter no mínimo 6 caracteres.'
    }
    if (!/[A-Z]/.test(value)) {
        return i18n.global.t('validation.password_uppercase') || 'A senha deve conter uma letra maiúscula.'
    }
    if (!/[0-9]/.test(value)) {
        return i18n.global.t('validation.password_number') || 'A senha deve conter um número.'
    }
    return true
})