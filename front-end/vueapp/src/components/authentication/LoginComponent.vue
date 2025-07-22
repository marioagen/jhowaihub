<!-- <template>
    <main class="flex-shrink-0">
        <div class="container" style="padding: 0">
            <div class="row">
                <div class="col-auto ">
                    <form @submit="login" style="text-align: center">
                        <img
                            v-if="showLogoDarkMode"
                            src="../assets/img/woopiai-hub-logo.png"
                            style="padding-bottom: 10px"
                            width="160"
                            height="80"
                        />

                        <img
                            v-else
                            src="../assets/img/woopiai-hub-logo.png"
                            style="padding-bottom: 10px"
                            width="160"
                            height="61"
                        />

                        <button v-if="!loading" type="submit" class="btn btn-primary">
                            <i class="fab fa-windows"></i>
                            Microsoft Login
                        </button>

                        <a class="btn btn-primary" v-else>
                            <i class="fas fa-spinner fa-pulse"></i>
                            Microsoft Login
                        </a>
                    </form>
                </div>
            </div>
        </div>
    </main>
</template> -->
<template>
    <main class="d-flex justify-content-center align-items-center min-vh-100">
        <div class="container" style="padding: 0">
            <div class="row justify-content-center">
                <div class="text-center">
                    <img
                        v-if="showLogoDarkMode"
                        src="../../assets/img/woopiai-hub-logo.png"
                        style="padding-bottom: 10px"
                        width="160"
                        height="80"
                    />

                    <img
                        v-else
                        src="../../assets/img/woopiai-hub-logo.png"
                        style="padding-bottom: 10px"
                        width="160"
                        height="61"
                    />
                </div>
                <div class="card mb-3" style="max-width: 25rem;">
                    <div class="text-center mt-3">
                        <h6 class="fw-bold">
                            Fazer Login
                            <!-- {{ $t("labelTypes") }} -->
                        </h6>
                        <p>
                            <small class="text-muted">
                                Acesse sua conta para gerenciar documentos
                                <!-- {{ $t("labelTypesMessage") }} -->
                            </small>
                        </p>
                    </div>

                    <div class="card-body">
                        <div class="mb-3">
                            <label for="email" class="form-label">Email</label>
                            <div class="input-group">
                                <span class="input-group-text border-end-0 bg-white">
                                    <LucideIcon icon="Mail" size="16" />
                                </span>
                                <input
                                    id="email"
                                    name="email"
                                    type="text"
                                    class="form-control form-control-sm border-start-0"
                                    placeholder="user@mail.com"
                                />
                            </div>
                        </div>

                        <div class="mb-3">
                            <label for="password" class="form-label">Senha</label>
                            <div class="input-group">
                                <span class="input-group-text border-end-0 bg-white">
                                    <LucideIcon icon="Lock" size="16" />
                                </span>
                                <input
                                    id="password"
                                    name="password"
                                    placeholder="******"
                                    class="form-control form-control-sm border-start-0 border-end-0"
                                    :type="showPassword ? 'text' : 'password'"                                    
                                />
                                <span class="input-group-text border-start-0 bg-white">
                                    <LucideIcon
                                        v-if="showPassword"
                                        icon="Eye" 
                                        size="16"
                                        @click="togglePassword"
                                    />
                                    <LucideIcon 
                                        v-else
                                        icon="EyeClosed" 
                                        size="16"
                                        @click="togglePassword"
                                    />
                                </span>
                            </div>
                        </div>

                        <div class="mb-3">
                            <a 
                                v-if="isLoading" 
                                class="btn btn-primary btn-sm w-100"
                            >
                                <i class="fas fa-spinner fa-pulse"></i>
                                Carregando...
                            </a>
                            <button
                                v-else
                                type="button" 
                                class="btn btn-primary btn-sm w-100"
                            >
                                <LucideIcon 
                                    icon="LogIn"
                                    size="15"
                                    class="me-1"
                                />
                                Login
                            </button>
                        </div>

                        <div class="d-flex align-items-center my-3">
                            <hr class="flex-grow-1" />
                            <span class="px-2 text-muted">Or</span>
                            <hr class="flex-grow-1" />
                        </div>

                        <div class="mb-3">
                            <button v-if="!loading" @click="login" type="submit" class="btn btn-primary w-100">
                                <i class="fab fa-windows"></i>
                                Login com Microsoft
                            </button>

                            <a class="btn btn-primary w-100" v-else>
                                <i class="fas fa-spinner fa-pulse"></i>
                                Login com Microsoft
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
    import { useRouter } from "vue-router";
    import api from "@/services/api";

    export default {
        name: "LoginIndex",
        data() {
            return {
                loading: false,
                isLoading: false,
                isLoadingMicrosoft: false,
                showLogoDarkMode: false,
                showPassword: false,
            };
        },
        methods: {
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
                    .then((response) => {})
                    .catch((error) => {
                        console.log(error);
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
                        this.redirectToDocument(response.account.name, response.account.username, response.accessToken);
                    })
                    .catch((error) => {
                        console.log(error);
                        this.loading = false;
                    });
            },
            login(e) {
                e.preventDefault();
                this.loading = true;
                let clientIdResponse = "";
                api.get("/Account/clientId")
                    .then((response) => {
                        this.microsoftLogin(response.data);
                    })
                    .catch((e) => {
                        console.log(e);
                        this.loading = false;
                        alert(
                            "Usuário sem autorização para acessar a plataforma!\nPor favor, entre em contato com suporte@woopi.com.br solicitando acesso ao seu usuário."
                        );
                    })
                    .finally(() => {
                        console.log("Finished request.");
                    });
            },
            redirectToDocument(userName, userEmail, userAzure) {
                var formData = new FormData();
                formData.append("login", userEmail);

                api.post("/Account/Authenticate", formData, {
                    headers: { Authorization: `Bearer ${userAzure}` },
                })
                    .then((response) => {
                        var dataUser = {
                            language: this.$store.state.userProfile.language,
                            image: "",
                            name: userName,
                            login: userEmail,
                            tokenAzure: userAzure,
                            tokenApi: response.data.token,
                            tenant: response.data.tenant,
                            keyMongoAccess: "",
                        };
                        this.$store.commit("updateUserProfile", { amount: dataUser });
                        window.localStorage.setItem("project", JSON.stringify({ isLogged: true }));
                        this.$router.push({ name: "DocumentList" });
                    })
                    .catch((e) => {
                        console.log(e);
                        this.loading = false;
                        alert(
                            "Usuário sem autorização para acessar a plataforma!\nPor favor, entre em contato com suporte@woopi.com.br solicitando acesso ao seu usuário."
                        );
                    })
                    .finally(() => {
                        console.log("Finished request.");
                    });
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
            if (useRouter().currentRoute.value.name === "Login") {
                this.$router.push({ name: "DocumentList" });
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
</style>