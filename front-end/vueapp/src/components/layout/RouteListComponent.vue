<template>
    <ul class="btn-toggle-nav list-unstyled fw-normal pb-1 small">
        <li
            v-for="(item, index) in items"
            :key="item.activeKey ? `${item.activeKey}-${index}` : `nav-${index}`"
            class="mb-1 sidebar-menu-item-enter"
            :class="{
                'is-active': item.visibleGroup ? isGroupActive(item) : isRouteActive(item),
            }"
            :style="{ '--item-index': index }"
        >
            <div v-if="item.visibleGroup">
                <button
                    type="button"
                    class="d-flex align-items-center custom-menu-item sidebar-group-toggle link-dark rounded border-0 bg-transparent w-100 text-start"
                    :class="[
                        isGroupActive(item) ? 'active' : '',
                        isGroupExpanded(item) ? 'is-expanded' : '',
                        isCollapsed ? 'justify-content-center' : '',
                    ]"
                    :aria-expanded="isGroupExpanded(item)"
                    :aria-controls="'sidebar-submenu-' + item.activeKey"
                    @click="toggleGroup(item)"
                >
                    <LucideIcon
                        strokeWidth="2"
                        :icon="item.icon.name"
                        :color="item.icon.color"
                    />
                    <span
                        v-show="!isCollapsed"
                        class="ms-2 flex-grow-1 text-truncate"
                    >
                        {{ $t(item.labelKey) }}
                    </span>
                    <LucideIcon
                        v-show="!isCollapsed"
                        icon="ChevronDown"
                        strokeWidth="2"
                        class="sidebar-group-chevron flex-shrink-0 ms-1"
                        :class="{ 'is-open': isGroupExpanded(item) }"
                    />
                </button>
                <ul
                    v-show="isGroupExpanded(item) && !isCollapsed"
                    :id="'sidebar-submenu-' + item.activeKey"
                    class="list-unstyled mb-0 mt-1 sidebar-group-submenu"
                >
                    <li
                        v-for="(sub, subIndex) in item.visibleGroup"
                        :key="`${sub.activeKey}-${sub.to}-${subIndex}`"
                        class="mb-1"
                    >
                        <router-link
                            class="d-flex align-items-center custom-menu-item link-dark rounded sidebar-group-link"
                            :class="{ active: matchesMenuPath(sub.to) }"
                            :to="sub.to"
                        >
                            <LucideIcon
                                strokeWidth="2"
                                :icon="sub.icon.name"
                                :color="sub.icon.color"
                            />
                            <span class="ms-2">{{ $t(sub.labelKey) }}</span>
                        </router-link>
                    </li>
                </ul>
            </div>
            <router-link
                v-else
                :class="[
                    'd-flex align-items-center custom-menu-item link-dark rounded',
                    isRouteActive(item) ? 'active' : '',
                    isCollapsed ? 'justify-content-center' : '',
                ]"
                :to="item.to"
            >
                <LucideIcon
                    strokeWidth="2"
                    :icon="item.icon.name"
                    :color="item.icon.color"
                />
                <span
                    v-show="!isCollapsed"
                    class="ms-2"
                >
                    {{ $t(item.labelKey) }}
                </span>
            </router-link>
        </li>
    </ul>
</template>
<script>
    export default {
        name: "RouteListComponent",
        props: {
            items: {
                type: Array,
                default: () => [],
            },
            isCollapsed: {
                type: Boolean,
                default: false,
            },
        },
        data() {
            return {
                expandedGroupKey: null,
            };
        },
        methods: {
            toggleGroup(item) {
                const key = item.activeKey;
                if (this.expandedGroupKey === key) {
                    this.expandedGroupKey = null;
                } else {
                    this.expandedGroupKey = key;
                }
            },
            isGroupExpanded(item) {
                return this.expandedGroupKey === item.activeKey;
            },
            matchesMenuPath(to) {
                return this.$route.path === to;
            },
            isGroupActive(item) {
                if (!item.visibleGroup?.length) {
                    return false;
                }
                const routeInGroup = item.visibleGroup.some((sub) => this.matchesMenuPath(sub.to));
                if (!routeInGroup) {
                    return false;
                }
                const submenuVisible = !this.isCollapsed && this.isGroupExpanded(item);
                return !submenuVisible;
            },
            isRouteActive(item) {
                return this.matchesMenuPath(item.to);
            },
        },
    };
</script>
<style scoped>
    .btn-toggle-nav a:hover:not(.active),
    .btn-toggle-nav button.custom-menu-item:not(.sidebar-group-toggle):hover {
        color: var(--color-body-content) !important;
        background-color: var(--color-sidebar-li-collapsed-hover) !important;
        cursor: pointer;
    }

    .btn-toggle-nav button.sidebar-group-toggle:hover {
        color: var(--color-body-content) !important;
        background-color: rgba(13, 110, 253, 0.08) !important;
        cursor: pointer;
    }

    .btn-toggle-nav button.sidebar-group-toggle.is-expanded:not(.active):hover {
        background-color: rgba(13, 110, 253, 0.1) !important;
        color: var(--color-body-content) !important;
        box-shadow: none;
    }

    .btn-toggle-nav button.sidebar-group-toggle.active:hover {
        background-color: rgba(13, 110, 253, 0.16) !important;
        color: #0d6efd !important;
        box-shadow: 0 2px 10px rgba(13, 110, 253, 0.22);
    }

    .btn-toggle-nav a.custom-menu-item.active,
    .sidebar-group-link.active {
        background-color: rgba(13, 110, 253, 0.12) !important;
        color: #0d6efd !important;
        font-weight: 600;
        box-shadow: 0 2px 8px rgba(13, 110, 253, 0.2);
        cursor: default;
    }

    .btn-toggle-nav button.sidebar-group-toggle.is-expanded:not(.active) {
        background-color: rgba(13, 110, 253, 0.06) !important;
        color: #676879 !important;
        font-weight: 400;
        box-shadow: none;
        cursor: pointer;
    }

    .btn-toggle-nav button.sidebar-group-toggle.is-expanded:not(.active) .sidebar-group-chevron {
        color: #676879;
    }

    .btn-toggle-nav button.sidebar-group-toggle.active {
        background-color: rgba(13, 110, 253, 0.12) !important;
        color: #0d6efd !important;
        font-weight: 600;
        box-shadow: 0 2px 8px rgba(13, 110, 253, 0.2);
        cursor: default;
    }

    .btn-toggle-nav button.sidebar-group-toggle.active .sidebar-group-chevron {
        color: #0d6efd;
    }

    .btn-toggle-nav a {
        margin-left: 0 !important;
        color: #676879;
        transition:
            background-color 0.2s ease,
            color 0.2s ease;
    }

    .btn-toggle-nav button.custom-menu-item {
        color: #676879;
        transition:
            background-color 0.2s ease,
            color 0.2s ease;
    }

    .btn-toggle-nav {
        padding: 10px !important;
    }

    .custom-menu-item {
        border-radius: 10px;
        transition: background-color 0.2s ease;
        height: 44px;
        line-height: 1.5;
    }

    .custom-menu-item:hover:not(.active):not(.sidebar-group-toggle) {
        background-color: var(--color-sidebar-li-collapsed-hover) !important;
        text-decoration: none;
    }

    .sidebar-group-chevron {
        transition: transform 0.2s ease;
        color: #676879;
    }

    .sidebar-group-chevron.is-open {
        transform: rotate(-180deg);
    }

    .sidebar-group-submenu {
        padding-left: 0.75rem;
        margin-left: 0.25rem;
        border-left: 2px solid var(--color-border-form-control, #dee2e6);
    }

    .sidebar-group-link {
        margin-left: 0 !important;
        text-decoration: none;
    }

    .sidebar-group-link:hover {
        background-color: var(--color-sidebar-li-collapsed-hover) !important;
        color: var(--color-body-content) !important;
    }

    .btn-toggle-nav a {
        display: inline-flex;
        padding: 0.1875rem 0.5rem;
        margin-top: 0.125rem;
        margin-left: 1.25rem;
        text-decoration: none;
    }

    .btn-toggle-nav button.custom-menu-item {
        display: inline-flex;
        padding: 0.1875rem 0.5rem;
        margin-top: 0.125rem;
        margin-left: 0 !important;
    }

    .btn-toggle-nav a:hover:not(.active),
    .btn-toggle-nav a:focus:not(.active) {
        background-color: var(--color-sidebar-li-collapsed-hover);
    }

    .btn-toggle-nav a.custom-menu-item.active:hover,
    .btn-toggle-nav a.custom-menu-item.active:focus,
    .sidebar-group-link.active:hover,
    .sidebar-group-link.active:focus {
        background-color: rgba(13, 110, 253, 0.14) !important;
        color: #0d6efd !important;
    }

    @media (max-height: 500px) {
        .scroll-area {
            display: list-item;
            overflow-y: auto;
        }
    }

    .custom-menu-item:not(.active):not(.sidebar-group-toggle) {
        opacity: 0.8;
    }

    .sidebar-group-toggle {
        opacity: 1;
    }

    .sidebar-menu-item-enter {
        opacity: 0;
        transform: translateX(-12px);
        animation: sidebar-item-enter 280ms cubic-bezier(0.25, 0.46, 0.45, 0.94) forwards;
        animation-delay: calc(var(--item-index) * 45ms);
    }

    @keyframes sidebar-item-enter {
        from {
            opacity: 0;
            transform: translateX(-12px);
        }
        to {
            opacity: 1;
            transform: translateX(0);
        }
    }
</style>
