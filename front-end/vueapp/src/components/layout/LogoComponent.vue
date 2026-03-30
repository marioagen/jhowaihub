<template>
    <router-link
        class="d-flex align-items-center text-decoration-none"
        :class="collapsed ? 'justify-content-center' : 'w-100'"
        :to="{ name: 'Home' }"
    >
        <img
            v-if="collapsed"
            :src="logoSmallSrc"
            :title="$t('common.home')"
            width="35"
            height="35"
        />
        <img
            v-else
            :src="fullLogoSrc"
            :title="$t('common.home')"
            height="35"
            alt="WOOPI AI"
            style="margin-left: 0px"
        />
    </router-link>
</template>
<script>
    import logoDark from "@/assets/img/woopiai-logo-dark.png";
    import logoLight from "@/assets/img/woopiai-logo-light.png";
    import logoSmall from "@/assets/img/woopiai-hub-small-logo.png";

    export default {
        name: "LogoComponent",
        props: {
            collapsed: {
                type: Boolean,
                default: false,
            },
        },
        computed: {
            isDarkMode() {
                const theme =
                    this.$store.state.theme || localStorage.getItem("theme") || "css-theme-light";
                return theme === "css-theme-dark";
            },
            fullLogoSrc() {
                return this.isDarkMode ? logoLight : logoDark;
            },
            logoSmallSrc() {
                return logoSmall;
            },
        },
    };
</script>
