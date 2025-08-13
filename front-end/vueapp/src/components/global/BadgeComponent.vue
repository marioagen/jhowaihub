<template>
    <span
        :class="badgeClasses"
        @click="handleClick"
        :style="clickable ? 'cursor: pointer;' : ''"
        role="button"
        tabindex="0"
    >
        {{ text }}
    </span>
</template>

<script>
    export default {
        name: "BadgeComponent",
        props: {
            text: {
                type: String,
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
        },
        computed: {
            badgeClasses() {
                return ["badge", `bg-${this.variant}`, { "rounded-pill": this.pill }];
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
    .badge {
        background-color: transparent;
        font-size: 0.75em;
        font-weight: 500;
        padding: 0.25em 0.5em;
        border-radius: 0.375rem;
        border: 1px solid currentColor;
        display: inline-block;
        white-space: nowrap;
    }
</style>
