<template>
    <main class="flex-shrink-0">
        <div class="container" style="padding: 0">
            <div class="row">
                <div class="col-auto col-fix">
                    <form @submit="returnToPreviousPage" style="text-align: center">
                        <img
                            src="../assets/img/woopiai-hub-logo.png"
                            style="padding-bottom: 10px"
                            width="160"
                            height="80"
                            v-if="showLogoDarkMode"
                        />

                        <img
                            src="../assets/img/woopiai-hub-logo.png"
                            style="padding-bottom: 10px"
                            width="160"
                            height="61"
                            v-else
                        />

                        <h5 class="fw-bold">{{ $t("unauthorized.title") }}</h5>
                        <button
                            class="btn btn-outline-primary"
                            @click="redirectTo"
                        >
                            <LucideIcon icon="Redo2" />
                            {{ $t("labelBack") }}
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
        methods: {
            returnToPreviousPage() {
                this. $router.back();
            },
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