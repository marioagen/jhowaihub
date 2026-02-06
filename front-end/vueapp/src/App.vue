<template>
    <component :is="layout === 'auth' ? AuthLayout : DefaultLayout" />
    <NotificationComponent />
</template>

<script setup>
    import { computed, onMounted } from "vue";
    import { useRoute } from "vue-router";
    import AuthLayout from "@/layouts/authLayout.vue";
    import DefaultLayout from "@/layouts/defaultLayout.vue";
    import { scheduleTokenRefresh } from "@/services/api";

    const route = useRoute();
    const layout = computed(() => route.meta.layout || "auth");

    onMounted(() => {
        scheduleTokenRefresh();
    });
</script>
