<template>
    <nav class="navbar navbar-expand-lg navbar-light">
        <div class="collapse navbar-collapse" id="navbarSupportedContent">
            <div class="navbar-main-area d-flex align-items-center flex-grow-1 ps-4">
                <div class="dropdown" id="tenantDropdownContainer">
                    <button class="btn dropdown-toggle d-flex align-items-center" type="button"
                        id="tenantDropdownButton" data-bs-toggle="dropdown" aria-expanded="false" style="
                            background-color: var(--dropdown-bg-color);
                            border: 1px solid var(--border-color);
                            color: var(--dropdown-text-color);
                            padding-right: 20px;
                            text-align: left;
                        ">
                        <div class="circle-icon">
                            {{ initials }}
                        </div>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-sidebar text-small shadow" aria-labelledby="tenantDropdown"
                        id="tenantDropdownMenu" style="white-space: normal; overflow-wrap: break-word">
                        <li v-for="tenant in tenantsFromState" :key="tenant" class="remove-hover">
                            <label class="dropdown-item" id="tenantDropdownLabel"
                                style="display: flex; align-items: center; word-break: break-word">
                                <input type="radio" name="tenant" :value="tenant" v-model="selectedTenant"
                                    @change="handleTenantChange" style="margin-right: 10px" />
                                {{ tenant }}
                            </label>
                        </li>
                    </ul>
                </div>

                <div class="dropdown nav-buttons">
                    <button
                        class="btn btn-outline-primary table-btn btn-sm"
                        type="button"
                        data-bs-toggle="dropdown"
                        aria-expanded="false"
                        style="display: flex; align-items: center; justify-content: center;"
                    >
                        <LucideIcon icon="Globe" />
                    </button>
                    <ul class="dropdown-menu dropdown-menu-button text-small shadow">
                        <li>
                            <a :class="$i18n.locale === 'pt'
                                ? 'btn btn-light lang-link lang-active'
                                : 'btn btn-light lang-link btn-lang'
                                " @click="setLanguage('pt')">
                                <img src="./../../assets/img/lang-pt.png" alt="PT" />
                                <span style="margin-left: 2px">PT</span>
                            </a>
                        </li>
                        <li>
                            <a :class="$i18n.locale === 'en'
                                ? 'btn btn-light lang-link lang-active btn-lang'
                                : 'btn btn-light lang-link btn-lang'
                                " @click="setLanguage('en')">
                                <img src="./../../assets/img/lang-en.png" alt="EN" />
                                <span style="margin-left: 2px">EN</span>
                            </a>
                        </li>
                        <li>
                            <a :class="$i18n.locale === 'es'
                                ? 'btn btn-light lang-link lang-active btn-lang'
                                : 'btn btn-light lang-link btn-lang'
                                " @click="setLanguage('es')">
                                <img src="./../../assets/img/lang-es.png" alt="ES" />
                                <span style="margin-left: 2px">ES</span>
                            </a>
                        </li>
                    </ul>
                </div>

                <div class="dropdown-menu-user" style="cursor: pointer">
                    <div class="dropdown" style="float: right">
                        <a class="d-flex align-items-center text-black text-decoration-none dropdown-toggle username"
                            id="dropdownUser1" data-bs-toggle="dropdown" aria-expanded="false">
                            <img :src="profileImage" alt="Imagem do perfil" width="32" height="32"
                                class="rounded-circle me-2" v-if="profileImage !== ''" />
                            {{ setBreakWord(user) }}
                        </a>
                        <a class="d-flex align-items-center text-black text-decoration-none dropdown-toggle username-collapsed"
                            id="dropdownUser1" data-bs-toggle="dropdown" aria-expanded="false">
                            <img :src="profileImage" alt="Imagem do perfil" width="32" height="32"
                                class="rounded-circle me-2" v-if="profileImage !== ''" />
                        </a>
                        <ul class="dropdown-menu dropdown-menu-sidebar text-small shadow"
                            aria-labelledby="dropdownUser1" id="dropdown-menu-button">
                            <li class="remove-hover mt-2">
                                <router-link class="dropdown-item mx-2 my-2" :to="{
                                    name: 'Logout',
                                    query: { darkMode: this.showLogoDarkMode },
                                }" title="Sair">
                                    <LucideIcon icon="LogOut" />
                                    {{ $t("labelSignOut") }}
                                </router-link>
                            </li>
                        </ul>
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

export default {
    name: "NavBarComponent",
    props: {
        sidebarData: {
            required: true,
            type: String,
            default: "",
        },
    },
    data() {
        return {
            title: "Component NavBar",
            showLogoDarkMode: false,
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

            return api.get("/Tenant/InitializeTenant/" + tenant).then(function (result) {
                if (result.data) {
                    self.$store.commit("updateUserProfileKeyMongo", {
                        amount: result.data,
                    });
                }
            });
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
            axios.get("https://graph.microsoft.com/v1.0/me/photos/48x48/$value", {
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
        toggleTheme() {
            if (localStorage.getItem("theme") === "css-theme-dark") {
                this.setTheme("css-theme-light");
                this.showLogoDarkMode = false;
            } else {
                this.setTheme("css-theme-dark");
                this.showLogoDarkMode = true;
            }
        },
        setTheme(themeName) {
            localStorage.setItem("theme", themeName);
            document.documentElement.className = themeName;
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
        if (localStorage.getItem("theme") === "css-theme-dark") {
            this.setTheme("css-theme-dark");
            this.showLogoDarkMode = true;
        } else {
            this.setTheme("css-theme-light");
            this.showLogoDarkMode = false;
        }
    },
};
</script>

<style scoped>
.navbar {
    padding: 1;
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
</style>
