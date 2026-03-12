<template>
    <div class="row g-3 auditor-cards-row">
        <div class="col-4">
            <div class="card rounded-3 auditor-summary-card">
                <div class="card-body d-flex flex-column auditor-summary-card-body">
                    <AuditorUserFilters
                        v-model:search="search"
                        v-model:team-id="teamId"
                        :team-options="teamOptions"
                    />
                    <AuditorUserSummary
                        ref="summaryRef"
                        :selected-user="selectedUser"
                        :search="search"
                        :team-id="teamId"
                        @select-user="onSelectUser"
                    />
                </div>
            </div>
        </div>
        <div class="col-8">
            <div class="card rounded-3 auditor-detail-card">
                <AuditorUserDetail :selected-user="selectedUser" />
            </div>
        </div>
    </div>
</template>
<script>
    import AuditorUserFilters from "./AuditorUserFilters.vue";
    import AuditorUserSummary from "./AuditorUserSummary.vue";
    import AuditorUserDetail from "./AuditorUserDetail.vue";

    export default {
        name: "AuditorUsersSection",
        components: {
            AuditorUserFilters,
            AuditorUserSummary,
            AuditorUserDetail,
        },
        data() {
            return {
                search: "",
                teamId: "",
                teamOptions: [
                    { value: "", label: "Todos os times" },
                    { value: "juridico", label: "Time Juridico" },
                    { value: "financeiro", label: "Time Financeiro" },
                    { value: "rh", label: "Time RH" },
                ],
                selectedUser: null,
            };
        },
        methods: {
            onSelectUser(user) {
                this.selectedUser = user;
            },
        },
    };
</script>
<style scoped>
    .auditor-detail-card {
        height: 70vh;
        display: flex;
        flex-direction: column;
        overflow: hidden;
    }
    .auditor-detail-card > * {
        flex: 1 1 0;
        min-height: 0;
        overflow: hidden;
        display: flex;
        flex-direction: column;
    }
</style>
