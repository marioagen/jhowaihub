<template>
    <aside class="sidebar d-flex flex-column flex-shrink-0 background-white text-black">
        <div class="sidebar-header d-flex align-items-center" :class="isCollapsed ? 'justify-content-center' : 'justify-content-start'" style="height: 60px; padding: 0 10px;">
            <router-link class="d-flex align-items-center text-decoration-none" :class="isCollapsed ? 'justify-content-center' : 'w-100'" :to="{ name: 'Home' }">
                <img
                    v-if="isCollapsed"
                    src="./../../assets/img/woopiai-hub-small-logo.png"
                    :title="$t('labelGoHome')"
                    width="30"
                    height="30"
                />
                <img
                    v-else
                    src="./../../assets/img/woopi-ai-text-logo.svg"
                    :title="$t('labelGoHome')"
                    width="140"
                    height="40"
                    alt="WOOPI AI"
                />
            </router-link>
        </div>        
        <div class="horizontal-separator-fixed"></div>
        <div class="collapse-toggle-container" @click="$emit('toggle-collapse')">
            <button class="btn toggle-button" type="button" aria-label="Toggle sidebar">
                <LucideIcon v-if="isCollapsed" icon="ChevronRight" />
                <LucideIcon v-else icon="ChevronLeft" />
            </button>
        </div>
        <div class="sidebar-horizontal-separator"></div>
        <ul class="btn-toggle-nav list-unstyled fw-normal pb-1 small">
            <li
                v-for="item in menuItems"
                :key="item.labelKey"
                class="mb-1"
                :class="{ 'is-active': isRouteActive(item) }"
            >
                <router-link
                    :class="[
                        'd-flex align-items-center custom-menu-item link-dark rounded',
                        isRouteActive(item) ? 'active' : '',
                        isCollapsed ? 'justify-content-center' : ''
                    ]"
                    :to="item.to"
                >
                    <LucideIcon
                        strokeWidth="2"
                        :icon="item.icon.name"
                        :color="item.icon.color"
                    />
                    <span v-show="!isCollapsed" class="ms-2">{{ $t(item.labelKey) }}</span>
                </router-link>
            </li>
        </ul>
    </aside>
</template>

<script>
    import { hasPermission } from '@/utils/permissions';
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
                permissions: {
                    management: hasPermission("Management", "View"),
                    questions: hasPermission("Questions", "View"),
                    documents: hasPermission("Documents","View"),
                    types: hasPermission("Types", "View"),
                    quizzes: hasPermission("Quizzes", "View"),
                    workflow: hasPermission("Workflow", "View"),
                },
                menuItems: [
                    {
                        activeKey: 'Home',
                        to: '/home',
                        icon: { name: 'Home', color: '#0d6efd' },
                        labelKey: 'labelGoHome'
                    },
                    {
                        permission: 'Management',
                        activeKey: 'DocumentList',
                        to: '/management',
                        icon: { name: 'Users', color: '#ff6900' },
                        labelKey: 'labelManageUsers'
                    },
                    {
                        activeKey: 'DocumentList',
                        to: '/documents',
                        icon: { 
                            name: 'FileText', 
                            color: '#2f80ed' 
                        },
                        labelKey: 'labelDocuments'
                    },
                    {
                        permission: 'Workflow',
                        activeKey: 'Workflow',
                        to: '/workflow',
                        icon: { 
                            name: 'Workflow', 
                            color: '#615FFF' 
                        },
                        labelKey: 'workflow.sidebar.index'
                    },
                    {
                        permission: 'Workflow',
                        activeKey: 'WorkflowManagement',
                        to: '/workflow/management',
                        icon: { 
                            name: 'Workflow', 
                            color: '#00bba7' 
                        },
                        labelKey: 'workflow.sidebar.management'
                    },
                    {
                        permission: 'Types',
                        activeKey: 'Type',
                        to: '/types',
                        icon: { 
                            name: 'BookmarkCheck', 
                            color: '#f2c94c' 
                        },
                        labelKey: 'labelTypes'
                    },
                    {
                        permission: 'Questions',
                        activeKey: 'Questions',
                        to: '/questions',
                        icon: { 
                            name: 'MessageSquare', 
                            color: '#3fd67b' 
                        },
                        labelKey: 'labelQuestions'
                    },
                    {
                        permission: 'Quizzes',
                        activeKey: 'Quizzes',
                        to: '/quizzes',
                        icon: { 
                            name: 'ClipboardList', 
                            color: '#a259ff' 
                        },
                        labelKey: 'quizzes.title'
                    },
                    {
                        permission: 'Tools',
                        activeKey: 'Tools',
                        to: '/tools',
                        icon: { 
                            name: 'PocketKnife', 
                            color: '#f59e0b'
                        },
                        labelKey: 'tools.title'
                    },
                    {
                        permission: 'Prompts',
                        activeKey: 'Prompts',
                        to: '/prompts',
                        icon: {
                            name: 'Braces',
                            color: '#8e51ff'
                        },
                        labelKey: 'prompts.title'
                    },
                ]
            };
        },
        methods: {
            isRouteActive(item) {
                return this.$route.path === item.to;
            }
        },
    };
</script>

<style scoped>
    .collapse-toggle-container:hover,
    .collapse-toggle-container .btn.toggle-button:hover {
        background-color: #e1e9f8 !important;
        border-color: #e1e9f8 !important;
        cursor: pointer;
    }

    .btn-toggle-nav a:hover {
        color: #676879 !important;
        background-color: #e1e9f8 !important;
        cursor: pointer;
    }

    .btn-toggle-nav a.active {
        background-color: #e1e9f8 !important;
        color: #007bff !important;
        font-weight: 600;
        cursor: default;
    }

    .btn-toggle-nav a {
        margin-left: 0 !important;
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

    .custom-menu-item:hover {
        background-color: #f0f2f5;
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
        background-color: #d0d4d9;
        margin: 0;
        padding: 0;
    }

    .offcanvas-start {
        width: initial;
    }

    .offcanvas-header {
        padding: 0;
    }

    .offcanvas-header .btn-close {
        padding: 0.5rem 1rem;
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

    .btn-toggle-nav a:hover,
    .btn-toggle-nav a:focus {
        background-color: #d2f4ea;
    }

    .btn-toggle-nav > li > .active {
        background-color: #d2f4ea;
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

    .custom-menu-item.active {
        background-color: var(--bs-primary-bg-subtle);
        font-weight: 600;
    }

    .custom-menu-item:not(.active) {
        opacity: 0.8;
    }
</style>
