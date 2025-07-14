<template>
    <main class="flex-shrink-0">
        <div class="container" style="padding: 0;">
            <div class="row">
                <div class="col-auto col-fix">
                    <form 
                        @submit="login" 
                        style="text-align: center;"
                    >
                        <img 
                            src="./../../../assets/img/woopiai-hub-logo.png" 
                            style="padding-bottom: 10px;" 
                            width="160" 
                            height="80" 
                            v-if="showLogoDarkMode" 
                        />

                        <img 
                            src="./../../../assets/img/woopiai-hub-logo.png" 
                            style="padding-bottom: 10px;" 
                            width="160" 
                            height="80" 
                            v-else 
                        />

                        <button 
                            v-if="!loading"
                            type="submit" 
                            class="btn btn-primary" 
                        >
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
        methods: {
            microsoftLogin(clientIdResponse) {
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
                            language: this.$store.state.userProfile.language,
                            image: "",
                            name: response.account.name,
                            login: response.account.username,
                            tokenAzure: response.accessToken,
                            tokenApi: "",
                            tenant: "",
                            keyMongoAccess: "",
                        };
                        this.$store.commit('updateUserProfile', { amount: dataUser });
                        this.redirectToDocument(response.account.name, response.account.username, response.accessToken);
                    }).catch(error => {
                        console.log(error);
                        this.loading = false;
                    });
            },
            login(e) {
                e.preventDefault();
                this.loading = true;
                let clientIdResponse = "";
                api.get('/Account/clientId')
                    .then(function (response) {
                      this.microsoftLogin(response.data);
                    }).catch(function (e) {
                        console.log(e);
                        this.loading = false;
                        alert("Usuário sem autorização para acessar a plataforma!\nPor favor, entre em contato com suporte@woopi.com.br solicitando acesso ao seu usuário.");
                    }).finally(function () {
                        console.log("Finished request.");
                    });
            },
            redirectToDocument(userName, userEmail, userAzure) {
                var formData = new FormData();
                formData.append("login", userEmail);

                api.post('/Account/Authenticate', formData,
                    {
                        headers: { 'Authorization': `Bearer ${userAzure}` }
                    }).then(function (response) {
                        var dataUser = {
                            language: this.$store.state.userProfile.language,
                            image: "",
                            name: userName,
                            login: userEmail,
                            tokenAzure: userAzure,
                            tokenApi: response.data.token,
                            tenant: response.data.tenant,
                            keyMongoAccess: ""
                        };
                        this.$store.commit('updateUserProfile', { amount: dataUser });
                        window.localStorage.setItem('project', JSON.stringify({ isLogged: true }));
                        this.$router.push({ name: 'DocumentList' });
                    }).catch(function (e) {
                        console.log(e);
                        this.loading = false;
                        alert("Usuário sem autorização para acessar a plataforma!\nPor favor, entre em contato com suporte@woopi.com.br solicitando acesso ao seu usuário.");
                    }).finally(function () {
                        console.log("Finished request.");
                    });
            },
            checkTheme() {
                const element = document.querySelector('html');
                    if (element.classList.value == 'css-theme-dark') {
                        this.showLogoDarkMode = true;
                    } else {
                        this.showLogoDarkMode = false;
                    }
            },
        },
        created() {
            if (useRouter().currentRoute.value.name === "Login") {
                this.$router.push({ name: 'DocumentList' });
            }
            this.checkTheme();
        },
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
