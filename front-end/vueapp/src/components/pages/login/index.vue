<template>
    <main class="flex-shrink-0">
        <div class="container" style="padding: 0;">
            <div class="row">
                <div class="col-auto col-fix">
                    <form @submit="login" style="text-align: center;">
                        <img src="./../../../assets/img/logo-login-light.png" style="padding-bottom: 10px;" width="200" v-if="showLogoDarkMode" />
                        <img src="./../../../assets/img/logo-login.png" style="padding-bottom: 10px;" v-else />
                        <button type="submit" class="btn btn-primary" v-if="!loading">
                            <i class="fab fa-windows"></i> Microsoft Login
                        </button>
                        <a  class="btn btn-primary" v-else>
                            <i class="fas fa-spinner fa-pulse"></i> Microsoft Login
                        </a>
                    </form>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
    import { useRouter } from 'vue-router';
    import api from "@/services/api";

    export default {
        name: "LoginIndex",
        data() {
            return {
                loading: false,
                showLogoDarkMode: false,
            }
        },
        components: {},
        watch: {},
        methods: {
            microsoftLogin: function (clientIdResponse) {
                let self = this;
                const msalConfig = {
                    auth: {
                        clientId: clientIdResponse,
                        authority: "https://login.microsoftonline.com/common/"
                    },
                    cache: {
                        cacheLocation: "sessionStorage",
                        storeAuthStateInCookie: false,
                    },
                };

                const MSALobj = new msal.PublicClientApplication(msalConfig);

                MSALobj.handleRedirectPromise()
                    .then((response) => {
                    })
                    .catch((error) => {
                        console.log(error);
                    });

                const loginRequest = {
                    scopes: ["User.Read"]
                };

                MSALobj.loginPopup(loginRequest)
                    .then(response => {
                        var dataUser = {
                            language: self.$store.state.userProfile.language,
                            image: "",
                            name: response.account.name,
                            login: response.account.username,
                            tokenAzure: response.accessToken,
                            tokenApi: "",
                            tenant: "",
                            keyMongoAccess: "",
                        };
                        self.$store.commit('updateUserProfile', { amount: dataUser });
                        self.redirectToDocument(response.account.name, response.account.username, response.accessToken);
                    }).catch(error => {
                        console.log(error);
                        self.loading = false;
                    });
            },

            login: function (e) {
                e.preventDefault();
                this.loading = true;
                let clientIdResponse = "";
                let self = this;
                api.get('/Account/clientId')
                    .then(function (response) { // Handle success
                      self.microsoftLogin(response.data);
                    }).catch(function (e) { // Handle error
                        console.log(e);
                        self.loading = false;
                        alert("Usuário sem autorização para acessar a plataforma!\nPor favor, entre em contato com suporte@woopi.com.br solicitando acesso ao seu usuário.");
                    }).finally(function () { // Always executed
                        console.log("Finished request.");
                    });
            },

            redirectToDocument: function (userName, userEmail, userAzure) {
                var formData = new FormData();
                formData.append("login", userEmail);
                let self = this;
                api.post('/Account/Authenticate', formData,
                    {
                        headers: { 'Authorization': `Bearer ${userAzure}` }
                    }).then(function (response) { // Handle success
                        var dataUser = {
                            language: self.$store.state.userProfile.language,
                            image: "",
                            name: userName,
                            login: userEmail,
                            tokenAzure: userAzure,
                            tokenApi: response.data.token,
                            tenant: response.data.tenant,
                            keyMongoAccess: ""
                        };
                        self.$store.commit('updateUserProfile', { amount: dataUser });
                        window.localStorage.setItem('project', JSON.stringify({ isLogged: true }));
                        self.$router.push({ name: 'DocumentList' });
                    }).catch(function (e) { // Handle error
                        console.log(e);
                        self.loading = false;
                        alert("Usuário sem autorização para acessar a plataforma!\nPor favor, entre em contato com suporte@woopi.com.br solicitando acesso ao seu usuário.");
                    }).finally(function () { // Always executed
                        console.log("Finished request.");
                    });
            },
            checkTheme: function () {
                let self = this;
                const element = document.querySelector('html');
                    if (element.classList.value == 'css-theme-dark') {
                        self.showLogoDarkMode = true;
                    } else {
                        self.showLogoDarkMode = false;
                    }
            }
        },
        computed: {},
        created() {
            if (useRouter().currentRoute.value.name === "Login") { // If user logged - Force redirect
                this.$router.push({ name: 'DocumentList' });
            }
            this.checkTheme();
        },

        mounted() {},
        updated() {},
        unmounted() { },
    }
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
