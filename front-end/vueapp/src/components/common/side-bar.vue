<template>
    <!-- Div sidebar -->
    <div id="offcanvas-sidebar" class="d-flex flex-column flex-shrink-0 p-3 background-white text-black offcanvas offcanvas-start">
        <div class="offcanvas-header">
            <div class="offcanvas-title" id="offcanvasExampleLabel">
                <router-link class="d-flex align-items-center link-dark text-decoration-none" :to="{ name: 'DocumentList' }">
                    <span class="fs-5">
                        <img src="./../../assets/img/logo-login.png" v-if="!showLogoDarkMode" :title="$t('labelGoHome')"/>
                        <img src="./../../assets/img/logo-home.png" style="padding: 10px 6px;" width="186"  v-else :title="$t('labelGoHome')" />
                    </span>
                </router-link>
            </div>
            <button type="button" class="btn-close text-reset" :title="$t('labelCloseSidebar')" data-bs-dismiss="offcanvas" aria-label="Close"></button>
        </div>
        <ul class="list-unstyled ps-0 nav nav-pills flex-column mb-auto scroll-area">
            <li class="mb-1">
                <div class="btn btn-toggle align-items-center rounded collapsed" data-bs-toggle="collapse" href="#documentos-collapse" aria-expanded="true">
                    <img src="./../../assets/img/docs-icon.svg" :title="$t('labelDocuments')" width="20" class="icon-sidebar" />
                    <span>{{ $t('labelDocuments') }}</span>
                </div>
                <div class="collapse" id="documentos-collapse">
                    <ul class="btn-toggle-nav list-unstyled fw-normal pb-1 small">
                        <li>
                            <router-link :class="menuActive == 'DocumentUpload' ? 'link-dark rounded active' : 'link-dark rounded'"
                                         to="/document-upload">
                                <img src="./../../assets/img/doc-upload-icon.svg" :title="$t('labelUpload')" width="20" class="icon-sidebar" />
                                {{ $t('labelUpload') }}
                            </router-link>
                        </li>
                        <li>
                            <router-link :class="menuActive == 'DocumentList' ? 'link-dark rounded active' : 'link-dark rounded'"
                                         to="/document-list">
                                <img src="./../../assets/img/list-icon.svg" :title="$t('labelListing')" width="20" class="icon-sidebar" />
                                {{ $t('labelListing') }}
                            </router-link>
                        </li>
                    </ul>
                </div>
            </li>
            <li class="mb-1">
                <div class="btn btn-toggle align-items-center rounded collapsed" data-bs-toggle="collapse" href="#manager-collapse" aria-expanded="true">
                    <img src="./../../assets/img/manage-icon.svg" :title="$t('labelManage')" width="20" class="icon-sidebar"/>
                    <span>{{ $t('labelManage') }}</span>
                </div>
                <div class="collapse" id="manager-collapse">
                    <ul class="btn-toggle-nav list-unstyled fw-normal pb-1 small">
                        <li>
                            <router-link :class="menuActive == 'Type' ? 'link-dark rounded active' : 'link-dark rounded'"
                                         to="/manage-type">
                                 <img src="./../../assets/img/types-icon.svg" :title="$t('labelTypes')" width="20" class="icon-sidebar"/>
                                 {{ $t('labelTypes') }}
                            </router-link>
                        </li>
                        <li>
                            <router-link :class="menuActive == 'Question' ? 'link-dark rounded active' : 'link-dark rounded'"
                                         to="/manage-question">
                                <img src="./../../assets/img/question-icon.svg" :title="$t('labelQuestions')" width="20" class="icon-sidebar" />
                                {{ $t('labelQuestions') }}
                            </router-link>
                        </li>
                        <li>
                            <router-link :class="menuActive == 'Quiz' || menuActive == 'QuizNew' || menuActive == 'QuizEdit' ? 'link-dark rounded active' : 'link-dark rounded'"
                                         to="/manage-quiz">
                                <img src="./../../assets/img/questionary-icon.svg" :title="$t('labelQuestionnaires')" width="20" class="icon-sidebar" />
                                {{ $t('labelQuestionnaires') }}
                            </router-link>
                        </li>
                    </ul>
                </div>
            </li>
        </ul>
    </div>
    <!-- Div toggle sidebar -->
    <div class="p-3 bt-toggle-sidebar" href="#" data-bs-toggle="offcanvas" data-bs-target="#offcanvas-sidebar">
        <a href="#" class="icon-black" title="Exibir sidebar" data-bs-toggle="offcanvas" data-bs-target="#offcanvas-sidebar">
            <i class="fas fa-bars"></i>
        </a>
    </div>
</template>

<script>
    export default {
        name: "SideBar",
        props: {
            menuActive: {
                required: true,
                type: String,
                default: ""
            },
            theme:{
                required: true,
                type: Boolean,
                default: false,
            },
        },
        data() {
            return {
                title: "Component SideBarTest",
                showLogoDarkMode: this.theme,
            }
        },
        components: {},
        watch: {},
        methods: {},
        computed: {},
        created() {},
        updated() {
            let self = this;
            (function () {
                if (localStorage.getItem('theme') === 'css-theme-dark') {
                    self.showLogoDarkMode = true;
                } else {
                    self.showLogoDarkMode = false;
                }
            })();
        },
        mounted() {},
        unmounted() {},
    }
</script>

<style scoped>
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
        padding: .25rem .5rem;
        font-weight: 600;
        color: rgba(0, 0, 0, .65);
        background-color: transparent;
        border: 0;
    }

    .btn-toggle:hover,
    .btn-toggle:focus {
        color: rgba(0, 0, 0, .85);
        background-color: #d2f4ea;
    }

    .btn-toggle:active {
        background-color: transparent;
    }

    .btn-toggle::before {
        width: 1.25em;
        line-height: 0;
        content: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 16 16'%3e%3cpath fill='none' stroke='rgba%280,0,0,.5%29' stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M5 14l6-6-6-6'/%3e%3c/svg%3e");
        transition: transform .35s ease;
        transform-origin: .5em 50%;
    }

    .btn-toggle[aria-expanded="true"] {
        color: rgba(0, 0, 0, .85);
    }
    
    .btn-toggle[aria-expanded="true"]::before {
        transform: rotate(90deg);
    }

    .btn-toggle-nav a {
        display: inline-flex;
        padding: .1875rem .5rem;
        margin-top: .125rem;
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
        margin-right: 5px
    }

    .title {
        color: black;
    }
</style>
