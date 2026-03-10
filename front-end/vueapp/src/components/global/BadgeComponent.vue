<template>
    <span
        class="badge-number"
        :class="[`badge-${variant}`, size !== 'md' && `badge-number-${size}`]"
        @click="handleClick"
        :style="clickable ? 'cursor: pointer;' : ''"
        role="button"
        tabindex="0"
    >
        {{ typeof text === "string" ? $t(text) : text }}
    </span>
</template>
<script>
    export default {
        name: "BadgeComponent",
        props: {
            text: {
                type: [Number, String],
                required: true,
            },
            variant: {
                type: String,
                default: "primary",
            },
            pill: {
                type: Boolean,
                default: true,
            },
            clickable: {
                type: Boolean,
                default: true,
            },
            size: {
                type: String,
                default: "md",
            },
        },
        methods: {
            handleClick(event) {
                if (this.clickable) {
                    this.$emit("setClick", event);
                }
            },
        },
    };
</script>
<style>
    .badge-number {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        border-radius: 999px;
        font-weight: 500;
        font-size: 13px;
        padding: 0 0.5rem;
        min-height: 24px;
    }

    .badge-number-sm {
        font-size: 11px;
        padding: 0 0.35rem;
        min-height: 18px;
    }

    .badge-primary {
        background-color: #e0f0ff;
        color: #4e85d7;
    }
    .badge-danger {
        background-color: #ffe0e0;
        color: #d74e4e;
    }
    .badge-success {
        background-color: #e0ffe9;
        color: #4ed77a;
    }
    .badge-warning {
        background-color: #fff7e0;
        color: #d7a54e;
    }
    .badge-info {
        background-color: #e0faff;
        color: #4ed7d7;
    }
    .badge-secondary {
        background-color: #ececec;
        color: #6c757d;
    }
</style>
