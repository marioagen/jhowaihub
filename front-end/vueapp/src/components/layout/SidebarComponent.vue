<template>
    <aside class="sidebar d-flex flex-column flex-shrink-0 background-white text-black">
        <div
            class="sidebar-header d-flex align-items-center"
            :class="isCollapsed ? 'justify-content-center' : 'justify-content-start'"
            style="height: 60px; padding: 0 10px"
        >
            <LogoComponent :collapsed="isCollapsed" />
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
        <RouteListComponent
            :items="visibleMenuItems"
            :isCollapsed="isCollapsed"
        />
    </aside>
</template>
<script>
    import { hasPermission } from "@/utils/permissions";
    import LogoComponent from "@/components/layout/LogoComponent.vue";
    import RouteListComponent from "@/components/layout/RouteListComponent.vue";

    export default {
        name: "SideBar",
        components: {
            LogoComponent,
            RouteListComponent,
        },
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
                        activeKey: "DocumentList",
                        to: "/management",
                        icon: {
                            name: "Users",
                            color: "#ff6900",
                        },
                        labelKey: "pages.management",
                        requiredPermissions: [
                            { permission: "Management", action: "View users" },
                            { permission: "Management", action: "View teams" },
                            { permission: "Management", action: "View profiles" },
                        ],
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
                        activeKey: "Tools",
                        icon: {
                            name: "PocketKnife",
                            color: "#8b5cf6",
                        },
                        labelKey: "pages.tools",
                        group: [
                            {
                                permission: "Tools",
                                action: "View prompts",
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
                                action: "View connectors",
                                activeKey: "Connectors",
                                to: "/tools",
                                icon: {
                                    name: "Plug",
                                    color: "#8b5cf6",
                                },
                                labelKey: "pages.connectors",
                            },
                            {
                                permission: "Tools",
                                action: "View APIs",
                                activeKey: "Templates",
                                to: "/templates",
                                icon: {
                                    name: "Zap",
                                    color: "#8b5cf6",
                                },
                                labelKey: "pages.templates",
                            },
                            {
                                permission: "Tools",
                                action: "View quizzes",
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
            visibleMenuItems() {
                return this.filterByPermission(this.menuItems)
                    .map((item) => {
                        if (!item.group?.length) {
                            return item;
                        }
                        const visibleGroup = this.filterByPermission(item.group);
                        if (!visibleGroup.length) {
                            return null;
                        }
                        return { ...item, visibleGroup };
                    })
                    .filter(Boolean);
            },
        },
        methods: {
            filterByPermission(list) {
                if (!list?.length) {
                    return [];
                }

                console.log(list);
                return list.filter((item) => {
                    if (item.requiredPermissions?.length) {
                        return item.requiredPermissions.some((perm) =>
                            hasPermission(perm.permission, perm.action)
                        );
                    }
                    if (!item.permission) {
                        return true;
                    }
                    return hasPermission(item.permission, item.action ?? "View");
                });
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

    .toggle-button:focus {
        outline: none;
        box-shadow: none;
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

    .collapse-toggle-container:hover {
        background-color: var(--color-sidebar-li-collapsed-hover) !important;
    }
</style>
