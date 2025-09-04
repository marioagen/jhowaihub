<template>
    <span
        :class="['badge-outline', 'variant-secondary', { clickable, [`hover-${variant}`]: clickable }]"
        :tabindex="clickable ? 0 : undefined"
        @click="handleClick"
        @keydown.enter="handleClick"
        role="button"
    >
        {{ text }}
    </span>
</template>

<script>
    export default {
        name: "BadgeOutlined",
        props: {
            text: {
                type: String,
                required: true,
            },
            variant: {
                type: String,
                default: "primary",
            },
            clickable: {
                type: Boolean,
                default: true,
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

<style scoped>
    .badge-outline {
        background-color: transparent;
        font-size: 0.75em;
        font-weight: 500;
        padding: 0.25em 0.5em;
        border-radius: 0.375rem;
        border: 1px solid currentColor;
        display: inline-block;
        white-space: nowrap;
        transition:
            color 0.2s,
            border-color 0.2s;
        cursor: default;
    }

    .variant-secondary {
        color: var(--bs-secondary);
        border-color: var(--bs-secondary);
    }

    .clickable {
        cursor: pointer;
    }

    .hover-primary:hover {
        color: var(--bs-primary);
        border-color: var(--bs-primary);
    }
    .hover-secondary:hover {
        color: var(--bs-secondary);
        border-color: var(--bs-secondary);
    }
    .hover-success:hover {
        color: var(--bs-success);
        border-color: var(--bs-success);
    }
    .hover-danger:hover {
        color: var(--bs-danger);
        border-color: var(--bs-danger);
    }
    .hover-warning:hover {
        color: var(--bs-warning);
        border-color: var(--bs-warning);
    }
    .hover-info:hover {
        color: var(--bs-info);
        border-color: var(--bs-info);
    }
    .hover-dark:hover {
        color: var(--bs-dark);
        border-color: var(--bs-dark);
    }
</style>
