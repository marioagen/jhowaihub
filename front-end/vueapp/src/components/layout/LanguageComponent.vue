<template>
    <div class="dropdown nav-buttons">
        <button
            class="btn btn-outline-primary table-btn btn-sm"
            type="button"
            data-bs-toggle="dropdown"
            aria-expanded="false"
            style="display: flex; align-items: center; justify-content: center"
        >
            <LucideIcon icon="Globe" />
        </button>
        <ul class="dropdown-menu dropdown-menu-button text-small shadow">
            <li>
                <a
                    :class="
                        $i18n.locale === 'pt'
                            ? 'btn btn-light lang-link lang-active'
                            : 'btn btn-light lang-link btn-lang'
                    "
                    @click="setLanguage('pt')"
                >
                    <img
                        src="@/assets/img/lang-pt.png"
                        alt="PT"
                    />
                    <span style="margin-left: 2px">PT</span>
                </a>
            </li>
            <li>
                <a
                    :class="
                        $i18n.locale === 'en'
                            ? 'btn btn-light lang-link lang-active btn-lang'
                            : 'btn btn-light lang-link btn-lang'
                    "
                    @click="setLanguage('en')"
                >
                    <img
                        src="@/assets/img/lang-en.png"
                        alt="EN"
                    />
                    <span style="margin-left: 2px">EN</span>
                </a>
            </li>
            <li>
                <a
                    :class="
                        $i18n.locale === 'es'
                            ? 'btn btn-light lang-link lang-active btn-lang'
                            : 'btn btn-light lang-link btn-lang'
                    "
                    @click="setLanguage('es')"
                >
                    <img
                        src="@/assets/img/lang-es.png"
                        alt="ES"
                    />
                    <span style="margin-left: 2px">ES</span>
                </a>
            </li>
        </ul>
    </div>
</template>
<script>
    export default {
        name: "LanguageComponent",
        methods: {
            setLanguage(lang) {
                this.$i18n.locale = lang;
                this.$store.commit("updateUserProfileLanguage", { amount: lang });
            },
        },
    };
</script>
<style scoped>
    .btn.btn-light.lang-link,
    .btn.btn-light.lang-link:hover,
    .btn.btn-light.lang-link.lang-active,
    .btn.btn-light.lang-link.lang-active:hover {
        padding: 0.35rem 0.55rem;
        background-color: transparent;
        border: none;
        cursor: pointer;
        min-height: 32px;
    }

    .lang-link {
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .lang-link.lang-active,
    .lang-link:hover {
        color: #0d6efd !important;
    }

    .btn-lang {
        padding: 2%;
        background-color: white;
        border-color: white;
    }
</style>
