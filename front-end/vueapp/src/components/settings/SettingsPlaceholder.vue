<template>
    <div class="settings-placeholder">
        <LucideIcon
            icon="Construction"
            :size="36"
            class="settings-placeholder__icon"
        />
        <h6 class="fw-semibold mb-1">{{ title }}</h6>
        <p class="text-muted small mb-0">{{ $t("settings.comingSoon") }}</p>
    </div>
</template>

<script>
    export default {
        name: "SettingsPlaceholder",
        props: {
            titleKey: {
                type: String,
                required: true,
            },
        },
        computed: {
            title() {
                return this.$t(this.titleKey);
            },
        },
    };
</script>

<style scoped>
    .settings-placeholder {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        text-align: center;
        min-height: 220px;
        padding: 2rem 1rem;
        color: var(--color-body-content);
    }

    .settings-placeholder__icon {
        color: var(--color-text-muted);
        margin-bottom: 0.75rem;
    }
</style>
