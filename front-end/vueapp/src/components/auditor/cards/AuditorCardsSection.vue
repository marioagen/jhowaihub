<template>
    <div class="row g-3 auditor-cards-row">
        <div class="col-4">
            <div class="card rounded-3 auditor-summary-card">
                <div class="card-body d-flex flex-column auditor-summary-card-body">
                    <AuditorCardFilters @filter="filterData" />
                    <AuditorCardSummary
                        @select-document="onSelectDocument"
                        ref="AuditorCardSummary"
                    />
                </div>
            </div>
        </div>
        <div class="col-8">
            <div class="card rounded-3 auditor-detail-card">
                <AuditorCardDetail
                    :selected-document="selectedDocument"
                    ref="AuditorCardDetail"
                />
            </div>
        </div>
    </div>
</template>
<script>
    import AuditorCardFilters from "@/components/auditor/cards/AuditorCardFilters.vue";
    import AuditorCardSummary from "@/components/auditor/cards/AuditorCardSummary.vue";
    import AuditorCardDetail from "@/components/auditor/cards/AuditorCardDetail.vue";

    export default {
        name: "AuditorCardsSection",
        components: {
            AuditorCardFilters,
            AuditorCardSummary,
            AuditorCardDetail,
        },
        data() {
            return {
                filterParams: {
                    search: "",
                    statusId: "",
                },
                statusFilterOptions: [
                    { value: "", label: "Todos os status" },
                    { value: "ativo", label: "Ativo" },
                    { value: "finalizado", label: "Finalizado" },
                ],
                selectedDocument: null,
            };
        },
        methods: {
            filterData(filters) {
                this.filterParams = filters;
                this.$refs.AuditorCardSummary.filters = filters;
                this.$refs.AuditorCardSummary.getAuditCardsSummary();
            },
            onSelectDocument(document) {
                this.selectedDocument = document;
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
