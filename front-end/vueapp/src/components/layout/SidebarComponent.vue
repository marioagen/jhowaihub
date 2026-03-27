<template>
    <aside class="sidebar d-flex flex-column flex-shrink-0 background-white text-black">
        <div
            class="sidebar-header d-flex align-items-center"
            :class="isCollapsed ? 'justify-content-center' : 'justify-content-start'"
            style="height: 60px; padding: 0 10px"
        >
            <router-link
                class="d-flex align-items-center text-decoration-none"
                :class="isCollapsed ? 'justify-content-center' : 'w-100'"
                :to="{ name: 'Home' }"
            >
                <img
                    v-if="isCollapsed"
                    :src="logoSmallSrc"
                    :title="$t('common.home')"
                    width="35"
                    height="35"
                />
                <img
                    v-else
                    :src="logoSrc"
                    :title="$t('common.home')"
                    height="35"
                    alt="WOOPI AI"
                    style="margin-left: 0px"
                />
            </router-link>
        </div>
        <div class="horizontal-separator-fixed"></div>
        <div
            class="collapse-toggle-container"
            @click="$emit('toggle-collapse')"
        >
            <button
                class="btn toggle-button"
                type="button"
                aria-label="Toggle sidebar"
            >
                <LucideIcon
                    v-if="isCollapsed"
                    icon="ChevronRight"
                />
                <LucideIcon
                    v-else
                    icon="ChevronLeft"
                />
            </button>
        </div>
        <div class="sidebar-horizontal-separator"></div>
        <ul class="btn-toggle-nav list-unstyled fw-normal pb-1 small">
            <li
                v-for="(item, index) in filteredMenuItems"
                :key="item.activeKey ? `${item.activeKey}-${index}` : `nav-${index}`"
                class="mb-1 sidebar-menu-item-enter"
                :class="{
                    'is-active': isRouteActive(item),
                }"
                :style="{ '--item-index': index }"
            >
                <div v-if="item.group && item.group.length">
                    <button
                        type="button"
                        class="d-flex align-items-center custom-menu-item sidebar-group-toggle link-dark rounded border-0 bg-transparent w-100 text-start"
                        :class="[
                            isRouteActive(item) ? 'active' : '',
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
                            v-for="(sub, subIndex) in filterByPermission(item.group)"
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
    </aside>
</template>
<script>
    import { hasPermission } from "@/utils/permissions";
    import logoDark from "@/assets/img/woopiai-logo-dark.png";
    import logoLight from "@/assets/img/woopiai-logo-light.png";
    import logoSmall from "@/assets/img/woopiai-hub-small-logo.png";
    export default {
        name: "SideBar",
        props: {
            menuActive: {
                required: true,
                type: String,
                default: "",
            },
            isCollapsed: {
                type: Boolean,
                default: false,
            },
        },
        data() {
            return {
                title: "SideBarComponent",
                expandedGroupKey: null,
                menuItems: [
                    {
                        activeKey: "Home",
                        to: "/home",
                        icon: {
                            name: "Home",
                            color: "#0d6efd",
                        },
                        labelKey: "common.home",
                    },
                    {
                        permission: "Management",
                        activeKey: "DocumentList",
                        to: "/management",
                        icon: {
                            name: "Users",
                            color: "#ff6900",
                        },
                        labelKey: "pages.management",
                    },
                    {
                        permission: "Workflow",
                        activeKey: "Workflow",
                        to: "/workflow",
                        icon: {
                            name: "Kanban",
                            color: "#615FFF",
                        },
                        labelKey: "pages.workflows",
                    },
                    {
                        permission: "WorkflowManagement",
                        activeKey: "WorkflowManagement",
                        to: "/workflow/management",
                        icon: {
                            name: "Workflow",
                            color: "#06b6d4",
                        },
                        labelKey: "pages.workflowManagement",
                    },
                    {
                        permission: "Tools",
                        icon: {
                            name: "PocketKnife",
                            color: "#8b5cf6",
                        },
                        labelKey: "pages.tools",
                        group: [
                            {
                                permission: "Prompts",
                                activeKey: "Prompts",
                                to: "/prompts",
                                icon: {
                                    name: "Bot",
                                    color: "#8b5cf6",
                                },
                                labelKey: "pages.prompts",
                            },
                            {
                                permission: "Tools",
                                activeKey: "Connectors",
                                to: "/tools",
                                icon: {
                                    name: "Plug",
                                    color: "#8b5cf6",
                                },
                                labelKey: "pages.connectors",
                            },
                            {
                                permission: "Templates",
                                activeKey: "Templates",
                                to: "/templates",
                                icon: {
                                    name: "Zap",
                                    color: "#8b5cf6",
                                },
                                labelKey: "pages.templates",
                            },
                            {
                                permission: "Quizzes",
                                activeKey: "ManagementQuizzes",
                                to: "/management-quizzes",
                                icon: {
                                    name: "ClipboardList",
                                    color: "#8b5cf6",
                                },
                                labelKey: "pages.quizzes",
                            },
                        ],
                    },
                    {
                        permission: "Dashboard",
                        activeKey: "Dashboard",
                        to: "/dashboard",
                        icon: {
                            name: "ChartColumn",
                            color: "#40b04d",
                        },
                        labelKey: "pages.dashboard",
                    },
                    {
                        permission: "Auditor",
                        activeKey: "Auditor",
                        to: "/auditor",
                        icon: {
                            name: "ShieldUser",
                            color: "#f56565",
                        },
                        labelKey: "pages.auditor",
                    },
                ],
            };
        },
        computed: {
            isDarkMode() {
                const theme =
                    this.$store.state.theme || localStorage.getItem("theme") || "css-theme-light";
                return theme === "css-theme-dark";
            },
            logoSrc() {
                return this.isDarkMode ? logoLight : logoDark;
            },
            logoSmallSrc() {
                return logoSmall;
            },
            filteredMenuItems() {
                return this.filterByPermission(this.menuItems);
            },
        },
        methods: {
            filterByPermission(list) {
                if (!list?.length) {
                    return [];
                }
                return list.filter((item) => {
                    if (!item.permission) {
                        return true;
                    }
                    return hasPermission(item.permission, "View");
                });
            },
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
            isRouteActive(item) {
                if (item.group?.length) {
                    return this.filterByPermission(item.group).some((sub) =>
                        this.matchesMenuPath(sub.to)
                    );
                }
                return this.matchesMenuPath(item.to);
            },
        },
    };
</script>
<style scoped>
    .collapse-toggle-container:hover,
    .collapse-toggle-container .btn.toggle-button:hover {
        background-color: var(--color-sidebar-li-collapsed-hover) !important;
        border-color: var(--color-sidebar-li-collapsed-hover) !important;
        cursor: pointer;
    }

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

    .btn-toggle-nav button.sidebar-group-toggle.is-expanded:hover,
    .btn-toggle-nav button.sidebar-group-toggle.active:hover {
        background-color: rgba(13, 110, 253, 0.16) !important;
        color: #0d6efd !important;
        box-shadow: 0 2px 10px rgba(13, 110, 253, 0.22);
    }

    .btn-toggle-nav a.custom-menu-item.active,
    .sidebar-group-link.active,
    .btn-toggle-nav button.sidebar-group-toggle.is-expanded,
    .btn-toggle-nav button.sidebar-group-toggle.active {
        background-color: rgba(13, 110, 253, 0.12) !important;
        color: #0d6efd !important;
        font-weight: 600;
        box-shadow: 0 2px 8px rgba(13, 110, 253, 0.2);
        cursor: default;
    }

    .btn-toggle-nav button.sidebar-group-toggle.is-expanded .sidebar-group-chevron,
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

    .toggle-button:focus {
        outline: none;
        box-shadow: none;
    }

    .custom-menu-item {
        border-radius: 10px;
        transition: background-color 0.2s ease;
        height: 44px;
        /* ou 48px se quiser mais */
        line-height: 1.5;
    }

    .custom-menu-item:hover:not(.active):not(.sidebar-group-toggle) {
        background-color: var(--color-sidebar-li-collapsed-hover) !important;
        text-decoration: none;
    }

    .collapse-toggle-container {
        display: flex;
        flex-direction: column;
        align-items: center;
        margin: 8px 0;
        width: 100%;
    }

    .toggle-button {
        padding-top: 10px;
        padding-bottom: 10px;
        padding-left: 4px;
        padding-right: 4px;
        background: transparent;
        border: none;
        font-size: 12px;
        color: #737477;
        width: 28px;
        height: 40px;
    }

    .sidebar-horizontal-separator {
        width: 100%;
        height: 1px;
        background-color: var(--color-border-form-control) !important;
        margin: 0;
        padding: 0;
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

    .btn-toggle {
        display: inline-flex;
        align-items: center;
        padding: 0.25rem 0.5rem;
        font-weight: 600;
        color: rgba(0, 0, 0, 0.65);
        background-color: transparent;
        border: 0;
    }

    .btn-toggle:hover,
    .btn-toggle:focus {
        color: rgba(0, 0, 0, 0.85);
        background-color: #d2f4ea;
    }

    .btn-toggle:active {
        background-color: transparent;
    }

    .btn-toggle::before {
        width: 1.25em;
        line-height: 0;
        content: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 16 16'%3e%3cpath fill='none' stroke='rgba%280,0,0,.5%29' stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M5 14l6-6-6-6'/%3e%3c/svg%3e");
        transition: transform 0.35s ease;
        transform-origin: 0.5em 50%;
    }

    .btn-toggle[aria-expanded="true"] {
        color: rgba(0, 0, 0, 0.85);
    }

    .btn-toggle[aria-expanded="true"]::before {
        transform: rotate(90deg);
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

    .text-black {
        color: black;
    }

    .icon-black {
        color: black;
    }

    .icon-sidebar {
        margin-right: 5px;
    }

    .title {
        color: black;
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

    .collapse-toggle-container:hover {
        background-color: var(--color-sidebar-li-collapsed-hover) !important;
    }
</style>
