<template>
    <div class="row g-3">
        <div class="col-4">
            <div class="card h-100 rounded-3">
                <div class="card-body d-flex flex-column">
                    <AuditorWorkflowFilters v-model:search="search" />
                    <AuditorWorkflowSummary
                        ref="summaryRef"
                        :selected-workflow="selectedWorkflow"
                        :search="search"
                        @select-workflow="onSelectWorkflow"
                    />
                </div>
            </div>
        </div>
        <div class="col-8">
            <div class="card h-100 rounded-3">
                <AuditorWorkflowDetail :selected-workflow="selectedWorkflow" />
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
                search: "",
                selectedWorkflow: null,
            };
        },
        methods: {
            onSelectWorkflow(workflow) {
                this.selectedWorkflow = workflow;
            },
        },
    };
</script>
