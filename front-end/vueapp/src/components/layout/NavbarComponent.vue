<template>
    <nav class="navbar navbar-expand-lg navbar-light">
        <div
            class="collapse navbar-collapse"
            id="navbarSupportedContent"
        >
            <div class="navbar-main-area d-flex align-items-center flex-grow-1 ps-4">
                <span
                    v-if="isMockMode"
                    class="badge bg-warning text-dark me-2"
                >
                    Protótipo (mock)
                </span>
                <span class="badge bg-light text-dark border">{{ this.selectedTenant }}</span>
                <div class="navbar-right-group d-flex align-items-center gap-1 pe-2 ms-auto">
                    <NavbarNotificationComponent />
                    <ThemeSwitchComponent />
                    <LanguageComponent />
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
                                    :alt="$t('common.profileImageAlt')"
                                    width="32"
                                    height="32"
                                    class="rounded-circle me-2 navbar-user-photo"
                                    v-if="profileImage !== ''"
                                />
                                <span
                                    v-else
                                    class="navbar-user-avatar rounded-circle me-2"
                                    aria-hidden="true"
                                >
                                    {{ userInitials }}
                                </span>
                                {{ setBreakWord(user) }}
                            </a>
                            <ul
                                class="dropdown-menu dropdown-menu-sidebar text-small shadow menu-right user-profile-menu"
                                aria-labelledby="dropdownUser1"
                                id="dropdown-menu-button"
                            >
                                <li>
                                    <a
                                        class="dropdown-item user-profile-menu__item"
                                        href="#"
                                        @click.prevent
                                    >
                                        <LucideIcon
                                            icon="User"
                                            :size="16"
                                        />
                                        {{ $t("common.myAccount") }}
                                    </a>
                                </li>
                                <li>
                                    <router-link
                                        class="dropdown-item user-profile-menu__item"
                                        :to="{ name: 'Settings' }"
                                        :title="$t('pages.settings')"
                                    >
                                        <LucideIcon
                                            icon="Settings"
                                            :size="16"
                                        />
                                        {{ $t("pages.settings") }}
                                    </router-link>
                                </li>
                                <li><hr class="dropdown-divider user-profile-menu__divider" /></li>
                                <li>
                                    <router-link
                                        class="dropdown-item user-profile-menu__item user-profile-menu__item--danger"
                                        :to="{
                                            name: 'Logout',
                                        }"
                                        :title="$t('common.signOut')"
                                    >
                                        <LucideIcon
                                            icon="LogOut"
                                            :size="16"
                                        />
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
    import LanguageComponent from "@/components/layout/LanguageComponent.vue";
    import NavbarNotificationComponent from "@/components/layout/NavbarNotificationComponent.vue";
    import ThemeSwitchComponent from "@/components/layout/ThemeSwitchComponent.vue";
    import { isMockMode } from "@/mock/mockConfig.js";

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
            LanguageComponent,
            NavbarNotificationComponent,
            ThemeSwitchComponent,
        },
        data() {
            return {
                title: "Component NavBar",
                profileImage: "",
                user: this.$store.state.userProfile.name,
                selectedTenant: null,
                isMockMode: isMockMode(),
            };
        },
        methods: {
            InitializeTenant(tenant) {
                return api.get("/Tenant/InitializeTenant/" + tenant);
            },
            initializeSelectedTenant(savedTenant) {
                this.selectedTenant = savedTenant;
                if (!this.tenantInitialized) {
                    this.InitializeTenant(this.selectedTenant);
                    this.$store.commit("setTenantInitialized", true);
                }
            },
            getProfileImage() {
                if (isMockMode()) {
                    return;
                }
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
                            amount: this.profileImage,
                        });
                    });
            },
            setBreakWord(str) {
                const strSplit = str.trim().split(" ");
                if (strSplit.length === 1) return strSplit[0];
                return `${strSplit[0]} ${strSplit[strSplit.length - 1]}`;
            },
        },
        computed: {
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
            userInitials() {
                const displayName = (this.user || "").trim();
                if (!displayName) {
                    return this.initials;
                }

                const parts = displayName.split(/\s+/).filter(Boolean);
                if (parts.length === 1) {
                    const name = parts[0];
                    return (
                        (name[0] || "").toUpperCase() + (name[name.length - 1] || "").toUpperCase()
                    );
                }

                const first = parts[0][0] || "";
                const last = parts[parts.length - 1][0] || "";
                return (first + last).toUpperCase();
            },
        },
        created() {
            const savedTenant = this.$store.state.userProfile.tenant;
            if (savedTenant === "" || this.$store.state.userProfile.login === "") {
                router.push({ name: "Logout" });
            }

            if (this.$store.state.userProfile.tokenAzure != "") {
                this.getProfileImage();
            }

            this.initializeSelectedTenant(savedTenant);
        },
    };
</script>
<style scoped>
    .navbar {
        padding: 1%;
        padding-top: 0.9rem;
        padding-bottom: 0.8rem;
    }

    .navbar-light {
        background-color: var(--color-bg-navbar) !important;
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
        font-size: 1rem;
    }

    .circle-icon {
        width: 32px;
        height: 32px;
        background-color: var(--color-bg-btn-primary) !important;
        text-align: center;
        color: white;
        border-radius: 50%;
        padding-top: 7%;
    }

    #dropdown-menu-button {
        margin-top: 0.65rem !important;
    }

    .navbar-user-photo {
        object-fit: cover;
    }

    .navbar-user-avatar {
        width: 32px;
        height: 32px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        background-color: var(--color-bg-btn-primary);
        color: #fff;
        font-size: 0.72rem;
        font-weight: 700;
        letter-spacing: 0.02em;
        flex-shrink: 0;
    }

    .user-profile-menu {
        min-width: 11.5rem;
        padding: 0.35rem 0;
        border: 1px solid var(--color-border-form-control);
        border-radius: 0.55rem;
        overflow: hidden;
    }

    .user-profile-menu__item {
        display: flex;
        align-items: center;
        gap: 0.55rem;
        padding: 0.55rem 0.95rem !important;
        font-size: 0.9rem;
        font-weight: 500;
        color: var(--color-body-content) !important;
        background-color: transparent !important;
    }

    .user-profile-menu__item:hover,
    .user-profile-menu__item:focus {
        background-color: var(--color-bg-sidebar-li-selected) !important;
        color: var(--color-body-content) !important;
    }

    .user-profile-menu__item--danger,
    .user-profile-menu__item--danger:hover,
    .user-profile-menu__item--danger:focus {
        color: #dc3545 !important;
    }

    .user-profile-menu__divider {
        margin: 0.25rem 0;
        border-color: var(--color-border-form-control);
        opacity: 1;
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

        .text-black {
            color: black;
        }

        #tenantDropdownButton {
            width: 100%;
            font-size: 1rem;
        }

        .circle-icon {
            width: 32px;
            height: 32px;
            background-color: var(--color-bg-btn-primary) !important;
            text-align: center;
            color: white;
            border-radius: 50%;
            padding-top: 7%;
        }

        #dropdown-menu-button {
            margin-top: 1rem !important;
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
                padding: 0.5rem;
            }

            #tenantDropdownMenu {
                width: 100%;
                font-size: 0.85rem;
            }

            #tenantDropdownLabel {
                font-size: 0.9rem;
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
            padding: 0.25rem 0.5rem;
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
    }
    .bg-light {
        background-color: var(--color-bg-body-content) !important;
        color: var(--color-body-content) !important;
    }

    .border {
        border: 1px solid var(--color-border-form-control) !important;
    }
</style>
