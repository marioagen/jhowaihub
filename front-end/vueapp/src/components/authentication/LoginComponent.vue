<template>
    <main class="d-flex justify-content-center align-items-center min-vh-100">
        <div class="container" style="padding: 0">
            <div class="row justify-content-center">
                <div class="text-center">
                    <img v-if="showLogoDarkMode" src="../../assets/img/woopiai-hub-logo.png"
                        style="padding-bottom: 10px" width="160" height="80" />
                    <img v-else src="../../assets/img/woopiai-hub-logo.png" style="padding-bottom: 10px" width="160"
                        height="61" />
                </div>
                <div class="card mb-3" style="max-width: 25rem;">
                    <div class="text-center mt-3">
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
                                    <span class="input-group-text border-end-0 bg-white">
                                        <LucideIcon icon="Mail" size="16" />
                                    </span>
                                    <input v-bind="field" type="text" id="email"
                                        class="form-control form-control-sm border-start-0"
                                        :class="{ 'is-invalid': errorMessage }" placeholder="user@mail.com" />
                                </div>
                                <span class="validation-message text-danger" v-if="errorMessage">{{ errorMessage }}</span>
                            </Field>
                        </div>

                        <div class="mb-3">
                            <label for="password" class="form-label">{{ $t("login.password") }}</label>
                            <div class="input-group">
                                <span class="input-group-text border-end-0 bg-white">
                                    <LucideIcon icon="Lock" size="16" />
                                </span>
                                <input v-bind="field" id="password" name="password" placeholder="******"
                                    v-model="credentials.password"
                                    class="form-control form-control-sm border-start-0 border-end-0"
                                    :type="showPassword ? 'text' : 'password'"
                                    :class="{ 'is-invalid': errorMessage }" />
                                <span class="input-group-text border-start-0 bg-white">
                                    <LucideIcon v-if="showPassword" icon="Eye" size="16" @click="togglePassword" />
                                    <LucideIcon v-else icon="EyeClosed" size="16" @click="togglePassword" />
                                </span>
                            </div>
                        </div>

                        <div class="mb-3">
                            <a v-if="isLoading" class="btn btn-primary btn-sm w-100">
                                <i class="fas fa-spinner fa-pulse"></i>
                                {{ $t("login.loading") }}
                            </a>
                            <button v-else type="button" class="btn btn-primary btn-sm w-100" @click="login">
                                <LucideIcon icon="LogIn" size="15" class="me-1" />
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
    </main>
</template>

<script>
import { Field, useForm } from "vee-validate";
import { getJWTPermissions } from "@/utils/permissions";
import AuthService from "@/services/authenticate/AuthService";

export default {
    name: "LoginIndex",
    components: {
        Field
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
            showLogoDarkMode: false,
            showPassword: false,
            credentials: {
                email: "",
                password: ""
            },
        };
    },
    methods: {
        async login() {
            const result = await this.validate();
            if (!result.valid) {
                return this.$notify({
                    title: 'Login',
                    message: 'Campo inválidos',
                    variant: 'warning',
                    icon: 'CircleAlert',
                });
            }

            this.isLoading = true;
            this.credentials.email = this.values.email;
            AuthService.Login(this.credentials)
                .then((response) => {
                    let tokenData = this.getPermissions(response.tokenApi);
                    this.$store.commit("updatePermissions", tokenData.permissions);
                    let dataUser = {
                        language: this.$store.state.userProfile.language,
                        image: "",
                        name: response.name,
                        login: response.login,
                        tokenAzure: "",
                        tokenApi: response.tokenApi,
                        tenant: response.tenant,
                        keyMongoAccess: "",
                        isAdmin: tokenData.isAdmin
                    };

                    this.$store.commit("updateUserProfile", { amount: dataUser });
                    window.localStorage.setItem("project", JSON.stringify({ isLogged: true }));
                    this.redirectToDocument();
                })
                .catch((error) => {
                    const labelKey = error.response?.data?.labelError ?? 'unexpectedError';
                    const exists = this.$te(labelKey);
                    const message = exists ? this.$t(labelKey) : this.$t('unexpectedError');

                    this.$notify({
                        title: 'Error',
                        message,
                        variant: 'danger',
                        icon: 'CircleX',
                    });
                })
                .finally(() => {
                    this.isLoading = false;
                })
        },
        loginSSO() {
            this.isLoadingSSO = true;
            AuthService.GetClientId()
                .then((response) => {
                    this.$notify({
                        title: 'Login',
                        message: 'login.validateClient',
                        variant: 'info',
                        icon: 'MessageCircle',
                    });
                    this.microsoftLogin(response);
                })
                .catch(() => {
                    this.$notify({
                        title: 'Error',
                        message: this.$t('unexpectedError'),
                        variant: 'danger',
                        icon: 'CircleX',
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
                        title: 'Error',
                        message: this.$t('unexpectedError'),
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
                        title: 'Login',
                        message: 'login.authSSO',
                        variant: 'info',
                        icon: 'MessageCircle',
                    });
                })
                .catch((error) => {
                    this.$notify({
                        title: 'Error',
                        message: this.$t('unexpectedError'),
                        variant: 'danger',
                        icon: 'CircleX',
                    });

                    this.isLoadingSSO = false;
                });
        },
        authenticateUser(userName, userEmail, userAzure) {
            var formData = new FormData();
            formData.append("login", userEmail);

            AuthService.LoginSSO(formData, userAzure)
                .then((response) => {
                    let tokenData = this.getPermissions(response.tokenApi);
                    this.$store.commit("updatePermissions", tokenData.permissions);

                    let dataUser = {
                        language: this.$store.state.userProfile.language,
                        image: "",
                        name: userName,
                        login: userEmail,
                        tokenAzure: userAzure,
                        tokenApi: response.tokenApi,
                        tenant: response.tenant,
                        keyMongoAccess: "",
                        isAdmin: tokenData.isAdmin
                    };

                    this.$store.commit("updateUserProfile", { amount: dataUser });
                    window.localStorage.setItem("project", JSON.stringify({ isLogged: true }));
                    this.redirectToDocument();
                })
                .catch((error) => {
                    const labelKey = error.response?.data?.labelError ?? 'unexpectedError';
                    const exists = this.$te(labelKey);
                    const message = exists ? this.$t(labelKey) : this.$t('unexpectedError');

                    this.$notify({
                        title: 'Error',
                        message,
                        variant: 'danger',
                        icon: 'CircleX',
                    });
                })
                .finally(() => {
                    this.isLoadingSSO = false;
                });
        },
        redirectToDocument() {
            this.$router.push({ name: "Documents" });
        },
        getPermissions(token) {
            return getJWTPermissions(token);
        },
        checkTheme() {
            const element = document.querySelector("html");
            if (element.classList.value == "css-theme-dark") {
                this.showLogoDarkMode = true;
            } else {
                this.showLogoDarkMode = false;
            }
        },
        togglePassword() {
            this.showPassword = !this.showPassword;
        },
    },
    created() {
        let login = this.$store.state.userProfile.login;
        let tenant = this.$store.state.userProfile.tenant;
        if (login !== "" || tenant !== "") {
            this.$router.push({ name: "Documents" });
        }
        this.checkTheme();
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
</style>