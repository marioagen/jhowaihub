<template>
    <a
        class="whatsapp-link"
        :href="whatsappLink"
        target="_blank"
        v-tooltip.up="$t('home.whatsApp.support')"
    >
        <i class="fab fa-whatsapp whatsapp-icon"></i>
    </a>
</template>
<script>
    export default {
        name: "WhatsAppComponent",
        data() {
            return {};
        },
        computed: {
            whatsappLink() {
                return (
                    ENV_CONFIG?.VUE_APP_WHATSAPP_LINK || ""
                );
            },
        },
    };
</script>
<style scoped>
    .whatsapp-link {
        position: fixed;
        width: 60px;
        height: 60px;
        bottom: 40px;
        right: 40px;
        background-color: #25d366;
        color: #fff;
        border-radius: 50px;
        text-align: center;
        font-size: 30px;
        box-shadow: 1px 1px 2px #888;
        z-index: 1000;
    }
    .whatsapp-icon {
        margin-top: 16px;
    }
</style>
