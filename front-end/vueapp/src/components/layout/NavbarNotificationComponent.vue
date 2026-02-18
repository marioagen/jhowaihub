<template>
    <div class="dropdown nav-buttons notification-dropdown">
        <button
            id="notificationDropdown"
            class="btn btn-outline-primary table-btn btn-sm position-relative"
            type="button"
            data-bs-toggle="dropdown"
            data-bs-auto-close="outside"
            aria-expanded="false"
            style="display: flex; align-items: center; justify-content: center"
        >
            <LucideIcon icon="Bell" />
            <span
                v-if="showNotificationDot"
                class="notification-dot"
                aria-hidden="true"
            ></span>
            <span
                v-if="unreadNotificationCount > 0"
                class="notification-badge"
            >
                {{ unreadNotificationCount }}
            </span>
        </button>
        <ul
            class="dropdown-menu dropdown-menu-notifications text-small shadow menu-right"
            aria-labelledby="notificationDropdown"
        >
            <li class="notification-list-header px-3 py-2 border-bottom">
                <span class="fw-semibold">
                    {{ $t("common.notifications", "Notifications") }}
                </span>
            </li>
            <li
                v-if="uploadNotifications.length === 0"
                class="px-3 py-4 text-muted text-center"
            >
                {{ $t("common.noNotifications", "No notifications") }}
            </li>
            <li
                v-for="notification in uploadNotifications"
                :key="notification.id"
                class="notification-item remove-hover"
            >
                <div
                    :class="[
                        'notification-row d-flex align-items-center justify-content-between px-3 py-2',
                        notification.status === 'in_progress'
                            ? 'notification-in-progress'
                            : notification.success !== false
                              ? 'notification-completed'
                              : 'notification-failed',
                    ]"
                >
                    <span class="notification-file-name text-truncate flex-grow-1 min-width-0">
                        {{ notification.fileName }}
                    </span>
                    <span
                        v-if="notification.status === 'in_progress'"
                        class="d-flex align-items-center ms-2 flex-shrink-0"
                    >
                        <span
                            class="spinner-border spinner-border-sm"
                            role="status"
                            aria-hidden="true"
                        ></span>
                    </span>
                    <button
                        v-if="notification.status === 'completed'"
                        type="button"
                        class="btn btn-link btn-sm p-0 ms-2 flex-shrink-0 text-muted notification-remove"
                        :aria-label="$t('common.remove', 'Remove')"
                        @click.stop="removeNotification(notification.id)"
                    >
                        <LucideIcon
                            icon="X"
                            :size="18"
                        />
                    </button>
                </div>
            </li>
        </ul>
    </div>
</template>
<script>
    export default {
        name: "NavbarNotificationComponent",
        methods: {
            removeNotification(id) {
                this.$store.commit("removeUploadNotification", { id });
            },
        },
        computed: {
            uploadNotifications() {
                return this.$store.state.uploadNotifications || [];
            },
            unreadNotificationCount() {
                return this.uploadNotifications.length;
            },
            showNotificationDot() {
                const list = this.$store.state.uploadNotifications || [];
                return list.some((n) => n.id && !String(n.id).startsWith("dummy-"));
            },
        },
    };
</script>
<style scoped>
    .notification-dropdown {
        margin-right: 0.25rem;
    }

    .notification-badge {
        position: absolute;
        top: -2px;
        right: -2px;
        min-width: 1.1rem;
        height: 1.1rem;
        padding: 0 0.25rem;
        font-size: 0.65rem;
        line-height: 1.1rem;
        text-align: center;
        color: white;
        background-color: var(--color-bg-btn-primary);
        border-radius: 50%;
    }

    .dropdown-menu-notifications {
        margin-top: 1rem !important;
        min-width: 320px;
        max-height: 360px;
        overflow-y: auto;
    }

    .notification-list-header {
        background-color: var(--color-bg-page-link);
    }

    .notification-item .dropdown-item,
    .notification-item a:hover {
        color: inherit;
        background-color: transparent;
    }

    .notification-row {
        border-radius: 0.25rem;
    }

    .notification-completed {
        background-color: var(--color-bg-toast-content-success);
        color: var(--color-toast-content-success);
    }

    .notification-in-progress {
        background-color: var(--color-bg-toast-content-primary);
        color: var(--color-toast-content-primary);
    }

    .notification-failed {
        background-color: var(--color-bg-toast-content-danger);
        color: var(--color-toast-content-danger);
    }

    .notification-dot {
        position: absolute;
        top: 2px;
        right: 2px;
        width: 8px;
        height: 8px;
        background-color: var(--color-bg-btn-danger);
        border-radius: 50%;
        border: 1px solid #fff;
    }

    .notification-file-name {
        max-width: 160px;
    }

    .notification-remove:hover {
        color: var(--color-body-content) !important;
    }

    .menu-right {
        right: 0 !important;
        left: auto !important;
    }
</style>
