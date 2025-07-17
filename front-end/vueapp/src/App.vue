<template>
    <component :is="layout === 'auth' ? AuthLayout : DefaultLayout" />
</template>

<script setup>
    import { computed } from "vue";
    import { useRoute } from "vue-router";
    import AuthLayout from "@/layouts/authLayout.vue";
    import DefaultLayout from "@/layouts/defaultLayout.vue";

    const route = useRoute();
    const layout = computed(() => route.meta.layout || "auth");
</script>
