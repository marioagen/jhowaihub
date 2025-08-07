<template>
    <aside class="sidebar d-flex flex-column flex-shrink-0 background-white text-black">
        <!-- Logo -->
        <div class="sidebar-header d-flex align-items-center justify-content-start px-3" style="height: 60px">
            <router-link class="d-flex align-items-center text-decoration-none w-100" :to="{ name: 'DocumentList' }">
                <img
                    v-if="!showLogoDarkMode && isCollapsed"
                    src="./../../assets/img/woopiai-hub-small-logo.png"
                    :title="$t('labelGoHome')"
                    width="30"
                    height="30"
                />
                <img
                    v-else-if="!showLogoDarkMode"
                    src="./../../assets/img/woopiai-hub-logo.png"
                    :title="$t('labelGoHome')"
                    width="120"
                    height="40"
                />
                <img
                    v-else-if="isCollapsed"
                    src="./../../assets/img/woopiai-hub-small-logo.png"
                    :title="$t('labelGoHome')"
                    width="30"
                    height="30"
                />
                <img
                    v-else
                    src="./../../assets/img/woopiai-hub-logo.png"
                    :title="$t('labelGoHome')"
                    width="186"
                    height="40"
                />
            </router-link>
        </div>

        <div class="collapse-toggle-container" @click="$emit('toggle-collapse')">
            <button class="btn toggle-button" type="button" aria-label="Toggle sidebar">
                <LucideIcon v-if="isCollapsed" icon="ChevronRight" />
                <LucideIcon v-else icon="ChevronLeft" />
            </button>
        </div>

        <div class="sidebar-horizontal-separator"></div>

        <ul class="btn-toggle-nav list-unstyled fw-normal pb-1 small">
            <li class="mb-1">
                <router-link :class="[
                        'd-flex align-items-center',
                        menuActive == 'DocumentList' ? 'link-dark rounded active' : 'link-dark rounded',
                        isCollapsed ? 'justify-content-center' : '',
                        'custom-menu-item',
                    ]"
                             to="/manage-user">
                    <img src="./../../assets/img/manage-users.svg"
                         :title="$t('labelManageUsers')"
                         width="20"
                         class="icon-sidebar" />
                    <span v-show="!isCollapsed" class="ms-2">{{ $t("labelManageUsers") }}</span>
                </router-link>
            </li>

            <li class="mb-1">
                <router-link :class="[
                        'd-flex align-items-center',
                        menuActive == 'DocumentList' ? 'link-dark rounded active' : 'link-dark rounded',
                        isCollapsed ? 'justify-content-center' : '',
                        'custom-menu-item',
                    ]"
                             to="/document-list">
                    <img src="./../../assets/img/docs-analyze.svg"
                         :title="$t('labelDocuments')"
                         width="20"
                         class="icon-sidebar" />
                    <span v-show="!isCollapsed" class="ms-2">{{ $t("labelDocuments") }}</span>
                </router-link>
            </li>

            <li class="mb-1">
                <router-link :class="[
                        'd-flex align-items-center',
                        menuActive == 'DocumentList' ? 'link-dark rounded active' : 'link-dark rounded',
                        isCollapsed ? 'justify-content-center' : '',
                        'custom-menu-item',
                    ]"
                             to="/workflow-documents">
                    <img src="./../../assets/img/workflow.svg"
                         :title="$t('labelDocuments')"
                         width="20"
                         class="icon-sidebar" />
                    <span v-show="!isCollapsed" class="ms-2">{{ $t("labelWorkflowDocs") }}</span>
                </router-link>
            </li>

            <li class="mb-1">
                <router-link :class="[
                        'd-flex align-items-center',
                        menuActive === 'Type' ? 'link-dark rounded active' : 'link-dark rounded',
                        isCollapsed ? 'justify-content-center' : '',
                        'custom-menu-item',
                    ]"
                             to="/types">
                    <img src="./../../assets/img/type-icon.svg"
                         :title="$t('labelTypes')"
                         width="20"
                         class="icon-sidebar" />
                    <span v-show="!isCollapsed" class="ms-2">
                        {{ $t("labelTypes") }}
                    </span>
                </router-link>
            </li>

            <li>
                <router-link :class="[
                        'd-flex align-items-center',
                        menuActive === 'Type' ? 'link-dark rounded active' : 'link-dark rounded',
                        isCollapsed ? 'justify-content-center' : '',
                        'custom-menu-item',
                    ]"
                             to="/questions">
                    <img src="../../assets/img/question-icon.svg"
                         :title="$t('labelQuestions')"
                         width="20"
                         class="icon-sidebar" />
                    <span v-show="!isCollapsed" class="ms-2">{{ $t("labelQuestions") }}</span>
                </router-link>
            </li>

            <li>
                <router-link :class="[
                        'd-flex align-items-center',
                        menuActive === 'Type' ? 'link-dark rounded active' : 'link-dark rounded',
                        isCollapsed ? 'justify-content-center' : '',
                        'custom-menu-item',
                    ]"
                             to="/quizzes">
                    <img src="./../../assets/img/questionnaires-icon.svg"
                         :title="$t('quizzes.title')"
                         width="20"
                         class="icon-sidebar" />
                    <span v-show="!isCollapsed" class="ms-2">{{ $t("quizzes.title") }}</span>
                </router-link>
            </li>
        </ul>
    </aside>
</template>

<script>
    export default {
        name: "SideBar",
        props: {
            menuActive: {
                required: true,
                type: String,
                default: "",
            },
            theme: {
                required: true,
                type: Boolean,
                default: false,
            },
            isCollapsed: {
                type: Boolean,
                default: false,
            },
            menuActive: {
                type: String,
                default: "",
            },
        },
        data() {
            return {
                title: "Component SideBarTest",
                showLogoDarkMode: this.theme,
            };
        },
        updated() {
            let self = this;
            (function () {
                if (localStorage.getItem("theme") === "css-theme-dark") {
                    self.showLogoDarkMode = true;
                } else {
                    self.showLogoDarkMode = false;
                }
            })();
        },
    };
</script>

<style scoped>
    /* Hover do botão colapsar */
    .collapse-toggle-container:hover,
    .collapse-toggle-container .btn.toggle-button:hover {
        background-color: #e1e9f8 !important;
        border-color: #e1e9f8 !important;
        cursor: pointer;
    }

    /* Hover: fundo azul clarinho e texto mantendo cor padrão */
    .btn-toggle-nav a:hover {
        color: #676879 !important;
        background-color: #e1e9f8 !important;
        cursor: pointer;
    }

    /* Ativo: fundo azul clarinho e texto azul */
    .btn-toggle-nav a.active {
        background-color: #e1e9f8 !important;
        color: #007bff !important;
        /* azul padrão do bootstrap, pode trocar */
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
        /* mantém lateral apertadinha */
        padding-right: 4px;
        background: transparent;
        border: none;
        font-size: 12px;
        /* se quiser manter a seta mais fina */
        color: #737477;
        width: 28px;
        /* ajuste se quiser */
        height: 40px;
        /* deixa a altura ser definida pelo padding */
    }

    .sidebar-horizontal-separator {
        width: 100%;
        height: 1px;
        background-color: #d0d4d9;
        margin: 0;
        /* remove espaçamentos extras */
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
</style>
