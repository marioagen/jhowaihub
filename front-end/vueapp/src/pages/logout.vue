<script>
    import store from "@/store";
    import AuthService from "@/services/authenticate/AuthService";
    import { cancelTokenRefresh } from "@/services/api";

    export default {
        name: "Logout",
        beforeRouteEnter(to, from, next) {
            AuthService.Logout()
                .catch((err) => {
                    console.warn("Erro ao deslogar no back-end:", err);
                })
                .finally(() => {
                    cancelTokenRefresh();
                    const savedTheme = window.localStorage.getItem("theme");
                    document.documentElement.className =
                        savedTheme === "css-theme-dark" ? "css-theme-dark" : "css-theme-light";
                    window.localStorage.removeItem("project");

                    const dataUser = {
                        language: store.state.userProfile.language,
                        image: "",
                        name: "",
                        login: "",
                        tokenAzure: "",
                        tokenApi: "",
                        tenant: "",
                        keyMongoAccess: "",
                        isAdmin: false,
                    };

                    store.commit("updateUserProfile", { amount: dataUser });
                    store.commit("setTenantInitialized", false);

                    next({ path: "/" });
                });
        },
    };
</script>
