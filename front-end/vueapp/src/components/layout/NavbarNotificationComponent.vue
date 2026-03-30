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
                        'notification-row row p-2 m-0',
                        notification.status === 'in_progress'
                            ? 'notification-in-progress'
                            : notification.success !== false
                              ? 'notification-completed'
                              : 'notification-failed',
                    ]"
                >
                    <div class="col-1 d-flex justify-content-center align-items-center p-0">
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
                        <LucideIcon
                            v-if="notification.status === 'completed'"
                            icon="CircleCheck"
                            size="20"
                        />
                    </div>
                    <div class="col-8">
                        <div class="d-flex flex-column">
                            <span
                                v-if="notification.title"
                                class="fw-bold mb-1"
                            >
                                {{ notification.title }}
                            </span>
                            <span class="text-sm">
                                {{ notification.fileName }}
                            </span>
                        </div>
                    </div>
                    <div class="col-3 d-flex justify-content-end align-items-center">
                        <span v-if="notification.link">
                            <a
                                :href="notification.link"
                                target="_blank"
                                rel="noopener noreferrer"
                                class="ms-2 flex-shrink-0"
                            >
                                <LucideIcon
                                    icon="ExternalLink"
                                    :size="18"
                                />
                            </a>
                        </span>
                        <button
                            v-if="notification.status === 'completed'"
                            type="button"
                            class="btn btn-link btn-sm p-0 ms-2 flex-shrink-0 text-danger notification-remove"
                            :aria-label="$t('common.remove', 'Remove')"
                            @click.stop="removeNotification(notification.id)"
                        >
                            <LucideIcon
                                icon="X"
                                :size="18"
                            />
                        </button>
                    </div>
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
        min-width: 400px;
        max-height: 360px;
        overflow-y: auto;
        padding: 0;
    }

    .notification-list-header {
        background-color: var(--color-bg-navbar);
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

    .notification-remove:hover {
        color: var(--color-body-content) !important;
    }

    .menu-right {
        right: 0 !important;
        left: auto !important;
    }
</style>
