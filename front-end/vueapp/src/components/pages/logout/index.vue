<script>
    import store from "@/store";

    export default {
        name: "Logout",
        beforeRouteEnter: function (to, from, next) {
            document.documentElement.className = to.query.darkMode === "true" ? "css-theme-dark" : "css-theme-light";
            window.localStorage.removeItem("project");
            var dataUser = {
                language: store.state.userProfile.language,
                image: "",
                name: "",
                login: "",
                tokenAzure: "",
                tokenApi: "",
                tenant: "",
                keyMongoAccess: "",
            };
            store.commit("updateUserProfile", { amount: dataUser });
            store.commit("setTenantInitialized", false);
            next({ path: "/" });
        },
    };
</script>
