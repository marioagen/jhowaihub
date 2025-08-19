<template>
    <component 
        :is="iconComponent" 
        :size="size" 
        :stroke-width="strokeWidth" 
        :color="color"
        v-bind="$attrs" 
    />
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
            color: {
                type: String,
                required: false,
                default: "currentColor",
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
