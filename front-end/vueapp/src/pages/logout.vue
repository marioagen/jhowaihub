<script>
import store from "@/store";
import AuthService from "@/services/authenticate/AuthService";

export default {
    name: "Logout",
    beforeRouteEnter(to, from, next) {
        AuthService.Logout()
            .catch((err) => {
                console.warn("Erro ao deslogar no back-end:", err);
            })
            .finally(() => {
                // Always use light theme
                document.documentElement.className = "css-theme-light";
                // Clear any theme data from localStorage
                window.localStorage.removeItem("theme");
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