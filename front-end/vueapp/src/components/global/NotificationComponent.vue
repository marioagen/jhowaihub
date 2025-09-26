<template>
    <div class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 9999; pointer-events: auto">
        <transition-group name="fade" tag="div">
            <div
                v-for="(notification, index) in notifications"
                :key="notification.id"
                :class="['alert', `alert-${notification.variant}`, 'd-flex', 'align-items-center', 'fade', 'show']"
                role="alert"
            >
                <LucideIcon v-if="notification.icon" :icon="notification.icon" :size="20" class="me-2" />

                <div class="flex-grow-1">
                    <strong v-if="notification.title">{{ $t(notification.title) }}:</strong>
                    {{ $t(notification.message) }}
                </div>

                <button
                    type="button"
                    class="btn-close ms-2"
                    aria-label="Close"
                    @mousedown.prevent
                    @click.stop="remove(notification.id)"
                ></button>
            </div>
        </transition-group>
    </div>
</template>

<script>
    export default {
        name: "NotificationComponent",
        data() {
            return {
                notifications: [],
            };
        },
        created() {
            window.addEventListener("notification:show", this.handleNotification);
        },
        unmounted() {
            window.removeEventListener("notification:show", this.handleNotification);
        },
        methods: {
            handleNotification(event) {
                const data = event.detail;
                const id = Date.now() + Math.random();

                const notification = {
                    id,
                    title: data.title || "",
                    message: data.message || "",
                    variant: data.variant || "primary",
                    icon: data.icon || null,
                    duration: data.duration || 3000,
                };

                this.notifications.push(notification);

                setTimeout(() => {
                    this.notifications = this.notifications.filter((n) => n.id !== id);
                }, notification.duration);
            },
            remove(id) {
                this.notifications = this.notifications.filter((n) => n.id !== id);
            },
        },
    };
</script>

<style scoped>
    .fade-enter-active,
    .fade-leave-active {
        transition: opacity 0.5s;
    }
    .fade-enter-from,
    .fade-leave-to {
        opacity: 0;
    }
</style>
