<template>
    <div class="row g-3 auditor-cards-row">
        <div class="col-4">
            <div class="card rounded-3 auditor-summary-card">
                <div class="card-body d-flex flex-column auditor-summary-card-body">
                    <AuditorWorkflowFilters @filter="filterData" />
                    <AuditorWorkflowSummary
                        ref="AuditorWorkflowSummary"
                        :filters="filterParams"
                        @select-workflow="onSelectWorkflow"
                    />
                </div>
            </div>
        </div>
        <div class="col-8">
            <div class="card rounded-3 auditor-detail-card">
                <AuditorWorkflowDetail
                    ref="AuditorWorkflowDetail"
                    :selected-workflow="selectedWorkflow"
                />
            </div>
        </div>
    </div>
</template>
<script>
    import AuditorWorkflowFilters from "./AuditorWorkflowFilters.vue";
    import AuditorWorkflowSummary from "./AuditorWorkflowSummary.vue";
    import AuditorWorkflowDetail from "./AuditorWorkflowDetail.vue";

    export default {
        name: "AuditorWorkflowsSection",
        components: {
            AuditorWorkflowFilters,
            AuditorWorkflowSummary,
            AuditorWorkflowDetail,
        },
        data() {
            return {
                filterParams: {
                    search: "",
                },
                selectedWorkflow: null,
            };
        },
        methods: {
            filterData(filters) {
                this.filterParams = filters;
                this.$refs.AuditorWorkflowSummary?.refreshWithCurrentFilters();
            },
            onSelectWorkflow(workflow) {
                this.selectedWorkflow = workflow;
                this.$nextTick(() => {
                    this.$refs.AuditorWorkflowDetail?.refreshWithCurrentDocument();
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
