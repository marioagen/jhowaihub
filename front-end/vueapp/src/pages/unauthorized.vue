<template>
    <main class="flex-shrink-0">
        <div class="container" style="padding: 0">
            <div class="row">
                <div class="col-auto col-fix">
                    <form @submit="returnToPreviousPage" style="text-align: center">
                        <img :src="logoSrc" style="padding-bottom: 10px" width="200"
                            height="60" alt="WOOPI AI" />

                        <h5 class="fw-bold">{{ $t("unauthorized.title") }}</h5>
                        <button class="btn btn-outline-primary" @click="returnToPreviousPage">
                            <LucideIcon icon="Redo2" />
                            {{ $t("common.back") }}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
export default {
    name: "UnauthorizedPage",
    computed: {
        logoSrc() {
            const theme = this.$store.state.theme || localStorage.getItem("theme") || "css-theme-light";
            return theme === "css-theme-dark"
                ? require("@/assets/img/woopiai-logo-dark.png")
                : require("@/assets/img/woopiai-logo-light.png");
        },
    },
    methods: {
        returnToPreviousPage() {
            this.$router.push({ name: 'Home' });
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
</style>