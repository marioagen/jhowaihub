<template>
    <nav class="navbar navbar-expand-lg navbar-light">
        <div
            class="collapse navbar-collapse"
            id="navbarSupportedContent"
        >
            <div class="navbar-main-area d-flex align-items-center flex-grow-1 ps-4">
                <span class="badge bg-light text-dark border">{{ this.selectedTenant }}</span>
                <div class="navbar-right-group d-flex align-items-center gap-1 pe-2 ms-auto">
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
                                    <span
                                        class="notification-file-name text-truncate flex-grow-1 min-width-0"
                                    >
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

                    <div class="dropdown nav-buttons">
                        <button
                            class="btn btn-outline-primary table-btn btn-sm"
                            type="button"
                            data-bs-toggle="dropdown"
                            aria-expanded="false"
                            style="display: flex; align-items: center; justify-content: center"
                        >
                            <LucideIcon icon="Globe" />
                        </button>
                        <ul class="dropdown-menu dropdown-menu-button text-small shadow">
                            <li>
                                <a
                                    :class="
                                        $i18n.locale === 'pt'
                                            ? 'btn btn-light lang-link lang-active'
                                            : 'btn btn-light lang-link btn-lang'
                                    "
                                    @click="setLanguage('pt')"
                                >
                                    <img
                                        src="./../../assets/img/lang-pt.png"
                                        alt="PT"
                                    />
                                    <span style="margin-left: 2px">PT</span>
                                </a>
                            </li>
                            <li>
                                <a
                                    :class="
                                        $i18n.locale === 'en'
                                            ? 'btn btn-light lang-link lang-active btn-lang'
                                            : 'btn btn-light lang-link btn-lang'
                                    "
                                    @click="setLanguage('en')"
                                >
                                    <img
                                        src="./../../assets/img/lang-en.png"
                                        alt="EN"
                                    />
                                    <span style="margin-left: 2px">EN</span>
                                </a>
                            </li>
                            <li>
                                <a
                                    :class="
                                        $i18n.locale === 'es'
                                            ? 'btn btn-light lang-link lang-active btn-lang'
                                            : 'btn btn-light lang-link btn-lang'
                                    "
                                    @click="setLanguage('es')"
                                >
                                    <img
                                        src="./../../assets/img/lang-es.png"
                                        alt="ES"
                                    />
                                    <span style="margin-left: 2px">ES</span>
                                </a>
                            </li>
                        </ul>
                    </div>

                    <div
                        class="dropdown-menu-user"
                        style="cursor: pointer"
                    >
                        <div class="dropdown">
                            <a
                                class="d-flex align-items-center text-black text-decoration-none dropdown-toggle username"
                                id="dropdownUser1"
                                data-bs-toggle="dropdown"
                                aria-expanded="false"
                            >
                                <img
                                    :src="profileImage"
                                    alt="Imagem do perfil"
                                    width="32"
                                    height="32"
                                    class="rounded-circle me-2"
                                    v-if="profileImage !== ''"
                                />
                                {{ setBreakWord(user) }}
                            </a>
                            <a
                                class="d-flex align-items-center text-black text-decoration-none dropdown-toggle username-collapsed"
                                id="dropdownUser1"
                                data-bs-toggle="dropdown"
                                aria-expanded="false"
                            >
                                <img
                                    :src="profileImage"
                                    alt="Imagem do perfil"
                                    width="32"
                                    height="32"
                                    class="rounded-circle me-2"
                                    v-if="profileImage !== ''"
                                />
                            </a>
                            <ul
                                class="dropdown-menu dropdown-menu-sidebar text-small shadow menu-right"
                                aria-labelledby="dropdownUser1"
                                id="dropdown-menu-button"
                            >
                                <li class="remove-hover mt-2">
                                    <router-link
                                        class="dropdown-item px-2 my-2"
                                        :to="{
                                            name: 'Logout',
                                        }"
                                        title="Sair"
                                    >
                                        <LucideIcon icon="LogOut" />
                                        {{ $t("common.signOut") }}
                                    </router-link>
                                </li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </nav>
</template>
<script>
    import axios from "axios";
    import api from "@/services/api";
    import router from "@/router";
    import AvatarComponent from "@/components/global/AvatarComponent.vue";

    export default {
        name: "NavBarComponent",
        props: {
            sidebarData: {
                required: false,
                type: String,
                default: "",
            },
        },
        components: {
            AvatarComponent,
        },
        data() {
            return {
                title: "Component NavBar",
                profileImage: "",
                user: this.$store.state.userProfile.name,
                selectedTenant: null,
                tenantsFromState: [],
            };
        },
        methods: {
            handleTenantChange(event) {
                let self = this;

                self.selectedTenant = event.target.value;
                self.$store.commit("updateUserProfileTenant", {
                    amount: self.selectedTenant,
                });

                self.InitializeTenant(self.selectedTenant)
                    .then(() => {
                        window.location.href = "/";
                    })
                    .catch((error) => {
                        console.log("Erro ao inicializar o tenant:", error);
                    });
            },
            InitializeTenant(tenant) {
                let self = this;

                return api.get("/Tenant/InitializeTenant/" + tenant);
            },
            getUserTenants(userEmail, savedTenant) {
                api.get("/Tenant/FindAllByUserEmail/" + userEmail)
                    .then((result) => {
                        if (JSON.stringify(result.data) !== JSON.stringify(this.tenantsFromState)) {
                            this.tenantsFromState = result.data;
                        }
                    })
                    .catch((e) => {
                        console.log(e);
                    });

                this.selectedTenant = savedTenant;
                if (!this.tenantInitialized) {
                    this.InitializeTenant(this.selectedTenant);
                    this.$store.commit("setTenantInitialized", true);
                }
            },
            setLanguage(lang) {
                this.$i18n.locale = lang;
                this.$store.commit("updateUserProfileLanguage", { amount: lang });
            },
            getProfileImage() {
                axios
                    .get("https://graph.microsoft.com/v1.0/me/photos/48x48/$value", {
                        headers: {
                            Authorization: `Bearer ${this.$store.state.userProfile.tokenAzure}`,
                        },
                        responseType: "blob",
                    })
                    .then((response) => {
                        this.profileImage = window.URL.createObjectURL(
                            new Blob([response.data], { type: "image/jpeg" })
                        );
                        this.$store.commit("updateUserProfileImage", {
                            amount: self.profileImage,
                        });
                    });
            },
            setBreakWord(str) {
                const strSplit = str.trim().split(" ");
                if (strSplit.length === 1) return strSplit[0];
                return `${strSplit[0]} ${strSplit[strSplit.length - 1]}`;
            },
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
            tenantInitialized() {
                return this.$store.state.tenantInitialized;
            },
            initials() {
                const tenantChunk = this.selectedTenant.split("@")[0];

                const splits = tenantChunk.split(/[.\-_ ]+/).filter(Boolean);

                if (splits.length === 1) {
                    return splits[0][0]?.toUpperCase() || "";
                }

                const first = splits[0]?.[0] || "";
                const last = splits[splits.length - 1]?.[0] || "";

                return (first + last).toUpperCase();
            },
        },
        created() {
            const userEmail = this.$store.state.userProfile.login;
            const savedTenant = this.$store.state.userProfile.tenant;
            if (userEmail === "" || savedTenant === "") {
                router.push({ name: "Logout" });
            }

            if (this.$store.state.userProfile.tokenAzure != "") {
                this.getProfileImage();
            }

            this.getUserTenants(userEmail, savedTenant);
        },
        mounted() {
            document.documentElement.className = "css-theme-light";
        },
    };
</script>
<style scoped>
    .navbar {
        padding: 1;
        padding-top: 0.9rem;
        padding-bottom: 0.8rem;
    }

    .navbar-light {
        background-color: #ffffff;
    }

    .navbar-toggler,
    .navbar-toggler-icon {
        display: none;
    }

    .navbar-expand-lg {
        flex-wrap: nowrap !important;
        justify-content: flex-start !important;
    }

    .navbar-expand-lg .navbar-collapse {
        display: flex !important;
        flex-basis: auto !important;
    }

    .collapse {
        justify-content: space-between;
    }

    .dropdown-menu-user {
        padding: 0px 10px;
    }

    .dropdown-toggle {
        outline: 0;
    }

    .dropdown-item {
        padding: 0rem 0.2rem !important;
    }

    .remove-hover .dropdown-item,
    .remove-hover a:hover {
        color: #212529;
        background-color: #ffffff;
    }

    .form-switch {
        padding-left: 2.7em !important;
    }

    .bd-circle {
        border: 1px solid #c7c8c9 !important;
    }

    .btn.btn-light.lang-link,
    .btn.btn-light.lang-link:hover,
    .btn.btn-light.lang-link.lang-active,
    .btn.btn-light.lang-link.lang-active:hover {
        padding: 0.1rem 0.3rem;
        background-color: transparent;
        border: none;
    }

    .lang-link {
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .lang-link.lang-active,
    .lang-link:hover {
        color: #0d6efd !important;
    }

    .btn-lang {
        padding: 2%;
        background-color: white;
        border-color: white;
    }

    .text-black {
        color: black;
    }

    #tenantDropdownButton {
        width: 100%;
        /* Ajusta o botão para ser responsivo */
        font-size: 1rem;
        /* Ajusta o tamanho da fonte */
    }

    .circle-icon {
        width: 32px;
        height: 32px;
        background-color: var(--color-bg-btn-primary) !important;
        /* Cor de fundo padrão */
        text-align: center;
        color: white;
        border-radius: 50%;
        padding-top: 7%;
    }

    #dropdown-menu-button {
        margin-top: 1rem !important;
    }

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

    @media (max-width: 309px) {
        .logo {
            width: 31px;
            height: 40px;
            overflow: hidden;
        }
    }

    @media (max-width: 768px) {
        .username-collapsed {
            display: block !important;
        }

        .username {
            display: none !important;
        }

        .dropdown-menu {
            right: 0 !important;
            left: auto !important;
        }

        #tenantDropdownButton {
            font-size: 0.9rem;
            /* Reduz o tamanho da fonte em telas menores */
            padding: 0.5rem;
            /* Ajusta o espaçamento interno */
        }

        #tenantDropdownMenu {
            width: 100%;
            /* Garante que o menu dropdown seja responsivo */
            font-size: 0.85rem;
            /* Ajusta o tamanho da fonte */
        }

        #tenantDropdownLabel {
            font-size: 0.9rem;
            /* Ajusta o tamanho da fonte no label */
        }

        .circle-icon {
            padding-top: 11%;
        }
    }

    #tenantDropdownMenu {
        border: 1px solid var(--border-color);
        border-radius: 0.25rem;
        padding: 0.5rem;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        overflow-y: auto;
    }

    #tenantDropdownMenu li {
        display: flex;
        align-items: center;
        justify-content: flex-start;
        white-space: normal;
        /* Permite quebra de linhas */
        padding: 0.25rem 0.5rem;
        /* Ajusta o padding dos itens */
    }

    .vertical-line {
        width: 1px;
        height: 40px;
        background-color: #ccc;
    }

    @media (min-width: 769px) {
        .username-collapsed {
            display: none !important;
        }

        .username {
            display: block !important;
        }
    }

    @media (max-width: 576px) {
        #tenantDropdownButton {
            font-size: 0.8rem;
            padding: 0.4rem;
        }

        #tenantDropdownMenu {
            max-width: 100%;
            font-size: 0.8rem;
        }

        #tenantDropdownMenu li {
            padding: 0.2rem 0.4rem;
        }

        #tenantDropdownLabel {
            font-size: 0.8rem;
        }
    }

    .menu-right {
        right: 0 !important;
        left: auto !important;
    }
</style>
