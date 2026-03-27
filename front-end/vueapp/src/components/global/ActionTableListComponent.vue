<template>
    <div class="d-flex flex-row justify-content-end w-100 gap-3">
        <slot :actionClass="actionClass" />
    </div>
</template>
<script>
    export default {
        name: "ActionTableListComponent",
        data() {
            return {
                actionClass: "p-0 d-flex align-items-center gap-2 link-action",
            };
        },
    };
</script>
<style scoped>
    :deep(.link-action) {
        color: var(--color-bg-icon-active);
    }
    :deep(.link-action:hover) {
        cursor: pointer;
    }
</style>
