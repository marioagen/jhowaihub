<template>
    <nav class="navbar navbar-expand-lg navbar-light">
        <div
            class="collapse navbar-collapse"
            id="navbarSupportedContent"
        >
            <div class="navbar-main-area d-flex align-items-center flex-grow-1 ps-4">
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
                                    alt="Imagem do perfil"
                                    width="32"
                                    height="32"
                                    class="rounded-circle me-2"
                                    v-if="profileImage !== ''"
                                />
                                {{ setBreakWord(user) }}
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
    import LanguageComponent from "@/components/layout/LanguageComponent.vue";
    import NavbarNotificationComponent from "@/components/layout/NavbarNotificationComponent.vue";
    import ThemeSwitchComponent from "@/components/layout/ThemeSwitchComponent.vue";

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
