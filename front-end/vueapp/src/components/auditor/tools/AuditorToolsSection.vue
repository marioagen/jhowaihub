<template>
    <div class="row g-3 auditor-cards-row">
        <div class="col-4">
            <div class="card rounded-3 auditor-summary-card">
                <div class="card-body d-flex flex-column auditor-summary-card-body">
                    <AuditorToolsFilters @filter="filterData" />
                    <AuditorToolsSummary
                        ref="AuditorToolsSummary"
                        :filters="filterParams"
                        @select-tool="onSelectTool"
                    />
                </div>
            </div>
        </div>
        <div class="col-8">
            <div class="card rounded-3 auditor-detail-card">
                <AuditorToolsDetail
                    ref="AuditorToolsDetail"
                    :selected-tool="selectedTool"
                />
            </div>
        </div>
    </div>
</template>
<script>
    import AuditorToolsFilters from "@/components/auditor/tools/AuditorToolsFilters.vue";
    import AuditorToolsSummary from "@/components/auditor/tools/AuditorToolsSummary.vue";
    import AuditorToolsDetail from "@/components/auditor/tools/AuditorToolsDetail.vue";

    export default {
        name: "AuditorToolsSection",
        components: {
            AuditorToolsFilters,
            AuditorToolsSummary,
            AuditorToolsDetail,
        },
        data() {
            return {
                filterParams: { search: "", category: "" },
                selectedTool: null,
            };
        },
        methods: {
            filterData(filters) {
                this.filterParams = filters;
                this.$refs.AuditorToolsSummary?.refreshWithCurrentFilters();
            },
            onSelectTool(tool) {
                this.selectedTool = tool;
                this.$nextTick(() => {
                    this.$refs.AuditorToolsDetail?.refresh();
                });
            },
        },
    };
</script>
<style scoped>
    .auditor-summary-card,
    .auditor-detail-card {
        height: calc(100vh - 230px);
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
