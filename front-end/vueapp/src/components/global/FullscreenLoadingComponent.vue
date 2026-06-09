<template>
    <div class="loading-overlay">
        <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">{{ $t("common.loading") }}</span>
        </div>
    </div>
</template>

<script>
    export default {
        name: "FullscreenLoadingComponent",
    };
</script>

<style scoped>
    .loading-overlay {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(255, 255, 255, 0.6);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1050;
        pointer-events: all;
    }

    .loading-overlay .spinner-border {
        width: 4rem;
        height: 4rem;
        animation: spin-scale 1.5s linear infinite;
    }

    @keyframes spin-scale {
        0% {
            transform: rotate(0deg) scale(1);
        }
        50% {
            transform: rotate(180deg) scale(1.1);
        }
        100% {
            transform: rotate(360deg) scale(1);
        }
    }
</style>