<template>
    <component :is="iconComponent" v-bind="$attrs" :size="size" :stroke-width="strokeWidth" />
</template>

<script>
    import * as LucideIcons from "lucide-vue-next";
    export default {
        props: {
            icon: {
                type: String,
                required: true,
            },
            size: {
                type: [Number, String],
                required: false,
                default: 20,
            },
            strokeWidth: {
                type: [Number, String],
                required: false,
                default: 1.5,
            },
        },
        computed: {
            iconComponent() {
                return LucideIcons[this.icon] || null;
            },
        },
    };
</script>
