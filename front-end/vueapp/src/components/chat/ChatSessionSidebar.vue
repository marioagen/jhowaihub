<template>
    <aside
        class="chat-session-sidebar"
        :class="{ 'chat-session-sidebar--mobile-open': mobileOpen }"
    >
        <div class="chat-session-sidebar__header">
            <h6 class="mb-0 fw-bold chat-session-sidebar__title">{{ $t("chat.sessionsTitle") }}</h6>
            <button
                type="button"
                class="btn btn-sm btn-primary"
                @click="$emit('new-session')"
            >
                <LucideIcon
                    icon="Plus"
                    :size="16"
                />
                <span class="d-none d-md-inline ms-1">{{ $t("chat.newSession") }}</span>
            </button>
        </div>

        <button
            type="button"
            class="btn btn-outline-secondary btn-sm w-100 mb-2"
            @click="$emit('generate-demo')"
        >
            <LucideIcon
                icon="Wand2"
                :size="16"
            />
            {{ $t("chat.generateDemo") }}
        </button>

        <div class="chat-session-sidebar__list">
            <button
                v-for="session in sessions"
                :key="session.id"
                type="button"
                class="chat-session-item"
                :class="{ active: session.id === activeSessionId }"
                @click="$emit('select-session', session.id)"
            >
                <LucideIcon
                    icon="MessageSquare"
                    :size="16"
                />
                <div class="chat-session-item__text">
                    <span>{{ session.title }}</span>
                    <small>{{ formatDate(session.updatedAt) }}</small>
                </div>
                <button
                    type="button"
                    class="btn btn-link btn-sm p-0 chat-session-item__delete"
                    :title="$t('common.delete')"
                    @click.stop="$emit('delete-session', session.id)"
                >
                    <LucideIcon
                        icon="Trash2"
                        :size="14"
                    />
                </button>
            </button>

            <p
                v-if="!sessions.length"
                class="text-muted small px-2"
            >
                {{ $t("chat.noSessions") }}
            </p>
        </div>
    </aside>
</template>

<script>
    export default {
        name: "ChatSessionSidebar",
        props: {
            sessions: {
                type: Array,
                default: () => [],
            },
            activeSessionId: {
                type: String,
                default: null,
            },
            mobileOpen: {
                type: Boolean,
                default: false,
            },
        },
        emits: ["new-session", "select-session", "delete-session", "generate-demo"],
        methods: {
            formatDate(iso) {
                if (!iso) return "";
                return new Date(iso).toLocaleDateString(undefined, {
                    day: "2-digit",
                    month: "short",
                });
            },
        },
    };
</script>

<style scoped>
    .chat-session-sidebar {
        width: 280px;
        flex-shrink: 0;
        display: flex;
        flex-direction: column;
        border-right: 1px solid var(--color-border-form-control);
        background: var(--color-bg-sidebar-content);
        color: var(--color-body-content);
        padding: 0.75rem;
        height: 100%;
    }

    .chat-session-sidebar__title {
        color: var(--color-heading-title, var(--color-body-content));
    }

    .chat-session-sidebar__header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 0.5rem;
        margin-bottom: 0.75rem;
    }

    .chat-session-sidebar__list {
        flex: 1;
        overflow-y: auto;
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
    }

    .chat-session-item {
        display: flex;
        align-items: flex-start;
        gap: 0.5rem;
        width: 100%;
        text-align: left;
        border: none;
        background: transparent;
        border-radius: 8px;
        padding: 0.55rem 0.5rem;
        color: var(--color-body-content);
    }

    .chat-session-item:hover {
        background: var(--color-sidebar-li-collapsed-hover);
    }

    .chat-session-item.active {
        background: var(--color-bg-sidebar-li-selected);
    }

    .chat-session-item__text {
        flex: 1;
        min-width: 0;
    }

    .chat-session-item__text span {
        display: block;
        font-size: 0.85rem;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .chat-session-item__text small {
        color: var(--color-text-muted);
    }

    .chat-session-item__delete {
        opacity: 0;
        color: var(--color-text-muted);
    }

    .chat-session-item__delete:hover {
        color: var(--color-bg-btn-danger);
    }

    .chat-session-item:hover .chat-session-item__delete {
        opacity: 1;
    }

    @media (max-width: 768px) {
        .chat-session-sidebar {
            position: fixed;
            top: 58px;
            left: 0;
            bottom: 0;
            z-index: 1040;
            transform: translateX(-100%);
            transition: transform 0.25s ease;
            box-shadow: 4px 0 24px rgba(0, 0, 0, 0.12);
        }

        .chat-session-sidebar--mobile-open {
            transform: translateX(0);
        }
    }
</style>
