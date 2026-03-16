<template>
    <main class="d-flex justify-content-center align-items-center min-vh-100">
        <div class="container" style="padding: 0">
            <div class="row justify-content-center">
                <div class="login-wrapper">
                <div class="card mb-3">
                    <div class="text-center mt-3">
                        <img :src="logoSrc" style="padding-bottom: 10px; height: 60px;"
                            alt="WOOPI AI" />
                        <h6 class="fw-bold">
                            {{ $t("login.title") }}
                        </h6>
                        <p>
                            <small class="text-muted">
                                {{ $t("login.subtitle") }}
                            </small>
                        </p>
                    </div>

                    <div class="card-body">
                        <div class="mb-3">
                            <label for="email" class="form-label">Email</label>
                            <Field name="email" rules="required|email" v-slot="{ field, errorMessage }">
                                <div class="input-group">
                                    <span class="input-group-text border-end-0">
                                        <LucideIcon icon="Mail" :size="16" />
                                    </span>
                                    <input v-bind="field" type="text" id="email"
                                        class="form-control form-control-sm border-start-0"
                                        :class="{ 'is-invalid': errorMessage }" placeholder="user@mail.com" />
                                </div>
                                <span class="validation-message text-danger" v-if="errorMessage">{{ errorMessage
                                    }}</span>
                            </Field>
                        </div>

                        <div class="mb-3">
                            <label for="password" class="form-label">{{ $t("login.password") }}</label>
                            <div class="input-group">
                                <span class="input-group-text border-end-0">
                                    <LucideIcon icon="Lock" :size="16" />
                                </span>
                                <input v-bind="field" id="password" name="password" placeholder="******"
                                    v-model="credentials.password"
                                    class="form-control form-control-sm border-start-0 border-end-0"
                                    :type="showPassword ? 'text' : 'password'"
                                    :class="{ 'is-invalid': errorMessage }" />
                                <span class="input-group-text border-start-0">
                                    <LucideIcon v-if="showPassword" icon="Eye" :size="16" @click="togglePassword" />
                                    <LucideIcon v-else icon="EyeClosed" :size="16" @click="togglePassword" />
                                </span>
                            </div>
                        </div>

                        <div class="mb-3">
                            <a v-if="isLoading" class="btn btn-primary btn-sm w-100">
                                <i class="fas fa-spinner fa-pulse"></i>
                                {{ $t("login.loading") }}
                            </a>
                            <button v-else type="button" class="btn btn-primary btn-sm w-100" @click="login">
                                <LucideIcon icon="LogIn" :size="15" class="me-1" />
                                Login
                            </button>
                        </div>

                        <div class="d-flex align-items-center my-3">
                            <hr class="flex-grow-1" />
                            <span class="px-2 text-muted">Or</span>
                            <hr class="flex-grow-1" />
                        </div>

                        <div class="mb-3">
                            <button v-if="!isLoadingSSO" type="submit" class="btn btn-outline-primary btn-sm w-100"
                                @click="loginSSO">
                                <img src="../../assets/img/microsoft-log.svg" width="30" height="15" />
                                {{ $t("login.sso") }}
                            </button>

                            <a class="btn btn-outline-primary w-100" v-else>
                                <i class="fas fa-spinner fa-pulse"></i>
                                {{ $t("login.loading") }}
                            </a>
                        </div>
                    </div>
                </div>
                </div>
            </div>
        </div>
    </main>
    <TenantModal :tenants="tenants" :typeLogin="typeLogin" @continueLogin="continueLogin" ref="TenantModal" />
</template>

<script>
import { Field, useForm } from "vee-validate";
import { getJWTPermissions } from "@/utils/permissions";
import AuthService from "@/services/authenticate/AuthService";
import { scheduleTokenRefresh } from "@/services/api";
import TenantModal from "@/components/authentication/TenantModal.vue";
import logoDark from "@/assets/img/woopiai-logo-dark.png";
import logoLight from "@/assets/img/woopiai-logo-light.png";

export default {
    name: "LoginIndex",
    components: {
        Field,
        TenantModal
    },
    setup() {
        const { validate, values } = useForm();
        return {
            validate,
            values
        }
    },
    data() {
        return {
            isLoading: false,
            isLoadingSSO: false,
            showPassword: false,
            credentials: {
                email: "",
                password: "",
                tenant: ""
            },
            field: {
                username: "",
                password: ""
            },
            errorMessage: "",
            tenants: [],
            typeLogin: ""
        };
    },
    computed: {
        logoSrc() {
            const theme = this.$store.state.theme || localStorage.getItem("theme") || "css-theme-light";
            return theme === "css-theme-dark" ? logoLight : logoDark;
        },
    },
    methods: {
        continueLogin(tenant, typeLogin) {
            this.credentials.tenant = tenant;
            if (typeLogin === "SSO") {
                this.authenticateUser(
                    this.$store.state.userProfile.name,
                    this.$store.state.userProfile.login,
                    this.$store.state.userProfile.tokenAzure
                );
            } else {
                this.login();
            }
        },
        async login() {
            this.typeLogin = "STANDARD";
            const result = await this.validate();
            if (!result.valid) {
                return this.$notify({
                    title: 'login.index',
                    message: 'login.invalid',
                    variant: 'warning',
                    icon: 'CircleAlert',
                });
            }

            this.isLoading = true;
            this.credentials.email = this.values.email;
            AuthService.Login(this.credentials)
                .then((response) => {

                    if (response?.tenants?.length > 0) {
                        
                        this.tenants = response.tenants;
                        this.$refs.TenantModal.open();
                        return;
                    }

                    let tokenData = this.getPermissions(response.token);
                    this.$store.commit("updatePermissions", tokenData.permissions);
                    let dataUser = {
                        language: this.$store.state.userProfile.language,
                        image: "",
                        name: response.name,
                        login: response.email,
                        tokenAzure: "",
                        tokenApi: response.token,
                        tenant: response.tenant,
                        keyMongoAccess: "",
                        isAdmin: tokenData.isAdmin
                    };

                    this.$store.commit("updateUserProfile", { amount: dataUser });
                    scheduleTokenRefresh();
                    window.localStorage.setItem("project", JSON.stringify({ isLogged: true }));
                    this.redirectToDocument();
                })
                .catch((error) => {
                    this.credentials.tenant = "";
                    const labelKey = error.response?.data?.labelError ?? 'unexpectedError';
                    const exists = this.$te(labelKey);
                    const message = exists ? this.$t(labelKey) : this.$t('unexpectedError');

                    this.$notify({
                        title: error.response?.data?.errorCode == 11 ? 'login.warning' : 'login.error',
                        message,
                        variant: error.response?.data?.errorCode == 11 ? 'warning' : 'danger',
                        icon: error.response?.data?.errorCode == 11 ? 'CircleAlert' :'CircleX',
                        duration: 6000
                    });
                })
                .finally(() => {
                    this.isLoading = false;
                })
        },
        loginSSO() {
            this.typeLogin = "SSO";
            this.isLoadingSSO = true;
            AuthService.GetClientId()
                .then((response) => {
                    this.$notify({
                        title: 'login.index',
                        message: 'login.validateClient',
                        variant: 'info',
                        icon: 'MessageCircle',
                    });
                    this.microsoftLogin(response);
                })
                .catch(() => {
                    this.$notify({
                        title: "login.error",
                        message: "unexpectedError",
                        variant: 'danger',
                        icon: 'CircleX'
                    });
                    this.isLoadingSSO = false;
                })
        },
        microsoftLogin(clientIdResponse) {
            const msalConfig = {
                auth: {
                    clientId: clientIdResponse,
                    authority: "https://login.microsoftonline.com/common/",
                },
                cache: {
                    cacheLocation: "sessionStorage",
                    storeAuthStateInCookie: false,
                },
            };

            const MSALobj = new msal.PublicClientApplication(msalConfig);
            MSALobj.handleRedirectPromise()
                .catch(() => {
                    this.$notify({
                        title: "login.error",
                        message: "unexpectedError",
                        variant: 'danger',
                        icon: 'CircleX',
                    });
                });

            const loginRequest = {
                scopes: ["User.Read"],
            };

            MSALobj.loginPopup(loginRequest)
                .then((response) => {
                    var dataUser = {
                        language: this.$store.state.userProfile.language,
                        image: "",
                        name: response.account.name,
                        login: response.account.username,
                        tokenAzure: response.accessToken,
                        tokenApi: "",
                        tenant: "",
                        keyMongoAccess: "",
                    };
                    this.$store.commit("updateUserProfile", { amount: dataUser });
                    this.authenticateUser(response.account.name, response.account.username, response.accessToken);
                    this.$notify({
                        title: 'login.index',
                        message: 'login.authSSO',
                        variant: 'info',
                        icon: 'MessageCircle',
                    });
                })
                .catch((error) => {
                    this.$notify({
                        title: "login.error",
                        message: "unexpectedError",
                        variant: 'danger',
                        icon: 'CircleX',
                    });

                    this.isLoadingSSO = false;
                });
        },
        authenticateUser(userName, userEmail, userAzure) {
            var formData = new FormData();
            formData.append("login", userEmail);
            formData.append("tenant", this.credentials.tenant ?? "");

            AuthService.LoginSSO(formData, userAzure)
                .then((response) => {
                    if (response?.tenants?.length > 0) {
                        this.tenants = response.tenants;
                        this.$refs.TenantModal.open();
                        return;
                    }

                    let tokenData = this.getPermissions(response.token);
                    this.$store.commit("updatePermissions", tokenData.permissions);

                    let dataUser = {
                        language: this.$store.state.userProfile.language,
                        image: "",
                        name: userName,
                        login: userEmail,
                        tokenAzure: userAzure,
                        tokenApi: response.token,
                        tenant: response.tenant,
                        keyMongoAccess: "",
                        isAdmin: tokenData.isAdmin
                    };

                    this.$store.commit("updateUserProfile", { amount: dataUser });
                    scheduleTokenRefresh();
                    window.localStorage.setItem("project", JSON.stringify({ isLogged: true }));
                    this.redirectToDocument();
                })
                .catch((error) => {
                    this.credentials.tenant = "";
                    const labelKey = error.response?.data?.labelError ?? 'unexpectedError';
                    const exists = this.$te(labelKey);
                    const message = exists ? this.$t(labelKey) : this.$t('unexpectedError');

                    this.$notify({
                        title: error.response?.data?.errorCode == 11 ? 'login.warning' : 'login.error',
                        message,
                        variant: error.response?.data?.errorCode == 11 ? 'warning' : 'danger',
                        icon: error.response?.data?.errorCode == 11 ? 'CircleAlert' :'CircleX',
                        duration: 6000
                    });
                })
                .finally(() => {
                    this.isLoadingSSO = false;
                });
        },
        redirectToDocument() {
            this.$router.push({ name: "Home" });
        },
        getPermissions(token) {
            return getJWTPermissions(token);
        },
        togglePassword() {
            this.showPassword = !this.showPassword;
        },
    },
    created() {
        let login = this.$store.state.userProfile.login;
        let tenant = this.$store.state.userProfile.tenant;
        if (login !== "" || tenant !== "") {
            this.$router.push({ name: "Home" });
        }
    },
};
</script>

<style scoped>
h5 {
    text-align: center;
}

.col-fix {
    margin: 0px auto;
    height: 100vh;
    align-items: center;
    display: flex;
    width: min-content;
}

.is-invalid {
    border-color: red;
}

main {
    background-color: var(--color-bg-body-content) !important;
}

.login-wrapper {
    width: 25rem;
    max-width: 100%;
    background-color: var(--color-bg-body-content);
}
</style>
