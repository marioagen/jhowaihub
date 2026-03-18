<template>
    <div class="row g-3 auditor-cards-row">
        <div class="col-4">
            <div class="card rounded-3 auditor-summary-card">
                <div class="card-body d-flex flex-column auditor-summary-card-body">
                    <AuditorUserFilters @filter="filterData" />
                    <AuditorUserSummary
                        ref="AuditorUserSummary"
                        :filters="filterParams"
                        @select-user="onSelectUser"
                    />
                </div>
            </div>
        </div>
        <div class="col-8">
            <div class="card rounded-3 auditor-detail-card">
                <AuditorUserDetail
                    ref="AuditorUserDetail"
                    :selected-user="selectedUser"
                />
            </div>
        </div>
    </div>
</template>
<script>
    import AuditorUserFilters from "@/components/auditor/users/AuditorUserFilters.vue";
    import AuditorUserSummary from "@/components/auditor/users/AuditorUserSummary.vue";
    import AuditorUserDetail from "@/components/auditor/users/AuditorUserDetail.vue";

    export default {
        name: "AuditorUsersSection",
        components: {
            AuditorUserFilters,
            AuditorUserSummary,
            AuditorUserDetail,
        },
        data() {
            return {
                filterParams: {
                    search: "",
                    teamId: "",
                },
                selectedUser: null,
            };
        },
        methods: {
            filterData(filters) {
                this.filterParams = filters;
                this.$refs.AuditorUserSummary?.refreshWithCurrentFilters();
            },
            onSelectUser(user) {
                this.selectedUser = user;
                this.$nextTick(() => {
                    this.$refs.AuditorUserDetail?.refreshWithCurrentDocument();
                });
            },
        },
    };
</script>
<style scoped>
    .auditor-summary-card,
    .auditor-detail-card {
        height: 70vh;
        display: flex;
        flex-direction: column;
        overflow: hidden;
    }
    .auditor-summary-card .auditor-summary-card-body,
    .auditor-detail-card > * {
        flex: 1 1 0;
        min-height: 0;
        overflow: hidden;
        display: flex;
        flex-direction: column;
    }
</style>
